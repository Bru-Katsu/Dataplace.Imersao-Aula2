using System.Collections.Generic;

namespace Dataplace.Imersao.Core.Domain.Orcamentos.Repositories
{
    public interface IOrcamentoItemRepository
    {

        // itens
        OrcamentoItem AdicionarItem(OrcamentoItem entity);
        bool AtualizarItem(OrcamentoItem entity);
        bool ExcluirItem(OrcamentoItem entity);
        OrcamentoItem ObterItem(string cdEmpresa, string cdFilail, int numOrcamento, int seq);
        IEnumerable<OrcamentoItem> ObterItens(string cdEmpresa, string cdFilail, int numOrcamento);
    }

    
}
