﻿using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace Keycloak.Auth.Api.Extensions;
internal sealed class RealmRoleHandler : AuthorizationHandler<RealmRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RealmRoleRequirement requirement)
    {
        Claim realmAccessClaim = context.User.FindFirst("realm_access");

        if (realmAccessClaim != null)
        {
            var parsed = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
            if (parsed.RootElement.TryGetProperty("roles", out JsonElement roles))
            {
                foreach (JsonElement role in roles.EnumerateArray())
                {
                    string roleString = role.GetString();

                    if (roleString != null && roleString.Equals(requirement.Role, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Succeed(requirement);
                    }

                }
            }
        }

        return Task.CompletedTask;
    }
    internal static string GetDescription(Enum value)
    {
        MemberInfo field = value.GetType().GetField(value.ToString());

        if(field == null)
        {
            return null;
        }

        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute == null ? value.ToString() : attribute.Description;
    }
}


internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            o.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "openid" },
                            { "profile", "profile" }
                        }
                    }
                }
            });

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Keycloak",
                            Type = ReferenceType.SecurityScheme
                        },
                        In = ParameterLocation.Header,
                        Name = "Bearer",
                        Scheme = "Bearer"
                    },
                    []
                }
            };

            o.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }
}
