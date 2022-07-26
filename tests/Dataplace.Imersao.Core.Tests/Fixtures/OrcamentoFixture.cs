using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;
using Dataplace.Imersao.Core.Domain.Orcamentos;
using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using System;

namespace Dataplace.Imersao.Core.Tests.Fixtures
{
    public class OrcamentoFixture
    {
        internal string CdEmpresa = "IMS";
        internal string CdFilial = "01";
        internal OrcamentoCliente Cliente = new OrcamentoCliente("CLI01");
        internal OrcamentoVendedor Vendedor = new OrcamentoVendedor("VDD01");
        internal OrcamentoUsuario Usuario = new OrcamentoUsuario("sym_usuario");
        internal OrcamentoTabelaPreco TabelaPreco = new OrcamentoTabelaPreco("2022", 1);
        internal OrcamentoProduto Produto = new OrcamentoProduto(TpRegistroEnum.ProdutoFinal, "001");


        public Orcamento NovoOrcamentoValido()
        {
            return Orcamento.Factory.NovoOrcamento(
                CdEmpresa,
                CdFilial,
                Cliente,
                Usuario,
                Vendedor,
                TabelaPreco);
        }

        public Orcamento NovoOrcamentoInvalido()
        {
            return Orcamento.Factory.NovoOrcamento(
                string.Empty,
                string.Empty,
                null,
                null,
                null,
                null);
        }

        //viewModels
        public OrcamentoViewModel GerarOrcamentoViewModelValido(int numero, decimal valorTotal)
        {
            return new OrcamentoViewModel
            {
                NumOrcamento = numero,
                CdCliente = Cliente.Codigo,
                DtOrcamento = DateTime.Now,
                ValorTotal = valorTotal,
                DiasValidade = null,
                DataValidade = null,
                CdTabela = TabelaPreco.CdTabela,
                SqTabela = TabelaPreco.SqTabela,
                DtFechamento = null,
                CdVendedor = Vendedor.Codigo,
                Usuario = Usuario.UserName,
                Situacao = OrcamentoStatusEnum.Aberto.ToDataValue(),
            };
        }

        public OrcamentoViewModel GerarOrcamentoViewModelSemCamposOpcionais(int numero, decimal valorTotal)
        {
            return new OrcamentoViewModel
            {
                NumOrcamento = numero,
                CdCliente = string.Empty,
                DtOrcamento = DateTime.MinValue,
                ValorTotal = 0,
                DiasValidade = null,
                DataValidade = null,
                CdTabela = string.Empty,
                SqTabela = 0,
                DtFechamento = null,
                CdVendedor = string.Empty,
                Usuario = string.Empty,
                Situacao = string.Empty,
            };
        }

    }
}
