# InclusiON — Catálogo Completo de Historias de Usuario (IN-21 a IN-340)

> **Formato:** Plantilla Oficial InclusiON (Tabla de Campos para Contexto de IA / Confluence / Documentación de Proyecto).  
> **Ámbito:** Plataforma Integral de Inclusión Educativa y Comunicación Aumentativa Adaptada.

---

## 📑 Índice de Sprints y Módulos

1. **Sprint 1 — Configuración del Sistema (IN-21 a IN-35)**
2. **Sprint 2 — Gestión de Usuarios y Asignaciones (IN-36 a IN-64)**
3. **Sprint 3 — Autenticación y Accesibilidad (IN-65 a IN-80)**
4. **Sprint 4 — Diagnóstico, Dashboard y Catálogos (IN-81 a IN-104)**
5. **Sprint 5 — Reportes Pedagógicos y Portal Familiar (IN-105 a IN-109)**
6. **Sprint 6 — Sendero Pedagógico Gamificado "Mi Camino" (IN-110 a IN-115)**
7. **Sprint 7 — Tableros Analíticos y Distribución de Aulas (IN-116 a IN-121)**
8. **Sprint 10 — Roadmap Estándar, Players y Modelo de Negocio (IN-200 a IN-222)**
9. **Sprint 11 — Aulas, Registro Unificado y Login Adaptado (IN-301 a IN-315)**
10. **Sprint 12 — "Mi Camino" Gamificado y Auto-Asignación (IN-320 a IN-326c)**
11. **Historias de Casos Borde y Red Colaborativa (IN-335, IN-340)**

---

## Sprint 1 — Configuración del Sistema

