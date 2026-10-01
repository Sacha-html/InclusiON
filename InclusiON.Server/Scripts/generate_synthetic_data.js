// generate_synthetic_data.js
// Genera un script SQL transaccional y coherente con las especificaciones exactas del usuario:
// - 6 Profesionales (Sacha + 5 nuevos con nombres reales, especialidades, matrículas y DNI).
// - 18 Aulas (A, B, C para cada uno de los 6 profesionales).
// - Entre 7 y 9 alumnos por aula (máximo 10 por aula).
// - Nombres, apellidos, emails y DNIs argentinos reales y limpios (sin caracteres que generen ??).
// - Perfil funcional completo para cada alumno (atención, comunicación, motricidad, adaptaciones sensoriales, diagnóstico clínico).
// - Roadmaps de 10 niveles inicializados para cada alumno.
// - Métricas (ActivitySessions) secuenciales por categoría y nivel con casos de éxito y alertas de frustración.
// - Reportes distribuidos en Draft, Submitted, Approved y Rejected para cada profesional.

const fs = require('fs');
const path = require('path');

function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

// Datos de referencia fijos
const ADMIN_USER_ID = '00000000-0000-0000-0000-000000000001';
const ROLE_ADMIN = '11111111-1111-1111-1111-111111111111';
const ROLE_PROFESSIONAL = '22222222-2222-2222-2222-222222222222';
const ROLE_TUTOR = '33333333-3333-3333-3333-333333333333';
const ROLE_STUDENT = '44444444-4444-4444-4444-444444444444';

// Hashes estándar ASP.NET Identity (Admin123!)
const PWD_HASH_PROF = 'AQAAAAIAAYagAAAAECZ1ffLYY9bgdZS3AZkFmZ3irOqypqKXsA4oXTab+jT7MRGw3Nc6Ek8iqY+7VsTuBg==';
const PWD_HASH_TUTOR = 'AQAAAAIAAYagAAAAECkq8gRB2gNVjHRzxcjGKZ5sXfSiVHfy/cHredgMBhPmWcfSNQ2AlxejDOjzUP8GGw==';
const PWD_HASH_STUDENT = 'AQAAAAIAAYagAAAAEG2sz3MvQvnF9f7zQRvmJ1RcPjZkMhOf0SQUQxD3GsfH9sa8HaC8OcA1avMOnJDYTA==';

// Actividades estándar del Roadmap (1 al 10)
const ROADMAP_ACTIVITIES = [
  { id: 85, seq: 1, title: 'Armando el vaso', cat: 21 },
  { id: 86, seq: 2, title: 'Necesito ir al bano', cat: 20 },
  { id: 87, seq: 3, title: 'Quien se rie', cat: 19 },
  { id: 88, seq: 4, title: 'Buscando la MESA', cat: 17 },
  { id: 89, seq: 5, title: 'Contando galletas', cat: 18 },
  { id: 90, seq: 6, title: 'Me sirvo agua', cat: 23 },
  { id: 91, seq: 7, title: 'Donde hacemos pis', cat: 19 },
  { id: 92, seq: 8, title: 'A ordenar la cocina', cat: 18 },
  { id: 93, seq: 9, title: 'Las letras de MESA', cat: 17 },
  { id: 94, seq: 10, title: 'La rutina de la manana', cat: 23 }
];

// 6 Profesionales (Sacha + 5 nuevos con nombres limpios y reales)
const PROFESSIONALS = [
  {
    isExisting: true,
    profId: '1033fcd8-8acb-42e2-ba94-1feba90ebecb',
    userId: '3e3a88e9-7d3e-4c19-9bd5-8b3731d6e16d',
    firstName: 'SACHA',
    lastName: 'DELBARRIO',
    email: 'sacha.delbarrio@test.com',
    specialtyId: 15,
    specialtyName: 'Psicologia',
    license: 'MN-45920',
    existingClassroomAId: 'fbaeaaa1-3db7-41b9-8e24-255e08b4f977'
  },
  {
    isExisting: false,
    profId: '22220001-0000-4000-8000-000000000001',
    userId: '22220001-0000-4000-8000-000000000002',
    firstName: 'Sofia Valentina',
    lastName: 'Gutierrez',
    dni: '34819204',
    phone: '+54 9 11 4920-3184',
    email: 'sofia.gutierrez@inclusion.edu.ar',
    specialtyId: 15, // Psicología
    specialtyName: 'Psicologia',
    license: 'MN-48215',
    birthDate: '1989-05-14T00:00:00Z'
  },
  {
    isExisting: false,
    profId: '22220002-0000-4000-8000-000000000001',
    userId: '22220002-0000-4000-8000-000000000002',
    firstName: 'Pedro Ignacio',
    lastName: 'Martinez',
    dni: '32491802',
    phone: '+54 9 11 5821-9043',
    email: 'pedro.martinez@inclusion.edu.ar',
    specialtyId: 16, // Psicopedagogía
    specialtyName: 'Psicopedagogia',
    license: 'MP-12940',
    birthDate: '1986-11-20T00:00:00Z'
  },
  {
    isExisting: false,
    profId: '22220003-0000-4000-8000-000000000001',
    userId: '22220003-0000-4000-8000-000000000002',
    firstName: 'Camila Lucia',
    lastName: 'Benitez',
    dni: '36210455',
    phone: '+54 9 11 6302-8819',
    email: 'camila.benitez@inclusion.edu.ar',
    specialtyId: 17, // Docente de Apoyo a la Inclusión
    specialtyName: 'Docente de Apoyo a la Inclusion',
    license: 'MN-51029',
    birthDate: '1991-03-08T00:00:00Z'
  },
  {
    isExisting: false,
    profId: '22220004-0000-4000-8000-000000000001',
    userId: '22220004-0000-4000-8000-000000000002',
    firstName: 'Martin Alejandro',
    lastName: 'Gomez',
    dni: '31782340',
    phone: '+54 9 11 4109-7722',
    email: 'martin.gomez@inclusion.edu.ar',
    specialtyId: 14, // Educación Especial
    specialtyName: 'Educacion Especial',
    license: 'MP-09823',
    birthDate: '1985-08-27T00:00:00Z'
  },
  {
    isExisting: false,
    profId: '22220005-0000-4000-8000-000000000001',
    userId: '22220005-0000-4000-8000-000000000002',
    firstName: 'Luciana Mariel',
    lastName: 'Rossi',
    dni: '35604119',
    phone: '+54 9 11 5019-3381',
    email: 'luciana.rossi@inclusion.edu.ar',
    specialtyId: 15, // Psicología
    specialtyName: 'Psicologia',
    license: 'MN-63102',
    birthDate: '1990-12-04T00:00:00Z'
  }
];

