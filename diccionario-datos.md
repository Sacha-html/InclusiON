# Diccionario de Datos — Sistema InclusiON

**Última actualización:** 2026-03-23

Este documento describe las entidades de datos del sistema InclusiON, organizadas por área funcional. Para cada entidad se listan sus atributos, tipo de dato, obligatoriedad y descripción.

---

## 1. Catálogos de Referencia

### Tipo de Discapacidad
Clasificación de discapacidades utilizada en el alta de personas.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (100) | Sí | Nombre del tipo (único) |
| Descripción | Texto (500) | No | Detalle del tipo |
| Activo | Booleano | Sí | Estado lógico (por defecto: sí) |

### Nivel de Autonomía
Niveles que determinan el método de login y el grado de supervisión requerido.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (100) | Sí | "Alta", "Media", "Baja" |
| Descripción | Texto (500) | No | Detalle del nivel |
| Requiere supervisión | Booleano | Sí | Si la persona necesita acompañamiento |
| Orden de visualización | Entero | Sí | Orden en dropdowns |
| Activo | Booleano | Sí | Estado lógico |

### Categoría de Actividad
Clasificación temática de las actividades educativas.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (100) | Sí | Nombre de la categoría |
| Descripción | Texto (500) | No | Detalle de la categoría |
| Activo | Booleano | Sí | Estado lógico |

