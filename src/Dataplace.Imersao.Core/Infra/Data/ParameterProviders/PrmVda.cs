using Dataplace.Core.Infra.CrossCutting.Configuration.ParameterProvider;
using Dataplace.Imersao.Core.Infra.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Infra.Data.ParameterProviders
{
    public class PrmVda : ConfigurationParameterBase, IPrmVda
    {
        public PrmVda() : base("prmvda", true)
        {
        }

        public string CdTabela => GetParameter(nameof(CdTabela)).ToString();
        public short SqTabela => Convert.ToInt16(GetParameter(nameof(SqTabela)));
        public string CdCliente => GetParameter(nameof(CdCliente)).ToString();
    }
}
