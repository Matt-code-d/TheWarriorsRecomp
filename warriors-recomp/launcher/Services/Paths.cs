namespace WarriorsLauncher.Services;

public static class Paths
{
    /// <summary>warriors-recomp root (CMakeLists + launcher/), never the loose port/ alpha folder alone.</summary>
    public static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 12 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = dir.FullName;

            // Source / EUR tree
            if (Directory.Exists(Path.Combine(candidate, "launcher")) &&
                File.Exists(Path.Combine(candidate, "CMakeLists.txt")))
                return candidate;

            // Workspace that contains warriors-recomp/
            var nested = Path.Combine(candidate, "warriors-recomp");
            if (Directory.Exists(Path.Combine(nested, "launcher")) &&
                File.Exists(Path.Combine(nested, "CMakeLists.txt")))
                return nested;
        }

        // Packaged: launcher next to Warriors.exe (release folder)
        dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Warriors.exe")))
                return dir.FullName;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }

    /// <summary>Folder with Warriors.exe — prefers EUR out/build/release over port/ NTSC zip.</summary>
    public static string FindPortRoot(string projectRoot)
    {
        string[] candidates =
        [
            Path.Combine(projectRoot, "out", "build", "release"),
            AppContext.BaseDirectory,
            projectRoot,
            Path.Combine(Path.GetDirectoryName(projectRoot) ?? projectRoot, "port"),
            Path.Combine(projectRoot, "port"),
        ];
        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(Path.Combine(full, "Warriors.exe")))
                return full;
        }
        return Path.GetFullPath(Path.Combine(projectRoot, "out", "build", "release"));
    }

    public static string FindWarriorsExe(string portRoot) =>
        Path.Combine(portRoot, "Warriors.exe");

    public static string FindExtractXiso(string projectRoot)
    {
        var workspace = Path.GetDirectoryName(projectRoot) ?? projectRoot;
        string[] candidates =
        [
            Path.Combine(workspace, "tools", "extract-xiso", "artifacts", "extract-xiso.exe"),
            Path.Combine(projectRoot, "tools", "extract-xiso", "artifacts", "extract-xiso.exe"),
            Path.Combine(AppContext.BaseDirectory, "extract-xiso.exe"),
        ];
        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(full)) return full;
        }
        return "";
    }
}
