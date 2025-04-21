using System.ComponentModel;
using System.Globalization;
using Google.Protobuf.WellKnownTypes;

namespace Keycloak.Auth.Api.Extensions;

internal sealed record Role(string Value)
{
    public static readonly Role Admin = new("admin");
    public static readonly Role User = new("user");
    public static readonly Role Guest = new("guest");
    public static readonly Role Gestor = new("gestor");

    public override string ToString() => Value.ToUpper(CultureInfo.InvariantCulture);
}