// Nombres y apellidos argentinos realistas sin caracteres acentuados que se degraden
const FIRST_NAMES_M = ['Mateo', 'Santiago', 'Benjamin', 'Lucas', 'Thiago', 'Joaquin', 'Bautista', 'Tomas', 'Facundo', 'Lautaro', 'Felipe', 'Nicolas', 'Agustin', 'Santino', 'Ignacio', 'Manuel', 'Franco', 'Emiliano', 'Julian', 'Leandro'];
const FIRST_NAMES_F = ['Emma', 'Olivia', 'Martina', 'Isabella', 'Sofia', 'Catalina', 'Mia', 'Julieta', 'Delfina', 'Valentina', 'Renata', 'Zoe', 'Elena', 'Victoria', 'Lucia', 'Abril', 'Emilia', 'Guadalupe', 'Camila', 'Paula'];
const SURNAMES = ['Gonzalez', 'Rodriguez', 'Gomez', 'Fernandez', 'Lopez', 'Diaz', 'Martinez', 'Perez', 'Romero', 'Sanchez', 'Sosa', 'Alvarez', 'Torres', 'Ruiz', 'Ramirez', 'Flores', 'Acosta', 'Benitez', 'Medina', 'Herrera', 'Aguirre', 'Pereyra', 'Castro', 'Molina', 'Ortiz', 'Silva', 'Nunez', 'Luna', 'Juarez', 'Cabrera', 'Rios', 'Morales', 'Godoy', 'Moreno', 'Ferreyra', 'Dominguez', 'Carrizo', 'Vega', 'Castillo', 'Ojeda'];

const DIAGNOSES_TEMPLATES = [
  {
    typeId: 12, // Cognitiva/Intelectual
    autonomyId: 8, // Media
    diagnosis: 'Trastorno del Espectro Autista (TEA) - Grado 1 con requerimiento de apoyo en comunicacion',
    supports: 'Uso de sistema CAA digital, agendas visuales de anticipacion y consignas directas.',
    objectives: 'Incrementar vocabulario nucleo mediante pictogramas y afianzar turnos de espera en el aula.',
    strategies: 'Apoyo con reforzadores visuales inmediatos y pausas activas programadas.',
    capabilities: 'Destacada memoria visual para esquemas y reconocimiento rapido de iconos.',
    challenges: 'Sobrecarga sensorial ante ambientes con reverberacion sonora excesiva.',
    notes: 'Responde con entusiasmo a las dinamicas de emparejamiento visual y rutinas predecibles.'
  },
  {
    typeId: 12, // Cognitiva/Intelectual
    autonomyId: 7, // Alta
    diagnosis: 'TDAH con predominio hiperactivo-impulsivo y dificultades en funcion ejecutiva',
    supports: 'Segmentacion de tareas complejas en micro-pasos y temporizadores visuales.',
    objectives: 'Fomentar la autorregulacion en momentos de transicion y tolerancia a la frustracion.',
    strategies: 'Utilizacion de recompensas contingentes y descansos motores breves.',
    capabilities: 'Alta creatividad y participacion entusiasta en propuestas grupales.',
    challenges: 'Dificultad para mantener el foco atencional en tareas repetitivas mayores a 15 minutos.',
    notes: 'Se beneficia de la estructura del Roadmap secuencial para visualizar su propia meta.'
  },
  {
    typeId: 11, // Sensorial
    autonomyId: 8, // Media
    diagnosis: 'Hipoacusia neurosensorial moderada bilateral compensada con audifonos digitales',
    supports: 'Soporte visual prioritario de alto contraste y transcripcion mediante pictogramas.',
    objectives: 'Consolidar la comprension lectoescritora y discriminacion visomotriz fina.',
    strategies: 'Ubicacion preferencial en el aula y empleo de material con tipografia legible.',
    capabilities: 'Gran capacidad de atencion sostenida ante estimulos graficos bien contrastados.',
    challenges: 'Perdida de informacion en entornos con ruido de fondo elevado.',
    notes: 'Interactua de forma autonoma con la tablet adaptada.'
  },
  {
    typeId: 13, // Motriz
    autonomyId: 8, // Media
    diagnosis: 'Paralisis Cerebral espastica con compromiso motor predominantemente braquial derecho',
    supports: 'Pulsador ergonomico accesible para mano izquierda y soporte vertical para tablet.',
    objectives: 'Lograr precision motriz en la seleccion de opciones en pantalla tactil.',
    strategies: 'Ampliacion de areas tactiles en botones y tiempo extendido de respuesta.',
    capabilities: 'Excelente comprension cognitiva y motivacion para superar desafios propuestos.',
    challenges: 'Fatiga muscular motriz ante sesiones prolongadas sin descansos intermedios.',
    notes: 'Demuestra perseverancia y gran satisfaccion al completar las actividades.'
  },
  {
    typeId: 15, // Múltiple
    autonomyId: 9, // Baja
    diagnosis: 'Sindrome de Down con compromiso del lenguaje expresivo y retraso madurativo leve',
    supports: 'Sistemas de Comunicacion Aumentativa y Alternativa (CAA) de barrido visual asistido.',
    objectives: 'Fomentar la expresion de necesidades fisiologicas basicas y preferencias cotidianas.',
    strategies: 'Modelado por parte del docente e imitacion de secuencias paso a paso.',
    capabilities: 'Gran empatia social y disposicion al trabajo cooperativo guiado.',
    challenges: 'Articulacion fonologica compleja; requiere soporte grafico constante.',
    notes: 'El uso diario de la tablet ha facilitado su comunicacion espontanea con su familia.'
  },
  {
    typeId: 14, // Mental/Psicosocial
    autonomyId: 8, // Media
    diagnosis: 'Trastorno de Ansiedad Social en el ambito escolar con mutismo selectivo parcial',
    supports: 'Interaccion no verbal a traves de tableros de comunicacion y respuesta digital.',
    objectives: 'Promover la comunicacion activa sin exigencia de respuesta verbal obligatoria.',
    strategies: 'Validacion emocional continua y creacion de un clima de aula seguro y libre de presion.',
    capabilities: 'Rendimiento academico sobresaliente en tareas estructuradas individuales.',
    challenges: 'Inhibicion acentuada en presencia de observadores externos o cambios imprevistos.',
    notes: 'El Roadmap digital le brinda un canal seguro de expresion y progreso.'
  }
];

