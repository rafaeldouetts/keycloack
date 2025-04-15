using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace keycloack_auth.test.Factory
{
    public class CleanArchitectureWebApplicationFactory : WebApplicationFactory<Program>
    {
        public delegate void RegisterCustomServicesHandler(
            IServiceCollection services);

        //private readonly HttpClient _httpClient;
        private readonly Action<IServiceCollection> _customServices;
        public CleanArchitectureWebApplicationFactory(Action<IServiceCollection> customServices = null)
        {
            //_httpClient = factory.CreateClient();
            _customServices = customServices;
        }

        //protected override void ConfigureWebHost(IWebHostBuilder builder)
        //{
        //    var tokenUrl = "http://localhost:7080/realms/myrealm";
        //    var response = _httpClient.GetAsync(tokenUrl).Result;

        //    var content = response.Content.ReadAsStringAsync().Result;

        //    builder.UseEnvironment("Development");

        //    Environment.SetEnvironmentVariable("Authentication:MetadataAddress",
        //        "http://localhost:8080/realms/keycloak-auth-demo/.well-known/openid-configuration");

        //    base.ConfigureWebHost(builder);
        //}
    }
}
