using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WarriorsLauncher.Models;

namespace WarriorsLauncher.Services;

/// <summary>Reads/writes the host <c>settings.toml</c> beside Warriors.exe (reference-compatible schema).</summary>
public static class GameSettingsToml
{
    public static string PathFor(LauncherSettings settings) =>
        System.IO.Path.Combine(settings.PortRoot, "settings.toml");

    public static void ApplyFromLauncher(LauncherSettings settings)
    {
        Directory.CreateDirectory(settings.PortRoot);
        var path = PathFor(settings);
        var map = File.Exists(path) ? ParseFlat(File.ReadAllText(path)) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        map["display_mode"] = Quote(settings.DisplayMode);
        map["width"] = settings.Width.ToString(CultureInfo.InvariantCulture);
        map["height"] = settings.Height.ToString(CultureInfo.InvariantCulture);
        map["fps_cap"] = settings.FpsCap.ToString(CultureInfo.InvariantCulture);
        map["vsync"] = settings.VSync ? "true" : "false";
        map["render_percent"] = settings.RenderPercent.ToString(CultureInfo.InvariantCulture);
        map["upscaler"] = Quote(settings.Upscaler);
        map["upscale_quality"] = Quote(settings.UpscaleQuality);
        map["frame_generation"] = Quote(settings.FrameGeneration);
        map["dlss_preset"] = Quote(settings.DlssPreset);
        map["sharpness"] = settings.Sharpness.ToString(CultureInfo.InvariantCulture);
        map["fsr"] = settings.Fsr.ToString(CultureInfo.InvariantCulture);
        map["anisotropic_filtering"] = settings.Anisotropic.ToString(CultureInfo.InvariantCulture);
        map["perf_overlay"] = settings.PerfOverlay.ToString(CultureInfo.InvariantCulture);
        map["keyboard_controls"] = settings.KeyboardControls ? "true" : "false";
        map["invert_camera_y"] = settings.InvertCameraY ? "true" : "false";
        map["vibration"] = settings.Vibration ? "true" : "false";
        map["custom_textures"] = settings.CustomTextures ? "true" : "false";
        map["dump_textures"] = settings.DumpTextures ? "true" : "false";
        if (!map.ContainsKey("texture_dump_format"))
            map["texture_dump_format"] = "0";
        if (!map.ContainsKey("join_ip"))
            map["join_ip"] = Quote("");
        if (!map.ContainsKey("graphics_api"))
            map["graphics_api"] = Quote(settings.GraphicsApi);
        else
            map["graphics_api"] = Quote(settings.GraphicsApi);

        var launcher = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["language"] = settings.UserLanguage.ToString(CultureInfo.InvariantCulture),
            ["graphics_api"] = Quote(settings.GraphicsApi),
        };

