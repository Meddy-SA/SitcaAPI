using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class EmpresaVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public List<string> Tipologias { get; set; }
        public string Responsable { get; set; }

        public string Direccion { get; set; }

        public string IdNacionalRepresentante { get; set; }

        public string Status { get; set; }

        public string Certificacion { get; set; }
        public string Vencimiento { get; set; }

        public bool Recertificacion { get; set; }
    }

    [NotMapped]
    public class EmpresaPersonalVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public List<string> Tipologias { get; set; }
        public string Responsable { get; set; }

        public string Direccion { get; set; }

        public string IdNacionalRepresentante { get; set; }

        public string Status { get; set; }

        public string Certificacion { get; set; }

        public CommonUserVm Asesor { get; set; }
        public CommonUserVm Auditor { get; set; }
        public CommonUserVm TecnicoPais { get; set; }
    }


    [NotMapped]
    public class EmpresaUpdateVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public CommonVm Pais { get; set; }
        public List<CommonVm> Tipologias { get; set; }
        public string Responsable { get; set; }
        public string CargoRepresentante { get; set; }
        public string Telefono { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }

        public string Email { get; set; }
        public string Website { get; set; }

        public string IdNacionalRepresentante { get; set; }

        public string MesHoy { get; set; }

        public string RutaPdf { get; set; }

        public decimal Estado { get; set; }
        public string ResultadoSugerido { get; set; }
        public List<ArchivoVm> Archivos { get; set; }
        public List<CertificacionDetailsVm> Certificaciones { get; set; }
        public CertificacionDetailsVm CertificacionActual { get; set; }

        public string Language { get; set; }

    }

}
