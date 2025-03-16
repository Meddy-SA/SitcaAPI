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
        public const string Consultor = "Consultor";
        public const string EmpresaAuditora = "EmpresaAuditora";
    }

    public static class Distintivos
    {
        public const string Rojo = "Distintivo Rojo";
        public const string Azul = "Distintivo Azul";
        public const string Verde = "Distintivo Verde";
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

    public static class ProcessStatusDecimal
    {
        public const decimal Initial = ProcessStatus.Initial;
        public const decimal ForConsulting = ProcessStatus.ForConsulting;
        public const decimal ConsultancyUnderway = ProcessStatus.ConsultancyUnderway;
        public const decimal ConsultancyCompleted = ProcessStatus.ConsultancyCompleted;
        public const decimal ForAuditing = ProcessStatus.ForAuditing;
        public const decimal AuditingUnderway = ProcessStatus.AuditingUnderway;
        public const decimal AuditCompleted = ProcessStatus.AuditCompleted;
        public const decimal UnderCCTReview = ProcessStatus.UnderCCTReview;
        public const decimal Completed = ProcessStatus.Completed;
    }

    public static readonly string[] KnowRoles =
    {
        Roles.Asesor,
        Roles.TecnicoPais,
        Roles.Empresa,
        Roles.CTC,
        Roles.Admin,
        Roles.Auditor,
        Roles.AsesorAuditor,
        Roles.Consultor,
        Roles.EmpresaAuditora,
    };

    public static class ProcessStatusText
    {
        // Textos en español
        public static class Spanish
        {
            public const string Initial = "0 - Inicial";
            public const string ForConsulting = "1 - Para Asesorar";
            public const string ConsultancyUnderway = "2 - Asesoria en Proceso";
            public const string ConsultancyCompleted = "3 - Asesoria Finalizada";
            public const string ForAuditing = "4 - Para Auditar";
            public const string AuditingUnderway = "5 - Auditoria en Proceso";
            public const string AuditCompleted = "6 - Auditoria Finalizada";
            public const string UnderCCTReview = "7 - En revisión de CTC";
            public const string Completed = "8 - Finalizado";

            // También podemos tener versiones sin el número para búsquedas
            public const string InitialNoNumber = "Inicial";
            public const string ForConsultingNoNumber = "Para Asesorar";
            public const string ConsultancyUnderwayNoNumber = "Asesoria en Proceso";
            public const string ConsultancyCompletedNoNumber = "Asesoria Finalizada";
            public const string ForAuditingNoNumber = "Para Auditar";
            public const string AuditingUnderwayNoNumber = "Auditoria en Proceso";
            public const string AuditCompletedNoNumber = "Auditoria Finalizada";
            public const string UnderCCTReviewNoNumber = "En revisión de CTC";
            public const string CompletedNoNumber = "Finalizado";
        }

        // Textos en inglés
        public static class English
        {
            public const string Initial = "0 - Start";
            public const string ForConsulting = "1 - For consulting";
            public const string ConsultancyUnderway = "2 - Consultancy underway";
            public const string ConsultancyCompleted = "3 - Consultancy completed";
            public const string ForAuditing = "4 - For auditing";
            public const string AuditingUnderway = "5 - Auditing underway";
            public const string AuditCompleted = "6 - Audit completed";
            public const string UnderCCTReview = "7 - Under CCT Review";
            public const string Completed = "8 - Completed";

            // También versiones sin el número
            public const string InitialNoNumber = "Start";
            public const string ForConsultingNoNumber = "For consulting";
            public const string ConsultancyUnderwayNoNumber = "Consultancy underway";
            public const string ConsultancyCompletedNoNumber = "Consultancy completed";
            public const string ForAuditingNoNumber = "For auditing";
            public const string AuditingUnderwayNoNumber = "Auditing underway";
            public const string AuditCompletedNoNumber = "Audit completed";
            public const string UnderCCTReviewNoNumber = "Under CCT Review";
            public const string CompletedNoNumber = "Completed";
        }
    }
}
