using System;
using Sitca.Models.Constants;
using Sitca.Models.Enums;

namespace Sitca.DataAccess.Extensions;

public static class StatusExtensions
{
  public static string ToLocalizedString(this CertificationStatus status, string language)
      => StatusLocalizations.GetDescription(status, language);

  public static string ToLocalizedString(this int statusId, string language)
  {
    return Enum.IsDefined(typeof(CertificationStatus), statusId)
        ? StatusLocalizations.GetDescription((CertificationStatus)statusId, language)
        : "Unknown Status";
  }
}
