namespace WarriorsLauncher.Services;

/// <summary>All launcher chrome follows the selected game language (EUR XC_LANGUAGE).</summary>
public static class LauncherText
{
    public sealed class Bundle
    {
        public string WindowTitle { get; init; } = "";
        public string NavHome { get; init; } = "";
        public string NavVideo { get; init; } = "";
        public string NavControls { get; init; } = "";
        public string NavGame { get; init; } = "";
        public string NavAdvanced { get; init; } = "";
        public string NavAbout { get; init; } = "";
        public string QuickSettings { get; init; } = "";
        public string FirstSetup { get; init; } = "";
        public string FirstSetupSub { get; init; } = "";
        public string ReadyNeedSetup { get; init; } = "";
        public string HomeSub { get; init; } = "";
        public string DisplayMode { get; init; } = "";
        public string WindowSize { get; init; } = "";
        public string RenderResolution { get; init; } = "";
        public string FpsCounter { get; init; } = "";
        public string FrameRate { get; init; } = "";
        public string VSync { get; init; } = "";
        public string VideoTitle { get; init; } = "";
        public string VideoSub { get; init; } = "";
        public string DisplaySec { get; init; } = "";
        public string RenderingSec { get; init; } = "";
        public string GraphicsApi { get; init; } = "";
        public string Upscaler { get; init; } = "";
        public string TextureFiltering { get; init; } = "";
        public string FrameGeneration { get; init; } = "";
        public string DlssModel { get; init; } = "";
        public string Sharpness { get; init; } = "";
        public string CustomTex { get; init; } = "";
        public string ControlsTitle { get; init; } = "";
        public string ControlsSub { get; init; } = "";
        public string General { get; init; } = "";
        public string KeyboardMouse { get; init; } = "";
        public string InvertY { get; init; } = "";
        public string Vibration { get; init; } = "";
        public string ControlsHint { get; init; } = "";
        public string GameTitle { get; init; } = "";
        public string GameSub { get; init; } = "";
        public string LanguageSec { get; init; } = "";
        public string LangNote { get; init; } = "";
        public string YourGameSec { get; init; } = "";
        public string DiscImage { get; init; } = "";
        public string BrowseIso { get; init; } = "";
        public string Install { get; init; } = "";
        public string OpenGameFolder { get; init; } = "";
        public string ExtractAlso { get; init; } = "";
        public string LauncherSec { get; init; } = "";
        public string Binary { get; init; } = "";
        public string Assets { get; init; } = "";
        public string AdvTitle { get; init; } = "";
        public string AdvSub { get; init; } = "";
        public string AboutTitle { get; init; } = "";
        public string AboutBody { get; init; } = "";
        public string BtnPlay { get; init; } = "";
        public string Save { get; init; } = "";
        public string Ready { get; init; } = "";
        public string ReadyMissing { get; init; } = "";
        public string StatusLanguage { get; init; } = "";
        public string LangHint { get; init; } = "";
        public string LangApplied { get; init; } = "";
        public string IsoBrowseTitle { get; init; } = "";
        public string Version { get; init; } = "";
        public string MsgSaved { get; init; } = "";
        public string MsgLaunched { get; init; } = "";
        public string On { get; init; } = "";
        public string Off { get; init; } = "";
        public string OptBorderless { get; init; } = "";
        public string OptFullscreen { get; init; } = "";
        public string OptWindowed { get; init; } = "";
        public string OptDesktop { get; init; } = "";
        public string OptMonitorRefresh { get; init; } = "";
        public string OptUnlimited { get; init; } = "";
        public string OptMatchWindow { get; init; } = "";
        public string OptBalanced { get; init; } = "";
        public string OptQuality { get; init; } = "";
        public string OptUltraQuality { get; init; } = "";
        public string OptPerformance { get; init; } = "";
        public string OptUltraPerf { get; init; } = "";
        public string OptNativeAa { get; init; } = "";
        public string OptD3d12 { get; init; } = "";
        public string OptVulkan { get; init; } = "";
        public string OptFsr { get; init; } = "";
        public string OptDlss { get; init; } = "";
        public string OptXess { get; init; } = "";
        public string OptFsrFg { get; init; } = "";
        public string OptDlssFg { get; init; } = "";
        public string OptDefault { get; init; } = "";
        public string OptAnisoOff { get; init; } = "";
        public string HintBorderless { get; init; } = "";
        public string HintWindow { get; init; } = "";
        public string HintVSync { get; init; } = "";
        public string HintApi { get; init; } = "";
        public string HintRender { get; init; } = "";
        public string HintUpscaler { get; init; } = "";
        public string HintAniso { get; init; } = "";
        public string HintFps { get; init; } = "";
        public string HintFrameGen { get; init; } = "";
        public string HintDlss { get; init; } = "";
        public string PresetSec { get; init; } = "";
        public string Preset { get; init; } = "";
        public string HintPreset { get; init; } = "";
        public string BtnUseRecommended { get; init; } = "";
        public string OptCustom { get; init; } = "";
        public string GpuLine { get; init; } = "";
        public string GpuLineUnknown { get; init; } = "";
        public string AdvVulkan { get; init; } = "";
        public string AdvVulkanBody { get; init; } = "";
        public string AdvFiles { get; init; } = "";
        public string AdvOpenLogs { get; init; } = "";
        public string AdvOpenPort { get; init; } = "";
        public string AdvUpscaler { get; init; } = "";
        public string AdvRefreshDll { get; init; } = "";
        public string AdvInGame { get; init; } = "";
        public string AdvInGameBody { get; init; } = "";
        public string AboutHow { get; init; } = "";
        public string AboutCredits { get; init; } = "";
        public string AboutCreditsBody { get; init; } = "";
        public string AboutLegal { get; init; } = "";
        public string AboutLegalBody { get; init; } = "";
    }

