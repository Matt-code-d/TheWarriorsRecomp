using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace WarriorsLauncher.Services;

/// <summary>
/// Packed HOME background clip (TWB1 JPEG frames, deflate + XOR). Embedded as title_bg.bin.
/// </summary>
public static class TitleBgVault
{
    static readonly object Gate = new();
    static BitmapImage[]? _frames;
    static int _fps = 10;

    static readonly byte[] Key =
    {
        (byte)'W', (byte)'A', (byte)'R', (byte)'R', (byte)'I', (byte)'O', (byte)'R', (byte)'S',
        (byte)'R', (byte)'E', (byte)'C', (byte)'O', (byte)'M', (byte)'P', (byte)'2', (byte)'6',
    };

    public static bool LoadedOk { get; private set; }
    public static string Status { get; private set; } = "not loaded";
    public static int Fps => Ensure().fps;
    public static IReadOnlyList<BitmapImage> Frames => Ensure().frames;

    static (BitmapImage[] frames, int fps) Ensure()
    {
        lock (Gate)
        {
            if (_frames != null) return (_frames, _fps);
            try
            {
                var raw = Unpack();
                using var ms = new MemoryStream(raw);
                using var br = new BinaryReader(ms);
                var magic = br.ReadBytes(4);
                if (magic.Length != 4 || magic[0] != 'T' || magic[1] != 'W' || magic[2] != 'B' || magic[3] != '1')
                    throw new InvalidOperationException("bad title_bg magic");

                // Packed clips sit around 5–8; 6 is a touch slower than 7 for HOME.
                _fps = Math.Clamp(br.ReadInt32(), 1, 30);
                if (_fps is >= 5 and <= 8) _fps = 6;
                int count = br.ReadInt32();
                _ = br.ReadInt32(); // width
                _ = br.ReadInt32(); // height
                if (count <= 0 || count > 600)
                    throw new InvalidOperationException("bad frame count " + count);

                var list = new BitmapImage[count];
                for (int i = 0; i < count; i++)
                {
                    int len = br.ReadInt32();
                    if (len <= 0 || len > 8_000_000) throw new InvalidOperationException("bad jpeg size");
                    byte[] jpeg = br.ReadBytes(len);
                    if (jpeg.Length != len) throw new EndOfStreamException();
                    list[i] = DecodeJpeg(jpeg);
                }

                _frames = list;
                LoadedOk = true;
                Status = $"ok frames={count} fps={_fps}";
            }
            catch (Exception ex)
            {
                LoadedOk = false;
                Status = ex.GetType().Name + ": " + ex.Message;
                _frames = [];
                _fps = 10;
            }
            return (_frames, _fps);
        }
    }

    static BitmapImage DecodeJpeg(byte[] jpeg)
    {
        var bmp = new BitmapImage();
        using var ms = new MemoryStream(jpeg, writable: false);
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    static byte[] Unpack()
    {
        var asm = typeof(TitleBgVault).Assembly;
        using var stream = asm.GetManifestResourceStream("WarriorsLauncher.Assets.title_bg.bin")
            ?? throw new InvalidOperationException("missing WarriorsLauncher.Assets.title_bg.bin");
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
