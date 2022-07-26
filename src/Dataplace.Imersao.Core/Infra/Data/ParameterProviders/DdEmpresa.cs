using Dataplace.Core.Infra.CrossCutting.Configuration.ParameterProvider;
using Dataplace.Imersao.Core.Infra.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Infra.Data.ParameterProviders
{
    public class DdEmpresa : ConfigurationParameterBase, IDdEmpresa
    {
        public DdEmpresa() : base("ddempresa", true) { }
        public string CdEmpresa => GetParameter(nameof(CdEmpresa)).ToString();
        public string CdFilial => GetParameter(nameof(CdFilial)).ToString();   
    }
}
