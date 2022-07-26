using Dataplace.Core.Domain.Entities;
using Dataplace.Core.Domain.Localization.Messages.Extensions;
using Dataplace.Imersao.Core.Domain.Exections;
using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dataplace.Imersao.Core.Domain.Orcamentos
{
    public class Orcamento : Entity<Orcamento>
    {

        #region constructors

 
        // uso orm
        protected Orcamento() { Itens = new List<OrcamentoItem>(); }

        private Orcamento(string cdEmpresa, string cdFilial, OrcamentoCliente cliente,
            OrcamentoUsuario usuario, OrcamentoVendedor vendedor, OrcamentoTabelaPreco tabelaPreco)
        {
       
            CdEmpresa = cdEmpresa;
            CdFilial = cdFilial;
            Cliente = cliente;
            Usuario = usuario;
            Vendedor = vendedor;
            TabelaPreco = tabelaPreco;
          

            // default
            Situacao = OrcamentoStatusEnum.Aberto;
            DtOrcamento = DateTime.Now;
            ValotTotal = 0;

            Itens = new List<OrcamentoItem>();

        }
        #endregion

        #region properties

        public string CdEmpresa { get; private set; }
        public string CdFilial { get; private set; }
        public int NumOrcamento { get; private set; }

        public DateTime DtOrcamento { get; private set; }
        public decimal ValotTotal { get; private set; }
        public DateTime? DtFechamento { get; private set; }
        public OrcamentoStatusEnum Situacao { get; private set; }

        public OrcamentoCliente Cliente { get; private set; }
        public OrcamentoValidade Validade { get; private set; }
        public OrcamentoTabelaPreco TabelaPreco { get; private set; }
        public OrcamentoVendedor Vendedor { get; private set; }
        public OrcamentoUsuario Usuario { get; private set; }

        public ICollection<OrcamentoItem> Itens { get; private set; }
        #endregion

        #region alteração de status
        internal void DefinirItens()
        {

        }

        public OrcamentoItem AdicionarItem(OrcamentoProduto produto, decimal quantidade, OrcamentoItemPreco preco)
        {
            var item = new OrcamentoItem(this.CdEmpresa, this.CdFilial, this.NumOrcamento, produto, quantidade, preco);
            if (!produto.IsValid())
                return default;

            this.Itens.Add(item);
            return item;

        }

        public void RemoverItem(int seq)
        {
            var item = Itens.FirstOrDefault(x => x.Seq == seq);
            if (item == null)
                throw new DomainException("O Item não existe no orçamento!");

            Itens.Remove(item);
        }

        public void FecharOrcamento()
        {
            if (Situacao == OrcamentoStatusEnum.Cancelado)
                throw new DomainException("Não é permitido fechar orçamento cancelado!");

            if (Situacao == OrcamentoStatusEnum.Fechado)
                throw new DomainException("Orçamento já está fechado!");

            Situacao = OrcamentoStatusEnum.Fechado;
            foreach (var item in Itens ?? new List<OrcamentoItem>())
            {
                item.FecharItem();
            }

            DtFechamento = DateTime.Now.Date;
        }

        public void ReabrirOrcamento()
        {

            if (Situacao == OrcamentoStatusEnum.Cancelado)
                throw new DomainException("Não é permitido reabrir orçamento cancelado!");

            if (Situacao == OrcamentoStatusEnum.Aberto)
                throw new DomainException("Orçamento já está Aberto!");

            Situacao = OrcamentoStatusEnum.Aberto;
            foreach (var item in Itens ?? new List<OrcamentoItem>())
            {
                item.ReabrirItem();
            }

            DtFechamento = null; 
        }

        public void CancelarOrcamento()
        {
            if (Situacao == OrcamentoStatusEnum.Fechado)
                throw new DomainException("Não é permitido cancelar orçamento fechado!");

            if (Situacao == OrcamentoStatusEnum.Cancelado)
                throw new DomainException("Orçamento já está Cancelado!");

            Situacao = OrcamentoStatusEnum.Cancelado;
            foreach (var item in Itens ?? new List<OrcamentoItem>())
            {
                item.CancelarItem();
            }

            DtFechamento = null;
        }

        #endregion

        #region settets
        public void DefinirValidade(int validade)
        {
            this.Validade = new OrcamentoValidade(this, validade);
        }
        public void RemoverValidade()
        {
            this.Validade = null;
        }

        public void DefinirUsuario(OrcamentoUsuario usuario)
        {
            this.Usuario = usuario.IsValid() ? usuario : default;
        }

        internal void DefinirVendedor(OrcamentoVendedor vendedor)
        {
            this.Vendedor = vendedor.IsValid() ? vendedor : default;
        }

        internal void RemoverVendedor()
        {
            this.Vendedor = default;
        }
   
        internal void DefinirValidade(OrcamentoValidade validade)
        {
            this.Validade = validade.IsValid() ? validade : default;
        }

        internal void DefinirCliente(OrcamentoCliente cliente)
        {
            this.Cliente = cliente.IsValid() ? cliente : default;
        }

        internal void RemoverCliente()
        {
            this.Cliente = null;
        }

        internal void SetNumOrcamento(int value)
        {
            this.NumOrcamento = value;
        }

        #endregion

        #region validations

        internal bool PermiteAlteracaoItem()
        {
            Validation = new Dataplace.Core.Domain.DomainValidation.FluentValidator.Validation.ValidationContract();
            
            Validation.Requires()
                 .AreEquals(this.Situacao,  OrcamentoStatusEnum.Aberto, nameof(Situacao), "O orcamento deve estar aberto para permitir alteração do itens")
                 .IsNotNull(this.TabelaPreco, nameof(TabelaPreco), "O orcamento deve estar vinculado a uma tabela de preço para permitir alteração do itens");

            return Validation.Valid;
        }

        public override bool IsValid()
        {
            Validation = new Dataplace.Core.Domain.DomainValidation.FluentValidator.Validation.ValidationContract();
            Validation.Requires()
                .IsNotNullOrEmpty(this.CdEmpresa, nameof(CdEmpresa), 18398.ToMessage())
                .IsNotNullOrEmpty(this.CdFilial, nameof(CdFilial), 13065.ToMessage())
                .IsNotNull(this.DtOrcamento, nameof(DtOrcamento), "A Data do orçamento é requirida!")
                .IsNotNull(this.Usuario, nameof(Usuario), "O Usuario é requirido!")
                .IsNotNull(this.TabelaPreco, nameof(TabelaPreco), "A Tabela de preço é requirido!");


            return Validation.Valid;
        }
        #endregion



        #region factory methods
        public static class Factory
        {
            public static Orcamento NovoOrcamento(
                string cdEmpresa, 
                string cdFilial, 
                OrcamentoCliente cliente, 
                OrcamentoUsuario usuario, 
                OrcamentoVendedor vendedor,
                OrcamentoTabelaPreco tabelaPreco)
            {
                return new Orcamento(cdEmpresa, cdFilial, cliente, usuario, vendedor, tabelaPreco);
            }
        }

 

        #endregion


        internal void DefinirTabelaPreco(OrcamentoTabelaPreco tabelaPreco)
        {
            this.TabelaPreco = tabelaPreco.IsValid() ? tabelaPreco : default;
        }

    }
}
