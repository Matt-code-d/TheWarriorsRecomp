using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WarriorsLauncher.Models;

namespace WarriorsLauncher.Services;

/// <summary>Reads/writes the host <c>settings.toml</c> beside Warriors.exe.</summary>
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
        map["anisotropic_filtering"] = settings.Anisotropic.ToString(CultureInfo.InvariantCulture);
        map["custom_textures"] = settings.CustomTextures ? "true" : "false";
        map["dump_textures"] = settings.DumpTextures ? "true" : "false";
        if (!map.ContainsKey("texture_dump_format"))
            map["texture_dump_format"] = "0";
        if (!map.ContainsKey("join_ip"))
            map["join_ip"] = Quote("");

        // Persist API choice for the launcher; the current host is D3D12-only.
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
            settings.FpsCap = cap;
        if (map.TryGetValue("vsync", out var vs))
            settings.VSync = vs is "true" or "1";
        if (map.TryGetValue("render_percent", out var rp) && uint.TryParse(rp, out var percent))
            settings.RenderPercent = Math.Clamp(percent, 25u, 200u);
        if (map.TryGetValue("anisotropic_filtering", out var an) && uint.TryParse(an, out var aniso))
            settings.Anisotropic = aniso;
        if (map.TryGetValue("custom_textures", out var ct))
            settings.CustomTextures = ct is "true" or "1";
        if (map.TryGetValue("dump_textures", out var dt))
            settings.DumpTextures = dt is "true" or "1";

        // [warriors_launcher] graphics_api
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
            // skip nested table keys we don't own at root
            if (key is "language" or "graphics_api") continue;
            map[key] = val;
        }
        return map;
    }

    static string FormatToml(Dictionary<string, string> root, Dictionary<string, string> launcher)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Written by WarriorsLauncher — host settings for Warriors.exe");
        foreach (var key in new[]
                 {
                     "display_mode", "width", "height", "render_percent", "fps_cap", "vsync",
                     "anisotropic_filtering", "custom_textures", "dump_textures", "texture_dump_format", "join_ip",
                 })
        {
            if (root.TryGetValue(key, out var v))
                sb.AppendLine($"{key} = {v}");
        }
        foreach (var kv in root.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (kv.Key is "display_mode" or "width" or "height" or "render_percent" or "fps_cap" or "vsync"
                or "anisotropic_filtering" or "custom_textures" or "dump_textures" or "texture_dump_format"
                or "join_ip")
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
