using System.Text.RegularExpressions;

namespace WarriorsLauncher.Services;

public sealed class DetectedLanguage
{
    public uint Id { get; init; }
    public string Code { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public override string ToString() => DisplayName;
}

public static class LanguageCatalog
{
    // Xbox XC_LANGUAGE — same ids the EUR host reads from language.cfg / XBE_LANGUAGE.
    public static readonly DetectedLanguage[] All =
    [
        new() { Id = 1, Code = "en", DisplayName = "English" },
        new() { Id = 3, Code = "de", DisplayName = "Deutsch" },
        new() { Id = 4, Code = "fr", DisplayName = "Français" },
        new() { Id = 5, Code = "es", DisplayName = "Español" },
        new() { Id = 6, Code = "it", DisplayName = "Italiano" },
    ];

    static readonly Dictionary<string, DetectedLanguage> ByToken =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["EN"] = All[0], ["ENG"] = All[0], ["ENGLISH"] = All[0],
            ["DE"] = All[1], ["GER"] = All[1], ["GERMAN"] = All[1], ["DEUTSCH"] = All[1],
            ["FR"] = All[2], ["FRE"] = All[2], ["FRENCH"] = All[2], ["FRANÇAIS"] = All[2], ["FRANCAIS"] = All[2],
            ["ES"] = All[3], ["SPA"] = All[3], ["SPANISH"] = All[3], ["ESPAÑOL"] = All[3], ["ESPANOL"] = All[3],
            ["IT"] = All[4], ["ITA"] = All[4], ["ITALIAN"] = All[4], ["ITALIANO"] = All[4],
        };

    /// <summary>
    /// Languages advertised on the ISO file name (Redump: "(Europe) (En,Fr,De,Es,It)").
    /// Tokens are taken from parenthetical lists only — never substring-match "En" inside "Europe".
    /// </summary>
    public static IReadOnlyList<DetectedLanguage> DetectFromIsoName(string? isoPath)
    {
        if (string.IsNullOrWhiteSpace(isoPath))
            return All;

        var name = Path.GetFileNameWithoutExtension(isoPath);
        var found = new List<DetectedLanguage>();

        foreach (Match m in Regex.Matches(name, @"\(([^)]+)\)"))
        {
            var inner = m.Groups[1].Value;
            // Skip region-only groups like "Europe" / "USA"
            if (!inner.Contains(',') && !ByToken.ContainsKey(inner.Trim()))
                continue;

            foreach (var raw in inner.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (ByToken.TryGetValue(raw, out var lang) && found.All(f => f.Id != lang.Id))
                    found.Add(lang);
            }
        }

        if (found.Count == 0 &&
            (name.Contains("Europe", StringComparison.OrdinalIgnoreCase) ||
             name.Contains("PAL", StringComparison.OrdinalIgnoreCase) ||
             name.Contains("EUR", StringComparison.OrdinalIgnoreCase)))
            return All;

        return found.Count > 0 ? found : All.Take(1).ToList();
    }

    public static DetectedLanguage? ById(uint id) => All.FirstOrDefault(l => l.Id == id);

    public static uint PreferForOs(IReadOnlyList<DetectedLanguage> available)
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var match = available.FirstOrDefault(l =>
            string.Equals(l.Code, culture, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match.Id;
        var en = available.FirstOrDefault(l => l.Id == 1);
        return en?.Id ?? available[0].Id;
    }
}
