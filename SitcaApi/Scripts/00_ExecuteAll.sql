-- Master Script: Execute All Test Data Scripts
-- This script executes all the test data generation scripts in the correct order
-- Run this script to populate the database with sample data for testing

PRINT '========================================';
PRINT 'SITCA Test Data Generation';
PRINT 'Starting data population...';
PRINT '========================================';

-- Script 01: Base Data (Countries and Tipologies)
PRINT '';
PRINT 'Executing Script 01: Base Data...';
GO
:r 01_BaseData.sql
GO

-- Script 02: Test Companies
PRINT '';
PRINT 'Executing Script 02: Test Companies...';
GO
:r 02_TestCompanies.sql
GO

-- Script 03: Advisor Users
PRINT '';
PRINT 'Executing Script 03: Advisor Users...';
GO
:r 03_AdvisorUser.sql
GO

-- Script 04: Certification Processes
PRINT '';
PRINT 'Executing Script 04: Certification Processes...';
GO
:r 04_CertificationProcesses.sql
GO

-- Script 05: Questionnaires
PRINT '';
PRINT 'Executing Script 05: Questionnaires...';
GO
:r 05_Questionnaires.sql
GO

PRINT '';
PRINT '========================================';
PRINT 'SITCA Test Data Generation Complete!';
PRINT '';
PRINT 'Test Environment Summary:';
PRINT '- Countries: 7 Central American countries';
PRINT '- Business Types: 5 tourism categories';
PRINT '- Companies: 15 sample companies';
PRINT '- Users: 3 test users (2 advisors + 1 auditor)';
PRINT '- Certification Processes: 15 processes';
PRINT '- Questionnaires: 12 questionnaires';
PRINT '';
PRINT 'Login Credentials:';
PRINT '  Main Advisor: test.asesor@sitca.test / TestAsesor123!';
PRINT '  Guatemala Advisor: asesor.guatemala@sitca.test / TestAsesor123!';
PRINT '  Test Auditor: auditor.test@sitca.test / TestAsesor123!';
PRINT '';
PRINT 'The advisor test.asesor@sitca.test now has:';
PRINT '- 13 companies to work with (CR and SV)';
PRINT '- Various certification statuses to review';
PRINT '- Questionnaires in different stages';
PRINT '- Mix of completed and pending processes';
PRINT '========================================';