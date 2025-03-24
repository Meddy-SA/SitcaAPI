using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Services.Notification;

namespace Sitca.Extensions;

/// <summary>
/// Clase de extensiones para manejo de notificaciones asíncronas
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Envía una notificación de forma asíncrona sin bloquear el hilo principal.
    /// Este método utiliza el patrón "fire and forget" para no retrasar la respuesta al cliente.
    /// </summary>
    /// <param name="notificationService">Servicio de notificaciones</param>
    /// <param name="procesoId">ID del proceso a notificar</param>
    /// <param name="notificationType">Tipo de notificación (opcional)</param>
    /// <param name="language">Idioma para la notificación</param>
    /// <param name="logger">Logger para registrar errores</param>
    /// <param name="additionalInfo">Información adicional para el registro en caso de error (opcional)</param>
    public static void SendNotificationProcessAsync(
        this INotificationService notificationService,
        int procesoId,
        string language,
        ILogger logger,
        int? status = null,
        string additionalInfo = null
    )
    {
        // Iniciamos una tarea sin esperar su finalización (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await notificationService.SendNotification(procesoId, status, language);
            }
            catch (Exception ex)
            {
                // Registrar el error sin interrumpir el flujo principal
                var errorMessage = string.IsNullOrEmpty(additionalInfo)
                    ? $"Error al enviar notificación para el proceso {procesoId}"
                    : $"Error al enviar notificación para el proceso {procesoId}. {additionalInfo}";

                logger.LogError(ex, errorMessage);
            }
        });
    }
}
