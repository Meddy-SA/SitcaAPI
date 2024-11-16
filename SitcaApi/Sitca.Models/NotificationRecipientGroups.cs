using Sitca.Models.ViewModels;

namespace Sitca.Models;

public class NotificationRecipientGroups
{
  public List<UsersListVm> InternalRecipients { get; } = new();
  public List<UsersListVm> CompanyRecipients { get; } = new();
}
