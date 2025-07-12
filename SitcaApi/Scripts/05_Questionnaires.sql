-- Script 05: Questionnaires and Related Data
-- This script creates sample questionnaires linked to certification processes

-- Get user IDs for the advisors and auditor
DECLARE @AsesorCRId NVARCHAR(450);
DECLARE @AsesorGTId NVARCHAR(450);
DECLARE @AuditorId NVARCHAR(450);

SELECT @AsesorCRId = Id FROM AspNetUsers WHERE Email = 'test.asesor@sitca.test';
SELECT @AsesorGTId = Id FROM AspNetUsers WHERE Email = 'asesor.guatemala@sitca.test';
SELECT @AuditorId = Id FROM AspNetUsers WHERE Email = 'auditor.test@sitca.test';

-- Insert questionnaires for certification processes
SET IDENTITY_INSERT Cuestionario ON;

INSERT INTO Cuestionario (
    Id,
    IdEmpresa,
    ProcesoCertificacionId,
    AsesorId,
    AuditorId,
    FechaInicio,
    FechaGenerado,
    FechaVisita,
    FechaFinalizado,
    Resultado,
    Prueba,
    FechaRevisionAuditor,
    IdTipologia,
    TipologiaId,
    TecnicoPaisId
) VALUES
-- Questionnaires for completed certifications
(1, 2, 2, @AsesorCRId, @AuditorId, DATEADD(day, -85, GETDATE()), DATEADD(day, -85, GETDATE()), DATEADD(day, -70, GETDATE()), DATEADD(day, -15, GETDATE()), 1, 0, DATEADD(day, -12, GETDATE()), 1, 1, NULL),

(2, 5, 5, @AsesorCRId, @AuditorId, DATEADD(day, -115, GETDATE()), DATEADD(day, -115, GETDATE()), DATEADD(day, -100, GETDATE()), DATEADD(day, -35, GETDATE()), 1, 0, DATEADD(day, -32, GETDATE()), 2, 2, NULL),

(3, 6, 6, @AsesorCRId, @AuditorId, DATEADD(day, -70, GETDATE()), DATEADD(day, -70, GETDATE()), DATEADD(day, -55, GETDATE()), DATEADD(day, -25, GETDATE()), 1, 0, DATEADD(day, -22, GETDATE()), 3, 3, NULL),

(4, 9, 9, @AsesorCRId, @AuditorId, DATEADD(day, -100, GETDATE()), DATEADD(day, -100, GETDATE()), DATEADD(day, -85, GETDATE()), DATEADD(day, -40, GETDATE()), 1, 0, DATEADD(day, -37, GETDATE()), 4, 4, NULL),

(5, 10, 10, @AsesorCRId, @AuditorId, DATEADD(day, -75, GETDATE()), DATEADD(day, -75, GETDATE()), DATEADD(day, -60, GETDATE()), DATEADD(day, -20, GETDATE()), 1, 0, DATEADD(day, -17, GETDATE()), 5, 5, NULL),

(6, 12, 12, @AsesorGTId, @AuditorId, DATEADD(day, -50, GETDATE()), DATEADD(day, -50, GETDATE()), DATEADD(day, -35, GETDATE()), DATEADD(day, -10, GETDATE()), 1, 0, DATEADD(day, -7, GETDATE()), 1, 1, NULL),

(7, 15, 15, @AsesorCRId, @AuditorId, DATEADD(day, -60, GETDATE()), DATEADD(day, -60, GETDATE()), DATEADD(day, -45, GETDATE()), DATEADD(day, -30, GETDATE()), 1, 0, DATEADD(day, -27, GETDATE()), 2, 2, NULL),

-- Questionnaires for processes in progress (without final results)
(8, 1, 1, @AsesorCRId, @AuditorId, DATEADD(day, -40, GETDATE()), DATEADD(day, -40, GETDATE()), DATEADD(day, -25, GETDATE()), NULL, 0, 0, NULL, 1, 1, NULL),

(9, 4, 4, @AsesorCRId, @AuditorId, DATEADD(day, -55, GETDATE()), DATEADD(day, -55, GETDATE()), DATEADD(day, -40, GETDATE()), NULL, 0, 0, NULL, 2, 2, NULL),

