-- Script 09: Certification Processes (Updated - No ID field)
-- This script creates certification processes using the test users and without manual ID insertion

-- Get user IDs for all test users
DECLARE @AsesorId NVARCHAR(450);
DECLARE @AuditorId NVARCHAR(450);
DECLARE @TecnicoPaisId NVARCHAR(450);
DECLARE @ATPId NVARCHAR(450);
DECLARE @CTCId NVARCHAR(450);

SELECT @AsesorId = Id FROM AspNetUsers WHERE Email = 'test.asesor@sitca.test';
SELECT @AuditorId = Id FROM AspNetUsers WHERE Email = 'test.auditor@sitca.test';
SELECT @TecnicoPaisId = Id FROM AspNetUsers WHERE Email = 'test.tecnicopais@sitca.test';
SELECT @ATPId = Id FROM AspNetUsers WHERE Email = 'test.atp@sitca.test';
SELECT @CTCId = Id FROM AspNetUsers WHERE Email = 'test.ctc@sitca.test';

-- Clear existing test certification processes if needed (optional)
-- DELETE FROM ProcesoCertificacion WHERE NumeroExpediente LIKE 'TEST-%';

-- Check what companies exist and get their IDs dynamically
DECLARE @CostaRica1 INT, @CostaRica2 INT, @CostaRica3 INT, @CostaRica4 INT;
DECLARE @Nicaragua1 INT, @Nicaragua2 INT;
DECLARE @Honduras1 INT, @Honduras2 INT;
DECLARE @Belize1 INT, @Belize2 INT;
DECLARE @Panama1 INT, @Panama2 INT;
DECLARE @Guatemala1 INT, @Guatemala2 INT;
DECLARE @ElSalvador1 INT, @ElSalvador2 INT;

-- Get Costa Rica company IDs (from 02_TestCompanies.sql) by IdNacional
SELECT @CostaRica1 = Id FROM Empresa WHERE IdNacional = 'CR-001-2024'; -- Hotel Paradise Beach Resort
SELECT @CostaRica2 = Id FROM Empresa WHERE IdNacional = 'CR-002-2024'; -- Eco Lodge Monteverde  
SELECT @CostaRica3 = Id FROM Empresa WHERE IdNacional = 'CR-003-2024'; -- Business Hotel Central
SELECT @CostaRica4 = Id FROM Empresa WHERE IdNacional = 'CR-004-2024'; -- Restaurante La Costa Brava

-- Get additional companies from script 08
SELECT @Nicaragua1 = Id FROM Empresa WHERE IdNacional = 'NI-001-2024'; -- Nicaragua Hotel
SELECT @Nicaragua2 = Id FROM Empresa WHERE IdNacional = 'NI-002-2024'; -- Nicaragua Restaurant
SELECT @Honduras1 = Id FROM Empresa WHERE IdNacional = 'HN-001-2024'; -- Honduras Hotel
SELECT @Honduras2 = Id FROM Empresa WHERE IdNacional = 'HN-002-2024'; -- Honduras Transport
SELECT @Belize1 = Id FROM Empresa WHERE IdNacional = 'BZ-001-2024'; -- Belize Hotel
SELECT @Belize2 = Id FROM Empresa WHERE IdNacional = 'BZ-002-2024'; -- Belize Restaurant
SELECT @Panama1 = Id FROM Empresa WHERE IdNacional = 'PA-001-2024'; -- Panama Hotel
SELECT @Panama2 = Id FROM Empresa WHERE IdNacional = 'PA-002-2024'; -- Panama Transport
SELECT @Guatemala1 = Id FROM Empresa WHERE IdNacional = 'GT-001-2024'; -- Guatemala Hotel (from 02_TestCompanies.sql)
SELECT @Guatemala2 = Id FROM Empresa WHERE IdNacional = 'GT-002-2024'; -- Guatemala Tours (from 02_TestCompanies.sql)
SELECT @ElSalvador1 = Id FROM Empresa WHERE IdNacional = 'SV-001-2024'; -- El Salvador Resort (from 02_TestCompanies.sql)
SELECT @ElSalvador2 = Id FROM Empresa WHERE IdNacional = 'SV-002-2024'; -- El Salvador Pupuseria (from 02_TestCompanies.sql)

-- Check if we have at least some companies to work with
IF @CostaRica1 IS NULL AND @CostaRica2 IS NULL AND @CostaRica3 IS NULL AND @CostaRica4 IS NULL 
   AND @Nicaragua1 IS NULL AND @Nicaragua2 IS NULL AND @Honduras1 IS NULL AND @Honduras2 IS NULL
   AND @Belize1 IS NULL AND @Belize2 IS NULL AND @Panama1 IS NULL AND @Panama2 IS NULL 
   AND @Guatemala1 IS NULL AND @Guatemala2 IS NULL AND @ElSalvador1 IS NULL AND @ElSalvador2 IS NULL
