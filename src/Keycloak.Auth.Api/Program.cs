using System.Security.Claims;
using Keycloak.Auth.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);


builder.Services.AddAuthorization();
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    o =>
    {
        o.RequireHttpsMetadata = false;
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
        o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"]
        };
    });


//builder.Services.AddOpenTelemetry

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("users/me", (ClaimsPrincipal claimsPrincipal) =>
{
    return claimsPrincipal.Claims.ToDictionary(x => x.Type, x => x.Value);
}).RequireAuthorization();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
