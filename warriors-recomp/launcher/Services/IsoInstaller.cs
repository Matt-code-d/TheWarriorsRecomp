using System.Diagnostics;
using System.Text;

namespace WarriorsLauncher.Services;

public sealed class IsoInstallProgress
{
    public string Phase { get; init; } = "";
    public string Detail { get; init; } = "";
    public double Fraction { get; init; }
}

public sealed class IsoInstaller
{
    /// <summary>
    /// Install mode for the Antriksh XBERecomp release: drop the .iso into assets/
    /// (preferred — the host mounts disc images), or optionally extract with extract-xiso.
    /// </summary>
    public async Task InstallIsoIntoAssetsAsync(
        string isoPath,
        string assetsRoot,
        bool extractFiles,
        string extractXisoPath,
        IProgress<IsoInstallProgress>? progress,
        CancellationToken ct)
    {
        if (!File.Exists(isoPath))
            throw new FileNotFoundException("ISO not found", isoPath);

        Directory.CreateDirectory(assetsRoot);

        progress?.Report(new IsoInstallProgress
        {
            Phase = "prepare",
            Detail = Path.GetFileName(isoPath),
            Fraction = 0.05
        });

        var destIso = Path.Combine(assetsRoot, Path.GetFileName(isoPath));
        if (!PathsEqual(isoPath, destIso))
        {
            progress?.Report(new IsoInstallProgress
            {
                Phase = "copy",
                Detail = "Linking ISO into assets…",
                Fraction = 0.2
            });

            // Prefer hardlink (no extra 7 GB). Fall back to copy.
            if (File.Exists(destIso))
            {
                try { File.Delete(destIso); } catch { /* keep existing */ }
            }

            if (!File.Exists(destIso))
            {
                try
                {
                    FileHardLink.CreateHardLink(destIso, Path.GetFullPath(isoPath));
                }
                catch
                {
                    await Task.Run(() => File.Copy(isoPath, destIso, overwrite: true), ct);
                }
            }
        }

        if (!extractFiles)
        {
            progress?.Report(new IsoInstallProgress
            {
                Phase = "done",
                Detail = "ISO ready in assets (disc-image mode).",
                Fraction = 1.0
            });
            return;
        }

        if (!File.Exists(extractXisoPath))
            throw new FileNotFoundException(
                "extract-xiso.exe not found under tools/extract-xiso/artifacts/",
                extractXisoPath);

        progress?.Report(new IsoInstallProgress
        {
            Phase = "extract",
            Detail = "Extracting disc files…",
            Fraction = 0.35
        });

        var staging = Path.Combine(Path.GetTempPath(), "warriors_iso_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = extractXisoPath,
                Arguments = $"-x \"{isoPath}\"",
                WorkingDirectory = staging,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var sb = new StringBuilder();
            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                sb.AppendLine(e.Data);
                progress?.Report(new IsoInstallProgress
                {
                    Phase = "extract",
                    Detail = TrimProgressLine(e.Data),
                    Fraction = 0.5
                });
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                sb.AppendLine(e.Data);
            };

            if (!proc.Start())
                throw new InvalidOperationException("Failed to start extract-xiso");

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await proc.WaitForExitAsync(ct);

            var xbe = Directory.EnumerateFiles(staging, "default.xbe", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (xbe == null)
                throw new InvalidOperationException(
                    "Extraction finished but default.xbe was not found.\n\n" + sb);

            var discRoot = Path.GetDirectoryName(xbe)!;
            progress?.Report(new IsoInstallProgress
            {
                Phase = "install",
                Detail = "Copying extracted files into assets…",
                Fraction = 0.8
            });

            foreach (var entry in Directory.EnumerateFileSystemEntries(discRoot))
            {
                var name = Path.GetFileName(entry);
                var dest = Path.Combine(assetsRoot, name);
                if (Directory.Exists(entry))
                    CopyDirectory(entry, dest);
                else
                    File.Copy(entry, dest, overwrite: true);
            }

            progress?.Report(new IsoInstallProgress
            {
                Phase = "done",
                Detail = "Extracted files installed.",
                Fraction = 1.0
            });
        }
        finally
        {
            try { Directory.Delete(staging, recursive: true); } catch { /* ignore */ }
        }
    }

    static bool PathsEqual(string a, string b) =>
        string.Equals(Path.GetFullPath(a).TrimEnd('\\'), Path.GetFullPath(b).TrimEnd('\\'),
            StringComparison.OrdinalIgnoreCase);

    static string TrimProgressLine(string line)
    {
        line = line.Trim();
        return line.Length > 90 ? line[..90] + "…" : line;
    }

    static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.EnumerateFiles(src))
            File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.EnumerateDirectories(src))
            CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
    }
}

file static class FileHardLink
{
    public static void CreateHardLink(string newPath, string existingPath)
    {
        if (!CreateHardLinkW(newPath, existingPath, IntPtr.Zero))
            throw new IOException($"CreateHardLink failed for {newPath}");
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern bool CreateHardLinkW(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
}
