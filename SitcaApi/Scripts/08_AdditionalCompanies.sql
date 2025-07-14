-- Script 08: Additional Companies for All Countries
-- This script adds 3 companies per country (different typologies) for countries that don't have companies yet

-- Note: Using the country IDs from the database
-- Nicaragua = 5, Honduras = 4, Belize = 1, Panama = 7
-- Guatemala = 2 and El Salvador = 3 already have some companies, so we'll add more
-- Costa Rica = 6 already has companies

-- Get the next available company ID
DECLARE @NextId INT;
SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM Empresa;

SET IDENTITY_INSERT Empresa ON;

-- NICARAGUA (Country Id = 5) - 3 companies with different typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Hotel (Alojamiento)
(@NextId, 'Hotel Colonial Granada (Ambiente 1)', 'Carlos Eduardo Mendoza', 'Granada', 'info@colonialhotelgranada.ni', '+505 2552-7581', 'Calle La Calzada, frente al Parque Central', 5, 5, 1, 1.0, 'www.colonialhotelgranada.ni', 'Calle La Calzada', 'frente al Parque', '-85.9560', '11.9344', 'NI-001-2024', 'Gerente General', 'Certificado vigente', 'Hotel histórico', 0),
-- Restaurant (Restaurantes)
(@NextId + 1, 'Restaurante El Buen Gusto (Ambiente 2)', 'María Isabel López', 'Managua', 'reservas@elbuengusto.ni', '+505 2278-9999', 'Carretera Masaya km 4.5', 5, 5, 1, 2.0, 'www.elbuengusto.ni', 'Carretera Masaya', 'km 4.5', '-86.2504', '12.1328', 'NI-002-2024', 'Propietaria', 'En proceso', 'Gastronomía típica', 0),
-- Tour Operator (Operadoras de Turismo)
(@NextId + 2, 'Nicaragua Adventures Tours (Ambiente 3)', 'Roberto Castillo Ruiz', 'San Juan del Sur', 'tours@nicaraguaadventures.ni', '+505 2568-4444', 'Playa San Juan del Sur, zona central', 5, 5, 1, 1.0, 'www.nicaraguaadventures.ni', 'Playa San Juan del Sur', 'zona central', '-85.8705', '11.2529', 'NI-003-2024', 'Director', 'Certificado vigente', 'Tours de aventura', 0);

SET @NextId = @NextId + 3;

-- HONDURAS (Country Id = 4) - 3 companies with different typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Hotel (Alojamiento)
(@NextId, 'Hotel Bahía de Roatán', 'Pedro Antonio Flores', 'Roatán', 'info@bahiaroatan.hn', '+504 2445-3200', 'West Bay Beach, Roatán', 4, 4, 1, 1.0, 'www.bahiaroatan.hn', 'West Bay Beach', 's/n', '-86.5708', '16.3248', 'HN-001-2024', 'Gerente General', 'Certificado vigente', 'Resort de playa', 0),
-- Transport (Empresas de Transporte y Rent a car)
(@NextId + 1, 'Rent a Car Honduras Express', 'Ana Lucía Mejía', 'Tegucigalpa', 'rentals@hondurasexpress.hn', '+504 2239-8888', 'Aeropuerto Toncontín, Counter 5', 4, 4, 1, 2.0, 'www.hondurasexpress.hn', 'Aeropuerto Toncontín', 'Counter 5', '-87.2172', '14.0723', 'HN-002-2024', 'Gerente de Operaciones', 'En evaluación', 'Flota renovada', 0),
-- Thematic Activities (Actividades Temáticas)
(@NextId + 2, 'Copán Archaeological Tours', 'Miguel Ángel Rodríguez', 'Copán Ruinas', 'info@copantours.hn', '+504 2651-4000', 'Calle Principal, Copán Ruinas', 4, 4, 1, 1.0, 'www.copantours.hn', 'Calle Principal', 'Copán Ruinas', '-89.1567', '14.8375', 'HN-003-2024', 'Director', 'Certificado vigente', 'Tours arqueológicos', 0);

SET @NextId = @NextId + 3;

-- BELIZE (Country Id = 1) - 3 companies with different typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Hotel (Alojamiento)
(@NextId, 'Belize Barrier Reef Resort', 'John Williams', 'San Pedro', 'info@belizereef.bz', '+501 226-3500', 'Ambergris Caye, San Pedro Town', 1, 1, 1, 1.0, 'www.belizereef.bz', 'Ambergris Caye', 'San Pedro', '-87.9619', '17.9211', 'BZ-001-2024', 'General Manager', 'Certificado vigente', 'Diving resort', 0),
-- Restaurant (Restaurantes)
(@NextId + 1, 'Maya Kitchen Restaurant', 'Sofia Martinez', 'Belize City', 'reservations@mayakitchen.bz', '+501 223-7777', 'Marine Parade Boulevard, Belize City', 1, 1, 1, 2.0, 'www.mayakitchen.bz', 'Marine Parade Boulevard', 's/n', '-88.1962', '17.4995', 'BZ-002-2024', 'Owner', 'En proceso', 'Local cuisine', 0),
-- Tour Operator (Operadoras de Turismo)
(@NextId + 2, 'Jungle Adventures Belize', 'Michael Chen', 'San Ignacio', 'tours@jungleadventures.bz', '+501 824-2222', 'Burns Avenue, San Ignacio', 1, 1, 1, 1.0, 'www.jungleadventures.bz', 'Burns Avenue', 'San Ignacio', '-89.0686', '17.1599', 'BZ-003-2024', 'Operations Director', 'Certificado vigente', 'Eco-tours', 0);

SET @NextId = @NextId + 3;

