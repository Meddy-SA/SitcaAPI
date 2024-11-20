using System.ComponentModel.DataAnnotations.Schema;

namespace Sitca.Models.ViewModels
{
  [NotMapped]
  public class CertificacionVm
  {
    public int Id { get; set; }
    public string Asesor { get; set; } = null!;

    public int EmpresaId { get; set; }

    public int Empresa { get; set; }
  }

  public class CertificacionStatusVm
  {
    public int CertificacionId { get; set; }
    public string Status { get; set; } = null!;
  }

  public class SaveCalificacionVm
  {
    public int idProceso { get; set; }
    public int? distintivoId { get; set; }
    public string Observaciones { get; set; } = null!;
    public string Dictamen { get; set; } = null!;
    public bool aprobado { get; set; }
  }

  public class CambioAuditor
  {
    public int idProceso { get; set; }
    public string userId { get; set; } = null!;
    public bool auditor { get; set; }
    public string motivo { get; set; } = null!;
  }

  public class CertificacionDetailsVm
  {
    public int Id { get; set; }

    public EmpresaVm Empresa { get; set; } = null!;
    public CommonUserVm Asesor { get; set; } = null!;
    public CommonUserVm Auditor { get; set; } = null!;
    public CommonUserVm Generador { get; set; } = null!;

    public bool Recertificacion { get; set; }
    public string FechaInicio { get; set; } = null!;
    public string FechaFin { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string TipologiaName { get; set; } = null!;
    public string Resultado { get; set; } = null!;
    public string FechaVencimiento { get; set; } = null!;
    public string Expediente { get; set; } = null!;

    public bool alertaVencimiento { get; set; }
  }

  [NotMapped]
  public class CuestionarioCreateVm
  {
    public int Id { get; set; }

    public int TipologiaId { get; set; }
    public int EmpresaId { get; set; }
    public int CertificacionId { get; set; }
    public string AsesorId { get; set; } = null!;
    public string? AuditorId { get; set; }

    public bool Prueba { get; set; }
  }

  public class CuestionarioDetailsMinVm
  {
    public int Id { get; set; }

    public CommonVm Tipologia { get; set; } = null!;
    public CommonVm Empresa { get; set; } = null!;
    public string FechaInicio { get; set; } = null!;
    public string FechaFin { get; set; } = null!;
    public string? FechaEvaluacion { get; set; }
    public string? FechaRevisionAuditor { get; set; }
    public CommonUserVm Asesor { get; set; } = null!;
    public bool Prueba { get; set; }

    public int IdCertificacion { get; set; }
  }

  [NotMapped]
  public class CuestionarioDetailsVm
  {
    public int Id { get; set; }

    public CommonVm Tipologia { get; set; } = null!;
    public CommonVm Empresa { get; set; } = null!;
    public CommonUserVm? Asesor { get; set; } = null!;
    public CommonUserVm? Auditor { get; set; }
    public bool Prueba { get; set; }

    public List<ModulosVm> Modulos { get; set; } = [];

    public string? path { get; set; }
    public string Expediente { get; set; } = null!;
    public string? FechaFinalizacion { get; set; }
    public string? FechaRevisionAuditor { get; set; }
    public string? TecnicoPaisId { get; set; }
    public bool Recertificacion { get; set; }
    public string? Lang { get; set; }
    public int Pais { get; set; }
  }

  [NotMapped]
  public class CuestionarioNoCumpleVm
  {
    public int Id { get; set; }

    public CommonVm Tipologia { get; set; } = null!;
    public CommonVm Empresa { get; set; } = null!;
    public CommonUserVm Asesor { get; set; } = null!;
    public CommonUserVm Auditor { get; set; } = null!;
    public bool Prueba { get; set; }

    public List<CuestionarioItemVm> Preguntas { get; set; } = [];

    public string path { get; set; } = null!;
    public string Expediente { get; set; } = null!;
    public string FechaFinalizacion { get; set; } = null!;
    public string Lenguage { get; set; } = null!;
  }

  [NotMapped]
  public class CuestionarioItemVm
  {
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public string Nomenclatura { get; set; } = null!;

    public int Order { get; set; }
    public string Type { get; set; } = null!;

    public int? Result { get; set; }

    public bool NoAplica { get; set; }
    public bool Obligatoria { get; set; }

    public int CuestionarioId { get; set; }
    public int? IdRespuesta { get; set; }
    public List<Archivo> Archivos { get; set; } = [];

    public string Observacion { get; set; } = string.Empty;

    public bool TieneArchivos { get; set; }
    public bool TieneObs { get; set; }
  }

  [NotMapped]
  public class HistorialVm
  {
    public string Fecha { get; set; } = null!;
    public int Cantidad { get; set; }
    public int Archivos { get; set; }
    public int Porcentaje { get; set; }
    public int PorcentajeAcumulado { get; set; }
  }
}
