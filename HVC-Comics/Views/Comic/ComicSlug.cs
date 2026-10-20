
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
            if (CharUnicodeInfo.GetUnicodeCategory(c) ==
                UnicodeCategory.NonSpacingMark)
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
            if (CharUnicodeInfo.GetUnicodeCategory(c) ==
                UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                builder.Append(char.ToLowerInvariant(c));
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }
}
