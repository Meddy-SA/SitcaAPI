namespace Sitca.DataAccess.Helpers;

public static class ErrorMessages
{
    public static class ProcesoCertification
    {
        public static string ActiveProcessExists(string language, int empresaId)
        {
            return language?.ToLower() == "en"
                ? $"The company with ID {empresaId} already has an active certification process. A recertification cannot be started until all processes are completed."
                : $"La empresa con ID {empresaId} ya tiene un proceso de certificación activo. No se puede iniciar una recertificación hasta que todos los procesos estén finalizados.";
        }
    }
}
