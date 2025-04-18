using keycloack_auth.test.Constants;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using keycloack_auth.test.Fixture;
using keycloack_auth.test.Fixture.Collection;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace keycloack_auth.test;

[Collection(nameof(ApiFactoryFixtureCollection))]
public class AuthenticateEndpointTests(ApiFactoryFixture apiFactory)
{
    HttpClient _client = new HttpClient();
    ApiFactoryFixture _apiFactory = apiFactory;

    [Fact]
    public async Task GetUsersMe_SemToken_DeveRetornarForbidden()
    {
        //Arrange
        var apiUrl = "users/me";

        //Act
        var result = await _apiFactory.ServerClient.GetAsync(apiUrl);

        var teste = result.Content.ReadAsStringAsync();

        //Assert
        Assert.False(result.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetUsersMe_ComTokenSemRoleGestor_DeveRetornarForbidden()
    {
        //Arrange
        var realm = "myrealm";
        var client = "myclient";

        var tokenUrl = $"http://localhost:7080/realms/{realm}/protocol/openid-connect/token";

        var apiUrl = "users/me";

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", $"{client}"),
            new KeyValuePair<string, string>("username", "myuser"),
            new KeyValuePair<string, string>("password", UserConstants.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        var response = await _client.PostAsync(tokenUrl, formData);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var token = content?["access_token"]?.ToString();

        _apiFactory.ServerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Act

        var result = await _apiFactory.ServerClient.GetAsync(apiUrl);

        var teste = result.Content.ReadAsStringAsync();

        //Assert
        Assert.False(result.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetUsersMe_ComTokenComRoleGestor_DeveRetornarOk()
    {
        //Arrange
        var realm = "myrealm";
        var client = "myclient";

        var tokenUrl = $"http://localhost:7080/realms/{realm}/protocol/openid-connect/token";

        var apiUrl = "users/me";

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", $"{client}"),
            new KeyValuePair<string, string>("username", "gestor"),
            new KeyValuePair<string, string>("password", UserConstants.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        var response = await _client.PostAsync(tokenUrl, formData);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var token = content?["access_token"]?.ToString();
        _apiFactory.ServerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
        //Act

        var result = await _apiFactory.ServerClient.GetAsync(apiUrl);

        var teste = result.Content.ReadAsStringAsync();

        //Assert
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
