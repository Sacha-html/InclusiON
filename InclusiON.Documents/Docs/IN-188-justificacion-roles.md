# IN-188 — Justificación de Roles del Sistema respecto a los Actores de Negocio

## Contexto

El análisis del dominio escolar y terapéutico identificó **4 actores canónicos de negocio** que interactúan operativamente en InclusiON:
1. **Persona con Discapacidad (Estudiante / Alumno)**
2. **Profesional (Docente / Terapeuta / Gabinete)**
3. **Representante Familiar (Padres / Tutores Legales)**
4. **Administrador de la Institución (Director / Equipo Directivo)**

Este documento justifica cómo y por qué cada actor de negocio se traduce en un rol técnico del sistema, garantizando el principio de mínimo privilegio, la protección de datos médicos sensibles (Ley 25.326) y la gobernanza institucional.

> **Nota sobre la exclusión de "Admin Global":**  
> En el modelo de negocio de InclusiON, **no existe la figura operativa de un Admin Global**. La administración de servidores, bases de datos e infraestructura corresponde al soporte técnico / DevOps del proveedor del servicio, no a un usuario del sistema escolar. Cada institución educativa funciona como un tenant autónomo bajo la autoridad exclusiva de su Equipo Directivo. Otorgar un rol transversal con acceso a los legajos clínicos de alumnos de diferentes escuelas violaría de forma flagrante la confidencialidad médica y el secreto profesional.

---

## Mapeo actor → rol

| Actor de Negocio | Rol técnico | Claim JWT / Contexto de Seguridad |
|---|---|---|
| **Persona con Discapacidad (Alumno)** | `persona` | `role: persona` |
| **Profesional (Docente / Terapeuta)** | `profesional` | `role: profesional` |
| **Representante Familiar (Padres / Tutores)** | `familia` | `role: familia` |
| **Administrador Institucional (Equipo Directivo)** | `admin` | `role: admin`, `institutionId: <id>`, `institutionalRoleId: <id>` |
| **Institución Educativa** | — (sin rol) | — (entidad organizativa de alcance) |

---

## Justificación por rol

### Rol `persona`
**Actor:** Persona con Discapacidad (Estudiante / Alumno)

La persona es el destinatario central del aprendizaje. Requiere una interfaz radicalmente distinta a la de los adultos: portal adaptativo con Comunicación Aumentativa y Alternativa (CAA), pictogramas oficiales de ARASAAC, navegación simplificada por teclado/pulsador, síntesis de voz y estímulos audiovisuales anti-frustración.

Separar este rol técnico garantiza que su entorno de ejecución, accesibilidad y métodos de autenticación sean completamente independientes de los paneles de gestión y expedientes clínicos.

**Decisiones técnicas que dependen de este rol:**
- **Métodos de login accesibles:** PIN numérico visual de 4 dígitos, acceso Asistido (supervisado por docente en el aula) y acceso Familiar, además del estándar por correo/clave.
- **Vistas exclusivas:** Portal del Alumno, recorrido del Roadmap ("Mi Camino"), reproductores de actividades interactivas adaptativas.
- **Aislamiento absoluto:** Cero acceso a datos de otros alumnos, notas clínicas, informes o herramientas administrativas.

---

### Rol `profesional`
**Actor:** Profesional (Docente de Educación Especial, Fonoaudiólogo/a, Psicopedagogo/a, Terapeuta Ocupacional)

Es el agente pedagógico y clínico directo. Diseña actividades interactivas, supervisa la trayectoria curricular y elabora diagnósticos e informes. Requiere acceso a información de salud sensible y datos evolutivos.

Separar este rol permite aplicar un estricto **control de acceso basado en recursos (`[PersonAccess]`)**: un docente no puede ver a todos los alumnos de la escuela de forma masiva, sino única y exclusivamente a aquellos que tiene formalmente asignados a su cargo.

**Decisiones técnicas que dependen de este rol:**
- **Aislamiento por asignación:** Filtro forzoso en backend por relación activa `Professional ↔ Person`. Cualquier intento de consultar un alumno no asignado es rechazado con `403 Forbidden`.
- **Vistas exclusivas:** Panel "Mi Aula", legajo pedagógico del alumno, editor dinámico de actividades DUA, configuración y calibración del Motor Adaptativo (MDA), registro de diagnósticos funcionales cifrados.
- **Elaboración de informes:** Redacción de informes de progreso en estado borrador (`Draft`) y elevación a revisión directiva (`Submitted`).
- **Vinculación familiar:** Generación de invitaciones con token criptográfico seguro (vigencia de 7 días) para vincular a los padres con su alumno a cargo.

---

### Rol `familia`
**Actor:** Representante Familiar (Padres, Madres, Tutores Legales o Cuidadores)

