using System.Collections.Generic;
using Sitca.Models.DTOs;
using Sitca.Models.Enums;

namespace Sitca.DataAccess.Extensions;

public static class FileCompanyExtensions
{
  public static List<EnumValueDto> GetFileTypes()
  {
    return EnumExtensions.ToEnumValueList<FileCompany>();
  }

  public static string GetFileTypeName(int value)
  {
    return EnumExtensions.GetDisplayNameFromValue<FileCompany>(value);
  }

  public static FileCompany GetFileType(int value)
  {
    return EnumExtensions.GetEnumFromValue<FileCompany>(value) ?? FileCompany.Informativo;
  }
}
