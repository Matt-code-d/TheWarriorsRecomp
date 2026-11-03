using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
    private bool _mirroring;
    private readonly DispatcherTimer _titleBgTimer = new();
    private int _titleBgIndex;
    private int _titleBgFps = 10;

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

        // Reference ImGui launcher tabs (reversed from Warriors Launcher.exe strings)
        _panels = new Dictionary<string, FrameworkElement>
        {
            ["HOME"] = PanelHome,
            ["VIDEO"] = PanelVideo,
            ["CONTROLS"] = PanelControls,
            ["GAME"] = PanelGame,
            ["ADVANCED"] = PanelAdvanced,
            ["ABOUT"] = PanelAbout,
        };
        _nav = new Dictionary<string, Button>
        {
            ["HOME"] = NavHome,
            ["VIDEO"] = NavVideo,
            ["CONTROLS"] = NavControls,
            ["GAME"] = NavGame,
            ["ADVANCED"] = NavAdvanced,
            ["ABOUT"] = NavAbout,
        };

        TxtIso.Text = _settings.IsoPath;
        FillCombos();
        SyncUiFromSettings();
        // Keep saved game language (launcher UI follows it). OS pick only when browsing a new ISO.
        RescanLanguages(preferOs: false);
        ApplyChrome();
        RefreshStatus();
        _titleBgTimer.Tick += (_, _) => AdvanceTitleBg();
        ShowPanel("HOME");
        Loaded += (_, _) =>
        {
            if (!FontVault.LoadedOk)
                TxtPlayMsg.Text = "Font pack failed: " + FontVault.Status;
            LoadTitleBgClip();
        };
        _uiReady = true;
    }

    /// <summary>Entire launcher chrome follows the selected game language (EUR XC_LANGUAGE).</summary>
    private void ApplyChrome()
    {
        var t = LauncherText.ForLanguageId(_settings.UserLanguage);
        _settings.UiLocale = LauncherText.CodeForId(_settings.UserLanguage);
        Title = t.WindowTitle;
        TxtVersion.Text = t.Version;

        NavHome.Content = t.NavHome;
        NavVideo.Content = t.NavVideo;
        NavControls.Content = t.NavControls;
        NavGame.Content = t.NavGame;
        NavAdvanced.Content = t.NavAdvanced;
        NavAbout.Content = t.NavAbout;

        LblHomeQuick.Text = t.QuickSettings;
        LblHomeSub.Text = t.HomeSub;
        LblHomeDisplay.Text = t.DisplayMode;
        LblHomeWindow.Text = t.WindowSize;
        LblHomeRender.Text = t.RenderResolution;
        LblHomeFpsCounter.Text = t.FpsCounter;
        LblHomeFps.Text = t.FrameRate;
        LblHomeVSync.Text = t.VSync;
        ChkVSync.Content = t.VSync;

        LblVideoTitle.Text = t.VideoTitle;
        LblVideoSub.Text = t.VideoSub;
        LblDisplaySec.Text = t.DisplaySec;
        LblRenderSec.Text = t.RenderingSec;
        LblVidDisplayMode.Text = t.DisplayMode;
        LblVidWindow.Text = t.WindowSize;
        LblVidVSync.Text = t.VSync;
        LblVidApi.Text = t.GraphicsApi;
        LblVidRender.Text = t.RenderResolution;
        LblVidUpscaler.Text = t.Upscaler;
        LblVidAniso.Text = t.TextureFiltering;
        LblVidFps.Text = t.FrameRate;
        LblVidFrameGen.Text = t.FrameGeneration;
        LblVidDlss.Text = t.DlssModel;
        LblVidSharp.Text = t.Sharpness;
        ChkCustomTex.Content = t.CustomTex;
        ChkVSyncVideo.Content = t.On;
        HintVidDisplay.Text = t.HintBorderless;
        HintVidWindow.Text = t.HintWindow;
        HintVidVSync.Text = t.HintVSync;
        HintVidApi.Text = t.HintApi;
        HintVidRender.Text = t.HintRender;
        HintVidUpscaler.Text = t.HintUpscaler;
        HintVidAniso.Text = t.HintAniso;
        HintVidFps.Text = t.HintFps;
        HintVidFrameGen.Text = t.HintFrameGen;
        HintVidDlss.Text = t.HintDlss;

        LblControlsTitle.Text = t.ControlsTitle;
        LblControlsSub.Text = t.ControlsSub;
        LblCtrlGeneral.Text = t.General;
        LblCtrlKb.Text = t.KeyboardMouse.ToUpperInvariant();
        ChkKeyboard.Content = t.KeyboardMouse;
        ChkInvertY.Content = t.InvertY;
        ChkVibration.Content = t.Vibration;
        LblControlsHint.Text = t.ControlsHint;

        LblGameTitle.Text = t.GameTitle;
        LblGameSub.Text = t.GameSub;
        LblLangSec.Text = t.LanguageSec;
        LblLangNote.Text = t.LangNote;
        LblYourGameSec.Text = t.YourGameSec;
        LblIsoPath.Text = t.DiscImage;
        BtnBrowseIso.Content = t.BrowseIso;
        BtnInstall.Content = t.Install;
        BtnOpenAssets.Content = t.OpenGameFolder;
        ChkExtract.Content = t.ExtractAlso;
        LblBinary.Text = t.Binary;
        LblAssets.Text = t.Assets;

        LblAdvTitle.Text = t.AdvTitle;
        LblAdvSub.Text = t.AdvSub;
        LblAdvVulkan.Text = t.AdvVulkan;
        TxtAdvVulkan.Text = t.AdvVulkanBody;
        LblAdvFiles.Text = t.AdvFiles;
        BtnOpenLogs.Content = t.AdvOpenLogs;
        BtnAdvOpenAssets.Content = t.OpenGameFolder;
        BtnOpenPort.Content = t.AdvOpenPort;
        LblAdvUpscaler.Text = t.AdvUpscaler;
        BtnRefreshDll.Content = t.AdvRefreshDll;
        LblAdvInGame.Text = t.AdvInGame;
        TxtAdvInGame.Text = t.AdvInGameBody;

        LblAboutTitle.Text = t.AboutTitle;
        LblAboutHow.Text = t.AboutHow;
        LblAboutBody.Text = t.AboutBody;
        LblAboutCredits.Text = t.AboutCredits;
        TxtAboutCredits.Text = CreditsVault.BodyFor(_settings.UiLocale);
        LblAboutLegal.Text = t.AboutLegal;
        TxtAboutLegal.Text = t.AboutLegalBody;

        BtnPlay.Content = t.BtnPlay;
        BtnSave.Content = t.Save;

        FillCombos();
        SyncUiFromSettings();
    }

    private void FillCombos()
    {
        var t = LauncherText.ForLanguageId(_settings.UserLanguage);
        var apis = new[]
        {
            new ComboOpt(t.OptD3d12, "d3d12"),
            new ComboOpt(t.OptVulkan, "vulkan"),
        };
        var modes = new[]
        {
            new ComboOpt(t.OptBorderless, "borderless"),
            new ComboOpt(t.OptFullscreen, "fullscreen"),
            new ComboOpt(t.OptWindowed, "windowed"),
        };
        var res = new[]
        {
            new ComboOpt(t.OptDesktop, "0x0"),
            new ComboOpt("1280 × 720", "1280x720"),
            new ComboOpt("1600 × 900", "1600x900"),
            new ComboOpt("1920 × 1080", "1920x1080"),
            new ComboOpt("2560 × 1440", "2560x1440"),
            new ComboOpt("3840 × 2160", "3840x2160"),
        };
        var fps = new[]
        {
            new ComboOpt("25", "25"),
            new ComboOpt("30", "30"),
            new ComboOpt("50 (PAL)", "50"),
            new ComboOpt("60", "60"),
            new ComboOpt("120", "120"),
            new ComboOpt("144", "144"),
            new ComboOpt("165", "165"),
            new ComboOpt("240", "240"),
            new ComboOpt(t.OptMonitorRefresh, "1"),
            new ComboOpt(t.OptUnlimited, "0"),
        };
        var render = new[]
        {
            new ComboOpt(t.OptMatchWindow, "100"),
            new ComboOpt("33%", "33"),
            new ComboOpt("50%", "50"),
            new ComboOpt($"59% ({t.OptBalanced})", "59"),
            new ComboOpt($"67% ({t.OptQuality})", "67"),
            new ComboOpt("75%", "75"),
            new ComboOpt($"77% ({t.OptUltraQuality})", "77"),
            new ComboOpt("125%", "125"),
            new ComboOpt("150%", "150"),
            new ComboOpt("200%", "200"),
        };

        CmbApi.ItemsSource = apis;
        CmbDisplayMode.ItemsSource = modes;
        CmbDisplayModeVideo.ItemsSource = modes;
        CmbResolution.ItemsSource = res;
        CmbResolutionVideo.ItemsSource = res;
        CmbFps.ItemsSource = fps;
        CmbFpsVideo.ItemsSource = fps;
        CmbRenderScale.ItemsSource = render;
        CmbRenderScaleVideo.ItemsSource = render;

        CmbPerfOverlay.ItemsSource = new[]
        {
            new ComboOpt(t.Off, "0"),
            new ComboOpt(t.On, "1"),
        };
        CmbUpscaler.ItemsSource = new[]
        {
            new ComboOpt(t.Off, "off"),
            new ComboOpt(t.OptFsr, "fsr"),
            new ComboOpt(t.OptDlss, "dlss"),
            new ComboOpt(t.OptXess, "xess"),
        };
        CmbUpscaleQuality.ItemsSource = new[]
        {
            new ComboOpt(t.OptNativeAa, "native"),
            new ComboOpt(t.OptUltraQuality, "ultra_quality"),
            new ComboOpt(t.OptQuality, "quality"),
            new ComboOpt(t.OptBalanced, "balanced"),
            new ComboOpt(t.OptPerformance, "performance"),
            new ComboOpt(t.OptUltraPerf, "ultra_performance"),
        };
        CmbFrameGen.ItemsSource = new[]
        {
            new ComboOpt(t.Off, "off"),
            new ComboOpt(t.OptFsrFg, "fsr"),
            new ComboOpt(t.OptDlssFg, "dlss"),
        };
        CmbDlssPreset.ItemsSource = new[]
        {
            new ComboOpt(t.OptDefault, "default"),
            new ComboOpt("DLSS 2x (RTX 40/50)", "dlss2"),
            new ComboOpt("DLSS 3x (RTX 50)", "dlss3"),
            new ComboOpt("DLSS 4x (RTX 50)", "dlss4"),
        };
        CmbAniso.ItemsSource = new[]
        {
            new ComboOpt(t.OptAnisoOff, "0"),
            new ComboOpt("2×", "2"),
            new ComboOpt("4×", "4"),
            new ComboOpt("8×", "8"),
            new ComboOpt("16×", "16"),
        };
    }

    private void SyncUiFromSettings()
    {
        _mirroring = true;
        SelectCombo(CmbApi, _settings.GraphicsApi);
        SelectCombo(CmbDisplayMode, _settings.DisplayMode);
        SelectCombo(CmbDisplayModeVideo, _settings.DisplayMode);
        SelectCombo(CmbResolution, $"{_settings.Width}x{_settings.Height}");
        SelectCombo(CmbResolutionVideo, $"{_settings.Width}x{_settings.Height}");
        if (_settings.FpsCap > 240 && _settings.FpsCap != 1) _settings.FpsCap = 50;
        SelectCombo(CmbFps, _settings.FpsCap.ToString());
        SelectCombo(CmbFpsVideo, _settings.FpsCap.ToString());
        SelectCombo(CmbRenderScale, _settings.RenderPercent.ToString());
        SelectCombo(CmbRenderScaleVideo, _settings.RenderPercent.ToString());
        SelectCombo(CmbPerfOverlay, _settings.PerfOverlay.ToString());
        SelectCombo(CmbUpscaler, _settings.Upscaler);
        SelectCombo(CmbUpscaleQuality, _settings.UpscaleQuality);
        SelectCombo(CmbFrameGen, _settings.FrameGeneration);
        SelectCombo(CmbDlssPreset, _settings.DlssPreset);
        SelectCombo(CmbAniso, _settings.Anisotropic.ToString());
        SldSharpness.Value = _settings.Sharpness;
        TxtSharpnessVal.Text = _settings.Sharpness.ToString();
        ChkVSync.IsChecked = _settings.VSync;
        ChkVSyncVideo.IsChecked = _settings.VSync;
        ChkCustomTex.IsChecked = _settings.CustomTextures;
        ChkKeyboard.IsChecked = _settings.KeyboardControls;
        ChkInvertY.IsChecked = _settings.InvertCameraY;
        ChkVibration.IsChecked = _settings.Vibration;
        _mirroring = false;
        UpdateRenderHint();
        RefreshAdvancedStatus();
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

    private void ReadUiIntoSettings()
    {
        if (CmbApi.SelectedItem is ComboOpt api) _settings.GraphicsApi = api.Value;
        if (CmbDisplayMode.SelectedItem is ComboOpt mode) _settings.DisplayMode = mode.Value;
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
            _settings.FpsCap = cap > 240 && cap != 1 ? 50u : cap;
        if (CmbRenderScale.SelectedItem is ComboOpt scale && uint.TryParse(scale.Value, out var percent))
            _settings.RenderPercent = percent;
        if (CmbPerfOverlay.SelectedItem is ComboOpt po && uint.TryParse(po.Value, out var overlay))
            _settings.PerfOverlay = overlay;
        if (CmbUpscaler.SelectedItem is ComboOpt up) _settings.Upscaler = up.Value;
        if (CmbUpscaleQuality.SelectedItem is ComboOpt uq) _settings.UpscaleQuality = uq.Value;
        if (CmbFrameGen.SelectedItem is ComboOpt fg) _settings.FrameGeneration = fg.Value;
        if (CmbDlssPreset.SelectedItem is ComboOpt dp) _settings.DlssPreset = dp.Value;
        if (CmbAniso.SelectedItem is ComboOpt an && uint.TryParse(an.Value, out var aniso))
            _settings.Anisotropic = aniso;
        _settings.Sharpness = (uint)Math.Clamp(SldSharpness.Value, 0, 100);
        _settings.VSync = ChkVSync.IsChecked == true;
        _settings.CustomTextures = ChkCustomTex.IsChecked == true;
        _settings.KeyboardControls = ChkKeyboard.IsChecked == true;
        _settings.InvertCameraY = ChkInvertY.IsChecked == true;
        _settings.Vibration = ChkVibration.IsChecked == true;
        UpdateRenderHint();
    }

    private void UpdateRenderHint()
    {
        var note = _settings.Upscaler.ToLowerInvariant() switch
        {
            "fsr" => "Host: FSR 1.0 spatial (built-in). Vendor FidelityFX DLLs detected separately in Advanced.",
            "dlss" => "DLSS needs Streamline in the host — not wired yet; falls back to FSR 1.0.",
            "xess" => "XeSS needs host wiring — falls back to FSR 1.0 for now.",
            _ => "Upscaler off.",
        };
        if (_settings.FrameGeneration is not "off" and not "")
            note += " Frame generation not available in this host yet.";
        TxtRenderHint.Text =
            $"{note}  upscaler={_settings.Upscaler} quality={_settings.UpscaleQuality} " +
            $"fsr={_settings.Fsr} render_percent={_settings.RenderPercent}";
    }

    private void Settings_Changed(object sender, RoutedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        ReadUiIntoSettings();
    }

    private void MirrorDisplay_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        if (CmbDisplayModeVideo.SelectedItem is ComboOpt opt)
        {
            _mirroring = true;
            SelectCombo(CmbDisplayMode, opt.Value);
            _mirroring = false;
            ReadUiIntoSettings();
        }
    }

    private void MirrorRes_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        if (CmbResolutionVideo.SelectedItem is ComboOpt opt)
        {
            _mirroring = true;
            SelectCombo(CmbResolution, opt.Value);
            _mirroring = false;
            ReadUiIntoSettings();
        }
    }

    private void MirrorFps_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        if (CmbFpsVideo.SelectedItem is ComboOpt opt)
        {
            _mirroring = true;
            SelectCombo(CmbFps, opt.Value);
            _mirroring = false;
            ReadUiIntoSettings();
        }
    }

    private void MirrorRender_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        if (CmbRenderScaleVideo.SelectedItem is ComboOpt opt)
        {
            _mirroring = true;
            SelectCombo(CmbRenderScale, opt.Value);
            _mirroring = false;
            ReadUiIntoSettings();
        }
    }

    private void MirrorVSync_Changed(object sender, RoutedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        _mirroring = true;
        ChkVSync.IsChecked = ChkVSyncVideo.IsChecked;
        _mirroring = false;
        ReadUiIntoSettings();
    }

    private void Upscaler_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_uiReady || _mirroring) return;
        string prevUp = _settings.Upscaler;
        string prevQ = _settings.UpscaleQuality;
        ReadUiIntoSettings();
        if (!string.Equals(prevUp, _settings.Upscaler, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(prevQ, _settings.UpscaleQuality, StringComparison.OrdinalIgnoreCase))
        {
            if (_settings.Upscaler is not "off" && _settings.UpscaleQuality is not "native")
            {
                uint suggested = LauncherSettings.SuggestedRenderPercent(_settings.UpscaleQuality);
                _settings.RenderPercent = suggested;
                _mirroring = true;
                SelectCombo(CmbRenderScale, suggested.ToString());
                SelectCombo(CmbRenderScaleVideo, suggested.ToString());
                _mirroring = false;
            }
            else
            {
                _settings.RenderPercent = 100;
                _mirroring = true;
                SelectCombo(CmbRenderScale, "100");
                SelectCombo(CmbRenderScaleVideo, "100");
                _mirroring = false;
            }
        }
        UpdateRenderHint();
    }

    private void Sharpness_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtSharpnessVal != null)
            TxtSharpnessVal.Text = ((int)e.NewValue).ToString();
        if (!_uiReady) return;
        _settings.Sharpness = (uint)Math.Clamp(e.NewValue, 0, 100);
        UpdateRenderHint();
    }

    private void ShowPanel(string name)
    {
        foreach (var kv in _panels)
            kv.Value.Visibility = kv.Key == name ? Visibility.Visible : Visibility.Collapsed;
        foreach (var kv in _nav)
            kv.Value.Tag = kv.Key == name ? "active" : null;
        if (name == "HOME")
        {
            LoadTitleBgClip();
            _titleBgTimer.Start();
        }
        else
            _titleBgTimer.Stop();
        if (name == "ADVANCED")
            RefreshAdvancedStatus();
    }

    private BitmapImage[] _titleBgImages = [];

    private void LoadTitleBgClip()
    {
        _titleBgIndex = 0;
        _titleBgImages = TitleBgVault.Frames.ToArray();
        _titleBgFps = TitleBgVault.Fps;
        _titleBgTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / Math.Max(1, _titleBgFps));
        if (_titleBgImages.Length == 0)
        {
            ImgHomeBg.Source = null;
            return;
        }
        ImgHomeBg.Source = _titleBgImages[0];
    }

    private void AdvanceTitleBg()
    {
        if (_titleBgImages.Length == 0) return;
        _titleBgIndex = (_titleBgIndex + 1) % _titleBgImages.Length;
        ImgHomeBg.Source = _titleBgImages[_titleBgIndex];
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b) return;
        string key = "HOME";
        if (b == NavVideo) key = "VIDEO";
        else if (b == NavControls) key = "CONTROLS";
        else if (b == NavGame) key = "GAME";
        else if (b == NavAdvanced) key = "ADVANCED";
        else if (b == NavAbout) key = "ABOUT";
        ShowPanel(key);
    }

    private void RefreshAdvanced_Click(object sender, RoutedEventArgs e) => RefreshAdvancedStatus();

    private void RefreshAdvancedStatus()
    {
        _settings.EnsureDefaults(_projectRoot);
        TxtUpscalerStatus.Text = UpscalerStatus.Describe(_settings.PortRoot);
    }

    private void RefreshStatus()
    {
        _settings.EnsureDefaults(_projectRoot);
        var exe = Paths.FindWarriorsExe(_settings.PortRoot);
        TxtExe.Text = File.Exists(exe) ? exe : "(Warriors.exe missing)";
        TxtAssets.Text = _settings.AssetsRoot;
        StatusInstall.Text = GameProcess.DescribeInstall(_settings);
        var t = LauncherText.ForLanguageId(_settings.UserLanguage);
        var lang = LanguageCatalog.All.FirstOrDefault(l => l.Id == _settings.UserLanguage);
        StatusLang.Text = lang != null
            ? $"{t.StatusLanguage}: {lang.DisplayName}"
            : $"{t.StatusLanguage} id {_settings.UserLanguage}";
        bool ready = File.Exists(exe);
        BtnPlay.IsEnabled = ready;
        BtnPlay.Opacity = ready ? 1 : 0.45;
        TxtReady.Text = ready ? t.Ready : t.ReadyMissing;
        TxtReady.Foreground = ready
            ? (Brush)FindResource("Bone")
            : (Brush)FindResource("Mute");
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
        ApplyChrome();
        var ui = LauncherText.ForLanguageId(lang.Id);
        TxtLangHint.Text = string.Format(ui.LangApplied, lang.Id, lang.DisplayName);
        RefreshStatus();
    }

    private void RescanLang_Click(object sender, RoutedEventArgs e) => RescanLanguages(preferOs: false);

    private void BrowseIso_Click(object sender, RoutedEventArgs e)
    {
        var t = LauncherText.ForLanguageId(_settings.UserLanguage);
        var dlg = new OpenFileDialog
        {
            Filter = "Xbox disc image (*.iso)|*.iso|All files (*.*)|*.*",
            Title = t.IsoBrowseTitle,
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
            TxtInstallDetail.Text = "Done. Press PLAY — first launch compiles the game DLL.";
            ShowPanel("HOME");
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

    private void PersistAndApply()
    {
        if (!string.IsNullOrWhiteSpace(TxtIso.Text))
            _settings.IsoPath = TxtIso.Text.Trim();
        ReadUiIntoSettings();
        _settings.EnsureDefaults(_projectRoot);
        _settings.Save();
        GameSettingsToml.ApplyFromLauncher(_settings);
        GameProcess.WriteLanguageSidecar(_settings);
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PersistAndApply();
            GameProcess.Start(_settings);
            TxtPlayMsg.Text = LauncherText.ForLanguageId(_settings.UserLanguage).MsgLaunched;
            WindowState = WindowState.Minimized;
        }
        catch (Exception ex)
        {
            TxtPlayMsg.Text = ex.Message;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        PersistAndApply();
        TxtPlayMsg.Text = LauncherText.ForLanguageId(_settings.UserLanguage).MsgSaved;
        RefreshStatus();
    }

    private void OpenAssets_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnsureDefaults(_projectRoot);
        Directory.CreateDirectory(_settings.AssetsRoot);
        Process.Start(new ProcessStartInfo { FileName = _settings.AssetsRoot, UseShellExecute = true });
    }

    private void OpenPort_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnsureDefaults(_projectRoot);
        Directory.CreateDirectory(_settings.PortRoot);
        Process.Start(new ProcessStartInfo { FileName = _settings.PortRoot, UseShellExecute = true });
    }

    private void OpenLogs_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnsureDefaults(_projectRoot);
        var logs = Path.Combine(_settings.PortRoot, "logs");
        Directory.CreateDirectory(logs);
        Process.Start(new ProcessStartInfo { FileName = logs, UseShellExecute = true });
    }
}