const TUTOR_RELATIONSHIPS = ['Madre', 'Padre', 'Tutor legal', 'Abuela', 'Abuelo'];

console.log('Iniciando generador de datos sintéticos limpios...');

let dniCounter = 45100200;
let tutorDniCounter = 31200100;
let phoneCounter = 49001000;

const sqlStatements = [];

sqlStatements.push(`-- ============================================================================`);
sqlStatements.push(`-- INCLUSION: CARGA INTEGRAL DE DATOS SINTETICOS PARA DEMO (100% LIMPIA)`);
sqlStatements.push(`-- 6 Profesionales, 18 Aulas (A, B, C), ~140 Alumnos, Métricas y Reportes`);
sqlStatements.push(`-- Nombres, apellidos y emails 100% naturales sin caracteres corrompidos`);
sqlStatements.push(`-- ============================================================================`);
sqlStatements.push(``);
sqlStatements.push(`BEGIN;`);
sqlStatements.push(``);

// 0. Limpieza previa de la tanda sintética para garantizar un estado 100% fresco e idempotente
sqlStatements.push(`-- 0. LIMPIEZA PREVIA DE REGISTROS SINTETICOS`);
sqlStatements.push(`
DELETE FROM "Reports" WHERE "Title" LIKE 'Informe P%';

DELETE FROM "ActivitySessions" WHERE "ProfessionalId" IN (
  '22220001-0000-4000-8000-000000000001',
  '22220002-0000-4000-8000-000000000001',
  '22220003-0000-4000-8000-000000000001',
  '22220004-0000-4000-8000-000000000001',
  '22220005-0000-4000-8000-000000000001'
) OR "StudentId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "PersonRoadmapActivities" WHERE "PersonRoadmapAreaId" IN (
  SELECT pra."Id" FROM "PersonRoadmapAreas" pra
  JOIN "PersonRoadmaps" prm ON pra."PersonRoadmapId" = prm."Id"
  WHERE prm."PersonId" NOT IN (
    'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
    'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
    '10d44b4f-a0ad-484d-b4a0-76d210118caa',
    'a20e9c0a-3ab9-4d14-a00e-50311f731027',
    '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
  )
);

DELETE FROM "PersonRoadmapAreas" WHERE "PersonRoadmapId" IN (
  SELECT prm."Id" FROM "PersonRoadmaps" prm
  WHERE prm."PersonId" NOT IN (
    'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
    'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
    '10d44b4f-a0ad-484d-b4a0-76d210118caa',
    'a20e9c0a-3ab9-4d14-a00e-50311f731027',
    '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
  )
);

DELETE FROM "PersonRoadmaps" WHERE "PersonId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "PersonEmbeddings" WHERE "PersonId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "Diagnoses" WHERE "PersonId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "ProfessionalPersons" WHERE "PersonId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "PersonRepresentatives" WHERE "PersonId" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "PersonsWithDisability" WHERE "Id" NOT IN (
  'e40fca9c-3180-4ee6-9e5e-9d2f83fe5125',
  'e6a2a47c-622b-4f8d-9f75-270eb7505a68',
  '10d44b4f-a0ad-484d-b4a0-76d210118caa',
  'a20e9c0a-3ab9-4d14-a00e-50311f731027',
  '92eb7d77-a444-45d4-8ba9-7c7886c2cb01'
);

DELETE FROM "FamilyRepresentatives" WHERE "UserId" NOT IN (
  'dc8143c3-a6f5-4f61-81d6-88fca7010df3',
  '9ea79eab-f3a2-4d7d-aec5-3d1695a3e0fb',
  '1ad3f509-af3d-4881-bd8b-6a2da9925b53',
  'bf40eb23-3085-4875-a93e-b2c596025cec',
  '39193bcf-85a8-45e9-acf5-1b99e2577df8'
);

DELETE FROM "Classrooms" WHERE "Id" <> 'fbaeaaa1-3db7-41b9-8e24-255e08b4f977';

DELETE FROM "Professionals" WHERE "Id" IN (
  '22220001-0000-4000-8000-000000000001',
  '22220002-0000-4000-8000-000000000001',
  '22220003-0000-4000-8000-000000000001',
  '22220004-0000-4000-8000-000000000001',
  '22220005-0000-4000-8000-000000000001'
);

DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (
  SELECT "Id" FROM "Users" WHERE "Id" NOT IN (
    '00000000-0000-0000-0000-000000000001',
    '3e3a88e9-7d3e-4c19-9bd5-8b3731d6e16d',
    'dc8143c3-a6f5-4f61-81d6-88fca7010df3',
    'c55cf8a3-c159-483c-8f7d-608fac02a400',
    '9ea79eab-f3a2-4d7d-aec5-3d1695a3e0fb',
    'e2195831-c357-4b0f-a24d-23af49f73c5e',
    '1ad3f509-af3d-4881-bd8b-6a2da9925b53',
    '71387958-d065-48a8-ac6d-33d2ec688793',
    '98defa16-a484-4967-b09b-970850491ffd',
    'bf40eb23-3085-4875-a93e-b2c596025cec',
    '39193bcf-85a8-45e9-acf5-1b99e2577df8',
    '16c5d01c-7a02-438f-bebd-9e2a90f97113'
  )
);

DELETE FROM "Users" WHERE "Id" NOT IN (
  '00000000-0000-0000-0000-000000000001',
  '3e3a88e9-7d3e-4c19-9bd5-8b3731d6e16d',
  'dc8143c3-a6f5-4f61-81d6-88fca7010df3',
  'c55cf8a3-c159-483c-8f7d-608fac02a400',
  '9ea79eab-f3a2-4d7d-aec5-3d1695a3e0fb',
  'e2195831-c357-4b0f-a24d-23af49f73c5e',
  '1ad3f509-af3d-4881-bd8b-6a2da9925b53',
  '71387958-d065-48a8-ac6d-33d2ec688793',
  '98defa16-a484-4967-b09b-970850491ffd',
  'bf40eb23-3085-4875-a93e-b2c596025cec',
  '39193bcf-85a8-45e9-acf5-1b99e2577df8',
  '16c5d01c-7a02-438f-bebd-9e2a90f97113'
);
`);