| Campo | Contenido |
|---|---|
| ID | HU [IN-21] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Registrar institución |
| Historia | **Como** administrador institucional<br>**Quiero** registrar y configurar la sede de mi centro educativo en el sistema<br>**Para** formalizar el espacio de trabajo institucional y que los reportes y constancias salgan con los datos oficiales |
| Criterios de aceptación | - Formulario con campos obligatorios: Nombre, Dirección, Teléfono, Email institucional<br>- Validación de formato de email y unicidad del nombre institucional<br>- Registro transaccional en base de datos<br>- Notificación visual de éxito y actualización inmediata del listado |
| Observaciones | Configuración institucional de la sede escolar. En Sprint 10 se consolidó la gestión en el portal de directores locales. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `POST /api/institutions` en `InstitutionsController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-22] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Consultar instituciones |
| Historia | **Como** administrador institucional<br>**Quiero** consultar los datos registrados de la institución educativa<br>**Para** tener visibilidad de la información de contacto y sede del colegio |
| Criterios de aceptación | - Muestra la información de la institución con nombre, dirección, teléfono y email<br>- Permite verificar el estado activo de la sede<br>- Acceso exclusivo para el equipo directivo y de administración |
| Observaciones | Soporta filtrado por estado activo/inactivo. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `GET /api/institutions` en `InstitutionsController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-23] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Editar institución |
| Historia | **Como** administrador institucional<br>**Quiero** modificar los datos de contacto y domicilio de la institución existente<br>**Para** mantener actualizada la información de contacto, domicilio y canales oficiales del centro educativo |
| Criterios de aceptación | - Permite modificar nombre, dirección, teléfono y correo institucional<br>- Valida campos obligatorios y formato de contacto antes del envío<br>- Guarda los cambios de forma inmediata y muestra toast de confirmación |
| Observaciones | El código identificador único permanece inmutable para preservar la integridad referencial. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `PUT /api/institutions/{id}` en `InstitutionsController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-24] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Consultar roles |
| Historia | **Como** administrador institucional<br>**Quiero** consultar los roles y niveles de acceso configurados en el sistema<br>**Para** auditar las funciones disponibles y supervisar los permisos asignados a cada tipo de actor |
| Criterios de aceptación | - Muestra el listado de roles del sistema (Administrador Institucional, Profesional, Familiar y Alumno)<br>- Permite ver los módulos y permisos asociados a cada rol<br>- Indica si el rol es estructural del sistema o editable |
| Observaciones | Los roles base no pueden ser eliminados para preservar la seguridad del sistema. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `GET /api/roles` en `RolesController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-25] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Asignar permisos por módulo |
| Historia | **Como** administrador institucional<br>**Quiero** asignar o modificar la matriz de permisos por módulo para un rol<br>**Para** adecuar las capacidades de cada actor a las responsabilidades requeridas por la institución |
| Criterios de aceptación | - Matriz con permisos de lectura, creación, edición y eliminación agrupados por módulo funcional<br>- Interfaz con switches interactivos de asignación<br>- Valida que no se retiren permisos críticos de administración esenciales<br>- Persiste la nueva configuración de claims en base de datos |
| Observaciones | Impacta en los guards de Angular (`permission.guard.ts`) y atributos `[HasPermission]` de backend. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `PUT /api/roles/{id}/permissions` en `RolesController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-26] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Crear administrador institucional |
| Historia | **Como** administrador institucional<br>**Quiero** registrar una cuenta directiva o de supervisión escolar vinculada al centro<br>**Para** coordinar la gestión de docentes, estudiantes y configuraciones escolares en el establecimiento |
| Criterios de aceptación | - Formulario con nombre, apellido, email institucional, DNI y selección de institución<br>- Generación automática de contraseña temporal segura<br>- Envío automático de notificación por correo con credenciales de acceso inicial<br>- Forzado de cambio de contraseña en el primer login |
| Observaciones | El administrador institucional solo tendrá alcance de datos sobre la institución asignada. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | `AdminInstitutionsController` / `POST /api/users`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-27] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Asignar institución a administrador |
| Historia | **Como** administrador institucional<br>**Quiero** asociar el perfil directivo al centro educativo correspondiente<br>**Para** delimitar formalmente el ámbito de gestión de dicho usuario |
| Criterios de aceptación | - Selector institucional con búsqueda predictiva<br>- Validación de que la institución esté activa<br>- Registro de la relación en la tabla intermedia de asignaciones<br>- Actualización del claim institucional en el token JWT tras relogueo o refresh |
| Observaciones | Un administrador institucional no puede gestionar registros de instituciones no vinculadas. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `POST /api/admin-institutions`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-28] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Filtrar datos por institución |
| Historia | **Como** usuario del sistema con rol de gestión<br>**Quiero** que las consultas de personas, profesionales y actividades se filtren según la institución seleccionada o asignada<br>**Para** asegurar que cada usuario opere en el contexto escolar que le corresponde sin mezclar datos |
| Criterios de aceptación | - Inclusión del encabezado o claim `InstitutionId` en las solicitudes de API<br>- Los listados filtran automáticamente por el ID institucional del usuario autenticado<br>- Los administradores globales pueden alternar entre instituciones mediante un selector global |
| Observaciones | Garantiza la separación lógica multicentro del sistema. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Filtro global en Entity Framework Core y middleware HTTP. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-29] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Enforcement de aislamiento por institución (InstitutionAccessFilter) |
| Historia | **Como** arquitecto de seguridad / administrador del sistema<br>**Quiero** que el backend aplique de forma estricta e inmutable el aislamiento de datos por institución<br>**Para** evitar que usuarios de una institución accedan o modifiquen registros de otra escuela por manipulación de IDs |
| Criterios de aceptación | - `InstitutionAccessFilter` intercepta cada petición a nivel de pipeline de ASP.NET Core<br>- Valida que el recurso solicitado pertenezca a la institución declarada en los claims del token<br>- Retorna código HTTP `403 Forbidden` ante cualquier intento de acceso cruzado no autorizado<br>- Registra el evento en logs de auditoría de seguridad |
| Observaciones | Regla transversal de ciberseguridad y aislamiento multi-tenant. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Implementado en `InstitutionAccessFilter.cs` en la capa de Infraestructura/API. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-30] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Confirmar al guardar permisos con aviso de cierre de sesiones |
| Historia | **Como** administrador institucional<br>**Quiero** visualizar un diálogo modal de confirmación antes de aplicar modificaciones a los permisos de un rol<br>**Para** estar advertido de que el cambio cerrará las sesiones activas de los usuarios asociados y evitar cambios accidentales |
| Criterios de aceptación | - Modal emergente que detalla el rol modificado y los cambios de privilegios<br>- Mensaje explícito: *"Esta acción revocará las sesiones activas de todos los usuarios con este rol. ¿Desea continuar?"*<br>- Botones claros de "Cancelar" y "Confirmar y Aplicar"<br>- Si se cancela, no se envía la petición y se mantiene la vista sin cambios |
| Observaciones | Previene deslogueos imprevistos en horario operativo escolar. |
| Prioridad | Media |
| Estimación | 2 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Componente modal de confirmación en Angular. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-31] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Revocar tokens al cambiar permisos de un rol |
| Historia | **Como** administrador del sistema<br>**Quiero** que al guardarse un cambio de permisos en un rol se invaliden de inmediato los refresh tokens y sesiones activas de sus usuarios<br>**Para** garantizar que los permisos retirados o agregados tengan vigencia inmediata y nadie opere con privilegios caducados |
| Criterios de aceptación | - Al completarse `PUT /api/roles/{id}/permissions`, el servidor marca como revocados todos los tokens activos de los usuarios pertenecientes al rol<br>- La próxima petición de los usuarios afectados requiere renovación o reautenticación<br>- Si el token fue revocado, el backend responde `401 Unauthorized` |
| Observaciones | Líneas 183-202 de `RolesController.cs`. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Servicio de revocación de tokens JWT en backend. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-32] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Invalidar caché de permisos |
| Historia | **Como** administrador del sistema<br>**Quiero** que la caché en memoria de permisos y claims se purgue inmediatamente tras una modificación de rol<br>**Para** que los servicios de autorización no sirvan definiciones obsoletas desde la memoria caché |
| Criterios de aceptación | - La clave de caché asociada a los permisos del rol modificado es purgada de inmediato (`IMemoryCache` / `IDistributedCache`)<br>- Las consultas subsecuentes de autorización leen los datos frescos desde la base de datos<br>- Tiempo de propagación inmediato (< 100ms) |
| Observaciones | Implementado en línea 181 de `RolesController.cs`. |
| Prioridad | Alta |
| Estimación | 2 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Invalidation hook en handler de permisos. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-33] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Consultar catálogos del sistema (6 tipos) |
| Historia | **Como** profesional o administrador<br>**Quiero** consultar los ítems de los catálogos del sistema<br>**Para** disponer de las opciones de referencia normalizadas al completar formularios de personas, diagnósticos y actividades |
| Criterios de aceptación | - Acceso de lectura a los 6 catálogos fundamentales: 1. Tipos de Discapacidad, 2. Niveles de Autonomía, 3. Áreas de Habilidad, 4. Categorías de Actividad, 5. Tipos de Template, 6. Métodos de Login<br>- Respuesta en formato JSON con ID, código, nombre y estado activo<br>- Soporte de filtrado por estado activo |
| Observaciones | Alimenta dinámicamente los dropdowns y selectores de toda la aplicación Angular. |
| Prioridad | Crítica |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `GET /api/catalogs` en `CatalogsController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-34] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Registrar ítem en catálogo |
| Historia | **Como** administrador institucional<br>**Quiero** agregar un nuevo valor a un catálogo del sistema<br>**Para** ampliar las opciones de clasificación pedagógica, funcional o técnica según las necesidades de la institución |
| Criterios de aceptación | - Formulario para seleccionar el catálogo de destino e ingresar nombre, código nemotécnico y descripción<br>- Validación de unicidad: no permite nombres ni códigos duplicados en el mismo catálogo<br>- Solo accesible por el rol Administrador Institucional<br>- El nuevo ítem queda inmediatamente disponible en los selectores del sistema |
| Observaciones | Previene la dispersión de categorías ingresadas manualmente en texto libre. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `POST /api/catalog-admin` en `CatalogAdminController`. |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-35] |
| Épica | IN-3 — Configuración del Sistema |
| Título | Editar ítem en catálogo |
| Historia | **Como** administrador institucional<br>**Quiero** editar el nombre o descripción de un ítem existente de un catálogo o desactivarlo<br>**Para** corregir errores tipográficos, actualizar nomenclaturas pedagógicas o retirar opciones en desuso |
| Criterios de aceptación | - Permite modificar nombre y descripción<br>- Permite alternar estado Activo / Inactivo (baja lógica sin romper referencias históricas)<br>- Validación de unicidad de nombre en caso de renombrado<br>- Notificación visual de éxito tras guardar |
| Observaciones | No se permite el borrado físico de ítems de catálogo si ya están asignados a personas o actividades. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 1 — Config de sistema |
| Estado | En revisión |
| Definiciones | Endpoint `PUT /api/catalog-admin/{id}` en `CatalogAdminController`. |

