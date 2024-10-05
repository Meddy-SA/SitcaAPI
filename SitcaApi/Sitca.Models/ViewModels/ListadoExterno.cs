using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    public class ListadoExterno
    {
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public string Id { get; set; }        
        public string Tipologia { get; set; }
    }
    public class ListadoExternoFiltro
    {
        public string Pais { get; set; }
    }

    public class ResponseListadoExterno
    {
        public bool Success { get; set; }

        public List<ListadoExterno> Data { get; set; }

        public string Message { get; set; }
    }


}
