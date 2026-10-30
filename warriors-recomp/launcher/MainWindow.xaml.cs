using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WarriorsLauncher.Models;
using WarriorsLauncher.Services;

namespace WarriorsLauncher;

public partial class MainWindow : Window
{
    private readonly string _projectRoot;
    private LauncherSettings _settings;
    private CancellationTokenSource? _installCts;
    private readonly Dictionary<string, FrameworkElement> _panels;
    private readonly Dictionary<string, Button> _nav;
    private bool _uiReady;

    private sealed record ComboOpt(string Label, string Value)
    {
        public override string ToString() => Label;
    }

    public MainWindow()
    {
        InitializeComponent();
        _projectRoot = Paths.FindProjectRoot();
        _settings = LauncherSettings.Load();
        _settings.EnsureDefaults(_projectRoot);
        GameSettingsToml.LoadInto(_settings);

        _panels = new Dictionary<string, FrameworkElement>
        {
            ["PLAY"] = PanelPlay,
            ["GRAPHICS"] = PanelGraphics,
            ["SETUP"] = PanelSetup,
            ["LANGUAGE"] = PanelLanguage,
        };
        _nav = new Dictionary<string, Button>
        {
            ["PLAY"] = NavPlay,
            ["GRAPHICS"] = NavGraphics,
            ["SETUP"] = NavSetup,
            ["LANGUAGE"] = NavLanguage,
        };

        TxtIso.Text = _settings.IsoPath;
        FillGraphicsCombos();
        SyncGraphicsUiFromSettings();
        RefreshStatus();
        RescanLanguages(preferOs: true);
        ApplyLauncherUi();
        ShowPanel("PLAY");
        _uiReady = true;
    }

    private void ApplyLauncherUi()
    {
        var t = LauncherText.ForLanguageId(_settings.UserLanguage);
        _settings.UiLocale = LauncherText.CodeForId(_settings.UserLanguage);
        Title = t.WindowTitle;
        NavPlay.Content = t.NavPlay;
        NavGraphics.Content = t.NavGraphics;
        NavSetup.Content = t.NavSetup;
        NavLanguage.Content = t.NavLanguage;
        LblPlayTitle.Text = t.PlayTitle;
        LblPlaySub.Text = t.PlaySub;
        LblGameBinary.Text = t.GameBinary;
        LblAssets.Text = t.Assets;
        LblPlayNote.Text = t.PlayNote;
        BtnPlay.Content = t.BtnPlay;
        BtnOpenPort.Content = t.OpenPort;
        BtnSave.Content = t.Save;
        LblSetupTitle.Text = t.SetupTitle;
        LblSetupSub.Text = t.SetupSub;
        LblIsoPath.Text = t.IsoPath;
        BtnBrowseIso.Content = t.Browse;
        ChkExtract.Content = t.ExtractAlso;
        BtnInstall.Content = t.Install;
        BtnOpenAssets.Content = t.OpenAssets;
        LblGfxTitle.Text = t.GfxTitle;
        LblGfxSub.Text = t.GfxSub;
        LblGfxApi.Text = t.GfxApi;
        LblDisplayMode.Text = t.DisplayMode;
        LblResolution.Text = t.Resolution;
        LblFpsCap.Text = t.FpsCap;
        LblRenderScale.Text = t.RenderScale;
        LblAniso.Text = t.Aniso;
        ChkVSync.Content = t.VSync;
        ChkCustomTex.Content = t.CustomTex;
        BtnApplyGfx.Content = t.ApplyGfx;
        BtnResetGfx.Content = t.ResetGfx;
        LblLangTitle.Text = t.LangTitle;
        LblLangSub.Text = t.LangSub;
        LblGameLanguage.Text = t.GameLanguage;
        BtnRescanLang.Content = t.RescanLang;
    }