// 1. Crear los 5 nuevos profesionales y sus usuarios
sqlStatements.push(`-- 1. CREACION DE LOS 5 NUEVOS PROFESIONALES`);
for (const p of PROFESSIONALS) {
  if (p.isExisting) continue;

  const userEmailUpper = p.email.toUpperCase();
  sqlStatements.push(`
INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "Name", "Surname", "CreatedAt", "IsActive", "MustChangePassword")
VALUES ('${p.userId}', '${p.email}', '${userEmailUpper}', '${p.email}', '${userEmailUpper}', true, '${PWD_HASH_PROF}', '${uuidv4()}', '${uuidv4()}', '${p.phone}', true, false, true, 0, '${p.firstName}', '${p.lastName}', NOW(), true, false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('${p.userId}', '${ROLE_PROFESSIONAL}')
ON CONFLICT DO NOTHING;

INSERT INTO "Professionals" ("Id", "UserId", "FirstName", "LastName", "DocumentNumber", "Phone", "Email", "Specialty", "SpecialtyId", "LicenseNumber", "BirthDate", "Status", "CreatedAt", "CreatedBy", "IsActive")
VALUES ('${p.profId}', '${p.userId}', '${p.firstName}', '${p.lastName}', '${p.dni}', '${p.phone}', '${p.email}', '${p.specialtyName}', ${p.specialtyId}, '${p.license}', '${p.birthDate}', 1, NOW(), '${ADMIN_USER_ID}', true)
ON CONFLICT ("Id") DO NOTHING;
`);
}

// 2. Crear las 18 Aulas (A, B, C para cada profesional)
sqlStatements.push(`-- 2. CREACION DE LAS 18 AULAS (A, B, C PARA CADA PROFESIONAL)`);
const CLASSROOMS = [];

for (const p of PROFESSIONALS) {
  if (p.isExisting) {
    CLASSROOMS.push({
      id: p.existingClassroomAId,
      name: 'Aula A',
      profId: p.profId,
      isExisting: true
    });
    const aulaBId = uuidv4();
    const aulaCId = uuidv4();
    CLASSROOMS.push({ id: aulaBId, name: 'Aula B', profId: p.profId, isExisting: false });
    CLASSROOMS.push({ id: aulaCId, name: 'Aula C', profId: p.profId, isExisting: false });

    sqlStatements.push(`
INSERT INTO "Classrooms" ("Id", "Name", "ProfessionalId", "IsActive", "CreatedAt", "CreatedBy")
VALUES ('${aulaBId}', 'Aula B', '${p.profId}', true, NOW(), '${ADMIN_USER_ID}')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Classrooms" ("Id", "Name", "ProfessionalId", "IsActive", "CreatedAt", "CreatedBy")
VALUES ('${aulaCId}', 'Aula C', '${p.profId}', true, NOW(), '${ADMIN_USER_ID}')
ON CONFLICT ("Id") DO NOTHING;
`);
  } else {
    const aulaAId = uuidv4();
    const aulaBId = uuidv4();
    const aulaCId = uuidv4();
    CLASSROOMS.push({ id: aulaAId, name: 'Aula A', profId: p.profId, isExisting: false });
    CLASSROOMS.push({ id: aulaBId, name: 'Aula B', profId: p.profId, isExisting: false });
    CLASSROOMS.push({ id: aulaCId, name: 'Aula C', profId: p.profId, isExisting: false });

    sqlStatements.push(`
INSERT INTO "Classrooms" ("Id", "Name", "ProfessionalId", "IsActive", "CreatedAt", "CreatedBy")
VALUES ('${aulaAId}', 'Aula A', '${p.profId}', true, NOW(), '${ADMIN_USER_ID}'),
       ('${aulaBId}', 'Aula B', '${p.profId}', true, NOW(), '${ADMIN_USER_ID}'),
       ('${aulaCId}', 'Aula C', '${p.profId}', true, NOW(), '${ADMIN_USER_ID}')
ON CONFLICT ("Id") DO NOTHING;
`);
  }
}

// 3. Generar Alumnos y Tutores para cada aula (entre 7 y 9 por aula)
sqlStatements.push(`-- 3. ALUMNOS, TUTORES, PERFIL FUNCIONAL Y VINCULACIONES`);

let studentTotalCount = 0;
const ALL_STUDENTS = [];

