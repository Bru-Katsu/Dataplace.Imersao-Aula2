﻿using Dataplace.Core.Domain.Events;
using Dataplace.Imersao.Core.Application.Orcamentos.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dataplace.Imersao.Core.Application.Orcamentos.Events
{
    public class OrcamentoItemAtualizadoEvent : OrcamentoItemEventBase
    {
        public OrcamentoItemAtualizadoEvent(OrcamentoItemViewModel item) : base(item) { }
    }
}