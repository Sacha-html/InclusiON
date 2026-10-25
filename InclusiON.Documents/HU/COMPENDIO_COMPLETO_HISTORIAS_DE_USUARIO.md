# 📘 Compendio Exhaustivo y Maestro de Historias de Usuario (HU) — InclusiON

> **Proyecto:** InclusiON — Plataforma Integral de Gestión Escolar, Aprendizaje Accesible y Comunicación Aumentativa Adaptada  
> **Formato:** Desglose individual y estructurado (*Jira Official Template + Confluence Matrix + Story Map*)  
> **Normativa y Estándares Aplicados:** Resolución CFE N.º 311/2016 (Educación Especial), Ley 25.326 (Protección de Datos Personales), Pautas WCAG 2.2 AA/AAA, Clasificación Internacional del Funcionamiento (CIF/OMS) y Diseño Universal para el Aprendizaje (DUA).

---

## 📑 Tabla de Contenidos

1. [Sprint 0 — Fundacionales de Arquitectura y Planificación (IN-14 a IN-20)](#1-sprint-0--fundacionales-de-arquitectura-y-planificación)
2. [Sprint 1 — Configuración del Sistema y Multi-Tenant (IN-21 a IN-35)](#2-sprint-1--configuración-del-sistema-y-multi-tenant)
3. [Sprint 2 — Gestión de Usuarios, Legajos y Asignaciones (IN-36 a IN-64)](#3-sprint-2--gestión-de-usuarios-legajos-y-asignaciones)
4. [Sprint 3 — Autenticación, Accesibilidad y Perfiles Visuales (IN-65 a IN-80)](#4-sprint-3--autenticación-accesibilidad-y-perfiles-visuales)
5. [Sprint 4 — Diagnóstico Funcional, Historial Evolutivo y Catálogos CIF (IN-81 a IN-104)](#5-sprint-4--diagnóstico-funcional-historial-evolutivo-y-catálogos-cif)
6. [Sprint 5 — Reportes Pedagógicos Oficiales QuestPDF y Portal Familiar (IN-105 a IN-109)](#6-sprint-5--reportes-pedagógicos-oficiales-questpdf-y-portal-familiar)
7. [Sprint 6 y 10 — Sendero Gamificado "Mi Camino", Players y Modelo de Negocio (IN-110 a IN-115, IN-200 a IN-222)](#7-sprint-6-y-10--sendero-gamificado-mi-camino-players-y-modelo-de-negocio)
8. [Sprint 7 — Tableros Analíticos, Gráficos de Alto Contraste y Monitoreo (IN-116 a IN-121)](#8-sprint-7--tableros-analíticos-gráficos-de-alto-contraste-y-monitoreo)
9. [Sprint 11 — Aulas, Registro Unificado y Login Adaptado Exclusivo (IN-301 a IN-315)](#9-sprint-11--aulas-registro-unificado-y-login-adaptado-exclusivo)
10. [Sprint 12 — Gamificación Avanzada, Casos Borde y Red Colaborativa (IN-320 a IN-340)](#10-sprint-12--gamificación-avanzada-casos-borde-y-red-colaborativa)
11. [Matriz de Trazabilidad: 19 Procesos de Negocio Escolares (US-01 a US-31)](#11-matriz-de-trazabilidad-19-procesos-de-negocio-escolares)

---

## 1. Sprint 0 — Fundacionales de Arquitectura y Planificación

### HU [IN-14]: Definición de Roles del Equipo y Asignación Funcional
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** equipo de desarrollo de InclusiON, **quiero** definir y consensuar los roles funcionales, técnicos y de calidad del proyecto, **para** garantizar responsabilidades claras en Scrum y evitar superposiciones operativas.
* **Criterios de Aceptación:**
  - Asignación formal de roles: Product Owner, Scrum Master, Arquitectos Backend (.NET) y Especialistas Frontend (Angular/Accesibilidad).
  - Publicación del acta constitutiva en el repositorio documental.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-15]: Selección y Homologación de Herramientas
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** equipo de desarrollo, **quiero** seleccionar y validar el ecosistema de herramientas colaborativas (Jira, GitHub, Visual Studio, VS Code, Teams), **para** unificar el flujo de trabajo y la comunicación del proyecto.
* **Criterios de Aceptación:**
  - Canales de comunicación y sincronización de Jira con GitHub configurados.
  - Linters, formatters y plugins de accesibilidad homologados en editores.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-16]: Estructuración de Repositorios y Políticas Git
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** DevOps / Arquitecto de Software, **quiero** inicializar los repositorios con Gitflow y ramas protegidas, **para** asegurar la trazabilidad del código y la revisión obligatoria por Pull Requests.
* **Criterios de Aceptación:**
  - Repositorios creados: `InclusiON.Server`, `InclusiON.Client`, `InclusiON.Documents`.
  - Reglas de protección en rama `main` y convención de commits convencionales activada.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-17]: Análisis y Estimación del Product Backlog Inicial
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** Product Owner y Scrum Team, **queremos** desglosar las necesidades de educación especial en un backlog priorizado con Story Points, **para** planificar los compromisos de entrega de los sprints.
* **Criterios de Aceptación:**
  - Relevamiento de normativas CFE 311/16 y WCAG 2.2.
  - Definición de 312 Story Points distribuidos a lo largo del ciclo formativo.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-18]: Definición de Ceremonias, DoR y DoD
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** Scrum Master, **quiero** fijar las definiciones de preparado (*Definition of Ready*) y terminado (*Definition of Done*), **para** mantener altos estándares de calidad y accesibilidad en cada incremento de software.
* **Criterios de Aceptación:**
  - DoR: HU con criterios medibles y diseño UI adaptado.
  - DoD: Pruebas unitarias aprobadas, validación de contrastes WCAG AA y revisión de PR.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-19]: Selección y Prueba de Concepto de la Plataforma Base
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** Arquitecto de Software, **quiero** validar la compatibilidad de ASP.NET Core 10, Angular 20 y PostgreSQL 17 con pgvector, **para** garantizar la viabilidad técnica antes del desarrollo masivo.
* **Criterios de Aceptación:**
  - Prueba de concepto de conexión con Entity Framework Core y Npgsql.
  - Prueba de compilación frontend y contenedorización Docker básica.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-20]: Diseño del Modelo Entidad-Relación Inicial (DER)
* **Épica:** IN-2 — Planificación del Inicio
* **Narrativa:** **Como** Arquitecto de Datos, **quiero** modelar las entidades relacionales de usuarios, alumnos, diagnósticos y actividades, **para** sustentar la persistencia relacional íntegra y segura.
* **Criterios de Aceptación:**
  - Definición de claves foráneas, borrado lógico (*soft-delete*) y cifrado de datos sensibles.
  - Diagrama DER formal documentado en markdown.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

---

## 2. Sprint 1 — Configuración del Sistema y Multi-Tenant

### HU [IN-21]: Registrar Institución Escolar
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador institucional, **quiero** registrar y configurar la sede de mi centro educativo en el sistema, **para** formalizar el espacio de trabajo institucional y que los reportes y constancias salgan con los datos oficiales.
* **Criterios de Aceptación:**
  - Formulario obligatorio: Nombre de la sede, Dirección física, Teléfono oficial y Correo de contacto.
  - Validación de unicidad de nombre de institución; ante colisión responde `409 Conflict`.
  - Persistencia transaccional en PostgreSQL con ID institucional único (`InstitutionId`).
* **Definición Técnica:** `POST /api/institutions` en `InstitutionsController.cs`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-22]: Consultar Instituciones
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** directivo escolar, **quiero** consultar los datos de la institución registrados, **para** auditar los canales de contacto y verificar que la sede se encuentre activa.
* **Criterios de Aceptación:**
  - Listado con nombre, domicilio, teléfono, email institucional y estado (`IsActive`).
  - Restricción de acceso: exclusivo para perfiles directivos y administradores globales.
* **Definición Técnica:** `GET /api/institutions` en `InstitutionsController.cs`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-23]: Editar Institución
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador institucional, **quiero** modificar los datos de contacto y domicilio de la sede, **para** mantener actualizados los canales oficiales del centro escolar.
* **Criterios de Aceptación:**
  - Modificación de domicilio, teléfonos y correo institucional.
  - El identificador único (`Id`) permanece inmutable para resguardar la integridad referencial.
* **Definición Técnica:** `PUT /api/institutions/{id}` en `InstitutionsController.cs`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-24]: Consultar Roles del Sistema
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador institucional, **quiero** listar los roles configurados en la plataforma, **para** auditar los niveles de acceso asignados a cada actor escolar.
* **Criterios de Aceptación:**
  - Retorna los cuatro roles troncales: Administrador, Profesional/Docente, Familiar y Alumno.
  - Indica si el rol es inmutable del sistema o administrable.
* **Definición Técnica:** `GET /api/roles` en `RolesController.cs`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-25]: Asignar Permisos por Módulo a Roles
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador del sistema, **quiero** configurar la matriz de permisos por módulo (Lectura, Creación, Modificación, Baja) para un rol, **para** adecuar las atribuciones a la función de cada perfil.
* **Criterios de Aceptación:**
  - Matriz con interruptores (*toggles*) independientes por módulo funcional.
  - Bloqueo defensivo: impide despojar al Administrador de permisos esenciales del sistema.
* **Definición Técnica:** `PUT /api/roles/{id}/permissions` en `RolesController.cs`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-26]: Crear Cuenta de Administrador Institucional
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador del sistema, **quiero** registrar una cuenta directiva vinculada a un centro escolar, **para** delegar la coordinación de alumnos y docentes en la sede.
* **Criterios de Aceptación:**
  - Registro con Nombre, Apellido, DNI, Email institucional y asignación de sede escolar.
  - Generación de contraseña temporal con envío por correo electrónico y forzado de cambio de clave al primer ingreso.
