using System.IO.Compression;
using System.Windows.Media;

namespace WarriorsLauncher.Services;

/// <summary>
/// UI typeface from obfuscated embedded blob (deflate + XOR). No .ttf ships beside the exe.
/// </summary>
public static class FontVault
{
    static FontFamily? _family;
    static string? _tempPath;
    static readonly object Gate = new();

    static readonly byte[] Key =
    {
        (byte)'W', (byte)'A', (byte)'R', (byte)'R', (byte)'I', (byte)'O', (byte)'R', (byte)'S',
        (byte)'R', (byte)'E', (byte)'C', (byte)'O', (byte)'M', (byte)'P', (byte)'2', (byte)'6',
    };

    public const string FaceName = "NBA Warriors";

    public static bool LoadedOk { get; private set; }

    public static string Status { get; private set; } = "not loaded";

    public static FontFamily Family
    {
        get
        {
            lock (Gate)
            {
                if (_family != null) return _family;
                try
                {
                    byte[] bytes = Unpack();
                    // Must use a real font extension for WPF/DirectWrite to accept the file.
                    _tempPath = Path.Combine(Path.GetTempPath(), $"twr-{Guid.NewGuid():N}.ttf");
                    File.WriteAllBytes(_tempPath, bytes);

                    // Correct WPF custom-font ctor (file:// string concat often fails silently).
                    var uri = new Uri(_tempPath, UriKind.Absolute);
                    var fam = new FontFamily(uri, "./#" + FaceName);

                    int faces = 0;
                    foreach (var _ in fam.GetTypefaces()) faces++;
                    if (faces == 0)
                        throw new InvalidOperationException("DirectWrite found 0 typefaces for " + FaceName);

                    _family = fam;
                    LoadedOk = true;
                    Status = $"ok faces={faces} path={_tempPath}";
                    AppDomain.CurrentDomain.ProcessExit += (_, _) => TryDeleteTemp();
                }
                catch (Exception ex)
                {
                    LoadedOk = false;
                    Status = ex.GetType().Name + ": " + ex.Message;
                    _family = new FontFamily("Bahnschrift Condensed, Segoe UI");
                }
                return _family;
            }
        }
    }

    static byte[] Unpack()
    {
        var asm = typeof(FontVault).Assembly;
        using var stream = asm.GetManifestResourceStream("WarriorsLauncher.Assets.chrome.bin")
            ?? throw new InvalidOperationException("missing WarriorsLauncher.Assets.chrome.bin");
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
        var ttf = outMs.ToArray();
        if (ttf.Length < 100 || ttf[0] != 0 || ttf[1] != 1)
            throw new InvalidOperationException("blob did not decompress to a TTF");
        return ttf;
    }

    static void TryDeleteTemp()
    {
        try
        {
            if (_tempPath != null && File.Exists(_tempPath))
                File.Delete(_tempPath);
        }
        catch { /* ignore */ }
    }
}
