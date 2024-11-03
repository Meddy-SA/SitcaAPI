
namespace Sitca.Models.ViewModels
{
  class EmailModelsVm
  {
  }

  public class LoginMailVm
  {
    public string Url { get; set; } = null!;
    public RegisterVm UserData { get; set; } = null!;
  }
}
