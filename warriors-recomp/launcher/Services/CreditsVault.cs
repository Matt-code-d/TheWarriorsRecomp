using System.IO.Compression;
using System.Text;

namespace WarriorsLauncher.Services;

/// <summary>
/// About credit bodies from obfuscated embedded blob (deflate + XOR).
/// Not a DRM lock — only keeps author strings out of plain resources / easy hex search.
/// </summary>
public static class CreditsVault
{
    static readonly object Gate = new();
    static Dictionary<string, string>? _byLang;

    static readonly byte[] Key =
    {
        (byte)'N', (byte)'O', (byte)'T', (byte)'I', (byte)'C', (byte)'E', (byte)'-', (byte)'T',
        (byte)'W', (byte)'R', (byte)'-', (byte)'2', (byte)'0', (byte)'2', (byte)'6', (byte)'!',
    };

    public static bool LoadedOk { get; private set; }
    public static string Status { get; private set; } = "not loaded";

    public static string BodyFor(string? langCode)
    {
        var map = Ensure();
        var code = string.IsNullOrWhiteSpace(langCode) ? "en" : langCode.Trim().ToLowerInvariant();
        if (map.TryGetValue(code, out var body) && !string.IsNullOrWhiteSpace(body))
            return body;
        return map.TryGetValue("en", out var en) ? en : "";
    }

    static Dictionary<string, string> Ensure()
    {
        lock (Gate)
        {
            if (_byLang != null) return _byLang;
            _byLang = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var text = Encoding.UTF8.GetString(Unpack());
                foreach (var rawLine in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith('#')) continue;
                    var bar = line.IndexOf('|');
                    if (bar <= 0 || bar >= line.Length - 1) continue;
                    var code = line[..bar].Trim().ToLowerInvariant();
                    var body = line[(bar + 1)..].Trim();
                    if (code.Length > 0 && body.Length > 0)
                        _byLang[code] = body;
                }
                if (_byLang.Count == 0)
                    throw new InvalidOperationException("notice.bin had no credit lines");
                LoadedOk = true;
                Status = $"ok langs={_byLang.Count}";
            }
            catch (Exception ex)
            {
                LoadedOk = false;
                Status = ex.GetType().Name + ": " + ex.Message;
                // Last-resort so About is never blank if the blob is missing.
                _byLang["en"] =
                    "EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. See CONTRIBUTING.md.";
            }
            return _byLang;
        }
    }

    static byte[] Unpack()
    {
        var asm = typeof(CreditsVault).Assembly;
        using var stream = asm.GetManifestResourceStream("WarriorsLauncher.Assets.notice.bin")
            ?? throw new InvalidOperationException("missing WarriorsLauncher.Assets.notice.bin");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var enc = ms.ToArray();
        var comp = new byte[enc.Length];
        for (int i = 0; i < enc.Length; i++)
            comp[i] = (byte)(enc[i] ^ Key[i % Key.Length]);
        using var input = new MemoryStream(comp);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        deflate.CopyTo(outMs);
        return outMs.ToArray();
    }
}
