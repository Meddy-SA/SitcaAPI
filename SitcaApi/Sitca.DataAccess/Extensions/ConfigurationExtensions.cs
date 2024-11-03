using Microsoft.Extensions.Configuration;

namespace Sitca.DataAccess.Extensions;

public static class ConfigurationExtensions
{
  public static bool GetBool(this IConfiguration config, string key, bool defaultValue = false)
  {
    return bool.TryParse(config[key], out bool result) ? result : defaultValue;
  }
}
