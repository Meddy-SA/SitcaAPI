-- Script 04: Certification Processes
-- This script creates sample certification processes linking advisors with companies

-- Get user IDs for the advisors
DECLARE @AsesorCRId NVARCHAR(450);
DECLARE @AsesorGTId NVARCHAR(450);
DECLARE @AuditorId NVARCHAR(450);

SELECT @AsesorCRId = Id FROM AspNetUsers WHERE Email = 'test.asesor@sitca.test';
SELECT @AsesorGTId = Id FROM AspNetUsers WHERE Email = 'asesor.guatemala@sitca.test';
SELECT @AuditorId = Id FROM AspNetUsers WHERE Email = 'auditor.test@sitca.test';

-- Insert certification processes
SET IDENTITY_INSERT ProcesoCertificacion ON;

INSERT INTO ProcesoCertificacion (
    Id,
    FechaInicio,
    FechaFinalizacion,
    Status,
    NumeroExpediente,
    AsesorId,
    AuditorId,
    EmpresaId,
    TipologiaId,
    UserGeneraId,
    Cantidad,
    Recertificacion,
    Enabled,
    CreatedBy,
    CreatedAt,
    UpdatedBy,
    UpdatedAt
) VALUES
-- Costa Rica processes (assigned to test.asesor@sitca.test)
(1, DATEADD(day, -45, GETDATE()), NULL, 'En Proceso', 'CERT-CR-2024-001', @AsesorCRId, @AuditorId, 1, 1, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -45, GETDATE()), @AsesorCRId, GETDATE()),
(2, DATEADD(day, -90, GETDATE()), DATEADD(day, -10, GETDATE()), 'Certificada', 'CERT-CR-2024-002', @AsesorCRId, @AuditorId, 2, 1, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -90, GETDATE()), @AuditorId, DATEADD(day, -10, GETDATE())),
(3, DATEADD(day, -30, GETDATE()), NULL, 'Pendiente Documentación', 'CERT-CR-2024-003', @AsesorCRId, NULL, 3, 1, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -30, GETDATE()), @AsesorCRId, GETDATE()),
(4, DATEADD(day, -60, GETDATE()), NULL, 'En Auditoría', 'CERT-CR-2024-004', @AsesorCRId, @AuditorId, 4, 2, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -60, GETDATE()), @AuditorId, DATEADD(day, -5, GETDATE())),
(5, DATEADD(day, -120, GETDATE()), DATEADD(day, -30, GETDATE()), 'Certificada', 'CERT-CR-2024-005', @AsesorCRId, @AuditorId, 5, 2, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -120, GETDATE()), @AuditorId, DATEADD(day, -30, GETDATE())),
(6, DATEADD(day, -75, GETDATE()), DATEADD(day, -20, GETDATE()), 'Certificada', 'CERT-CR-2024-006', @AsesorCRId, @AuditorId, 6, 3, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -75, GETDATE()), @AuditorId, DATEADD(day, -20, GETDATE())),
(7, DATEADD(day, -25, GETDATE()), NULL, 'En Proceso', 'CERT-CR-2024-007', @AsesorCRId, NULL, 7, 3, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -25, GETDATE()), @AsesorCRId, GETDATE()),
(8, DATEADD(day, -15, GETDATE()), NULL, 'Pendiente Documentación', 'CERT-CR-2024-008', @AsesorCRId, NULL, 8, 4, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -15, GETDATE()), @AsesorCRId, GETDATE()),
(9, DATEADD(day, -105, GETDATE()), DATEADD(day, -35, GETDATE()), 'Certificada', 'CERT-CR-2024-009', @AsesorCRId, @AuditorId, 9, 4, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -105, GETDATE()), @AuditorId, DATEADD(day, -35, GETDATE())),
(10, DATEADD(day, -80, GETDATE()), DATEADD(day, -15, GETDATE()), 'Certificada', 'CERT-CR-2024-010', @AsesorCRId, @AuditorId, 10, 5, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -80, GETDATE()), @AuditorId, DATEADD(day, -15, GETDATE())),
(11, DATEADD(day, -40, GETDATE()), NULL, 'En Auditoría', 'CERT-CR-2024-011', @AsesorCRId, @AuditorId, 11, 5, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -40, GETDATE()), @AuditorId, DATEADD(day, -10, GETDATE())),

-- Guatemala processes (assigned to asesor.guatemala@sitca.test)
(12, DATEADD(day, -55, GETDATE()), DATEADD(day, -5, GETDATE()), 'Certificada', 'CERT-GT-2024-001', @AsesorGTId, @AuditorId, 12, 1, @AsesorGTId, 0, 0, 1, @AsesorGTId, DATEADD(day, -55, GETDATE()), @AuditorId, DATEADD(day, -5, GETDATE())),
(13, DATEADD(day, -35, GETDATE()), NULL, 'En Proceso', 'CERT-GT-2024-002', @AsesorGTId, NULL, 13, 3, @AsesorGTId, 0, 0, 1, @AsesorGTId, DATEADD(day, -35, GETDATE()), @AsesorGTId, GETDATE()),

-- El Salvador processes (also assigned to Costa Rica advisor for multi-country support)
(14, DATEADD(day, -20, GETDATE()), NULL, 'Pendiente Documentación', 'CERT-SV-2024-001', @AsesorCRId, NULL, 14, 1, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -20, GETDATE()), @AsesorCRId, GETDATE()),
(15, DATEADD(day, -65, GETDATE()), DATEADD(day, -25, GETDATE()), 'Certificada', 'CERT-SV-2024-002', @AsesorCRId, @AuditorId, 15, 2, @AsesorCRId, 0, 0, 1, @AsesorCRId, DATEADD(day, -65, GETDATE()), @AuditorId, DATEADD(day, -25, GETDATE()));

SET IDENTITY_INSERT ProcesoCertificacion OFF;

-- Add some process status updates/notes if there's a related table
-- This would depend on your actual schema structure

PRINT 'Certification processes created successfully';
PRINT 'Processes assigned to advisors:';
PRINT '  - test.asesor@sitca.test: 13 processes (CR and SV companies)';
PRINT '  - asesor.guatemala@sitca.test: 2 processes (GT companies)';
PRINT 'Process status distribution:';
PRINT '  - Certificada: 7 processes';
PRINT '  - En Proceso: 3 processes';
PRINT '  - En Auditoría: 2 processes';
PRINT '  - Pendiente Documentación: 3 processes';