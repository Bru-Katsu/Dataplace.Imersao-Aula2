using Dataplace.Core.Domain.Events;
using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Events
{
    public class OrcamentoItemAdicionadoEvent : OrcamentoItemEventBase
    {
        public OrcamentoItemAdicionadoEvent(OrcamentoItemViewModel item) : base(item) { }
    }
}
