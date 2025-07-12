-- Script 11: Fix Status Format in ProcesoCertificacion
-- This script updates the Status field to include both ID and Name (format: "ID - Name")

PRINT 'Fixing Status format in ProcesoCertificacion table...';

-- Show current status values before update
PRINT 'Current Status values in test processes:';
SELECT DISTINCT Status, COUNT(*) as Count
FROM ProcesoCertificacion 
WHERE NumeroExpediente LIKE 'TEST-%'
GROUP BY Status
ORDER BY Status;

-- Update Status field to correct format (ID - Name)
UPDATE ProcesoCertificacion 
SET Status = CASE 
    WHEN Status = '0' OR Status = 0 OR CAST(Status AS VARCHAR) = '0' THEN '0 - Inicial'
    WHEN Status = '1' OR Status = 1 OR CAST(Status AS VARCHAR) = '1' THEN '1 - Para Asesorar'
    WHEN Status = '2' OR Status = 2 OR CAST(Status AS VARCHAR) = '2' THEN '2 - Asesoria en Proceso'
    WHEN Status = '3' OR Status = 3 OR CAST(Status AS VARCHAR) = '3' THEN '3 - Asesoria Finalizada'
    WHEN Status = '4' OR Status = 4 OR CAST(Status AS VARCHAR) = '4' THEN '4 - Para Auditar'
    WHEN Status = '5' OR Status = 5 OR CAST(Status AS VARCHAR) = '5' THEN '5 - Auditoria en Proceso'
    WHEN Status = '6' OR Status = 6 OR CAST(Status AS VARCHAR) = '6' THEN '6 - Auditoria Finalizada'
    WHEN Status = '7' OR Status = 7 OR CAST(Status AS VARCHAR) = '7' THEN '7 - En revisión de CTC'
    WHEN Status = '8' OR Status = 8 OR CAST(Status AS VARCHAR) = '8' THEN '8 - Finalizado'
    ELSE Status -- Keep current value if already in correct format
END
WHERE NumeroExpediente LIKE 'TEST-%'
AND (Status NOT LIKE '% - %' OR Status IN ('0','1','2','3','4','5','6','7','8'));

DECLARE @UpdatedRows INT = @@ROWCOUNT;

-- Show updated status values
PRINT 'Updated Status values in test processes:';
SELECT DISTINCT Status, COUNT(*) as Count
FROM ProcesoCertificacion 
WHERE NumeroExpediente LIKE 'TEST-%'
GROUP BY Status
ORDER BY Status;

PRINT 'Status field updated successfully for ' + CAST(@UpdatedRows AS VARCHAR(10)) + ' test processes.';
PRINT 'Format: "ID - Name" (e.g., "4 - Para Auditar", "8 - Finalizado")';

-- Optional: Update ALL ProcesoCertificacion records, not just test ones
-- Uncomment the following section if you want to fix all records in the table

/*
PRINT 'Updating ALL ProcesoCertificacion records...';

UPDATE ProcesoCertificacion 
SET Status = CASE 
    WHEN Status = '0' OR Status = 0 OR CAST(Status AS VARCHAR) = '0' THEN '0 - Inicial'
    WHEN Status = '1' OR Status = 1 OR CAST(Status AS VARCHAR) = '1' THEN '1 - Para Asesorar'
    WHEN Status = '2' OR Status = 2 OR CAST(Status AS VARCHAR) = '2' THEN '2 - Asesoria en Proceso'
    WHEN Status = '3' OR Status = 3 OR CAST(Status AS VARCHAR) = '3' THEN '3 - Asesoria Finalizada'
    WHEN Status = '4' OR Status = 4 OR CAST(Status AS VARCHAR) = '4' THEN '4 - Para Auditar'
    WHEN Status = '5' OR Status = 5 OR CAST(Status AS VARCHAR) = '5' THEN '5 - Auditoria en Proceso'
    WHEN Status = '6' OR Status = 6 OR CAST(Status AS VARCHAR) = '6' THEN '6 - Auditoria Finalizada'
    WHEN Status = '7' OR Status = 7 OR CAST(Status AS VARCHAR) = '7' THEN '7 - En revisión de CTC'
    WHEN Status = '8' OR Status = 8 OR CAST(Status AS VARCHAR) = '8' THEN '8 - Finalizado'
    ELSE Status
END
WHERE Status NOT LIKE '% - %' OR Status IN ('0','1','2','3','4','5','6','7','8');

SET @UpdatedRows = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedRows AS VARCHAR(10)) + ' total records in ProcesoCertificacion.';
*/