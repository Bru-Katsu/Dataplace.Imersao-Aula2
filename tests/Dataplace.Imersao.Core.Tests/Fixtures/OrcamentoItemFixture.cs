using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;
using Dataplace.Imersao.Core.Domain.Orcamentos;
using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Tests.Fixtures
{
    public class OrcamentoItemFixture
    {
        internal string CdEmpresa = "IMS";
        internal string CdFilial = "01";
        internal OrcamentoProduto Produto = new OrcamentoProduto(TpRegistroEnum.ProdutoFinal, "001");
        internal OrcamentoItemPreco Preco = new OrcamentoItemPrecoPercentual(10, 0);
        public OrcamentoItem NovoOrcamentoItemValido(int numOrcamento, int quantidade)
        {
            return new OrcamentoItem(CdEmpresa, CdFilial, numOrcamento, Produto, quantidade, Preco);
        }

        public OrcamentoItemViewModel GerarOrcamentoItemViewModelValido(int numero, int seq, int quantidade, decimal precoTabela, decimal precAltPreco, decimal precoVenda)
        {
            return new OrcamentoItemViewModel
            {
                NumOrcamento = numero,
                CdEmpresa = CdEmpresa,
                CdFilial = CdFilial,
                Seq = seq,
                TpRegistro = Produto.TpProduto.ToDataValue(),
                CdProduto = Produto.CdProduto,
                DsProduto = "Produto Teste",
                Quantidade = quantidade,
                PrecoTabela = precoTabela,
                PercAltPreco = precAltPreco,
                PrecoVenda = precoVenda,
                Total = quantidade * precoVenda,
                Status = OrcamentoItemStatusEnum.Aberto.ToDataValue(),
            };
        }

        public OrcamentoItemViewModel GerarOrcamentoItemViewModelInvalido(int numero, int seq)
        {
            return new OrcamentoItemViewModel
            {
                NumOrcamento = numero,
                CdEmpresa = CdEmpresa,
                CdFilial = CdFilial,
                Seq = seq,
                TpRegistro = string.Empty,
                CdProduto = string.Empty,
                DsProduto = string.Empty,
                Quantidade = -1,
                PrecoTabela = 0,
                PercAltPreco = 0,
                PrecoVenda = 0,
                Total = 0,
                Status = string.Empty,
            };
        }
    }
}
