namespace Utilities.Common;

public static class AuthorizationPolicies
{
    public static class Auth
    {
        public const string CreateUser = $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}";
    }

    public static class Capacitaciones
    {
        public const string DeleteFiles = $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}";
    }

    public static class Certificaciones
    {
        public const string Comenzar = $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}";
        public const string AsignaAuditor = Constants.Roles.TecnicoPais;
        public const string GenerarCuestionario =
            $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}, {Constants.Roles.Auditor}, {Constants.Roles.Asesor} ";
    }

    public static class Empresa
    {
        public const string View = Constants.Roles.Empresa;
        public const string AdminTecnico =
            $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}";
        public const string AdmTecCons =
            $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}, {Constants.Roles.Consultor}";
        public const string Details =
            $"{Constants.Roles.TecnicoPais}, {Constants.Roles.Admin}, {Constants.Roles.Asesor}, {Constants.Roles.Auditor}, {Constants.Roles.CTC}, {Constants.Roles.Consultor}";
    }
}
