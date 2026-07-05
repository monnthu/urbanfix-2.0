using System.Text.RegularExpressions;

namespace UrbanFix.API.Helpers;

public static partial class SalvadoranIdValidator
{
    [GeneratedRegex(@"^\d{8}-\d$")]
    private static partial Regex DuiPattern();

    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        if (DuiPattern().IsMatch(trimmed))
        {
            normalized = trimmed;
            return true;
        }

        var digitsOnly = trimmed.Replace("-", string.Empty, StringComparison.Ordinal);
        if (digitsOnly.Length != 9 || !digitsOnly.All(char.IsDigit))
        {
            return false;
        }

        normalized = $"{digitsOnly[..8]}-{digitsOnly[8]}";
        return true;
    }
}
