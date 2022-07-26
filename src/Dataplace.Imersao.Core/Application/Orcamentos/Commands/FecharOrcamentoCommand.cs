using Dataplace.Core.Domain.Commands;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Commands
{
    public class FecharOrcamentoCommand : Command
    {
        public FecharOrcamentoCommand(int numOcamento)
        {
            NumOcamento = numOcamento;
        }

        public int NumOcamento { get; }
    }
}