    static readonly Dictionary<string, Bundle> ByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = En(),
        ["it"] = It(),
        ["de"] = De(),
        ["fr"] = Fr(),
        ["es"] = Es(),
    };

    public static Bundle ForLanguageId(uint xcLanguage)
    {
        var lang = LanguageCatalog.ById(xcLanguage);
        if (lang != null && ByCode.TryGetValue(lang.Code, out var b)) return b;
        return ByCode["en"];
    }

    public static string CodeForId(uint xcLanguage) =>
        LanguageCatalog.ById(xcLanguage)?.Code ?? "en";

    static Bundle En() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavHome = "HOME", NavVideo = "VIDEO", NavControls = "CONTROLS",
        NavGame = "GAME", NavAdvanced = "ADVANCED", NavAbout = "ABOUT",
        QuickSettings = "QUICK SETTINGS",
        FirstSetup = "FIRST SETUP",
        FirstSetupSub = "Bring your own legally owned European disc image, then install it into assets. Settings unlock after that.",
        ReadyNeedSetup = "Finish FIRST SETUP before you play.",
        HomeSub = "How the game is shown: the window, the 3D resolution and the frame rate.",
        DisplayMode = "Display mode", WindowSize = "Window size",
        RenderResolution = "Render resolution", FpsCounter = "FPS counter",
        FrameRate = "Frame rate", VSync = "VSync",
        VideoTitle = "VIDEO", VideoSub = "How the game is shown: the window, the 3D resolution and the frame rate.",
        DisplaySec = "DISPLAY", RenderingSec = "RENDERING",
        GraphicsApi = "Graphics API", Upscaler = "Upscaler",
        TextureFiltering = "Texture filtering", FrameGeneration = "Frame generation",
        DlssModel = "DLSS model", Sharpness = "Sharpness",
        CustomTex = "Custom textures",
        ControlsTitle = "CONTROLS",
        ControlsSub = "Keyboard, mouse and controller all work together; whichever you use answers.",
        General = "GENERAL", KeyboardMouse = "Keyboard and mouse",
        InvertY = "Invert camera Y", Vibration = "Vibration",
        ControlsHint = "WASD move. Click to capture mouse look; F1 toggles, Esc releases (Esc is not Back — use Backspace). Space / Shift / E / F actions.",
        GameTitle = "GAME",
        GameSub = "Your copy of the game, its language and the launcher itself.",
        LanguageSec = "LANGUAGE",
        LangNote = "English is the standard. Others play where your disc has them. The launcher UI follows this language.",
        YourGameSec = "YOUR GAME", DiscImage = "Disc image",
        BrowseIso = "Choose the ISO…", Install = "Install into assets",
        OpenGameFolder = "Open the game folder",
        ExtractAlso = "Also extract with extract-xiso (optional)",
        LauncherSec = "LAUNCHER",
        Binary = "Binary", Assets = "Assets",
        AdvTitle = "ADVANCED", AdvSub = "Logs, folders and runtime.",
        AboutTitle = "ABOUT",
        AboutBody = "The game's Xbox code is statically recompiled into native x64 (XboxXBERecomp). Nothing from the game ships with the port.",
        BtnPlay = "PLAY", Save = "Save",
        Ready = "Ready to play. Changes are saved when you press Play.",
        ReadyMissing = "Warriors.exe missing — build or sync the port folder.",
        StatusLanguage = "Language",
        LangHint = "Game language → XC_LANGUAGE={0} ({1}). Packs: {2}. Launcher UI uses this language.",
        LangApplied = "Applied XC_LANGUAGE={0} ({1}). Launcher UI updated.",
        IsoBrowseTitle = "Select The Warriors EUR ISO",
        Version = "EUR · native recompilation",
        MsgSaved = "Saved.", MsgLaunched = "Launched. First start may open a compile progress window.",
        On = "On", Off = "Off",
        OptBorderless = "Borderless", OptFullscreen = "Fullscreen", OptWindowed = "Windowed",
        OptDesktop = "Desktop (native)", OptMonitorRefresh = "Monitor refresh", OptUnlimited = "Unlimited",
        OptMatchWindow = "Match the window", OptBalanced = "Balanced", OptQuality = "Quality",
        OptUltraQuality = "Ultra Quality", OptPerformance = "Performance", OptUltraPerf = "Ultra Performance",
        OptNativeAa = "Native AA (DLAA)",
        OptD3d12 = "Direct3D 12 (supported)", OptVulkan = "Vulkan (needs vkd3d-proton — not wired yet)",
        OptFsr = "AMD FSR (any GPU)", OptDlss = "DLSS (NVIDIA RTX)", OptXess = "XeSS (any GPU)",
        OptFsrFg = "FSR Frame Gen", OptDlssFg = "DLSS Frame Gen + Reflex", OptDefault = "Default",
        OptAnisoOff = "Off (as the game asks)",
        HintBorderless = "Borderless fills the screen and switches instantly.",
        HintWindow = "Output size.",
        HintVSync = "Turn VSync off for rates above your monitor's.",
        HintApi = "D3D12 only for now. Vulkan is disabled until it is wired.",
        HintRender = "The frame's height. Match the window = 100%.",
        HintUpscaler = "AMD FSR / DLSS / XeSS. This host runs FSR 1.0; others fall back until wired.",
        HintAniso = "Anisotropic filtering keeps far-off ground sharp.",
        HintFps = "The game runs every frame natively at this rate.",
        HintFrameGen = "Generated frames between the game's own. Not in this host yet.",
        HintDlss = "Which DLSS model runs.",
        PresetSec = "GRAPHICS PRESET",
        Preset = "Preset",
        HintPreset = "Bundles upscaler, filmic and filtering for your GPU tier.",
        BtnUseRecommended = "Use recommended",
        OptCustom = "Custom",
        GpuLine = "Detected: {0}{1} — tier {2} — recommended: {3}",
        GpuLineUnknown = "GPU not detected — recommended: Balanced.",
        AdvVulkan = "VULKAN",
        AdvVulkanBody = "Vulkan is disabled in this build. The host runs Direct3D 12 only; the API selector cannot switch.",
        AdvFiles = "FILES", AdvOpenLogs = "Open the logs folder", AdvOpenPort = "Open port folder",
        AdvUpscaler = "UPSCALER RUNTIME", AdvRefreshDll = "Refresh DLL status",
        AdvInGame = "IN GAME",
        AdvInGameBody = "F10 toggles the performance overlay setting.",
        AboutHow = "HOW IT WORKS", AboutCredits = "CREDITS",
        // Body comes from CreditsVault (obfuscated notice.bin) — keep empty here.
        AboutCreditsBody = "",
        AboutLegal = "LEGAL",
        AboutLegalBody = "The Warriors is a trademark of its owners. This project is not affiliated with Rockstar Games, Take-Two Interactive or Paramount. Bring your own legally owned EUR dump. MIT covers only original repo work — see LICENSE.",
    };

    static Bundle It() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavHome = "HOME", NavVideo = "VIDEO", NavControls = "COMANDI",
        NavGame = "GIOCO", NavAdvanced = "AVANZATE", NavAbout = "INFO",
        QuickSettings = "IMPOSTAZIONI RAPIDE",
        FirstSetup = "PRIMO SETUP",
        FirstSetupSub = "Porta la tua immagine disco EUR di proprietà legale, poi installala in assets. Le impostazioni si sbloccano dopo.",
        ReadyNeedSetup = "Completa il PRIMO SETUP prima di giocare.",
        HomeSub = "Come viene mostrato il gioco: finestra, risoluzione 3D e frame rate.",
        DisplayMode = "Modalità display", WindowSize = "Dimensione finestra",
        RenderResolution = "Risoluzione render", FpsCounter = "Contatore FPS",
        FrameRate = "Frame rate", VSync = "VSync",
        VideoTitle = "VIDEO", VideoSub = "Come viene mostrato il gioco: finestra, risoluzione 3D e frame rate.",
        DisplaySec = "DISPLAY", RenderingSec = "RENDERING",
        GraphicsApi = "API grafica", Upscaler = "Upscaler",
        TextureFiltering = "Filtro texture", FrameGeneration = "Frame generation",
        DlssModel = "Modello DLSS", Sharpness = "Nitidezza",
        CustomTex = "Texture custom",
        ControlsTitle = "COMANDI",
        ControlsSub = "Tastiera, mouse e controller funzionano insieme; risponde quello che usi.",
        General = "GENERALE", KeyboardMouse = "Tastiera e mouse",
        InvertY = "Inverti camera Y", Vibration = "Vibrazione",
        ControlsHint = "WASD muovi. Click per catturare il mouse; F1 alterna, Esc rilascia (Esc non è Indietro — usa Backspace). Spazio / Shift / E / F azioni.",
        GameTitle = "GIOCO",
        GameSub = "La tua copia del gioco, la lingua e il launcher.",
        LanguageSec = "LINGUA",
        LangNote = "L’inglese è lo standard. Le altre lingue dove il disco le ha. L’interfaccia del launcher segue questa lingua.",
        YourGameSec = "IL TUO GIOCO", DiscImage = "Immagine disco",
        BrowseIso = "Scegli l’ISO…", Install = "Installa in assets",
        OpenGameFolder = "Apri cartella gioco",
        ExtractAlso = "Estrai anche con extract-xiso (opzionale)",
        LauncherSec = "LAUNCHER",
        Binary = "Eseguibile", Assets = "Assets",
        AdvTitle = "AVANZATE", AdvSub = "Log, cartelle e runtime.",
        AboutTitle = "INFO",
        AboutBody = "Il codice Xbox è ricompilato in x64 nativo (XboxXBERecomp). Nessun file di gioco è incluso nel port.",
        BtnPlay = "GIOCA", Save = "Salva",
        Ready = "Pronto. Le modifiche si salvano quando premi Gioca.",
        ReadyMissing = "Manca Warriors.exe — compila o sincronizza la cartella port.",
        StatusLanguage = "Lingua",
        LangHint = "Lingua di gioco → XC_LANGUAGE={0} ({1}). Pack: {2}. UI launcher in questa lingua.",
        LangApplied = "Applicato XC_LANGUAGE={0} ({1}). UI launcher aggiornata.",
        IsoBrowseTitle = "Seleziona l’ISO EUR di The Warriors",
        Version = "EUR · ricompilazione nativa",
        MsgSaved = "Salvato.", MsgLaunched = "Avviato. Al primo avvio può aprirsi la finestra di compilazione.",
        On = "On", Off = "Off",
        OptBorderless = "Borderless", OptFullscreen = "Schermo intero", OptWindowed = "Finestra",
        OptDesktop = "Desktop (nativo)", OptMonitorRefresh = "Refresh monitor", OptUnlimited = "Illimitato",
        OptMatchWindow = "Come la finestra", OptBalanced = "Bilanciato", OptQuality = "Qualità",
        OptUltraQuality = "Ultra qualità", OptPerformance = "Prestazioni", OptUltraPerf = "Ultra prestazioni",
        OptNativeAa = "AA nativo (DLAA)",
        OptD3d12 = "Direct3D 12 (supportato)", OptVulkan = "Vulkan (serve vkd3d-proton — non ancora collegato)",
        OptFsr = "AMD FSR (qualsiasi GPU)", OptDlss = "DLSS (NVIDIA RTX)", OptXess = "XeSS (qualsiasi GPU)",
        OptFsrFg = "FSR Frame Gen", OptDlssFg = "DLSS Frame Gen + Reflex", OptDefault = "Predefinito",
        OptAnisoOff = "Off (come chiede il gioco)",
        HintBorderless = "Borderless riempie lo schermo e cambia subito.",
        HintWindow = "Dimensione in uscita.",
        HintVSync = "Disattiva VSync per frame rate sopra il monitor.",
        HintApi = "Solo D3D12 per ora. Vulkan disabilitato finché non è collegato.",
        HintRender = "Altezza del frame. Come la finestra = 100%.",
        HintUpscaler = "AMD FSR / DLSS / XeSS. Questo host usa FSR 1.0; gli altri tornano indietro finché non collegati.",
        HintAniso = "Il filtro anisotropico tiene nitido il terreno lontano.",
        HintFps = "Il gioco gira ogni frame in modo nativo a questo rate.",
        HintFrameGen = "Frame generati tra quelli del gioco. Non ancora in questo host.",
        HintDlss = "Quale modello DLSS viene usato.",
        PresetSec = "PRESET GRAFICI",
        Preset = "Preset",
        HintPreset = "Raggruppa upscaler, filmic e filtri in base al tier GPU.",
        BtnUseRecommended = "Usa consigliato",
        OptCustom = "Personalizzato",
        GpuLine = "Rilevata: {0}{1} — tier {2} — consigliato: {3}",
        GpuLineUnknown = "GPU non rilevata — consigliato: Bilanciato.",
        AdvVulkan = "VULKAN",
        AdvVulkanBody = "Vulkan è disabilitato in questa build. L’host usa solo Direct3D 12; il selettore API non può cambiare.",
        AdvFiles = "FILE", AdvOpenLogs = "Apri cartella log", AdvOpenPort = "Apri cartella port",
        AdvUpscaler = "RUNTIME UPSCALER", AdvRefreshDll = "Aggiorna stato DLL",
        AdvInGame = "IN GIOCO",
        AdvInGameBody = "F10 cambia l'impostazione overlay prestazioni.",
        AboutHow = "COME FUNZIONA", AboutCredits = "CREDITI",
        AboutCreditsBody = "",
        AboutLegal = "NOTE LEGALI",
        AboutLegalBody = "The Warriors è un marchio dei rispettivi proprietari. Progetto non affiliato a Rockstar Games, Take-Two o Paramount. Usa un dump EUR di tua proprietà. MIT solo sul lavoro originale del repo — vedi LICENSE.",
    };

    static Bundle De() => WithEnFallback(new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavHome = "HOME", NavVideo = "VIDEO", NavControls = "STEUERUNG",
        NavGame = "SPIEL", NavAdvanced = "ERWEITERT", NavAbout = "INFO",
        QuickSettings = "SCHNELLEINSTELLUNGEN",
        HomeSub = "Darstellung: Fenster, 3D-Auflösung und Bildrate.",
        DisplayMode = "Anzeigemodus", WindowSize = "Fenstergröße",
        RenderResolution = "Renderauflösung", FpsCounter = "FPS-Zähler",
        FrameRate = "Bildrate", VSync = "VSync",
        VideoTitle = "VIDEO", VideoSub = "Darstellung: Fenster, 3D-Auflösung und Bildrate.",
        DisplaySec = "ANZEIGE", RenderingSec = "RENDERING",
        GraphicsApi = "Grafik-API", Upscaler = "Upscaler",
        TextureFiltering = "Texturfilter", FrameGeneration = "Frame Generation",
        DlssModel = "DLSS-Modell", Sharpness = "Schärfe", CustomTex = "Eigene Texturen",
        ControlsTitle = "STEUERUNG",
        ControlsSub = "Tastatur, Maus und Controller funktionieren zusammen.",
        General = "ALLGEMEIN", KeyboardMouse = "Tastatur und Maus",
        InvertY = "Kamera Y umkehren", Vibration = "Vibration",
        ControlsHint = "Volles Remapping folgt später. Flags gehen in settings.toml.",
        GameTitle = "SPIEL", GameSub = "Dein Spiel, Sprache und Launcher.",
        LanguageSec = "SPRACHE",
        LangNote = "Englisch ist Standard. Andere Sprachen je nach Disc. Die Launcher-UI folgt dieser Sprache.",
        YourGameSec = "DEIN SPIEL", DiscImage = "Disc-Image",
        BrowseIso = "ISO wählen…", Install = "In assets installieren",
        OpenGameFolder = "Spielordner öffnen",
        ExtractAlso = "Auch mit extract-xiso entpacken (optional)",
        LauncherSec = "LAUNCHER",
        Binary = "Programm", Assets = "Assets",
        AdvTitle = "ERWEITERT", AdvSub = "Hintergrund, Logs und Ordner.",
        AboutTitle = "INFO",
        AboutBody = "Xbox-Code wird nativ nach x64 recompiliert (XboxXBERecomp). Keine Spieldaten im Port.",
        BtnPlay = "SPIELEN", Save = "Speichern",
        Ready = "Bereit. Änderungen speichern sich beim Spielen.",
        ReadyMissing = "Warriors.exe fehlt — Port-Ordner bauen/syncen.",
        StatusLanguage = "Sprache",
        LangHint = "Spielsprache → XC_LANGUAGE={0} ({1}). Packs: {2}. Launcher-UI in dieser Sprache.",
        LangApplied = "XC_LANGUAGE={0} ({1}) gesetzt. Launcher-UI aktualisiert.",
        IsoBrowseTitle = "EUR-ISO von The Warriors wählen",
        Version = "EUR · native Rekompilation",
        MsgSaved = "Gespeichert.", MsgLaunched = "Gestartet. Erster Start kann das Compile-Fenster öffnen.",
    });

    static Bundle Fr() => WithEnFallback(new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavHome = "ACCUEIL", NavVideo = "VIDEO", NavControls = "COMMANDES",
        NavGame = "JEU", NavAdvanced = "AVANCÉ", NavAbout = "À PROPOS",
        QuickSettings = "RÉGLAGES RAPIDES",
        HomeSub = "Affichage : fenêtre, résolution 3D et fréquence d’images.",
        DisplayMode = "Mode d’affichage", WindowSize = "Taille de fenêtre",
        RenderResolution = "Résolution de rendu", FpsCounter = "Compteur FPS",
        FrameRate = "Fréquence", VSync = "VSync",
        VideoTitle = "VIDEO", VideoSub = "Affichage : fenêtre, résolution 3D et fréquence d’images.",
        DisplaySec = "AFFICHAGE", RenderingSec = "RENDU",
        GraphicsApi = "API graphique", Upscaler = "Upscaler",
        TextureFiltering = "Filtrage texture", FrameGeneration = "Frame generation",
        DlssModel = "Modèle DLSS", Sharpness = "Netteté", CustomTex = "Textures custom",
        ControlsTitle = "COMMANDES",
        ControlsSub = "Clavier, souris et manette fonctionnent ensemble.",
        General = "GÉNÉRAL", KeyboardMouse = "Clavier et souris",
        InvertY = "Inverser caméra Y", Vibration = "Vibration",
        ControlsHint = "Le remapping complet viendra plus tard. Flags écrits dans settings.toml.",
        GameTitle = "JEU", GameSub = "Votre copie, la langue et le launcher.",
        LanguageSec = "LANGUE",
        LangNote = "L’anglais est la norme. Autres langues selon le disque. L’UI du launcher suit cette langue.",
        YourGameSec = "VOTRE JEU", DiscImage = "Image disque",
        BrowseIso = "Choisir l’ISO…", Install = "Installer dans assets",
        OpenGameFolder = "Ouvrir le dossier du jeu",
        ExtractAlso = "Aussi extraire avec extract-xiso (optionnel)",
        LauncherSec = "LAUNCHER",
        Binary = "Exécutable", Assets = "Assets",
        AdvTitle = "AVANCÉ", AdvSub = "Fond, journaux et dossiers.",
        AboutTitle = "À PROPOS",
        AboutBody = "Le code Xbox est recompilé en x64 natif (XboxXBERecomp). Aucune donnée de jeu n’est fournie.",
        BtnPlay = "JOUER", Save = "Enregistrer",
        Ready = "Prêt. Les changements s’enregistrent quand vous lancez.",
        ReadyMissing = "Warriors.exe manquant — compiler/sync le dossier port.",
        StatusLanguage = "Langue",
        LangHint = "Langue du jeu → XC_LANGUAGE={0} ({1}). Packs : {2}. UI launcher dans cette langue.",
        LangApplied = "XC_LANGUAGE={0} ({1}) appliqué. UI launcher mise à jour.",
        IsoBrowseTitle = "Sélectionner l’ISO EUR de The Warriors",
        Version = "EUR · recompilation native",
        MsgSaved = "Enregistré.", MsgLaunched = "Lancé. Le premier démarrage peut ouvrir la fenêtre de compilation.",
    });

    static Bundle Es() => WithEnFallback(new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavHome = "INICIO", NavVideo = "VIDEO", NavControls = "CONTROLES",
        NavGame = "JUEGO", NavAdvanced = "AVANZADO", NavAbout = "ACERCA",
        QuickSettings = "AJUSTES RÁPIDOS",
        HomeSub = "Cómo se muestra el juego: ventana, resolución 3D y frame rate.",
        DisplayMode = "Modo de pantalla", WindowSize = "Tamaño de ventana",
        RenderResolution = "Resolución de render", FpsCounter = "Contador FPS",
        FrameRate = "Frame rate", VSync = "VSync",
        VideoTitle = "VIDEO", VideoSub = "Cómo se muestra el juego: ventana, resolución 3D y frame rate.",
        DisplaySec = "PANTALLA", RenderingSec = "RENDER",
        GraphicsApi = "API gráfica", Upscaler = "Upscaler",
        TextureFiltering = "Filtro de texturas", FrameGeneration = "Frame generation",
        DlssModel = "Modelo DLSS", Sharpness = "Nitidez", CustomTex = "Texturas custom",
        ControlsTitle = "CONTROLES",
        ControlsSub = "Teclado, ratón y mando funcionan juntos.",
        General = "GENERAL", KeyboardMouse = "Teclado y ratón",
        InvertY = "Invertir cámara Y", Vibration = "Vibración",
        ControlsHint = "El remapeo completo llegará después. Flags en settings.toml.",
        GameTitle = "JUEGO", GameSub = "Tu copia, el idioma y el launcher.",
        LanguageSec = "IDIOMA",
        LangNote = "El inglés es el estándar. Otros idiomas según el disco. La UI del launcher sigue este idioma.",
        YourGameSec = "TU JUEGO", DiscImage = "Imagen de disco",
        BrowseIso = "Elegir ISO…", Install = "Instalar en assets",
        OpenGameFolder = "Abrir carpeta del juego",
        ExtractAlso = "También extraer con extract-xiso (opcional)",
        LauncherSec = "LAUNCHER",
        Binary = "Ejecutable", Assets = "Assets",
        AdvTitle = "AVANZADO", AdvSub = "Fondo, logs y carpetas.",
        AboutTitle = "ACERCA",
        AboutBody = "El código Xbox se recompila a x64 nativo (XboxXBERecomp). No se incluyen datos del juego.",
        BtnPlay = "JUGAR", Save = "Guardar",
        Ready = "Listo. Los cambios se guardan al pulsar Jugar.",
        ReadyMissing = "Falta Warriors.exe — compila/sincroniza la carpeta port.",
        StatusLanguage = "Idioma",
        LangHint = "Idioma del juego → XC_LANGUAGE={0} ({1}). Packs: {2}. UI del launcher en este idioma.",
        LangApplied = "Aplicado XC_LANGUAGE={0} ({1}). UI del launcher actualizada.",
        IsoBrowseTitle = "Seleccionar la ISO EUR de The Warriors",
        Version = "EUR · recompilación nativa",
        MsgSaved = "Guardado.", MsgLaunched = "Lanzado. El primer inicio puede abrir la ventana de compilación.",
    });

    /// <summary>Fill option/hint/advanced strings from English when a locale only overrides chrome.</summary>
    static Bundle WithEnFallback(Bundle partial)
    {
        var e = En();
        return new Bundle
        {
            WindowTitle = Nz(partial.WindowTitle, e.WindowTitle),
            NavHome = Nz(partial.NavHome, e.NavHome),
            NavVideo = Nz(partial.NavVideo, e.NavVideo),
            NavControls = Nz(partial.NavControls, e.NavControls),
            NavGame = Nz(partial.NavGame, e.NavGame),
            NavAdvanced = Nz(partial.NavAdvanced, e.NavAdvanced),
            NavAbout = Nz(partial.NavAbout, e.NavAbout),
            QuickSettings = Nz(partial.QuickSettings, e.QuickSettings),
            FirstSetup = Nz(partial.FirstSetup, e.FirstSetup),
            FirstSetupSub = Nz(partial.FirstSetupSub, e.FirstSetupSub),
            ReadyNeedSetup = Nz(partial.ReadyNeedSetup, e.ReadyNeedSetup),
            HomeSub = Nz(partial.HomeSub, e.HomeSub),
            DisplayMode = Nz(partial.DisplayMode, e.DisplayMode),
            WindowSize = Nz(partial.WindowSize, e.WindowSize),
            RenderResolution = Nz(partial.RenderResolution, e.RenderResolution),
            FpsCounter = Nz(partial.FpsCounter, e.FpsCounter),
            FrameRate = Nz(partial.FrameRate, e.FrameRate),
            VSync = Nz(partial.VSync, e.VSync),
            VideoTitle = Nz(partial.VideoTitle, e.VideoTitle),
            VideoSub = Nz(partial.VideoSub, e.VideoSub),
            DisplaySec = Nz(partial.DisplaySec, e.DisplaySec),
            RenderingSec = Nz(partial.RenderingSec, e.RenderingSec),
            GraphicsApi = Nz(partial.GraphicsApi, e.GraphicsApi),
            Upscaler = Nz(partial.Upscaler, e.Upscaler),
            TextureFiltering = Nz(partial.TextureFiltering, e.TextureFiltering),
            FrameGeneration = Nz(partial.FrameGeneration, e.FrameGeneration),
            DlssModel = Nz(partial.DlssModel, e.DlssModel),
            Sharpness = Nz(partial.Sharpness, e.Sharpness),
            CustomTex = Nz(partial.CustomTex, e.CustomTex),
            ControlsTitle = Nz(partial.ControlsTitle, e.ControlsTitle),
            ControlsSub = Nz(partial.ControlsSub, e.ControlsSub),
            General = Nz(partial.General, e.General),
            KeyboardMouse = Nz(partial.KeyboardMouse, e.KeyboardMouse),
            InvertY = Nz(partial.InvertY, e.InvertY),
            Vibration = Nz(partial.Vibration, e.Vibration),
            ControlsHint = Nz(partial.ControlsHint, e.ControlsHint),
            GameTitle = Nz(partial.GameTitle, e.GameTitle),
            GameSub = Nz(partial.GameSub, e.GameSub),
            LanguageSec = Nz(partial.LanguageSec, e.LanguageSec),
            LangNote = Nz(partial.LangNote, e.LangNote),
            YourGameSec = Nz(partial.YourGameSec, e.YourGameSec),
            DiscImage = Nz(partial.DiscImage, e.DiscImage),
            BrowseIso = Nz(partial.BrowseIso, e.BrowseIso),
            Install = Nz(partial.Install, e.Install),
            OpenGameFolder = Nz(partial.OpenGameFolder, e.OpenGameFolder),
            ExtractAlso = Nz(partial.ExtractAlso, e.ExtractAlso),
            LauncherSec = Nz(partial.LauncherSec, e.LauncherSec),
            Binary = Nz(partial.Binary, e.Binary),
            Assets = Nz(partial.Assets, e.Assets),
            AdvTitle = Nz(partial.AdvTitle, e.AdvTitle),
            AdvSub = Nz(partial.AdvSub, e.AdvSub),
            AboutTitle = Nz(partial.AboutTitle, e.AboutTitle),
            AboutBody = Nz(partial.AboutBody, e.AboutBody),
            BtnPlay = Nz(partial.BtnPlay, e.BtnPlay),
            Save = Nz(partial.Save, e.Save),
            Ready = Nz(partial.Ready, e.Ready),
            ReadyMissing = Nz(partial.ReadyMissing, e.ReadyMissing),
            StatusLanguage = Nz(partial.StatusLanguage, e.StatusLanguage),
            LangHint = Nz(partial.LangHint, e.LangHint),
            LangApplied = Nz(partial.LangApplied, e.LangApplied),
            IsoBrowseTitle = Nz(partial.IsoBrowseTitle, e.IsoBrowseTitle),
            Version = Nz(partial.Version, e.Version),
            MsgSaved = Nz(partial.MsgSaved, e.MsgSaved),
            MsgLaunched = Nz(partial.MsgLaunched, e.MsgLaunched),
            On = Nz(partial.On, e.On),
            Off = Nz(partial.Off, e.Off),
            OptBorderless = Nz(partial.OptBorderless, e.OptBorderless),
            OptFullscreen = Nz(partial.OptFullscreen, e.OptFullscreen),
            OptWindowed = Nz(partial.OptWindowed, e.OptWindowed),
            OptDesktop = Nz(partial.OptDesktop, e.OptDesktop),
            OptMonitorRefresh = Nz(partial.OptMonitorRefresh, e.OptMonitorRefresh),
            OptUnlimited = Nz(partial.OptUnlimited, e.OptUnlimited),
            OptMatchWindow = Nz(partial.OptMatchWindow, e.OptMatchWindow),
            OptBalanced = Nz(partial.OptBalanced, e.OptBalanced),
            OptQuality = Nz(partial.OptQuality, e.OptQuality),
            OptUltraQuality = Nz(partial.OptUltraQuality, e.OptUltraQuality),
            OptPerformance = Nz(partial.OptPerformance, e.OptPerformance),
            OptUltraPerf = Nz(partial.OptUltraPerf, e.OptUltraPerf),
            OptNativeAa = Nz(partial.OptNativeAa, e.OptNativeAa),
            OptD3d12 = Nz(partial.OptD3d12, e.OptD3d12),
            OptVulkan = Nz(partial.OptVulkan, e.OptVulkan),
            OptFsr = Nz(partial.OptFsr, e.OptFsr),
            OptDlss = Nz(partial.OptDlss, e.OptDlss),
            OptXess = Nz(partial.OptXess, e.OptXess),
            OptFsrFg = Nz(partial.OptFsrFg, e.OptFsrFg),
            OptDlssFg = Nz(partial.OptDlssFg, e.OptDlssFg),
            OptDefault = Nz(partial.OptDefault, e.OptDefault),
            OptAnisoOff = Nz(partial.OptAnisoOff, e.OptAnisoOff),
            HintBorderless = Nz(partial.HintBorderless, e.HintBorderless),
            HintWindow = Nz(partial.HintWindow, e.HintWindow),
            HintVSync = Nz(partial.HintVSync, e.HintVSync),
            HintApi = Nz(partial.HintApi, e.HintApi),
            HintRender = Nz(partial.HintRender, e.HintRender),
            HintUpscaler = Nz(partial.HintUpscaler, e.HintUpscaler),
            HintAniso = Nz(partial.HintAniso, e.HintAniso),
            HintFps = Nz(partial.HintFps, e.HintFps),
            HintFrameGen = Nz(partial.HintFrameGen, e.HintFrameGen),
            HintDlss = Nz(partial.HintDlss, e.HintDlss),
            PresetSec = Nz(partial.PresetSec, e.PresetSec),
            Preset = Nz(partial.Preset, e.Preset),
            HintPreset = Nz(partial.HintPreset, e.HintPreset),
            BtnUseRecommended = Nz(partial.BtnUseRecommended, e.BtnUseRecommended),
            OptCustom = Nz(partial.OptCustom, e.OptCustom),
            GpuLine = Nz(partial.GpuLine, e.GpuLine),
            GpuLineUnknown = Nz(partial.GpuLineUnknown, e.GpuLineUnknown),
            AdvVulkan = Nz(partial.AdvVulkan, e.AdvVulkan),
            AdvVulkanBody = Nz(partial.AdvVulkanBody, e.AdvVulkanBody),
            AdvFiles = Nz(partial.AdvFiles, e.AdvFiles),
            AdvOpenLogs = Nz(partial.AdvOpenLogs, e.AdvOpenLogs),
            AdvOpenPort = Nz(partial.AdvOpenPort, e.AdvOpenPort),
            AdvUpscaler = Nz(partial.AdvUpscaler, e.AdvUpscaler),
            AdvRefreshDll = Nz(partial.AdvRefreshDll, e.AdvRefreshDll),
            AdvInGame = Nz(partial.AdvInGame, e.AdvInGame),
            AdvInGameBody = Nz(partial.AdvInGameBody, e.AdvInGameBody),
            AboutHow = Nz(partial.AboutHow, e.AboutHow),
            AboutCredits = Nz(partial.AboutCredits, e.AboutCredits),
            AboutCreditsBody = Nz(partial.AboutCreditsBody, e.AboutCreditsBody),
            AboutLegal = Nz(partial.AboutLegal, e.AboutLegal),
            AboutLegalBody = Nz(partial.AboutLegalBody, e.AboutLegalBody),
        };
    }

    static string Nz(string v, string fallback) => string.IsNullOrEmpty(v) ? fallback : v;
}
