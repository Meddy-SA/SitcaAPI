-- Script 02: Test Companies (Empresas)
-- This script creates sample companies for testing

-- Insert test companies for Costa Rica (Country Id = 1)
SET IDENTITY_INSERT Empresa ON;

INSERT INTO Empresa (Id, Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, Active, Estado, SitioWeb, Descripcion, CreatedAt, UpdatedAt, Enabled) VALUES
-- Hotels (Alojamiento)
(1, 'Hotel Paradise Beach Resort', 'Juan Carlos Mendoza', 'San José', 'info@paradisebeach.cr', '+506 2234-5678', 'Avenida Central, 100m Norte del Teatro Nacional', 1, 1, 'En Proceso', 'www.paradisebeach.cr', 'Resort de playa con 150 habitaciones y servicios completos', GETDATE(), GETDATE(), 1),
(2, 'Eco Lodge Monteverde', 'María Fernández López', 'Monteverde', 'reservas@ecolodgemonteverde.cr', '+506 2645-7890', 'Carretera a Santa Elena, km 5', 1, 1, 'Certificada', 'www.ecolodgemonteverde.cr', 'Lodge ecológico en el bosque nuboso', GETDATE(), GETDATE(), 1),
(3, 'Business Hotel Central', 'Roberto Vargas Solis', 'San José', 'gerencia@hotelcentral.cr', '+506 2222-3333', 'Paseo Colón, frente al edificio Torre Mercedes', 1, 1, 'Pendiente', 'www.hotelcentral.cr', 'Hotel de negocios en el centro de la capital', GETDATE(), GETDATE(), 1),

-- Restaurants
(4, 'Restaurante La Costa Brava', 'Ana Patricia Rojas', 'Manuel Antonio', 'contacto@lacostabrava.cr', '+506 2777-8888', 'Playa Espadilla Sur, local 15', 1, 1, 'En Proceso', 'www.lacostabrava.cr', 'Restaurante de mariscos con vista al mar', GETDATE(), GETDATE(), 1),
(5, 'Soda Típica El Cafetal', 'Carlos Jiménez Mora', 'Alajuela', 'info@elcafetal.cr', '+506 2441-5555', 'Centro de Alajuela, 50m sur del Parque Central', 1, 1, 'Certificada', NULL, 'Comida típica costarricense', GETDATE(), GETDATE(), 1),

-- Tour Operators
(6, 'Costa Rica Adventures', 'Luis Alberto Campos', 'La Fortuna', 'tours@costaricaadventures.cr', '+506 2479-9999', 'La Fortuna, frente al Parque Central', 1, 1, 'Certificada', 'www.costaricaadventures.cr', 'Operadora de tours de aventura y naturaleza', GETDATE(), GETDATE(), 1),
(7, 'Tropical Tours CR', 'Sofía Gutiérrez Pérez', 'Tamarindo', 'info@tropicaltours.cr', '+506 2653-4444', 'Tamarindo Beach, Plaza Conchal', 1, 1, 'En Proceso', 'www.tropicaltours.cr', 'Tours en playa y actividades acuáticas', GETDATE(), GETDATE(), 1),

-- Transport Companies
(8, 'Rent a Car Premium', 'Diego Ramírez Castro', 'San José', 'reservas@rentacarpremium.cr', '+506 2290-8888', 'Aeropuerto Juan Santamaría, counter 12', 1, 1, 'Pendiente', 'www.rentacarpremium.cr', 'Alquiler de vehículos premium y económicos', GETDATE(), GETDATE(), 1),
(9, 'Shuttle Express CR', 'Patricia Monge Arias', 'Liberia', 'servicios@shuttleexpress.cr', '+506 2666-7777', 'Liberia, 200m oeste del aeropuerto', 1, 1, 'Certificada', 'www.shuttleexpress.cr', 'Servicio de transporte turístico', GETDATE(), GETDATE(), 1),

-- Thematic Activities
(10, 'Canopy Extremo', 'Fernando Solís Vega', 'Monteverde', 'info@canopyextremo.cr', '+506 2645-6666', 'Monteverde, entrada principal', 1, 1, 'Certificada', 'www.canopyextremo.cr', 'Canopy tours y puentes colgantes', GETDATE(), GETDATE(), 1),
(11, 'Aguas Termales Paradise', 'Laura Vindas Rodríguez', 'La Fortuna', 'reservas@aguastermales.cr', '+506 2479-5555', 'La Fortuna, camino al Volcán Arenal km 8', 1, 1, 'En Proceso', 'www.aguastermales.cr', 'Complejo de aguas termales y spa', GETDATE(), GETDATE(), 1),

-- Companies from Guatemala (Country Id = 2)
(12, 'Hotel Colonial Antigua', 'Ricardo Martínez Pérez', 'Antigua Guatemala', 'info@colonialantigua.gt', '+502 7832-1234', '5a Avenida Norte #12, Antigua', 2, 1, 'Certificada', 'www.colonialantigua.gt', 'Hotel boutique en el centro histórico', GETDATE(), GETDATE(), 1),
(13, 'Maya Tours Guatemala', 'Carmen López de García', 'Ciudad de Guatemala', 'tours@mayatours.gt', '+502 2368-9999', 'Zona 10, Edificio Reforma Tower', 2, 1, 'En Proceso', 'www.mayatours.gt', 'Tours arqueológicos y culturales', GETDATE(), GETDATE(), 1),

-- Companies from El Salvador (Country Id = 3)
(14, 'Beach Resort El Salvador', 'José Manuel Hernández', 'La Libertad', 'contacto@beachresort.sv', '+503 2346-7890', 'Playa El Tunco, km 42', 3, 1, 'Pendiente', 'www.beachresort.sv', 'Resort frente al mar con surf point', GETDATE(), GETDATE(), 1),
(15, 'Pupusería Gourmet', 'Rosa María Vásquez', 'San Salvador', 'info@pupuseriagourmet.sv', '+503 2222-5555', 'Colonia Escalón, Paseo General Escalón', 3, 1, 'Certificada', NULL, 'Restaurante de comida típica salvadoreña', GETDATE(), GETDATE(), 1);

SET IDENTITY_INSERT Empresa OFF;

-- Associate companies with their business types (Tipologias)
INSERT INTO TipologiasEmpresa (EmpresaId, TipologiaId) VALUES
-- Hotels
(1, 1), -- Paradise Beach - Alojamiento
(2, 1), -- Eco Lodge - Alojamiento
(3, 1), -- Business Hotel - Alojamiento
(12, 1), -- Colonial Antigua - Alojamiento
(14, 1), -- Beach Resort SV - Alojamiento

-- Restaurants
(4, 2), -- La Costa Brava - Restaurantes
(5, 2), -- Soda Típica - Restaurantes
(15, 2), -- Pupusería - Restaurantes

-- Tour Operators
(6, 3), -- CR Adventures - Operadoras
(7, 3), -- Tropical Tours - Operadoras
(13, 3), -- Maya Tours - Operadoras

-- Transport
(8, 4), -- Rent a Car - Transporte
(9, 4), -- Shuttle Express - Transporte

-- Thematic Activities
(10, 5), -- Canopy Extremo - Actividades
(11, 5); -- Aguas Termales - Actividades

-- Some companies might have multiple types
INSERT INTO TipologiasEmpresa (EmpresaId, TipologiaId) VALUES
(1, 2), -- Paradise Beach also has restaurant
(14, 2), -- Beach Resort SV also has restaurant
(6, 5); -- CR Adventures also offers thematic activities

PRINT 'Test companies inserted successfully';