for (const classroom of CLASSROOMS) {
  let targetCount;
  if (classroom.isExisting && classroom.name === 'Aula A') {
    targetCount = 3; // + 5 existentes = 8
  } else {
    targetCount = (studentTotalCount % 3 === 0) ? 7 : (studentTotalCount % 3 === 1 ? 8 : 9);
  }

  for (let i = 0; i < targetCount; i++) {
    studentTotalCount++;
    const isMale = (studentTotalCount % 2 === 0);
    const firstName = isMale 
      ? FIRST_NAMES_M[(studentTotalCount * 3) % FIRST_NAMES_M.length]
      : FIRST_NAMES_F[(studentTotalCount * 5) % FIRST_NAMES_F.length];
    const lastName = SURNAMES[(studentTotalCount * 7) % SURNAMES.length];
    
    dniCounter += (studentTotalCount % 5 + 3);
    const studentDni = dniCounter.toString();
    const birthYear = 2011 + (studentTotalCount % 7);
    const birthMonth = String((studentTotalCount % 12) + 1).padStart(2, '0');
    const birthDay = String((studentTotalCount % 25) + 1).padStart(2, '0');
    const birthDate = `${birthYear}-${birthMonth}-${birthDay}T00:00:00Z`;

    const studentUserId = uuidv4();
    const studentId = uuidv4();
    // Emails limpios sin letras comidas: mateo.gonzalez1@inclusion.local
    const cleanStudentFn = firstName.toLowerCase();
    const cleanStudentLn = lastName.toLowerCase();
    const studentUsername = `${cleanStudentFn}.${cleanStudentLn}${studentTotalCount}@inclusion.local`;

    // Tutor
    tutorDniCounter += (studentTotalCount % 7 + 4);
    phoneCounter += (studentTotalCount % 11 + 5);
    const tutorDni = tutorDniCounter.toString();
    const tutorIsMale = (studentTotalCount % 3 !== 0);
    const tutorFirstName = tutorIsMale ? FIRST_NAMES_M[(studentTotalCount * 2) % FIRST_NAMES_M.length] : FIRST_NAMES_F[(studentTotalCount * 4) % FIRST_NAMES_F.length];
    const tutorLastName = lastName;
    const cleanTutorFn = tutorFirstName.toLowerCase();
    const cleanTutorLn = tutorLastName.toLowerCase();
    const tutorPhone = `+54 9 11 ${phoneCounter.toString().slice(0, 4)}-${phoneCounter.toString().slice(4, 8)}`;
    const domain = (studentTotalCount % 3 === 0) ? 'gmail.com' : (studentTotalCount % 3 === 1 ? 'outlook.com' : 'yahoo.com.ar');
    const tutorEmail = `${cleanTutorFn}.${cleanTutorLn}${studentTotalCount}@${domain}`;
    const tutorRel = TUTOR_RELATIONSHIPS[studentTotalCount % TUTOR_RELATIONSHIPS.length];
    const tutorUserId = uuidv4();
    const tutorId = uuidv4();

    // Perfil funcional y diagnóstico
    const diag = DIAGNOSES_TEMPLATES[studentTotalCount % DIAGNOSES_TEMPLATES.length];
    const attentionLevel = 2 + (studentTotalCount % 4);
    const commLevel = 2 + ((studentTotalCount + 1) % 4);
    const motorLevel = 2 + ((studentTotalCount + 2) % 4);
    const usesSignLang = (diag.typeId === 11);
    const reqLargeFont = (studentTotalCount % 4 === 0);
    const reqHighContrast = (studentTotalCount % 3 === 0);
    const visualNoise = (studentTotalCount % 2 === 0);
    const soundSens = (studentTotalCount % 3 === 0);
    const colorBlind = (studentTotalCount % 15 === 0) ? 'Deuteranopia' : null;

    ALL_STUDENTS.push({
      studentId,
      studentUserId,
      firstName,
      lastName,
      profId: classroom.profId,
      classroomId: classroom.id,
      diag
    });

    sqlStatements.push(`
-- Alumno: ${firstName} ${lastName} (Aula: ${classroom.name})
INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "Name", "Surname", "CreatedAt", "IsActive", "MustChangePassword")
VALUES ('${tutorUserId}', '${tutorEmail}', '${tutorEmail.toUpperCase()}', '${tutorEmail}', '${tutorEmail.toUpperCase()}', true, '${PWD_HASH_TUTOR}', '${uuidv4()}', '${uuidv4()}', true, false, true, 0, '${tutorFirstName}', '${tutorLastName}', NOW(), true, false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('${tutorUserId}', '${ROLE_TUTOR}')
ON CONFLICT DO NOTHING;

INSERT INTO "FamilyRepresentatives" ("Id", "UserId", "FirstName", "LastName", "DocumentNumber", "Phone", "Relationship", "IsActive", "Status", "CreatedAt", "CreatedBy")
VALUES ('${tutorId}', '${tutorUserId}', '${tutorFirstName}', '${tutorLastName}', '${tutorDni}', '${tutorPhone}', '${tutorRel}', true, 1, NOW(), '${ADMIN_USER_ID}')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "Name", "Surname", "CreatedAt", "IsActive", "MustChangePassword")
VALUES ('${studentUserId}', '${studentUsername}', '${studentUsername.toUpperCase()}', '${studentUsername}', '${studentUsername.toUpperCase()}', true, '${PWD_HASH_STUDENT}', '${uuidv4()}', '${uuidv4()}', false, false, true, 0, '${firstName}', '${lastName}', NOW(), true, false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('${studentUserId}', '${ROLE_STUDENT}')
ON CONFLICT DO NOTHING;

INSERT INTO "PersonsWithDisability" (
  "Id", "UserId", "FirstName", "LastName", "DocumentNumber", "BirthDate", 
  "DisabilityTypeId", "AutonomyLevelId", "AttentionLevel", "CommunicationLevel", 
  "UsesAAC", "UsesSignLanguage", "MotorSkillLevel", "InterestsAndMotivators", 
  "LearningStyle", "AvailableResources", "AdditionalTherapies", "RequiresLargeFont", 
  "RequiresHighContrast", "VisualNoiseSensitivity", "SoundSensitivity", "ColorBlindnessType",
  "SupervisorUserId", "AvatarColor", "CreatedAt", "CreatedBy", "IsActive"
)
VALUES (
  '${studentId}', '${studentUserId}', '${firstName}', '${lastName}', '${studentDni}', '${birthDate}',
  ${diag.typeId}, ${diag.autonomyId}, ${attentionLevel}, ${commLevel},
  true, ${usesSignLang}, ${motorLevel}, '${diag.notes.replace(/'/g, "''")}',
  '${diag.strategies.replace(/'/g, "''")}', '${diag.supports.replace(/'/g, "''")}', '${diag.objectives.replace(/'/g, "''")}', ${reqLargeFont},
  ${reqHighContrast}, ${visualNoise}, ${soundSens}, ${colorBlind ? `'${colorBlind}'` : 'NULL'},
  '${tutorUserId}', '#2196F3', NOW(), '${ADMIN_USER_ID}', true
)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "PersonRepresentatives" ("Id", "PersonId", "RepresentativeId", "Relationship", "IsPrimary", "HasInformedConsent", "CanSuperviseLogin", "IsActive", "CreatedAt")
VALUES ('${uuidv4()}', '${studentId}', '${tutorId}', '${tutorRel}', true, true, true, true, NOW())
ON CONFLICT DO NOTHING;

INSERT INTO "ProfessionalPersons" ("ProfessionalId", "PersonId", "ClassroomId", "IsPrimaryProfessional", "CanSuperviseLogin", "IsActive", "AssignedAt")
VALUES ('${classroom.profId}', '${studentId}', '${classroom.id}', true, true, true, NOW())
ON CONFLICT DO NOTHING;

INSERT INTO "Diagnoses" (
  "PersonId", "ProfessionalId", "DiagnosisDate", "PrimaryDiagnosis", 
  "InitialObservations", "IdentifiedCapabilities", "IdentifiedChallenges", 
  "RequiredSupports", "PedagogicalObjectives", "RecommendedStrategies", 
  "CreatedAt", "CreatedBy", "IsActive"
)
VALUES 
  (
    '${studentId}', '${classroom.profId}', NOW() - INTERVAL '210 days', 'Evaluacion Diagnostica Inicial: ${diag.diagnosis.replace(/'/g, "''")}',
    '${diag.notes.replace(/'/g, "''")}', '${diag.capabilities.replace(/'/g, "''")}', '${diag.challenges.replace(/'/g, "''")}',
    '${diag.supports.replace(/'/g, "''")}', '${diag.objectives.replace(/'/g, "''")}', '${diag.strategies.replace(/'/g, "''")}',
    NOW() - INTERVAL '210 days', '${classroom.profId}', true
  ),
  (
    '${studentId}', '${classroom.profId}', NOW() - INTERVAL '155 days', 'Lengua y Comunicacion: Evaluacion de Competencias Expresivas y CAA',
    'Seguimiento bimestral. Se consolido el uso del comunicador dinamico en el aula y en rutinas compartidas.',
    'Asociacion agil de pictogramas con acciones y objetos concretos. Expresa necesidades basicas espontaneamente.',
    'Dificultad en estructuracion sintactica verbal de oraciones complejas; requiere acompanamiento en grupos.',
    'Tablero CAA digital con pictogramas de alta frecuencia y modelado asistido por docente.',
    'Incrementar vocabulario activo a 30 pictogramas de uso cotidiano y participar en rondas grupales.',
    'Estimulacion del lenguaje natural asistido, pausas de espera y refuerzo positivo contingente.',
    NOW() - INTERVAL '155 days', '${classroom.profId}', true
  ),
  (
    '${studentId}', '${classroom.profId}', NOW() - INTERVAL '110 days', 'Matematica y Logica: Pensamiento Cuantitativo y Nocion de Numero',
    'Evaluacion de mitad de ciclo. Responde con gran motivacion al trabajo con materiales concretos manipulativos.',
    'Habilidad destacada para el emparejamiento por color, forma y tamano. Seracion numerica precisa del 1 al 10.',
    'Dificultad en la abstraccion de operaciones matematicas sin soporte grafico concreto.',
    'Material didactico multisensorial (bloques encastrables, regletas) y recta numerica visual.',
    'Consolidar la correspondencia uno a uno hasta 20 elementos y resolver problemas aditivos basicos.',
    'Fraccionamiento de problemas en pasos secuenciales simples y contextualizacion cotidiana.',
    NOW() - INTERVAL '110 days', '${classroom.profId}', true
  ),
  (
    '${studentId}', '${classroom.profId}', NOW() - INTERVAL '60 days', 'Habilidades Sociales: Convivencia Escolar y Autorregulacion en Aula',
    'Evaluacion formativa del segundo trimestre. Mayor contacto espontaneo con pares de pupitre y en recreos.',
    'Actitud empatica y solidaria con sus pares. Respeto por las normas de convivencia del aula.',
    'Inseguridad o ansiedad ante cambios inesperados de rutina o rotacion imprevista de docentes.',
    'Tarjeta visual de pedido de descanso (Pausa activa) y espacio de calma en el aula.',
    'Gestionar momentos de transicion de forma autonoma y participar en dinamicas colectivas sin desregulacion.',
    'Historias sociales breves previas a actividades novedosas y asignacion de roles colaborativos.',
    NOW() - INTERVAL '60 days', '${classroom.profId}', true
  ),
  (
    '${studentId}', '${classroom.profId}', NOW() - INTERVAL '12 days', 'Seguimiento DUA: Ajustes Curriculares y Evaluacion Formativa Continua',
    'Reevaluacion actualizada del plan de inclusion. Progreso notable en el Roadmap y en habitos de trabajo.',
    'Alta perseverancia en la resolucion de actividades en pantalla y consolidacion de habitos de autonomia.',
    'Fatiga en tareas de motricidad fina manual prolongada; requiere soporte en herramientas digitales adaptadas.',
    'Alineacion continua de la propuesta curricular con el DUA y articulacion con la familia.',
    'Completar los siguientes modulos del Roadmap con mas del 80 por ciento de precision.',
    'Multiples formas de representacion y expresion, evaluacion formativa procesual y andamiaje continuo.',
    NOW() - INTERVAL '12 days', '${classroom.profId}', true
  );

INSERT INTO "PersonEmbeddings" ("PersonId", "Model", "Dimensions", "CreatedAt", "CreatedBy", "IsActive")
VALUES ('${studentId}', 'paraphrase-multilingual-MiniLM-L12-v2', 384, NOW(), '${ADMIN_USER_ID}', true)
ON CONFLICT ("PersonId") DO NOTHING;
`);
  }
}