* **Definición Técnica:** `POST /api/users` en `AdminUsersController.cs`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-27]: Asignar Institución a Administrador
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador, **quiero** vincular a un usuario directivo con su escuela correspondiente, **para** restringir su gestión a su propio establecimiento educativo.
* **Criterios de Aceptación:**
  - Registro de la relación en tabla intermedia `AdminInstitutions`.
  - Inyección del claim `InstitutionId` en el token JWT para gobernanza de seguridad.
* **Definición Técnica:** `POST /api/admin-institutions`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-28]: Filtrado Automático por Institución
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** usuario autenticado, **quiero** que los listados de alumnos, docentes y actividades se filtren según mi institución, **para** no mezclar información con otros centros educativos.
* **Criterios de Aceptación:**
  - Filtrado automático a nivel de Entity Framework Core mediante `InstitutionId`.
  - Los administradores globales disponen de un conmutador de sedes en la barra superior.
* **Definición Técnica:** Global Query Filters en `ApplicationDbContext.cs`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-29]: Enforcement de Aislamiento de Sede (InstitutionAccessFilter)
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** arquitecto de seguridad, **quiero** que un filtro global de pipeline intercepte las peticiones y verifique la pertenencia de los recursos a la sede, **para** bloquear ataques de manipulación de IDs (IDOR).
* **Criterios de Aceptación:**
  - Intercepta solicitudes HTTP y compara el `InstitutionId` del recurso con los claims del token.
  - Responde de inmediato con `403 Forbidden` y genera un log de advertencia ante accesos cruzados.
* **Definición Técnica:** `InstitutionAccessFilter.cs` en capa API.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-30]: Confirmación con Aviso de Revocación de Sesiones al Cambiar Permisos
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador institucional, **quiero** recibir una advertencia antes de guardar cambios en los permisos de un rol, **para** ser consciente de que se revocarán las sesiones activas de dichos usuarios.
* **Criterios de Aceptación:**
  - Modal accesible con confirmación explícita sobre el impacto operativo del cambio.
