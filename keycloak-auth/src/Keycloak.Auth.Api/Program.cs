using System.Security.Claims;
using Keycloak.Auth.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"]
        };
    });

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Keycloak.Auth.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter();
    });

Role[] allRoles = typeof(Role)
    .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
    .Where(f => f.FieldType == typeof(Role))
    .Select(f => (Role)f.GetValue(null)!)
    .ToArray();

foreach (Role role in allRoles)
{
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(role.Value, policy =>
        {
            policy.Requirements.Add(new RealmRoleRequirement(role.Value));
        });
    });
}

// E registrar o handler
builder.Services.AddSingleton<IAuthorizationHandler, RealmRoleHandler>();


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("users/me", (ClaimsPrincipal claimsPrincipal) =>
{
    return "sucesso";
}).RequireAuthorization(Role.Gestor.ToString());

app.UseAuthentication();

app.UseAuthorization();

app.Run();



#pragma warning disable CA1515
public partial class Program { }
#pragma warning restore CA1515
