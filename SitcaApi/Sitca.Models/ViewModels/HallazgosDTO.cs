using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    public class RegistroHallazgos
    {
        public string Empresa { get; set; }
        public string Generador { get; set; }

        public string Language { get; set; }

        public string RutaPdf { get; set; }
        public List<HallazgosDTO> HallazgosItems { get; set; }
    }
    public class HallazgosDTO
    {
        public string Modulo { get; set; }
        public string Referencia { get; set; }
        public Version ReferenciaOrden { get; set; }
        public string Descripcion { get; set; }
        public string Obligatorio { get; set; }
    }
}
