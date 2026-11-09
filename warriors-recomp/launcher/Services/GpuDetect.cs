using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace WarriorsLauncher.Services;

public enum GpuKind { Unknown, Integrated, Discrete, Software }

public enum GpuTier { Unknown, T0, T1, T2, T3 }

/// <summary>Maps to our graphics presets.</summary>
public enum GraphicsPresetKind { Performance, Balanced, Quality, Custom }

public sealed record GpuAdapterInfo
{
    public string Name { get; init; } = "";
    public uint VendorId { get; init; }
    public uint DeviceId { get; init; }
    public ulong DedicatedVramBytes { get; init; }
    public GpuKind Kind { get; init; }
}

public sealed class GpuDetectionResult
{
    public bool Found { get; init; }
    public GpuAdapterInfo? Adapter { get; init; }
    public GpuTier Tier { get; init; } = GpuTier.Unknown;
    public GraphicsPresetKind Recommended { get; init; } = GraphicsPresetKind.Balanced;
    public int TableVersion { get; init; }

    public string DisplayName => Adapter is null ? "" : DisplayNameOf(Adapter.Name);
    public int VramMb => Adapter is null || Adapter.DedicatedVramBytes == 0
        ? 0
        : (int)Math.Max(1, Adapter.DedicatedVramBytes / (1024ul * 1024ul));

    public string TierKey => Tier switch
    {
        GpuTier.T0 => "t0",
        GpuTier.T1 => "t1",
        GpuTier.T2 => "t2",
        GpuTier.T3 => "t3",
        _ => "unknown",
    };

    public string RecommendedKey => PresetKey(Recommended);

    public static string PresetKey(GraphicsPresetKind p) => p switch
    {
        GraphicsPresetKind.Performance => "performance",
        GraphicsPresetKind.Quality => "quality",
        GraphicsPresetKind.Custom => "custom",
        _ => "balanced",
    };

    public static bool TryParsePreset(string? key, out GraphicsPresetKind preset)
    {
        preset = GraphicsPresetKind.Balanced;
        switch ((key ?? "").Trim().ToLowerInvariant())
        {
            case "performance":
            case "laptop":
                preset = GraphicsPresetKind.Performance;
                return true;
            case "balanced":
                preset = GraphicsPresetKind.Balanced;
                return true;
            case "quality":
            case "gaming":
                preset = GraphicsPresetKind.Quality;
                return true;
            case "custom":
                preset = GraphicsPresetKind.Custom;
                return true;
            default:
                return false;
        }
    }

    public static string DisplayNameOf(string raw)
    {
        var s = raw ?? "";
        s = Regex.Replace(s, @"\s*\((?:R|TM|C)\)", "", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\s*\([^)]*(?:RADV|LLVM|Mesa|OpenGL|Direct3D)[^)]*\)\s*$", "", RegexOptions.IgnoreCase);
        return s.Trim();
    }
}

/// <summary>WMI adapter probe + name/id tier table → recommended graphics preset.</summary>
public static class GpuDetect
{
    const uint VendorNvidia = 0x10de;
    const uint VendorAmd = 0x1002;
    const uint VendorIntel = 0x8086;
    const ulong GiB = 1024ul * 1024ul * 1024ul;

    public static GpuDetectionResult Detect()
    {
        var table = BuiltinTable();
        var adapters = ProbeWmi();
        var choice = Choose(adapters);
        if (choice is null)
            return new GpuDetectionResult { TableVersion = table.Version, Recommended = GraphicsPresetKind.Balanced };

        var tier = ClassifyTier(table, choice);
        return new GpuDetectionResult
        {
            Found = true,
            Adapter = choice,
            Tier = tier,
            Recommended = PresetForTier(tier),
            TableVersion = table.Version,
        };
    }

    public static GraphicsPresetKind PresetForTier(GpuTier tier) => tier switch
    {
        GpuTier.T0 => GraphicsPresetKind.Performance,
        GpuTier.T2 => GraphicsPresetKind.Quality,
        GpuTier.T3 => GraphicsPresetKind.Quality,
        _ => GraphicsPresetKind.Balanced,
    };

