using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities.Common;

public static class StringUtilities
{
  public static string CamelCase(this string phrase)
  {
    if (string.IsNullOrEmpty(phrase)) return string.Empty;

    var terms = phrase.Split(' ');
    return string.Join(" ", terms
        .Where(term => term.Length > 2)
        .Select(term => char.ToUpper(term[0]) + term[1..].ToLower()))
        .Trim();
  }

  public static string ToCode(this string value)
  {
    if (string.IsNullOrEmpty(value)) return string.Empty;

    value = value.RemoveAccent().ToUpper();
    return value.StartsWith("#") ? value : "#" + value;
  }

  public static string? ToSlug(this string phrase)
  {
    if (string.IsNullOrWhiteSpace(phrase)) return null;

    var str = phrase.RemoveAccent().ToLower();
    str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
    str = Regex.Replace(str, @"\s+", " ").Trim();
    str = str[..Math.Min(str.Length, 45)].Trim();
    return Regex.Replace(str, @"\s", "-");
  }

  public static string RemoveAccent(this string text)
  {
    if (string.IsNullOrEmpty(text)) return string.Empty;

    var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();

    foreach (var c in normalizedString)
    {
      if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        stringBuilder.Append(c);
    }

    return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
  }
}
