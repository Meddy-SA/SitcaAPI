namespace Utilities.Common;

public static class AuthorizationPolicies
{
    public static class Empresa
    {
        public const string View = Constants.Roles.Empresa;
        public const string AdminTecnico = $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}";
        public const string AdmTecCons = $"{Constants.Roles.Admin}, {Constants.Roles.TecnicoPais}, {Constants.Roles.Consultor}";
        public const string Details = $"{Constants.Roles.TecnicoPais}, {Constants.Roles.Admin}, {Constants.Roles.Asesor}, {Constants.Roles.Auditor}, {Constants.Roles.CTC}, {Constants.Roles.Consultor}";
    }
}
