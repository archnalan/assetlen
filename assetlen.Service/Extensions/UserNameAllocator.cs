using assetlen.Shared.Models.Models;
using Microsoft.AspNetCore.Identity;

namespace assetlen.Service.Extensions;

/// <summary>
/// Resolves the username a new account should actually get.
/// </summary>
/// <remarks>
/// The desired name is used verbatim. A numeric suffix is appended only when
/// that name is already taken — the first "userone" is <c>userone</c>, not
/// <c>af4buserone</c>.
///
/// This replaces an unconditional 4-character GUID prefix that ran on every
/// insert. It made the seeded admin's credentials unguessable (you had to read
/// them out of the database to log in) and it disguised genuine collisions,
/// since two accounts could never visibly conflict.
/// </remarks>
public static class UserNameAllocator
{
    private const int MaxAttempts = 100;

    /// <summary>
    /// Returns <paramref name="desired"/> if free, else <c>desired2</c>,
    /// <c>desired3</c>, … Falls back to a short GUID suffix if a hundred
    /// candidates are somehow all taken.
    /// </summary>
    public static async Task<string> ResolveUserNameAsync(
        UserManager<AppUser> userManager, string desired)
    {
        if (string.IsNullOrWhiteSpace(desired))
            throw new ArgumentException("A desired username is required.", nameof(desired));

        if (await userManager.FindByNameAsync(desired) is null)
            return desired;

        for (var n = 2; n <= MaxAttempts; n++)
        {
            var candidate = $"{desired}{n}";
            if (await userManager.FindByNameAsync(candidate) is null)
                return candidate;
        }

        return $"{desired}-{Guid.NewGuid().ToString("N")[..6]}";
    }

    /// <summary>
    /// Same rule for an email address: keep the local part, suffix before the
    /// <c>@</c> only on conflict.
    /// </summary>
    public static async Task<string> ResolveEmailAsync(
        UserManager<AppUser> userManager, string desired)
    {
        if (string.IsNullOrWhiteSpace(desired))
            throw new ArgumentException("A desired email is required.", nameof(desired));

        if (await userManager.FindByEmailAsync(desired) is null)
            return desired;

        var at = desired.IndexOf('@');
        var local = at > 0 ? desired[..at] : desired;
        var domain = at > 0 ? desired[at..] : string.Empty;

        for (var n = 2; n <= MaxAttempts; n++)
        {
            var candidate = $"{local}{n}{domain}";
            if (await userManager.FindByEmailAsync(candidate) is null)
                return candidate;
        }

        return $"{local}-{Guid.NewGuid().ToString("N")[..6]}{domain}";
    }
}
