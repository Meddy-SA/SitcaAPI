using System.Globalization;

namespace Utilities.Common;

public static class NumberUtilities
{
  private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

  public static decimal? ToDecimal(this string value)
  {
    if (string.IsNullOrWhiteSpace(value)) return null;

    value = value.Replace(',', '.');
    if (value.Contains("E-")) return 0;

    return decimal.TryParse(value,
        NumberStyles.Any,
        InvariantCulture,
        out var result) ? result : null;
  }

  public static string? ToFormattedString(this decimal? value, string format = "G29")
  {
    return value?.ToString(format, InvariantCulture);
  }

  public static string ToFormattedString(this decimal value, string format = "G29")
  {
    return value.ToString(format, InvariantCulture);
  }
}
