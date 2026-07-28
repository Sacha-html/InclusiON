# Diccionario de Datos — Sistema InclusiON

**Última actualización:** 2026-05-31

Este documento describe las entidades de datos del sistema InclusiON, organizadas por área funcional. Para cada campo se documenta: tipo de dato, si es obligatorio, si debe ser único en el sistema, un ejemplo de valor real, y la descripción en lenguaje del negocio.

---

## 1. Catálogos de Referencia

### Tipo de Discapacidad
Clasificación de discapacidades utilizada en el alta de personas.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (100) | Sí | No | — | Nombre del tipo (único) |
| Descripción | Texto (500) | No | No | — | Detalle del tipo |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: no aparece en listados pero conserva historial. |

### Nivel de Autonomía
Niveles que determinan el método de login y el grado de supervisión requerido.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (100) | Sí | No | — | "Alta", "Media", "Baja" |
| Descripción | Texto (500) | No | No | — | Detalle del nivel |
| Requiere supervisión | Booleano | Sí | No | — | Si la persona necesita acompañamiento |
| Orden de visualización | Entero | Sí | No | — | Orden en dropdowns |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Categoría de Actividad
Clasificación temática de las actividades educativas.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (100) | Sí | No | — | Nombre de la categoría |
| Descripción | Texto (500) | No | No | — | Detalle de la categoría |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Área de Habilidad
Dominios de competencia que se trabajan con cada persona (Comunicación, Alfabetización, Lógica-Matemática, Conducta, Motricidad).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (100) | Sí | No | — | Nombre del área (único) |
| Descripción | Texto (500) | No | No | — | Detalle del área |
| Ícono | Texto (50) | No | No | — | Identificador del ícono visual |
| Color | Texto (10) | No | No | — | Color hexadecimal (#RRGGBB) |
| Orden de visualización | Entero | Sí | No | — | Orden en listados |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Tipo de Template de Actividad
Plantillas que definen la estructura de contenido de cada tipo de actividad interactiva.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Área de habilidad | Referencia | Sí | No | — | Área a la que pertenece |
| Nombre | Texto (150) | Sí | No | — | Nombre de la plantilla |
| Código | Texto (50) | Sí | Sí | SELECT_FIGURE | Identificador técnico del template. Determina qué componente Angular renderiza la actividad. |
| Descripción | Texto (500) | No | No | — | Detalle de la plantilla |
| Esquema de contenido | Texto largo | Sí | No | — | Estructura JSON que define los campos del formulario dinámico |
| Nombre del componente | Texto (100) | Sí | No | — | Componente visual que renderiza la actividad |
| Usa pictogramas | Booleano | Sí | No | — | Si el template requiere pictogramas |
| Tiene audio | Booleano | Sí | No | — | Si el template soporta audio |
| Orden de visualización | Entero | Sí | No | — | Orden en listados |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Método de Login
Métodos de autenticación adaptados al nivel de autonomía.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Código | Texto (20) | Sí | Sí | STANDARD | Código técnico del método de login. Determina qué credenciales exige el portal. |
| Nombre | Texto (100) | Sí | No | — | Nombre descriptivo |
| Descripción | Texto (500) | No | No | — | Detalle del método |
| Nivel mínimo de autonomía | Entero | Sí | No | — | Autonomía mínima requerida |
| Requiere email | Booleano | Sí | No | — | Si necesita email para autenticarse |
| Requiere contraseña | Booleano | Sí | No | — | Si necesita contraseña |
| Requiere PIN | Booleano | Sí | No | — | Si necesita PIN numérico |
| Requiere supervisor | Booleano | Sí | No | — | Si necesita autorización de un supervisor |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Tipo de Reporte
Clasificación de los reportes de progreso.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (100) | Sí | No | — | Nombre del tipo |
| Descripción | Texto (500) | No | No | — | Detalle del tipo |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 2. Usuarios y Perfiles

### Refresh Token
Token de renovación de sesión JWT. Permite mantener la sesión activa sin reautenticarse. Cada token puede revocarse individualmente (logout remoto desde múltiples dispositivos).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | UUID | Sí | Sí | a1b2-c3d4-... | Identificador del token. |
| Usuario | Referencia | Sí | No | — | Usuario al que pertenece el token. |
| Token | Texto (512) | Sí | Sí | eyJhb... | Cadena del refresh token. Única en el sistema. |
| Expira en | Fecha/hora | Sí | No | 2026-06-15 00:00 | Cuándo vence el token. Después de esta fecha no puede usarse para renovar. |
| Revocado en | Fecha/hora | No | No | — | Cuándo fue revocado manualmente. Null = todavía válido. |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: token invalidado. |

### Dispositivo de Confianza
Dispositivo autorizado para login asistido sin credenciales propias. Un supervisor registra el dispositivo para que la persona pueda iniciar sesión desde él sin ingresar contraseña.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Usuario | Referencia | Sí | No | — | Persona cuyo login se facilita desde este dispositivo. |
| Autorizado por | Referencia | No | No | — | Supervisor que autorizó el dispositivo. Null si fue auto-registrado. |
| ID de dispositivo | Texto (256) | Sí | No | chrome-abc123 | Identificador técnico del navegador/dispositivo. |
| Nombre del dispositivo | Texto (100) | No | No | Tablet sala terapia | Nombre amigable asignado al dispositivo. |
| Registrado en | Fecha/hora | Sí | No | 2026-03-01 09:00 | Cuándo se autorizó el dispositivo. |
| Último uso | Fecha/hora | No | No | 2026-05-20 14:30 | Última vez que se usó para hacer login. |

### Usuario
Cuenta de acceso al sistema. Cada usuario tiene exactamente un perfil asociado (Profesional, Persona con Discapacidad o Familiar).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador del usuario |
| Email | Texto (100) | Sí | Sí | lucia@inclusiOn.edu.ar | Email de la cuenta. Único en el sistema. Requerido para login estándar y notificaciones. |
| Nombre | Texto (50) | No | No | — | Nombre de pila |
| Apellido | Texto (50) | No | No | — | Apellido |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se creó la cuenta |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |
| Último login | Fecha/hora | No | No | — | Fecha del último acceso |
| Debe cambiar contraseña | Booleano | Sí | No | — | Si tiene contraseña temporal que debe cambiar |

### Profesional
Profesionales que trabajan con personas con discapacidad (docentes, terapeutas, psicólogos).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador del profesional |
| Usuario | Referencia | Sí | No | — | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | No | Lucía | Nombre de pila del usuario tal como aparece en la interfaz. |
| Apellido | Texto (100) | Sí | No | García | Apellido del usuario. |
| DNI | Texto (20) | Sí* | Sí | 28456789 | DNI del profesional. Único en el sistema. Requerido al crear; admite null para registros legacy. |
| Teléfono | Texto (20) | No | No | — | Número de contacto |
| Especialidad | Texto (100) | No | No | — | Área de especialización |
| Matrícula | Texto (50) | No | Sí | MP-12345 | Matrícula habilitante del profesional. Única en el sistema si está cargada. |
| Fecha de nacimiento | Fecha | No | No | — | Fecha de nacimiento |
| Email | Texto (255) | No | Sí | dr.garcia@inclusiOn.edu.ar | Email previo a crear la cuenta. Único en el sistema. |
| **Estado de validación** | | | |
| Status | Enumerado | Sí | No | Approved | Estado del profesional. Pending = espera validación admin. Solo Approved puede operar el sistema. Ver diagrama de estados. |
| Fecha de validación | Fecha/hora | No | No | — | Cuándo fue validado/aprobado |
| Validado por | Referencia | No | No | — | Usuario admin que validó al profesional |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Historial de Estados del Profesional
Registro de cada cambio de estado de un profesional.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador del registro |
| Profesional | Referencia | Sí | No | — | Profesional al que corresponde |
| Estado anterior | Enumerado | No | No | — | Estado antes del cambio (null si es el primero) |
| Estado nuevo | Enumerado | Sí | No | — | Estado después del cambio |
| Observación | Texto (500) | No | No | — | Motivo del cambio (ej: motivo de rechazo o desactivación) |
| Modificado por | Referencia | No | No | — | Usuario que realizó el cambio |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se realizó el cambio |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Persona con Discapacidad
Destinatario central del sistema. Recibe planes de trabajo, realiza actividades y su progreso es monitoreado.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador de la persona |
| Usuario | Referencia | Sí | No | — | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | No | Lucía | Nombre de pila del usuario tal como aparece en la interfaz. |
| Apellido | Texto (100) | Sí | No | García | Apellido del usuario. |
| DNI | Texto (20) | No | Sí | 35789012 | DNI de la persona. Único si está cargado; puede omitirse. |
| Fecha de nacimiento | Fecha | Sí | No | 2005-03-14 | Fecha de nacimiento de la persona. Requerida para calcular la edad en reportes clínicos. |
| Tipo de discapacidad | Referencia | Sí | No | — | Del catálogo de tipos de discapacidad |
| Foto | Texto (500) | No | No | — | URL de la foto de perfil |
| **Perfil funcional** | | | |
| Nivel de atención | Entero (1-5) | No | No | — | Capacidad de atención |
| Nivel de comunicación | Entero (1-5) | No | No | — | Capacidad comunicativa |
| Nivel de motricidad | Entero (1-5) | No | No | — | Capacidad motriz |
| Usa CAA | Booleano | No | No | — | Si usa Comunicación Aumentativa y Alternativa |
| Usa lengua de señas | Booleano | No | No | — | Si se comunica con LSA |
| Intereses y motivadores | Texto (500) | No | No | — | Qué le interesa y motiva |
| Estilo de aprendizaje | Texto (50) | No | No | — | Visual, Auditivo o Kinestésico |
| Recursos disponibles | Texto (255) | No | No | — | Recursos con los que cuenta |
| Terapias adicionales | Texto (500) | No | No | — | Otras terapias que recibe |
| **Accesibilidad** | | | |
| Requiere fuente grande | Booleano | No | No | — | Ajuste visual |
| Requiere alto contraste | Booleano | No | No | — | Ajuste visual |
| Sensibilidad al ruido visual | Booleano | No | No | — | Si las animaciones lo perturban |
| Sensibilidad al sonido | Booleano | No | No | — | Si los sonidos lo perturban |
| **Autenticación** | | | |
| Nivel de autonomía | Referencia | No | No | — | Del catálogo de niveles de autonomía |
| Método de login | Referencia | No | No | — | Del catálogo de métodos de login |
| Color de avatar | Texto (20) | No | No | — | Color hexadecimal para identificación visual |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Representante Familiar
Familiares o tutores que acompañan el proceso de la persona con discapacidad.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador del familiar |
| Usuario | Referencia | Sí | No | — | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | No | Lucía | Nombre de pila del usuario tal como aparece en la interfaz. |
| Apellido | Texto (100) | Sí | No | García | Apellido del usuario. |
| DNI | Texto (20) | No | Sí | 35789012 | DNI de la persona. Único si está cargado; puede omitirse. |
| Teléfono | Texto (20) | No | No | — | Número de contacto |
| Relación | Texto (50) | No | No | — | Madre, Padre, Tutor, Abuelo, etc. |
| Onboarding completado | Booleano | Sí | No | — | Si el familiar completó la pantalla de bienvenida. Default: false |
| Estado | Enum (FamilyStatusEnum) | Sí | No | — | Estado del familiar en el sistema (Active/Terminated). Default: Active |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Historial de Estados del Familiar
Registro de cambios de estado del familiar.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | Identificador del registro |
| Familiar | Referencia | Sí | No | — | Familiar al que pertenece |
| Estado anterior | Enum (FamilyStatusEnum) | No | No | — | Estado anterior |
| Estado nuevo | Enum (FamilyStatusEnum) | Sí | No | — | Nuevo estado |
| Observación | Texto (500) | No | No | — | Motivo del cambio |
| Usuario que cambió | Referencia | No | No | — | Usuario que realizó el cambio |
| Fecha de cambio | Fecha/hora | Sí | No | — | Cuándo se realizó el cambio |

---

## 3. Instituciones y Relaciones

### Institución Educativa
Escuelas, centros de rehabilitación o instituciones donde trabajan los profesionales.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Nombre | Texto (255) | Sí | No | — | Nombre de la institución |
| Dirección | Texto (255) | No | No | — | Domicilio |
| Teléfono | Texto (20) | No | No | — | Número de contacto |
| Email | Texto (100) | No | Sí | info@escuela.edu.ar | Email de contacto de la institución. Único si está presente. |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Admin ↔ Institución
Vinculación de administradores institucionales a sus instituciones.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Usuario admin | Referencia | Sí | No | — | El administrador asignado |
| Institución | Referencia | Sí | No | — | La institución asignada |
| Fecha de asignación | Fecha/hora | Sí | No | — | Cuándo se estableció la relación |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Profesional ↔ Institución
Vinculación de profesionales a las instituciones donde trabajan.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Profesional | Referencia | Sí | No | — | El profesional asignado |
| Institución | Referencia | Sí | No | — | La institución donde trabaja |
| Fecha de asignación | Fecha/hora | Sí | No | — | Cuándo se estableció la relación |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Profesional ↔ Persona
Vinculación de profesionales a las personas que atienden.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Profesional | Referencia | Sí | No | — | El profesional asignado |
| Persona | Referencia | Sí | No | — | La persona atendida |
| Fecha de asignación | Fecha/hora | Sí | No | — | Cuándo se estableció la relación |
| Es profesional principal | Booleano | Sí | No | — | Si es el profesional principal de la persona |
| Puede supervisar login | Booleano | Sí | No | — | Si puede autorizar el login asistido |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Persona ↔ Familiar
Vinculación de personas con discapacidad a sus representantes familiares.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | ID del registro de vinculación |
| Persona | Referencia | Sí | No | — | La persona con discapacidad |
| Familiar | Referencia | Sí | No | — | El representante familiar |
| Relación | Texto (50) | No | No | — | Tipo de parentesco (Madre, Padre, Tutor, etc.) |
| Es primario | Booleano | Sí | No | — | Si es el representante principal |
| Tiene consentimiento informado | Booleano | Sí | No | — | Si firmó consentimiento |
| Fecha de consentimiento | Fecha | No | No | — | Cuándo firmó el consentimiento |
| Puede supervisar login | Booleano | Sí | No | — | Si puede autorizar el login asistido |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se creó el vínculo |
| Fecha de modificación | Fecha/hora | No | No | — | Última modificación |
| Fecha de desvinculación | Fecha/hora | No | No | — | Cuando se desvinculó (null si activo) |
| Observación de desvinculación | Texto (500) | No | No | — | Motivo de desvinculación |

### Historial de Cambios de Vinculación
Registro de cambios en las vinculaciones familiar-persona.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Identificador único | Sí | No | — | ID del registro |
| ID de vínculo | Referencia | Sí | No | — | ID del PersonRepresentative |
| Persona | Referencia | Sí | No | — | La persona con discapacidad |
| Familiar | Referencia | Sí | No | — | El representante familiar |
| Tipo de cambio | Enum | Sí | No | — | Linked, Updated, Unlinked |
| Relación | Texto (50) | No | No | — | Relación en el momento del cambio |
| Era primario | Booleano | No | No | — | Si era principal en el momento |
| Observación | Texto (500) | No | No | — | Motivo (especialmente para Unlinked) |
| Usuario que cambió | Referencia | No | No | — | Usuario que realizó el cambio |
| Fecha de cambio | Fecha/hora | Sí | No | — | Cuándo se realizó el cambio |

### Perfil de Habilidades (Persona ↔ Área)
Áreas de habilidad asignadas a cada persona para trabajar.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Persona | Referencia | Sí | No | — | La persona con discapacidad |
| Área de habilidad | Referencia | Sí | No | — | El área a trabajar |
| Fecha de asignación | Fecha/hora | Sí | No | — | Cuándo se asignó |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 4. Invitaciones

### Invitación
Invitaciones por email para el registro de familiares.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Profesional creador | Referencia | Sí | No | — | Quién generó la invitación |
| Persona asociada | Referencia | No | No | — | Persona con discapacidad vinculada |
| Código | Texto (64) | Sí | Sí | INV-7f3a2b9c | Token aleatorio del link de invitación. De un solo uso. Expira en 7 días. |
| Email | Texto (100) | Sí | Sí | familia@gmail.com | Email del familiar invitado. Único por invitación activa. |
| Nombre | Texto (100) | No | No | — | Nombre del familiar |
| Apellido | Texto (100) | No | No | — | Apellido del familiar |
| Relación | Texto (50) | No | No | — | Relación con la persona (Madre, Padre, etc.) |
| Fecha de expiración | Fecha/hora | Sí | No | — | Cuándo expira la invitación (7 días) |
| Usada | Booleano | Sí | No | false | En true: la invitación fue aceptada y no puede usarse de nuevo. Solo un uso por código. |
| Fecha de uso | Fecha/hora | No | No | — | Cuándo fue aceptada |
| Usada por | Referencia | No | No | — | Usuario que aceptó la invitación |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 5. Actividades y Contenido

### Actividad
Actividades educativas creadas por profesionales o predefinidas por el sistema.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Profesional creador | Referencia | Sí | No | — | Quién creó la actividad |
| Categoría | Referencia | Sí | No | — | Categoría de actividad (del catálogo) |
| Área de habilidad | Referencia | No | No | — | Área a la que pertenece |
| Título | Texto (150) | Sí | No | — | Nombre de la actividad |
| Descripción | Texto largo | No | No | — | Descripción detallada |
| Instrucciones | Texto largo | No | No | — | Instrucciones para realizar la actividad |
| Soporte visual | Booleano | No | No | — | Si incluye soporte visual |
| Soporte auditivo | Booleano | No | No | — | Si incluye soporte auditivo |
| Lectura fácil | Booleano | No | No | — | Si usa lectura simplificada |
| Usa pictogramas | Booleano | No | No | — | Si usa pictogramas ARASAAC |
| URL de recursos | Texto (500) | No | No | — | Enlace a recursos externos |
| Duración estimada (min) | Entero | No | No | — | Minutos estimados de duración |
| Nivel de complejidad | Entero (1-5) | No | No | — | Nivel de dificultad |
| Requiere supervisión | Booleano | Sí | No | — | Si necesita supervisor presente |
| Es actividad estándar | Booleano | Sí | No | — | Si es del sistema (no editable) |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Contenido de Actividad
Contenido interactivo vinculado a una actividad y su plantilla (1:1 con Actividad).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Actividad | Referencia | Sí | No | — | Actividad asociada (1:1) |
| Tipo de template | Referencia | Sí | No | — | Plantilla que define la estructura |
| Contenido JSON | Texto largo | Sí | No | — | Contenido dinámico en formato JSON según el esquema de la plantilla |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Embedding de Actividad
Vector semántico de la actividad generado por IA (pgvector). Permite búsqueda por similitud ("encuentra actividades parecidas a esta"). Se genera o actualiza automáticamente al crear/editar la actividad.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Actividad | Referencia (PK) | Sí | Sí | 42 | Actividad asociada (1:1). La PK del embedding es el mismo Id de la actividad. |
| Modelo | Texto (100) | Sí | No | text-embedding-3-small | Nombre del modelo de IA que generó el vector. |
| Dimensiones | Entero | Sí | No | 1536 | Número de dimensiones del vector. Depende del modelo usado. |
| Embedding JSON | Texto largo | Sí | No | [0.012, -0.043, ...] | Vector serializado como JSON. Usado internamente por pgvector para búsquedas de similitud. |

---

## 6. Roadmap (Plan de Trabajo)

### Roadmap de la Persona
Plan de trabajo personalizado (1:1 con Persona).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Persona | Referencia | Sí | No | — | Persona con discapacidad (1:1) |
| Profesional creador | Referencia | Sí | No | — | Quién armó el roadmap |
| Notas | Texto (2000) | No | No | — | Observaciones generales |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Área del Roadmap
Sección del roadmap correspondiente a un área de habilidad.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Roadmap | Referencia | Sí | No | — | Roadmap al que pertenece |
| Área de habilidad | Referencia | Sí | No | — | Área de habilidad (única por roadmap) |
| Orden de visualización | Entero | Sí | No | — | Orden de las áreas |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Actividad del Roadmap
Actividad dentro de un área del roadmap, con parámetros de personalización.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Área del roadmap | Referencia | Sí | No | — | Área a la que pertenece |
| Actividad | Referencia | Sí | No | — | Actividad asignada (única por área) |
| Orden secuencial | Entero | Sí | No | — | Posición en la secuencia (único por área) |
| Desbloqueada | Booleano | Sí | No | — | Si la persona puede acceder |
| Fecha de desbloqueo | Fecha/hora | No | No | — | Cuándo se desbloqueó |
| Umbral de desbloqueo (%) | Entero | Sí | No | — | Porcentaje mínimo de éxito para desbloquear la siguiente (por defecto: 60%) |
| Tiempo límite (seg) | Entero | No | No | — | Tiempo máximo para completar |
| Máximo de intentos | Entero | No | No | — | Intentos permitidos |
| Mostrar pistas | Booleano | Sí | No | — | Si se muestran ayudas |
| Nivel de dificultad | Entero (1-3) | Sí | No | — | Dificultad actual (por defecto: 1) |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 7. Asignaciones y Respuestas

### Asignación de Actividad
Actividad asignada a una persona con discapacidad.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Actividad | Referencia | Sí | No | — | Actividad asignada |
| Persona | Referencia | Sí | No | — | Persona asignada |
| Profesional asignador | Referencia | Sí | No | — | Quién hizo la asignación |
| Fecha de asignación | Fecha/hora | Sí | No | — | Cuándo se asignó |
| Fecha límite | Fecha/hora | No | No | — | Fecha de vencimiento |
| Estado | Enumerado | Sí | No | Pendiente | Estado de la asignación. Solo Pendiente puede cancelarse. EnProgreso no se puede cancelar. Completada es final e inmutable. Ver diagrama de estados. |
| Orden secuencial | Entero | No | No | — | Posición en la secuencia |
| Es actividad de evaluación | Booleano | Sí | No | — | Si es parte de la evaluación inicial |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Respuesta de Actividad
Registro de cada intento de resolución de una actividad asignada.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Asignación | Referencia | Sí | No | — | Asignación a la que responde |
| Fecha de inicio | Fecha/hora | Sí | No | — | Cuándo comenzó el intento |
| Fecha de finalización | Fecha/hora | No | No | — | Cuándo terminó |
| Tiempo empleado (seg) | Entero | No | No | — | Segundos que tardó |
| Resultado | Texto | No | No | — | Éxito, Parcial, Fallido, Abandonado |
| Porcentaje de éxito | Decimal (0-100) | No | No | 85.00 | Porcentaje de respuestas correctas. ≥80% = Éxito, ≥50% = Parcial, <50% = Fallido. |
| Cantidad de intentos | Entero | Sí | No | — | Número de intentos realizados |
| Patrón de respuesta | Texto largo | No | No | — | Registro JSON de las respuestas individuales |
| Requirió soporte | Booleano | Sí | No | — | Si necesitó ayuda de un supervisor |
| Nivel de frustración | Entero (1-5) | No | No | 2 | Escala 1 (sin frustración) a 5 (frustración severa). El motor adaptativo interviene si supera el umbral configurado. |
| Observaciones | Texto (1000) | No | No | — | Notas del profesional o del sistema |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Resultado de Actividad (Roadmap)
Resultado consolidado de un intento sobre una actividad del roadmap. Alimenta el radar chart de habilidades y es el input principal del motor adaptativo. A diferencia de `ActivityResponse` (que registra la ejecución), `ActivityResult` almacena la puntuación normalizada para análisis de progreso.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Actividad del roadmap | Referencia | Sí | No | — | Actividad del roadmap evaluada. |
| Número de intento | Entero | Sí | No | 3 | Secuencia del intento dentro de la actividad. El primero es 1. |
| Puntuación (%) | Decimal (0-1) | Sí | No | 0.85 | Score normalizado entre 0.0 y 1.0. 0.85 = 85% de éxito. |
| Tiempo empleado (seg) | Entero | Sí | No | 120 | Segundos que tardó la persona en completar el intento. |
| Completado en | Fecha/hora | Sí | No | 2026-05-20 10:45 | Cuándo terminó el intento. |

---

## 8. Motor de Dificultad Adaptativa

### Configuración Adaptativa
Parámetros del motor de dificultad adaptativa para cada actividad del roadmap (1:0..1 con Actividad del Roadmap).

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Actividad del roadmap | Referencia | Sí | No | — | Actividad asociada (1:1) |
| Habilitado | Booleano | Sí | No | true | Si el motor adaptativo está activo para esta actividad. Si es false, la dificultad no se ajusta automáticamente. |
| Dificultad mínima | Entero | Sí | No | — | Nivel mínimo (por defecto: 1) |
| Dificultad máxima | Entero | Sí | No | — | Nivel máximo (por defecto: 5) |
| Tiempo mínimo (seg) | Entero | No | No | — | Tiempo límite mínimo |
| Tiempo máximo (seg) | Entero | No | No | — | Tiempo límite máximo |
| Éxitos consecutivos para subir | Entero | Sí | No | — | Cantidad necesaria (por defecto: 3) |
| Fracasos consecutivos para bajar | Entero | Sí | No | — | Cantidad necesaria (por defecto: 2) |
| Umbral de éxito (%) | Entero | Sí | No | — | Porcentaje mínimo (por defecto: 70%) |
| Umbral de frustración | Entero (1-5) | Sí | No | — | Nivel que dispara intervención (por defecto: 3) |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Registro de Ajuste Adaptativo
Historial de cada ajuste realizado por el motor.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Actividad del roadmap | Referencia | Sí | No | — | Actividad ajustada |
| Respuesta de actividad | Referencia | Sí | No | — | Respuesta que disparó el ajuste |
| Tipo de ajuste | Texto | Sí | No | — | DifficultyUp, DifficultyDown, HintsEnabled, HintsDisabled, TimeLimitIncreased, TimeLimitDecreased, AttemptsIncreased, FrustrationIntervention |
| Valor anterior | Texto | Sí | No | — | Valor antes del ajuste (JSON) |
| Valor nuevo | Texto | Sí | No | — | Valor después del ajuste (JSON) |
| Motivo | Texto | Sí | No | — | Explicación del por qué del ajuste |
| Fecha de ajuste | Fecha/hora | Sí | No | — | Cuándo se realizó |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 9. Diagnósticos y Reportes

### Diagnóstico Funcional
Evaluaciones formales registradas por el profesional.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Persona | Referencia | Sí | No | — | Persona evaluada |
| Profesional | Referencia | Sí | No | — | Profesional que registra |
| Fecha del diagnóstico | Fecha | Sí | No | — | Fecha de la evaluación |
| Diagnóstico principal | Texto (255) | Sí | No | — | Diagnóstico resumido |
| Observaciones iniciales | Texto largo | No | No | — | Observaciones del profesional |
| Capacidades identificadas | Texto largo | No | No | — | Fortalezas de la persona |
| Desafíos identificados | Texto largo | No | No | — | Dificultades detectadas |
| Apoyos requeridos | Texto largo | No | No | — | Qué apoyos necesita |
| Objetivos pedagógicos | Texto largo | No | No | — | Metas a trabajar |
| Estrategias recomendadas | Texto largo | No | No | — | Cómo abordar los objetivos |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

### Reporte de Progreso
Reportes formales de avance para un período determinado.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Persona | Referencia | Sí | No | — | Persona reportada |
| Profesional | Referencia | Sí | No | — | Profesional que genera el reporte |
| Tipo de reporte | Referencia | Sí | No | — | Del catálogo de tipos de reporte |
| Título | Texto (200) | Sí | No | — | Título del reporte |
| Contenido | Texto largo | Sí | No | — | Descripción del progreso |
| Fecha del reporte | Fecha | Sí | No | — | Fecha de emisión |
| Inicio del período | Fecha | No | No | — | Desde cuándo abarca |
| Fin del período | Fecha | No | No | — | Hasta cuándo abarca |
| Metas alcanzadas | Texto largo | No | No | — | Objetivos logrados |
| Áreas a reforzar | Texto largo | No | No | — | Donde se necesita más trabajo |
| Recomendaciones futuras | Texto largo | No | No | — | Sugerencias para el siguiente período |
| Próximos objetivos | Texto largo | No | No | — | Metas para el futuro |
| **Estado** | | | |
| Status | Enumerado | Sí | No | Draft | Estado del reporte. Draft = editable. Submitted = en revisión. Approved = visible para familiares. |
| Fecha de envío | Fecha/hora | No | No | — | Cuándo se envió (Submitted) |
| Fecha de aprobación | Fecha/hora | No | No | — | Cuándo fue aprobado/rechazado |
| Motivo de rechazo | Texto (500) | No | No | — | Razón del rechazo (solo si Status = Rejected) |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 10. Comunicación

### Mensaje
Mensajes internos entre profesionales y familiares.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Remitente | Referencia | Sí | No | — | Usuario que envía |
| Destinatario | Referencia | Sí | No | — | Usuario que recibe |
| Persona relacionada | Referencia | No | No | — | Persona con discapacidad sobre la que se habla |
| Asunto | Texto (200) | No | No | — | Asunto del mensaje |
| Contenido | Texto largo | Sí | No | — | Cuerpo del mensaje |
| Fecha de envío | Fecha/hora | Sí | No | — | Cuándo se envió |
| Fecha de lectura | Fecha/hora | No | No | — | Cuándo lo leyó el destinatario |
| Leído | Booleano | Sí | No | — | Si fue leído |
| Mensaje padre | Referencia | No | No | — | Mensaje al que responde (para hilos) |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: entidad desactivada, oculta en la interfaz. |

---

## 11. Auditoría

### Registro de Acceso (`AccessAudit`)
Rastro de auditoría para accesos a datos sensibles. Generado por `ResourceAuthorizationService` en cada verificación de acceso por recurso (capa 3 de autorización — HU-IN-172). Escritura write-behind (fire-and-forget) para no impactar latencia.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | UUID | Sí | No | — | Identificador único del registro |
| UserId | UUID | Sí | No | — | ID del usuario que intentó el acceso |
| Role | Texto (50) | Sí | No | — | Rol del usuario en el momento del acceso (Admin, Professional, FamilyRepresentative, PersonWithDisability) |
| AccessedPersonId | UUID | No | No | — | ID de la persona cuyo dato se intentó acceder (null si el recurso no es una persona directa) |
| ActionType | Texto (50) | Sí | No | — | Tipo de acción: `Read`, `Create`, `Update`, `Delete` |
| Result | Texto (20) | Sí | No | — | Resultado: `Allowed` o `Denied` |
| AffectedTable | Texto (100) | No | No | — | Entidad accedida: `Persons`, `Diagnoses`, `Reports`, etc. |
| AffectedRecordId | Texto (50) | No | No | — | ID del registro específico accedido |
| IpAddress | Texto (45) | No | No | — | Dirección IP del solicitante (IPv4 o IPv6) |
| CorrelationId | Texto (100) | No | No | — | ID de correlación del request HTTP para trazabilidad |
| Timestamp | Fecha/hora UTC | Sí | No | — | Cuándo ocurrió el acceso |
| Details | Texto largo | No | No | — | Información adicional (motivo de denegación, contexto) |

**Notas:**
- Retención propuesta: 2 años para accesos a datos clínicos (Ley 25.326)
- La tabla es append-only: no se actualiza ni elimina
- GlobalAdmin: sus accesos se registran siempre aunque no estén restringidos (cumplimiento ante auditoría)
- Migración: `20260418062012_ExtendAccessAuditResources`

---

## 12. Soporte y Ayuda

> **Estado:** Planificado — estas entidades **no están en el DbContext ni tienen migraciones**. Se documentan como diseño para implementación futura (HU-13).

### Entrada de FAQ
Pregunta frecuente del centro de ayuda, gestionada por el administrador.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Pregunta | Texto (500) | Sí | No | — | La pregunta frecuente |
| Respuesta | Texto largo | Sí | No | — | La respuesta a la pregunta |
| Categoría | Enumerado | Sí | No | — | Cuenta y Acceso, Actividades, Reportes, Comunicación, Accesibilidad, General |
| Orden de visualización | Entero | Sí | No | — | Para ordenar dentro de la categoría |
| Activo | Booleano | Sí | No | true | Soft-delete. En false: elemento de FAQ desactivado, no visible en la app. |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se creó |

### Ticket de Soporte
Reporte de problema o consulta creado por un usuario.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Usuario creador | Referencia | Sí | No | — | Quién reportó el problema |
| Asunto | Texto (200) | Sí | No | — | Título breve del problema |
| Descripción | Texto largo | Sí | No | — | Detalle del problema |
| Categoría | Enumerado | Sí | No | — | Bug, Consulta, Sugerencia |
| Prioridad | Enumerado | Sí | No | — | Baja, Media, Alta |
| Estado | Enumerado | Sí | No | — | Abierto, En Revisión, Respondido, Resuelto, Cerrado |
| URL actual | Texto (500) | No | No | — | Sección de la app donde se reportó (captura automática) |
| User Agent | Texto (500) | No | No | — | Navegador y SO del usuario (captura automática) |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se creó |
| Fecha de actualización | Fecha/hora | Sí | No | — | Última modificación |

### Respuesta de Ticket
Respuesta del administrador a un ticket de soporte.

| Campo | Tipo | Obligatorio | Único | Ejemplo | Descripción |
|-------|------|:-----------:|:-----:|---------|-------------|
| Id | Entero | Sí | Sí | 42 | Identificador interno autoincremental. No visible para el usuario. |
| Ticket | Referencia | Sí | No | — | Ticket al que responde |
| Usuario respondedor | Referencia | Sí | No | — | Admin que respondió |
| Contenido | Texto largo | Sí | No | — | Texto de la respuesta |
| Fecha de creación | Fecha/hora | Sí | No | — | Cuándo se respondió |

---

## Resumen de Entidades

| # | Entidad (`DbSet`) | Área | Relaciones principales |
|---|-------------------|------|------------------------|
| 1 | `DisabilityType` | Catálogo | → Persona |
| 2 | `AutonomyLevel` | Catálogo | → Persona |
| 3 | `ActivityCategory` | Catálogo | → Actividad |
| 4 | `SkillArea` | Catálogo | → PerfilHabilidades, Actividad, Roadmap |
| 5 | `ActivityTemplateType` | Catálogo | → ContenidoActividad |
| 6 | `LoginMethod` | Catálogo | → Persona |
| 7 | `ReportType` | Catálogo | → Reporte |
| 8 | `User` | Auth | → Professional / PersonWithDisability / FamilyRepresentative (1:1) |
| 9 | `RefreshToken` | Auth | → User |
| 10 | `TrustedDevice` | Auth | → User |
| 11 | `Professional` | Perfiles | → Instituciones, Personas, Actividades |
| 12 | `ProfessionalStatusHistory` | Perfiles | → Professional |
| 13 | `PersonWithDisability` | Perfiles | → Profesionales, Familiares, Roadmap |
| 14 | `FamilyRepresentative` | Perfiles | → Personas |
| 15 | `FamilyStatusHistory` | Perfiles | → FamilyRepresentative |
| 16 | `EducationalInstitution` | Instituciones | → Profesionales, Admins |
| 17 | `AdminInstitution` | Instituciones | Admin → Institución |
| 18 | `ProfessionalInstitution` | Instituciones | Professional → Institución |
| 19 | `ProfessionalPerson` | Relaciones | Professional → PersonWithDisability |
| 20 | `PersonRepresentative` | Relaciones | PersonWithDisability → FamilyRepresentative |
| 21 | `PersonRepresentativeHistory` | Relaciones | → PersonRepresentative |
| 22 | `PersonSkillProfile` | Relaciones | PersonWithDisability → SkillArea |
| 23 | `Invitation` | Invitaciones | Professional → FamilyRepresentative |
| 24 | `Activity` | Actividades | Professional, Category, SkillArea |
| 25 | `ActivityContent` | Actividades | Activity (1:1), TemplateType |
| 26 | `ActivityEmbedding` | Actividades | Activity (1:1) — búsqueda semántica |
| 27 | `PersonRoadmap` | Plan de Trabajo | PersonWithDisability (1:1), Professional |
| 28 | `PersonRoadmapArea` | Plan de Trabajo | PersonRoadmap, SkillArea |
| 29 | `PersonRoadmapActivity` | Plan de Trabajo | PersonRoadmapArea, Activity |
| 30 | `ActivityAssignment` | Ejecución | Activity, PersonWithDisability, Professional |
| 31 | `ActivityResponse` | Ejecución | → ActivityAssignment |
| 32 | `ActivityResult` | Ejecución | → PersonRoadmapActivity |
| 33 | `AdaptiveEngineConfig` | MDA | PersonRoadmapActivity (1:1) |
| 34 | `AdaptiveAdjustmentLog` | MDA | PersonRoadmapActivity, ActivityResponse |
| 35 | `Diagnosis` | Clínico | PersonWithDisability, Professional |
| 36 | `Report` | Reportes | PersonWithDisability, Professional, ReportType |
| 37 | `Message` | Comunicación | User (sender/receiver), PersonWithDisability |
| 38 | `AccessAudit` | Auditoría | User, PersonWithDisability |
| — | ~~EntradaFAQ~~ | Soporte *(planificado)* | No implementado — HU-13 |
| — | ~~TicketSoporte~~ | Soporte *(planificado)* | No implementado — HU-13 |
| — | ~~RespuestaTicket~~ | Soporte *(planificado)* | No implementado — HU-13 |
