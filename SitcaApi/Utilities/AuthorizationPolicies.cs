using Rol = Utilities.Common.Constants.Roles;

namespace Utilities.Common;

public static class AuthorizationPolicies
{
    public static class Auth
    {
        public const string CreateUser = $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.ATP}";
    }

    public static class Capacitaciones
    {
        public const string DeleteFiles = $"{Rol.Admin}, {Rol.TecnicoPais}";
    }

    public static class Certificaciones
    {
        public const string ConvertirARecetificacion = $"{Rol.Admin}, {Rol.TecnicoPais}";
        public const string Comenzar = $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.EmpresaAuditora}";
        public const string UpdateAuditor = $"{Rol.TecnicoPais},{Rol.EmpresaAuditora},{Rol.Admin}";
        public const string GenerarCuestionario =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.Auditor}, {Rol.Asesor}";
    }

    public static class Proceso
    {
        public const string UpdateCaseNumber =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.EmpresaAuditora}";
        public const string SaveCalification = $"{Rol.Admin}, {Rol.CTC}";
        public const string CreateRecertification = $"{Rol.Admin}, {Rol.TecnicoPais}";
        public const string StartedConsulting =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.EmpresaAuditora}, {Rol.ATP}";
        public const string AssignAuditor = $"{Rol.TecnicoPais},{Rol.EmpresaAuditora},{Rol.Admin}";
    }

    public static class Empresa
    {
        public const string View = Rol.Empresa;
        public const string AdminTecnico = $"{Rol.Admin}, {Rol.TecnicoPais}";
        public const string ListCompany =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.Consultor}, {Rol.EmpresaAuditora}";
        public const string Details =
            $"{Rol.TecnicoPais}, {Rol.Admin}, {Rol.Asesor}, {Rol.Auditor}, {Rol.CTC}, {Rol.Consultor}, {Rol.EmpresaAuditora}";
    }

    public static class CrossCountryAudit
    {
        public const string Manage = $"{Rol.Admin}, {Rol.TecnicoPais}";
    }
}
