using Dapper;

namespace Sitca.DataAccess.Extensions;

public static class DynamicParametersExtensions
{
  public static DynamicParameters AddParameter(
      this DynamicParameters parameters,
      string name,
      object value)
  {
    parameters.Add(name, value);
    return parameters;
  }
}