// 4. Inicializar Roadmaps (PersonRoadmaps, PersonRoadmapAreas, PersonRoadmapActivities)
sqlStatements.push(`-- 4. ROADMAPS DE 10 NIVELES PARA CADA ALUMNO`);
sqlStatements.push(`
DO $$
DECLARE
  v_student RECORD;
  v_roadmap_id INT;
  v_area_id INT;
  v_skill_area_id INT := 41; -- Trayectoria
BEGIN
  FOR v_student IN 
    SELECT p."Id" AS student_id, pp."ProfessionalId" AS prof_id
    FROM "PersonsWithDisability" p
    JOIN "ProfessionalPersons" pp ON p."Id" = pp."PersonId"
    WHERE NOT EXISTS (SELECT 1 FROM "PersonRoadmaps" rm WHERE rm."PersonId" = p."Id")
  LOOP
    INSERT INTO "PersonRoadmaps" ("PersonId", "CreatedByProfessionalId", "Notes", "CreatedAt", "CreatedBy", "IsActive")
    VALUES (v_student.student_id, v_student.prof_id, 'Roadmap pedagogico oficial estandar', NOW(), v_student.prof_id, true)
    RETURNING "Id" INTO v_roadmap_id;

    INSERT INTO "PersonRoadmapAreas" ("PersonRoadmapId", "SkillAreaId", "DisplayOrder", "CreatedAt", "CreatedBy", "IsActive")
    VALUES (v_roadmap_id, v_skill_area_id, 1, NOW(), v_student.prof_id, true)
    RETURNING "Id" INTO v_area_id;

    INSERT INTO "PersonRoadmapActivities" ("PersonRoadmapAreaId", "ActivityId", "SequenceOrder", "IsUnlocked", "UnlockThresholdPercent", "ShowHints", "DifficultyLevel", "CreatedAt", "CreatedBy", "IsActive")
    VALUES 
      (v_area_id, 85, 1, true, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 86, 2, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 87, 3, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 88, 4, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 89, 5, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 90, 6, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 91, 7, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 92, 8, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 93, 9, false, 60, true, 1, NOW(), v_student.prof_id, true),
      (v_area_id, 94, 10, false, 60, true, 1, NOW(), v_student.prof_id, true);
  END LOOP;
END $$;
`);