### Área de Habilidad
Dominios de competencia que se trabajan con cada persona (Comunicación, Alfabetización, Lógica-Matemática, Conducta, Motricidad).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (100) | Sí | Nombre del área (único) |
| Descripción | Texto (500) | No | Detalle del área |
| Ícono | Texto (50) | No | Identificador del ícono visual |
| Color | Texto (10) | No | Color hexadecimal (#RRGGBB) |
| Orden de visualización | Entero | Sí | Orden en listados |
| Activo | Booleano | Sí | Estado lógico |

### Tipo de Template de Actividad
Plantillas que definen la estructura de contenido de cada tipo de actividad interactiva.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Área de habilidad | Referencia | Sí | Área a la que pertenece |
| Nombre | Texto (150) | Sí | Nombre de la plantilla |
| Código | Texto (único) | Sí | SELECT_FIGURE, VISUAL_SUM, etc. |
| Descripción | Texto (500) | No | Detalle de la plantilla |
| Esquema de contenido | Texto largo | Sí | Estructura JSON que define los campos del formulario dinámico |
| Nombre del componente | Texto (100) | Sí | Componente visual que renderiza la actividad |
| Usa pictogramas | Booleano | Sí | Si el template requiere pictogramas |
| Tiene audio | Booleano | Sí | Si el template soporta audio |
| Orden de visualización | Entero | Sí | Orden en listados |
| Activo | Booleano | Sí | Estado lógico |

### Método de Login
Métodos de autenticación adaptados al nivel de autonomía.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Código | Texto (único) | Sí | STANDARD, PIN, ASSISTED, FAMILY |
| Nombre | Texto (100) | Sí | Nombre descriptivo |
| Descripción | Texto (500) | No | Detalle del método |
| Nivel mínimo de autonomía | Entero | Sí | Autonomía mínima requerida |
| Requiere email | Booleano | Sí | Si necesita email para autenticarse |
| Requiere contraseña | Booleano | Sí | Si necesita contraseña |
| Requiere PIN | Booleano | Sí | Si necesita PIN numérico |
| Requiere supervisor | Booleano | Sí | Si necesita autorización de un supervisor |
| Activo | Booleano | Sí | Estado lógico |

### Tipo de Reporte
Clasificación de los reportes de progreso.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (100) | Sí | Nombre del tipo |
| Descripción | Texto (500) | No | Detalle del tipo |
| Activo | Booleano | Sí | Estado lógico |

---

## 2. Usuarios y Perfiles

### Usuario
Cuenta de acceso al sistema. Cada usuario tiene exactamente un perfil asociado (Profesional, Persona con Discapacidad o Familiar).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Identificador único | Sí | Identificador del usuario |
| Email | Texto (100) | Sí | Email único, usado para login estándar |
| Nombre | Texto (50) | No | Nombre de pila |
| Apellido | Texto (50) | No | Apellido |
| Fecha de creación | Fecha/hora | Sí | Cuándo se creó la cuenta |
| Activo | Booleano | Sí | Estado lógico |
| Último login | Fecha/hora | No | Fecha del último acceso |
| Debe cambiar contraseña | Booleano | Sí | Si tiene contraseña temporal que debe cambiar |

### Profesional
Profesionales que trabajan con personas con discapacidad (docentes, terapeutas, psicólogos).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Identificador único | Sí | Identificador del profesional |
| Usuario | Referencia | Sí | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | Nombre de pila |
| Apellido | Texto (100) | Sí | Apellido |
| DNI | Texto (20) | No | Documento de identidad (único) |
| Teléfono | Texto (20) | No | Número de contacto |
| Especialidad | Texto (100) | No | Área de especialización |
| Matrícula | Texto (50) | No | Número de matrícula profesional (único) |
| Fecha de nacimiento | Fecha | No | Fecha de nacimiento |
| Email | Texto (255) | No | Email del profesional (guardado antes de crear el usuario) |
| **Estado de validación** | | | |
| Status | Enumerado | Sí | `Pending`, `Approved`, `Rejected`, `Suspended`, `Terminated` |
| Fecha de validación | Fecha/hora | No | Cuándo fue validado/aprobado |
| Validado por | Referencia | No | Usuario admin que validó al profesional |
| Activo | Booleano | Sí | Estado lógico |

### Historial de Estados del Profesional
Registro de cada cambio de estado de un profesional.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Identificador único | Sí | Identificador del registro |
| Profesional | Referencia | Sí | Profesional al que corresponde |
| Estado anterior | Enumerado | No | Estado antes del cambio (null si es el primero) |
| Estado nuevo | Enumerado | Sí | Estado después del cambio |
| Observación | Texto (500) | No | Motivo del cambio (ej: motivo de rechazo o desactivación) |
| Modificado por | Referencia | No | Usuario que realizó el cambio |
| Fecha de creación | Fecha/hora | Sí | Cuándo se realizó el cambio |
| Activo | Booleano | Sí | Estado lógico |

### Persona con Discapacidad
Destinatario central del sistema. Recibe planes de trabajo, realiza actividades y su progreso es monitoreado.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Identificador único | Sí | Identificador de la persona |
| Usuario | Referencia | Sí | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | Nombre de pila |
| Apellido | Texto (100) | Sí | Apellido |
| DNI | Texto (20) | No | Documento de identidad (único) |
| Fecha de nacimiento | Fecha | Sí | Fecha de nacimiento |
| Tipo de discapacidad | Referencia | No | Del catálogo de tipos de discapacidad |
| Foto | Texto (500) | No | URL de la foto de perfil |
| **Perfil funcional** | | | |
| Nivel de atención | Entero (1-5) | No | Capacidad de atención |
| Nivel de comunicación | Entero (1-5) | No | Capacidad comunicativa |
| Nivel de motricidad | Entero (1-5) | No | Capacidad motriz |
| Usa CAA | Booleano | No | Si usa Comunicación Aumentativa y Alternativa |
| Usa lengua de señas | Booleano | No | Si se comunica con LSA |
| Intereses y motivadores | Texto (500) | No | Qué le interesa y motiva |
| Estilo de aprendizaje | Texto (50) | No | Visual, Auditivo o Kinestésico |
| Recursos disponibles | Texto (255) | No | Recursos con los que cuenta |
| Terapias adicionales | Texto (500) | No | Otras terapias que recibe |
| **Accesibilidad** | | | |
| Requiere fuente grande | Booleano | No | Ajuste visual |
| Requiere alto contraste | Booleano | No | Ajuste visual |
| Sensibilidad al ruido visual | Booleano | No | Si las animaciones lo perturban |
| Sensibilidad al sonido | Booleano | No | Si los sonidos lo perturban |
| **Autenticación** | | | |
| Nivel de autonomía | Referencia | No | Del catálogo de niveles de autonomía |
| Método de login | Referencia | No | Del catálogo de métodos de login |
| Color de avatar | Texto (20) | No | Color hexadecimal para identificación visual |
| Activo | Booleano | Sí | Estado lógico |

### Representante Familiar
Familiares o tutores que acompañan el proceso de la persona con discapacidad.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Identificador único | Sí | Identificador del familiar |
| Usuario | Referencia | Sí | Cuenta de usuario asociada (1:1) |
| Nombre | Texto (100) | Sí | Nombre de pila |
| Apellido | Texto (100) | Sí | Apellido |
| DNI | Texto (20) | No | Documento de identidad (único) |
| Teléfono | Texto (20) | No | Número de contacto |
| Relación | Texto (50) | No | Madre, Padre, Tutor, Abuelo, etc. |
| Onboarding completado | Booleano | Sí | Si el familiar completó la pantalla de bienvenida. Default: false |
| Activo | Booleano | Sí | Estado lógico |

---

## 3. Instituciones y Relaciones

### Institución Educativa
Escuelas, centros de rehabilitación o instituciones donde trabajan los profesionales.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Nombre | Texto (255) | Sí | Nombre de la institución |
| Dirección | Texto (255) | No | Domicilio |
| Teléfono | Texto (20) | No | Número de contacto |
| Email | Texto (100) | No | Email institucional |
| Activo | Booleano | Sí | Estado lógico |

### Admin ↔ Institución
Vinculación de administradores institucionales a sus instituciones.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Usuario admin | Referencia | Sí | El administrador asignado |
| Institución | Referencia | Sí | La institución asignada |
| Fecha de asignación | Fecha/hora | Sí | Cuándo se estableció la relación |
| Activo | Booleano | Sí | Estado lógico |

### Profesional ↔ Institución
Vinculación de profesionales a las instituciones donde trabajan.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Profesional | Referencia | Sí | El profesional asignado |
| Institución | Referencia | Sí | La institución donde trabaja |
| Fecha de asignación | Fecha/hora | Sí | Cuándo se estableció la relación |
| Activo | Booleano | Sí | Estado lógico |

### Profesional ↔ Persona
Vinculación de profesionales a las personas que atienden.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Profesional | Referencia | Sí | El profesional asignado |
| Persona | Referencia | Sí | La persona atendida |
| Fecha de asignación | Fecha/hora | Sí | Cuándo se estableció la relación |
| Es profesional principal | Booleano | Sí | Si es el profesional principal de la persona |
| Puede supervisar login | Booleano | Sí | Si puede autorizar el login asistido |
| Activo | Booleano | Sí | Estado lógico |

### Persona ↔ Familiar
Vinculación de personas con discapacidad a sus representantes familiares.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Persona | Referencia | Sí | La persona con discapacidad |
| Familiar | Referencia | Sí | El representante familiar |
| Es primario | Booleano | Sí | Si es el representante principal |
| Tiene consentimiento informado | Booleano | Sí | Si firmó consentimiento |
| Fecha de consentimiento | Fecha | No | Cuándo firmó el consentimiento |
| Puede supervisar login | Booleano | Sí | Si puede autorizar el login asistido |
| Activo | Booleano | Sí | Estado lógico |

### Perfil de Habilidades (Persona ↔ Área)
Áreas de habilidad asignadas a cada persona para trabajar.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Persona | Referencia | Sí | La persona con discapacidad |
| Área de habilidad | Referencia | Sí | El área a trabajar |
| Fecha de asignación | Fecha/hora | Sí | Cuándo se asignó |
| Activo | Booleano | Sí | Estado lógico |

---

## 4. Invitaciones

### Invitación
Invitaciones por email para el registro de familiares.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Profesional creador | Referencia | Sí | Quién generó la invitación |
| Persona asociada | Referencia | No | Persona con discapacidad vinculada |
| Código | Texto (único) | Sí | Código único del link de registro |
| Email | Texto (100) | Sí | Email del familiar invitado |
| Nombre | Texto (100) | No | Nombre del familiar |
| Apellido | Texto (100) | No | Apellido del familiar |
| Relación | Texto (50) | No | Relación con la persona (Madre, Padre, etc.) |
| Fecha de expiración | Fecha/hora | Sí | Cuándo expira la invitación (7 días) |
| Usada | Booleano | Sí | Si ya fue aceptada |
| Fecha de uso | Fecha/hora | No | Cuándo fue aceptada |
| Usada por | Referencia | No | Usuario que aceptó la invitación |
| Activo | Booleano | Sí | Estado lógico |

---

## 5. Actividades y Contenido

### Actividad
Actividades educativas creadas por profesionales o predefinidas por el sistema.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Profesional creador | Referencia | Sí | Quién creó la actividad |
| Categoría | Referencia | Sí | Categoría de actividad (del catálogo) |
| Área de habilidad | Referencia | No | Área a la que pertenece |
| Título | Texto (150) | Sí | Nombre de la actividad |
| Descripción | Texto largo | No | Descripción detallada |
| Instrucciones | Texto largo | No | Instrucciones para realizar la actividad |
| Soporte visual | Booleano | No | Si incluye soporte visual |
| Soporte auditivo | Booleano | No | Si incluye soporte auditivo |
| Lectura fácil | Booleano | No | Si usa lectura simplificada |
| Usa pictogramas | Booleano | No | Si usa pictogramas ARASAAC |
| URL de recursos | Texto (500) | No | Enlace a recursos externos |
| Duración estimada (min) | Entero | No | Minutos estimados de duración |
| Nivel de complejidad | Entero (1-5) | No | Nivel de dificultad |
| Requiere supervisión | Booleano | Sí | Si necesita supervisor presente |
| Es actividad estándar | Booleano | Sí | Si es del sistema (no editable) |
| Activo | Booleano | Sí | Estado lógico |

### Contenido de Actividad
Contenido interactivo vinculado a una actividad y su plantilla (1:1 con Actividad).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Actividad | Referencia | Sí | Actividad asociada (1:1) |
| Tipo de template | Referencia | Sí | Plantilla que define la estructura |
| Contenido JSON | Texto largo | Sí | Contenido dinámico en formato JSON según el esquema de la plantilla |
| Activo | Booleano | Sí | Estado lógico |

---

## 6. Roadmap (Plan de Trabajo)

### Roadmap de la Persona
Plan de trabajo personalizado (1:1 con Persona).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Persona | Referencia | Sí | Persona con discapacidad (1:1) |
| Profesional creador | Referencia | Sí | Quién armó el roadmap |
| Notas | Texto (2000) | No | Observaciones generales |
| Activo | Booleano | Sí | Estado lógico |

### Área del Roadmap
Sección del roadmap correspondiente a un área de habilidad.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Roadmap | Referencia | Sí | Roadmap al que pertenece |
| Área de habilidad | Referencia | Sí | Área de habilidad (única por roadmap) |
| Orden de visualización | Entero | Sí | Orden de las áreas |
| Activo | Booleano | Sí | Estado lógico |

### Actividad del Roadmap
Actividad dentro de un área del roadmap, con parámetros de personalización.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Área del roadmap | Referencia | Sí | Área a la que pertenece |
| Actividad | Referencia | Sí | Actividad asignada (única por área) |
| Orden secuencial | Entero | Sí | Posición en la secuencia (único por área) |
| Desbloqueada | Booleano | Sí | Si la persona puede acceder |
| Fecha de desbloqueo | Fecha/hora | No | Cuándo se desbloqueó |
| Umbral de desbloqueo (%) | Entero | Sí | Porcentaje mínimo de éxito para desbloquear la siguiente (por defecto: 60%) |
| Tiempo límite (seg) | Entero | No | Tiempo máximo para completar |
| Máximo de intentos | Entero | No | Intentos permitidos |
| Mostrar pistas | Booleano | Sí | Si se muestran ayudas |
| Nivel de dificultad | Entero (1-3) | Sí | Dificultad actual (por defecto: 1) |
| Activo | Booleano | Sí | Estado lógico |

---

## 7. Asignaciones y Respuestas

### Asignación de Actividad
Actividad asignada a una persona con discapacidad.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Actividad | Referencia | Sí | Actividad asignada |
| Persona | Referencia | Sí | Persona asignada |
| Profesional asignador | Referencia | Sí | Quién hizo la asignación |
| Fecha de asignación | Fecha/hora | Sí | Cuándo se asignó |
| Fecha límite | Fecha/hora | No | Fecha de vencimiento |
| Estado | Texto | Sí | Pendiente, EnProgreso, Completada, Cancelada |
| Orden secuencial | Entero | No | Posición en la secuencia |
| Es actividad de evaluación | Booleano | Sí | Si es parte de la evaluación inicial |
| Activo | Booleano | Sí | Estado lógico |

### Respuesta de Actividad
Registro de cada intento de resolución de una actividad asignada.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Asignación | Referencia | Sí | Asignación a la que responde |
| Fecha de inicio | Fecha/hora | Sí | Cuándo comenzó el intento |
| Fecha de finalización | Fecha/hora | No | Cuándo terminó |
| Tiempo empleado (seg) | Entero | No | Segundos que tardó |
| Resultado | Texto | No | Éxito, Parcial, Fallido, Abandonado |
| Porcentaje de éxito | Decimal (0-100) | No | Resultado numérico |
| Cantidad de intentos | Entero | Sí | Número de intentos realizados |
| Patrón de respuesta | Texto largo | No | Registro JSON de las respuestas individuales |
| Requirió soporte | Booleano | Sí | Si necesitó ayuda de un supervisor |
| Nivel de frustración | Entero (1-5) | No | Nivel de frustración detectado |
| Observaciones | Texto (1000) | No | Notas del profesional o del sistema |
| Activo | Booleano | Sí | Estado lógico |

---

## 8. Motor de Dificultad Adaptativa

### Configuración Adaptativa
Parámetros del motor de dificultad adaptativa para cada actividad del roadmap (1:0..1 con Actividad del Roadmap).

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Actividad del roadmap | Referencia | Sí | Actividad asociada (1:1) |
| Habilitado | Booleano | Sí | Si el motor está activo |
| Dificultad mínima | Entero | Sí | Nivel mínimo (por defecto: 1) |
| Dificultad máxima | Entero | Sí | Nivel máximo (por defecto: 5) |
| Tiempo mínimo (seg) | Entero | No | Tiempo límite mínimo |
| Tiempo máximo (seg) | Entero | No | Tiempo límite máximo |
| Éxitos consecutivos para subir | Entero | Sí | Cantidad necesaria (por defecto: 3) |
| Fracasos consecutivos para bajar | Entero | Sí | Cantidad necesaria (por defecto: 2) |
| Umbral de éxito (%) | Entero | Sí | Porcentaje mínimo (por defecto: 70%) |
| Umbral de frustración | Entero (1-5) | Sí | Nivel que dispara intervención (por defecto: 3) |
| Activo | Booleano | Sí | Estado lógico |

### Registro de Ajuste Adaptativo
Historial de cada ajuste realizado por el motor.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Actividad del roadmap | Referencia | Sí | Actividad ajustada |
| Respuesta de actividad | Referencia | Sí | Respuesta que disparó el ajuste |
| Tipo de ajuste | Texto | Sí | DifficultyUp, DifficultyDown, HintsEnabled, HintsDisabled, TimeLimitIncreased, TimeLimitDecreased, AttemptsIncreased, FrustrationIntervention |
| Valor anterior | Texto | Sí | Valor antes del ajuste (JSON) |
| Valor nuevo | Texto | Sí | Valor después del ajuste (JSON) |
| Motivo | Texto | Sí | Explicación del por qué del ajuste |
| Fecha de ajuste | Fecha/hora | Sí | Cuándo se realizó |
| Activo | Booleano | Sí | Estado lógico |

---

## 9. Diagnósticos y Reportes

### Diagnóstico Funcional
Evaluaciones formales registradas por el profesional.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Persona | Referencia | Sí | Persona evaluada |
| Profesional | Referencia | Sí | Profesional que registra |
| Fecha del diagnóstico | Fecha | Sí | Fecha de la evaluación |
| Diagnóstico principal | Texto (255) | Sí | Diagnóstico resumido |
| Observaciones iniciales | Texto largo | No | Observaciones del profesional |
| Capacidades identificadas | Texto largo | No | Fortalezas de la persona |
| Desafíos identificados | Texto largo | No | Dificultades detectadas |
| Apoyos requeridos | Texto largo | No | Qué apoyos necesita |
| Objetivos pedagógicos | Texto largo | No | Metas a trabajar |
| Estrategias recomendadas | Texto largo | No | Cómo abordar los objetivos |
| Activo | Booleano | Sí | Estado lógico |

### Reporte de Progreso
Reportes formales de avance para un período determinado.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Persona | Referencia | Sí | Persona reportada |
| Profesional | Referencia | Sí | Profesional que genera el reporte |
| Tipo de reporte | Referencia | Sí | Del catálogo de tipos de reporte |
| Título | Texto (200) | Sí | Título del reporte |
| Contenido | Texto largo | Sí | Descripción del progreso |
| Fecha del reporte | Fecha | Sí | Fecha de emisión |
| Inicio del período | Fecha | No | Desde cuándo abarca |
| Fin del período | Fecha | No | Hasta cuándo abarca |
| Metas alcanzadas | Texto largo | No | Objetivos logrados |
| Áreas a reforzar | Texto largo | No | Donde se necesita más trabajo |
| Recomendaciones futuras | Texto largo | No | Sugerencias para el siguiente período |
| Próximos objetivos | Texto largo | No | Metas para el futuro |
| Activo | Booleano | Sí | Estado lógico |

---

## 10. Comunicación

### Mensaje
Mensajes internos entre profesionales y familiares.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Remitente | Referencia | Sí | Usuario que envía |
| Destinatario | Referencia | Sí | Usuario que recibe |
| Persona relacionada | Referencia | No | Persona con discapacidad sobre la que se habla |
| Asunto | Texto (200) | No | Asunto del mensaje |
| Contenido | Texto largo | Sí | Cuerpo del mensaje |
| Fecha de envío | Fecha/hora | Sí | Cuándo se envió |
| Fecha de lectura | Fecha/hora | No | Cuándo lo leyó el destinatario |
| Leído | Booleano | Sí | Si fue leído |
| Mensaje padre | Referencia | No | Mensaje al que responde (para hilos) |
| Activo | Booleano | Sí | Estado lógico |

---

## 11. Auditoría

### Registro de Acceso
Rastro de auditoría para accesos a datos sensibles.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Usuario | Referencia | Sí | Quién accedió |
| Persona accedida | Referencia | No | Datos de qué persona se consultaron |
| Tipo de acción | Texto (50) | Sí | Lectura, Creación, Modificación, Eliminación |
| Tabla afectada | Texto (100) | No | Qué entidad se accedió |
| Registro afectado | Texto (50) | No | ID del registro accedido |
| Dirección IP | Texto (45) | No | IP desde donde se accedió |
| Fecha y hora | Fecha/hora | Sí | Cuándo ocurrió |
| Detalles | Texto largo | No | Información adicional |

---

## 12. Soporte y Ayuda

### Entrada de FAQ
Pregunta frecuente del centro de ayuda, gestionada por el administrador.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Pregunta | Texto (500) | Sí | La pregunta frecuente |
| Respuesta | Texto largo | Sí | La respuesta a la pregunta |
| Categoría | Enumerado | Sí | Cuenta y Acceso, Actividades, Reportes, Comunicación, Accesibilidad, General |
| Orden de visualización | Entero | Sí | Para ordenar dentro de la categoría |
| Activo | Booleano | Sí | Estado lógico (soft-delete) |
| Fecha de creación | Fecha/hora | Sí | Cuándo se creó |

### Ticket de Soporte
Reporte de problema o consulta creado por un usuario.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Usuario creador | Referencia | Sí | Quién reportó el problema |
| Asunto | Texto (200) | Sí | Título breve del problema |
| Descripción | Texto largo | Sí | Detalle del problema |
| Categoría | Enumerado | Sí | Bug, Consulta, Sugerencia |
| Prioridad | Enumerado | Sí | Baja, Media, Alta |
| Estado | Enumerado | Sí | Abierto, En Revisión, Respondido, Resuelto, Cerrado |
| URL actual | Texto (500) | No | Sección de la app donde se reportó (captura automática) |
| User Agent | Texto (500) | No | Navegador y SO del usuario (captura automática) |
| Fecha de creación | Fecha/hora | Sí | Cuándo se creó |
| Fecha de actualización | Fecha/hora | Sí | Última modificación |

### Respuesta de Ticket
Respuesta del administrador a un ticket de soporte.

| Atributo | Tipo | Obligatorio | Descripción |
|----------|------|:-----------:|-------------|
| Id | Entero | Sí | Identificador único |
| Ticket | Referencia | Sí | Ticket al que responde |
| Usuario respondedor | Referencia | Sí | Admin que respondió |
| Contenido | Texto largo | Sí | Texto de la respuesta |
| Fecha de creación | Fecha/hora | Sí | Cuándo se respondió |

---

## Resumen de Entidades

| # | Entidad | Área | Relaciones principales |
|---|---------|------|------------------------|
| 1 | Tipo de Discapacidad | Catálogo | → Persona |
| 2 | Nivel de Autonomía | Catálogo | → Persona |
| 3 | Categoría de Actividad | Catálogo | → Actividad |
| 4 | Área de Habilidad | Catálogo | → Perfil de Habilidades, Actividad, Roadmap |
| 5 | Tipo de Template | Catálogo | → Contenido de Actividad |
| 6 | Método de Login | Catálogo | → Persona |
| 7 | Tipo de Reporte | Catálogo | → Reporte |
| 8 | Usuario | Usuarios | → Profesional / Persona / Familiar (1:1) |
| 9 | Profesional | Usuarios | → Instituciones, Personas, Actividades |
| 9b | Historial de Estados | Usuarios | → Profesional |
| 10 | Persona con Discapacidad | Usuarios | → Profesionales, Familiares, Roadmap |
| 11 | Representante Familiar | Usuarios | → Personas |
| 12 | Institución Educativa | Instituciones | → Profesionales, Admins |
| 13 | Admin ↔ Institución | Relaciones | Admin → Institución |
| 14 | Profesional ↔ Institución | Relaciones | Profesional → Institución |
| 15 | Profesional ↔ Persona | Relaciones | Profesional → Persona |
| 16 | Persona ↔ Familiar | Relaciones | Persona → Familiar |
| 17 | Perfil de Habilidades | Relaciones | Persona → Área |
| 18 | Invitación | Invitaciones | Profesional → Familiar |
| 19 | Actividad | Actividades | Profesional, Categoría, Área |
| 20 | Contenido de Actividad | Actividades | Actividad (1:1), Template |
| 21 | Roadmap | Plan de Trabajo | Persona (1:1), Profesional |
| 22 | Área del Roadmap | Plan de Trabajo | Roadmap, Área de Habilidad |
| 23 | Actividad del Roadmap | Plan de Trabajo | Área del Roadmap, Actividad |
| 24 | Asignación de Actividad | Ejecución | Actividad, Persona, Profesional |
| 25 | Respuesta de Actividad | Ejecución | Asignación |
| 26 | Configuración Adaptativa | MDA | Actividad del Roadmap (1:1) |
| 27 | Registro de Ajuste | MDA | Actividad del Roadmap, Respuesta |
| 28 | Diagnóstico Funcional | Clínico | Persona, Profesional |
| 29 | Reporte de Progreso | Reportes | Persona, Profesional, Tipo |
| 30 | Mensaje | Comunicación | Remitente, Destinatario, Persona |
| 31 | Registro de Acceso | Auditoría | Usuario, Persona |
| 32 | Entrada de FAQ | Soporte | — |
| 33 | Ticket de Soporte | Soporte | Usuario creador |
| 34 | Respuesta de Ticket | Soporte | Ticket, Usuario respondedor |
