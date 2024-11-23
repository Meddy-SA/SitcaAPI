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
      { Constants.Roles.Auditor, ("Auditor", "Auditor") },
      { Constants.Roles.TecnicoPais, ("TecnicoPais", "Country Technician") },
      { Constants.Roles.Empresa, ("Empresa", "Company") },
      { Constants.Roles.CTC, ("CTC", "CCT") },
      { Constants.Roles.AsesorAuditor, ("Asesor/Auditor", "Consultant/Auditor") },
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

  public static int? GetStatusIdExact(string statusText, string lang)
  {
    if (string.IsNullOrWhiteSpace(statusText))
      return null;

    return StatusTranslations
        .FirstOrDefault(x =>
            (lang == "es" && x.Value.Es.Equals(statusText, StringComparison.OrdinalIgnoreCase)) ||
            (lang == "en" && x.Value.En.Equals(statusText, StringComparison.OrdinalIgnoreCase)))
        .Key;
  }

  public static int? GetStatusId(string statusText, string lang)
  {
    if (string.IsNullOrWhiteSpace(statusText))
      return null;

    // Normalizar el texto de búsqueda removiendo números y guiones al inicio
    var normalizedSearchText = statusText.Trim()
        .Replace("- ", "")
        .TrimStart("0123456789 -".ToCharArray())
        .Trim();

    return StatusTranslations
        .FirstOrDefault(x =>
            (lang == "es" && x.Value.Es.EndsWith(normalizedSearchText, StringComparison.OrdinalIgnoreCase)) ||
            (lang == "en" && x.Value.En.EndsWith(normalizedSearchText, StringComparison.OrdinalIgnoreCase)))
        .Key;
  }

  /// <summary>
  /// Comprueba si un rol coincide con cualquiera de sus traducciones
  /// </summary>
  /// <param name="roleToCheck">Rol a comprobar</param>
  /// <param name="constantRole">Constante del rol (desde Constants.Roles)</param>
  /// <returns>True si coincide con alguna traducción, False en caso contrario</returns>
  public static bool CompareInAllLanguages(string roleToCheck, string constantRole)
  {
    if (string.IsNullOrEmpty(roleToCheck) || string.IsNullOrEmpty(constantRole))
      return false;

    // Verificar coincidencia directa con la constante
    if (roleToCheck.Equals(constantRole, StringComparison.OrdinalIgnoreCase))
      return true;

    // Verificar coincidencia con las traducciones
    if (LocalizationUtilities.RoleTranslations.TryGetValue(constantRole, out var translation))
    {
      return roleToCheck.Equals(translation.Es, StringComparison.OrdinalIgnoreCase) ||
             roleToCheck.Equals(translation.En, StringComparison.OrdinalIgnoreCase);
    }

    return false;
  }
}
