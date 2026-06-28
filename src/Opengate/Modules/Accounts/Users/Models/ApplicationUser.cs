using Microsoft.AspNetCore.Identity;

namespace Opengate.Modules.Accounts.Users.Models;

public class ApplicationUser : IdentityUser
{
    public string? OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}