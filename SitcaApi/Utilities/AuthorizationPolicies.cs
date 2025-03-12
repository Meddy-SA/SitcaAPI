using Rol = Utilities.Common.Constants.Roles;

namespace Utilities.Common;

public static class AuthorizationPolicies
{
    public static class Auth
    {
        public const string CreateUser = $"{Rol.Admin}, {Rol.TecnicoPais}";
    }

    public static class Capacitaciones
    {
        public const string DeleteFiles = $"{Rol.Admin}, {Rol.TecnicoPais}";
    }

    public static class Certificaciones
    {
        public const string ConvertirARecetificacion = $"{Rol.Admin}, {Rol.TecnicoPais}";
        public const string Comenzar = $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.EmpresaAuditora}";
        public const string AsignaAuditor = $"{Rol.TecnicoPais},{Rol.EmpresaAuditora},{Rol.Admin}";
        public const string UpdateAuditor = $"{Rol.TecnicoPais},{Rol.EmpresaAuditora},{Rol.Admin}";
        public const string GenerarCuestionario =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.Auditor}, {Rol.Asesor}";
    }

    public static class Proceso
    {
        public const string UpdateCaseNumber =
            $"{Rol.Admin}, {Rol.TecnicoPais}, {Rol.EmpresaAuditora}";
        public const string SaveCalification = $"{Rol.Admin}, {Rol.CTC}";
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
}
