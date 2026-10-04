# 📘 Contexto Maestro de Negocio, Controles y Requerimientos — InclusiON

**Institución Cervantes — Prácticas Profesionalizantes**  
**Proyecto:** InclusiON — Plataforma Web de Inclusión Educativa y Terapéutica  
**Fecha de consolidación:** Octubre 2026  
**Propósito del documento:** Compendio exhaustivo del contexto de negocio, marco regulatorio, controles pedagógicos, evaluación de TIC, resolución de problemáticas, matriz de trazabilidad de requerimientos (funcionales y no funcionales), oportunidades y auditoría de consistencia de la plataforma.

---

## ÍNDICE GENERAL

1. [Visión de Negocio y Marco Legal](#1-visión-de-negocio-y-marco-legal)
   - 1.1. Propósito y Misión de InclusiON
   - 1.2. Marco Jurídico y Normativo Vigente
   - 1.3. Ecosistema de Actores del Negocio
2. [Controles de Gestión Pedagógica y Terapéutica](#2-controles-de-gestión-pedagógica-y-terapéutica)
   - 2.1. Control de Evaluación Inicial
   - 2.2. Control de Planificación
   - 2.3. Control de Intervención
   - 2.4. Control de Seguridad y Bienestar
   - 2.5. Control de Seguimiento y Progreso
   - 2.6. Control de Comunicación y Coordinación
   - 2.7. Control de Documentación
3. [Tecnologías de Información y Comunicación (TIC)](#3-tecnologías-de-información-y-comunicación-tic)
   - 3.1. TIC para la Comunicación
   - 3.2. TIC para la Educación y el Aprendizaje
   - 3.3. TIC para el Control y Seguimiento
   - 3.4. TIC de Apoyo Físico y Accesibilidad
   - 3.5. TIC de Inclusión Social
4. [Problemas del Sistema Actual y Solución Aportada](#4-problemas-del-sistema-actual-y-solución-aportada)
5. [Matriz de Trazabilidad de Requerimientos Funcionales (32 RF)](#5-matriz-de-trazabilidad-de-requerimientos-funcionales-32-rf)
   - 5.1. Matriz de ABMs (CRUDs)
   - 5.2. Asignaciones, Vinculaciones y Circuitos Transaccionales
   - 5.3. Reportes, Diagnósticos y Comunicación
   - 5.4. Precisiones de Arquitectura sobre Roles y Bajas Lógicas
6. [Requerimientos No Funcionales (RNF)](#6-requerimientos-no-funcionales-rnf)
   - 6.1. Seguridad, Confidencialidad y Privacidad de Datos
   - 6.2. Cumplimiento del Marco Legal
   - 6.3. Accesibilidad Universal y Usabilidad (WCAG 2.2 AA)
7. [Las 7 Oportunidades de Negocio e Innovación](#7-las-7-oportunidades-de-negocio-e-innovación)
8. [Auditoría de Consistencia: Elementos No Usados, Gaps y Desvíos Documentales](#8-auditoría-de-consistencia-elementos-no-usados-gaps-y-desvíos-documentales)

---

## 1. Visión de Negocio y Marco Legal

### 1.1. Propósito y Misión de InclusiON
InclusiON nace como respuesta a la histórica fragmentación y desarticulación que sufren las escuelas especiales, centros de rehabilitación y familias en el seguimiento de personas con discapacidad (PCD).

La plataforma adopta el **modelo social de la discapacidad**, superando el enfoque exclusivamente médico/asistencialista. En este modelo:
- La persona con discapacidad es un **sujeto pleno de derecho**, con capacidad de aprender, elegir y progresar a su propio ritmo.
- Las limitaciones no residen en el individuo, sino en las **barreras físicas, comunicacionales y de diseño** que el entorno le impone.
- La tecnología actúa como puente inclusivo, democratizando el acceso al conocimiento y dotando a los profesionales de herramientas objetivas para defender y certificar las trayectorias de sus alumnos.

### 1.2. Marco Jurídico y Normativo Vigente
El diseño conceptual y funcional de InclusiON se sustenta en normativas de jerarquía constitucional, nacional y provincial:

1. **Ley de Educación Nacional N.º 26.206 (2006):**
   - *Art. 11:* Consagra la igualdad de oportunidades y dispone brindar a las personas con discapacidad una propuesta pedagógica que desarrolle al máximo sus posibilidades.
   - *Art. 42, 43 al 45:* Establece a la Educación Especial como modalidad transversal y garantiza recursos didácticos y tecnologías asistivas.
   - *Art. 88:* Incorpora las TIC como contenidos indispensables para la inclusión ciudadana.
2. **Resolución CFE N.º 311/2016 (Consejo Federal de Educación):**
   - Eje medular que exige la confección y seguimiento formal del **Proyecto Pedagógico Individual (PPI)** para la promoción, acreditación y titulación oficial del estudiante.
   - Ordena la aplicación de ajustes razonables y elimina barreras en la evaluación.
3. **Ley Provincial de Educación N.º 9870 / Resolución Ministerial N.º 1825/2019 (Córdoba):**
   - Implementa los lineamientos de educación inclusiva en el sistema educativo provincial, obligando a la articulación documental entre escuela común, escuela especial y equipos externos.
4. **Convención Internacional sobre los Derechos de las Personas con Discapacidad (ONU) — Ley Nacional N.º 27.044 (2014):**
   - Con jerarquía constitucional en Argentina. El *Artículo 24* prohíbe taxativamente la exclusión del sistema educativo general por motivos de discapacidad.
5. **Ley N.º 25.326 de Protección de los Datos Personales (Habeas Data):**
   - Exige resguardo inmutable, cifrado y consentimiento informado para los datos sensibles y clínicos de menores de edad.

### 1.3. Ecosistema de Actores del Negocio
La plataforma articula 4 roles esenciales:
* **Persona con Discapacidad (Estudiante):** Destinatario de las experiencias pedagógicas lúdicas ("Mi Camino"). Interactúa mediante interfaz AAC adaptativa con pictogramas ARASAAC, síntesis de voz y reintentos ilimitados.
* **Profesional (Docente, Psicopedagogo, Terapeuta, Fonoaudiólogo):** Diseña y cursa diagnósticos funcionales, organiza los roadmaps de actividades, evalúa bajo métrica GAS y monitorea en tiempo real el aula.
* **Representante Familiar (Padres / Tutores):** Recibe invitaciones, consulta informes oficiales de avance aprobados por la escuela y mantiene comunicación fluida y formal con el equipo docente.
* **Administrador Institucional (Dirección / Secretaría Escolar):** Administra la sede escolar, gestiona la vigencia de matrículas de profesionales, asigna salas/aulas, fiscaliza los informes antes de su publicación y custodia la seguridad del establecimiento.

---

## 2. Controles de Gestión Pedagógica y Terapéutica

InclusiON implementa en su arquitectura de software y reglas de negocio los 7 controles operacionales del trabajo interdisciplinario:

### 2.1. Control de Evaluación Inicial
* **Objetivo:** Garantizar un diagnóstico integral de partida antes de asignar cargas pedagógicas.
* **Implementación en el Sistema:**
  - `DiagnosesController` y entidad `Diagnosis`: estructura los campos obligatorios de *Diagnóstico General*, *Limitaciones Funcionales*, *Objetivos Pedagógicos* y *Estrategias Recomendadas*.
  - En la ficha de la persona (`PersonWithDisability`) se registran antecedentes médicos, turno escolar, ayudas técnicas requeridas (audífonos, silla de ruedas, tablero de comunicación) y tutores legales.
  - Perfil de habilidades multidimensional (`SkillProfile`) que califica de 0 a 100 las áreas motriz, cognitiva, comunicativa y social.

### 2.2. Control de Planificación
* **Objetivo:** Asegurar que cada estudiante tenga un programa de trabajo individualizado, claro y medible.
* **Implementación en el Sistema:**
  - `RoadmapController` y entidades `PersonRoadmap` / `PersonRoadmapActivity`: asignan un camino de 10 niveles progresivos agrupados por áreas de habilidad.
  - Adopción de la metodología **GAS (Goal Attainment Scaling)** en `AnalyticsController`: escala estandarizada de -2 (mucho menos de lo esperado) a +2 (mucho más de lo esperado) para evaluar el progreso real frente a las metas fijadas en el PPI.

### 2.3. Control de Intervención
* **Objetivo:** Fiscalizar la correcta ejecución de actividades, la adecuación de tecnologías asistivas y la respuesta del alumno.
* **Implementación en el Sistema:**
  - Panel "Mi Aula" (`AnalyticsController`): el profesional supervisa en tiempo real los alumnos activos, actividades en curso, tiempos dedicados y tasa de aciertos.
  - Integración nativa del catálogo oficial de pictogramas **ARASAAC** para garantizar apoyos visuales estandarizados.
  - Ajuste de consignas por tipo de plantilla interactiva (Matching, Ordenamiento, Flashcards, Memoria, Rompecabezas).

### 2.4. Control de Seguridad y Bienestar
* **Objetivo:** Preservar la integridad emocional del estudiante durante su interacción digital.
* **Implementación en el Sistema:**
  - **Mecanismo anti-frustración (HU-21 / CB-07):** Tras 3-4 intentos fallidos consecutivos, el sistema detiene la actividad, despliega una pantalla amigable de respiración y descanso ("Tomemos un recreo"), y emite una alerta pedagógica al panel del docente.
  - **Filosofía de Cero Punición:** No existen puntajes negativos, descuentos de vidas ni pantallas de derrota que generen ansiedad o sobrecarga sensorial.

### 2.5. Control de Seguimiento y Progreso
* **Objetivo:** Medir logros continuos y ajustar las estrategias pedagógicas en función de la evidencia.
* **Implementación en el Sistema:**
  - Cálculo automático de indicadores: Tasa de Éxito (`SuccessRate`), Promedio de Intentos y Tiempo Invertido.
  - Componente gráfico nativo **Radar Chart de Habilidades** (`SkillRadarChartComponent`) y visualizaciones de barras SVG con los 10 niveles superados.
  - Registro de auditoría por cada respuesta enviada (`ActivityResponse`).

### 2.6. Control de Comunicación y Coordinación
* **Objetivo:** Mantener sincronizados a docentes, terapeutas externos y familias.
* **Implementación en el Sistema:**
  - `MessagesController`: módulo de mensajería interna estructurada por hilos de conversación, asunto y notificaciones de lectura.
  - Asignación multidisciplinaria: varios profesionales de distintas especialidades (fonoaudiología, psicología, educación especial) pueden compartir el seguimiento del mismo estudiante sin solapamiento de datos.

### 2.7. Control de Documentación
* **Objetivo:** Cumplir con la inmutabilidad y disponibilidad de la historia educativa.
* **Implementación en el Sistema:**
  - `ReportsController`: circuito de vida del informe (*Borrador ➔ Enviado para Revisión ➔ Aprobado por Dirección ➔ Publicado para el Familiar*).
  - **Generación y exportación a PDF oficial institucional** con membrete de la escuela, firma digital del profesional y datos de matrícula.

---

## 3. Tecnologías de Información y Comunicación (TIC)

InclusiON explota las 5 dimensiones tecnológicas identificadas en el relevamiento:

| Dimensión TIC | Tecnologías Implementadas en InclusiON |
| :--- | :--- |
| **1. Comunicación** | • Portal AAC (Comunicación Aumentativa y Alternativa) con pictogramas ARASAAC.<br>• Aplicación web progresiva (PWA) adaptable a tablets y teléfonos táctiles.<br>• Síntesis de voz (Text-to-Speech / TTS) que verbaliza las consignas en español neutro. |
| **2. Educación y Aprendizaje** | • 4 portales virtuales por rol con layouts y guards dedicados.<br>• Motor adaptativo con progresión automática según umbral de superación.<br>• 5 plantillas didácticas multimedia con retroalimentación sonora y visual positiva. |
| **3. Control y Seguimiento** | • Servidor de base de datos relacional PostgreSQL 17 con Docker y Entity Framework Core.<br>• **Teleasistencia educativa asincrónica:** práctica guiada en el hogar auditada remotamente desde el gabinete escolar. |
| **4. Apoyo Físico y Accesibilidad** | • 100% navegable mediante teclado (foco visible y accesible para pulsadores y conmutadores).<br>• 14 perfiles visuales: 7 modos (Default, Alto Contraste, Dislexia con fuente *OpenDyslexic*, Baja Visión con tipografía escalada, Deuteranopía, Protanopía, Tritanopía) × 2 temas (Claro / Oscuro).<br>• Lector de pantalla asistido mediante atributos semánticos WAI-ARIA. |
| **5. Inclusión Social** | • Entorno protegido y privado de vinculación escuela-familia.<br>• Portal familiar con estrategias de estimulación en el hogar y guías de acompañamiento. |

---

## 4. Problemas del Sistema Actual y Solución Aportada

| Problema Relevado | Causa en el Modelo Tradicional | Solución Diseñada en InclusiON |
| :--- | :--- | :--- |
| **Información descentralizada** | Carpetas físicas dispersas entre la escuela, consultorios privados y cuadernos de comunicaciones. | Base de datos centralizada con ficha única del alumno, historial diagnóstico, actividades y reportes en una sola URL institucional. |
| **Procesos manuales de seguimiento** | Docentes dedicando horas extra en calcular promedios y tabular planillas manuscritas. | Registro telemétrico instantáneo: el sistema computa errores, aciertos y tiempos al instante en que el alumno interactúa. |
| **Barreras de accesibilidad** | Plataformas comerciales que exigen contraseñas alfanuméricas complejas y carecen de adaptaciones visuales. | **Login Adaptativo:** acceso mediante **PIN numérico de 4 dígitos** o **Login Asistido** (autorizado por docente/tutor), más panel de accesibilidad global (`Alt+A`). |
| **Seguimiento familiar demorado** | Familias que solo se enteran de las dificultades en las reuniones de fin de trimestre. | Portal Familiar en tiempo real: los padres observan logros diarios, medallas obtenidas e informes oficiales apenas son aprobados. |
| **Comunicación dispersa** | Mensajes cruzados por cuadernos de comunicaciones que se pierden, notas en papel o grupos informales de WhatsApp. | Módulo oficial de mensajería interna con constancia de envío, lectura y resguardo institucional. |
| **Dificultad para estandarizar actividades** | Cada docente improvisa fotocopias o aplicaciones genéricas no diseñadas para educación especial. | Catálogo estandarizado de actividades clasificadas por áreas curriculares y ligadas a un Roadmap estructurado de 10 niveles. |
| **Sobrestimulación y frustración** | Videojuegos comerciales con anuncios invasivos, sonidos estridentes y penalizaciones que generan angustia. | Entorno calmo, limpio, libre de publicidad, con refuerzo positivo contingente y detección automática de frustración. |

---

## 5. Matriz de Trazabilidad de Requerimientos Funcionales (32 RF)

### 5.1. Matriz de ABMs (CRUDs)

| Entidad / Objeto | Registrar (Alta) | Actualizar (Modificación) | Dar de Baja (Eliminación) | Implementación Técnica |
| :--- | :---: | :---: | :---: | :--- |
| **Usuarios** | ✅ | ✅ | ✅ | `AdminUsersController` / Soft-delete con revocación inmediata de tokens JWT. |
| **Roles** | ⚠️ | ✅ | ⚠️ | Roles estructurales fijos de dominio; la actualización es sobre sus **Permisos** (Claims). |
| **Profesional** | ✅ | ✅ | ✅ | `ProfessionalsController` / Alta directiva centralizada con validación registral de matrícula y especialidad. |
| **Institución Educativa** | ✅ | ✅ | ✅ | `InstitutionsController` y `AdminInstitutionsController`. |
| **Persona con Discapacidad** | ✅ | ✅ | ✅ | `PersonsController` / Wizard de matrícula unificada en 3 pasos. |
| **Representante Familiar** | ✅ | ✅ | ✅ | `FamilyController` e `InvitationsController` (invitación por correo). |
| **Actividad** | ✅ | ✅ | ✅ | `ActivitiesController` (con selector de pictogramas ARASAAC y plantillas). |
| **Diagnóstico** | ✅ | ✅ | ✅ | `DiagnosesController` / Baja lógica (`IsActive = false`) para resguardo histórico. |

### 5.2. Asignaciones, Vinculaciones y Circuitos Transaccionales

| Requerimiento Funcional | Cumple | Justificación y Endpoint |
| :--- | :---: | :--- |
| **Relacionar profesional con persona con discapacidad** | ✅ Sí | `POST /api/assignments/professional-to-person`: vincula al docente con el alumno en un aula determinada. |
| **Relacionar profesional, alumno y familiar** | ✅ Sí | `POST /api/persons/wizard` y `POST /api/family/{id}/link-person`: vinculación transaccional unificada. |
| **Asignar rol a usuario** | ✅ Sí | Asignación en el alta y gestión de membresías de ASP.NET Identity. |
| **Asignar profesional a institución educativa** | ✅ Sí | `POST /api/assignments/professional-to-institution`. |
| **Asignar actividades a persona con discapacidad** | ✅ Sí | `RoadmapController` y `ActivityAssignmentsController`. |

### 5.3. Reportes, Diagnósticos y Comunicación

| Requerimiento Funcional | Cumple | Justificación y Endpoint |
| :--- | :---: | :--- |
| **Generar y emitir reportes de actividades** | ✅ Sí | `ReportsController` e informes de métricas pedagógicas en `AnalyticsController`. |
| **Generar y emitir reporte de seguimiento** | ✅ Sí | `ReportsController`: circuito de aprobación y **exportación a PDF oficial** (`/api/reports/{id}/export-pdf`). |
| **Enviar y recibir mensajes entre profesional y familias** | ✅ Sí | `MessagesController`: bandeja de entrada, envío con asunto/cuerpo e hilos de respuesta. |

### 5.4. Precisiones de Arquitectura sobre Roles y Bajas Lógicas

1. **Gestión de Roles mediante Permisos (Claims):**
   - En software de misión crítica con autenticación basada en Identity, los roles son constructos del dominio (`Admin`, `Professional`, `FamilyRepresentative`, `PersonWithDisability`).
   - Crear o borrar nombres de roles arbitrarios en tiempo de ejecución rompería los controladores y pantallas. Por ende, la **administración de roles** se materializa en la **asignación dinámica de permisos granulares por módulo** (`users:read`, `diagnoses:create`, `reports:approve`, etc.) mediante `PUT /api/roles/{roleId}/permissions`.
2. **Inmutabilidad y Baja Lógica en Diagnósticos e Informes:**
   - La Ley 25.326 y los principios de bioética médica/pedagógica prohíben la destrucción física (`HARD DELETE`) de antecedentes clínicos y educativos.
   - En InclusiON, dar de baja un diagnóstico o reporte aplica **baja lógica (`IsActive = false`)**, garantizando la trazabilidad histórica ante requerimientos judiciales o de supervisión escolar.

---

## 6. Requerimientos No Funcionales (RNF)

### 6.1. Seguridad, Confidencialidad y Privacidad de Datos
* **Cifrado de Datos Clínicos:** Campos sensibles de diagnósticos protegidos con **AES-256-GCM** en reposo mediante el atributo `[Encrypted]`.
* **Protección de Credenciales:** Contraseñas hasheadas con ASP.NET Identity y códigos PIN de estudiantes hasheados con **Argon2id** (estándar recomendado por OWASP).
* **Autenticación y Sesiones:** Tokens JWT firmados, Refresh Tokens seguros e invalidación inmediata de sesiones activas al modificar permisos o desactivar cuentas.
* **Aislamiento Multi-Tenant:** Filtro global `InstitutionAccessFilter` que impide que un administrador o docente acceda a datos de otra sede escolar.
* **Autorización por Recurso (Row-Level Security):** Verificación server-side de que el profesional autenticado tenga asignado efectivamente al alumno antes de permitir lectura/escritura.

### 6.2. Cumplimiento del Marco Legal
* Operación alineada a la Ley 26.206 (acceso universal a TIC), Res. CFE 311/16 (soporte documental al PPI), Ley Provincial 9870 / Res. 1825/19 y Convención de la ONU (Ley 27.044).

### 6.3. Accesibilidad Universal y Usabilidad (WCAG 2.2 AA)
* **Panel de Accesibilidad Global:** Activación por atajo de teclado `Alt+A` en cualquier portal.
* **14 Combinaciones Visuales:** 7 perfiles (Default, Alto Contraste con ratio > 7:1, Dislexia con tipografía adaptada, Baja Visión con fuentes ampliadas en unidades `rem`, y 3 modos para daltonismo: Protanopía, Deuteranopía y Tritanopía) combinables con temas Claro y Oscuro.
* **Navegación por Teclado:** Foco visible en todos los botones y selectores, permitiendo la operación mediante pulsadores o conmutadores adaptados.

---

## 7. Las 7 Oportunidades de Negocio e Innovación

1. **Incorporación de tecnología accesible:** Portal AAC con pictogramas ARASAAC, interfaces despojadas de elementos distractores y lectura por voz integrada.
2. **Digitalización del seguimiento profesional:** Sustitución de cuadernos y planillas manuales por dashboards interactivos con cálculo automático de aciertos, tiempos y tasa de éxito.
3. **Fortalecimiento del trabajo interdisciplinario:** Unificación en un solo perfil de las intervenciones de docentes de grado, maestras integradoras, psicólogos y fonoaudiólogos.
4. **Difusión del enfoque inclusivo y marco legal:** La plataforma opera como garante formal del PPI (Res. CFE 311/16), estructurando la labor escolar bajo el paradigma de derechos.
5. **Entorno adaptable y escalable:** Arquitectura en capas desacoplada con patrón CQRS (159 handlers auto-registrados), lista para incorporar nuevas sedes e instituciones sin rediseño estructural.
6. **Mejora en la comunicación escuela-familia:** Canal formal y confidencial con hilos de mensajes y consulta inmediata de informes oficiales aprobados.
7. **Prevención de la sobreestimulación y frustración:** Diseño sobrio, sin elementos invasivos de ludopatía/adicción, con pausas preventivas ante 3-4 fallos y refuerzo motivacional positivo.

---

## 8. Auditoría de Consistencia: Elementos No Usados, Gaps y Desvíos Documentales

Al contrastar la documentación académica histórica con el código fuente real del repositorio, se identifican las siguientes observaciones clave para mantener la defensa del proyecto pulida y consistente:

1. **Módulo de Soporte y Tickets / FAQ (Proceso 19 / HU-13 / CU-13):**
   * *Estado:* Está profusamente diagramado y documentado en Confluence y casos de uso, pero **no existe en el código** (no hay controladores ni vistas de tickets ni FAQ). En [checklist-procesos.md](file:///D:/git/InclusiON.Documents/State/checklist-procesos.md) figura correctamente como "Post-MVP / Futuro". Debe presentarse como *trabajo futuro / roadmap versión 2.0*.
2. **Tour Interactivo y Onboarding Guiado (Proceso 18 / HU-12):**
   * *Estado:* Se documentó como un walkthrough con tooltips paso a paso (tipo Shepherd.js). En la implementación real se resolvió la facilidad de uso mediante el **Wizard de Matriculación en 3 Pasos** (`register-student-wizard`), pero no se construyó el tour interactivo.
3. **Supresión del "Administrador Global" en favor del "Administrador Institucional":**
   * *Estado:* En la base de datos solo existe el rol `Admin`. El concepto de un "Super Admin Global" que administra todas las escuelas fue reemplazado por la figura del Administrador de Sede Escolar con aislamiento multi-tenant. Los documentos antiguos que mencionan al Admin Global deben interpretarse bajo esta unificación.
4. **Eliminación del Código CUE (Código Único de Establecimiento):**
   * *Estado:* Fue suprimido intencionalmente del modelo de datos para permitir que centros terapéuticos, consultorios privados y ONG sin matrícula CUE ministerial puedan utilizar la plataforma.
5. **Eliminación del Login Visual por Nombre y Contraseña para Alumnos:**
   * *Estado:* Fue discontinuado por inaccesible (IN-310). Se consolidó el acceso de alumnos exclusivamente mediante **PIN Numérico (4 dígitos)** y **Login Asistido**.
6. **Desfase en la Sección 7 de `proyecto-final-actualizado.md`:**
   * *Estado:* Ese documento quedó con métricas congeladas del Sprint 2 (afirmando que los Sprints 3 a 5 estaban pendientes). La realidad del repositorio es que se completaron con éxito los **12 Sprints**, alcanzando el 100% de la funcionalidad core comprometida.

---
*Documento consolidado para consulta, validación académica y defensa de proyecto final.*
