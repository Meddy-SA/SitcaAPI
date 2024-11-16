using Utilities.Common;

namespace Sitca.DataAccess.Data.Repository.Constants;

public static class StatusConstants
{
  public const int Finalizado = 8;

  public static string GetLocalizedStatus(string language = "es")
  {
    return LocalizationUtilities.GetStatus(Finalizado, language);
  }

  public static string GetLocalizedStatus(int status, string language = "es")
  {
    return LocalizationUtilities.GetStatus(status, language);
  }

  public static int GetStatusId(string search, string language = "es")
  {
    int? idStatus = LocalizationUtilities.GetStatusId(search, language);
    if (!idStatus.HasValue)
    {
      idStatus = int.Parse(search[0].ToString());
    }

    return idStatus.Value;
  }
}