BEGIN
    PRINT 'Error: No test companies found. Please run 02_TestCompanies.sql and/or 08_AdditionalCompanies.sql first.';
    RETURN;
END

PRINT 'Found companies. Creating certification processes...';

-- Insert certification processes WITHOUT ID field (auto-incremental)
-- Insert Costa Rica certification processes if companies exist
IF @CostaRica1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Hotel Paradise Beach Resort - Para Auditar
    (DATEADD(day, -45, GETDATE()), NULL, '4 - Para Auditar', 'TEST-CR-2024-001', @AsesorId, @AuditorId, @CostaRica1, 1, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -45, GETDATE()), @AsesorId, GETDATE(), NULL);
END

IF @CostaRica2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Eco Lodge Monteverde - Finalizado
    (DATEADD(day, -90, GETDATE()), DATEADD(day, -10, GETDATE()), '8 - Finalizado', 'TEST-CR-2024-002', @AsesorId, @AuditorId, @CostaRica2, 1, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -90, GETDATE()), @AuditorId, DATEADD(day, -10, GETDATE()), DATEADD(year, 2, DATEADD(day, -10, GETDATE())));
END

IF @CostaRica3 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Business Hotel Central - Asesoria en Proceso
    (DATEADD(day, -30, GETDATE()), NULL, '2 - Asesoria en Proceso', 'TEST-CR-2024-003', @AsesorId, NULL, @CostaRica3, 1, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -30, GETDATE()), @AsesorId, GETDATE(), NULL);
END

IF @CostaRica4 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Restaurante La Costa Brava - Auditoria Finalizada
    (DATEADD(day, -60, GETDATE()), NULL, '6 - Auditoria Finalizada', 'TEST-CR-2024-004', @AsesorId, @AuditorId, @CostaRica4, 2, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -60, GETDATE()), @AuditorId, DATEADD(day, -5, GETDATE()), NULL);
END

-- Insert additional country processes only if companies exist
-- Nicaragua companies
IF @Nicaragua1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Nicaragua Hotel - Para Auditar
    (DATEADD(day, -20, GETDATE()), NULL, '4 - Para Auditar', 'TEST-NI-2024-001', @ATPId, @AuditorId, @Nicaragua1, 1, @ATPId, 0, 0, 1, @ATPId, DATEADD(day, -20, GETDATE()), @ATPId, GETDATE(), NULL);
END

IF @Nicaragua2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Nicaragua Restaurant - Finalizado
    (DATEADD(day, -70, GETDATE()), DATEADD(day, -15, GETDATE()), '8 - Finalizado', 'TEST-NI-2024-002', @ATPId, @AuditorId, @Nicaragua2, 2, @ATPId, 0, 0, 1, @ATPId, DATEADD(day, -70, GETDATE()), @AuditorId, DATEADD(day, -15, GETDATE()), DATEADD(year, 2, DATEADD(day, -15, GETDATE())));
END

-- Honduras companies
IF @Honduras1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Honduras Hotel - Asesoria Finalizada
    (DATEADD(day, -35, GETDATE()), NULL, 3, 'TEST-HN-2024-001', @TecnicoPaisId, NULL, @Honduras1, 1, @TecnicoPaisId, 0, 0, 1, @TecnicoPaisId, DATEADD(day, -35, GETDATE()), @TecnicoPaisId, GETDATE(), NULL);
END

IF @Honduras2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Honduras Transport - Finalizado
    (DATEADD(day, -100, GETDATE()), DATEADD(day, -40, GETDATE()), 8, 'TEST-HN-2024-002', @TecnicoPaisId, @AuditorId, @Honduras2, 4, @TecnicoPaisId, 0, 0, 1, @TecnicoPaisId, DATEADD(day, -100, GETDATE()), @AuditorId, DATEADD(day, -40, GETDATE()), DATEADD(year, 2, DATEADD(day, -40, GETDATE())));
END

-- Belize companies
IF @Belize1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Belize Hotel - Auditoria en Proceso
    (DATEADD(day, -50, GETDATE()), NULL, 5, 'TEST-BZ-2024-001', @CTCId, @AuditorId, @Belize1, 1, @CTCId, 0, 0, 1, @CTCId, DATEADD(day, -50, GETDATE()), @CTCId, GETDATE(), NULL);
END

IF @Belize2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Belize Restaurant - Para Asesorar
    (DATEADD(day, -25, GETDATE()), NULL, 1, 'TEST-BZ-2024-002', @CTCId, NULL, @Belize2, 2, @CTCId, 0, 0, 1, @CTCId, DATEADD(day, -25, GETDATE()), @CTCId, GETDATE(), NULL);
END

-- Panama companies
IF @Panama1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Panama Hotel - Finalizado
    (DATEADD(day, -85, GETDATE()), DATEADD(day, -25, GETDATE()), 8, 'TEST-PA-2024-001', @AsesorId, @AuditorId, @Panama1, 1, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -85, GETDATE()), @AuditorId, DATEADD(day, -25, GETDATE()), DATEADD(year, 2, DATEADD(day, -25, GETDATE())));
