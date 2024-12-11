using System.ComponentModel.DataAnnotations;

namespace Sitca.Models.Enums;

public enum FileCompany
{
  [Display(Name = "Informativo")]
  Informativo,
  [Display(Name = "Protocolo de Adhesión")]
  Adhesion,
  [Display(Name = "Recomendación del Auditor al CTC")]
  AuditoraCTC,
  [Display(Name = "Compromiso de Confidencialidad")]
  ComprosimoConfidencialidad,
  [Display(Name = "Declaración Jurada")]
  DeclaracionJurada,
  [Display(Name = "Solicitud de Certificación")]
  SolicitudCertificacion,
  [Display(Name = "Solicitud de Recertificación")]
  SolicitudRecertificacion,
  [Display(Name = "Documento de Identidad")]
  DocumentoIdentidad,
  [Display(Name = "Licencia de Operación")]
  LicenciaOperacion
}
