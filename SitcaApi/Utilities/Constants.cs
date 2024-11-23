namespace Utilities.Common;

public static class Constants
{
  public static class Roles
  {
    public const string Asesor = "Asesor";
    public const string TecnicoPais = "TecnicoPais";
    public const string Empresa = "Empresa";
    public const string CTC = "CTC";
    public const string Admin = "Admin";
    public const string Auditor = "Auditor";
    public const string AsesorAuditor = "Asesor/Auditor";
  }

  public static class ProcessStatus
  {
    public const int Initial = 0;
    public const int ForConsulting = 1;
    public const int ConsultancyUnderway = 2;
    public const int ConsultancyCompleted = 3;
    public const int ForAuditing = 4;
    public const int AuditingUnderway = 5;
    public const int AuditCompleted = 6;
    public const int UnderCCTReview = 7;
    public const int Completed = 8;
  }
}
