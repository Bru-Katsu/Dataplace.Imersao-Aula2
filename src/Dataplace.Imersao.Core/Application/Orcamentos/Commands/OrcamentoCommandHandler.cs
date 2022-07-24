using Dataplace.Core.Comunications;
using Dataplace.Core.Domain.CommandHandlers;
using Dataplace.Core.Domain.Interfaces.UoW;
using Dataplace.Core.Domain.Notifications;
using Dataplace.Imersao.Core.Application.Orcamentos.Events;
using Dataplace.Imersao.Core.Domain.Orcamentos;
using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using Dataplace.Imersao.Core.Domain.Services;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Commands
{
    public class OrcamentoCommandHandler: 
         CommandHandler,
         IRequestHandler<AdicionarOrcamentoCommand, bool>,
         IRequestHandler<FecharOrcamentoCommand, bool>,
         IRequestHandler<AdicionarOrcamentoItemCommand, bool>,
         IRequestHandler<ReabrirOrcamentoCommand, bool>,
         IRequestHandler<CancelarOrcamentoCommand, bool>,
         IRequestHandler<AtualizarOrcamentoCommand, bool>,
         IRequestHandler<DeletarOrcamentoCommand, bool>
    {
        #region fields
        private Domain.Orcamentos.Repositories.IOrcamentoRepository _orcamentoRepository;
        private Domain.Orcamentos.Repositories.IOrcamentoItemRepository _orcamentoItemRepository;
        private readonly IOrcamentoService _orcamentoService;
        #endregion

        #region constructor
        public OrcamentoCommandHandler(
            IUnitOfWork uow,
            IMediatorHandler mediator,
            INotificationHandler<DomainNotification> notifications,
            Domain.Orcamentos.Repositories.IOrcamentoRepository orcamentoRepository,
            Domain.Orcamentos.Repositories.IOrcamentoItemRepository orcamentoItemRepository,
            IOrcamentoService orcamentoService) : base(uow, mediator, notifications)
        {
            _orcamentoRepository = orcamentoRepository;
            _orcamentoItemRepository = orcamentoItemRepository;
            _orcamentoService = orcamentoService;
        }
        #endregion

        #region orçamento
        public async Task<bool> Handle(AdicionarOrcamentoCommand message, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;

            var cliente = message.Item.CdCliente.Trim().Length > 0 ? new OrcamentoCliente(message.Item.CdCliente) : ObterClientePadrao();
            var usuario = ObterUsuarioLogado();
            var tabelaPreco = string.IsNullOrEmpty(message.Item.CdTabela) && message.Item.SqTabela.HasValue ? new OrcamentoTabelaPreco(message.Item.CdTabela, message.Item.SqTabela.Value) : ObterTabelaPrecoPadrao();
            var vendedor = message.Item.CdVendedor.Trim().Length > 0 ? new OrcamentoVendedor(message.Item.CdVendedor) : ObterVendedorPadrao();

            var orcamento = Orcamento.Factory.NovoOrcamento(
                cdEmpresa,
                cdEmpresa,
                cliente,
                usuario,
                vendedor,
                tabelaPreco);

            if (message.Item.DiasValidade.HasValue && message.Item.DiasValidade.Value > 0)
                orcamento.DefinirValidade(message.Item.DiasValidade.Value);

            if (!orcamento.IsValid())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return false;
            }
            if (!_orcamentoRepository.AdicionarOrcamento(orcamento))
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");

            message.Item.NumOrcamento = orcamento.NumOrcamento;

            AddEvent(new OrcamentoAdicionadoEvent(message.Item));

            return Commit(transactionId);

        }

        public async Task<bool> Handle(FecharOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;

            var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.NumOcamento);
            if(orcamento == null)          
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return false;
            }

            orcamento.FecharOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return false;
            }
   

            AddEvent(new Orcamentos.Events.OrcamentoFechadoEvent(request.NumOcamento));
            return Commit(transactionId);
        }
        #endregion

        #region itens
        public async Task<bool> Handle(AdicionarOrcamentoItemCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();


            var orcamento = _orcamentoRepository.ObterOrcamento(request.Item.CdEmpresa, request.Item.CdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "Orçamento não encontrado");
                return false;
            }

            if (orcamento.PermiteAlteracaoItem())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return false;
            }

            var tpRegistro = request.Item.TpRegistro.ToTpRegistroEnum();
            var produto = !string.IsNullOrEmpty((request.Item.CdProduto ?? "").Trim()) && tpRegistro.HasValue ?
                new OrcamentoProduto(tpRegistro.Value, request.Item.CdProduto) : default;
            if (produto == null)
            {
                NotifyErrorValidation("notFound", "Dados do produto inválido");
                return false;
            }


            var quantidade = request.Item.Quantidade;
            // cross aggreagate service
            var preco = _orcamentoService.ObterProdutoPreco(orcamento, produto);
            if (preco == null)
            {
                NotifyErrorValidation("notFound", "Dados do preço inválido");
                return false;
            }

            var item = orcamento.AdicionarItem(produto, quantidade, preco);


            var itemAdicionado = _orcamentoItemRepository.AdicionarItem(item);

            if (itemAdicionado == null)
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");
            request.Item.Seq = itemAdicionado.Seq;


            AddEvent(new OrcamentoItemAdicionadoEvent(request.Item));

            return Commit(transactionId);
        }
        #endregion


        public async Task<bool> Handle(ReabrirOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;

            var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return false;
            }

            orcamento.ReabrirOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return false;
            }

            AddEvent(new Orcamentos.Events.OrcamentoFechadoEvent(request.NumOrcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(CancelarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;

            var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return false;
            }

            orcamento.CancelarOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return false;
            }

            AddEvent(new Orcamentos.Events.OrcamentoCanceladoEvent(request.NumOrcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(AtualizarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;

            var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return false;
            }

            orcamento.DefinirCliente(new OrcamentoCliente(request.Item.CdCliente));

            if (request.Item.DiasValidade.HasValue)
                orcamento.DefinirValidade(request.Item.DiasValidade.Value);
            else
                orcamento.RemoverValidade();

            orcamento.DefinirVendedor(new OrcamentoVendedor(request.Item.CdVendedor));
            orcamento.DefinirTabelaPreco(new OrcamentoTabelaPreco(request.Item.CdTabela, request.Item.SqTabela.Value));

            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocorreu um problema com a persistência dos dados");
                return false;
            }

            AddEvent(new Orcamentos.Events.OrcamentoAtualizadoEvent(request.Item.NumOrcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(DeletarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();
            var cdEmpresa = dpLibrary05.mGenerico.SymPRM.cdempresa;
            var cdFilial = dpLibrary05.mGenerico.SymPRM.cdfilial;


            var orcamento = _orcamentoRepository.ObterOrcamento(cdEmpresa, cdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "Orçamento não encontrado");
                return false;
            }

            if (!_orcamentoRepository.ExcluirOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocorreu um problema com a persistência dos dados");
                return false;
            }

            foreach (var item in _orcamentoItemRepository.ObterItens(cdEmpresa, cdFilial, request.Item.NumOrcamento))
            {
                if (!_orcamentoItemRepository.ExcluirItem(item))
                {
                    NotifyErrorValidation("orcamento", "Ocorreu um problema com a persistência dos dados");
                }
            }

            if (HasNotifications())
                return false;

            AddEvent(new Orcamentos.Events.OrcamentoDeletadoEvent(request.Item.NumOrcamento));
            return Commit(transactionId);
        }

        #region internals
        public OrcamentoCliente ObterClientePadrao()
        {
            return new OrcamentoCliente("31112");
        }
        public OrcamentoTabelaPreco ObterTabelaPrecoPadrao()
        {
            return new OrcamentoTabelaPreco("2005", 1);
        }
        public OrcamentoUsuario ObterUsuarioLogado()
        {
            return new OrcamentoUsuario("sa");
        }
        public OrcamentoVendedor ObterVendedorPadrao()
        {
            return new OrcamentoVendedor("00");
        }


        #endregion

    }
}
