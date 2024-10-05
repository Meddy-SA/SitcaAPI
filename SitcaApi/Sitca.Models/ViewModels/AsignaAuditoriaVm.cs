using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    public class AsignaAuditoriaVm
    {
        public string AuditorId { get; set; }
        public string Fecha { get; set; }
        public int TipologiaId { get; set; }
        public int EmpresaId { get; set; }
    }
}