* **Definición Técnica:** Diálogo modal de confirmación en cliente Angular.
* **Prioridad / Estimación:** Media · 2 SP · Estado: `Done`

### HU [IN-31]: Revocación Inmediata de Tokens JWT al Alterar Roles
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** administrador de seguridad, **quiero** que al cambiar los permisos de un rol se invaliden al instante los refresh tokens de los usuarios afectados, **para** evitar que continúen operando con privilegios obsoletos.
* **Criterios de Aceptación:**
  - El backend marca como revocados los tokens en base de datos.
  - La siguiente petición de los clientes afectados rechaza con `401 Unauthorized`.
* **Definición Técnica:** `RolesController.cs` (Líneas 183-202).
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-32]: Invalidación de Caché en Memoria de Permisos
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** desarrollador backend, **quiero** purgar la memoria caché de permisos cuando estos se actualizan, **para** que los servicios de autorización lean las definiciones recién guardadas.
* **Criterios de Aceptación:**
  - Purgado de claves en `IMemoryCache` con latencia menor a 100ms.
* **Definición Técnica:** `RolesController.cs` (Línea 181).
* **Prioridad / Estimación:** Alta · 2 SP · Estado: `Done`

### HU [IN-33]: Consulta de Catálogos Maestros del Sistema (6 Tipos)
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** docente o administrador, **quiero** consultar los 6 catálogos normalizados del sistema, **para** poblar desplegables en legajos, actividades y perfiles.
* **Criterios de Aceptación:**
  - Acceso a: 1. Tipos de Discapacidad, 2. Niveles de Autonomía, 3. Áreas de Habilidad, 4. Categorías de Actividad, 5. Tipos de Plantilla DUA, 6. Métodos de Acceso.
  - Entrega de identificadores, nombres y códigos normalizados.
* **Definición Técnica:** `GET /api/catalogs` en `CatalogsController.cs`.
* **Prioridad / Estimación:** Crítica · 3 SP · Estado: `Done`

### HU [IN-34]: Alta de Ítem en Catálogo
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** directivo escolar, **quiero** incorporar una nueva categoría en los catálogos del sistema, **para** adaptar la plataforma a particularidades de la institución.
* **Criterios de Aceptación:**
  - Validación de unicidad de nombre y código mnemotécnico.
  - Disponibilidad inmediata en la interfaz tras el registro.
* **Definición Técnica:** `POST /api/catalog-admin` en `CatalogAdminController.cs`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-35]: Edición y Baja Lógica de Ítems de Catálogo
* **Épica:** IN-3 — Configuración del Sistema
* **Narrativa:** **Como** directivo escolar, **quiero** modificar o desactivar ítems de catálogos obsoletos, **para** evitar su selección futura sin quebrar registros históricos.
* **Criterios de Aceptación:**
  - Prohibido el borrado físico si el ítem ya fue utilizado en alumnos o actividades (`Soft-delete`).
* **Definición Técnica:** `PUT /api/catalog-admin/{id}` en `CatalogAdminController.cs`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

---

## 3. Sprint 2 — Gestión de Usuarios, Legajos y Asignaciones

### HU [IN-36]: Alta de Profesional con Contraseña Temporal
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** administrador institucional, **quiero** dar de alta a un docente o terapeuta, **para** permitirle acceder al sistema y gestionar sus aulas.
* **Criterios de Aceptación:**
  - Registro con Nombre, Apellido, DNI, Email, Matrícula Profesional y Teléfono.
  - Envío automático de correo con credenciales de acceso inicial.
* **Definición Técnica:** `POST /api/professionals`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-37]: Consulta Paginada de Profesionales con Filtros
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** administrador escolar, **quiero** buscar y filtrar profesionales por nombre o especialidad, **para** supervisar el plantel docente activo.
* **Criterios de Aceptación:**
  - Paginación del lado del servidor, búsqueda predictiva y ordenamiento alfabético.
* **Definición Técnica:** `GET /api/professionals`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-38]: Edición del Legajo Profesional
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** directivo escolar, **quiero** actualizar los datos de un profesional, **para** mantener al día sus datos de contacto y especialidad terapéutica.
* **Criterios de Aceptación:**
  - Edición de especialidad y contacto; DNI inmutable para resguardar la identidad legal.
* **Definición Técnica:** `PUT /api/professionals/{id}`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-39]: Desactivación Lógica de Profesional
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** administrador escolar, **quiero** suspender el acceso de un profesional que cesó funciones, **para** resguardar los datos del colegio conservando sus registros históricos.
* **Criterios de Aceptación:**
  - `IsActive = false`, revocación de tokens JWT activos y preservación de informes elaborados.
* **Definición Técnica:** `DELETE /api/professionals/{id}`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-40]: Matricular Alumno con Perfil Funcional DUA
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** docente o administrador, **quiero** matricular a un alumno cargando sus datos personales y perfil funcional (discapacidad, autonomía y apoyos), **para** habilitar su legajo pedagógico y trayectorias.
* **Criterios de Aceptación:**
  - Carga de datos filiatorios, selección de tipo de discapacidad y grado de autonomía.
  - Creación automática de cuenta sin exigir dirección de correo electrónico al alumno.
* **Definición Técnica:** `POST /api/persons`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-41]: Consulta Paginada de Alumnos con Filtros Pedagógicos
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** docente o administrador, **quiero** listar a los estudiantes matriculados filtrando por aula o nivel de autonomía, **para** consultar rápidamente sus expedientes.
* **Criterios de Aceptación:**
  - Filtro por sala, autonomía y condición; el docente solo visualiza sus alumnos asignados.
