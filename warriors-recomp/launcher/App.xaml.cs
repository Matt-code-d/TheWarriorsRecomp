using System.Windows;
using WarriorsLauncher.Services;

namespace WarriorsLauncher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Custom athletic face ONLY on sidebar NavBtn (MenuFont). Body stays Segoe (UiFont).
        Resources["MenuFont"] = FontVault.Family;
    }
}
