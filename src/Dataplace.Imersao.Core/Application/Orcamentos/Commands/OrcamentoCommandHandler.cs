using Dataplace.Core.Comunications;
using Dataplace.Core.Domain.CommandHandlers;
using Dataplace.Core.Domain.Interfaces.UoW;
using Dataplace.Core.Domain.Notifications;
using Dataplace.Imersao.Core.Application.Orcamentos.Events;
using Dataplace.Imersao.Core.Domain.Orcamentos;
using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using Dataplace.Imersao.Core.Domain.Services;
using Dataplace.Imersao.Core.Infra.Configurations;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Commands
{
    public class OrcamentoCommandHandler :
         CommandHandler,
         IRequestHandler<AdicionarOrcamentoCommand, bool>,
         IRequestHandler<FecharOrcamentoCommand, bool>,
         IRequestHandler<AdicionarOrcamentoItemCommand, bool>,
         IRequestHandler<ReabrirOrcamentoCommand, bool>,
         IRequestHandler<CancelarOrcamentoCommand, bool>,
         IRequestHandler<AtualizarOrcamentoCommand, bool>,
         IRequestHandler<DeletarOrcamentoCommand, bool>,
         IRequestHandler<AtualizarOrcamentoItemCommand, bool>,
         IRequestHandler<DeletarOrcamentoItemCommand, bool>
    {
        #region fields
        private Domain.Orcamentos.Repositories.IOrcamentoRepository _orcamentoRepository;
        private Domain.Orcamentos.Repositories.IOrcamentoItemRepository _orcamentoItemRepository;
        private readonly IOrcamentoService _orcamentoService;
        private readonly IDdEmpresa _ddEmpresa;
        private readonly IPrmVda _prmVda;
        #endregion

        #region constructor
        public OrcamentoCommandHandler(
            IUnitOfWork uow,
            IMediatorHandler mediator,
            INotificationHandler<DomainNotification> notifications,
            Domain.Orcamentos.Repositories.IOrcamentoRepository orcamentoRepository,
            Domain.Orcamentos.Repositories.IOrcamentoItemRepository orcamentoItemRepository,
            IOrcamentoService orcamentoService,
            IDdEmpresa ddEmpresa,
            IPrmVda prmVda) : base(uow, mediator, notifications)
        {
            _orcamentoRepository = orcamentoRepository;
            _orcamentoItemRepository = orcamentoItemRepository;
            _orcamentoService = orcamentoService;
            _ddEmpresa = ddEmpresa;
            _prmVda = prmVda;
        }
        #endregion

        #region orçamento
        public async Task<bool> Handle(AdicionarOrcamentoCommand message, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();


            var cliente = message.Item.CdCliente.Trim().Length > 0 ? new OrcamentoCliente(message.Item.CdCliente) : ObterClientePadrao();
            var usuario = ObterUsuarioLogado();
            var tabelaPreco = string.IsNullOrEmpty(message.Item.CdTabela) && message.Item.SqTabela.HasValue ? new OrcamentoTabelaPreco(message.Item.CdTabela, message.Item.SqTabela.Value) : ObterTabelaPrecoPadrao();
            var vendedor = message.Item.CdVendedor.Trim().Length > 0 ? new OrcamentoVendedor(message.Item.CdVendedor) : ObterVendedorPadrao();

            var orcamento = Orcamento.Factory.NovoOrcamento(
                _ddEmpresa.CdEmpresa,
                _ddEmpresa.CdFilial,
                cliente,
                usuario,
                vendedor,
                tabelaPreco);

            if (message.Item.DiasValidade.HasValue && message.Item.DiasValidade.Value > 0)
                orcamento.DefinirValidade(message.Item.DiasValidade.Value);

            if (!orcamento.IsValid())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return Commit(transactionId);
            }

            if (!_orcamentoRepository.AdicionarOrcamento(orcamento))
            {
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            message.Item.NumOrcamento = orcamento.NumOrcamento;

            AddEvent(new OrcamentoAdicionadoEvent(message.Item));

            return Commit(transactionId);

        }

        public async Task<bool> Handle(FecharOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.NumOcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return Commit(transactionId);
            }

            orcamento.FecharOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }


            AddEvent(new Orcamentos.Events.OrcamentoFechadoEvent(request.NumOcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(ReabrirOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return Commit(transactionId);
            }

            orcamento.ReabrirOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            AddEvent(new Orcamentos.Events.OrcamentoFechadoEvent(request.NumOrcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(CancelarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return Commit(transactionId);
            }

            orcamento.CancelarOrcamento();
            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            AddEvent(new Orcamentos.Events.OrcamentoCanceladoEvent(request.NumOrcamento));
            return Commit(transactionId);
        }

        public async Task<bool> Handle(AtualizarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "orçamento não encotrado");
                return Commit(transactionId);
            }

            orcamento.DefinirCliente(new OrcamentoCliente(request.Item.CdCliente));

            if (request.Item.DiasValidade.HasValue)
                orcamento.DefinirValidade(request.Item.DiasValidade.Value);
            else
                orcamento.RemoverValidade();

            orcamento.DefinirVendedor(new OrcamentoVendedor(request.Item.CdVendedor));
            orcamento.DefinirTabelaPreco(new OrcamentoTabelaPreco(request.Item.CdTabela, request.Item.SqTabela.Value));
            
            if (!orcamento.IsValid())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return Commit(transactionId);
            }

            if (!_orcamentoRepository.AtualizarOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocorreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            AddEvent(new Orcamentos.Events.OrcamentoAtualizadoEvent(request.Item.NumOrcamento));
            return Commit(transactionId);
        }

        //testar permissão de exclusão quando orçamento está fechado ou cancelado
        public async Task<bool> Handle(DeletarOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "Orçamento não encontrado");
                return Commit(transactionId);
            }

            if (!_orcamentoRepository.ExcluirOrcamento(orcamento))
            {
                NotifyErrorValidation("orcamento", "Ocorreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            foreach (var item in _orcamentoItemRepository.ObterItens(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.Item.NumOrcamento))
            {
                if (!_orcamentoItemRepository.ExcluirItem(item))
                {
                    NotifyErrorValidation("orcamento", $"Ocorreu um problema com a persistência dos dados do item {item.Produto}");
                    return Commit(transactionId);
                }
            }

            if (HasNotifications())
                return Commit(transactionId);

            AddEvent(new Orcamentos.Events.OrcamentoDeletadoEvent(request.Item.NumOrcamento));
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
                return Commit(transactionId);
            }

            if (orcamento.PermiteAlteracaoItem())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return Commit(transactionId);
            }

            var tpRegistro = request.Item.TpRegistro.ToTpRegistroEnum();

            var produto = !string.IsNullOrEmpty((request.Item.CdProduto ?? "").Trim()) && tpRegistro.HasValue ? new OrcamentoProduto(tpRegistro.Value, request.Item.CdProduto) : default;
            if (produto == null)
            {
                NotifyErrorValidation("notFound", "Dados do produto inválido");
                return Commit(transactionId);
            }


            var quantidade = request.Item.Quantidade;
            // cross aggreagate service
            var preco = _orcamentoService.ObterProdutoPreco(orcamento, produto);
            if (preco == null)
            {
                NotifyErrorValidation("notFound", "Dados do preço inválido");
                return Commit(transactionId);
            }

            var item = orcamento.AdicionarItem(produto, quantidade, preco);
            var itemAdicionado = _orcamentoItemRepository.AdicionarItem(item);

            if (itemAdicionado == null)
            {
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            request.Item.Seq = itemAdicionado.Seq;

            AddEvent(new OrcamentoItemAdicionadoEvent(request.Item));

            return Commit(transactionId);
        }

        //ver se pode fechar/abrir por item do orçamento
        public async Task<bool> Handle(AtualizarOrcamentoItemCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(request.Item.CdEmpresa, request.Item.CdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "Orçamento não encontrado");
                return Commit(transactionId);
            }

            if (orcamento.PermiteAlteracaoItem())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return Commit(transactionId);
            }

            var item = _orcamentoItemRepository.ObterItem(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.Item.NumOrcamento, request.Item.Seq);
            if (item == null)
            {
                NotifyErrorValidation("notFound", "Item não encontrado!");
                return Commit(transactionId);
            }

            //produto
            var tpRegistro = request.Item.TpRegistro.ToTpRegistroEnum();
            var produto = !string.IsNullOrEmpty((request.Item.CdProduto ?? "").Trim()) && tpRegistro.HasValue ? new OrcamentoProduto(tpRegistro.Value, request.Item.CdProduto) : default;
            if (produto == null)
            {
                NotifyErrorValidation("notFound", "Dados do produto inválido");
                return Commit(transactionId);
            }

            item.DefinirProduto(produto);
            item.DefinirQuantidade(request.Item.Quantidade);

            var preco = _orcamentoService.ObterProdutoPreco(orcamento, produto);
            if (preco == null)
            {
                NotifyErrorValidation("notFound", "Dados do preço inválido");
                return Commit(transactionId);
            }

            item.AtrubuirPreco(preco);

            if (!_orcamentoItemRepository.AtualizarItem(item))
            {
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            AddEvent(new OrcamentoItemAtualizadoEvent(request.Item));

            return Commit(transactionId);
        }

        public async Task<bool> Handle(DeletarOrcamentoItemCommand request, CancellationToken cancellationToken)
        {
            var transactionId = BeginTransaction();

            var orcamento = _orcamentoRepository.ObterOrcamento(request.Item.CdEmpresa, request.Item.CdFilial, request.Item.NumOrcamento);
            if (orcamento == null)
            {
                NotifyErrorValidation("notFound", "Orçamento não encontrado");
                return Commit(transactionId);
            }

            if (orcamento.PermiteAlteracaoItem())
            {
                orcamento.Validation.Notifications.ToList().ForEach(val => NotifyErrorValidation(val.Property, val.Message));
                return Commit(transactionId);
            }

            var item = _orcamentoItemRepository.ObterItem(_ddEmpresa.CdEmpresa, _ddEmpresa.CdFilial, request.Item.NumOrcamento, request.Item.Seq);
            if (item == null)
            {
                NotifyErrorValidation("notFound", "Item não encontrado!");
                return Commit(transactionId);
            }

            if (!_orcamentoItemRepository.ExcluirItem(item))
            {
                NotifyErrorValidation("database", "Ocoreu um problema com a persistência dos dados");
                return Commit(transactionId);
            }

            AddEvent(new OrcamentoItemDeletadoEvent(request.Item));

            return Commit(transactionId);
        }

        #endregion




        #region internals
        //prmVda
        public OrcamentoCliente ObterClientePadrao()
        {
            return new OrcamentoCliente(_prmVda.CdCliente);
        }
        //prmVda
        public OrcamentoTabelaPreco ObterTabelaPrecoPadrao()
        {
            return new OrcamentoTabelaPreco(_prmVda.CdTabela, _prmVda.SqTabela);
        }
        //sei lá
        public OrcamentoUsuario ObterUsuarioLogado()
        {
            return new OrcamentoUsuario("sa");
        }
        //buscar com base em cliente, criar um repository para buscar
        public OrcamentoVendedor ObterVendedorPadrao()
        {
            return new OrcamentoVendedor("00");
        }


        #endregion

    }
}