// 5. Métricas: ActivitySessions realistas para cada alumno
sqlStatements.push(`-- 5. HISTORICO DE SESIONES DE METRICAS (ÚLTIMOS 30 DÍAS)`);
sqlStatements.push(`
DO $$
DECLARE
  v_student RECORD;
  v_act RECORD;
  v_current_date TIMESTAMPTZ;
  v_level_count INT;
  v_is_frustrated BOOLEAN;
  v_success_rate NUMERIC(5,2);
  v_error_count INT;
  v_gas_score INT;
  v_time_spent INT;
  v_idx INT := 0;
BEGIN
  FOR v_student IN 
    SELECT p."Id" AS student_id, pp."ProfessionalId" AS prof_id
    FROM "PersonsWithDisability" p
    JOIN "ProfessionalPersons" pp ON p."Id" = pp."PersonId"
    WHERE NOT EXISTS (SELECT 1 FROM "ActivitySessions" s WHERE s."StudentId" = p."Id")
  LOOP
    v_idx := v_idx + 1;
    v_current_date := NOW() - (25 + (v_idx % 5)) * INTERVAL '1 day' + (v_idx % 8 + 8) * INTERVAL '1 hour';
    
    IF v_idx % 5 = 0 THEN
      v_level_count := 3 + (v_idx % 2);
    ELSIF v_idx % 3 = 0 THEN
      v_level_count := 5 + (v_idx % 2);
    ELSIF v_idx % 2 = 0 THEN
      v_level_count := 7 + (v_idx % 2);
    ELSE
      v_level_count := 9 + (v_idx % 2);
    END IF;

    v_is_frustrated := (v_idx % 6 = 0);

    FOR v_act IN
      SELECT "Id" AS act_id, "RoadmapOrder" AS seq, "CategoryId" AS cat_id
      FROM "Activities"
      WHERE "RoadmapOrder" IS NOT NULL AND "RoadmapOrder" <= v_level_count
      ORDER BY "RoadmapOrder"
    LOOP
      IF v_act.seq = v_level_count AND v_is_frustrated THEN
        v_success_rate := 45.00 + (v_idx % 12);
        v_error_count := 5 + (v_idx % 4);
        v_gas_score := -1;
        v_time_spent := 180 + (v_idx % 60);
      ELSE
        v_success_rate := 75.00 + (v_idx % 26);
        v_error_count := (v_idx % 3);
        IF v_success_rate >= 90 THEN
          v_gas_score := 2;
        ELSIF v_success_rate >= 80 THEN
          v_gas_score := 1;
        ELSE
          v_gas_score := 0;
        END IF;
        v_time_spent := 60 + (v_idx % 80);
      END IF;

      INSERT INTO "ActivitySessions" (
        "StudentId", "ProfessionalId", "ActivityId", "DateCompleted", 
        "SuccessRate", "ErrorCount", "TimeSpentSeconds", "GasScore", 
        "CreatedAt", "CreatedBy", "IsActive"
      )
      VALUES (
        v_student.student_id, v_student.prof_id, v_act.act_id, v_current_date,
        v_success_rate, v_error_count, v_time_spent, v_gas_score,
        v_current_date, v_student.prof_id, true
      );

      UPDATE "PersonRoadmapActivities" pra
      SET "IsUnlocked" = true, "UnlockedAt" = v_current_date
      FROM "PersonRoadmapAreas" prarea
      JOIN "PersonRoadmaps" prm ON prarea."PersonRoadmapId" = prm."Id"
      WHERE pra."PersonRoadmapAreaId" = prarea."Id"
        AND prm."PersonId" = v_student.student_id
        AND pra."SequenceOrder" <= v_act.seq;

      v_current_date := v_current_date + (2 + (v_idx % 2)) * INTERVAL '1 day' + (v_idx % 3) * INTERVAL '1 hour';
      IF v_current_date > NOW() THEN
        v_current_date := NOW() - INTERVAL '2 hours';
      END IF;
    END LOOP;
  END LOOP;
END $$;
`);

