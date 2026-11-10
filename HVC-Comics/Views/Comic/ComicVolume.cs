namespace HVC_Comics.Views.Comic;

public static class ComicVolume
{
    private static readonly Dictionary<int, string> RomanNumerals = new()
    {
        [1] = "I",
        [2] = "II",
        [3] = "III",
        [4] = "IV",
        [5] = "V",
        [6] = "VI",
        [7] = "VII",
        [8] = "VIII",
        [9] = "IX",
        [10] = "X",
        [11] = "XI",
        [12] = "XII",
        [13] = "XIII",
        [14] = "XIV",
        [15] = "XV"
    };

    public static string ToRoman(int volume)
    {
        return RomanNumerals.GetValueOrDefault(volume, volume.ToString());
    }

    public static string GetBaseName(string name, int volume)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        name = name.Trim();

        if (volume <= 1) return name;

        var roman = ToRoman(volume);
        var suffix = $" {roman}";

        if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return name[..^suffix.Length].TrimEnd();

        return name;
    }

    public static string Normalize(string value)
    {
        return ComicSlug.Normalize(value);
    }
}