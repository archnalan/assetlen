namespace assetlen.Shared.Models.Models.Authorization;

// Higher values imply all lower capabilities — compare with >=.
public enum ModuleAccess
{
    None = 0,
    Read = 1,
    Write = 2,
    Admin = 3
}
