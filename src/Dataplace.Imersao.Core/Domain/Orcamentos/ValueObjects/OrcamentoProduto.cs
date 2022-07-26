using Dataplace.Imersao.Core.Domain.Orcamentos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Domain.Orcamentos.ValueObjects
{
    public class OrcamentoProduto
    {
        protected OrcamentoProduto() { }
        public OrcamentoProduto(TpRegistroEnum tpRegistro, string cdRegistro)
        {
            TpProduto = tpRegistro;
            CdProduto = cdRegistro;
        }

        public TpRegistroEnum TpProduto { get; private set; }
        public string CdProduto { get; private set; }

        internal bool IsValid()
        {
            return ! string.IsNullOrEmpty((CdProduto ?? string.Empty).Trim());
        }

        public override string ToString()
        {
            return $"{CdProduto}";
        }
    }
}