    private void FillGraphicsCombos()
    {
        CmbApi.ItemsSource = new[]
        {
            new ComboOpt("Direct3D 12 (supported)", "d3d12"),
            new ComboOpt("Vulkan (not available yet)", "vulkan"),
        };
        CmbDisplayMode.ItemsSource = new[]
        {
            new ComboOpt("Borderless", "borderless"),
            new ComboOpt("Fullscreen", "fullscreen"),
            new ComboOpt("Windowed", "windowed"),
        };
        CmbResolution.ItemsSource = new[]
        {
            new ComboOpt("Desktop (native)", "0x0"),
            new ComboOpt("1280 × 720", "1280x720"),
            new ComboOpt("1600 × 900", "1600x900"),
            new ComboOpt("1920 × 1080", "1920x1080"),
            new ComboOpt("2560 × 1440", "2560x1440"),
            new ComboOpt("3840 × 2160", "3840x2160"),
        };
        CmbFps.ItemsSource = new[]
        {
            new ComboOpt("Unlimited", "0"),
            new ComboOpt("Monitor refresh", "1"),
            new ComboOpt("30", "30"),
            new ComboOpt("60", "60"),
            new ComboOpt("120", "120"),
            new ComboOpt("144", "144"),
            new ComboOpt("165", "165"),
            new ComboOpt("240", "240"),
        };
        CmbRenderScale.ItemsSource = new[]
        {
            new ComboOpt("50%", "50"),
            new ComboOpt("75%", "75"),
            new ComboOpt("100%", "100"),
            new ComboOpt("125%", "125"),
            new ComboOpt("150%", "150"),
            new ComboOpt("200%", "200"),
        };
        CmbAniso.ItemsSource = new[]
        {
            new ComboOpt("Game default", "0"),
            new ComboOpt("2×", "2"),
            new ComboOpt("4×", "4"),
            new ComboOpt("8×", "8"),
            new ComboOpt("16×", "16"),
        };
    }

    private void SyncGraphicsUiFromSettings()
    {
        SelectCombo(CmbApi, _settings.GraphicsApi);
        SelectCombo(CmbDisplayMode, _settings.DisplayMode);
        SelectCombo(CmbResolution, $"{_settings.Width}x{_settings.Height}");
        SelectCombo(CmbFps, _settings.FpsCap.ToString());
        SelectCombo(CmbRenderScale, _settings.RenderPercent.ToString());
        SelectCombo(CmbAniso, _settings.Anisotropic.ToString());
        ChkVSync.IsChecked = _settings.VSync;
        ChkCustomTex.IsChecked = _settings.CustomTextures;
        UpdateGfxHint();
    }

    private static void SelectCombo(ComboBox box, string value)
    {
        foreach (ComboOpt opt in box.Items)
        {
            if (string.Equals(opt.Value, value, StringComparison.OrdinalIgnoreCase))
            {
                box.SelectedItem = opt;
                return;
            }
        }
        if (box.Items.Count > 0) box.SelectedIndex = 0;
    }

    private void ReadGraphicsUiIntoSettings()
    {
        if (CmbApi.SelectedItem is ComboOpt api)
            _settings.GraphicsApi = api.Value;
        if (CmbDisplayMode.SelectedItem is ComboOpt mode)
            _settings.DisplayMode = mode.Value;
        if (CmbResolution.SelectedItem is ComboOpt res)
        {
            var parts = res.Value.Split('x', '×');
            if (parts.Length == 2 &&
                uint.TryParse(parts[0].Trim(), out var w) &&
                uint.TryParse(parts[1].Trim(), out var h))
            {
                _settings.Width = w;
                _settings.Height = h;
            }
        }
        if (CmbFps.SelectedItem is ComboOpt fps && uint.TryParse(fps.Value, out var cap))
            _settings.FpsCap = cap;
        if (CmbRenderScale.SelectedItem is ComboOpt scale && uint.TryParse(scale.Value, out var percent))
            _settings.RenderPercent = percent;
        if (CmbAniso.SelectedItem is ComboOpt an && uint.TryParse(an.Value, out var aniso))
            _settings.Anisotropic = aniso;
        _settings.VSync = ChkVSync.IsChecked == true;
        _settings.CustomTextures = ChkCustomTex.IsChecked == true;
        UpdateGfxHint();
    }

    private void UpdateGfxHint()
    {
        var api = _settings.GraphicsApi;
        if (string.Equals(api, "vulkan", StringComparison.OrdinalIgnoreCase))
        {
            TxtGfxHint.Text =
                "Vulkan is not implemented in XboxXBERecomp yet — PLAY still uses Direct3D 12. " +
                "Your choice is saved for when a Vulkan backend exists.";
        }
        else
        {
            TxtGfxHint.Text =
                $"Will write display_mode={_settings.DisplayMode}, " +
                $"{(_settings.Width == 0 ? "desktop" : $"{_settings.Width}x{_settings.Height}")}, " +
                $"fps_cap={_settings.FpsCap}, vsync={_settings.VSync}, " +
                $"render_percent={_settings.RenderPercent} → settings.toml";
        }
    }

