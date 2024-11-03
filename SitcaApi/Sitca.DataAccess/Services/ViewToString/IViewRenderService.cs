using System.Threading.Tasks;

namespace Sitca.DataAccess.Services.ViewToString
{
  public interface IViewRenderService
  {
    Task<string> RenderToStringAsync(string viewName, object model);
  }
}
