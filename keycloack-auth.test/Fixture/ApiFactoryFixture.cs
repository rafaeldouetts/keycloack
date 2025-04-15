using keycloack_auth.test.Factory;
using keycloack_auth.test.Fixture.Base;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Testcontainers.Keycloak;
using DotNet.Testcontainers.Builders;
using keycloack_auth.test.Constants;

namespace keycloack_auth.test.Fixture
{
    public class ApiFactoryFixture : TestFixtureBase, IAsyncLifetime
    {
        public string? BaseAddress { get; set; } = "https://localhost:7443";
        private readonly KeycloakContainer _container;
        public CleanArchitectureWebApplicationFactory Factory { get; private set; }

        public HttpClient ServerClient { get; private set; }
        public string _tokenAdmin;
        public ApiFactoryFixture()
        {
            _container = new KeycloakBuilder()
           .WithImage("quay.io/keycloak/keycloak:24.0")  // Definindo a imagem do Keycloak
           .WithName("keycloack")
           .WithPortBinding(7080, 7080)  // Bind para a porta 7080 (HTTP)
           .WithPortBinding(7443, 7443)  // Bind para a porta 7443 (HTTPS)

           // Definindo variáveis de ambiente
           .WithEnvironment("KC_HOSTNAME", "localhost")  // Definindo o hostname
           .WithEnvironment("KC_HOSTNAME_PORT", "7080")  // Definindo a porta
           .WithEnvironment("KC_HOSTNAME_STRICT_BACKCHANNEL", "true")  // Definindo o backchannel
           .WithEnvironment("KEYCLOAK_ADMIN", "admin")  // Definindo o usuário administrador
           .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")  // Definindo a senha do admin
           .WithEnvironment("KC_HEALTH_ENABLED", "true")  // Habilitando health check
           .WithEnvironment("KC_LOG_LEVEL", "info")  // Configuração de log
           .WithResourceMapping("./Import/import.json", "/opt/keycloak/data/import")
           //.WithResourceMapping("./Certs", "/opt/keycloak/certs")
           //.WithEnvironment("KC_HTTPS_CERTIFICATE_FILE", "/opt/keycloak/certs/certificate.pem")
           //.WithEnvironment("KC_HTTPS_CERTIFICATE_KEY_FILE", "/opt/keycloak/certs/certificate.key")
           .WithCommand("--http-port", "7080", "--https-port", "7443", "--import-realm")  // Configura o comando para iniciar o Keycloak
           .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(7080))  // Espera até o Keycloak estar disponível na porta 7080
                                                                                  //.WithVolumeMount("./certs", "/opt/keycloak/certs")  // Mapeia o diretório de certificados
           .Build();  // Construi o contêiner
        }

        public List<User?> GetUsers()
        {
            string jsonFilePath = "Import/import.json";

            string jsonContent = File.ReadAllText(jsonFilePath);

            var jsonResponse = JsonConvert.DeserializeObject<JsonResponse>(jsonContent);

            if (jsonResponse?.users?.Count > 0) return jsonResponse.users;
            else return null;
        }

        public string GetRealmName()
        {
            string jsonFilePath = "Import/import.json";

            string jsonContent = File.ReadAllText(jsonFilePath);

            JObject jsonObject = JObject.Parse(jsonContent);

            return jsonObject["realm"]?.ToString();
        }

        public async Task InitializeAsync()
        {
            //await _container.StartAsync();

            await StartKeycloakAndGetToken();

        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _container.StopAsync();
        }

        public async Task StartKeycloakAndGetToken()
        {
            await _container.StartAsync();

            using (var client = new HttpClient())
            {
                var tokenUrl = "http://localhost:7080/realms/master/protocol/openid-connect/token";

                var formData = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", UserConstants.AdminPassword),
                new KeyValuePair<string, string>("grant_type", "password")
                });

                var response = await client.PostAsync(tokenUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token: {responseContent}");

                    var accessToken = ExtractAccessToken(responseContent);

                    _tokenAdmin = accessToken;

                    var users = GetUsers();

                    if (users == null || users.Count == 0) return;

                    var realmName = GetRealmName();

                    Factory = new CleanArchitectureWebApplicationFactory(RegisterCustomServicesHandler);

                    ServerClient = Factory.CreateClient(); // HttpClient configurado com base address
                }
                else
                {
                    Console.WriteLine($"Erro ao obter o token: {response.StatusCode}");
                }
            }
        }

        private string ExtractAccessToken(string tokenResponseBody)
        {
            var tokenJson = JObject.Parse(tokenResponseBody);
            return tokenJson["access_token"]?.ToString();
        }
    }

    public class JsonResponse
    {
        public List<User> users { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public bool emailVerified { get; set; }
        public long createdTimestamp { get; set; }
        public bool enabled { get; set; }
        public bool totp { get; set; }
        public List<object> credentials { get; set; }
        public List<object> disableableCredentialTypes { get; set; }
        public List<object> requiredActions { get; set; }
        public List<string> realmRoles { get; set; }
        public int notBefore { get; set; }
        public List<object> groups { get; set; }
    }
}