---

## Sprint 2 — Gestión de Usuarios y Asignaciones

| Campo | Contenido |
|---|---|
| ID | HU [IN-36] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Alta de profesional con contraseña temporal y envío de email |
| Historia | **Como** administrador institucional<br>**Quiero** registrar a un nuevo profesional docente o terapeuta<br>**Para** habilitarle el acceso a la plataforma y permitirle gestionar a sus alumnos asignados |
| Criterios de aceptación | - Formulario con Nombre, Apellido, DNI, Email, Matrícula/Especialidad y Teléfono<br>- Validación de unicidad de DNI y Email<br>- Generación automática de contraseña temporal segura<br>- Envío automático de correo electrónico con datos de acceso |
| Observaciones | Debe forzar el cambio de contraseña en el primer inicio de sesión. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/professionals` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-37] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Consulta paginada de profesionales con filtros |
| Historia | **Como** administrador institucional<br>**Quiero** listar a los profesionales de mi centro con filtros de búsqueda y paginación<br>**Para** localizar rápidamente al personal activo o inactivo |
| Criterios de aceptación | - Listado paginado con tamaño configurable de página<br>- Filtro por nombre, especialidad y estado (Activo/Inactivo)<br>- Ordenamiento por apellido y nombre ascendente/descendente |
| Observaciones | Restringido por aislamiento multi-tenant a la institución del administrador. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `GET /api/professionals` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-38] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Edición de profesional |
| Historia | **Como** administrador institucional<br>**Quiero** editar los datos de un profesional registrado<br>**Para** mantener actualizados sus datos de contacto y especialidad |
| Criterios de aceptación | - Permite editar teléfono, especialidad y datos personales<br>- DNI y Email inmutables o con validación estricta de colisión<br>- Confirmación visual al guardar cambios |
| Observaciones | El profesional también puede editar ciertos datos desde su propio perfil. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `PUT /api/professionals/{id}` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-39] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Desactivación de profesional |
| Historia | **Como** administrador institucional<br>**Quiero** dar de baja lógica a un profesional<br>**Para** revocar su acceso sin perder el historial de actividades y diagnósticos registrados |
| Criterios de aceptación | - Soft delete (`IsActive = false`)<br>- Cierre inmediato de sesiones y revocación de tokens JWT activos<br>- Preservación íntegra de registros históricos de alumnos y reportes |
| Observaciones | Previene la pérdida accidental de datos clínicos o pedagógicos. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `DELETE /api/professionals/{id}` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-40] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Alta de persona con perfil funcional |
| Historia | **Como** profesional o administrador institucional<br>**Quiero** dar de alta a una persona con discapacidad registrando sus datos personales y perfil funcional inicial<br>**Para** habilitar su legajo pedagógico y permitirle interactuar en la plataforma |
| Criterios de aceptación | - Datos filiatorios: Nombre, Apellido, Fecha de Nacimiento, DNI<br>- Perfil funcional: Tipo de discapacidad, Nivel de autonomía y Método de login preferido<br>- Asignación opcional a un aula o profesional a cargo<br>- Validación de unicidad de DNI |
| Observaciones | Cumple con requerimientos de inclusión y accesibilidad desde el alta. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/persons` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-41] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Consulta paginada de personas con filtros |
| Historia | **Como** profesional o administrador<br>**Quiero** consultar la lista de alumnos con filtros avanzados<br>**Para** gestionar los legajos de las personas asignadas a mi aula o centro |
| Criterios de aceptación | - Paginación en servidor con debounce en búsqueda<br>- Filtros por nombre, tipo de discapacidad, nivel de autonomía y aula<br>- Acceso restringido según rol (el docente solo ve sus alumnos asignados) |
| Observaciones | Aplica Row-Level Security en backend. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `GET /api/persons` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-42] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Edición de datos personales y funcionales de persona |
| Historia | **Como** profesional a cargo<br>**Quiero** actualizar los datos personales y el perfil funcional de un alumno<br>**Para** reflejar avances en su nivel de autonomía o cambios en su información de contacto |
| Criterios de aceptación | - Permite modificar datos personales, nivel de autonomía y observaciones pedagógicas<br>- Registro de fecha y usuario que realizó la modificación<br>- Validación de consistencia de datos |
| Observaciones | Dispone de edición inline y modal completo de edición. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `PUT /api/persons/{id}` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-43] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Configuración del método de login con confirm popup |
| Historia | **Como** profesional o administrador<br>**Quiero** configurar el método de acceso de una persona con discapacidad (PIN o Asistido)<br>**Para** adaptar la autenticación a las capacidades cognitivas y motoras del estudiante |
| Criterios de aceptación | - Selector de método: PIN numérico de 4 dígitos o Login Asistido (con supervisor)<br>- Popup de confirmación al cambiar de método explicando el impacto en el alumno<br>- Si se elige PIN, solicita y valida el PIN de 4 dígitos (encriptado con Argon2id) |
| Observaciones | En Sprint 11 (IN-310) se bloqueó definitivamente el acceso por Email para alumnos. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `PUT /api/persons/{id}/login-config` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-44] |
| Épica | IN-4 — Gestión de Usuarios |
| Título | Desactivación de persona (soft-delete + revocación de tokens) |
| Historia | **Como** administrador institucional<br>**Quiero** desactivar la cuenta de un alumno que ya no asiste a la institución<br>**Para** suspender su acceso conservando todas sus métricas y trayectorias históricas |
| Criterios de aceptación | - Baja lógica (`IsActive = false`)<br>- Revocación inmediata de refresh tokens y cierre de sesiones activas<br>- Desvinculación de asignaciones pendientes de actividades |
| Observaciones | Los reportes e historial clínico permanecen disponibles para auditoría. |
| Prioridad | Media |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `DELETE /api/persons/{id}` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-45] |
| Épica | IN-5 — Invitaciones y Asignaciones |
| Título | Alta directa de familiar con selector de persona |
| Historia | **Como** profesional o administrador<br>**Quiero** registrar directamente a un tutor familiar vinculándolo a una persona con discapacidad<br>**Para** que la familia pueda dar seguimiento al progreso pedagógico desde el portal familiar |
| Criterios de aceptación | - Formulario con Nombre, Apellido, DNI, Email, Teléfono, Parentesco y Selector de Alumno<br>- Creación de cuenta de usuario con rol Familiar<br>- Generación de contraseña temporal enviada por email |
| Observaciones | Permite vincular más de un alumno al mismo familiar. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/family-members` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-46] |
| Épica | IN-5 — Invitaciones y Asignaciones |
| Título | Alta de familiar por invitación (auto-registro) |
| Historia | **Como** tutor familiar<br>**Quiero** recibir un enlace de invitación por email y completar mi propio registro<br>**Para** activar mi cuenta familiar de forma autónoma sin que la escuela gestione mi contraseña |
| Criterios de aceptación | - El email contiene un token único y seguro de invitación con fecha de caducidad<br>- Al hacer clic, abre formulario de bienvenida y definición de contraseña personal<br>- Valida que el token no haya expirado ni haya sido utilizado previamente |
| Observaciones | Mejora la privacidad y facilita el onboarding de las familias. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/invitations/accept` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-58] |
| Épica | IN-5 — Invitaciones y Asignaciones |
| Título | Asignar profesional a institución |
| Historia | **Como** administrador institucional<br>**Quiero** vincular a un profesional existente a mi institución escolar<br>**Para** incorporarlo al cuerpo docente de la escuela |
| Criterios de aceptación | - Selección de profesional mediante autocompletado<br>- Validación de que no esté previamente vinculado a la misma escuela<br>- Registro en tabla relacional con fecha de alta y rol docente |
| Observaciones | Un profesional puede trabajar en múltiples instituciones educativas. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/institution-professionals` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-60] |
| Épica | IN-5 — Invitaciones y Asignaciones |
| Título | Asignar persona a profesional |
| Historia | **Como** administrador institucional o docente<br>**Quiero** asignar un alumno a un profesional específico<br>**Para** que el profesional pueda diseñar actividades personalizadas, evaluar diagnósticos y monitorear su avance |
| Criterios de aceptación | - Selección de Alumno y Profesional<br>- Flag booleano de "Profesional Principal"<br>- Permiso explícito para autorizar Login Asistido<br>- Visualización inmediata del alumno en "Mi Aula" del docente |
| Observaciones | Una persona puede tener un equipo interdisciplinario (ej. docente + fonoaudióloga). |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 2 — Gestión de Usuarios |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/professional-persons` |

