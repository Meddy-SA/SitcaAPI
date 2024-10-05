using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models
{
    public class TipologiasEmpresa
    {
        public int IdEmpresa { get; set; }
        public Empresa Empresa { get; set; }

        public int IdTipologia { get; set; }
        public Tipologia Tipologia { get; set; }
        

    }
}
