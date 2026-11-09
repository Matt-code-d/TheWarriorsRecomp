using WarriorsLauncher.Models;

namespace WarriorsLauncher.Services;

/// <summary>Applies Performance / Balanced / Quality bundles onto launcher + host settings.</summary>
public static class GraphicsPresets
{
    public static void Apply(LauncherSettings s, GraphicsPresetKind preset)
    {
        if (preset == GraphicsPresetKind.Custom) return;

        // Shared baseline for every named preset.
        s.GraphicsApi = "d3d12";
        s.VSync = true;
        s.FpsCap = 50;
        s.FrameGeneration = "off";
        s.DlssPreset = "default";
        s.PipelineCache = true;
        s.GpuSkinning = true;
        s.CustomTextures = true;
        s.GraphicsPreset = GpuDetectionResult.PresetKey(preset);

        switch (preset)
        {
            case GraphicsPresetKind.Performance:
                s.Upscaler = "fsr";
                s.UpscaleQuality = "performance";
                s.RenderPercent = 50;
                s.Anisotropic = 4;
                s.Sharpness = 10;
                s.Filmic = false;
                s.FilmicStrength = 40;
                s.FilmicVignette = 20;
                s.FilmicGrain = 0;
                s.SceneGlow = false;
                s.SceneGlowStrength = 30;
                s.AudioResampler = "linear";
                break;

            case GraphicsPresetKind.Balanced:
                s.Upscaler = "fsr";
                s.UpscaleQuality = "balanced";
                s.RenderPercent = 59;
                s.Anisotropic = 8;
                s.Sharpness = 20;
                s.Filmic = true;
                s.FilmicStrength = 40;
                s.FilmicVignette = 25;
                s.FilmicGrain = 0;
                s.SceneGlow = false;
                s.SceneGlowStrength = 35;
                s.AudioResampler = "linear";
                break;

            case GraphicsPresetKind.Quality:
                s.Upscaler = "fsr";
                s.UpscaleQuality = "quality";
                s.RenderPercent = 67;
                s.Anisotropic = 16;
                s.Sharpness = 25;
                s.Filmic = true;
                s.FilmicStrength = 50;
                s.FilmicVignette = 35;
                s.FilmicGrain = 0;
                s.SceneGlow = true;
                s.SceneGlowStrength = 40;
                s.AudioResampler = "hq";
                break;
        }
    }

    public static GraphicsPresetKind DetectCurrent(LauncherSettings s)
    {
        foreach (var kind in new[] { GraphicsPresetKind.Performance, GraphicsPresetKind.Balanced, GraphicsPresetKind.Quality })
        {
            var probe = CloneGraphics(s);
            Apply(probe, kind);
            if (GraphicsMatch(s, probe)) return kind;
        }
        return GraphicsPresetKind.Custom;
    }

    static bool GraphicsMatch(LauncherSettings a, LauncherSettings b) =>
        string.Equals(a.Upscaler, b.Upscaler, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(a.UpscaleQuality, b.UpscaleQuality, StringComparison.OrdinalIgnoreCase) &&
        a.RenderPercent == b.RenderPercent &&
        a.Anisotropic == b.Anisotropic &&
        a.Sharpness == b.Sharpness &&
        a.Filmic == b.Filmic &&
        a.SceneGlow == b.SceneGlow &&
        string.Equals(a.AudioResampler, b.AudioResampler, StringComparison.OrdinalIgnoreCase) &&
        a.PipelineCache == b.PipelineCache &&
        a.GpuSkinning == b.GpuSkinning;

    static LauncherSettings CloneGraphics(LauncherSettings s) => new()
    {
        Upscaler = s.Upscaler,
        UpscaleQuality = s.UpscaleQuality,
        RenderPercent = s.RenderPercent,
        Anisotropic = s.Anisotropic,
        Sharpness = s.Sharpness,
        Filmic = s.Filmic,
        FilmicStrength = s.FilmicStrength,
        FilmicVignette = s.FilmicVignette,
        FilmicGrain = s.FilmicGrain,
        SceneGlow = s.SceneGlow,
        SceneGlowStrength = s.SceneGlowStrength,
        AudioResampler = s.AudioResampler,
        PipelineCache = s.PipelineCache,
        GpuSkinning = s.GpuSkinning,
        GraphicsPreset = s.GraphicsPreset,
    };

    public static void StoreDetection(LauncherSettings s, GpuDetectionResult d)
    {
        s.GpuName = d.DisplayName;
        s.GpuTier = d.TierKey;
        s.GpuVramMb = d.VramMb;
        s.GpuRecommendedPreset = d.RecommendedKey;
        s.GpuTierTableVersion = d.TableVersion;
        if (d.Adapter is not null)
            s.GpuId = $"{d.Adapter.VendorId:x4}:{d.Adapter.DeviceId:x4}";
    }
}
