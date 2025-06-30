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
    FirstName,
    LastName,
    PaisId,
    Active,
    Lenguage,
    Notificaciones,
    CreatedAt,
    UpdatedAt
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
    'María Elena',
    'González',
    2, -- Guatemala
    1, 'es', 1, GETDATE(), GETDATE()
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
    'Carlos',
    'Auditor',
    1, -- Costa Rica
    1, 'es', 1, GETDATE(), GETDATE()
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
