using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sitca.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [StringLength(150)]
        public string TituloInterno { get; set; }
        [StringLength(150)]
        public string TituloParaEmpresa { get; set; }

        public int Status { get; set; }

        [StringLength(200)]
        public string TextoInterno { get; set; }
        [StringLength(200)]
        public string TextoParaEmpresa { get; set; }

        public int Pais { get; set; }





        [StringLength(150)]
        public string TituloInternoEn { get; set; }

        [StringLength(150)]
        public string TituloParaEmpresaEn { get; set; }

        [StringLength(200)]
        public string TextoInternoEn { get; set; }
        [StringLength(200)]
        public string TextoParaEmpresaEn { get; set; }



        public ICollection<NotificationGroups> NotificationGroups { get; set; }
    }
}
