using keycloack_auth.test.Constants;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using keycloack_auth.test.Fixture;
using keycloack_auth.test.Fixture.Collection;
using System.Net.Http.Json;

namespace keycloack_auth.test;

[Collection(nameof(ApiFactoryFixtureCollection))]
public class AuthenticateEndpointTests(ApiFactoryFixture apiFactory)
{
    HttpClient _client = new HttpClient();
    ApiFactoryFixture _apiFactory = apiFactory;

    [Fact]
    public async Task Test1()
    {
        //Arrange

        //The realm and the client configured in the Keycloak server
        var realm = "myrealm";
        var client = "myclient";

        //Keycloak server token endpoint
        var tokenUrl = $"http://localhost:7080/realms/{realm}/protocol/openid-connect/token";

        var apiUrl = "users/me";

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", $"{client}"),
            new KeyValuePair<string, string>("username", "myuser"),
            new KeyValuePair<string, string>("password", UserConstants.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        //Get the access token from the Keycloak server
        var response = await _client.PostAsync(tokenUrl, formData);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var token = content?["access_token"]?.ToString();

        //token = _apiFactory.GetToken();
        //Act

        //Add the access token to request header
        _apiFactory.ServerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Call the Api secure endpoint
        //var result = await _httpClient.GetAsync(apiUrl);
        var result = await _apiFactory.ServerClient.GetAsync(apiUrl);

        //Assert
        Assert.True(result.IsSuccessStatusCode);
    }
}
