namespace WarriorsLauncher.Services;

/// <summary>Detects vendor upscaler DLLs beside the game exe.</summary>
public static class UpscalerStatus
{
    public static string Describe(string portRoot)
    {
        static bool Has(string root, string name) => File.Exists(Path.Combine(root, name));

        bool fsr1Local = Directory.Exists(Path.Combine(portRoot, "fsr"));
        bool ffxUpscaler = Has(portRoot, "amd_fidelityfx_upscaler_dx12.dll");
        bool ffxLoader = Has(portRoot, "amd_fidelityfx_loader_dx12.dll");
        bool ffxFg = Has(portRoot, "amd_fidelityfx_framegeneration_dx12.dll");
        bool dlss = Has(portRoot, "nvngx_dlss.dll");
        bool streamline = Has(portRoot, "sl.interposer.dll");
        bool dlssg = Has(portRoot, "nvngx_dlssg.dll") || Has(portRoot, "sl.dlss_g.dll");
        bool xess = Has(portRoot, "libxess.dll");

        return
            "Copied from reference / expected beside Warriors.exe:\n" +
            $"  FSR 1.0 shaders (fsr\\): {(fsr1Local ? "yes — used by this host" : "no")}\n" +
            $"  AMD FidelityFX upscaler: {(ffxUpscaler ? "yes" : "no")}  loader: {(ffxLoader ? "yes" : "no")}\n" +
            $"  AMD FidelityFX frame gen: {(ffxFg ? "yes" : "no")}\n" +
            $"  NVIDIA DLSS (nvngx_dlss): {(dlss ? "yes" : "no")}  Streamline: {(streamline ? "yes" : "no")}\n" +
            $"  DLSS Frame Gen: {(dlssg ? "yes" : "no")}\n" +
            $"  Intel XeSS: {(xess ? "yes" : "no")}\n\n" +
            "This EUR host currently runs FSR 1.0 in-process. DLSS / XeSS / FG need Streamline / XeSS / FidelityFX hooks in Warriors.exe (reference has them; we do not yet).";
    }
}