---

## Sprint 3 — Autenticación y Accesibilidad

| Campo | Contenido |
|---|---|
| ID | HU [IN-65] |
| Épica | IN-6 — Autenticación y Accesibilidad |
| Título | Login estándar (email + contraseña) |
| Historia | **Como** profesional, administrador o familiar<br>**Quiero** iniciar sesión con mi correo electrónico y contraseña<br>**Para** acceder de manera segura a las funciones correspondientes a mi rol |
| Criterios de aceptación | - Formulario de login responsivo y accesible<br>- Validación de credenciales contra ASP.NET Identity con hash PBKDF2<br>- Generación de par Access Token (JWT) y Refresh Token con claims de rol e institución<br>- Manejo de errores con mensajes claros y protección ante fuerza bruta |
| Observaciones | Restringido exclusivamente a roles Administrativos, Docentes y Tutores. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 3 — Autenticación y Accesibilidad |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/auth/login` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-67] |
| Épica | IN-6 — Autenticación y Accesibilidad |
| Título | Login por PIN (4 dígitos) |
| Historia | **Como** persona con discapacidad (alumno)<br>**Quiero** ingresar al portal seleccionando mi nombre/foto e introduciendo mi PIN de 4 dígitos<br>**Para** acceder a mis actividades de manera autónoma y simplificada sin requerir correo ni contraseñas alfanuméricas complejas |
| Criterios de aceptación | - Teclado numérico en pantalla adaptado con botones grandes y alto contraste<br>- Validación de PIN de 4 dígitos verificado con Argon2id<br>- Redirección directa al portal del alumno (`/app/roadmap` o `/app/home`)<br>- Límite de intentos con bloqueo preventivo y feedback sonoro/visual |
| Observaciones | Método prioritario de autonomía para personas con discapacidad motriz o cognitiva. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 3 — Autenticación y Accesibilidad |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/auth/login-pin` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-68] |
| Épica | IN-6 — Autenticación y Accesibilidad |
| Título | Login asistido (supervisor autoriza) |
| Historia | **Como** alumno con alta dependencia funcional<br>**Quiero** ingresar con el apoyo de mi docente o terapeuta a cargo mediante su autorización directa<br>**Para** acceder a mis actividades terapéuticas sin barreras cognitivas ni memorización de códigos |
| Criterios de aceptación | - Selección de alumno en pantalla de login asistido<br>- El docente introduce sus credenciales o código de supervisor rápido en la misma terminal<br>- El sistema valida que el supervisor sea el "Profesional Principal" vinculado al alumno<br>- Se genera una sesión temporal con alcance exclusivo del perfil del alumno |
| Observaciones | Pensado para contextos de aula especial y terapias presenciales. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 3 — Autenticación y Accesibilidad |
| Estado | Implementado / En revisión |
| Definiciones | Endpoint `POST /api/auth/assisted-login` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-75] |
| Épica | IN-6 — Autenticación y Accesibilidad |
| Título | 7 perfiles visuales de accesibilidad |
| Historia | **Como** usuario con requerimientos específicos de visión o lectura<br>**Quiero** aplicar perfiles de accesibilidad predefinidos (Alto contraste, Dislexia, Baja visión, Daltonismo: Deuteranopía, Protanopía, Tritanopía)<br>**Para** percibir y operar la interfaz sin fatiga visual ni barreras de contraste |
| Criterios de aceptación | - 7 perfiles implementados mediante tokens CSS y tipografías adaptadas (ej. OpenDyslexic)<br>- Modos claro y oscuro combinables (14 combinaciones visuales totales)<br>- Atajo de teclado Alt+A para apertura instantánea del menú de accesibilidad<br>- Persistencia de la preferencia en el perfil de usuario y `localStorage` |
| Observaciones | Cumple con pautas de diseño universal WCAG 2.1 AA/AAA. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 3 — Autenticación y Accesibilidad |
| Estado | Implementado / En revisión |
| Definiciones | `ThemeService.ts` / CSS Variables |