* **Definición Técnica:** `GET /api/persons`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-42]: Actualización de Datos y Autonomía del Alumno
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** profesional a cargo, **quiero** actualizar la valoración de autonomía y observaciones pedagógicas del alumno, **para** reflejar sus progresos en el legajo.
* **Criterios de Aceptación:**
  - Registro de auditoría con fecha y docente que ejecutó la modificación.
* **Definición Técnica:** `PUT /api/persons/{id}`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-43]: Configuración del Método de Acceso (PIN o Asistido)
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** docente o directivo, **quiero** configurar si el alumno accede mediante PIN de 4 números o Login Asistido, **para** adaptar el ingreso a sus posibilidades motrices o cognitivas.
* **Criterios de Aceptación:**
  - Cuadro de confirmación que advierte el impacto en la autonomía del estudiante.
  - Validación y hash seguro (Argon2id) del PIN de 4 dígitos.
* **Definición Técnica:** `PUT /api/persons/{id}/access-method`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-44]: Baja Lógica del Alumno con Resguardo de Legajo
* **Épica:** IN-4 — Gestión de Usuarios
* **Narrativa:** **Como** directivo escolar, **quiero** dar de baja la matrícula de un alumno egresado o trasladado, **para** cerrar su ciclo lectivo preservando todas sus producciones y diagnósticos históricos.
* **Criterios de Aceptación:**
  - Baja lógica (`IsActive = false`); el historial permanece accesible para inspecciones ministeriales.
* **Definición Técnica:** `DELETE /api/persons/{id}`.
* **Prioridad / Estimación:** Media · 3 SP · Estado: `Done`

### HU [IN-45]: Registro Directo de Tutor Legal y Vinculación
* **Épica:** IN-5 — Invitaciones y Asignaciones
* **Narrativa:** **Como** docente o directivo, **quiero** registrar los datos del adulto responsable y vincularlo al alumno, **para** que reciba los comunicados escolares y boletines de avance.
* **Criterios de Aceptación:**
  - Formulario con DNI, parentesco (Madre, Padre, Tutor), correo y teléfono de contacto.
  - Emisión de credenciales con asignación del rol Familiar.
* **Definición Técnica:** `POST /api/family`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-46]: Auto-Registro de Familiares mediante Invitación por Enlace
* **Épica:** IN-5 — Invitaciones y Asignaciones
* **Narrativa:** **Como** tutor familiar, **quiero** recibir un enlace seguro en mi correo y definir mi contraseña, **para** activar mi cuenta familiar de forma personal y autónoma.
* **Criterios de Aceptación:**
  - Enlace con token criptográfico de un solo uso y vigencia de 7 días.
  - Al completarse, queda enlazado de inmediato con su hijo en la escuela.
* **Definición Técnica:** `POST /api/invitations` y `POST /api/invitations/accept`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-58]: Vincular Profesional con Institución Escolar
* **Épica:** IN-5 — Invitaciones y Asignaciones
* **Narrativa:** **Como** directivo escolar, **quiero** asociar a un profesional existente a mi sede, **para** que forme parte del claustro docente oficial de la escuela.
* **Criterios de Aceptación:**
  - Soporte de profesionales multi-institución sin colisión de identidades.
* **Definición Técnica:** `POST /api/institution-professionals`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-60]: Asignar Alumnos a Docentes y Equipos Terapéuticos
* **Épica:** IN-5 — Invitaciones y Asignaciones
* **Narrativa:** **Como** directivo escolar, **quiero** asignar uno o más alumnos a un docente o terapeuta, **para** autorizar el acceso a sus legajos y la planificación de tareas.
* **Criterios de Aceptación:**
  - Designación obligatoria de un "Profesional Principal" responsable legal del alumno.
  - El alumno se refleja al instante en el panel "Mi Aula" del docente.
* **Definición Técnica:** `POST /api/professional-persons`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

---

## 4. Sprint 3 — Autenticación, Accesibilidad y Perfiles Visuales

### HU [IN-65]: Login Estándar para Adultos (Email + Contraseña)
* **Épica:** IN-6 — Autenticación y Accesibilidad
* **Narrativa:** **Como** docente, directivo o tutor familiar, **quiero** iniciar sesión con mi correo electrónico y contraseña, **para** acceder a mis funciones de gestión escolar.
* **Criterios de Aceptación:**
  - Validación con PBKDF2 y emisión de Access Token (JWT) y Refresh Token seguro.
  - Exclusivo para roles administrativos, docentes y familiares; deniega alumnos.
* **Definición Técnica:** `POST /api/auth/login`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-67]: Login Adaptativo para Alumnos por PIN de 4 Dígitos
* **Épica:** IN-6 — Autenticación y Accesibilidad
* **Narrativa:** **Como** alumno con discapacidad, **quiero** ingresar seleccionando mi nombre/avatar e introduciendo mi PIN de 4 números en un teclado grande, **para** entrar a jugar de forma autónoma sin contraseñas difíciles.
* **Criterios de Aceptación:**
  - Teclado táctil en pantalla de alto contraste y opción de ingreso con teclado físico.
  - Validación del PIN con Argon2id y redirección inmediata a "Mi Camino".
* **Definición Técnica:** `POST /api/auth/person-login`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-68]: Login Asistido por Validación de Supervisor
* **Épica:** IN-6 — Autenticación y Accesibilidad
* **Narrativa:** **Como** alumno con dependencia motriz o cognitiva severa, **quiero** que mi docente autorice mi ingreso con su clave en mi pantalla, **para** poder realizar mis terapias sin barreras de acceso.
* **Criterios de Aceptación:**
  - El docente a cargo introduce su clave y el sistema valida su vínculo con el menor.
  - Sesión circunscrita exclusivamente a las actividades del alumno asistido.
