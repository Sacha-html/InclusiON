# Módulo 2 — Gestión de Usuarios

Este módulo abarca los procesos institucionales y pedagógicos para la incorporación de docentes, estudiantes y familias al centro escolar, así como el gobierno de credenciales y accesos de la sede.

---

## CU-05: Registrar e Incorporar Profesional al Centro Escolar

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo (Director/a, Vicedirector/a o Secretaría) |
| **Actores secundarios** | Sistema (generación de credenciales y envío de notificación), Profesional Docente (receptor) |
| **HU de referencia** | HU-IN-148 / HU-IN-150 |
| **Prioridad** | Crítica |
| **Precondiciones** | La autoridad escolar se encuentra autenticada en el panel de gestión institucional. |

**Flujo principal**
1. La Dirección o Secretaría accede al módulo **Profesionales** y selecciona **"Nuevo Profesional"**.
2. Completa la ficha oficial del docente o terapeuta:
   - Nombre y Apellido.
   - Correo electrónico oficial (con validación asíncrona de disponibilidad).
   - DNI / Documento de identidad y teléfono de contacto.
   - Especialidad pedagógica o terapéutica (seleccionada del catálogo institucional).
   - Número de Matrícula profesional habilitante.
   - Fecha de nacimiento (validando mayoría de edad, mínimo 18 años).
3. El sistema valida en tiempo real que el correo, DNI y matrícula no se encuentren asignados previamente.
4. El directivo confirma el registro.
5. El sistema da de alta el usuario, crea el legajo del profesional vinculado automáticamente a la sede escolar en estado `Habilitado / Activo`, genera una contraseña temporal segura y activa el flag `MustChangePassword = true`.
6. El sistema envía automáticamente las credenciales de acceso al correo del profesional en segundo plano.
7. El sistema despliega un modal institucional con las credenciales temporales generadas para que la dirección pueda informarlas al profesional en mano si fuera necesario, y redirige al legajo del nuevo integrante.

**Flujos alternativos**
- **2a. Menor de edad:** El sistema advierte: *"La persona debe ser mayor de 18 años"* y bloquea el guardado.
- **3a. Email institucional ya registrado:** El sistema muestra error inline y desactiva el botón de guardado.
- **3b. Matrícula profesional duplicada:** El sistema advierte el conflicto registral de la matrícula.

**Postcondiciones**
- El profesional queda habilitado institucionalmente para recibir aulas, grupos y estudiantes asignados.
- En su primer inicio de sesión, el sistema le exigirá de forma obligatoria cambiar la contraseña temporal por una personal.

---

## CU-06: Asignar Aulas y Estudiantes a Profesional

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo (Director/a, Vicedirector/a o Secretaría) |
| **Actores secundarios** | Profesional Docente |
| **HU de referencia** | HU-02 / HU-IN-151 |
| **Prioridad** | Alta |
| **Precondiciones** | El profesional se encuentra dado de alta y habilitado en la institución escolar. |

**Flujo principal**
1. La autoridad escolar accede al legajo de un Profesional o a la sección **Aulas**.
2. Selecciona las aulas o grupos pedagógicos a los cuales se integrará el profesional.
3. El sistema lista los estudiantes que conforman cada aula con sus perfiles de apoyo.
4. La dirección confirma la asignación del profesional como docente titular o de apoyo.
5. El sistema actualiza los permisos de acceso del profesional para que pueda consultar las historias pedagógicas, registrar sesiones y emitir informes únicamente de los estudiantes a su cargo.

**Postcondiciones**
- El profesional visualiza en su portal (`/pro`) a sus alumnos asignados y sus agendas de trabajo.

---

## CU-07: Registrar Estudiante (Persona con Discapacidad)

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo o Profesional Titular Asignado |
| **Actores secundarios** | Sistema (inicialización de Roadmap pedagógico) |
| **HU de referencia** | HU-01 |
| **Prioridad** | Crítica |
| **Precondiciones** | Usuario escolar autenticado con permisos de gestión de alumnos. |

**Flujo principal**
1. El responsable accede a la sección **Alumnos / Personas** y presiona **"Nuevo Alumno"**.
2. Completa los datos personales y escolares: Nombre, Apellido, DNI y Fecha de nacimiento.
3. Asigna los parámetros pedagógicos y de apoyo:
   - Tipo de discapacidad (del catálogo oficial).
   - Nivel de autonomía inicial (Alto, Medio, Con Asistencia Continua).
4. Configura el método de interacción y login adaptativo del alumno:
   - **Visual estándar:** acceso mediante avatar y selección gráfica guiada.
   - **PIN numérico:** código numérico simplificado para uso asistido en tablet.
   - **Asistido por docente/tutor:** sin requerimiento de autenticación individual autónoma.
5. El sistema valida los datos y da de alta al estudiante en la institución.
6. El sistema inicializa la estructura de su trayectoria formativa (Roadmap adaptativo).
7. Se despliega la ficha pedagógica del alumno lista para que el equipo docente complete la evaluación funcional inicial (DUA).

**Flujos alternativos**
- **4a. Método por PIN:** El sistema solicita un código de 4 dígitos numéricos, el cual se encripta de forma segura (Argon2id) antes de persistir.

**Postcondiciones**
- El estudiante queda registrado formalmente en la nómina escolar y disponible para asignación de aula, vinculación familiar y trabajo con actividades adaptadas.

---

