using Dataplace.Core.Comunications;
using Dataplace.Core.Domain.Bus;
using Dataplace.Core.Domain.Commands;
using Dataplace.Core.Domain.Events;
using Dataplace.Core.Domain.Interfaces.UoW;
using Dataplace.Core.Domain.Notifications;
using Dataplace.Imersao.Core.Application.Orcamentos.Commands;
using Dataplace.Imersao.Core.Application.Orcamentos.Events;
using Dataplace.Imersao.Core.Domain.Exections;
using Dataplace.Imersao.Core.Domain.Orcamentos;
using Dataplace.Imersao.Core.Domain.Orcamentos.Repositories;
using Dataplace.Imersao.Core.Infra.Configurations;
using Dataplace.Imersao.Core.Tests.Fixtures;
using Dataplace.Imersao.Core.Tests.Fixtures.FakeOjetcts;
using MediatR;
using Moq;
using Moq.AutoMock;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace Dataplace.Imersao.Core.Tests.Application.Orcamento
{
    [Collection(nameof(OrcamentoCollection))]
    public class OrcamentoCommandHandlerTests
    {
        private readonly OrcamentoFixture _fixture;

        public OrcamentoCommandHandlerTests(OrcamentoFixture fixture)
        {
            _fixture = fixture;
        }

        #region Inserir
        [Fact, Trait("OrcamentoCommandHandler", "Inserir")]
        public async Task OrcamentoComCamposConformesDeveInserirComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);

            var command = new AdicionarOrcamentoCommand(viewModel);

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
                .Setup(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);


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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoAdicionadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Inserir")]
        public async Task OrcamentoSemInformacoesDeveBuscarDefault()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelSemCamposOpcionais(1, 0);

            var command = new AdicionarOrcamentoCommand(viewModel);

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
                .Setup(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoAdicionadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Inserir")]
        public async Task OrcamentoInvalidoNaoDeveInserir()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelSemCamposOpcionais(1, 0);

            var command = new AdicionarOrcamentoCommand(viewModel);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdEmpresa)
                .Returns(string.Empty);

            mocker
                .GetMock<IDdEmpresa>()
                .Setup(ddEmpresa => ddEmpresa.CdFilial)
                .Returns(string.Empty);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdTabela)
                .Returns(string.Empty);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.SqTabela)
                .Returns(null);

            mocker
                .GetMock<IPrmVda>()
                .Setup(prmVda => prmVda.CdCliente)
                .Returns(string.Empty);

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
                .Setup(repo => repo.AdicionarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

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
        [Fact, Trait("OrcamentoCommandHandler", "Atualizar")]
        public async Task OrcamentoComCamposConformesDeveAtualizarComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new AtualizarOrcamentoCommand(viewModel);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoAtualizadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Atualizar")]
        public async Task OrcamentoInvalidoNaoDeveAtualizar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelSemCamposOpcionais(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new AtualizarOrcamentoCommand(viewModel);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

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
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoAtualizadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);

            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }
        #endregion

        #region Cancelar
        [Fact, Trait("OrcamentoCommandHandler", "Cancelar")]
        public async Task OrcamentoAbertoEValidoDeveCancelarComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new CancelarOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoCanceladoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Cancelar")]
        public async Task OrcamentoFechadoNaoDeveCancelar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.FecharOrcamento();

            var command = new CancelarOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act & assert
            await Assert.ThrowsAsync<Core.Domain.Exections.DomainException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });            

            mocker
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoCanceladoEvent))), Times.Never);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Cancelar")]
        public async Task OrcamentoJaCanceladoNaoDeveCancelar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.CancelarOrcamento();

            var command = new CancelarOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act & assert
            await Assert.ThrowsAsync<Core.Domain.Exections.DomainException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            mocker
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoCanceladoEvent))), Times.Never);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }
        #endregion

        #region Fechar
        [Fact, Trait("OrcamentoCommandHandler", "Fechar")]
        public async Task OrcamentoAbertoEValidoDeveFecharComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new FecharOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoFechadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Fechar")]
        public async Task OrcamentoCanceladoNaoDeveFechar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.CancelarOrcamento();

            var command = new FecharOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act & assert
            await Assert.ThrowsAsync<DomainException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            mocker
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoFechadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Never);
        }

        [Fact, Trait("OrcamentoCommandHandler", "Fechar")]
        public async Task OrcamentoJaFechadoNaoDeveFechar()
        {
            //arrange
            var mocker = new AutoMocker();
            mocker.Use<INotificationHandler<DomainNotification>>(new FakeDomainNotification());

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.FecharOrcamento();

            var command = new FecharOrcamentoCommand(viewModel.NumOrcamento);

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
                .Setup(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IMediatorHandler>()
                .Setup(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(DomainNotification))))
                .Callback<Event>(async e =>
                {
                    var notification = mocker.Get<INotificationHandler<DomainNotification>>();
                    await notification.Handle((DomainNotification)e, CancellationToken.None);
                });

            //act & assert
            await Assert.ThrowsAsync<DomainException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            mocker
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.AtualizarOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoFechadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Never);
        }
        #endregion

        #region Deletar
        [Fact, Trait("OrcamentoCommandHandler", "Deletar")]
        public async Task OrcamentoAbertoDeveDeletarComSucesso()
        {
            //arrange
            var mocker = new AutoMocker();
            var notificationHandlerInstance = new FakeDomainNotification();
            mocker.Use<INotificationHandler<DomainNotification>>(notificationHandlerInstance);

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();

            var command = new DeletarOrcamentoCommand(viewModel);

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
                .Setup(repo => repo.ExcluirOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(true);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

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
                .GetMock<IOrcamentoRepository>()
                .Verify(repo => repo.ExcluirOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoDeletadoEvent))), Times.Once);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Commit(default), Times.Once);

            Assert.False(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
        }

        [Fact, Trait("OrcamentoCommandHandler", "Deletar")]
        public async Task OrcamentoInexistenteDeveRetornarMensagemErro()
        {
            //arrange
            var mocker = new AutoMocker();
            var notificationHandlerInstance = new FakeDomainNotification();
            mocker.Use<INotificationHandler<DomainNotification>>(notificationHandlerInstance);

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.CancelarOrcamento();

            var command = new DeletarOrcamentoCommand(viewModel);

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
                .Returns(CommandResponse.Fail);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(default(Core.Domain.Orcamentos.Orcamento));

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
                .Verify(repo => repo.ExcluirOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Never);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoDeletadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);


            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
            Assert.Contains(mocker.Get<INotificationHandler<DomainNotification>>().GetNotifications().Select(x => x.Value), (validation) => validation == "Orçamento não encontrado");
        }

        [Fact, Trait("OrcamentoCommandHandler", "Deletar")]
        public async Task OrcamentoFalhaEmPersistenciaDeveRetornarMensagemErro()
        {
            //arrange
            var mocker = new AutoMocker();
            var notificationHandlerInstance = new FakeDomainNotification();
            mocker.Use<INotificationHandler<DomainNotification>>(notificationHandlerInstance);

            var handler = mocker.CreateInstance<OrcamentoCommandHandler>();
            var viewModel = _fixture.GerarOrcamentoViewModelValido(1, 0);
            var orcamento = _fixture.NovoOrcamentoValido();
            orcamento.CancelarOrcamento();

            var command = new DeletarOrcamentoCommand(viewModel);

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
                .Returns(CommandResponse.Fail);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ObterOrcamento(It.Is<string>(s => s == _fixture.CdEmpresa), It.Is<string>(s => s == _fixture.CdFilial), viewModel.NumOrcamento))
                .Returns(orcamento);

            mocker
                .GetMock<IOrcamentoRepository>()
                .Setup(repo => repo.ExcluirOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()))
                .Returns(false);

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
                .Verify(repo => repo.ExcluirOrcamento(It.IsAny<Core.Domain.Orcamentos.Orcamento>()), Times.Once);

            //literalmente gambiarra esse It.Is<>, se fosse um dynamic lá dentro da lista do CommandHandler recebendo o evento no AddEvent por restrição genérica daria para usar o:

            //mocker
            //    .GetMock<IMediatorHandler>()
            //    .Verify(m => m.RaiseEvent(It.IsAny<OrcamentoAdicionadoEvent>()), Times.Once);

            //pois em tempo de exec vai bater a tipagem. Fica uma sugestão, rs.

            mocker
                .GetMock<IMediatorHandler>()
                .Verify(m => m.RaiseEvent(It.Is<Event>(x => x.GetType().Name == nameof(OrcamentoDeletadoEvent))), Times.Never);

            mocker
                .GetMock<IUnitOfWork>()
                .Verify(uow => uow.Rollback(default), Times.Once);


            Assert.True(mocker.Get<INotificationHandler<DomainNotification>>().HasNotifications());
            Assert.Contains(mocker.Get<INotificationHandler<DomainNotification>>().GetNotifications().Select(x => x.Value), (validation) => validation == "Ocorreu um problema com a persistência dos dados");
        }
        #endregion
    }
}