-- PANAMA (Country Id = 7) - 3 companies with different typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Hotel (Alojamiento)
(@NextId, 'Hotel Casco Viejo Plaza', 'Ricardo Morales Díaz', 'Ciudad de Panamá', 'info@cascoviejo.pa', '+507 228-9900', 'Casco Viejo, Plaza Herrera', 7, 7, 1, 1.0, 'www.cascoviejo.pa', 'Plaza Herrera', 'Casco Viejo', '-79.5343', '8.9533', 'PA-001-2024', 'Gerente General', 'Certificado vigente', 'Hotel boutique', 0),
-- Transport (Empresas de Transporte y Rent a car)
(@NextId + 1, 'Panama Car Rental Premium', 'Gabriela Sánchez Luna', 'Ciudad de Panamá', 'rentals@panamacar.pa', '+507 263-5555', 'Aeropuerto de Tocumen, Terminal 1', 7, 7, 1, 2.0, 'www.panamacar.pa', 'Aeropuerto de Tocumen', 'Terminal 1', '-79.3833', '9.0713', 'PA-002-2024', 'Gerente de Flota', 'En evaluación', 'Servicio premium', 0),
-- Thematic Activities (Actividades Temáticas)
(@NextId + 2, 'Panama Canal Tours', 'Luis Fernando Vargas', 'Ciudad de Panamá', 'info@canaltourspa.com', '+507 314-1111', 'Miraflores Locks Visitor Center', 7, 7, 1, 1.0, 'www.canaltourspa.com', 'Miraflores Locks', 'Visitor Center', '-79.5943', '8.9936', 'PA-003-2024', 'Director de Tours', 'Certificado vigente', 'Tours del canal', 0);

SET @NextId = @NextId + 3;

-- Additional companies for GUATEMALA (Country Id = 2) - to complete 3 typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Restaurant (Restaurantes) - Guatemala needs this typology
(@NextId, 'Restaurante Pepián Tradicional', 'Marta Elena Quiñonez', 'Antigua Guatemala', 'reservas@pepiantradition.gt', '+502 7832-5555', '6a Calle Poniente #14', 2, 2, 1, 1.0, 'www.pepiantradition.gt', '6a Calle Poniente', '#14', '-90.7343', '14.5586', 'GT-003-2024', 'Propietaria', 'Certificado vigente', 'Cocina tradicional', 0);

SET @NextId = @NextId + 1;

-- Additional companies for EL SALVADOR (Country Id = 3) - to complete 3 typologies
INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Tour Operator (Operadoras de Turismo) - El Salvador needs this typology
(@NextId, 'Ruta Maya Tours El Salvador', 'Francisco Javier Molina', 'San Salvador', 'info@rutamayasv.com', '+503 2264-8888', 'Boulevard del Hipódromo, Colonia San Benito', 3, 3, 1, 2.0, 'www.rutamayasv.com', 'Boulevard del Hipódromo', 'Colonia San Benito', '-89.2182', '13.7042', 'SV-003-2024', 'Director General', 'En proceso', 'Tours culturales', 0);

SET IDENTITY_INSERT Empresa OFF;

-- Now create the relationships with Tipologias (TipologiasEmpresa table)
-- First, let's get the Tipologia IDs
DECLARE @TipologiaAlojamiento INT = 1;  -- Alojamiento
DECLARE @TipologiaRestaurante INT = 2;  -- Restaurantes
DECLARE @TipologiaOperadora INT = 3;    -- Operadoras de Turismo
DECLARE @TipologiaTransporte INT = 4;   -- Empresas de Transporte y Rent a car
DECLARE @TipologiaActividades INT = 5;  -- Actividades Temáticas

-- Insert the tipologia relationships for the new companies
-- Nicaragua companies
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaAlojamiento, Id FROM Empresa WHERE IdNacional = 'NI-001-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaRestaurante, Id FROM Empresa WHERE IdNacional = 'NI-002-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaOperadora, Id FROM Empresa WHERE IdNacional = 'NI-003-2024';

-- Honduras companies
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaAlojamiento, Id FROM Empresa WHERE IdNacional = 'HN-001-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaTransporte, Id FROM Empresa WHERE IdNacional = 'HN-002-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaActividades, Id FROM Empresa WHERE IdNacional = 'HN-003-2024';

-- Belize companies
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaAlojamiento, Id FROM Empresa WHERE IdNacional = 'BZ-001-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaRestaurante, Id FROM Empresa WHERE IdNacional = 'BZ-002-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaOperadora, Id FROM Empresa WHERE IdNacional = 'BZ-003-2024';

-- Panama companies
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaAlojamiento, Id FROM Empresa WHERE IdNacional = 'PA-001-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaTransporte, Id FROM Empresa WHERE IdNacional = 'PA-002-2024';
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaActividades, Id FROM Empresa WHERE IdNacional = 'PA-003-2024';

-- Additional Guatemala company
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaRestaurante, Id FROM Empresa WHERE IdNacional = 'GT-003-2024';

-- Additional El Salvador company
INSERT INTO TipologiasEmpresa (IdTipologia, IdEmpresa) 
SELECT @TipologiaOperadora, Id FROM Empresa WHERE IdNacional = 'SV-003-2024';

PRINT 'Additional companies created successfully:';
PRINT '  - Nicaragua: 3 companies (Hotel, Restaurant, Tour Operator)';
PRINT '  - Honduras: 3 companies (Hotel, Transport, Thematic Activities)';
PRINT '  - Belize: 3 companies (Hotel, Restaurant, Tour Operator)';
PRINT '  - Panama: 3 companies (Hotel, Transport, Thematic Activities)';
PRINT '  - Guatemala: +1 company (Restaurant) - Total 3 companies';
PRINT '  - El Salvador: +1 company (Tour Operator) - Total 3 companies';
PRINT '';
PRINT 'All countries now have companies with diverse typologies.';