* **Definición Técnica:** `POST /api/auth/person-login-assisted`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-75]: Panel de Accesibilidad con 7 Perfiles Visuales y Modos de Color
* **Épica:** IN-6 — Autenticación y Accesibilidad
* **Narrativa:** **Como** usuario con baja visión, dislexia o daltonismo, **quiero** activar perfiles de accesibilidad adaptados (Alto contraste, Dislexia, Deuteranopía, Protanopía, Tritanopía, etc.), **para** interactuar con la plataforma sin fatiga visual.
* **Criterios de Aceptación:**
  - 7 perfiles visuales combinables con modo Claro y Oscuro (14 estados visuales).
  - Atajos de teclado globales (`Alt + A` para panel, `Alt + S` para lectura, `Escape` para cerrar).
  - Focus Trap estricto (WCAG 2.1.2) que confina la navegación por teclado dentro del panel abierto.
* **Definición Técnica:** `AccessibilityPanelComponent` y `AccessibilityService.ts`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

---

## 5. Sprint 4 — Diagnóstico Funcional, Historial Evolutivo y Catálogos CIF

### HU [IN-81]: Carga de Diagnóstico Funcional Inicial con Cifrado AES-256
* **Épica:** IN-8 — Diagnósticos y Evaluación
* **Narrativa:** **Como** docente o terapeuta, **quiero** registrar el diagnóstico funcional inicial del alumno, **para** documentar el punto de partida pedagógico y los apoyos requeridos.
* **Criterios de Aceptación:**
  - Cifrado simétrico en reposo (AES-256-GCM) para salvaguardar datos de salud (Ley 25.326).
  - Clasificación de fortalezas, barreras contextuales y metas del Plan Pedagógico Individual (PPI).
* **Definición Técnica:** `POST /api/diagnoses`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-86]: Línea de Tiempo Histórica de Diagnósticos Evolutivos
* **Épica:** IN-8 — Diagnósticos y Evaluación
* **Narrativa:** **Como** docente o psicopedagogo, **quiero** consultar la evolución diagnóstica del alumno a lo largo de los años en una línea de tiempo cronológica, **para** evaluar la eficacia de los apoyos implementados.
* **Criterios de Aceptación:**
  - Representación visual cronológica ordenada de valoraciones e informes de avance.
  - Indicadores de fecha, profesional firmante y comparativa de autonomía alcanzada.
* **Definición Técnica:** `GET /api/diagnoses/person/{personId}/timeline`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-100]: Integración de Pictogramas ARASAAC
* **Épica:** IN-8 — Banco de Recursos
* **Narrativa:** **Como** docente, **quiero** buscar e incorporar pictogramas oficiales de ARASAAC en las actividades didácticas, **para** ofrecer consignas visuales normalizadas de Comunicación Aumentativa y Alternativa (CAA).
* **Criterios de Aceptación:**
  - Buscador integrado por palabras clave con caché local de imágenes para evitar demoras.
* **Definición Técnica:** Cliente HTTP del catálogo ARASAAC en frontend/backend.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-104]: Búsqueda Semántica de Actividades con pgvector y ONNX
* **Épica:** IN-8 — Inteligencia Artificial Local
* **Narrativa:** **Como** docente, **quiero** buscar actividades didácticas describiendo conceptos pedagógicos en lenguaje natural, **para** que el motor inteligente me sugiera ejercicios acordes a las necesidades de mi alumno.
* **Criterios de Aceptación:**
  - Ejecución de embeddings vectoriales 100% local con ONNX Runtime (sin enviar datos a nubes externas).
  - Consulta de similitud por coseno en PostgreSQL mediante la extensión `pgvector`.
* **Definición Técnica:** `GET /api/activities/semantic-search` en `ActivitiesController.cs`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

---

## 6. Sprint 5 — Reportes Pedagógicos Oficiales QuestPDF y Portal Familiar

### HU [IN-105]: Redacción de Informe Pedagógico Periódico
* **Épica:** IN-9 — Reportes Escolares
* **Narrativa:** **Como** docente, **quiero** redactar un informe de avance pedagógico consolidando métricas y observaciones cualitativas, **para** dejar constancia formal del progreso del estudiante.
* **Criterios de Aceptación:**
  - Consolidación automática de actividades resueltas, porcentajes de logro y tiempos.
  - Estado inicial `Draft` (Borrador editable exclusivamente por el autor).
* **Definición Técnica:** `POST /api/reports`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-106]: Circuito de Envío a Revisión y Aval Directivo
* **Épica:** IN-9 — Reportes Escolares
* **Narrativa:** **Como** docente y directivo escolar, **queremos** un circuito formal de aprobación (`Draft` $\rightarrow$ `Submitted` $\rightarrow$ `Approved`), **para** certificar institucionalmente la información antes de compartirla con la familia.
* **Criterios de Aceptación:**
  - El docente envía el informe (`Submitted`), quedando bloqueada su edición.
  - El directivo puede Aprobar (`Approved`) o Devolver con observaciones obligatorias (`Rejected`, Regla CB-14).
* **Definición Técnica:** `PATCH /api/reports/{id}/submit` y `PATCH /api/reports/{id}/approve`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-107]: Generación y Descarga de Informe Oficial en PDF (QuestPDF)
* **Épica:** IN-9 — Reportes Escolares
* **Narrativa:** **Como** docente, directivo o familiar, **quiero** descargar el informe en un documento PDF inmutable en formato A4 con membrete formal, **para** contar con un instrumento probatorio oficial con validez legal.
* **Criterios de Aceptación:**
  - Generación vectorial en servidor con QuestPDF respetando estructura A4 institucional.
  - Incluye membrete, firmas digitales, gráficos de desempeño y observaciones del docente.
* **Definición Técnica:** `GET /api/reports/{id}/pdf`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-108]: Consulta de Informes en el Portal Familiar y Acuse de Lectura
* **Épica:** IN-9 — Portal Familiar
* **Narrativa:** **Como** tutor familiar, **quiero** ingresar al portal familiar y visualizar únicamente los informes aprobados de mis hijos, **para** acompañar su evolución escolar desde el hogar.
* **Criterios de Aceptación:**
  - Aislamiento familiar: solo visualizan informes en estado `Approved` de sus hijos tutelados.
  - Al abrir o descargar el PDF, se estampa automáticamente la marca de lectura (`IsReadByFamily = true`).