    static GpuAdapterInfo? Choose(List<GpuAdapterInfo> adapters)
    {
        var usable = adapters.Where(a => a.Kind != GpuKind.Software).ToList();
        if (usable.Count == 0) return null;
        return usable
            .OrderByDescending(a => a.Kind == GpuKind.Discrete)
            .ThenByDescending(a => a.DedicatedVramBytes)
            .ThenByDescending(a => a.Name.Length)
            .First();
    }

    static GpuTier ClassifyTier(TierTable table, GpuAdapterInfo a)
    {
        var kind = InferKind(a);
        if (kind == GpuKind.Software) return GpuTier.Unknown;
        if (kind == GpuKind.Integrated) return GpuTier.T0;

        var name = GpuDetectionResult.DisplayNameOf(a.Name).ToLowerInvariant();
        var tier = GpuTier.Unknown;
        var matched = false;
        foreach (var r in table.Rules)
        {
            if (r.Kind == RuleKind.Demote) continue;
            if (r.Vendor != 0 && r.Vendor != a.VendorId) continue;
            var hit = r.Kind == RuleKind.Name
                ? NameMatches(r.Pattern, name)
                : a.DeviceId != 0 && a.DeviceId >= r.Lo && a.DeviceId <= r.Hi;
            if (!hit) continue;
            tier = r.Tier;
            matched = true;
            break;
        }

        if (matched)
        {
            foreach (var r in table.Rules)
            {
                if (r.Kind != RuleKind.Demote) continue;
                if (r.Vendor != 0 && r.Vendor != a.VendorId) continue;
                if (!NameMatches(r.Pattern, name)) continue;
                if (tier > GpuTier.T1) tier = (GpuTier)((int)tier - 1);
                break;
            }
        }
        else
        {
            var gb = a.DedicatedVramBytes;
            if (gb == 0) return GpuTier.Unknown;
            if (gb < 4 * GiB) tier = GpuTier.T1;
            else if (gb < 8 * GiB) tier = GpuTier.T2;
            else tier = GpuTier.T3;
        }

        if (a.DedicatedVramBytes != 0 && a.DedicatedVramBytes < (7 * GiB) / 2 && tier > GpuTier.T1)
            tier = GpuTier.T1;
        return tier;
    }

    static GpuKind InferKind(GpuAdapterInfo a)
    {
        if (a.Kind != GpuKind.Unknown) return a.Kind;
        var n = a.Name.ToLowerInvariant();
        if (n.Contains("microsoft basic") || n.Contains("llvmpipe") || n.Contains("swiftshader") ||
            n.Contains("remote desktop") || n.Contains("virtual") && n.Contains("display"))
            return GpuKind.Software;
        if (n.Contains("uhd graphics") || n.Contains("hd graphics") || n.Contains("iris") ||
            n.Contains("radeon graphics") || n.Contains("graphics") && a.VendorId == VendorAmd && !n.Contains("rx ") ||
            (a.VendorId == VendorIntel && !n.Contains("arc") && a.DedicatedVramBytes < 2 * GiB))
            return GpuKind.Integrated;
        if (a.VendorId is VendorNvidia or VendorAmd || n.Contains("geforce") || n.Contains("radeon rx") || n.Contains("arc"))
            return GpuKind.Discrete;
        return a.DedicatedVramBytes >= 2 * GiB ? GpuKind.Discrete : GpuKind.Unknown;
    }