---

## Sprint 10 — Roadmap Estándar, Players y Modelo de Negocio

| Campo | Contenido |
|---|---|
| ID | HU [IN-200] |
| Épica | IN-10 — Plan de Trabajo (Roadmap) |
| Título | Implementar RoadmapInitializer con 10 actividades estándar |
| Historia | **Como** alumno o docente<br>**Quiero** que el sistema cuente de base con una trayectoria pedagógica oficial de 10 niveles listos para jugar<br>**Para** disponer de un camino educativo probado sin necesidad de configurar actividades desde cero |
| Criterios de aceptación | - `RoadmapInitializer.cs` siembra 10 actividades globales con metadatos completos y `RoadmapOrder` de 1 a 10<br>- Cada actividad incluye objetivos, áreas de habilidad y umbral de aprobación predeterminado (60%)<br>- Las 10 actividades se vinculan con sus respectivos tipos de player |
| Observaciones | Corazón pedagógico del modo "Mi Camino" en el portal de alumnos. |
| Prioridad | Crítica |
| Estimación | 5 SP |
| Sprint asignado | Sprint 10 — Roadmap Estándar |
| Estado | Hecho (Done) |
| Definiciones | Seeders de Entity Framework Core |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-212] |
| Épica | IN-10 / Modelo de Negocio |
| Título | Eliminar Calendario del perfil Persona (modelo de negocio) |
| Historia | **Como** diseñador pedagógico / administrador<br>**Quiero** remover la navegación y vistas de calendario en el perfil de los alumnos con discapacidad<br>**Para** simplificar su interfaz y concentrar su atención en la comunicación aumentativa y sus actividades didácticas |
| Criterios de aceptación | - Se remueven rutas de calendario de `aac/routes.ts`<br>- Se eliminan enlaces de navegación en `aac-nav.component.ts`<br>- El calendario continúa estando plenamente disponible para los perfiles Profesional y Familiar |
| Observaciones | Regla de negocio: la agenda de turnos es coordinada entre profesionales y familias. |
| Prioridad | Alta |
| Estimación | 2 SP |
| Sprint asignado | Sprint 10 — Modelo de Negocio |
| Estado | Hecho (Done) |
| Definiciones | Angular Client Routing |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-214] |
| Épica | IN-10 / Gamificación |
| Título | Permitir repetir actividades completadas desde "Mi Camino" y backend |
| Historia | **Como** alumno<br>**Quiero** volver a jugar cualquier nivel del Roadmap que ya haya aprobado anteriormente<br>**Para** afianzar mis conocimientos, superar mi propio puntaje y disfrutar nuevamente del desafío |
| Criterios de aceptación | - Los nodos con estado "Aprobado" (checkmark verde) se mantienen interactivos y cliqueables<br>- Al ingresar, el player permite iniciar una nueva sesión de juego sin bloquear al alumno<br>- El backend actualiza la mejor marca (`HighestScore`) sin penalizar el progreso ya adquirido |
| Observaciones | Fomenta el aprendizaje por refuerzo positivo y sin frustración. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 10 — Gamificación |
| Estado | Hecho (Done) |
| Definiciones | `AacRoadmapComponent` & `PlayerBaseComponent` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-215] |
| Épica | IN-10 / Experiencia de Usuario |
| Título | Redireccionar a la pantalla de "Mi Camino" (/app/roadmap) al finalizar |
| Historia | **Como** alumno<br>**Quiero** que al completar una actividad del Roadmap la pantalla me devuelva directamente al mapa de "Mi Camino"<br>**Para** ver reflejada de inmediato mi medalla dorada y el desbloqueo del siguiente nivel |
| Criterios de aceptación | - Al concluir la animación de celebración en el modal de resultados, el botón "Continuar" redirige a `/app/roadmap`<br>- El componente del Roadmap recarga reactivamente el estado mostrando la animación de pulso en el nuevo nivel desbloqueado |
| Observaciones | Evita desorientar al alumno enviándolo al inicio general. |
| Prioridad | Media |
| Estimación | 2 SP |
| Sprint asignado | Sprint 10 — Experiencia de Usuario |
| Estado | Hecho (Done) |
| Definiciones | `PlayerBaseComponent.ts` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-217] |
| Épica | IN-20 — Calendario y Seguridad |
| Título | Bloqueo de notificaciones y rutas de calendario para rol Administrador |
| Historia | **Como** administrador institucional<br>**Quiero** no recibir avisos de turnos clínicos de profesionales ni acceder a rutas de calendario terapéutico<br>**Para** mantener mi panel enfocado exclusivamente en la gestión directiva de la escuela |
| Criterios de aceptación | - Los guards de Angular interceptan accesos a `/pro/calendar` por parte de usuarios con rol Admin y redirigen al dashboard administrativo con toast informativo<br>- El despachador de notificaciones de calendario excluye destinatarios con rol Administrador |
| Observaciones | Cumple con la estricta separación de incumbencias operativas y directivas. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 10 — Calendario y Seguridad |
| Estado | Hecho (Done) |
| Definiciones | Guards de Angular y EventHandlers |