        File.WriteAllText(path, FormatToml(map, launcher), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public static void LoadInto(LauncherSettings settings)
    {
        var path = PathFor(settings);
        if (!File.Exists(path)) return;
        var map = ParseFlat(File.ReadAllText(path));
        if (map.TryGetValue("display_mode", out var mode))
            settings.DisplayMode = Unquote(mode);
        if (map.TryGetValue("width", out var w) && uint.TryParse(w, out var width))
            settings.Width = width;
        if (map.TryGetValue("height", out var h) && uint.TryParse(h, out var height))
            settings.Height = height;
        if (map.TryGetValue("fps_cap", out var fps) && uint.TryParse(fps, out var cap))
            settings.FpsCap = cap > 240 && cap != 1 ? 50u : cap;
        if (map.TryGetValue("vsync", out var vs))
            settings.VSync = vs is "true" or "1";
        if (map.TryGetValue("render_percent", out var rp) && uint.TryParse(rp, out var percent))
            settings.RenderPercent = Math.Clamp(percent, 25u, 200u);
        if (map.TryGetValue("upscaler", out var up))
            settings.Upscaler = Unquote(up);
        if (map.TryGetValue("upscale_quality", out var uq))
            settings.UpscaleQuality = Unquote(uq);
        if (map.TryGetValue("frame_generation", out var fg))
            settings.FrameGeneration = Unquote(fg);
        if (map.TryGetValue("dlss_preset", out var dp))
            settings.DlssPreset = Unquote(dp);
        if (map.TryGetValue("sharpness", out var sh) && uint.TryParse(sh, out var sharp))
            settings.Sharpness = Math.Clamp(sharp, 0u, 100u);
        if (map.TryGetValue("anisotropic_filtering", out var an) && uint.TryParse(an, out var aniso))
            settings.Anisotropic = aniso;
        if (map.TryGetValue("perf_overlay", out var po) && uint.TryParse(po, out var overlay))
            settings.PerfOverlay = overlay;
        if (map.TryGetValue("keyboard_controls", out var kc))
            settings.KeyboardControls = kc is "true" or "1";
        if (map.TryGetValue("invert_camera_y", out var icy))
            settings.InvertCameraY = icy is "true" or "1";
        if (map.TryGetValue("vibration", out var vib))
            settings.Vibration = vib is "true" or "1";
        if (map.TryGetValue("custom_textures", out var ct))
            settings.CustomTextures = ct is "true" or "1";
        if (map.TryGetValue("dump_textures", out var dt))
            settings.DumpTextures = dt is "true" or "1";

        // Legacy: only `fsr` without upscaler → synthesize
        if (!map.ContainsKey("upscaler") && map.TryGetValue("fsr", out var fsrRaw) &&
            uint.TryParse(Unquote(fsrRaw), out var fsrQ) && fsrQ > 0)
        {
            settings.Upscaler = "fsr";
            settings.UpscaleQuality = fsrQ switch
            {
                1 => "ultra_quality",
                2 => "quality",
                3 => "balanced",
                _ => "performance",
            };
        }

        var text = File.ReadAllText(path);
        var m = Regex.Match(text, @"graphics_api\s*=\s*""?(?<v>[^""\r\n]+)""?", RegexOptions.IgnoreCase);
        if (m.Success)
            settings.GraphicsApi = Unquote(m.Groups["v"].Value.Trim());
    }

    static Dictionary<string, string> ParseFlat(string text)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith('[')) continue;
            var eq = line.IndexOf('=');
            if (eq <= 0) continue;
            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();
            if (key is "language") continue;
            map[key] = val;
        }
        return map;
    }

    static string FormatToml(Dictionary<string, string> root, Dictionary<string, string> launcher)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Written by WarriorsLauncher — compatible with Warriors Xbox Recomp keys");
        foreach (var key in new[]
                 {
                     "display_mode", "width", "height", "render_percent", "fps_cap", "vsync",
                     "upscaler", "upscale_quality", "frame_generation", "dlss_preset", "sharpness", "fsr",
                     "anisotropic_filtering", "perf_overlay", "keyboard_controls", "invert_camera_y", "vibration",
                     "custom_textures", "dump_textures", "texture_dump_format", "graphics_api", "join_ip",
                 })
        {
            if (root.TryGetValue(key, out var v))
                sb.AppendLine($"{key} = {v}");
        }
        foreach (var kv in root.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (kv.Key is "display_mode" or "width" or "height" or "render_percent" or "fps_cap" or "vsync"
                or "upscaler" or "upscale_quality" or "frame_generation" or "dlss_preset" or "sharpness" or "fsr"
                or "anisotropic_filtering" or "perf_overlay" or "keyboard_controls" or "invert_camera_y" or "vibration"
                or "custom_textures" or "dump_textures" or "texture_dump_format" or "graphics_api" or "join_ip"
                or "language")
                continue;
            sb.AppendLine($"{kv.Key} = {kv.Value}");
        }
        sb.AppendLine();
        sb.AppendLine("[warriors_launcher]");
        foreach (var kv in launcher)
            sb.AppendLine($"{kv.Key} = {kv.Value}");
        sb.AppendLine();
        return sb.ToString();
    }

    static string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    static string Unquote(string s)
    {
        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
            return s[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\");
        return s;
    }
}
