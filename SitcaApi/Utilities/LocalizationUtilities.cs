namespace Utilities.Common;

public static class LocalizationUtilities
{
  private static readonly Dictionary<int, string> Countries = new()
    {
        { 1, "Belize" },
        { 2, "Guatemala" },
        { 3, "El Salvador" },
        { 4, "Honduras" },
        { 5, "Nicaragua" },
        { 6, "Costa Rica" },
        { 7, "Panama" },
        { 8, "Republica Dominicana" }
    };

  private static readonly Dictionary<string, (string Es, string En)> RoleTranslations = new()
    {
        { Constants.Roles.Asesor, ("Asesor", "Consultant") },
        { Constants.Roles.TecnicoPais, ("Técnico País", "Country Technician") },
        { Constants.Roles.Empresa, ("Empresa", "Company") },
        { Constants.Roles.CTC, ("CTC", "CCT") }
    };

  private static readonly Dictionary<int, (string Es, string En)> StatusTranslations = new()
    {
        { Constants.ProcessStatus.Initial, ("0 - Inicial", "0 - Start") },
        { Constants.ProcessStatus.ForConsulting, ("1 - Para Asesorar", "1 - For consulting") },
        { Constants.ProcessStatus.ConsultancyUnderway, ("2 - Asesoria en Proceso", "2 - Consultancy underway") },
        { Constants.ProcessStatus.ConsultancyCompleted, ("3 - Asesoria Finalizada", "3 - Consultancy completed") },
        { Constants.ProcessStatus.ForAuditing, ("4 - Para Auditar", "4 - For auditing") },
        { Constants.ProcessStatus.AuditingUnderway, ("5 - Auditoria en Proceso", "5 - Auditing underway") },
        { Constants.ProcessStatus.AuditCompleted, ("6 - Auditoria Finalizada", "6 - Audit completed") },
        { Constants.ProcessStatus.UnderCCTReview, ("7 - En revisión de CTC", "7 - Under CCT Review") },
        { Constants.ProcessStatus.Completed, ("8 - Finalizado", "8 - Completed") }
    };

  public static string? GetCountry(int countryId) =>
      Countries.GetValueOrDefault(countryId);

  public static string RoleToText(IList<string> roles, string lang)
  {
    if (!roles.Any()) return string.Empty;

    var translatedRoles = roles.Select(role =>
    {
      if (RoleTranslations.TryGetValue(role, out var translation))
        return lang == "es" ? translation.Es : translation.En;
      return role;
    });

    return string.Join("/", translatedRoles);
  }

  public static List<string> TranslateRoles(IList<string> roles, string lang) =>
      roles.Select(role =>
      {
        if (RoleTranslations.TryGetValue(role, out var translation))
          return lang == "es" ? translation.Es : translation.En;
        return role;
      }).ToList();

  public static string GetStatus(decimal? status, string lang)
  {
    if (!status.HasValue) return string.Empty;

    if (StatusTranslations.TryGetValue((int)status.Value, out var translation))
      return lang == "es" ? translation.Es : translation.En;

    return string.Empty;
  }
}
