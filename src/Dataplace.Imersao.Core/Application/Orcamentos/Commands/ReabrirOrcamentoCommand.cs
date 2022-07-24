using Dataplace.Core.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Commands
{
    public class ReabrirOrcamentoCommand : Command
    {
        public ReabrirOrcamentoCommand(int numOcamento)
        {
            NumOrcamento = numOcamento;
        }

        public int NumOrcamento { get; }
    }
}
