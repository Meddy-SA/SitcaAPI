using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sitca.DataAccess.Extensions;

public static class ModelStateExtensions
{
  public static string GetErrorMessages(this ModelStateDictionary modelState)
  {
    return string.Join("; ", modelState.Values
        .SelectMany(x => x.Errors)
        .Select(x => x.ErrorMessage));
  }
}
