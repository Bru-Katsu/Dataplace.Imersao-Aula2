using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Infra.Configurations
{
    public interface IPrmVda
    {
        string CdTabela { get; }
        short SqTabela { get; }
        string CdCliente { get; }
    }
}
