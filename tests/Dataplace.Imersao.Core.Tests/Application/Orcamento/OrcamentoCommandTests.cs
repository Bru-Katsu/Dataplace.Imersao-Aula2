using Dataplace.Imersao.Core.Application.Orcamentos.Commands;
using Dataplace.Imersao.Core.Domain.Orcamentos.Repositories;
using Dataplace.Imersao.Core.Tests.Fixtures;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dataplace.Imersao.Core.Tests.Application.Orcamento
{
    public class OrcamentoCommandTests
    {
        private readonly OrcamentoFixture _fixture;
        public OrcamentoCommandTests()
        {

        }
        [Fact, Trait("Orçamento", "Reabrir Item")]
        public void OrcamentoReabertoCommandTest()
        {
            //arrange
            var command = new ReabrirOrcamentoCommand(1);
            var orcamento = _fixture.NovoOrcamento();

            var mocker = new AutoMocker();


            var commandHandler = mocker.CreateInstance<OrcamentoCommandHandler>();

            //mocker
            //    .GetMock<IPetUnitOfWork>()
            //    .Setup(uow => uow.BeginTransaction())
            //    .Returns(default(int));

            //mocker
            //    .GetMock<IOrcamentoRepository>()
            //    .Setup(repo => repo.ObterOrcamento(, orcamento.NumOrcamento))
            //    .Returns(orcamento);

            //mocker
            //    .GetMock<IServicoRepository>()
            //    .Setup(repo => repo.GetById(servico.Id))
            //    .Returns(servico);

            //act

            //assert
        }
    }
}