    static bool NameMatches(string pattern, string lowerName)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(lowerName)) return false;
        for (var i = 0; i <= lowerName.Length; i++)
            if (MatchHere(pattern, 0, lowerName, i)) return true;
        return false;
    }

    static bool MatchHere(string p, int pi, string s, int si)
    {
        while (pi < p.Length)
        {
            if (p[pi] == '*')
            {
                pi++;
                if (pi >= p.Length) return true;
                for (var k = si; k <= s.Length; k++)
                    if (MatchHere(p, pi, s, k)) return true;
                return false;
            }
            if (si >= s.Length) return false;
            var ok = p[pi] switch
            {
                '#' => char.IsDigit(s[si]),
                '?' => true,
                _ => p[pi] == s[si],
            };
            if (!ok) return false;
            pi++;
            si++;
        }
        return true;
    }

    static List<GpuAdapterInfo> ProbeWmi()
    {
        var list = new List<GpuAdapterInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, AdapterRAM, PNPDeviceID FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                try
                {
                    var name = (mo["Name"] as string)?.Trim() ?? "";
                    if (name.Length == 0) continue;
                    ulong vram = 0;
                    if (mo["AdapterRAM"] is uint u) vram = u;
                    else if (mo["AdapterRAM"] is ulong ul) vram = ul;
                    // WMI AdapterRAM is often capped at ~4 GB-1 on older paths; still useful as floor.
                    ParsePciIds(mo["PNPDeviceID"] as string, out var vendor, out var device);
                    var info = new GpuAdapterInfo
                    {
                        Name = name,
                        VendorId = vendor,
                        DeviceId = device,
                        DedicatedVramBytes = vram,
                        Kind = GpuKind.Unknown,
                    };
                    list.Add(info with { Kind = InferKind(info) });
                }
                catch
                {
                    // skip bad adapter row
                }
            }
        }
        catch
        {
            // WMI unavailable
        }
        return list;
    }

    static void ParsePciIds(string? pnp, out uint vendor, out uint device)
    {
        vendor = 0;
        device = 0;
        if (string.IsNullOrEmpty(pnp)) return;
        var ven = Regex.Match(pnp, @"VEN_([0-9A-Fa-f]{4})", RegexOptions.IgnoreCase);
        var dev = Regex.Match(pnp, @"DEV_([0-9A-Fa-f]{4})", RegexOptions.IgnoreCase);
        if (ven.Success) uint.TryParse(ven.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out vendor);
        if (dev.Success) uint.TryParse(dev.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out device);
    }

    #region Tier table

    enum RuleKind { Name, Id, Demote }

    sealed class TierRule
    {
        public RuleKind Kind;
        public uint Vendor;
        public GpuTier Tier;
        public string Pattern = "";
        public uint Lo, Hi;
    }

    sealed class TierTable
    {
        public int Version;
        public List<TierRule> Rules = new();
    }

    static TierTable? _table;

    static TierTable BuiltinTable()
    {
        if (_table != null) return _table;
        _table = ParseTable(EmbeddedTiers) ?? new TierTable { Version = 1 };
        return _table;
    }

    static TierTable? ParseTable(string text)
    {
        var t = new TierTable();
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = SplitWords(line);
            if (parts.Count == 0) continue;
            var first = parts[0].ToLowerInvariant();
            if (first == "version")
            {
                if (parts.Count >= 2 && int.TryParse(parts[1], out var v) && v > 0) t.Version = v;
                continue;
            }

            var r = new TierRule();
            var idx = 0;
            if (first == "demote")
            {
                r.Kind = RuleKind.Demote;
                idx = 1;
            }
            else if (first.Length == 2 && first[0] == 't' && first[1] is >= '0' and <= '3')
            {
                // Unknown=0, T0=1 … T3=4
                r.Tier = (GpuTier)((first[1] - '0') + 1);
                idx = 1;
            }
            else continue;

            if (idx >= parts.Count) continue;
            var vend = parts[idx++].ToLowerInvariant();
            r.Vendor = vend switch
            {
                "nvidia" => VendorNvidia,
                "amd" => VendorAmd,
                "intel" => VendorIntel,
                "any" => 0,
                _ => uint.MaxValue,
            };
            if (r.Vendor == uint.MaxValue) continue;
            if (idx >= parts.Count) continue;
            var how = parts[idx++].ToLowerInvariant();
            if (how == "name")
            {
                r.Pattern = string.Join(' ', parts.Skip(idx)).ToLowerInvariant();
                if (r.Pattern.Length == 0) continue;
                if (r.Kind != RuleKind.Demote) r.Kind = RuleKind.Name;
            }
            else if (how == "id" && r.Kind != RuleKind.Demote)
            {
                var range = string.Join("", parts.Skip(idx));
                var dash = range.IndexOf('-');
                if (dash < 0) continue;
                if (!TryHex(range[..dash], out r.Lo) || !TryHex(range[(dash + 1)..], out r.Hi) || r.Lo > r.Hi)
                    continue;
                r.Kind = RuleKind.Id;
            }
            else continue;
            t.Rules.Add(r);
        }
        return t.Version > 0 ? t : null;
    }

    static bool TryHex(string s, out uint v)
    {
        v = 0;
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) s = s[2..];
        return uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out v) && v <= 0xFFFF;
    }

    static List<string> SplitWords(string line)
    {
        var list = new List<string>();
        var cur = new StringBuilder();
        foreach (var ch in line)
        {
            if (ch is ' ' or '\t')
            {
                if (cur.Length > 0) { list.Add(cur.ToString()); cur.Clear(); }
            }
            else cur.Append(ch);
        }
        if (cur.Length > 0) list.Add(cur.ToString());
        return list;
    }

    const string EmbeddedTiers = """
version 1
T0 nvidia name geforce mx
T0 nvidia name geforce gt #
T3 nvidia name rtx 3070
T3 nvidia name rtx 3080
T3 nvidia name rtx 3090
T3 nvidia name rtx 4060 ti
T2 nvidia name rtx 4060
T2 nvidia name rtx 4050
T3 nvidia name rtx 40#0
T2 nvidia name rtx 5050
T3 nvidia name rtx 50#0
T2 nvidia name rtx 2060
T2 nvidia name rtx 2070
T2 nvidia name rtx 2080
T2 nvidia name rtx 3050
T2 nvidia name rtx 3060
T2 nvidia name gtx 1660
T2 nvidia name gtx 1070
T2 nvidia name gtx 1080
T2 nvidia name titan
T1 nvidia name gtx 1630
T1 nvidia name gtx 1650
T1 nvidia name gtx 1050
T1 nvidia name gtx 1060
T1 nvidia name gtx 9#0
T1 nvidia name gtx 7#0
T0 nvidia id 0x1d00-0x1dff
T1 nvidia id 0x1c00-0x1cff
T2 nvidia id 0x1b00-0x1bff
T1 nvidia id 0x1f80-0x1fff
T2 nvidia id 0x1e00-0x1f7f
T2 nvidia id 0x2180-0x21ff
T3 nvidia id 0x2200-0x223f
T3 nvidia id 0x2480-0x24ff
T2 nvidia id 0x2500-0x25ff
T3 nvidia id 0x2680-0x287f
T2 nvidia id 0x2880-0x28ff
demote nvidia name laptop
demote nvidia name max-q
demote nvidia name mobile
T3 amd name rx 9070
T3 amd name rx 9060 xt
T2 amd name rx 9060
T3 amd name rx 79#0
T3 amd name rx 78#0
T3 amd name rx 77#0
T2 amd name rx 76#0
T3 amd name rx 69#0
T3 amd name rx 68#0
T3 amd name rx 67#0
T2 amd name rx 66#0
T1 amd name rx 65#0
T1 amd name rx 64#0
T2 amd name rx 57#0
T2 amd name rx 56#0
T1 amd name rx 55#0
T2 amd name rx vega
T2 amd name radeon vii
T1 amd name rx 5#0
T1 amd name rx 4#0
T1 amd name r9
T1 amd name r7
T1 amd name radeon hd
demote amd name rx ####m
demote amd name rx ####s
T2 intel name arc b5
T2 intel name arc a7
T1 intel name arc a5
T1 intel name arc a3
""";

    #endregion
}
