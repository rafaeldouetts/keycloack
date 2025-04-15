using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using keycloack_auth.test.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace keycloack_auth.test.Fixture.Base
{
    public class TestFixtureBase
    {
        public HttpClient ServerClient { get; set; }
        public CleanArchitectureWebApplicationFactory Factory { get; set; }

        public TestFixtureBase(bool useTestAuthentication = true)
        {
        }

        protected virtual void RegisterCustomServicesHandler(
            IServiceCollection services)
        {
        }
    }
}