---

## Sprint 11 — Aulas, Registro Unificado y Login Adaptado

| Campo | Contenido |
|---|---|
| ID | HU [IN-301] |
| Épica | IN-17 — Gestión de Aulas |
| Título | Creación de aulas vacías sin alumnos obligatorios |
| Historia | **Como** administrador institucional o docente<br>**Quiero** crear un aula o grupo escolar sin estar obligado a matricular alumnos en el mismo momento<br>**Para** organizar la estructura pedagógica de la escuela antes del inicio del ciclo lectivo |
| Criterios de aceptación | - Formulario de creación de aula con Nombre, Nivel Educativo y Docente a cargo<br>- La lista de alumnos es opcional durante la creación<br>- El aula se guarda correctamente y aparece disponible en selectores de matriculación posterior |
| Observaciones | Resuelve la restricción previa que exigía alumnos cargados de antemano. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 11 — Aulas y Registro |
| Estado | Hecho (Done) |
| Definiciones | Endpoint `POST /api/classrooms` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-302] |
| Épica | IN-17 — Registro Unificado |
| Título | Endpoint transaccional de registro unificado Alumno + Tutor + Aula |
| Historia | **Como** docente o directivo escolar<br>**Quiero** registrar en una única operación al Alumno, a su Tutor Familiar y matricularlo en un Aula<br>**Para** evitar tener que ir a tres pantallas distintas para dar de alta a un estudiante |
| Criterios de aceptación | - Endpoint atómico que recibe datos de la persona con discapacidad, datos de contacto del familiar y el aula asignada<br>- Manejo transaccional: si cualquier entidad falla, se hace rollback completo<br>- Emisión simultánea de credenciales y vinculaciones de parentesco |
| Observaciones | Reduce el tiempo de carga escolar en más de un 70%. |
| Prioridad | Crítica |
| Estimación | 8 SP |
| Sprint asignado | Sprint 11 — Aulas y Registro |
| Estado | Hecho (Done) |
| Definiciones | Endpoint `POST /api/onboarding/unified-registration` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-310] |
| Épica | IN-18 — Autonomía de Acceso |
| Título | Restricción de inicio de sesión por email para rol PersonWithDisability |
| Historia | **Como** responsable de seguridad y accesibilidad<br>**Quiero** deshabilitar el acceso por Email/Contraseña alfanumérica para el rol de Persona con Discapacidad<br>**Para** asegurar que solo ingresen mediante métodos adaptados (PIN o Asistido) y evitar confusiones en el login general |
| Criterios de aceptación | - Si un usuario con rol `PersonWithDisability` intenta autenticarse en el login estándar por email, el backend rechaza la solicitud con código `403 Forbidden` y mensaje explicativo<br>- La UI del login público redirige a los alumnos directamente a su portal adaptado |
| Observaciones | Reemplaza y depreca definitivamente la historia IN-66. |
| Prioridad | Alta |
| Estimación | 3 SP |
| Sprint asignado | Sprint 11 — Autonomía de Acceso |
| Estado | Hecho (Done) |
| Definiciones | Modificación en `LoginCommandHandler.cs` |

