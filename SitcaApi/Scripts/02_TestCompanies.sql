-- Script 02: Test Companies (Empresas)
-- This script creates sample companies for testing
-- Note: Empresa.Id is auto-incremental, so we don't specify it

INSERT INTO Empresa (Nombre, NombreRepresentante, Ciudad, Email, Telefono, Direccion, IdPais, PaisId, Active, Estado, WebSite, Calle, Numero, Longitud, Latitud, IdNacional, CargoRepresentante, ResultadoActual, ResultadoSugerido, EsHomologacion) VALUES
-- Hotels (Alojamiento)
('Hotel Paradise Beach Resort', 'Juan Carlos Mendoza', 'San José', 'info@paradisebeach.cr', '+506 2234-5678', 'Avenida Central, 100m Norte del Teatro Nacional', 1, 1, 1, 2.0, 'www.paradisebeach.cr', 'Avenida Central', '100m Norte', '-84.0877', '9.9281', 'CR-001-2024', 'Gerente General', 'En proceso de certificación', 'Certificación recomendada', 0),
('Eco Lodge Monteverde', 'María Fernández López', 'Monteverde', 'reservas@ecolodgemonteverde.cr', '+506 2645-7890', 'Carretera a Santa Elena, km 5', 1, 1, 1, 1.0, 'www.ecolodgemonteverde.cr', 'Carretera a Santa Elena', 'km 5', '-84.8169', '10.3181', 'CR-002-2024', 'Directora', 'Certificado vigente', 'Mantener certificación', 0),
('Business Hotel Central', 'Roberto Vargas Solis', 'San José', 'gerencia@hotelcentral.cr', '+506 2222-3333', 'Paseo Colón, frente al edificio Torre Mercedes', 1, 1, 1, 3.0, 'www.hotelcentral.cr', 'Paseo Colón', 's/n', '-84.0877', '9.9281', 'CR-003-2024', 'Gerente', 'Pendiente documentación', 'Completar requisitos', 0),

-- Restaurants
('Restaurante La Costa Brava', 'Ana Patricia Rojas', 'Manuel Antonio', 'contacto@lacostabrava.cr', '+506 2777-8888', 'Playa Espadilla Sur, local 15', 1, 1, 1, 2.0, 'www.lacostabrava.cr', 'Playa Espadilla Sur', 'local 15', '-84.1506', '9.3963', 'CR-004-2024', 'Propietaria', 'En evaluación', 'Certificación en proceso', 0),
('Soda Típica El Cafetal', 'Carlos Jiménez Mora', 'Alajuela', 'info@elcafetal.cr', '+506 2441-5555', 'Centro de Alajuela, 50m sur del Parque Central', 1, 1, 1, 1.0, '', 'Centro de Alajuela', '50m sur', '-84.2164', '10.0162', 'CR-005-2024', 'Propietario', 'Certificado vigente', 'Renovar en 2025', 0),

-- Tour Operators
('Costa Rica Adventures', 'Luis Alberto Campos', 'La Fortuna', 'tours@costaricaadventures.cr', '+506 2479-9999', 'La Fortuna, frente al Parque Central', 1, 1, 1, 1.0, 'www.costaricaadventures.cr', 'La Fortuna', 'frente al Parque', '-84.6431', '10.4667', 'CR-006-2024', 'Director de Operaciones', 'Certificado vigente', 'Excelente estado', 0),
('Tropical Tours CR', 'Sofía Gutiérrez Pérez', 'Tamarindo', 'info@tropicaltours.cr', '+506 2653-4444', 'Tamarindo Beach, Plaza Conchal', 1, 1, 1, 2.0, 'www.tropicaltours.cr', 'Tamarindo Beach', 'Plaza Conchal', '-85.8439', '10.2998', 'CR-007-2024', 'Gerente', 'En proceso', 'Documentación pendiente', 0),

-- Transport Companies
('Rent a Car Premium', 'Diego Ramírez Castro', 'San José', 'reservas@rentacarpremium.cr', '+506 2290-8888', 'Aeropuerto Juan Santamaría, counter 12', 1, 1, 1, 3.0, 'www.rentacarpremium.cr', 'Aeropuerto Juan Santamaría', 'counter 12', '-84.2139', '9.9937', 'CR-008-2024', 'Gerente de Operaciones', 'Pendiente revisión', 'Completar documentos', 0),
('Shuttle Express CR', 'Patricia Monge Arias', 'Liberia', 'servicios@shuttleexpress.cr', '+506 2666-7777', 'Liberia, 200m oeste del aeropuerto', 1, 1, 1, 1.0, 'www.shuttleexpress.cr', 'Liberia', '200m oeste', '-85.4378', '10.5926', 'CR-009-2024', 'Directora', 'Certificado vigente', 'Excelente servicio', 0),

-- Thematic Activities
('Canopy Extremo', 'Fernando Solís Vega', 'Monteverde', 'info@canopyextremo.cr', '+506 2645-6666', 'Monteverde, entrada principal', 1, 1, 1, 1.0, 'www.canopyextremo.cr', 'Monteverde', 'entrada principal', '-84.8169', '10.3181', 'CR-010-2024', 'Propietario', 'Certificado vigente', 'Seguridad excelente', 0),
('Aguas Termales Paradise', 'Laura Vindas Rodríguez', 'La Fortuna', 'reservas@aguastermales.cr', '+506 2479-5555', 'La Fortuna, camino al Volcán Arenal km 8', 1, 1, 1, 2.0, 'www.aguastermales.cr', 'Camino al Volcán Arenal', 'km 8', '-84.6431', '10.4667', 'CR-011-2024', 'Administradora', 'En evaluación', 'Proceso avanzado', 0),

-- Companies from Guatemala (Country Id = 2)
('Hotel Colonial Antigua', 'Ricardo Martínez Pérez', 'Antigua Guatemala', 'info@colonialantigua.gt', '+502 7832-1234', '5a Avenida Norte #12, Antigua', 2, 2, 1, 1.0, 'www.colonialantigua.gt', '5a Avenida Norte', '#12', '-90.7343', '14.5586', 'GT-001-2024', 'Director General', 'Certificado vigente', 'Patrimonio conservado', 0),
('Maya Tours Guatemala', 'Carmen López de García', 'Ciudad de Guatemala', 'tours@mayatours.gt', '+502 2368-9999', 'Zona 10, Edificio Reforma Tower', 2, 2, 1, 2.0, 'www.mayatours.gt', 'Zona 10', 'Edificio Reforma Tower', '-90.5069', '14.6349', 'GT-002-2024', 'Gerente de Ventas', 'En proceso', 'Tours culturales', 0),

-- Companies from El Salvador (Country Id = 3)
('Beach Resort El Salvador', 'José Manuel Hernández', 'La Libertad', 'contacto@beachresort.sv', '+503 2346-7890', 'Playa El Tunco, km 42', 3, 3, 1, 3.0, 'www.beachresort.sv', 'Playa El Tunco', 'km 42', '-89.4094', '13.4894', 'SV-001-2024', 'Gerente General', 'Pendiente documentos', 'Resort playero', 0),
('Pupusería Gourmet', 'Rosa María Vásquez', 'San Salvador', 'info@pupuseriagourmet.sv', '+503 2222-5555', 'Colonia Escalón, Paseo General Escalón', 3, 3, 1, 1.0, '', 'Colonia Escalón', 'Paseo General Escalón', '-89.2182', '13.7042', 'SV-002-2024', 'Propietaria', 'Certificado vigente', 'Calidad gourmet', 0);

PRINT 'Test companies inserted successfully (using auto-incremental IDs)';