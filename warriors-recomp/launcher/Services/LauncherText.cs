namespace WarriorsLauncher.Services;

/// <summary>Launcher chrome strings follow the selected game language (EUR packs).</summary>
public static class LauncherText
{
    public sealed class Bundle
    {
        public string WindowTitle { get; init; } = "";
        public string NavPlay { get; init; } = "";
        public string NavGraphics { get; init; } = "";
        public string NavSetup { get; init; } = "";
        public string NavLanguage { get; init; } = "";
        public string PlayTitle { get; init; } = "";
        public string PlaySub { get; init; } = "";
        public string GameBinary { get; init; } = "";
        public string Assets { get; init; } = "";
        public string PlayNote { get; init; } = "";
        public string BtnPlay { get; init; } = "";
        public string OpenPort { get; init; } = "";
        public string Save { get; init; } = "";
        public string SetupTitle { get; init; } = "";
        public string SetupSub { get; init; } = "";
        public string IsoPath { get; init; } = "";
        public string Browse { get; init; } = "";
        public string ExtractAlso { get; init; } = "";
        public string Install { get; init; } = "";
        public string OpenAssets { get; init; } = "";
        public string GfxTitle { get; init; } = "";
        public string GfxSub { get; init; } = "";
        public string GfxApi { get; init; } = "";
        public string DisplayMode { get; init; } = "";
        public string Resolution { get; init; } = "";
        public string FpsCap { get; init; } = "";
        public string RenderScale { get; init; } = "";
        public string Aniso { get; init; } = "";
        public string VSync { get; init; } = "";
        public string CustomTex { get; init; } = "";
        public string ApplyGfx { get; init; } = "";
        public string ResetGfx { get; init; } = "";
        public string LangTitle { get; init; } = "";
        public string LangSub { get; init; } = "";
        public string GameLanguage { get; init; } = "";
        public string RescanLang { get; init; } = "";
        public string StatusLanguage { get; init; } = "";
        public string LangHint { get; init; } = "";
        public string LangApplied { get; init; } = "";
        public string IsoBrowseTitle { get; init; } = "";
    }

