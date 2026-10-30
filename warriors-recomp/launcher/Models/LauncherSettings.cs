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

    /// <summary>0 unlimited, 1 = monitor refresh, else target FPS.</summary>
    public uint FpsCap { get; set; } = 60;

    public bool VSync { get; set; } = true;

    /// <summary>Render scale percent of output height (25–200).</summary>
    public uint RenderPercent { get; set; } = 100;

    /// <summary>0 = as the game asks.</summary>
    public uint Anisotropic { get; set; }

    public bool CustomTextures { get; set; } = true;

    public bool DumpTextures { get; set; }

    /// <summary>d3d12 (only backend today) or vulkan (reserved — not built yet).</summary>
    public string GraphicsApi { get; set; } = "d3d12";

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

        var besideExe = Path.Combine(PortRoot, "assets");
        var releaseAssets = Path.Combine(projectRoot, "out", "build", "release", "assets");
        AssetsRoot = PickAssetsRoot(AssetsRoot, besideExe, releaseAssets);
        Directory.CreateDirectory(AssetsRoot);

        if (string.IsNullOrWhiteSpace(IsoPath) || !File.Exists(IsoPath))
        {
            var workspace = Path.GetDirectoryName(projectRoot) ?? projectRoot;
            var hit = EnumerateIsos(AssetsRoot)
                .Concat(EnumerateIsos(besideExe))
                .Concat(EnumerateIsos(projectRoot))
                .Concat(EnumerateIsos(workspace))
                .FirstOrDefault();
            if (hit != null) IsoPath = hit;
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
