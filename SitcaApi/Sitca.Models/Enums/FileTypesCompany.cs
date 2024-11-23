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
}