    static readonly Dictionary<string, Bundle> ByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = En(),
        ["de"] = De(),
        ["fr"] = Fr(),
        ["es"] = Es(),
        ["it"] = It(),
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
        NavPlay = "PLAY",
        NavGraphics = "GRAPHICS",
        NavSetup = "SETUP / ISO",
        NavLanguage = "LANGUAGE",
        PlayTitle = "Play",
        PlaySub = "Launch the port with your EUR disc. First start builds Warriors.dll from your copy (a few minutes). F10 opens in-game settings.",
        GameBinary = "GAME BINARY",
        Assets = "ASSETS",
        PlayNote = "Use the EUR build under warriors-recomp\\out\\build\\release (Warriors.exe + Warriors.dll). Language and graphics are applied from this launcher before launch.",
        BtnPlay = "PLAY",
        OpenPort = "Open port folder",
        Save = "Save",
        SetupTitle = "Install from ISO",
        SetupSub = "Point at your original Xbox EUR dump of The Warriors. Disc-image mode drops the .iso into assets (recommended). Extract mode unpacks default.xbe and data folders.",
        IsoPath = "ISO PATH",
        Browse = "Browse…",
        ExtractAlso = "Also extract files with extract-xiso (optional — ISO alone is enough)",
        Install = "INSTALL INTO ASSETS",
        OpenAssets = "Open assets",
        GfxTitle = "Graphics",
        GfxSub = "Writes settings.toml beside Warriors.exe (same keys as F10). Apply before PLAY. Vulkan is reserved — this recomp host renders with Direct3D 12 only.",
        GfxApi = "GRAPHICS API",
        DisplayMode = "DISPLAY MODE",
        Resolution = "RESOLUTION",
        FpsCap = "FPS CAP",
        RenderScale = "RENDER SCALE",
        Aniso = "ANISOTROPIC FILTERING",
        VSync = "VSync",
        CustomTex = "Custom textures (textures\\replacement)",
        ApplyGfx = "APPLY TO SETTINGS.TOML",
        ResetGfx = "Reset defaults",
        LangTitle = "Language",
        LangSub = "Languages listed from your ISO name (EUR packs: En, Fr, De, Es, It). Written to language.cfg and settings.toml for the EUR host. The launcher UI follows this choice.",
        GameLanguage = "GAME LANGUAGE",
        RescanLang = "Rescan from ISO name",
        StatusLanguage = "Language",
        LangHint = "Game language → XC_LANGUAGE={0} ({1}). Packs on this ISO: {2}. Written to language.cfg + settings.toml; host reads XBE_LANGUAGE / XC_LANGUAGE. Launcher UI uses this language.",
        LangApplied = "Applied XC_LANGUAGE={0} ({1}) → language.cfg. Launcher UI updated. Restart PLAY for the title to pick UI/audio.",
        IsoBrowseTitle = "Select The Warriors EUR ISO",
    };

    static Bundle It() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavPlay = "GIOCA",
        NavGraphics = "GRAFICA",
        NavSetup = "SETUP / ISO",
        NavLanguage = "LINGUA",
        PlayTitle = "Gioca",
        PlaySub = "Avvia il port con il tuo disco EUR. Al primo avvio costruisce Warriors.dll dalla tua copia (alcuni minuti). F10 apre le impostazioni in gioco.",
        GameBinary = "ESEGUIBILE",
        Assets = "ASSETS",
        PlayNote = "Usa la build EUR in warriors-recomp\\out\\build\\release (Warriors.exe + Warriors.dll). Lingua e grafica vengono applicate da questo launcher prima dell’avvio.",
        BtnPlay = "GIOCA",
        OpenPort = "Apri cartella port",
        Save = "Salva",
        SetupTitle = "Installa da ISO",
        SetupSub = "Seleziona il dump Xbox EUR originale di The Warriors. La modalità immagine copia l’.iso in assets (consigliata). L’estrazione scompatta default.xbe e le cartelle dati.",
        IsoPath = "PERCORSO ISO",
        Browse = "Sfoglia…",
        ExtractAlso = "Estrai anche i file con extract-xiso (opzionale — basta l’ISO)",
        Install = "INSTALLA IN ASSETS",
        OpenAssets = "Apri assets",
        GfxTitle = "Grafica",
        GfxSub = "Scrive settings.toml accanto a Warriors.exe (stesse chiavi di F10). Applica prima di GIOCA. Vulkan è riservato — questo host usa solo Direct3D 12.",
        GfxApi = "API GRAFICA",
        DisplayMode = "MODALITÀ DISPLAY",
        Resolution = "RISOLUZIONE",
        FpsCap = "LIMITE FPS",
        RenderScale = "SCALA RENDER",
        Aniso = "FILTRO ANISOTROPICO",
        VSync = "VSync",
        CustomTex = "Texture custom (textures\\replacement)",
        ApplyGfx = "APPLICA A SETTINGS.TOML",
        ResetGfx = "Ripristina default",
        LangTitle = "Lingua",
        LangSub = "Lingue dal nome ISO (pack EUR: En, Fr, De, Es, It). Scritte in language.cfg e settings.toml per l’host EUR. L’interfaccia del launcher segue questa scelta.",
        GameLanguage = "LINGUA DI GIOCO",
        RescanLang = "Rileggi dal nome ISO",
        StatusLanguage = "Lingua",
        LangHint = "Lingua di gioco → XC_LANGUAGE={0} ({1}). Pack su questa ISO: {2}. Scritto in language.cfg + settings.toml; l’host legge XBE_LANGUAGE / XC_LANGUAGE. UI launcher in questa lingua.",
        LangApplied = "Applicato XC_LANGUAGE={0} ({1}) → language.cfg. UI launcher aggiornata. Riavvia GIOCA perché il titolo carichi UI/audio.",
        IsoBrowseTitle = "Seleziona l’ISO EUR di The Warriors",
    };

    static Bundle De() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavPlay = "SPIELEN",
        NavGraphics = "GRAFIK",
        NavSetup = "SETUP / ISO",
        NavLanguage = "SPRACHE",
        PlayTitle = "Spielen",
        PlaySub = "Starte den Port mit deiner EUR-Disc. Beim ersten Start wird Warriors.dll aus deiner Kopie gebaut (einige Minuten). F10 öffnet die Ingame-Einstellungen.",
        GameBinary = "SPIELPROGRAMM",
        Assets = "ASSETS",
        PlayNote = "Nutze den EUR-Build unter warriors-recomp\\out\\build\\release (Warriors.exe + Warriors.dll). Sprache und Grafik setzt dieser Launcher vor dem Start.",
        BtnPlay = "SPIELEN",
        OpenPort = "Port-Ordner öffnen",
        Save = "Speichern",
        SetupTitle = "Von ISO installieren",
        SetupSub = "Wähle dein originales Xbox-EUR-Dump von The Warriors. Bildmodus legt die .iso in assets (empfohlen). Extrahieren entpackt default.xbe und Datenordner.",
        IsoPath = "ISO-PFAD",
        Browse = "Durchsuchen…",
        ExtractAlso = "Dateien zusätzlich mit extract-xiso entpacken (optional — ISO reicht)",
        Install = "IN ASSETS INSTALLIEREN",
        OpenAssets = "Assets öffnen",
        GfxTitle = "Grafik",
        GfxSub = "Schreibt settings.toml neben Warriors.exe (gleiche Keys wie F10). Vor SPIELEN anwenden. Vulkan ist reserviert — dieser Host nutzt nur Direct3D 12.",
        GfxApi = "GRAFIK-API",
        DisplayMode = "ANZEIGEMODUS",
        Resolution = "AUFLÖSUNG",
        FpsCap = "FPS-LIMIT",
        RenderScale = "RENDER-SKALA",
        Aniso = "ANISOTROPE FILTERUNG",
        VSync = "VSync",
        CustomTex = "Eigene Texturen (textures\\replacement)",
        ApplyGfx = "AUF SETTINGS.TOML ANWENDEN",
        ResetGfx = "Standard zurücksetzen",
        LangTitle = "Sprache",
        LangSub = "Sprachen aus dem ISO-Namen (EUR-Packs: En, Fr, De, Es, It). Werden in language.cfg und settings.toml geschrieben. Die Launcher-UI folgt dieser Wahl.",
        GameLanguage = "SPIELSPRACHE",
        RescanLang = "Aus ISO-Namen neu lesen",
        StatusLanguage = "Sprache",
        LangHint = "Spielsprache → XC_LANGUAGE={0} ({1}). Packs auf dieser ISO: {2}. Geschrieben nach language.cfg + settings.toml; Host liest XBE_LANGUAGE / XC_LANGUAGE. Launcher-UI in dieser Sprache.",
        LangApplied = "XC_LANGUAGE={0} ({1}) → language.cfg. Launcher-UI aktualisiert. SPIELEN neu starten, damit Titel UI/Audio lädt.",
        IsoBrowseTitle = "EUR-ISO von The Warriors wählen",
    };

    static Bundle Fr() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavPlay = "JOUER",
        NavGraphics = "GRAPHIQUES",
        NavSetup = "SETUP / ISO",
        NavLanguage = "LANGUE",
        PlayTitle = "Jouer",
        PlaySub = "Lance le port avec ton disque EUR. Au premier démarrage, Warriors.dll est construit depuis ta copie (quelques minutes). F10 ouvre les réglages en jeu.",
        GameBinary = "EXÉCUTABLE",
        Assets = "ASSETS",
        PlayNote = "Utilise le build EUR sous warriors-recomp\\out\\build\\release (Warriors.exe + Warriors.dll). Langue et graphismes sont appliqués par ce launcher avant le lancement.",
        BtnPlay = "JOUER",
        OpenPort = "Ouvrir le dossier port",
        Save = "Enregistrer",
        SetupTitle = "Installer depuis l’ISO",
        SetupSub = "Indique ton dump Xbox EUR original de The Warriors. Le mode image place le .iso dans assets (recommandé). L’extraction décompresse default.xbe et les dossiers de données.",
        IsoPath = "CHEMIN ISO",
        Browse = "Parcourir…",
        ExtractAlso = "Extraire aussi avec extract-xiso (optionnel — l’ISO suffit)",
        Install = "INSTALLER DANS ASSETS",
        OpenAssets = "Ouvrir assets",
        GfxTitle = "Graphiques",
        GfxSub = "Écrit settings.toml à côté de Warriors.exe (mêmes clés que F10). Appliquer avant JOUER. Vulkan est réservé — cet hôte utilise uniquement Direct3D 12.",
        GfxApi = "API GRAPHIQUE",
        DisplayMode = "MODE D’AFFICHAGE",
        Resolution = "RÉSOLUTION",
        FpsCap = "LIMITE FPS",
        RenderScale = "ÉCHELLE DE RENDU",
        Aniso = "FILTRAGE ANISOTROPIQUE",
        VSync = "VSync",
        CustomTex = "Textures custom (textures\\replacement)",
        ApplyGfx = "APPLIQUER À SETTINGS.TOML",
        ResetGfx = "Réinitialiser",
        LangTitle = "Langue",
        LangSub = "Langues lues depuis le nom d’ISO (packs EUR : En, Fr, De, Es, It). Écrites dans language.cfg et settings.toml. L’UI du launcher suit ce choix.",
        GameLanguage = "LANGUE DU JEU",
        RescanLang = "Relire depuis le nom d’ISO",
        StatusLanguage = "Langue",
        LangHint = "Langue du jeu → XC_LANGUAGE={0} ({1}). Packs sur cette ISO : {2}. Écrit dans language.cfg + settings.toml ; l’hôte lit XBE_LANGUAGE / XC_LANGUAGE. UI launcher dans cette langue.",
        LangApplied = "XC_LANGUAGE={0} ({1}) → language.cfg. UI launcher mise à jour. Relance JOUER pour que le titre charge UI/audio.",
        IsoBrowseTitle = "Sélectionner l’ISO EUR de The Warriors",
    };

    static Bundle Es() => new()
    {
        WindowTitle = "The Warriors — Recomp",
        NavPlay = "JUGAR",
        NavGraphics = "GRÁFICOS",
        NavSetup = "SETUP / ISO",
        NavLanguage = "IDIOMA",
        PlayTitle = "Jugar",
        PlaySub = "Lanza el port con tu disco EUR. En el primer arranque construye Warriors.dll desde tu copia (unos minutos). F10 abre los ajustes en juego.",
        GameBinary = "EJECUTABLE",
        Assets = "ASSETS",
        PlayNote = "Usa el build EUR en warriors-recomp\\out\\build\\release (Warriors.exe + Warriors.dll). Idioma y gráficos se aplican desde este launcher antes de lanzar.",
        BtnPlay = "JUGAR",
        OpenPort = "Abrir carpeta port",
        Save = "Guardar",
        SetupTitle = "Instalar desde ISO",
        SetupSub = "Elige tu dump Xbox EUR original de The Warriors. El modo imagen deja el .iso en assets (recomendado). Extraer descomprime default.xbe y carpetas de datos.",
        IsoPath = "RUTA ISO",
        Browse = "Examinar…",
        ExtractAlso = "Extraer también con extract-xiso (opcional — basta con la ISO)",
        Install = "INSTALAR EN ASSETS",
        OpenAssets = "Abrir assets",
        GfxTitle = "Gráficos",
        GfxSub = "Escribe settings.toml junto a Warriors.exe (mismas claves que F10). Aplica antes de JUGAR. Vulkan está reservado — este host solo usa Direct3D 12.",
        GfxApi = "API GRÁFICA",
        DisplayMode = "MODO DE PANTALLA",
        Resolution = "RESOLUCIÓN",
        FpsCap = "LÍMITE FPS",
        RenderScale = "ESCALA DE RENDER",
        Aniso = "FILTRADO ANISOTRÓPICO",
        VSync = "VSync",
        CustomTex = "Texturas custom (textures\\replacement)",
        ApplyGfx = "APLICAR A SETTINGS.TOML",
        ResetGfx = "Restablecer",
        LangTitle = "Idioma",
        LangSub = "Idiomas del nombre de la ISO (packs EUR: En, Fr, De, Es, It). Se escriben en language.cfg y settings.toml. La UI del launcher sigue esta elección.",
        GameLanguage = "IDIOMA DEL JUEGO",
        RescanLang = "Releer desde el nombre ISO",
        StatusLanguage = "Idioma",
        LangHint = "Idioma del juego → XC_LANGUAGE={0} ({1}). Packs en esta ISO: {2}. Escrito en language.cfg + settings.toml; el host lee XBE_LANGUAGE / XC_LANGUAGE. UI del launcher en este idioma.",
        LangApplied = "Aplicado XC_LANGUAGE={0} ({1}) → language.cfg. UI del launcher actualizada. Reinicia JUGAR para que el título cargue UI/audio.",
        IsoBrowseTitle = "Seleccionar la ISO EUR de The Warriors",
    };
}
