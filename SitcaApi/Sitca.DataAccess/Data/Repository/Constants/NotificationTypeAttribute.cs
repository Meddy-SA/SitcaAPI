using System;

namespace Sitca.DataAccess.Data.Repository.Constants;

public class NotificationTypeAttribute : Attribute
{
  public string[] SpanishNames { get; }
  public string[] EnglishNames { get; }

  public NotificationTypeAttribute(string[] spanishNames, string[] englishNames)
  {
    SpanishNames = spanishNames;
    EnglishNames = englishNames;
  }
}