* **Definición Técnica:** `GET /api/reports/family`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

---

## 7. Sprint 6 y 10 — Sendero Gamificado "Mi Camino", Players y Modelo de Negocio

### HU [IN-110]: Mapa Interactivo de Progresión Pedagógica "Mi Camino"
* **Épica:** IN-10 — Plan de Aprendizaje
* **Narrativa:** **Como** alumno, **quiero** ver mis actividades organizadas en un sendero en zigzag de 10 estaciones, **para** percibir mi aprendizaje como una aventura atractiva y motivadora.
* **Criterios de Aceptación:**
  - Interfaz gráfica en zigzag accesible con colores armoniosos y refuerzos visuales amables.
  - Estaciones identificadas con íconos de habilidad, estado (completado, actual, bloqueado) y medallas.
* **Definición Técnica:** `AacRoadmapComponent` en cliente Angular.
* **Prioridad / Estimación:** Crítica · 8 SP · Estado: `Done`

### HU [IN-200]: Inicialización del Roadmap Estándar (RoadmapInitializer)
* **Épica:** IN-10 — Plan de Aprendizaje
* **Narrativa:** **Como** alumno o docente, **quiero** que el sistema cuente con una trayectoria canónica de 10 niveles DUA preconfigurada, **para** disponer de actividades evaluadas desde el primer instante.
* **Criterios de Aceptación:**
  - `RoadmapInitializer.cs` siembra en base de datos 10 actividades globales con umbral del 60%.
  - Se vinculan automáticamente los niveles 1 al 10 con sus respectivos tipos de interactores.
* **Definición Técnica:** Seeders automáticos de Entity Framework Core.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-212]: Eliminación de Calendario en Perfil Persona (Regla de Negocio)
* **Épica:** IN-10 — Modelo de Negocio
* **Narrativa:** **Como** especialista en accesibilidad, **quiero** suprimir las vistas de agenda y calendario en el perfil de alumnos, **para** evitar sobrecarga cognitiva y centrar su atención en jugar y comunicarse.
* **Criterios de Aceptación:**
  - Remoción de rutas de calendario para rol Alumno; plenamente conservadas para docentes y familias.
* **Definición Técnica:** Rutas de navegación en `aac/routes.ts`.
* **Prioridad / Estimación:** Alta · 2 SP · Estado: `Done`

### HU [IN-214]: Posibilidad de Repetir Actividades Superadas sin Penalizaciones
* **Épica:** IN-10 — Gamificación
* **Narrativa:** **Como** alumno, **quiero** volver a jugar cualquier estación que ya haya aprobado, **para** afianzar mis aprendizajes y superar mi propia marca personal.
* **Criterios de Aceptación:**
  - Los niveles aprobados permanecen activos e interactivos.
  - El sistema conserva el puntaje histórico más alto (`HighestScore`) sin penalizar avances previos.
* **Definición Técnica:** Lógica de completitud en `RoadmapController.cs`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-215]: Retorno Automático al Sendero "Mi Camino" con Celebración
* **Épica:** IN-10 — Experiencia de Usuario
* **Narrativa:** **Como** alumno, **quiero** que al finalizar un juego se muestre una celebración y la pantalla vuelva a "Mi Camino", **para** ver la medalla obtenida y el siguiente nivel titilando listo para jugar.
* **Criterios de Aceptación:**
  - Animación de confeti visual no invasiva (respetando `prefers-reduced-motion`).
  - Redirección automática a `/app/roadmap` con anillo de pulso en la nueva estación desbloqueada.
* **Definición Técnica:** `PlayerBaseComponent.ts` y animaciones CSS.
* **Prioridad / Estimación:** Media · 2 SP · Estado: `Done`

### HU [IN-217]: Restricción de Rutas Clínicas para Rol Administrador
* **Épica:** IN-20 — Seguridad y Gobernanza
* **Narrativa:** **Como** administrador directivo, **quiero** no recibir alertas clínicas ni acceder a turnos terapéuticos, **para** mantener mi panel enfocado estrictamente en la gestión institucional escolar.
* **Criterios de Aceptación:**
  - Los route guards de Angular impiden el acceso a `/pro/calendar` al rol Administrador.
* **Definición Técnica:** `RoleGuard` en cliente Angular.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

---

## 8. Sprint 7 — Tableros Analíticos, Gráficos de Alto Contraste y Monitoreo

### HU [IN-116]: Tablero de Seguimiento del Aula ("Mi Aula")
* **Épica:** IN-14 — Analítica y Monitoreo
* **Narrativa:** **Como** docente, **quiero** visualizar el estado de actividad de todos mis alumnos en un único panel, **para** identificar de un vistazo quiénes avanzan a buen ritmo y quiénes necesitan ayuda.
* **Criterios de Aceptación:**
  - Tarjetas individuales de estudiantes con porcentaje de avance, última actividad y alertas activas.
* **Definición Técnica:** Componente `ClassroomDashboardComponent`.
* **Prioridad / Estimación:** Crítica · 5 SP · Estado: `Done`

### HU [IN-118]: Gráfico de Radar de Habilidades DUA
* **Épica:** IN-14 — Analítica y Monitoreo
* **Narrativa:** **Como** docente o terapeuta, **quiero** consultar un gráfico de radar que compare el desempeño del alumno en las distintas áreas pedagógicas, **para** calibrar los apoyos curriculares.
* **Criterios de Aceptación:**
  - Visualización polar de 4 ejes: Comunicación, Lógica, Cognitiva y Motricidad.
  - Paletas de colores adaptadas para alto contraste y daltonismo.