    private void Graphics_Changed(object sender, RoutedEventArgs e)
    {
        if (!_uiReady) return;
        ReadGraphicsUiIntoSettings();
    }

    private void ApplyGraphics_Click(object sender, RoutedEventArgs e)
    {
        ReadGraphicsUiIntoSettings();
        _settings.EnsureDefaults(_projectRoot);
        _settings.Save();
        GameSettingsToml.ApplyFromLauncher(_settings);
        GameProcess.WriteLanguageSidecar(_settings);
        TxtGfxHint.Text = $"Applied → {GameSettingsToml.PathFor(_settings)}";
        RefreshStatus();
    }

    private void ResetGraphics_Click(object sender, RoutedEventArgs e)
    {
        _settings.DisplayMode = "borderless";
        _settings.Width = 0;
        _settings.Height = 0;
        _settings.FpsCap = 60;
        _settings.VSync = true;
        _settings.RenderPercent = 100;
        _settings.Anisotropic = 0;
        _settings.CustomTextures = true;
        _settings.GraphicsApi = "d3d12";
        SyncGraphicsUiFromSettings();
        ApplyGraphics_Click(sender, e);
    }

    private void ShowPanel(string name)
    {
        foreach (var kv in _panels)
            kv.Value.Visibility = kv.Key == name ? Visibility.Visible : Visibility.Collapsed;
        foreach (var kv in _nav)
            kv.Value.Tag = kv.Key == name ? "active" : null;
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b) return;
        string key = "PLAY";
        if (b == NavSetup) key = "SETUP";
        else if (b == NavLanguage) key = "LANGUAGE";
        else if (b == NavGraphics) key = "GRAPHICS";
        ShowPanel(key);
    }

    private void RefreshStatus()
    {
        _settings.EnsureDefaults(_projectRoot);
        var exe = Paths.FindWarriorsExe(_settings.PortRoot);
        TxtExe.Text = File.Exists(exe) ? exe : "(Warriors.exe missing — extract Warriors.zip into port/)";
        TxtAssets.Text = _settings.AssetsRoot;
        StatusInstall.Text = GameProcess.DescribeInstall(_settings);
        var ui = LauncherText.ForLanguageId(_settings.UserLanguage);
        var lang = LanguageCatalog.All.FirstOrDefault(l => l.Id == _settings.UserLanguage);
        StatusLang.Text = lang != null
            ? $"{ui.StatusLanguage}: {lang.DisplayName}"
            : $"{ui.StatusLanguage} id {_settings.UserLanguage}";
        BtnPlay.IsEnabled = File.Exists(exe);
        BtnPlay.Opacity = BtnPlay.IsEnabled ? 1 : 0.45;
    }

    private void RescanLanguages(bool preferOs)
    {
        var langs = LanguageCatalog.DetectFromIsoName(_settings.IsoPath);
        uint pick = preferOs ? LanguageCatalog.PreferForOs(langs) : _settings.UserLanguage;
        if (langs.All(l => l.Id != pick))
            pick = langs[0].Id;

        CmbLanguage.SelectionChanged -= Language_Changed;
        CmbLanguage.ItemsSource = langs;
        CmbLanguage.SelectedItem = langs.FirstOrDefault(l => l.Id == pick) ?? langs.FirstOrDefault();
        CmbLanguage.SelectionChanged += Language_Changed;

        if (CmbLanguage.SelectedItem is DetectedLanguage sel)
        {
            _settings.UserLanguage = sel.Id;
            _settings.UiLocale = LauncherText.CodeForId(sel.Id);
            var ui = LauncherText.ForLanguageId(sel.Id);
            TxtLangHint.Text = string.Format(
                ui.LangHint,
                sel.Id,
                sel.DisplayName,
                string.Join(", ", langs.Select(l => l.DisplayName)));
        }
        ApplyLauncherUi();
        RefreshStatus();
    }

    private void Language_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady) return;
        if (CmbLanguage.SelectedItem is not DetectedLanguage lang) return;
        _settings.UserLanguage = lang.Id;
        _settings.UiLocale = LauncherText.CodeForId(lang.Id);
        _settings.Save();
        GameProcess.WriteLanguageSidecar(_settings);
        GameSettingsToml.ApplyFromLauncher(_settings);
        ApplyLauncherUi();
        var ui = LauncherText.ForLanguageId(lang.Id);
        TxtLangHint.Text = string.Format(ui.LangApplied, lang.Id, lang.DisplayName);
        RefreshStatus();
    }

    private void RescanLang_Click(object sender, RoutedEventArgs e) => RescanLanguages(preferOs: false);

    private void BrowseIso_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Xbox disc image (*.iso)|*.iso|All files (*.*)|*.*",
            Title = LauncherText.ForLanguageId(_settings.UserLanguage).IsoBrowseTitle,
        };
        if (!string.IsNullOrWhiteSpace(_settings.IsoPath) && File.Exists(_settings.IsoPath))
            dlg.InitialDirectory = Path.GetDirectoryName(_settings.IsoPath);
        if (dlg.ShowDialog() != true) return;
        TxtIso.Text = dlg.FileName;
        _settings.IsoPath = dlg.FileName;
        _settings.Save();
        RescanLanguages(preferOs: true);
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        var iso = TxtIso.Text.Trim();
        if (!File.Exists(iso))
        {
            TxtInstallDetail.Text = "Pick a valid .iso first.";
            return;
        }

        _settings.IsoPath = iso;
        _settings.EnsureDefaults(_projectRoot);
        _installCts?.Cancel();
        _installCts = new CancellationTokenSource();
        BtnInstall.IsEnabled = false;
        BarInstall.Value = 0;
        TxtInstallDetail.Text = "Starting…";

        var progress = new Progress<IsoInstallProgress>(p =>
        {
            BarInstall.Value = p.Fraction < 0 ? 0 : p.Fraction;
            TxtInstallDetail.Text = string.IsNullOrWhiteSpace(p.Detail)
                ? p.Phase
                : $"{p.Phase}: {p.Detail}";
        });

        try
        {
            var installer = new IsoInstaller();
            await installer.InstallIsoIntoAssetsAsync(
                iso,
                _settings.AssetsRoot,
                ChkExtract.IsChecked == true,
                Paths.FindExtractXiso(_projectRoot),
                progress,
                _installCts.Token);

            _settings.Save();
            RescanLanguages(preferOs: false);
            RefreshStatus();
            TxtInstallDetail.Text = "Done. You can PLAY — first launch compiles the game DLL.";
            ShowPanel("PLAY");
        }
        catch (Exception ex)
        {
            TxtInstallDetail.Text = ex.Message;
        }
        finally
        {
            BtnInstall.IsEnabled = true;
        }
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings.EnsureDefaults(_projectRoot);
            if (!string.IsNullOrWhiteSpace(TxtIso.Text))
                _settings.IsoPath = TxtIso.Text.Trim();
            ReadGraphicsUiIntoSettings();
            _settings.Save();
            GameSettingsToml.ApplyFromLauncher(_settings);
            GameProcess.WriteLanguageSidecar(_settings);
            GameProcess.Start(_settings);
            TxtPlayMsg.Text = "Launched. First start may open a compile progress window.";
            WindowState = WindowState.Minimized;
        }
        catch (Exception ex)
        {
            TxtPlayMsg.Text = ex.Message;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(TxtIso.Text))
            _settings.IsoPath = TxtIso.Text.Trim();
        ReadGraphicsUiIntoSettings();
        _settings.Save();
        GameSettingsToml.ApplyFromLauncher(_settings);
        GameProcess.WriteLanguageSidecar(_settings);
        TxtPlayMsg.Text = "Settings saved.";
        RefreshStatus();
    }

    private void OpenPort_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnsureDefaults(_projectRoot);
        Directory.CreateDirectory(_settings.PortRoot);
        Process.Start(new ProcessStartInfo
        {
            FileName = _settings.PortRoot,
            UseShellExecute = true,
        });
    }

    private void OpenAssets_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnsureDefaults(_projectRoot);
        Directory.CreateDirectory(_settings.AssetsRoot);
        Process.Start(new ProcessStartInfo
        {
            FileName = _settings.AssetsRoot,
            UseShellExecute = true,
        });
    }
}
