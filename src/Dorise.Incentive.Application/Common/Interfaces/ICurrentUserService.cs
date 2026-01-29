namespace Dorise.Incentive.Application.Common.Interfaces;

/// <summary>
/// Interface for accessing current user information.
/// "Super Nintendo Chalmers!" - And this tells us who the current superintendent is!
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's display name.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Gets the current user's Azure AD object ID.
    /// </summary>
    string? AzureAdObjectId { get; }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current user's roles.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    bool IsInRole(string role);
}
