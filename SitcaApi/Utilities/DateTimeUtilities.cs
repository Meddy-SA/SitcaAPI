using System.Globalization;

namespace Utilities.Common;

public static class DateTimeUtilities
{
  private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

  public static DateTime? ToDateFromFormat(this string date, string format)
  {
    if (string.IsNullOrEmpty(date)) return null;

    return DateTime.TryParseExact(date, format,
        InvariantCulture,
        DateTimeStyles.None,
        out var result) ? result : null;
  }

  public static string? ToFormattedString(this DateTime? date, string format)
  {
    return date?.ToString(format, InvariantCulture);
  }

  public static string ToUtcString(this DateTime date)
  {
    return date.ToString("s") + "Z";
  }
}
