using Dataplace.Core.Domain.Query;
using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;


namespace Dataplace.Imersao.Core.Application.Orcamentos.Queries
{
    public class ObterOrcamentoItemQuery : QueryRefeshItem<OrcamentoItemViewModel>, IQueryRefeshItem<OrcamentoItemViewModel>
    {
        public int NumOrcamento { get; set; }
        public int Seq { get; set; }
    }
}
