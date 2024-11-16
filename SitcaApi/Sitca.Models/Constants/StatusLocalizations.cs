using Sitca.Models.Enums;

namespace Sitca.Models.Constants;

public static class StatusLocalizations
{
  private static readonly Dictionary<CertificationStatus, (string Spanish, string English)> _statusDescriptions = new()
  {
    [CertificationStatus.Initial] = ("Inicial", "Initial"),
    [CertificationStatus.ToBeAdvised] = ("Para Asesorar", "To be Advised"),
    [CertificationStatus.AdvisingInProcess] = ("Asesoría en Proceso", "In Advising Process"),
    [CertificationStatus.AdvisingFinalized] = ("Asesoría Finalizada", "Advising Finalized"),
    [CertificationStatus.ToBeAudited] = ("Para Auditar", "To be Audited"),
    [CertificationStatus.AuditingInProcess] = ("Auditoría en Proceso", "In Auditing Process"),
    [CertificationStatus.AuditingFinalized] = ("Auditoría Finalizada", "Auditing Finalized"),
    [CertificationStatus.UnderCTCReview] = ("En revisión de CTC", "Under CTC Review"),
    [CertificationStatus.Ended] = ("Finalizado", "Ended")
  };

  public static string GetDescription(CertificationStatus status, string lang) =>
        lang == LanguageCodes.Spanish
            ? _statusDescriptions[status].Spanish
            : _statusDescriptions[status].English;
}
