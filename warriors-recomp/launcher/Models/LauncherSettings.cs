using System.Text.Json;
using System.Text.Json.Serialization;
using WarriorsLauncher.Services;

namespace WarriorsLauncher.Models;

public sealed class LauncherSettings
{
    /// <summary>Folder that holds Warriors.exe (the XBERecomp release).</summary>
    public string PortRoot { get; set; } = "";

    /// <summary>assets folder (ISO or extracted default.xbe).</summary>
    public string AssetsRoot { get; set; } = "";

    public string IsoPath { get; set; } = "";

    /// <summary>Xbox XC_LANGUAGE: 1 En, 3 De, 4 Fr, 5 Es, 6 It.</summary>
    public uint UserLanguage { get; set; } = 1;

    public string UiLocale { get; set; } = "en";

    /// <summary>When the NTSC-tuned host warns about a different XBE, auto-confirm continue.</summary>
    public bool AllowDifferentRelease { get; set; } = true;

    /// <summary>fullscreen | borderless | windowed — written to settings.toml.</summary>
    public string DisplayMode { get; set; } = "borderless";

    /// <summary>0 = use desktop resolution.</summary>
    public uint Width { get; set; }

    /// <summary>0 = use desktop resolution.</summary>
    public uint Height { get; set; }

    /// <summary>0 unlimited, 1 monitor refresh, else target FPS (PAL base 50; host scales dt above 50).</summary>
    public uint FpsCap { get; set; } = 50;

    public bool VSync { get; set; } = true;

    /// <summary>Render scale percent of output height (25–200).</summary>
    public uint RenderPercent { get; set; } = 100;

    /// <summary>off | fsr | dlss | xess — settings.toml upscaler key.</summary>
    public string Upscaler { get; set; } = "off";

    /// <summary>native | ultra_quality | quality | balanced | performance | ultra_performance</summary>
    public string UpscaleQuality { get; set; } = "quality";

    /// <summary>off | fsr | dlss — frame gen not in our host yet; still written for compatibility.</summary>
    public string FrameGeneration { get; set; } = "off";

    public string DlssPreset { get; set; } = "default";

    /// <summary>0–100 RCAS / upscaler sharpness.</summary>
    public uint Sharpness { get; set; } = 20;

    public uint PerfOverlay { get; set; }

    public bool KeyboardControls { get; set; } = true;

    public bool InvertCameraY { get; set; }

    public bool Vibration { get; set; } = true;

    /// <summary>0 = as the game asks.</summary>
    public uint Anisotropic { get; set; }

    public bool CustomTextures { get; set; } = true;

    public bool DumpTextures { get; set; }

    /// <summary>d3d12 (only backend today) or vulkan (reserved — not built yet).</summary>
    public string GraphicsApi { get; set; } = "d3d12";

    /// <summary>Present filmic grade (opt-in; off = baseline blit).</summary>
    public bool Filmic { get; set; }

    public uint FilmicStrength { get; set; } = 50;
    public uint FilmicGrain { get; set; }
    public uint FilmicVignette { get; set; } = 35;

    /// <summary>Optional user .cube path relative to the exe; empty = built-in grade only.</summary>
    public string FilmicLut { get; set; } = "";

    /// <summary>Soft bloom from bright pixels (not SoftLight). Opt-in.</summary>
    public bool SceneGlow { get; set; }

    public uint SceneGlowStrength { get; set; } = 40;

    /// <summary>linear (baseline) | hq</summary>
    public string AudioResampler { get; set; } = "linear";

    public bool PipelineCache { get; set; } = true;

    public bool GpuSkinning { get; set; } = true;

    /// <summary>performance | balanced | quality | custom — graphics preset bundle.</summary>
    public string GraphicsPreset { get; set; } = "custom";

    /// <summary>Last detected GPU display name (informational).</summary>
    public string GpuName { get; set; } = "";

    /// <summary>vendor:device hex from WMI PNP id.</summary>
    public string GpuId { get; set; } = "";

    /// <summary>unknown | t0 | t1 | t2 | t3</summary>
    public string GpuTier { get; set; } = "unknown";

    public int GpuVramMb { get; set; }

    /// <summary>Recommended preset key from the tier table.</summary>
    public string GpuRecommendedPreset { get; set; } = "balanced";

    public int GpuTierTableVersion { get; set; }

    /// <summary>True after we auto-applied the recommended preset once for this GPU/table.</summary>
    public bool GpuPresetAutoApplied { get; set; }

    /// <summary>Legacy FSR1 index derived from Upscaler + UpscaleQuality (also written as <c>fsr</c>).</summary>
    public uint Fsr => DeriveFsr(Upscaler, UpscaleQuality);

    public static uint DeriveFsr(string upscaler, string quality)
    {
        var u = (upscaler ?? "off").Trim().ToLowerInvariant();
        if (u is "off" or "") return 0;
        // fsr / dlss / xess — host currently implements spatial FSR1 for all of these
        var q = (quality ?? "quality").Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
        return q switch
        {
            "native" or "dlaa" or "off" => 0,
            "ultra_quality" or "ultraquality" => 1,
            "quality" => 2,
            "balanced" => 3,
            "performance" => 4,
            "ultra_performance" or "ultraperformance" => 4,
            _ => 2,
        };
    }

