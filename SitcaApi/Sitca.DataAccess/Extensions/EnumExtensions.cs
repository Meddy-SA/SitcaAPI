using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;

namespace Sitca.DataAccess.Extensions;

public static class EnumExtensions
{
  public static string GetDisplayName(this Enum value)
  {
    var field = value.GetType().GetField(value.ToString());
    var attribute = field?.GetCustomAttribute<DisplayAttribute>();

    return attribute?.Name ?? value.ToString();
  }

  public static string GetNameForLanguage(this Enum value, string language = "es")
  {
    var field = value.GetType().GetField(value.ToString());
    var attribute = field?.GetCustomAttribute<MultiLanguageDisplayAttribute>();

    if (attribute == null)
      return value.ToString();

    return language.ToLower() switch
    {
      "en" => attribute.English,
      "es" => attribute.Spanish,
      _ => attribute.Spanish // Español como idioma por defecto
    };
  }

  public static List<EnumValueDto> ToEnumValueList<TEnum>() where TEnum : Enum
  {
    return Enum.GetValues(typeof(TEnum))
        .Cast<TEnum>()
        .Select(e => new EnumValueDto
        {
          Id = Convert.ToInt32(e),
          Name = e.GetDisplayName()
        })
        .ToList();
  }

  public static TEnum? GetEnumFromValue<TEnum>(int value) where TEnum : struct, Enum
  {
    if (Enum.IsDefined(typeof(TEnum), value))
    {
      return (TEnum)Enum.ToObject(typeof(TEnum), value);
    }
    return null;
  }

  public static string GetDisplayNameFromValue<TEnum>(int value) where TEnum : struct, Enum
  {
    var enumValue = GetEnumFromValue<TEnum>(value);
    return enumValue?.GetDisplayName();
  }
}
