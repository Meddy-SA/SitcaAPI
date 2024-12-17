namespace Sitca.Models.Enums;

public enum FileCompany
{
  [MultiLanguageDisplay("Informativo", "Informative")]
  Informativo,

  [MultiLanguageDisplay("Protocolo de Adhesión", "Adhesion Protocol")]
  Adhesion,

  [MultiLanguageDisplay("Recomendación del Auditor al CTC", "Auditor Recommendation to CCT")]
  AuditoraCTC,

  [MultiLanguageDisplay("Compromiso de Confidencialidad", "Confidentiality Agreement")]
  ComprosimoConfidencialidad,

  [MultiLanguageDisplay("Declaración Jurada", "Sworn Statement")]
  DeclaracionJurada,

  [MultiLanguageDisplay("Solicitud de Certificación", "Certification Request")]
  SolicitudCertificacion,

  [MultiLanguageDisplay("Solicitud de Recertificación", "Recertification Request")]
  SolicitudRecertificacion,

  [MultiLanguageDisplay("Documento de Identidad", "National Id Card")]
  DocumentoIdentidad,

  [MultiLanguageDisplay("Licencia de Operación", "Operating License")]
  LicenciaOperacion,

  [MultiLanguageDisplay("Informe de Asesoría", "Consultancy Report")]
  InformeAsesoria
}