Acompaña la trayectoria educativa y terapéutica del estudiante desde el hogar. Su rol es fundamentalmente de seguimiento, consulta y comunicación bidireccional protegida con la escuela. No interviene en la configuración curricular ni accede a notas clínicas en bruto.

El acceso de la familia está blindado por un mecanismo de alta controlada: **no existe auto-registro libre**. Solo pueden incorporarse mediante invitación formal validada por el equipo docente o directivo.

**Decisiones técnicas que dependen de este rol:**
- **Alta exclusivamente por invitación:** Registro condicionado a la posesión de un token de vinculación unívoco con el alumno.
- **Acceso a informes validados:** Solo visualiza informes en estado `Approved` (aprobados previamente por la dirección escolar).
- **Trazabilidad y acuse de lectura:** Al abrir un informe por primera vez, el sistema despacha asíncronamente `PATCH /api/reports/{id}/mark-read`, dejando constancia formal del acceso familiar para la escuela.
- **Vistas exclusivas:** Dashboard familiar con resumen de logros, visualizador de informes con descarga oficial en PDF, y canal de mensajería interna acotado al legajo del menor.

---

### Rol `admin` — Administrador Institucional (Equipo Directivo)
**Actor:** Administrador de la Institución (Director/a, Vicedirector/a, Secretaría de la escuela)

Representa la máxima autoridad legal, operativa y pedagógica del establecimiento. Su responsabilidad es garantizar la calidad del servicio educativo, auditar los procesos y salvaguardar la privacidad de los menores conforme a las normativas de educación y protección de datos personales.

Su alcance está delimitado de forma inviolable por el `institutionId` emitido en su token JWT: todas las consultas en la base de datos filtran automáticamente por la sede escolar correspondiente, impidiendo cualquier visibilidad interinstitucional cruzada.

Para reflejar fielmente la estructura orgánica del centro educativo, este rol técnico se complementa con el catálogo canónico de **Roles Institucionales (`InstitutionalRoles`)**:
- *Director / Directora*
- *Vicedirector / Vicedirectora*
- *Secretario / Secretaria*
- *Preceptor / Preceptora*

**Decisiones técnicas que dependen de este rol:**
- **Gobierno del personal docente:** Alta directa de profesionales en la nómina institucional, fiscalización de matrículas y títulos habilitantes, reseteo de contraseñas olvidadas (`MustChangePassword = true`) y baja lógica inmediata de accesos ante desvinculaciones laborales.
- **Gobierno de la matrícula estudiantil:** Alta de legajos de alumnos y asignación formal de estudiantes a docentes y salas/cursos ("Mi Aula").
- **Control de calidad e intermediación obligatoria:** Auditoría de informes pedagógicos elevados por los docentes. La dirección es el único actor facultado para **Aprobar** (publicando a la familia con firma institucional) o **Rechazar** (con observaciones pedagógicas obligatorias para su corrección docente).
- **Configuración institucional:** Mantenimiento de los datos de sede (nombre, CUE ministerial, dirección, membrete y logo oficial) y estandarización de los catálogos escolares (materias, diagnósticos, etc.).

---

### Por qué la Institución no tiene rol

La **Institución Educativa** es una entidad organizativa y un límite de aislamiento multi-tenant, no una persona ni un actor que inicie sesión. No posee credenciales ni ejecuta comandos. Su función técnica es suministrar el contexto de pertenencia (`institutionId`) que los middleware y políticas de autorización de .NET validan en cada petición para confinar la información dentro de los muros digitales de esa escuela.

---

## Resumen de separación de responsabilidades

| Responsabilidad / Acción | `persona` | `profesional` | `familia` | `admin` (Dirección) |
|---|:---:|:---:|:---:|:---:|
| Ejecutar actividades didácticas interactivas | ✓ | | | |
| Diseñar actividades con templates DUA y pictogramas | | ✓ | | |
| Calibrar y supervisar el Roadmap del estudiante | | ✓ | | |
| Registrar diagnósticos funcionales cifrados (AES-256-GCM) | | ✓ (asignados) | | |
| Redactar informes de progreso preliminares (`Draft`) | | ✓ | | |
| Auditar, aprobar o rechazar informes escolares oficiales | | | | ✓ |
| Consultar informes aprobados y registrar acuse de lectura | | | ✓ | |
| Descargar constancias e informes oficiales en PDF | | ✓ | ✓ | ✓ |
| Canal de mensajería escolar interna | | ✓ | ✓ | ✓ |
| Dar de alta a docentes y fiscalizar matrículas | | | | ✓ |
| Gestionar credenciales y seguridad (reseteo, bloqueo, baja) | | | | ✓ |
| Matricular alumnos y asignar docentes/salas | | | | ✓ |
| Configurar datos de la sede (CUE, logo, membrete) y catálogos | | | | ✓ |
