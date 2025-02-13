using Microsoft.AspNetCore.Identity;

namespace Sitca.Models;

public class ApplicationRole : IdentityRole
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    public ApplicationRole()
        : base()
    {
        UserRoles = new HashSet<ApplicationUserRole>();
    }

    public ApplicationRole(string roleName)
        : base(roleName)
    {
        UserRoles = new HashSet<ApplicationUserRole>();
    }
}
