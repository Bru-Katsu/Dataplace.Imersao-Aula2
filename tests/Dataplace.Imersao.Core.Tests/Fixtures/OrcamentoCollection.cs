using Xunit;

namespace Dataplace.Imersao.Core.Tests.Fixtures
{
    [CollectionDefinition(nameof(OrcamentoCollection))]
    public class OrcamentoCollection : ICollectionFixture<OrcamentoFixture>,
                                       ICollectionFixture<OrcamentoItemFixture>
    { }
}
