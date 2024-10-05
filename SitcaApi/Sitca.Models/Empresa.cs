using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Spatial;

using System.Text;

namespace Sitca.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }
        [StringLength(200)]
        public string Nombre { get; set; }


        [StringLength(150)]
        public string NombreRepresentante { get; set; }

        [StringLength(60)]
        public string CargoRepresentante { get; set; }

        [StringLength(50)]
        public string Ciudad { get; set; }

        [StringLength(15)]
        public string IdNacional { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(150)]
        public string Calle { get; set; }
        [StringLength(60)]
        public string Numero { get; set; }
        [StringLength(150)]
        public string Direccion { get; set; }
        [StringLength(20)]
        public string Longitud { get; set; }
        [StringLength(20)]
        public string Latitud { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(100)]
        public string WebSite { get; set; }

        //public NetTopologySuite.Geometries.Point Location { get; set; }

        //public Geography Location { get; set; }
        [Required]
        public int IdPais { get; set; }
        [Required]
        public bool Active { get; set; }

        
        public decimal? Estado { get; set; }

        public int? PaisId { get; set; }

        public string ResultadoSugerido { get; set; }

        public string ResultadoActual { get; set; }

        public DateTime? ResultadoVencimiento { get; set; }

        public bool EsHomologacion { get; set; }

        public Pais Pais { get; set; }

        public DateTime? FechaAutoNotif { get; set; }

        public ICollection<TipologiasEmpresa> Tipologias { get; set; }

        public ICollection<Archivo> Archivos { get; set; }

        public ICollection<ProcesoCertificacion> Certificaciones { get; set; }

        public ICollection<Homologacion> Homologaciones { get; set; }

    }
}