## CU-08: Invitar y Vincular Familiar al Sistema

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente (a cargo del alumno) o Equipo Directivo |
| **Actores secundarios** | Sistema (despacho de invitación), Familiar / Representante (destinatario) |
| **HU de referencia** | HU-04 |
| **Prioridad** | Alta |
| **Precondiciones** | El alumno ya se encuentra registrado y el profesional tiene asignada su atención pedagógica. |

**Flujo principal**
1. El docente o directivo accede a la ficha del alumno asignado o al listado de **Invitaciones Familiares**.
2. Selecciona la opción **"Invitar Familiar / Tutor"**.
3. Completa los datos del tutor: Nombre, Apellido, Email de contacto y Relación de parentesco (Madre, Padre, Tutor/a Legal, etc.).
4. El sistema valida que el profesional tenga atribución formal sobre el estudiante y verifica que no exista una invitación vigente para ese correo y alumno.
5. El sistema genera un enlace de invitación seguro con token criptográfico único y validez de 7 días.
6. El sistema envía automáticamente un correo formal de bienvenida con el enlace de registro al familiar.
7. La invitación se registra con estado **`Enviada`** en el tablero del profesional, mostrando el código y permitiendo copiar el link directo si el centro requiere enviarlo por canal directo (WhatsApp institucional o cuaderno escolar).

**Flujos alternativos**
- **4a. Invitación previa existente y vigente:** El sistema informa la fecha de envío anterior y ofrece la opción de **"Reenviar invitación"**, renovando su vigencia.
- **4b. Familiar ya registrado y vinculado:** El sistema advierte que el representante ya cuenta con credenciales activas vinculadas a dicho estudiante.

**Postcondiciones**
- La invitación queda registrada y a la espera de ser completada por la familia.

---

## CU-09: Completar Registro de Familiar por Invitación

| Campo | Detalle |
|---|---|
| **Actor principal** | Familiar / Tutor Legal (no autenticado) |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-04 |
| **Prioridad** | Alta |
| **Precondiciones** | El familiar cuenta con un enlace de invitación con vigencia inferior a 7 días y no utilizado previamente. |

**Flujo principal**
1. El familiar abre el enlace de invitación recibido en su navegador (`#/invite/{token}`).
2. El sistema valida el token de seguridad y presenta el formulario de alta con los datos institucionales precargados y bloqueados para edición:
   - Nombre y Apellido.
   - Vínculo/Parentesco.
   - Nombre del estudiante al que acompaña.
3. El familiar completa su DNI y define una contraseña personal (mínimo 8 caracteres, mayúscula, minúscula, número y símbolo).
4. El familiar confirma el registro.
5. El sistema activa la cuenta de usuario, crea el legajo de `Representante Familiar` y formaliza el vínculo legal `PersonRepresentative` con su hijo/a o representado/a.
6. La invitación pasa a estado **`Aceptada`**.
7. El sistema muestra un mensaje de confirmación y redirige al familiar a la pantalla de ingreso para acceder inmediatamente a su portal (`/family`).

**Flujos alternativos**
- **1a. Invitación vencida (más de 7 días):** El sistema muestra el mensaje: *"Este enlace de invitación ha expirado. Por favor, solicita a la institución escolar que te reenvíe una nueva invitación"*.
- **1b. Invitación ya utilizada:** El sistema indica: *"Esta invitación ya fue completada previamente"* y ofrece botón para ir directo al inicio de sesión.
- **3a. Contraseña débil o no coincidente:** El sistema señala los requisitos no cumplidos en tiempo real y bloquea el botón de confirmación.

**Postcondiciones**
- El familiar dispone de cuenta activa y puede consultar el progreso, comunicados, reportes aprobados e interactuar con el equipo escolar.

---

## CU-10: Gestionar Cuentas y Credenciales Institucionales

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo (Director/a, Vicedirector/a o Secretaría) |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-11 |
| **Prioridad** | Alta |
| **Precondiciones** | Usuario directivo autenticado con atribución de gestión sobre su centro educativo. |

**Flujo principal — Restablecer Contraseña Olvidada del Personal**
1. La autoridad escolar ingresa al módulo de **Gestión de Usuarios** de su institución.
2. Filtra por rol (Docente, Familiar, Alumno) o busca por apellido/nombre.
3. Selecciona al usuario y presiona **"Resetear Contraseña"**.
4. El sistema revoca de forma inmediata todas las sesiones activas del usuario, genera una nueva clave temporal segura y activa el requerimiento `MustChangePassword`.
5. El sistema exhibe la clave temporal una única vez para que la dirección pueda comunicársela de forma segura al usuario.

**Flujo alternativo — Desactivación / Suspensión de Cuenta**
- **2a.** La autoridad selecciona **"Desactivar Cuenta"** ante una baja laboral o desvinculación institucional.
- **3a.** El sistema aplica soft-delete (`IsActive = false`), revoca de inmediato tokens de acceso vigentes y registra la auditoría institucional en `AccessAudit`.

**Flujo alternativo — Reactivación de Cuenta**
- **2b.** La dirección selecciona **"Reactivar Cuenta"**.
- **3b.** El sistema rehabilita el usuario, genera una nueva credencial provisoria y exige cambio de clave en el próximo acceso.

**Restricciones de Negocio**
- Ninguna autoridad puede desactivar su propia cuenta en sesión.
- La gestión está estrictamente acotada al personal y familias pertenecientes a la propia sede escolar (sin visibilidad transversal de otras instituciones).
- Toda acción de revocación, blanqueo o cambio de estado queda asentada en la bitácora de auditoría.