* **Definición Técnica:** Renderizado vectorial en `RadarChartComponent`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-120]: Gráficos Accesibles de Distribución y Niveles Superados
* **Épica:** IN-14 — Analítica y Monitoreo
* **Narrativa:** **Como** docente o familiar, **quiero** consultar gráficos de torta e histogramas de avance en los 10 niveles, **para** tener una perspectiva gráfica clara y comprensible del rendimiento.
* **Criterios de Aceptación:**
  - Gráfico de torta de distribución horaria por materia e histograma de 10 estaciones.
  - Cumplimiento de ratios de contraste cromático mayores a 4.5:1.
* **Definición Técnica:** Integración de componentes analíticos en portal docente y familiar.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

---

## 9. Sprint 11 — Aulas, Registro Unificado y Login Adaptado Exclusivo

### HU [IN-301]: Creación Flexible de Aulas sin Alumnos Obligatorios
* **Épica:** IN-17 — Gestión de Aulas
* **Narrativa:** **Como** directivo escolar, **quiero** crear aulas y divisiones escolares sin necesidad de cargar alumnos en ese mismo instante, **para** planificar la estructura escolar antes de comenzar la matriculación.
* **Criterios de Aceptación:**
  - Permite crear aulas vacías con Nombre, Nivel y Docente a cargo.
  - Las aulas quedan inmediatamente disponibles para asignación posterior.
* **Definición Técnica:** `POST /api/classrooms`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

### HU [IN-302]: Registro Unificado Transaccional (Alumno + Tutor + Aula)
* **Épica:** IN-17 — Matrícula Unificada
* **Narrativa:** **Como** directivo escolar o secretario, **quiero** registrar en una sola pantalla al estudiante, a su tutor familiar y matricularlo en su aula correspondiente, **para** reducir tiempos burocráticos y agilizar el alta escolar.
* **Criterios de Aceptación:**
  - Operación atómica en una única transacción: si falla el alta del tutor o alumno, se ejecuta rollback.
  - Emisión conjunta de credenciales y establecimiento del vínculo familiar formal.
* **Definición Técnica:** `POST /api/onboarding/unified-registration`.
* **Prioridad / Estimación:** Crítica · 8 SP · Estado: `Done`

### HU [IN-310]: Bloqueo Definitivo de Acceso por Email para Alumnos
* **Épica:** IN-18 — Autonomía y Accesibilidad
* **Narrativa:** **Como** arquitecto de seguridad y usabilidad, **quiero** prohibir el ingreso por correo electrónico a cuentas con rol Alumno, **para** forzar el uso exclusivo de métodos adaptados (PIN o Asistido) y evitar confusiones en el portal.
* **Criterios de Aceptación:**
  - Si una cuenta de alumno intenta loguearse en el formulario estándar de email, responde `403 Forbidden` con mensaje orientador indicando la ruta del login adaptativo.
* **Definición Técnica:** Validación en `LoginCommandHandler.cs`.
* **Prioridad / Estimación:** Alta · 3 SP · Estado: `Done`

---

## 10. Sprint 12 — Gamificación Avanzada, Casos Borde y Red Colaborativa

### HU [IN-320]: Aislamiento de Actividades Estándar en la Mesa de Trabajo Docente
* **Épica:** IN-19 — Gamificación y Limpieza Visual
* **Narrativa:** **Como** docente, **quiero** que las 10 actividades oficiales del Roadmap no se mezclen en mi grilla de trabajo personal de "Mis Actividades", **para** mantener mi banco de recursos limpio y ordenado.
* **Criterios de Aceptación:**
  - Las 10 actividades estándar se marcan como plantillas globales (`IsTemplate = true`, `ProfessionalId = null`).
  - La consulta docente filtra estrictamente `ProfessionalId == currentUserId && !IsTemplate`.
* **Definición Técnica:** `ActivitiesRepository.cs`.
* **Prioridad / Estimación:** Alta · 5 SP · Estado: `Done`

### HU [IN-321]: Experiencia Gamificada "Mi Camino" en Portal Alumno
* **Épica:** IN-19 — Gamificación
* **Narrativa:** **Como** alumno, **quiero** recorrer un sendero interactivo en zigzag con estética violeta, anillos de pulso y desbloqueo progresivo, **para** disfrutar de una experiencia lúdica y estimulante.
* **Criterios de Aceptación:**
  - El Nivel 1 está siempre disponible; los siguientes se desbloquean al obtener $\ge$ 60% de aciertos.
  - Feedback sonoro y animaciones de invitación en la estación disponible.
* **Definición Técnica:** `AacRoadmapComponent` con estilos SCSS dedicados.
* **Prioridad / Estimación:** Crítica · 8 SP · Estado: `Done`

### HU [IN-322]: Auto-Asignación Transaccional para los Reproductores Didácticos
* **Épica:** IN-19 — Ejecución Transaccional
* **Narrativa:** **Como** alumno, **quiero** que al pulsar en un nivel disponible se genere mi asignación automáticamente sin que mi docente tenga que enviármela manualmente, **para** jugar de inmediato sin bloqueos ni errores 403.
* **Criterios de Aceptación:**
  - Endpoint `POST /api/activity-assignments/auto-assign/{activityId}` genera o recupera la asignación para el alumno autenticado.
  - Reutiliza asignaciones abiertas evitando duplicación innecesaria en la base de datos.
* **Definición Técnica:** `AutoAssignActivityCommandHandler.cs`.
* **Prioridad / Estimación:** Crítica · 8 SP · Estado: `Done`

