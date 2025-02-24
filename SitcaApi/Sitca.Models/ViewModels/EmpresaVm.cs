using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class EmpresaVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Pais { get; set; } = null!;
        public List<string> Tipologias { get; set; } = [];
        public string Responsable { get; set; } = null!;
        public string? Direccion { get; set; } = null!;
        public string? IdNacionalRepresentante { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal? StatusId { get; set; }
        public string Certificacion { get; set; } = null!;
        public string? Vencimiento { get; set; } = null!;
        public string Distintivo { get; set; } = null!;
        public Personnal? Asesor { get; set; }
        public Personnal? Auditor { get; set; }

        public bool Recertificacion { get; set; }
        public bool Activo { get; set; }
        public string? FechaRevision { get; set; }
    }

    [NotMapped]
    public class EmpresaPersonalVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Pais { get; set; } = null!;
        public List<string> Tipologias { get; set; } = [];
        public string Responsable { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string IdNacionalRepresentante { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Certificacion { get; set; } = null!;
        public CommonUserVm Asesor { get; set; } = null!;
        public CommonUserVm Auditor { get; set; } = null!;
        public CommonUserVm TecnicoPais { get; set; } = null!;
    }

    [NotMapped]
    public class EmpresaUpdateVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public CommonVm? Pais { get; set; }
        public List<CommonVm> Tipologias { get; set; } = [];
        public string Responsable { get; set; } = null!;
        public string CargoRepresentante { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Website { get; set; } = null!;
        public string IdNacionalRepresentante { get; set; } = null!;
        public string? MesHoy { get; set; }
        public string? RutaPdf { get; set; }
        public decimal Estado { get; set; }
        public string? ResultadoSugerido { get; set; }
        public List<ArchivoVm> Archivos { get; set; } = [];
        public List<CertificacionDetailsVm> Certificaciones { get; set; } = [];
        public CertificacionDetailsVm? CertificacionActual { get; set; }

        public string? Language { get; set; }
    }

    [NotMapped]
    public class Personnal
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
