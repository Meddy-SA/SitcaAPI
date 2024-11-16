using System;
using System.Collections.Generic;
using System.Reflection;
using Sitca.DataAccess.Data.Repository.Constants;

namespace Sitca.DataAccess.Helpers;

public static class NotificationHelper
{
  private static readonly Dictionary<string, NotificationTypes> _notificationTypeMap;

  static NotificationHelper()
  {
    _notificationTypeMap = new Dictionary<string, NotificationTypes>(StringComparer.OrdinalIgnoreCase);

    foreach (var notificationType in Enum.GetValues<NotificationTypes>())
    {
      var attribute = notificationType.GetType()
          .GetField(notificationType.ToString())
          ?.GetCustomAttribute<NotificationTypeAttribute>();

      if (attribute != null)
      {
        foreach (var spanishName in attribute.SpanishNames)
        {
          _notificationTypeMap[spanishName] = notificationType;
        }
        foreach (var englishName in attribute.EnglishNames)
        {
          _notificationTypeMap[englishName] = notificationType;
        }
      }
    }
  }

  public static NotificationTypes? GetNotificationTypeByName(string name)
  {
    return _notificationTypeMap.TryGetValue(name, out var notificationType)
        ? notificationType
        : null;
  }
}