    public static uint SuggestedRenderPercent(string quality) =>
        (quality ?? "").Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_') switch
        {
            "ultra_quality" or "ultraquality" => 77u,
            "quality" => 67u,
            "balanced" => 59u,
            "performance" => 50u,
            "ultra_performance" or "ultraperformance" => 33u,
            "native" or "dlaa" => 100u,
            _ => 100u,
        };

    public static string DefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "thewarriors");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "launcher_settings.json");
    }

    public static LauncherSettings Load(string? path = null)
    {
        path ??= DefaultPath();
        if (!File.Exists(path))
            return new LauncherSettings();
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LauncherSettings>(json, JsonOpts()) ?? new LauncherSettings();
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOpts()));
    }

    public void EnsureDefaults(string projectRoot)
    {
        // Always re-resolve: stale Documents JSON often still points at port\ (old NTSC zip).
        PortRoot = Paths.FindPortRoot(projectRoot);

        var besideExe = Path.GetFullPath(Path.Combine(PortRoot, "assets"));
        var portFull = Path.GetFullPath(PortRoot);
        var packaged = File.Exists(Path.Combine(PortRoot, "Warriors.exe")) &&
                       PathsEqual(PortRoot, AppContext.BaseDirectory);

        // Packaged / side-by-side zip: NEVER reuse Documents AssetsRoot/IsoPath from another install.
        if (packaged)
        {
            AssetsRoot = besideExe;
            Directory.CreateDirectory(AssetsRoot);
            if (!IsoBelongsToInstall(IsoPath, portFull, AssetsRoot))
                IsoPath = "";
            // Only auto-fill ISO from THIS install's assets — never walk D:\Games or the repo.
            if (string.IsNullOrWhiteSpace(IsoPath) || !File.Exists(IsoPath))
                IsoPath = EnumerateIsos(AssetsRoot).FirstOrDefault() ?? "";
            return;
        }

        var releaseAssets = Path.Combine(projectRoot, "out", "build", "release", "assets");
        // Drop JSON AssetsRoot that points outside this PortRoot (other test folders / old port).
        if (!string.IsNullOrWhiteSpace(AssetsRoot))
        {
            try
            {
                var cur = Path.GetFullPath(AssetsRoot);
                if (!cur.StartsWith(portFull, StringComparison.OrdinalIgnoreCase) &&
                    !cur.StartsWith(Path.GetFullPath(projectRoot), StringComparison.OrdinalIgnoreCase))
                    AssetsRoot = besideExe;
            }
            catch
            {
                AssetsRoot = besideExe;
            }
        }

        AssetsRoot = PickAssetsRoot(AssetsRoot, besideExe, releaseAssets);
        Directory.CreateDirectory(AssetsRoot);

        if (!IsoBelongsToInstall(IsoPath, portFull, AssetsRoot) || !File.Exists(IsoPath))
        {
            var hit = EnumerateIsos(AssetsRoot)
                .Concat(EnumerateIsos(besideExe))
                .FirstOrDefault();
            IsoPath = hit ?? "";
        }
    }

    static bool PathsEqual(string a, string b)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    static bool IsoBelongsToInstall(string? isoPath, string portFull, string assetsRoot)
    {
        if (string.IsNullOrWhiteSpace(isoPath)) return false;
        try
        {
            if (!File.Exists(isoPath)) return false;
            var iso = Path.GetFullPath(isoPath);
            if (iso.StartsWith(portFull, StringComparison.OrdinalIgnoreCase)) return true;
            if (iso.StartsWith(Path.GetFullPath(assetsRoot), StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        catch
        {
            return false;
        }
    }

    static string PickAssetsRoot(string current, string besideExe, string releaseAssets)
    {
        static bool HasXbe(string dir) =>
            Directory.Exists(dir) && File.Exists(Path.Combine(dir, "default.xbe"));
        static bool HasIso(string dir) =>
            Directory.Exists(dir) && Directory.EnumerateFiles(dir, "*.iso").Any();

        if (HasXbe(besideExe)) return Path.GetFullPath(besideExe);
        if (HasXbe(releaseAssets)) return Path.GetFullPath(releaseAssets);
        if (HasXbe(current)) return Path.GetFullPath(current);
        if (HasIso(besideExe)) return Path.GetFullPath(besideExe);
        if (HasIso(current)) return Path.GetFullPath(current);
        if (HasIso(releaseAssets)) return Path.GetFullPath(releaseAssets);
        return Path.GetFullPath(besideExe);
    }

    static IEnumerable<string> EnumerateIsos(string dir)
    {
        if (!Directory.Exists(dir)) return [];
        return Directory.EnumerateFiles(dir, "*.iso");
    }

    static JsonSerializerOptions JsonOpts() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