END

IF @Panama2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Panama Transport - Asesoria en Proceso
    (DATEADD(day, -15, GETDATE()), NULL, 2, 'TEST-PA-2024-002', @ATPId, NULL, @Panama2, 4, @ATPId, 0, 0, 1, @ATPId, DATEADD(day, -15, GETDATE()), @ATPId, GETDATE(), NULL);
END

-- Guatemala companies (from 02_TestCompanies.sql)
IF @Guatemala1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Guatemala Hotel - Finalizado
    (DATEADD(day, -65, GETDATE()), DATEADD(day, -5, GETDATE()), 8, 'TEST-GT-2024-001', @TecnicoPaisId, @AuditorId, @Guatemala1, 1, @TecnicoPaisId, 0, 0, 1, @TecnicoPaisId, DATEADD(day, -65, GETDATE()), @AuditorId, DATEADD(day, -5, GETDATE()), DATEADD(year, 2, DATEADD(day, -5, GETDATE())));
END

IF @Guatemala2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- Guatemala Tours - En revisión de CTC
    (DATEADD(day, -40, GETDATE()), NULL, 7, 'TEST-GT-2024-002', @AsesorId, @AuditorId, @Guatemala2, 3, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -40, GETDATE()), @AuditorId, DATEADD(day, -10, GETDATE()), NULL);
END

-- El Salvador companies (from 02_TestCompanies.sql)
IF @ElSalvador1 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- El Salvador Resort - Asesoria Finalizada
    (DATEADD(day, -30, GETDATE()), NULL, 3, 'TEST-SV-2024-001', @CTCId, NULL, @ElSalvador1, 1, @CTCId, 0, 0, 1, @CTCId, DATEADD(day, -30, GETDATE()), @CTCId, GETDATE(), NULL);
END

IF @ElSalvador2 IS NOT NULL
BEGIN
    INSERT INTO ProcesoCertificacion (
        FechaInicio, FechaFinalizacion, Status, NumeroExpediente, AsesorId, AuditorId, EmpresaId, TipologiaId, 
        UserGeneraId, Cantidad, Recertificacion, Enabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, FechaVencimiento
    ) VALUES
    -- El Salvador Pupuseria - Finalizado
    (DATEADD(day, -95, GETDATE()), DATEADD(day, -30, GETDATE()), 8, 'TEST-SV-2024-002', @AsesorId, @AuditorId, @ElSalvador2, 2, @AsesorId, 0, 0, 1, @AsesorId, DATEADD(day, -95, GETDATE()), @AuditorId, DATEADD(day, -30, GETDATE()), DATEADD(year, 2, DATEADD(day, -30, GETDATE())));
END

-- Summary of statuses used:
-- 0 - Inicial
-- 1 - Para Asesorar
-- 2 - Asesoria en Proceso
-- 3 - Asesoria Finalizada
-- 4 - Para Auditar
-- 5 - Auditoria en Proceso
-- 6 - Auditoria Finalizada
-- 7 - En revisión de CTC
-- 8 - Finalizado

PRINT 'Certification processes created successfully using test users:';
PRINT '  - test.asesor@sitca.test: Costa Rica, Panama, Guatemala, El Salvador processes';
PRINT '  - test.auditor@sitca.test: As auditor in multiple processes';
PRINT '  - test.tecnicopais@sitca.test: Honduras and Guatemala processes';
PRINT '  - test.atp@sitca.test: Nicaragua and Panama processes';
PRINT '  - test.ctc@sitca.test: Belize and El Salvador processes';
PRINT '';
PRINT 'All processes created without manual ID insertion (auto-incremental).';

-- UPDATE existing test processes to fix Status format (ID - Name)
UPDATE ProcesoCertificacion 
SET Status = CASE 
    WHEN Status = '0' OR Status = 0 THEN '0 - Inicial'
    WHEN Status = '1' OR Status = 1 THEN '1 - Para Asesorar'
    WHEN Status = '2' OR Status = 2 THEN '2 - Asesoria en Proceso'
    WHEN Status = '3' OR Status = 3 THEN '3 - Asesoria Finalizada'
    WHEN Status = '4' OR Status = 4 THEN '4 - Para Auditar'
    WHEN Status = '5' OR Status = 5 THEN '5 - Auditoria en Proceso'
    WHEN Status = '6' OR Status = 6 THEN '6 - Auditoria Finalizada'
    WHEN Status = '7' OR Status = 7 THEN '7 - En revisión de CTC'
    WHEN Status = '8' OR Status = 8 THEN '8 - Finalizado'
    ELSE Status -- Keep current value if already in correct format
END
WHERE NumeroExpediente LIKE 'TEST-%'
AND (Status NOT LIKE '% - %' OR Status IN ('0','1','2','3','4','5','6','7','8'));

PRINT 'Status field updated to correct format (ID - Name) for all test processes.';