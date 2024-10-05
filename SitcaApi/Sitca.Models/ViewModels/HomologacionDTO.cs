using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public  class HomologacionDTO
    {
        public int id { get; set; }
        public int empresaId { get; set; }
        public string nombre { get; set; }
        public Tipologia tipologia { get; set; }
        public string datosProceso { get; set; }
        public SelloItc selloItc { get; set; }
        public CommonVm distintivoSiccs { get; set; }
        public DateTime fechaOtorgamiento { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public List<ArchivoVm> archivos { get; set; }
        public bool enProceso { get; set; }
    }

    [NotMapped]
    public class SelloItc
    {
        public string name { get; set; }
    }

}
