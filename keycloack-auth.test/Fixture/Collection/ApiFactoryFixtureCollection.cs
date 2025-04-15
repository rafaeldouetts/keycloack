using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace keycloack_auth.test.Fixture.Collection
{
    [CollectionDefinition(nameof(ApiFactoryFixtureCollection))]
    public class ApiFactoryFixtureCollection : ICollectionFixture<ApiFactoryFixture>
    {
    }
}
