using Dataplace.Core.Comunications;
using Dataplace.Core.Domain.Commands;
using Dataplace.Core.Domain.Events;
using Dataplace.Core.Domain.Interfaces.UoW;
using Dataplace.Core.Domain.Notifications;
using Dataplace.Imersao.Core.Application.Orcamentos.Commands;
using Dataplace.Imersao.Core.Application.Orcamentos.Events;
using Dataplace.Imersao.Core.Domain.Orcamentos.Repositories;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using Dataplace.Imersao.Core.Domain.Services;
using Dataplace.Imersao.Core.Infra.Configurations;
using Dataplace.Imersao.Core.Tests.Fixtures;
using Dataplace.Imersao.Core.Tests.Fixtures.FakeOjetcts;
using MediatR;
using Moq;
using Moq.AutoMock;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dataplace.Imersao.Core.Tests.Application.Orcamento
{
    [Collection(nameof(OrcamentoCollection))]
    public class OrcamentoItemCommandHandler
    {
        private readonly OrcamentoFixture _fixture;
        private readonly OrcamentoItemFixture _itemFixture;

        public OrcamentoItemCommandHandler(OrcamentoFixture fixture, OrcamentoItemFixture itemFixture)
        {
            _fixture = fixture;
            _itemFixture = itemFixture;
        }

        #region Inserir
        [Fact, Trait("OrcamentoItemCommandHandler", "Inserir")]
        public async Task OrcamentoItemComCamposConformesDeveInserirComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var viewModel = _itemFixture.GerarOrcamentoItemViewModelValido(orcamento.NumOrcamento, 1, 10, 5, 5, 5);

            var command = new AdicionarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.Commit(default))
                .Returns(CommandResponse.Ok);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.AdicionarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns<Core.Domain.Orcamentos.OrcamentoItem>((item) => item);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.True(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.AdicionarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemAdicionadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoItemCommandHandler", "Inserir")]
        public async Task OrcamentoItemInvalidoNaoDeveInserir()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _itemFixture.GerarOrcamentoItemViewModelInvalido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new AdicionarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.Commit(default))
                .Returns(CommandResponse.Fail);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.AdicionarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns<Core.Domain.Orcamentos.OrcamentoItem>((item) => item);

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.False(result);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoAdicionadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);

            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }
        #endregion

        #region Atualizar
        [Fact, Trait("OrcamentoItemCommandHandler", "Atualizar")]
        public async Task OrcamentoItemComCamposConformesDeveAtualizarComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var item = _itemFixture.NovoOrcamentoItemValido(orcamento.NumOrcamento, 10);

            var viewModel = _itemFixture.GerarOrcamentoItemViewModelValido(orcamento.NumOrcamento, 0, 10, 5, 5, 5);

            var command = new AtualizarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.Commit(default))
                .Returns(CommandResponse.Ok);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ObterItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(item);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.AtualizarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.True(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.AtualizarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemAtualizadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoItemCommandHandler", "Atualizar")]
        public async Task OrcamentoItemInvalidoNaoDeveAtualizar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var item = _itemFixture.NovoOrcamentoItemValido(orcamento.NumOrcamento, 10);

            var viewModel = _itemFixture.GerarOrcamentoItemViewModelInvalido(orcamento.NumOrcamento, 0);

            var command = new AtualizarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ObterItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(item);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.AtualizarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.False(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.AtualizarItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemAtualizadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);

            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }
        #endregion

        #region Deletar
        [Fact, Trait("OrcamentoItemCommandHandler", "Deletar")]
        public async Task OrcamentoItemConformeDeveDeletarComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var item = _itemFixture.NovoOrcamentoItemValido(orcamento.NumOrcamento, 10);

            var viewModel = _itemFixture.GerarOrcamentoItemViewModelValido(orcamento.NumOrcamento, 0, 10, 5, 5, 5);

            var command = new DeletarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.Commit(default))
                .Returns(CommandResponse.Ok);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ObterItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(item);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.True(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemDeletadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoItemCommandHandler", "Deletar")]
        public async Task OrcamentoItemTentarDeletarItemInexistenteDeveRetornarErro()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var item = _itemFixture.NovoOrcamentoItemValido(orcamento.NumOrcamento, 10);

            var viewModel = _itemFixture.GerarOrcamentoItemViewModelValido(orcamento.NumOrcamento, 0, 10, 5, 5, 5);

            var command = new DeletarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));
            
            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ObterItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => default);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.False(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemDeletadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);

            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoItemCommandHandler", "Deletar")]
        public async Task OrcamentoItemTentarDeletarItemDeOrcamentoInexistenteDeveRetornarErro()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var orcamento = _fixture.NovoOrcamentoValido();
            var item = _itemFixture.NovoOrcamentoItemValido(orcamento.NumOrcamento, 10);

            var viewModel = _itemFixture.GerarOrcamentoItemViewModelValido(orcamento.NumOrcamento, 0, 10, 5, 5, 5);

            var command = new DeletarOrcamentoItemCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(_fixture.CdEmpresa);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(_fixture.CdFilial);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(_fixture.TabelaPreco.CdTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(_fixture.TabelaPreco.SqTabela);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(_fixture.Cliente.Codigo);

            mocker
                .GetMock<IUnitOfWork>()
                .Setup(uow => uow.BeginTransaction())
                .Returns(default(int));

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(_fixture.CdEmpresa, _fixture.CdFilial, orcamento.NumOrcamento))
                .Returns(() => default);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ObterItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(item);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Setup(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoService>()
                .Setup(repo => repo.ObterProdutoPreco(It.IsAny<Core.Domain.Orcamentos.Orcamento>(), It.IsAny<Core.Domain.Orcamentos.ValueObjects.OrcamentoProduto>()))
                .Returns(() => new OrcamentoItemPrecoPercentual(viewModel.PrecoVenda, 0));

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act
            var result = await handler.Handle(command, CancellationToken.None);

            //assert
            Assert.False(result);

            mocker
                .GetMock<IOrcamentoItemRepository>()
                .Verify(repo => repo.ExcluirItem(It.IsAny<Core.Domain.Orcamentos.OrcamentoItem>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoItemDeletadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);

            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }
        #endregion
    }
}