---

## Sprint 12 — "Mi Camino" Gamificado y Auto-Asignación

| Campo | Contenido |
|---|---|
| ID | HU [IN-320] |
| Épica | IN-19 — "Mi Camino" Gamificado |
| Título | Aislamiento de actividades del Roadmap para profesionales |
| Historia | **Como** profesional de inclusión<br>**Quiero** que las 10 actividades oficiales del Roadmap no contaminen mi grilla de trabajo "Mis Actividades"<br>**Para** trabajar con mis propios recursos pedagógicos sin mezclarlos con las plantillas globales del sistema |
| Criterios de aceptación | - Las 10 actividades estándar del Roadmap se marcan con `IsTemplate = true` y `ProfessionalId = null`<br>- El repositorio filtra estrictamente `ProfessionalId == currentUserId && !IsTemplate`<br>- Se retiran botones innecesarios de gestión de plantillas de la cabecera docente |
| Observaciones | Mantiene limpia la mesa de trabajo del docente. |
| Prioridad | Alta |
| Estimación | 5 SP |
| Sprint asignado | Sprint 12 — Gamificación y Auto-Asignación |
| Estado | Hecho (Done) |
| Definiciones | `ActivitiesRepository.cs` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-321] |
| Épica | IN-19 — "Mi Camino" Gamificado |
| Título | Experiencia gamificada "Mi Camino" en Portal Alumno |
| Historia | **Como** alumno (persona con discapacidad)<br>**Quiero** recorrer un mapa interactivo de 10 niveles en zigzag con estética violeta, anillos de pulso y desbloqueo progresivo<br>**Para** sentirme motivado por mi progreso y disfrutar aprendiendo como en un juego |
| Criterios de aceptación | - Interfaz en zigzag en `/app/roadmap` con estética visual violeta (`#673AB7` / `#5C6BC0`)<br>- El Nivel 1 está siempre desbloqueado; los niveles siguientes se desbloquean al obtener 60% o más de aciertos en el nivel previo<br>- Animaciones de pulso (`.pulse-ring`) en el nivel disponible para invitar a la acción |
| Observaciones | Diseño pedagógico de baja frustración y refuerzo positivo. |
| Prioridad | Crítica |
| Estimación | 8 SP |
| Sprint asignado | Sprint 12 — Gamificación y Auto-Asignación |
| Estado | Hecho (Done) |
| Definiciones | `AacRoadmapComponent` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-322] |
| Épica | IN-19 — "Mi Camino" Gamificado |
| Título | Flujo de Auto-Asignación Transaccional para el Reproductor |
| Historia | **Como** alumno<br>**Quiero** que al hacer clic en un nivel disponible se genere automáticamente mi asignación personal de juego<br>**Para** jugar de forma transparente y sin encontrar errores 403 ni pantallas bloqueadas |
| Criterios de aceptación | - Endpoint `POST /api/activity-assignments/auto-assign/{activityId}` genera o recupera la asignación para el alumno autenticado<br>- Si ya existía asignación previa no cancelada, la reutiliza evitando duplicaciones en la base de datos<br>- El reproductor interactivo recibe un ID de asignación cifrado válido y carga con código `200 OK`<br>- Se persisten respuestas y porcentajes en `localStorage` y en la base de datos |
| Observaciones | Garantiza cero errores en la experiencia del estudiante. |
| Prioridad | Crítica |
| Estimación | 8 SP |
| Sprint asignado | Sprint 12 — Gamificación y Auto-Asignación |
| Estado | Hecho (Done) |
| Definiciones | `AutoAssignActivityCommandHandler.cs` |

