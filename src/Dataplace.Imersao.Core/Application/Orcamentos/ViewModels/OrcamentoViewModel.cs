using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Application.Orcamentos.ViewModels
{
    public class OrcamentoViewModel
    {
        //public string CdEmpresa { get; set; }
        //public string CdFilial { get; set; }
        public int NumOrcamento { get; set; }
        public string CdCliente { get; set; }
        public DateTime DtOrcamento { get; set; }
        public Decimal ValorTotal { get; set; }
        public int? DiasValidade { get; set; }
        public DateTime? DataValidade { get; set; }
        public string CdTabela { get; set; }
        public short? SqTabela { get; set; }
        public DateTime? DtFechamento { get; set; }
        public string CdVendedor { get; set; }
        public string Usuario { get; set; }
        public string Situacao { get; set; }
    }
}
