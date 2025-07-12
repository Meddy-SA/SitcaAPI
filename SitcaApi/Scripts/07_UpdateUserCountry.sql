-- Script 07: Update User Country by Role and Country Name
-- Usage: Execute this script by uncommenting the desired role and country combination
-- Example: To update auditor to Nicaragua, uncomment the corresponding section

-- Country IDs reference:
-- 1 = Belize
-- 2 = Guatemala  
-- 3 = El Salvador
-- 4 = Honduras
-- 5 = Nicaragua
-- 6 = Costa Rica
-- 7 = Panama
-- 8 = República Dominicana

-- Create a stored procedure for easy reuse
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'UpdateUserCountryByRole')
    DROP PROCEDURE UpdateUserCountryByRole;
GO

CREATE PROCEDURE UpdateUserCountryByRole
    @RoleName NVARCHAR(50),
    @CountryName NVARCHAR(100)
AS
BEGIN
    DECLARE @PaisId INT;
    DECLARE @UpdatedCount INT;
    
    -- Get country ID based on name
    SELECT @PaisId = Id FROM Pais WHERE LOWER(Nombre) = LOWER(@CountryName);
    
    IF @PaisId IS NULL
    BEGIN
        PRINT 'Error: Country "' + @CountryName + '" not found.';
        PRINT 'Valid countries are: Belize, Guatemala, El Salvador, Honduras, Nicaragua, Costa Rica, Panama, República Dominicana';
        RETURN;
    END
    
    -- Update users with the specified role
    UPDATE u
    SET u.PaisId = @PaisId,
        u.Ciudad = p.Capital,
        u.Nacionalidad = p.Gentilicio
    FROM AspNetUsers u
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    INNER JOIN Pais p ON p.Id = @PaisId
    WHERE r.Name = @RoleName 
    AND u.Email LIKE '%@sitca.test';
    
    SET @UpdatedCount = @@ROWCOUNT;
    
    PRINT 'Updated ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' ' + @RoleName + ' user(s) to country: ' + @CountryName + ' (ID: ' + CAST(@PaisId AS VARCHAR(10)) + ')';
END
GO

-- Example usage - uncomment the lines you want to execute:

-- Update all test auditors to Nicaragua
-- EXEC UpdateUserCountryByRole 'Auditor', 'Nicaragua';

-- Update all test advisors to Guatemala  
-- EXEC UpdateUserCountryByRole 'Asesor', 'Guatemala';

-- Update all test ATP users to El Salvador
-- EXEC UpdateUserCountryByRole 'ATP', 'El Salvador';

-- Update all test TecnicoPais users to Honduras
-- EXEC UpdateUserCountryByRole 'TecnicoPais', 'Honduras';

-- Update all test CTC users to Panama
-- EXEC UpdateUserCountryByRole 'CTC', 'Panama';

-- Alternative: Direct update commands (uncomment to use)
-- These update ALL users with the specified role, not just test users

-- Update Auditor to Nicaragua
/*
UPDATE u
SET u.PaisId = 5,
    u.Ciudad = 'Managua',
    u.Nacionalidad = 'Nicaragüense'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Auditor' AND u.Email LIKE '%@sitca.test';
*/

-- Update Asesor to Guatemala
/*
UPDATE u
SET u.PaisId = 2,
    u.Ciudad = 'Ciudad de Guatemala',
    u.Nacionalidad = 'Guatemalteco'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Asesor' AND u.Email LIKE '%@sitca.test';
*/

-- Update ATP to El Salvador
/*
UPDATE u
SET u.PaisId = 3,
    u.Ciudad = 'San Salvador',
    u.Nacionalidad = 'Salvadoreño'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'ATP' AND u.Email LIKE '%@sitca.test';
*/

-- Update TecnicoPais to Honduras
/*
UPDATE u
SET u.PaisId = 4,
    u.Ciudad = 'Tegucigalpa',
    u.Nacionalidad = 'Hondureño'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'TecnicoPais' AND u.Email LIKE '%@sitca.test';
*/

-- Update CTC to Panama
/*
UPDATE u
SET u.PaisId = 7,
    u.Ciudad = 'Ciudad de Panamá',
    u.Nacionalidad = 'Panameño'
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'CTC' AND u.Email LIKE '%@sitca.test';
*/

PRINT 'Script ready. Uncomment the desired update command and execute.';