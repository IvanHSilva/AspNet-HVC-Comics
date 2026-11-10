using System.Globalization;
using System.Text;

namespace HVC_Comics.Views.Comic;

public static class ComicSlug
{
    public static string Create(string name, int number)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                builder.Append(char.ToLowerInvariant(c));
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        var nameSlug = builder.ToString().Trim('-');

        return $"{nameSlug}-{number:D2}";
    }

    public static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                builder.Append(char.ToLowerInvariant(c));
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }

    public static string CreateGallery(string name, int volume)
    {
        var slug = Normalize(ComicVolume.GetBaseName(name, volume));

        if (volume == 0) return $"{slug}-sem-volume";
        if (volume == 1) return slug;

        return $"{slug}-{ComicVolume.ToRoman(volume).ToLowerInvariant()}";
    }

    public static bool TryParseGallery(string slug, out string collectionSlug,
        out int volume)
    {
        collectionSlug = Normalize(slug);
        volume = 1;

        var parts = collectionSlug.Split('-');

        // No Volume (0)
        if (parts.Length >= 2 &&
            parts[^1] == "volume" &&
            parts[^2] == "sem")
        {
            volume = 0;
            collectionSlug = string.Join('-', parts[..^2]);
            return true;
        }

        // No sufix (1)
        if (parts.Length < 2) return false;

        var roman = parts[^1];
        var parsedVolume = roman switch
        {
            "ii" => 2,
            "iii" => 3,
            "iv" => 4,
            "v" => 5,
            "vi" => 6,
            "vii" => 7,
            "viii" => 8,
            "ix" => 9,
            "x" => 10,
            "xi" => 11,
            "xii" => 12,
            "xiii" => 13,
            "xiv" => 14,
            "xv" => 15,
            _ => 0
        };

        if (parsedVolume == 0) return false;

        volume = parsedVolume;
        collectionSlug = string.Join('-', parts[..^1]);

        return true;
    }

    private static string ToRoman(int volume)
    {
        return volume switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            11 => "XI",
            12 => "XII",
            13 => "XIII",
            14 => "XIV",
            15 => "XV",
            _ => volume.ToString()
        };
    }
}
