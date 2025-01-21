using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sitca.Extensions;

public static class ModelStateExtensions
{
    public static string GetErrorMessagesLines(this ModelStateDictionary modelState)
    {
        return string.Join(
            "; ",
            modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)
        );
    }
}