---

## Historias de Casos Borde y Red Colaborativa (Últimas incorporaciones)

| Campo | Contenido |
|---|---|
| ID | HU [IN-335] |
| Épica | HU-21 — Motor Adaptativo (MDA) & Monitoreo Docente |
| Título | CB-07: Gestión de Agotamiento de Intentos (4 errores) con Aviso Pedagógico, Notificación en Tiempo Real e Impacto en Dashboard |
| Historia | **Como** docente / profesional a cargo del seguimiento del alumno<br>**Quiero** que cuando un estudiante agote sus intentos permitidos (4 errores consecutivos) el sistema active un aviso pedagógico en lugar de penalizarlo con notas rojas o bloquear su camino<br>**Para** recibir una alerta en tiempo real en mi campana de notificaciones, visualizar el impacto en el contador de frustración del Dashboard y poder intervenir presencialmente para apoyarlo |
| Criterios de aceptación | - El alumno no recibe mensajes punitivos; el reproductor finaliza amigablemente indicando que el profesor se acercará a ayudarlo<br>- Se despacha un evento en tiempo real vía SignalR al profesional a cargo<br>- La campana de notificaciones incrementa el badge (+1) y muestra toast informativo de alerta pedagógica<br>- La métrica de "Alertas de frustración" en el Dashboard docente (`/pro/dashboard`) incrementa en +1 con atajo directo de intervención |
| Observaciones | Implementa la regla pedagógica anti-punitiva de InclusiON para el manejo de la frustración. |
| Prioridad | Alta |
| Estimación | 8 SP |
| Sprint asignado | Sprint 12 / Backlog Prioritario |
| Estado | Ready for Sprint / Backlog |
| Definiciones | Documentado en detalle en `InclusiON.Documents/Docs/HU/HU-21-cb07-alerta-pedagogica-intentos-agotados.md` |

---

| Campo | Contenido |
|---|---|
| ID | HU [IN-340] |
| Épica | HU-22 — Banco de Recursos Pedagógicos & Red Colaborativa |
| Título | Biblioteca Colaborativa de Actividades Multi-Especialidad: Adaptación Curricular sin Polución Comunitaria (Anti-Duplicación) y Filtros Propias / Compartidas / Editadas |
| Historia | **Como** profesional de inclusión (docente, psicopedagogo o terapeuta)<br>**Quiero** disponer de una biblioteca colaborativa donde se compartan las actividades creadas por cualquier profesional y poder adaptarlas a las necesidades de mis alumnos<br>**Para** enriquecer mi práctica pedagógica colaborando con la comunidad, asegurando que si adapto una actividad ajena quede guardada como copia personal (evitando duplicar la misma actividad en la biblioteca compartida) y pudiendo filtrar entre Propias, Compartidas y Editadas |
| Criterios de aceptación | - Galería comunitaria donde cualquier profesional visualiza y asigna actividades creadas por otros colegas<br>- Al editar una actividad creada por otro colega, el sistema clona automáticamente la actividad como copia privada del docente que edita, sin alterar la original de la comunidad<br>- Filtros segmentados en interfaz: "Todas", "Creadas por mí (Propias)", "De la Comunidad (Compartidas)" y "Adaptadas por mí (Editadas)" |
| Observaciones | Resuelve el problema clásico de saturación y polución en repositorios compartidos de recursos educativos. |
| Prioridad | Alta |
| Estimación | 8 SP |
| Sprint asignado | Sprint 12 / Backlog Prioritario |
| Estado | Ready for Sprint / Backlog |
| Definiciones | Documentado en detalle en `InclusiON.Documents/Docs/HU/HU-22-biblioteca-colaborativa-actividades.md` |
