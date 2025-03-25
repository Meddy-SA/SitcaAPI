using System.ComponentModel;

namespace Sitca.DataAccess.Data.Repository.Constants;

public enum NotificationTypes
{
    [Description("Se ha registrado una nueva empresa")]
    NuevaEmpresa = -1,

    [Description("Protocolo de Adhision de empresa")]
    [NotificationType(
        new[] { NotificationConstants.NotificationNames.Protocol.Es },
        new[] { NotificationConstants.NotificationNames.Protocol.En }
    )]
    ProtocoloAdhision = -2,

    [NotificationType(
        new[]
        {
            NotificationConstants.NotificationNames.Certification.Es1,
            NotificationConstants.NotificationNames.Certification.Es2,
        },
        new[] { NotificationConstants.NotificationNames.Certification.En }
    )]
    [Description("Se solicito auditoria de empresa")]
    SolicitudAuditoria = -3,

    [Description("Cambio de asesor")]
    CambioAsesor = -4,

    [Description("Cambio de auditor")]
    CambioAuditor = -5,

    [Description("El destintivo de la empresa vence en 6 meses")]
    Expiracion = -10,

    [NotificationType(
        new[] { NotificationConstants.NotificationNames.Recommendation.Es },
        new[] { NotificationConstants.NotificationNames.Recommendation.En }
    )]
    [Description("En espera de analisis de CTC")]
    AnalisisCTC = -11,

    [NotificationType(
        new[]
        {
            NotificationConstants.NotificationNames.Recertification.Es1,
            NotificationConstants.NotificationNames.Recertification.Es2,
        },
        new[] { NotificationConstants.NotificationNames.Recertification.En }
    )]
    [Description("Empresa solicita recertificacion")]
    SolicitudReCertificacion = -12,

    [Description("Se ha creado nuevo expediente por Homologacion")]
    ExpedienteHomologacion = -13,

    [Description("Nueva Homologacion")]
    Homologacion = -14,
}