### HU [IN-335]: Gestión Pedagógica de Agotamiento de Intentos (4 Errores) con Alerta Silenciosa (CB-07)
* **Épica:** HU-21 — Motor Adaptativo & Frustración Escolar
* **Narrativa:** **Como** docente de inclusión, **quiero** que cuando un alumno acumule 4 errores consecutivos el juego se cierre amablemente sin carteles rojos de fracaso y me despache una alerta silenciosa en tiempo real, **para** acercarme presencialmente a mediar pedagógicamente sin exponer al niño frente a sus pares.
* **Criterios de Aceptación:**
  - La pantalla del alumno no muestra notas rojas ni penalizaciones; comunica: *"¡Buen intento! En un momento continuamos con tu profe"*.
  - Despacho inmediato de evento SignalR al panel del docente a cargo.
  - La campana de notificaciones del docente incrementa el badge (+1) y muestra toast informativo de alerta pedagógica.
  - Incremento automático en el contador de frustración del Dashboard escolar (`/pro/dashboard`).
* **Definición Técnica:** Handler SignalR de telemetría de intentos en backend y modal de mediación docente.
* **Prioridad / Estimación:** Alta · 8 SP · Estado: `Done`

### HU [IN-340]: Biblioteca Colaborativa Comunitaria con Prevención de Polución
* **Épica:** HU-22 — Banco de Recursos Pedagógicos
* **Narrativa:** **Como** docente o terapeuta, **quiero** explorar actividades compartidas por otros colegas y poder adaptarlas a mi aula sin alterar el original, **para** enriquecer mi práctica pedagógica colaborando con la comunidad educativa.
* **Criterios de Aceptación:**
  - Si un docente edita una actividad ajena de la comunidad, el sistema genera automáticamente una copia privada en su catálogo personal (*forking pedagógico*).
  - La actividad comunitaria original permanece intacta, evitando la proliferación de duplicados o modificaciones accidentales.
  - Filtros en la biblioteca: "Todas", "Propias", "De la Comunidad" y "Adaptadas por mí".
* **Definición Técnica:** Clonación automática en `ActivitiesController.cs`.
* **Prioridad / Estimación:** Alta · 8 SP · Estado: `Done`

---

## 11. Matriz de Trazabilidad: 19 Procesos de Negocio Escolares

Alineada con el Story Map académico del proyecto ([8-story-map-backlog-procesos-user-stories.md](file:///d:/git/login/8-story-map-backlog-procesos-user-stories.md)), esta matriz sintetiza las 31 Historias de Usuario por proceso de negocio escolar:

| Proceso | ID HU | Título y Propósito de Negocio | Rol Principal |
|:---:|:---:|---|:---:|
| **P01** | **US-01** | Apertura y parametrización de la sede escolar con membrete oficial | Directivo |
| **P01** | **US-04** | Organización escolar en salas, turnos y divisiones específicas | Directivo |
| **P02** | **US-02** | Asignación de roles y permisos RBAC con secreto profesional | Directivo |
| **P03** | **US-03** | Estandarización de áreas pedagógicas y catálogos de apoyos CIF | Directivo |
| **P04** | **US-05** | Matrícula escolar con perfil visual y PIN autónomo de 4 dígitos (V-08) | Directivo / Docente |
| **P05** | **US-06** | Habilitación institucional de docentes previa validación de matrícula | Directivo |
| **P06** | **US-07** | Registro del tutor legal con control de orfandad y consentimiento | Directivo |
| **P07** | **US-09** | Envío de invitaciones digitales con expiración automática (7 días) | Docente / Directivo |
| **P08** | **US-08** | Asignación formal de alumnos a docentes y salas escolares | Directivo |
| **P09** | **US-15** | Creación de actividades interactivas con pictogramas ARASAAC y DUA | Docente |
| **P09** | **US-18** | Búsqueda semántica de recursos pedagógicos con IA local (`pgvector`) | Docente |
| **P09b**| **US-13** | Carga de diagnóstico funcional de partida bajo cifrado AES-256-GCM | Docente / Terapeuta |
| **P09b**| **US-14** | Consulta de línea temporal histórica de diagnósticos interanuales | Docente |
| **P10a**| **US-17** | Asignación de tareas escolares del día sin alterar el sendero personal libre (CB-20) | Docente |
| **P10b**| **US-10** | Ingreso asistido de alumnos con supervisión docente en terminal presencial | Docente |
| **P10b**| **US-20** | Acceso táctil adaptado mediante PIN simplificado de 4 números | Alumno |
| **P10b**| **US-21** | Resolución de actividades didácticas con apoyos visuales y sin frustración | Alumno |
| **P10b**| **US-22** | Telemetría automática transaccional de aciertos, intentos y tiempos | Sistema |
| **P10b**| **US-23** | Plantillas interactivas avanzadas de pareo, secuencias y fonemas | Alumno |
| **P10b**| **US-24** | Alerta silenciosa de frustración ante 3 fallos consecutivos vía SignalR (CB-07) | Docente |
| **P11** | **US-16** | Supervisión del sendero de 10 hitos universales y desbloqueos manuales | Docente |
| **P11** | **US-19** | Detección automática del umbral formativo de superación ($\ge$ 60%, CB-21) | Sistema |
| **P12** | **US-28** | Confección de informes pedagógicos oficiales en formato A4 vectorial (QuestPDF) | Docente |
| **P13** | **US-26** | Calibración dinámica de exigencia mediante motor adaptativo con clamping (CB-08) | Sistema |
| **P14** | **US-25** | Panel en tiempo real "Mi Aula" para monitoreo preventivo de alumnos | Docente |
| **P14** | **US-27** | Gráficos accesibles de alto contraste (radar, torta e histograma de 10 niveles) | Docente / Familia |
| **P15** | **US-29** | Circuito de validación directiva con justificación obligatoria de rechazos (CB-14) | Directivo |
| **P16** | **US-30** | Cuaderno de comunicaciones digital bidireccional con acuse de lectura en vivo | Docente / Familia |
| **P17** | **US-12** | Mantenimiento centralizado de credenciales, reseteo de PIN y bajas lógicas | Directivo |
| **P18** | **US-11** | Recorrido guiado de onboarding adaptado por rol en el primer ingreso | Todos los roles |
| **P19** | **US-31** | Centro de ayuda interactivo con preguntas frecuentes y soporte por pictogramas | Docente / Familia |
