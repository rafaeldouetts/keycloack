using Microsoft.AspNetCore.Authorization;

namespace Keycloak.Auth.Api.Extensions;

internal sealed class RealmRoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public RealmRoleRequirement(string role)
    {
        Role = role;
    }
}
