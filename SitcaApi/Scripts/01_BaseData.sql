-- Script 01: Base Data - Countries and Tipologies
-- This script creates the base data needed for the test environment

-- Insert Countries (Central American countries)
-- INSERT INTO Pais (Id, Name, Active) VALUES 
-- (1, 'Costa Rica', 1),
-- (2, 'Guatemala', 1),
-- (3, 'El Salvador', 1),
-- (4, 'Honduras', 1),
-- (5, 'Nicaragua', 1),
-- (6, 'Panamá', 1),
-- (7, 'Belice', 1);
--
-- -- Insert Tipologies (Business Types)
-- SET IDENTITY_INSERT Tipologia ON;
--
-- INSERT INTO Tipologia (Id, Name, NameEnglish, Active) VALUES
-- (1, 'Alojamiento', 'Accommodation', 1),
-- (2, 'Restaurantes', 'Restaurants', 1),
-- (3, 'Operadoras de Turismo', 'Tour Operators', 1),
-- (4, 'Empresas de Transporte y Rent a car', 'Transport and Car Rental Companies', 1),
-- (5, 'Actividades Temáticas', 'Thematic Activities', 1);
--
-- SET IDENTITY_INSERT Tipologia OFF;

-- Insert some additional useful data that might be needed

-- Insert Estados (States/Status) if needed
-- You may need to adjust this based on your actual Estados table structure
/*
INSERT INTO Estados (Id, Nombre, Descripcion, Active) VALUES
(1, 'En Proceso', 'Certificación en proceso', 1),
(2, 'Certificada', 'Empresa certificada', 1),
(3, 'Pendiente', 'Pendiente de revisión', 1),
(4, 'Rechazada', 'Certificación rechazada', 1),
(5, 'Vencida', 'Certificación vencida', 1);
*/

PRINT 'Base data inserted successfully';
