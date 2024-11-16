using Utilities.Common;

namespace Sitca.DataAccess.Data.Repository.Constants;

public static class StatusConstants
{
  public const int Finalizado = 8;

  public static string GetLocalizedStatus(string language = "es")
  {
    return LocalizationUtilities.GetStatus(Finalizado, language);
  }
}