// 6. Reportes distribuidos en Draft, Submitted, Approved, Rejected para cada profesional
sqlStatements.push(`-- 6. GENERACION DE REPORTES CLINICOS EN TODOS LOS ESTADOS`);
sqlStatements.push(`
DO $$
DECLARE
  v_prof RECORD;
  v_student RECORD;
  v_rep_idx INT;
  v_status TEXT;
  v_is_read BOOLEAN;
  v_report_type_id INT;
  v_admin_comment TEXT;
  v_app_date TIMESTAMPTZ;
  v_rep_date TIMESTAMPTZ;
BEGIN
  FOR v_prof IN 
    SELECT p."Id" AS prof_id, p."FirstName" AS fn, p."LastName" AS ln
    FROM "Professionals" p
    WHERE p."IsActive" = true
  LOOP
    v_rep_idx := 0;

    FOR v_student IN
      SELECT pp."PersonId" AS student_id, pwd."FirstName" AS s_fn, pwd."LastName" AS s_ln
      FROM "ProfessionalPersons" pp
      JOIN "PersonsWithDisability" pwd ON pp."PersonId" = pwd."Id"
      WHERE pp."ProfessionalId" = v_prof.prof_id
      LIMIT 8
    LOOP
      v_rep_idx := v_rep_idx + 1;
      v_rep_date := NOW() - (v_rep_idx * 4) * INTERVAL '1 day';

      IF v_rep_idx <= 2 THEN
        v_status := 'Draft';
        v_is_read := false;
        v_admin_comment := NULL;
        v_app_date := NULL;
        v_report_type_id := 12;
      ELSIF v_rep_idx <= 4 THEN
        v_status := 'Submitted';
        v_is_read := false;
        v_admin_comment := NULL;
        v_app_date := NULL;
        v_report_type_id := 13;
      ELSIF v_rep_idx <= 7 THEN
        v_status := 'Approved';
        v_is_read := (v_rep_idx = 5 OR v_rep_idx = 6);
        v_admin_comment := 'Informe evaluado y aprobado conforme a los objetivos pedagogicos del periodo.';
        v_app_date := v_rep_date + INTERVAL '1 day';
        v_report_type_id := 14;
      ELSE
        v_status := 'Rejected';
        v_is_read := false;
        v_admin_comment := 'Se solicita ampliar el apartado de Areas a Reforzar especificando las estrategias de autorregulacion y apoyos visuales propuestos.';
        v_app_date := NULL;
        v_report_type_id := 11;
      END IF;

      INSERT INTO "Reports" (
        "PersonId", "ProfessionalId", "ReportTypeId", "Title", "Content", 
        "ReportDate", "PeriodStartDate", "PeriodEndDate", "AchievedGoals", 
        "AreasToReinforce", "FutureRecommendations", "NextObjectives", 
        "Status", "IsReadByFamily", "AdminComment", "ApprovedAt", "ApprovedBy", 
        "CreatedAt", "CreatedBy", "IsActive"
      )
      VALUES (
        v_student.student_id, v_prof.prof_id, v_report_type_id,
        'Informe Pedagogico y Evolutivo - ' || v_student.s_fn || ' ' || v_student.s_ln,
        'Durante el periodo evaluado, el estudiante demostro una notable constancia en el seguimiento de rutinas adaptadas y en la interaccion con el sistema digital de comunicacion aumentativa (CAA). Logro completar las actividades asignadas con autonomia y buena disposicion ante los desafios propuestos.',
        v_rep_date, v_rep_date - INTERVAL '30 days', v_rep_date,
        'Consolidacion del uso de pictogramas para requerimientos basicos de alimentacion e higiene. Incremento del tiempo de atencion sostenida en pantalla a 20 minutos sin signos de fatiga.',
        'Manejo de la frustracion ante consignas que requieren discriminacion auditiva y turnos de espera en dinamicas colectivas.',
        'Continuar con el refuerzo positivo en el hogar mediante el uso del panel de comunicacion familiar sincronizado con la escuela.',
        'Avanzar hacia la construccion de oraciones simples de dos elementos (Sujeto + Accion) en el Roadmap CAA.',
        v_status, v_is_read, v_admin_comment, v_app_date, 
        CASE WHEN v_status = 'Approved' THEN '${ADMIN_USER_ID}'::UUID ELSE NULL END,
        v_rep_date, v_prof.prof_id, true
      );
    END LOOP;
  END LOOP;
END $$;
`);

sqlStatements.push(`COMMIT;`);

const outputPath = path.join(__dirname, 'seed_full_demo_data.sql');
fs.writeFileSync(outputPath, sqlStatements.join('\n'), 'utf8');

console.log(`Script SQL generado exitosamente en: ${outputPath}`);
console.log(`Total de profesionales: ${PROFESSIONALS.length}`);
console.log(`Total de aulas: ${CLASSROOMS.length}`);
console.log(`Total de alumnos generados: ${studentTotalCount}`);