(10, 11, 11, @AsesorCRId, @AuditorId, DATEADD(day, -35, GETDATE()), DATEADD(day, -35, GETDATE()), DATEADD(day, -20, GETDATE()), NULL, 0, 0, NULL, 5, 5, NULL),

(11, 7, 7, @AsesorCRId, NULL, DATEADD(day, -20, GETDATE()), DATEADD(day, -20, GETDATE()), NULL, NULL, 0, 0, NULL, 3, 3, NULL),

(12, 13, 13, @AsesorGTId, NULL, DATEADD(day, -30, GETDATE()), DATEADD(day, -30, GETDATE()), NULL, NULL, 0, 0, NULL, 3, 3, NULL);

SET IDENTITY_INSERT Cuestionario OFF;

-- Insert sample questionnaire items (detailed questions and responses)
-- This assumes there's a CuestionarioItem or similar table for individual questions
-- Adjust according to your actual schema

-- Sample data for questionnaire items (basic structure)
IF OBJECT_ID('CuestionarioItem', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT CuestionarioItem ON;
    
    INSERT INTO CuestionarioItem (
        Id,
        CuestionarioId,
        Pregunta,
        Respuesta,
        Puntuacion,
        Observaciones,
        Categoria,
        Requerido,
        Orden
    ) VALUES
    -- Items for completed questionnaire (Eco Lodge - ID 1)
    (1, 1, '¿La empresa cuenta con políticas ambientales documentadas?', 'Sí', 5, 'Políticas claramente definidas y actualizadas', 'Sostenibilidad', 1, 1),
    (2, 1, '¿Se implementan prácticas de ahorro de agua?', 'Sí', 5, 'Sistema de reutilización de aguas grises', 'Sostenibilidad', 1, 2),
    (3, 1, '¿El personal está capacitado en turismo sostenible?', 'Sí', 4, 'Capacitación anual documentada', 'Capacitación', 1, 3),
    (4, 1, '¿Se cuenta con certificados de calidad vigentes?', 'Sí', 5, 'Certificados ISO actualizados', 'Calidad', 1, 4),
    
    -- Items for restaurant questionnaire (Soda Típica - ID 2)
    (5, 2, '¿Se cumplen las normas de higiene alimentaria?', 'Sí', 5, 'Certificado de salud vigente', 'Higiene', 1, 1),
    (6, 2, '¿El personal usa uniformes y equipo de protección?', 'Sí', 5, 'Uniformes completos y limpios', 'Higiene', 1, 2),
    (7, 2, '¿Se utilizan ingredientes locales?', 'Sí', 4, '80% de ingredientes de productores locales', 'Sostenibilidad', 0, 3),
    
    -- Items for tour operator questionnaire (Costa Rica Adventures - ID 3)
    (8, 3, '¿Los guías tienen licencia oficial?', 'Sí', 5, 'Todos los guías certificados por ICT', 'Calidad', 1, 1),
    (9, 3, '¿Se promueve el turismo responsable?', 'Sí', 5, 'Programas educativos implementados', 'Sostenibilidad', 1, 2),
    (10, 3, '¿Existe seguro de responsabilidad civil?', 'Sí', 5, 'Póliza vigente por $500,000', 'Seguridad', 1, 3);
    
    SET IDENTITY_INSERT CuestionarioItem OFF;
END

-- Create some sample audit findings or recommendations if there's such a table
IF OBJECT_ID('AuditFindings', 'U') IS NOT NULL OR OBJECT_ID('Recomendaciones', 'U') IS NOT NULL
BEGIN
    -- Insert sample findings/recommendations
    PRINT 'Sample audit findings would be inserted here based on actual schema';
END

PRINT 'Questionnaires and related data created successfully';
PRINT 'Questionnaires created:';
PRINT '  - Completed with results: 7 questionnaires';
PRINT '  - In progress: 5 questionnaires';
PRINT '  - Total companies with questionnaires: 12';
PRINT '  - Sample questionnaire items: 10 items across 3 questionnaires';