-- Script 10: Assign User to Company
-- This script allows you to assign a user to a company by providing email and company ID

-- Create a stored procedure for easy reuse
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'AssignUserToCompany')
    DROP PROCEDURE AssignUserToCompany;
GO

CREATE PROCEDURE AssignUserToCompany
    @UserEmail NVARCHAR(256),
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId NVARCHAR(450);
    DECLARE @CompanyName NVARCHAR(200);
    DECLARE @CurrentCompanyId INT;
    DECLARE @UpdateResult INT;
    
    -- Check if user exists
    SELECT @UserId = Id, @CurrentCompanyId = EmpresaId 
    FROM AspNetUsers 
    WHERE Email = @UserEmail;
    
    IF @UserId IS NULL
    BEGIN
        PRINT 'Error: User with email "' + @UserEmail + '" not found.';
        RETURN;
    END
    
    -- Check if company exists
    SELECT @CompanyName = Nombre 
    FROM Empresa 
    WHERE Id = @CompanyId;
    
    IF @CompanyName IS NULL
    BEGIN
        PRINT 'Error: Company with ID ' + CAST(@CompanyId AS VARCHAR(10)) + ' not found.';
        RETURN;
    END
    
    -- Show current assignment
    IF @CurrentCompanyId IS NOT NULL
    BEGIN
        DECLARE @CurrentCompanyName NVARCHAR(200);
        SELECT @CurrentCompanyName = Nombre FROM Empresa WHERE Id = @CurrentCompanyId;
        PRINT 'User currently assigned to: ' + @CurrentCompanyName + ' (ID: ' + CAST(@CurrentCompanyId AS VARCHAR(10)) + ')';
    END
    ELSE
    BEGIN
        PRINT 'User currently not assigned to any company.';
    END
    
    -- Update user's company assignment
    UPDATE AspNetUsers 
    SET EmpresaId = @CompanyId,
        UpdatedAt = GETDATE()
    WHERE Id = @UserId;
    
    SET @UpdateResult = @@ROWCOUNT;
    
    IF @UpdateResult > 0
    BEGIN
        PRINT 'Success: User ' + @UserEmail + ' has been assigned to company "' + @CompanyName + '" (ID: ' + CAST(@CompanyId AS VARCHAR(10)) + ')';
    END
    ELSE
    BEGIN
        PRINT 'Error: Failed to update user company assignment.';
    END
END
GO

-- Alternative: Function to get company info by various criteria
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetCompanyInfo')
    DROP PROCEDURE GetCompanyInfo;
GO

CREATE PROCEDURE GetCompanyInfo
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Search by ID, Name, or IdNacional
    SELECT 
        Id,
        Nombre,
        IdNacional,
        Email,
        PaisId,
        (SELECT Nombre FROM Pais WHERE Id = e.PaisId) AS Pais,
        Active,
        Estado
    FROM Empresa e
    WHERE 
        (TRY_CAST(@SearchTerm AS INT) IS NOT NULL AND Id = TRY_CAST(@SearchTerm AS INT))
        OR Nombre LIKE '%' + @SearchTerm + '%'
        OR IdNacional LIKE '%' + @SearchTerm + '%'
        OR Email LIKE '%' + @SearchTerm + '%'
    ORDER BY Id;
END
GO

-- USAGE EXAMPLES:

-- Example 1: Assign a user to a company
-- EXEC AssignUserToCompany 'test.asesor@sitca.test', 5;

-- Example 2: Search for companies to get their IDs
-- EXEC GetCompanyInfo 'Hotel';
-- EXEC GetCompanyInfo 'CR-001-2024';
-- EXEC GetCompanyInfo '1';

-- Example 3: Direct update commands (uncomment to use)
/*
-- Assign user to Hotel Paradise Beach Resort (ID = 1)
UPDATE AspNetUsers 
SET EmpresaId = 1 
WHERE Email = 'test.asesor@sitca.test';
*/

/*
-- Assign user to Eco Lodge Monteverde (ID = 2)
UPDATE AspNetUsers 
SET EmpresaId = 2 
WHERE Email = 'test.auditor@sitca.test';
*/

/*
-- Remove company assignment from user
UPDATE AspNetUsers 
SET EmpresaId = NULL 
WHERE Email = 'test.tecnicopais@sitca.test';
*/

-- Query to check current user-company assignments
/*
SELECT 
    u.Email,
    u.FirstName + ' ' + u.LastName AS FullName,
    u.EmpresaId,
    e.Nombre AS CompanyName,
    e.IdNacional AS CompanyTaxId,
    p.Nombre AS Country
FROM AspNetUsers u
LEFT JOIN Empresa e ON u.EmpresaId = e.Id
LEFT JOIN Pais p ON e.PaisId = p.Id
WHERE u.Email LIKE '%@sitca.test'
ORDER BY u.Email;
*/

PRINT 'Script ready. Use the stored procedures or uncomment the direct commands as needed.';
PRINT '';
PRINT 'Usage:';
PRINT '  EXEC AssignUserToCompany ''user@email.com'', CompanyId;';
PRINT '  EXEC GetCompanyInfo ''search term'';';