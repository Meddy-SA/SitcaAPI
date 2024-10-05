using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    public class DatosCapacitacion
    {
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string tipo { get; set; }
        public string url { get; set; }
    }

    public class ArchivoVm
    {
        public int Id { get; set; }
        public string  Ruta { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string FechaCarga { get; set; }
        public string Cargador { get; set; }

        public bool Propio { get; set; }
    }

    public class ArchivoFilterVm
    {
        public int? idPregunta { get; set; }
        public int? idCuestionario { get; set; }
        public string type { get; set; }        
    }
}
