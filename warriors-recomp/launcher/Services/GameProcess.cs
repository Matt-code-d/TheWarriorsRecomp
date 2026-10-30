using System.Diagnostics;
using System.Text;
using WarriorsLauncher.Models;

namespace WarriorsLauncher.Services;

public static class GameProcess
{
    public static Process Start(LauncherSettings settings)
    {
        var exe = Paths.FindWarriorsExe(settings.PortRoot);
        if (!File.Exists(exe))
            throw new FileNotFoundException(
                "Warriors.exe not found. Extract Warriors.zip into the port/ folder first.", exe);

        var assets = settings.AssetsRoot;
        var hasIso = Directory.Exists(assets) &&
                     Directory.EnumerateFiles(assets, "*.iso").Any();
        var hasXbe = File.Exists(Path.Combine(assets, "default.xbe"));
        if (!hasIso && !hasXbe)
            throw new InvalidOperationException(
                "No game in assets. Use SETUP to place your EUR ISO (or extracted default.xbe) into assets/.");

        WriteLanguageSidecar(settings);
        GameSettingsToml.ApplyFromLauncher(settings);

        var psi = new ProcessStartInfo
        {
            FileName = exe,
            WorkingDirectory = Path.GetDirectoryName(exe)!,
            UseShellExecute = false,
        };

        psi.Environment["XBE_LANGUAGE"] = settings.UserLanguage.ToString();
        psi.Environment["XC_LANGUAGE"] = settings.UserLanguage.ToString();

        var p = Process.Start(psi);
        if (p == null) throw new InvalidOperationException("Failed to start Warriors.exe");
        return p;
    }

    /// <summary>language.cfg beside the exe (XC_LANGUAGE). Graphics live in settings.toml.</summary>
    public static void WriteLanguageSidecar(LauncherSettings settings)
    {
        var port = settings.PortRoot;
        Directory.CreateDirectory(port);
        File.WriteAllText(Path.Combine(port, "language.cfg"), settings.UserLanguage.ToString(), Encoding.ASCII);
    }

    public static string DescribeInstall(LauncherSettings settings)
    {
        var assets = settings.AssetsRoot;
        if (!Directory.Exists(assets))
            return "assets/ missing — run SETUP";

        var isos = Directory.EnumerateFiles(assets, "*.iso").Select(Path.GetFileName).ToList();
        var xbe = File.Exists(Path.Combine(assets, "default.xbe"));
        if (isos.Count == 0 && !xbe)
            return "No ISO / default.xbe in assets";
        if (isos.Count > 0 && xbe)
            return $"ISO + extracted files ({isos[0]})";
        if (isos.Count > 0)
            return $"Disc image: {isos[0]}";
        return "Extracted files (default.xbe)";
    }
}
