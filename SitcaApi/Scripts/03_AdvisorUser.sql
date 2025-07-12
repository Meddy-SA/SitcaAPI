-- Script 03: Advisor User and Related Data
-- This script creates the advisor user test.asesor@sitca.test and related authentication data

-- First, insert the user into AspNetUsers
-- Password: TestAsesor123! (hashed with Identity default hasher)
-- Note: You may need to adjust the password hash based on your Identity configuration


-- Get the UserId for role assignment
DECLARE @UserId NVARCHAR(450);
-- Assign roles to the advisor user
DECLARE @RoleId NVARCHAR(450);
DECLARE @RoleAsesorId NVARCHAR(450);

-- Create the main advisor user mentioned in other scripts
INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumber,
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnabled, 
    AccessFailedCount,
    Discriminator,
    FirstName,
    LastName,
    PaisId,
    Active,
    Lenguage,
    Notificaciones,
    Codigo,
    Ciudad,
    Departamento,
    Direccion,
    DocumentoAcreditacion,
    FechaIngreso,
    HojaDeVida,
    NumeroCarnet,
    DocumentoIdentidad,
    Nacionalidad,
    Profesion
) VALUES 
-- Main advisor user
(
    NEWID(),
    'test.asesor@sitca.test',
    'TEST.ASESOR@SITCA.TEST',
    'test.asesor@sitca.test',
    'TEST.ASESOR@SITCA.TEST',
    1,
    'AQAAAAEAACcQAAAAEKqJQrZOXqJdJdJMPTzHuAI8pJCUJlsAOcyKUKrGR8hxKfFgb3xF8MrHxH0wZVjG8Q==',
    NEWID(),
    NEWID(),
    '+506 8888-9999',
    1, 0, 1, 0,
    'ApplicationUser',
    'Juan Carlos',
    'Asesor Principal',
    1, -- Costa Rica
    1, 'es', 1,
    'ASES-CR-001', 'San José', 'San José', 'Escazú, San José', 'Cert-Asesor-CR-001', '2021-01-10', 'CV-JuanCarlos.pdf', 'CR-ASES-001', '1122334455667', 'Costarricense', 'Ingeniero Industrial'
);

-- Assign role to main advisor
DECLARE @MainUserId NVARCHAR(450);
SELECT @MainUserId = Id FROM AspNetUsers WHERE Email = 'test.asesor@sitca.test';
SELECT @RoleAsesorId = Id FROM AspNetRoles WHERE Name = 'Asesor';
IF @RoleAsesorId IS NOT NULL AND @MainUserId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@MainUserId, @RoleAsesorId);

-- Create additional test users for variety
INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumber,
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnabled, 
    AccessFailedCount,
    Discriminator,
    FirstName,
    LastName,
    PaisId,
    Active,
    Lenguage,
    Notificaciones,
    Codigo,
    Ciudad,
    Departamento,
    Direccion,
    DocumentoAcreditacion,
    FechaIngreso,
    HojaDeVida,
    NumeroCarnet,
    DocumentoIdentidad,
    Nacionalidad,
    Profesion
) VALUES 
-- Additional advisor for Guatemala
(
    NEWID(),
    'asesor.guatemala@sitca.test',
    'ASESOR.GUATEMALA@SITCA.TEST',
    'asesor.guatemala@sitca.test',
    'ASESOR.GUATEMALA@SITCA.TEST',
    1,
    'AQAAAAEAACcQAAAAEKqJQrZOXqJdJdJMPTzHuAI8pJCUJlsAOcyKUKrGR8hxKfFgb3xF8MrHxH0wZVjG8Q==',
    NEWID(),
    NEWID(),
    '+502 5555-6666',
    1, 0, 1, 0,
    'ApplicationUser',
    'María Elena',
    'González',
    2, -- Guatemala
    1, 'es', 1, 
    'ASES-GT-001', 'Guatemala City', 'Guatemala', 'Zona 10, Ciudad de Guatemala', 'Cert-Asesor-GT-001', '2023-01-15', 'CV-MariaElena.pdf', 'GT-ASES-001', '1234567890123', 'Guatemalteca', 'Ingeniera en Turismo'
),
-- Test auditor
(
    NEWID(),
    'auditor.test@sitca.test',
    'AUDITOR.TEST@SITCA.TEST',
    'auditor.test@sitca.test',
    'AUDITOR.TEST@SITCA.TEST',
    1,
    'AQAAAAEAACcQAAAAEKqJQrZOXqJdJdJMPTzHuAI8pJCUJlsAOcyKUKrGR8hxKfFgb3xF8MrHxH0wZVjG8Q==',
    NEWID(),
    NEWID(),
    '+506 7777-8888',
    1, 0, 1, 0,
    'ApplicationUser',
    'Carlos',
    'Auditor',
    1, -- Costa Rica
    1, 'es', 1,
    'AUD-CR-001', 'San José', 'San José', 'Sabana Norte, San José', 'Cert-Auditor-CR-001', '2022-03-20', 'CV-CarlosAuditor.pdf', 'CR-AUD-001', '9876543210987', 'Costarricense', 'Auditor de Calidad'
);

-- Assign roles to additional users
-- Guatemala advisor
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'asesor.guatemala@sitca.test';
SELECT @RoleAsesorId = Id FROM AspNetRoles WHERE Name = 'Asesor';
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleAsesorId);

-- Test auditor
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'auditor.test@sitca.test';

SELECT @RoleId = Id FROM AspNetRoles WHERE Name = 'Auditor';
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId); -- Auditor
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleAsesorId); -- Asesor

PRINT 'Advisor users created successfully';
PRINT 'Login credentials:';
PRINT '  - asesor.guatemala@sitca.test / TestAsesor123!';
PRINT '  - auditor.test@sitca.test / TestAsesor123!';
