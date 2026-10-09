# 📘 MANUAL DE USUARIO OFICIAL — PLATAFORMA INCLUSION

---

## 🏛️ CARÁTULA

```text
========================================================================================
                          INSTITUCIÓN CERVANTES
       CARRERA: TECNICATURA SUPERIOR EN ANÁLISIS DE SISTEMAS DE COMPUTACIÓN
     ESPACIO CURRICULAR: PRÁCTICA PROFESIONALIZANTE II / III & ADM. DE PROYECTOS IT
========================================================================================

           PROYECTO FINAL DE APLICACIÓN Y SISTEMAS PROFESIONALES:
                               InclusiON
    Plataforma Integral de Gestión, Aprendizaje y Comunicación Adaptativa
                     para la Inclusión Educativa y Terapéutica

========================================================================================
DOCUMENTO:            MANUAL DE USUARIO INTEGRAL DEL SISTEMA
VERSIÓN:              3.0 (Edición Definitiva Consolidada — Todos los Sprints)
FECHA DE EMISIÓN:     Octubre 2026
ESTADO:               Homologado / Listo para Producción

EQUIPO DE PROYECTO Y DESARROLLO (AUTORES):
  • Mariano Decalli       | Scrum Master, Analista de Procesos y QA Lead
  • Sacha Del Barrio      | Analista Funcional, Calidad (QA) y Accesibilidad Cognitiva Lead
  • Germán Cochis         | Frontend Lead, Diseñador UX/UI y Accesibilidad Visual Lead
  • Fernando Aparicio     | Backend Lead, Arquitectura de Datos y QA Automatización

PRODUCT OWNERS / REFERENTES DE EDUCACIÓN ESPECIAL:
  • Lic. Candelaria Ferreyra | Referente Pedagógica y de Educación Especial
  • Lic. Catalina Vettorazzi  | Referente en Terapia Ocupacional y Neurodesarrollo

DOCENTES EVALUADORES ACADÉMICOS:
  • Prof. Ing. González
  • Prof. Lic. Ferrando

MARCO NORMATIVO Y ESTÁNDARES DE CONFORMIDAD:
  • Ley Nacional de Educación N° 26.206
  • Resolución del Consejo Federal de Educación (CFE) N° 311/16
  • Ley Nacional de Protección de Datos Personales N° 25.326
  • Pautas de Accesibilidad para el Contenido Web (WCAG 2.2 — Niveles AA / AAA)
  • Sistema Aumentativo y Alternativo de Comunicación (SAAC / ARASAAC)
========================================================================================
```

---

## 📝 HISTORIAL DE REVISIÓN O CONTROL DE CAMBIOS

| Versión | Fecha | Autor / Rol | Descripción del Cambio y Alcance Funcional | Estado / Aprobación |
|:---:|:---:|---|---|:---:|
| **0.1** | 10/04/2026 | **Mariano Decalli**<br>Scrum Master / Analista | **Sprint 0:** Definición del alcance inicial, relevamiento metodológico, definición de actores, herramientas de trabajo colaborativo y repositorio base. | Aprobado (PO) |
| **0.5** | 22/05/2026 | **Sacha Del Barrio**<br>Analista Funcional | **Sprints 1 a 3:** Especificación de requerimientos funcionales para los 4 roles. Maquetación del diseño adaptativo universal y definición de la arquitectura de datos relacional. | Aprobado (Equipo) |
| **1.0** | 15/07/2026 | **Fernando Aparicio**<br>Backend Lead | **Sprints 4 y 5:** Implementación del motor de autenticación JWT, soporte de multi-tenancy institucional, alta de plantillas didácticas DUA y buscador ARASAAC. | Aprobado (QA Lead) |
| **1.5** | 01/09/2026 | **Germán Cochis**<br>Frontend Lead | **Sprints 6 y 7:** Incorporación de tableros analíticos institucionales, gráficos de alto contraste, persistencia de eventos de agenda/calendario y mensajería multirrol en tiempo real. | Aprobado (Docentes) |
| **2.0** | 06/10/2026 | **Sacha Del Barrio**<br>Analista Funcional | **Sprints 8 a 10:** Consolidación de "Mi Camino" en 10 estaciones, alerta temprana en vivo (CB-07/SignalR), eliminación de bloqueo punitivo y auditoría de informes con firma digital. | Aprobado (Comité) |
| **3.0** | 08/10/2026 | **Equipo InclusiON**<br>Consolidado Final | **Sprints 11 y 12 / Release Final:** Confección integral del manual definitivo con carátula formal, especificación técnica profunda, matriz de endpoints, guías operativas paso a paso, simulaciones reales y glosario extendido unificado. | **Versión Final Homologada** |

---

## 📑 TABLA DE CONTENIDOS

1. [Introducción y Bienvenida](#1-introducción-y-bienvenida)
   * 1.1. [Propósito de InclusiON y Enfoque Social de la Discapacidad](#11-propósito-de-inclusion-y-enfoque-social-de-la-discapacidad)
   * 1.2. [Los Cuatro Portales del Sistema](#12-los-cuatro-portales-del-sistema)
2. [Especificaciones Técnicas del Sistema](#2-especificaciones-técnicas-del-sistema)
   * 2.1. [Arquitectura Tecnológica General y Diagrama de Despliegue](#21-arquitectura-tecnológica-general-y-diagrama-de-despliegue)
   * 2.2. [Stack Tecnológico Frontend y Accesibilidad](#22-stack-tecnológico-frontend-y-accesibilidad)
   * 2.3. [Stack Tecnológico Backend y Persistencia](#23-stack-tecnológico-backend-y-persistencia)
   * 2.4. [Matriz Completa de Endpoints de la API REST](#24-matriz-completa-de-endpoints-de-la-api-rest)
   * 2.5. [Seguridad, Cifrado y Privacidad de Datos](#25-seguridad-cifrado-y-privacidad-de-datos)
   * 2.6. [Requisitos Mínimos de Hardware, Software y Conectividad](#26-requisitos-mínimos-de-hardware-software-y-conectividad)
3. [Ingreso al Sistema y Gestión de Acceso](#3-ingreso-al-sistema-y-gestión-de-acceso)
   * 3.1. [Direcciones Web Oficiales de Acceso](#31-direcciones-web-oficiales-de-acceso)
   * 3.2. [Ingreso del Personal Directivo y Profesional](#32-ingreso-del-personal-directivo-y-profesional)
   * 3.3. [Registro Público y Validación de Profesionales](#33-registro-público-y-validación-de-profesionales)
   * 3.4. [Ingreso Adaptativo para Estudiantes con Discapacidad](#34-ingreso-adaptativo-para-estudiantes-con-discapacidad)
   * 3.5. [Ingreso y Activación de Familias por Invitación](#35-ingreso-y-activación-de-familias-por-invitación)
   * 3.6. [Recuperación y Cambio de Contraseñas](#36-recuperación-y-cambio-de-contraseñas)
   * 3.7. [Catálogo Maestro de Cuentas de Demostración y Prueba](#37-catálogo-maestro-de-cuentas-de-demostración-y-prueba)
4. [Módulo Transversal: Accesibilidad Universal (Panel Alt + A)](#4-módulo-transversal-accesibilidad-universal-panel-alt--a)
   * 4.1. [Cómo Abrir el Panel de Accesibilidad](#41-cómo-abrir-el-panel-de-accesibilidad)
   * 4.2. [Los 7 Perfiles Visuales Adaptativos](#42-los-7-perfiles-visuales-adaptativos)
   * 4.3. [Herramientas de Apoyo Cognitivo y Motor](#43-herramientas-de-apoyo-cognitivo-y-motor)
5. [Explicación de cada Funcionalidad por Rol](#5-explicación-de-cada-funcionalidad-por-rol)
   * 5.1. [Módulo I: Portal del Administrador Institucional (/admin)](#51-módulo-i-portal-del-administrador-institucional-admin)
   * 5.2. [Módulo II: Portal del Profesional Docente y Terapeuta (/pro)](#52-módulo-ii-portal-del-profesional-docente-y-terapeuta-pro)
   * 5.3. [Módulo III: Portal del Estudiante con Discapacidad (/app)](#53-módulo-iii-portal-del-estudiante-con-discapacidad-app)
   * 5.4. [Módulo IV: Portal de la Familia y Tutores Legales (/family)](#54-módulo-iv-portal-de-la-familia-y-tutores-legales-family)
6. [Simulaciones Paso a Paso de Flujos Reales de Negocio](#6-simulaciones-paso-a-paso-de-flujos-reales-de-negocio)
   * 6.1. [Simulación 1: Administrador (Admisión Docente, Configuración y Aval Directivo)](#61-simulación-1-administrador-admisión-docente-configuración-y-aval-directivo)
   * 6.2. [Simulación 2: Profesional (Mi Aula, Wizard 3 Pasos, ARASAAC, IA y Alerta Temprana)](#62-simulación-2-profesional-mi-aula-wizard-3-pasos-arasaac-ia-y-alerta-temprana)
   * 6.3. [Simulación 3: Alumno (Ingreso Adaptativo, Recorrido Lúdico y SAAC)](#63-simulación-3-alumno-ingreso-adaptativo-recorrido-lúdico-y-saac)
   * 6.4. [Simulación 4: Familia (Activación, Monitoreo, Descarga de PDF y Mensajería)](#64-simulación-4-familia-activación-monitoreo-descarga-de-pdf-y-mensajería)
7. [Resolución de Problemas Frecuentes (FAQ / Troubleshooting)](#7-resolución-de-problemas-frecuentes-faq--troubleshooting)
8. [Glosario General del Sistema](#8-glosario-general-del-sistema)

---

## 1. INTRODUCCIÓN Y BIENVENIDA

### 1.1. Propósito de InclusiON y Enfoque Social de la Discapacidad
**InclusiON** es un entorno informático de vanguardia diseñado para transformar de forma integral el acompañamiento pedagógico, terapéutico y familiar de personas con discapacidad en instituciones educativas y centros de rehabilitación.

Históricamente, los procesos formativos y de integración escolar sufren una severa fragmentación operativa:
* Los planes de apoyo se registran en cuadernos impresos susceptibles de extravío o deterioro.
* Las actividades didácticas se adaptan de manera artesanal sin trazabilidad objetiva de métricas.
* Los informes trimestrales de evolución demandan incontables horas de redacción en procesadores de texto, careciendo de un vínculo directo con las evidencias de desempeño diario.
* Las familias experimentan angustia e incertidumbre al desconocer el avance cotidiano de sus hijos.

Bajo el **modelo social de la discapacidad**, InclusiON fundamenta su concepción en una premisa ineludible: **la discapacidad no reside como una deficiencia en el sujeto, sino que surge como el resultado directo de las barreras físicas, actitudinales y comunicacionales que el entorno impone**. 

InclusiON remueve sistemáticamente dichas barreras mediante interfaces configurables basadas en el **Diseño Universal para el Aprendizaje (DUA)**, métodos adaptativos de ingreso, analítica de datos en vivo, sistemas de comunicación aumentativa (SAAC) y un circuito formal y legalmente respaldado que une a la dirección escolar, el gabinete profesional, el estudiante y su núcleo familiar.

---

### 1.2. Los Cuatro Portales del Sistema
El ecosistema InclusiON se estructura en cuatro entornos especializados con control de acceso por roles (RBAC):

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                ECOSISTEMA INCLUSION                                    │
├────────────────────┬────────────────────┬────────────────────┬─────────────────────────┤
│  🏢 DIRECTIVO      │  👩‍🏫 PROFESIONAL    │  🎒 ALUMNO/A       │  👨‍👩‍👧 FAMILIA            │
│  Ruta: /admin      │  Ruta: /pro        │  Ruta: /app        │  Ruta: /family          │
├────────────────────┼────────────────────┼────────────────────┼─────────────────────────┤
│  • Admisión nómina │  • Gestión Mi Aula │  • Mi Camino       │  • Tablero sin jerga    │
│  • Padrón general  │  • Wizard 3 Pasos  │    (10 Estaciones) │  • Boletines PDF A4     │
│  • Aval de reportes│  • Creador DUA/AAC │  • 5 Jugadores     │  • Notificación lectura │
│  • Tablero KPIs    │  • Alerta en vivo  │  • Teclado PIN     │  • Calendario escolar   │
│  • Auditoría sedes │  • Búsqueda ONNX   │  • Tablero SAAC    │  • Mensajería directa   │
└────────────────────┴────────────────────┴────────────────────┴─────────────────────────┘
```

---

## 2. ESPECIFICACIONES TÉCNICAS DEL SISTEMA

### 2.1. Arquitectura Tecnológica General y Diagrama de Despliegue
InclusiON adopta un patrón arquitectónico desacoplado cliente-servidor basado en **Clean Architecture**, principios de **CQRS** (Command Query Responsibility Segregation) y comunicación bidireccional en tiempo real:

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                       1. DISPOSITIVOS CLIENTES (FRONTEND SPA)                         │
├──────────────────────────────┬──────────────────────────────┬──────────────────────────┤
│    Puesto Directivo/Docente  │    Tablet del Estudiante     │  Smartphone de la Familia│
│   • Navegador Web Moderno    │   • Tablet Android/iPad 10"+ │ • Vista Web Responsiva   │
│   • Resolución: >= 1280x720  │   • Touchscreen / App APK    │ • Notificaciones push    │
└──────────────┬───────────────┴──────────────┬───────────────┴─────────────┬────────────┘
               │                              │                             │
               │ HTTPS (TLS 1.3)              │ HTTPS / WSS (SignalR)       │ HTTPS
               ▼                              ▼                             ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                 2. SERVIDOR WEB Y API CENTRAL (ASP.NET CORE 10 / DOCKER)               │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  [Reverse Proxy: Nginx Alpine Linux]                                                   │
│  • Terminación SSL, compresión Gzip/Brotli, caché de estáticos y protección CORS       │
│  ───────────────────────────────────────────────────────────────────────────────────   │
│  [Capa de Aplicación: ASP.NET Core 10 Web API]                                         │
│  • InclusiON.Api: Controladores REST, Endpoints versionados, Filtros de Autorización   │
│  • InclusiON.Application: Casos de uso, Manejadores CQRS (MediatR), FluentValidation   │
│  • InclusiON.Domain: Entidades relacionales, Reglas de negocio puras, Value Objects    │
│  • InclusiON.Infrastructure: Repositorios EF Core, Generador QuestPDF, Hubs SignalR    │
│  • Motor de IA Local: Inferencia de embeddings vectoriales ONNX Runtime                │
│  ───────────────────────────────────────────────────────────────────────────────────   │
│  [Capa de Datos: PostgreSQL 17 + Extensión pgvector / SQLite Local para Dev]           │
│  • Cifrado criptográfico AES-256-GCM para diagnósticos y datos sensibles (Ley 25.326)   │
│  • Búsqueda vectorial semántica integrada sin dependencias de nubes comerciales        │
│  • Integridad referencial con borrado lógico universal (Soft-Delete)                   │
└──────────────────────────────────────────┬─────────────────────────────────────────────┘
                                           │
                    ┌──────────────────────┴──────────────────────┐
                    ▼                                             ▼
┌───────────────────────────────────────┐     ┌────────────────────────────────────────┐
│  3. Catálogo ARASAAC (API / CDN)      │     │  4. Servicio de Correo SMTP            │
│  • Biblioteca internacional de CAA    │     │  • Despacho de invitaciones digitales  │
│  • Miles de pictogramas normalizados  │     │  • Avisos y reseteo de claves          │
└───────────────────────────────────────┘     └────────────────────────────────────────┘
```

---

### 2.2. Stack Tecnológico Frontend y Accesibilidad
* **Framework:** Angular 19 / 20 (Single Page Application - SPA) estructurado con componentes independientes (*Standalone Components*), señales reactivas (*Signals*) y carga perezosa (*Lazy Loading*).
* **Tipado:** TypeScript 5.8 con verificación estricta de nulabilidad y modelos de datos fuertemente tipados.
* **Estilos y Diseño:** SCSS Dinámico parametrizado con variables CSS personalizadas que orquestan el motor de accesibilidad visual en tiempo real.
* **Empaquetado Móvil:** Capacitor 8 para generación de compilados nativos Android (.apk).
* **Accesibilidad Web (Normativa WCAG 2.2):**
  * Cumplimiento estricto de criterios de conformidad **Nivel AA** (y Nivel **AAA** en perfiles específicos como Alto Contraste).
  * Soporte completo de navegación por teclado con trampas de foco (*Focus Trap*) y anillos de enfoque visual (*Focus Visible*).
  * Integración con lectores de pantalla a través de etiquetas semánticas HTML5 y atributos WAI-ARIA.
  * Compatibilidad con síntesis de voz en el navegador (Web Speech API / SpeechSynthesis).

---

### 2.3. Stack Tecnológico Backend y Persistencia
* **Framework de Servidor:** .NET 10 (C# 13) sobre ASP.NET Core Web API.
* **Patrón de Arquitectura:** Clean Architecture estructurada en 4 capas desacopladas:
  * `InclusiON.Api`: Puntos de entrada HTTP, autenticación JWT, configuración de CORS y middlewares globales de manejo de excepciones.
  * `InclusiON.Application`: DTOs de transporte, comandos y consultas MediatR, y validadores de entrada FluentValidation.
  * `InclusiON.Domain`: Entidades del modelo de dominio, enumeradores de estados y reglas de invariantes de negocio.
  * `InclusiON.Infrastructure`: Acceso a base de datos mediante Entity Framework Core, repositorios específicos, generador de informes PDF (QuestPDF), canal en tiempo real (SignalR) e integraciones externas.
* **Persistencia:** PostgreSQL 17 relacional con soporte vectorial (`pgvector`) y compatibilidad transparente con SQLite para despliegues autónomos o entornos de prueba.
* **Generación Documental:** QuestPDF para compilación de documentos PDF inmutables en formato vectorial A4 con membrete institucional formal.
* **Tiempo Real:** SignalR Core mediante WebSockets seguros con fallback automático a Server-Sent Events / Long Polling.
* **IA Local:** ONNX Runtime en C# para ejecución local y privada de modelos de embeddings de texto, garantizando soberanía de datos y costo cero de consumo en la nube.

---

### 2.4. Matriz Completa de Endpoints de la API REST

La API REST de InclusiON expone una interfaz estructurada y protegida por autenticación basada en tokens JWT:

#### 🔐 1. Autenticación y Cuentas (`AuthController`, `UsersController`, `AdminUsersController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `POST` | `/api/Auth/login` | Público | Autenticación con email/contraseña para administradores, docentes y familias. |
| `POST` | `/api/Auth/person-login` | Público | Autenticación visual y adaptativa para alumnos mediante PIN numérico de 4 dígitos. |
| `POST` | `/api/Auth/person-login-assisted` | Docente/Admin | Autenticación de alumnos en modo asistido validada por credenciales del supervisor. |
| `POST` | `/api/Auth/forgot-password` | Público | Solicitud de enlace para restablecimiento de contraseña vía correo electrónico. |
| `POST` | `/api/Auth/reset-password` | Público | Consumo de token temporal y configuración de nueva contraseña. |
| `POST` | `/api/Auth/change-password` | Autenticado | Cambio de contraseña obligatorio en el primer ingreso o voluntario desde perfil. |
| `GET` | `/api/Auth/profile` | Autenticado | Obtención de los datos y permisos del usuario actualmente autenticado. |
| `GET` | `/api/AdminUsers` | Administrador | Listado de cuentas directivas institucionales. |
| `POST` | `/api/AdminUsers` | Administrador | Alta de una nueva cuenta directiva. |

#### 🏢 2. Gestión Institucional y Docente (`ProfessionalsController`, `ProfessionalValidationController`, `AdminInstitutionsController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Professionals` | Administrador | Nómina completa de profesionales con filtrado por sede y estado de habilitación. |
| `POST` | `/api/Professionals` | Administrador | Alta directa de un profesional con generación de credenciales iniciales. |
| `POST` | `/api/Professionals/register` | Público | Formulario de preinscripción y solicitud pública de profesionales docentes. |
| `POST` | `/api/ProfessionalValidation/{id}/approve` | Administrador | Aprobación directiva de un profesional pendiente con activación de cuenta. |
| `POST` | `/api/ProfessionalValidation/{id}/reject` | Administrador | Rechazo directivo de una solicitud con justificación obligatoria de rechazo. |
| `DELETE`| `/api/Professionals/{id}` | Administrador | Desvinculación segura con regla Hard Stop (bloqueo si posee alumnos a cargo). |

#### 🎒 3. Legajos de Estudiantes y Aulas (`PersonsController`, `DiagnosesController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Persons` | Admin / Pro | Padrón de alumnos matriculados (filtrado por asignación de aula en docentes). |
| `GET` | `/api/Persons/{id}` | Admin / Pro | Detalle integral del legajo: datos filiatorios, apoyos DUA y diagnósticos CIF. |
| `POST` | `/api/Persons` | Admin / Pro | Alta individual o unificada de un estudiante con inicialización de Roadmap. |
| `PUT` | `/api/Persons/{id}` | Admin / Pro | Actualización de datos del legajo y ajustes de autonomía. |
| `PUT` | `/api/Persons/{id}/access-method` | Admin / Pro | Configuración del método de acceso del alumno (PIN de 4 dígitos o Asistido). |
| `GET` | `/api/Diagnoses/person/{personId}` | Pro / Admin | Historial clínico y pedagógico de diagnósticos funcionales del estudiante. |

#### 👨‍👩‍👧 4. Familias e Invitaciones (`FamilyController`, `InvitationsController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Family` | Administrador | Listado del padrón familiar y vínculos con estudiantes. |
| `GET` | `/api/Family/my-students` | Familia | Consulta de los estudiantes tutelados por el familiar autenticado. |
| `POST` | `/api/Invitations` | Admin / Pro | Generación y despacho por email de invitación familiar con vigencia de 7 días. |
| `GET` | `/api/Invitations/validate/{code}` | Público | Verificación del estado y vigencia de un enlace de invitación familiar. |
| `POST` | `/api/Invitations/accept` | Público | Creación de cuenta y vinculación definitiva del tutor legal con el estudiante. |

#### 🧩 5. Actividades, Hojas de Ruta y Respuestas (`ActivitiesController`, `RoadmapController`, `AssignmentsController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Activities` | Pro / Admin | Catálogo de actividades didácticas con filtros de área, dificultad y búsqueda. |
| `POST` | `/api/Activities` | Profesional | Creación de nueva actividad didáctica basada en plantillas DUA y ARASAAC. |
| `GET` | `/api/Activities/semantic-search` | Profesional | Búsqueda semántica por IA local ONNX en lenguaje natural. |
| `GET` | `/api/Roadmap/{personId}` | Todos | Obtención del sendero "Mi Camino" con el estado de las 10 estaciones. |
| `POST` | `/api/Roadmap/{personId}/unlock-node`| Profesional | Desbloqueo manual extraordinario de una estación por criterio docente. |
| `POST` | `/api/Assignments/response` | Alumno / Pro | Registro transaccional de un intento de resolución y cálculo del resultado. |

#### 📄 6. Reportes Oficiales, Auditoría y PDF (`ReportsController`, `AnalyticsController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Reports` | Pro / Admin | Listado de informes pedagógicos con filtros por estado (`Draft`, `Submitted`, etc.). |
| `POST` | `/api/Reports` | Profesional | Creación y redacción de un nuevo informe de avance pedagógico. |
| `POST` | `/api/Reports/{id}/submit` | Profesional | Envío del informe al directivo (cambio de estado a `Submitted` / Pendiente). |
| `PATCH`| `/api/Reports/{id}/approve` | Administrador | Aprobación directiva, estampa de firma y despacho de aviso en vivo a la familia. |
| `PATCH`| `/api/Reports/{id}/reject` | Administrador | Devolución del informe con observaciones obligatorias para su corrección. |
| `GET` | `/api/Reports/{id}/pdf` | Todos aut. | Descarga del archivo PDF oficial A4 (dispara `IsReadByFamily = true` en familias). |
| `GET` | `/api/Analytics/admin/reports` | Administrador | Métricas consolidadas: pendientes, índice de lectura familiar y ranking docente. |
| `GET` | `/api/Analytics/professional` | Profesional | KPIs de rendimiento por aula, tasas de éxito y alertas de frustración activas. |

#### 📅 7. Agenda, Mensajería y Catálogos (`CalendarController`, `MessagesController`, `CatalogsController`)
| Método | Ruta | Rol Requerido | Descripción Funcional |
|---|---|:---:|---|
| `GET` | `/api/Calendar` | Todos aut. | Lectura de eventos según alcance del rol (institucionales o por estudiante). |
| `POST` | `/api/Calendar` | Docente / Admin | Creación de eventos, tutorías o clases con validación de no retroactividad. |
| `GET` | `/api/Messages/conversations` | Todos aut. | Listado de canales de chat abiertos ordenados cronológicamente. |
| `POST` | `/api/Messages` | Todos aut. | Envío de mensajes formales institucionales entre roles habilitados. |
| `GET` | `/api/Catalogs/{type}` | Autenticado | Consulta de catálogos maestros (discapacidades, autonomías, áreas, cargos). |

---

### 2.5. Seguridad, Cifrado y Privacidad de Datos
1. **Protección de Datos Personales de Menores y Salud (Ley 25.326):**
   * Toda la información relativa a diagnósticos médicos, certificados de discapacidad (CUD) y observaciones clínicas está sujeta a cifrado simétrico en reposo mediante el algoritmo **AES-256-GCM**.
   * Los registros de alumnos y familias nunca se eliminan físicamente de la base de datos; se implementa una política estricta de borrado lógico (**Soft-Delete**) preservando la trazabilidad histórica de los legajos.
2. **Aislamiento Multi-Tenant Institucional:**
   * Las consultas a la base de datos están protegidas por filtros globales (`InstitutionAccessFilter`). Ningún directivo o docente puede visualizar datos pertenecientes a otra institución escolar.
3. **Canal Seguro y Tokens Criptográficos:**
   * La totalidad del tráfico se transmite a través del protocolo seguro **HTTPS / TLS 1.3**.
   * Los tokens de sesión JWT incorporan identificador de usuario, rol institucional, identificador de sede y expiración automática, firmados digitalmente con algoritmos de clave asimétrica.

---

### 2.6. Requisitos Mínimos de Hardware, Software y Conectividad

#### A. Servidor Central Institucional
* **Procesador (CPU):** Arquitectura x64 / ARM64 con mínimo 4 núcleos físicos (2.4 GHz o superior).
* **Memoria RAM:** Mínimo 8 GB (16 GB recomendado para instituciones con más de 500 alumnos concurrentes).
* **Almacenamiento:** 50 GB de espacio disponible en unidad de estado sólido (SSD NVMe).
* **Sistema Operativo del Host:** Ubuntu Server 22.04 LTS / Debian 12 / Windows Server 2022.
* **Entorno de Ejecución:** Docker Engine 24+ con Docker Compose v2, o .NET 10 Runtime con PostgreSQL 17.

#### B. Dispositivos Clientes
* **Estaciones de Trabajo (Directivos y Docentes):**
  * Computadora de escritorio o laptop con procesador Dual-Core (1.8 GHz+), 4 GB RAM.
  * Pantalla con resolución mínima de 1280 x 720 píxeles.
  * Navegadores compatibles: Google Chrome (115+), Microsoft Edge (115+), Mozilla Firefox (115+) o Safari (16+).
* **Tablets Escolares (Estudiantes):**
  * Pantalla táctil capacitiva recomendada de **10 pulgadas o superior** (resolución mínima 1280 x 800 px).
  * 3 GB de memoria RAM y salida de audio activa (parlantes o auriculares accesibles).
  * Sistema Operativo: Android 10+ (App nativa APK o Chrome móvil) o iPadOS 15+ (Safari).
* **Smartphones (Familias y Tutores):**
  * Teléfono inteligente con Android 9+ o iOS 14+ y navegador web actualizado.

#### C. Conectividad de Red
* Ancho de banda de red local (LAN) o Internet de al menos 10 Mbps de bajada y 2 Mbps de subida en la institución.
* Soporte habilitado para conexiones WebSockets permanentes (puerto 443 / WSS).

---

## 3. INGRESO AL SISTEMA Y GESTIÓN DE ACCESO

### 3.1. Direcciones Web Oficiales de Acceso
El sistema cuenta con dos puntos de acceso claramente diferenciados según el perfil del usuario:

1. **Portal Institucional Directivo y Docente:**
   * **URL:** `http://localhost:4200/admin-login`
   * **Destinatarios:** Directores de sede, secretarios académicos, docentes de grado, terapeutas ocupacionales, fonoaudiólogos y psicopedagogos.
2. **Portal Visual Adaptativo para Alumnos y Familias:**
   * **URL:** `http://localhost:4200/login`
   * **Destinatarios:** Estudiantes con discapacidad y tutores familiares.

---

### 3.2. Ingreso del Personal Directivo y Profesional
1. Acceda a la dirección `/admin-login`.
2. Ingrese su correo electrónico institucional registrado (ej: `admin@test.com` o `profesional@test.com`).
3. Ingrese su contraseña de acceso.
4. Presione el botón **`Iniciar Sesión`**.
5. **Primer Ingreso con Clave Provisoria:** Si el administrador creó su cuenta recientemente, el sistema detectará el indicador de clave obligatoria y lo redirigirá automáticamente a la pantalla `/change-password`, donde deberá definir una contraseña personal de al menos 6 caracteres que incluya mayúsculas, minúsculas y números antes de acceder al panel general.

---

### 3.3. Registro Público y Validación de Profesionales
Los docentes y terapeutas que deseen incorporarse a la institución sin una cuenta previa pueden autoregistrarse:
1. En la pantalla `/admin-login`, haga clic en **`¿Eres profesional? Solicita tu cuenta aquí`** (o acceda directamente a `/register-professional`).
2. Complete sus datos personales, correo, teléfono, especialidad y **número de matrícula habilitante**.
3. Al enviar el formulario, su solicitud pasará al estado **`Pendiente`**.
4. La dirección de la escuela auditará su legajo y matrícula desde el portal directivo. Al ser aprobado, recibirá una notificación por correo con sus credenciales de ingreso habilitadas.

---

### 3.4. Ingreso Adaptativo para Estudiantes con Discapacidad
El acceso para estudiantes está concebido para fomentar la autonomía sin generar fricción cognitiva:

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        PORTAL VISUAL DE INGRESO (/login)                              │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│          ┌───────────────────────────┐       ┌───────────────────────────┐             │
│          │   🎒 SOY UN ALUMNO        │       │   👨‍👩‍👧 SOY FAMILIA         │             │
│          │                           │       │                           │             │
│          │   (Botón Verde Gigante)   │       │   (Botón Violeta)         │             │
│          └───────────────────────────┘       └───────────────────────────┘             │
│                                                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

1. El estudiante o su acompañante ingresa a `/login` y presiona la tarjeta grande verde con ícono de mochila: **`"Soy un alumno"`**.
2. **Selección Visual:** Escribe las primeras letras de su nombre o selecciona su fotografía/avatar distintivo en la cuadrícula de estudiantes.
3. **Autenticación según su Nivel de Autonomía:**
   * **Método 1: PIN Numérico de 4 Dígitos (Mayor Autonomía):**
     * En pantalla se despliega un teclado táctil numérico amplio con botones de alto contraste (del 0 al 9) y botón de borrado.
     * El estudiante pulsa sus 4 números secretos (ej: `1` - `2` - `3` - `4`).
     * Cada pulsación emite un tono suave y animaciones de confirmación. Al marcar el 4to dígito, el portal se abre inmediatamente en `/app`.
   * **Método 2: Modo Asistido (Baja Autonomía o Apoyo Severo):**
     * El alumno selecciona su tarjeta y la pantalla exhibe el mensaje: *"Tu docente autorizará tu ingreso"*.
     * El docente o terapeuta presente en el aula introduce su propia contraseña profesional en la ventana emergente de confirmación y presiona **`Autorizar Ingreso`**.
     * La sesión del estudiante se inicializa de forma inmediata sin exigirle memorizar códigos.

---

### 3.5. Ingreso y Activación de Familias por Invitación
A fin de proteger la privacidad de los menores, no existe autoregistro público de familias:
1. La escuela genera una **Invitación Digital** asignada a un estudiante.
2. El tutor recibe un correo electrónico con un enlace seguro (`/invite/:codigo`) que cuenta con **validez de 7 días**.
3. Al pulsar el enlace, se abre la pantalla de activación con los datos del estudiante ya precargados.
4. El familiar define su contraseña segura y acepta el consentimiento informado de tratamiento de datos.
5. Para sus ingresos habituales, el familiar accederá a `/login`, presionará la tarjeta violeta **`"Soy familia"`**, e ingresará con su correo y contraseña.

---

### 3.6. Recuperación y Cambio de Contraseñas
* En las pantallas de login tradicionales (`/admin-login` y `/login` pestaña familiar), se encuentra disponible el enlace **`¿Olvidaste tu contraseña?`**.
* Al ingresar el correo registrado, el sistema despacha un token temporal de reseteo con validez de 1 hora hacia la bandeja de entrada del usuario.
* Al hacer clic en el enlace del correo (`/reset-password?token=...`), el usuario ingresa su nueva contraseña con validación de seguridad en tiempo real.

---

### 3.7. Catálogo Maestro de Cuentas de Demostración y Prueba

Para instancias de capacitación, auditorías académicas o demostraciones guiadas, la base de datos incluye las siguientes cuentas preconfiguradas:

#### 🏢 Equipo Directivo (Administración)
| Nombre y Apellido | Correo Electrónico | Contraseña | Portal de Ingreso | Rol Institucional |
|---|---|---|:---:|---|
| Admin Sistema | `admin@test.com` | `Admin123!` | `/admin-login` | Administrador de Sede |

#### 👩‍🏫 Equipo Profesional (Docentes y Terapeutas)
| Especialista | Cargo / Especialidad | Correo Electrónico | Contraseña | Matrícula |
|---|---|---|---|:---:|
| **Pedro Martínez** | Terapeuta Ocupacional (Supervisor de Aula) | `profesional@test.com` | `Prof123!` | PROF-001 |
| **Sofía Gutiérrez** | Psicopedagoga (Co-supervisora) | `profesional2@test.com` | `Password123!` | PROF-003 |
| **Sacha Del Barrio** | Docente de Apoyo a la Inclusión (DAI) | `sacha.delbarrio@test.com` | `Sacha123!` | 31293 |
| **Docente Prueba** | Docente General | `docente@test.com` | `Doc123!` | PROF-002 |

#### 🎒 Estudiantes con Discapacidad
| Estudiante | Método de Acceso | Código PIN | Supervisor Autorizado | Familiar Responsable |
|---|:---:|:---:|---|---|
| **María García** | **PIN Numérico** | `1234` | — | Rosa Sánchez (`familia@test.com`) |
| **Juan López** | **PIN Numérico** | `1234` | — | Miguel Fernández (`tutor@test.com`) |
| **Carlos Rodríguez** | **PIN Numérico** | `5678` | — | Roberto Rodríguez (`carlostu@test.com`) |
| **Tomás Pérez** | **PIN Numérico** | `1234` | — | Carlos Pérez (`carlostutor@test.com`) |
| **Sofía Rodríguez** | **PIN Numérico** | `1234` | — | Ana Rodríguez (`anatutor@test.com`) |
| **Mateo Díaz** | **PIN Numérico** | `1234` | — | Luis Díaz (`luistutor@test.com`) |
| **Valentina Silva** | **PIN Numérico** | `1234` | — | Elena Silva (`elenatutor@test.com`) |
| **Ana Martínez** | **Ingreso Asistido** | *(Sin PIN)* | Pedro Martínez | Patricia Martínez (`anatu@test.com`) |
| **Benjamín Castro** | **Ingreso Asistido** | *(Sin PIN)* | Pedro Martínez | Jorge Castro (`jorgetutor@test.com`) |

#### 👨‍👩‍👧 Familias y Tutores Legales
| Tutor / Representante | Parentesco | Correo Electrónico | Contraseña | Estudiante a Cargo |
|---|---|---|---|---|
| **Rosa Sánchez** | Madre | `familia@test.com` | `Familia123!` | María García |
| **Miguel Fernández** | Tutor Legal | `tutor@test.com` | `Tutor123!` | Juan López |
| **Patricia Martínez** | Madre | `anatu@test.com` | `Tutor123!` | Ana Martínez |
| **Roberto Rodríguez** | Padre | `carlostu@test.com` | `Tutor123!` | Carlos Rodríguez |
| **Carlos Pérez** | Padre | `carlostutor@test.com` | `Tutor123!` | Tomás Pérez |
| **Ana Rodríguez** | Madre | `anatutor@test.com` | `Tutor123!` | Sofía Rodríguez |
| **Luis Díaz** | Padre | `luistutor@test.com` | `Tutor123!` | Mateo Díaz |
| **Elena Silva** | Madre | `elenatutor@test.com` | `Tutor123!` | Valentina Silva |
| **Jorge Castro** | Tutor Legal | `jorgetutor@test.com` | `Tutor123!` | Benjamín Castro |

---

## 4. MÓDULO TRANSVERSAL: ACCESIBILIDAD UNIVERSAL (PANEL ALT + A)

La accesibilidad en InclusiON no es un complemento opcional, sino el pilar estructural que rige la totalidad de la experiencia de usuario.

```text
       ┌─────────────────────────────────────────────────────────────────┐
       │             PANEL DE ACCESIBILIDAD UNIVERSAL (ALT + A)          │
       ├─────────────────────────────────────────────────────────────────┤
       │  🎨 Perfil Visual: [ Alto Contraste                         ▼ ] │
       │  🌓 Modo de Luz:   ( ) Claro        (•) Oscuro                  │
       │  🔤 Tamaño Letra:  [-] 100% [+]     🔠 Fuente: [ OpenDyslexic ▼]│
       │  📏 Espaciado:     ( ) Normal       (•) Ampliado                │
       │  📖 Guía de Foco:  [✓ Activada ]                                │
       │  🎯 Cursor Grande: [✓ Con Reborde Luminoso ]                    │
       │  🔇 Reducción Sensorial (Cero Animaciones / Confeti): [✓ Activo]│
       └─────────────────────────────────────────────────────────────────┘
```

### 4.1. Cómo Abrir el Panel de Accesibilidad
El panel de accesibilidad está disponible en **todas las pantallas del sistema**:
1. **Atajo de Teclado Rápido:** Presione la combinación de teclas **`Alt + A`**. El foco se posicionará inmediatamente en el primer control del panel.
2. **Botón Flotante:** Haga clic en el botón circular flotante identificado con el isotipo de la figura humana accesible en el margen superior o inferior de la pantalla.
3. **Manejo Accesible de Teclado (Focus Trap):** Mientras el panel permanece abierto, presionar la tecla `Tab` circula exclusivamente entre sus controles, evitando que el usuario pierda el foco en el fondo. Al pulsar `Esc` o presionar el botón cerrar, el foco retorna con exactitud al elemento interactivo donde se encontraba anteriormente.

---

### 4.2. Los 7 Perfiles Visuales Adaptativos
El motor de estilos reconfigura instantáneamente las variables CSS de toda la plataforma:
1. **Estándar:** Paleta armónica moderna con relaciones de contraste conformes a WCAG 2.2 Nivel AA.
2. **Alto Contraste:** Fondo negro puro con textos amarillos o blancos intensos y bordes reforzados. Relación de contraste superior a 7:1 (Nivel AAA), óptimo para baja visión severa o fotofobia.
3. **Dislexia (OpenDyslexic):** Sustituye la tipografía del sistema por una fuente especializada con centros de gravedad engrosados en la base de los caracteres para evitar la rotación visual involuntaria (como confundir la `p` con la `q` o la `b` con la `d`).
4. **Baja Visión:** Maximiza las áreas de toque de los botones, amplía los íconos un 40% y simplifica la densidad de información visual.
5. **Deuteranopía:** Corrige la gama visual para usuarios con insensibilidad al verde; adapta semáforos e indicadores de acierto/error a escalas azuladas y ocres.
6. **Protanopía:** Adapta la interfaz para insensibilidad al rojo, reemplazando tonos de error por contrastes magenta y patrones geométricos distintivos.
7. **Tritanopía:** Ajuste especializado para insensibilidad a tonos azules y amarillos.

---

### 4.3. Herramientas de Apoyo Cognitivo y Motor
* **Regla / Guía de Lectura:** Proyecta una franja horizontal sombreada que sigue los movimientos del cursor, permitiendo que personas con TDAH o dificultades oculomotoras no pierdan la línea de texto.
* **Cursor de Alto Impacto:** Incrementa el puntero del mouse al doble de su tamaño e introduce un halo brillante contrastante.
* **Reducción Sensorial de Movimiento:** Neutraliza de inmediato transiciones CSS, confeti festivo y parpadeos, garantizando la seguridad de estudiantes con epilepsia fotosensible o hipersensibilidad al estímulo visual.

---

## 5. EXPLICACIÓN DE CADA FUNCIONALIDAD POR ROL

### 5.1. Módulo I: Portal del Administrador Institucional (`/admin`)

El Administrador Institucional es el responsable máximo del establecimiento o centro de integración. Su entorno centraliza la gobernanza operativa, la supervisión pedagógica y el aval documental formal.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                          PORTAL DEL ADMINISTRADOR INSTITUCIONAL                        │
├──────────────┬─────────────────────────────────────────────────────────────────────────┤
│ MENÚ         │ TABLERO DE CONTROL Y AUDITORÍA INSTITUCIONAL                            │
│ • Dashboard  │ ┌───────────────┐  ┌───────────────┐  ┌───────────────┐ ┌─────────────┐ │
│ • Profesionales│ 4 Informes    │  │ 92% Lectura   │  │ 3 Alertas     │ │ 25 Alumnos  │ │
│ • Alumnos    │ │ Pendientes ⚠️ │  │ Familiar ✅   │  │ Frustración 🔔│ │ Activos 🎒  │ │
│ • Familias   │ └───────────────┘  └───────────────┘  └───────────────┘ └─────────────┘ │
│ • Reportes   │                                                                         │
│ • Catálogos  │ [ Gráfico de Torta: Auditoría de Estados de Informes ]                  │
│ • Invitaciones [ Podio de Productividad Docente: Pedro M. (1°) - Sofía G. (2°) ]       │
│ • Usuarios   │ [ Histograma de Superación en Mi Camino por Niveles ]                   │
└──────────────┴─────────────────────────────────────────────────────────────────────────┘
```

#### A. Tablero de Comando Institucional (KPIs y Analítica)
* **Scorecards de Decisión Directiva:**
  * **Informes Pendientes de Revisión (Naranja):** Indica cuántos reportes pedagógicos aguardan su aval. Al hacer clic sobre esta tarjeta (`.clickable-kpi-card`), el sistema navega automáticamente a la lista filtrada de informes pendientes.
  * **Índice de Lectura Familiar (Verde):** Porcentaje de boletines aprobados que ya han sido visualizados en sus hogares por los tutores.
  * **Tasa de Rechazo:** Métrica porcentual de informes devueltos a los docentes para corrección pedagógica.
* **Visualizaciones Gráficas de Auditoría:**
  * **Torta de Estados:** Monitorea la proporción exacta entre borradores, informes enviados, aprobados y observados.
  * **Podio de Productividad Docente:** Ranking que pondera informes redactados, actividades adaptadas y seguimiento de alumnos.

#### B. Gestión de la Nómina Profesional (`/admin/professionals`)
* **Validación de Solicitudes Docentes (HU-IN-150):** Los profesionales preinscriptos se exhiben con la etiqueta `Pendiente`. El directivo evalúa su matrícula profesional y especialidad, resolviendo su aprobación (con despacho de credenciales) o su rechazo con motivo obligatorio.
* **Alta Directa:** Posibilidad de crear docentes asignando su cargo institucional y sede escolar de forma inmediata.
* **Regla Hard Stop de Desvinculación Segura (CB-01):** El sistema **bloquea e impide terminantemente** dar de baja a cualquier docente que conserve alumnos a cargo o informes sin cerrar, exigiendo su previa reasignación para proteger la continuidad pedagógica del menor.

#### C. Padrón General de Alumnos y Métodos de Acceso (`/admin/persons`)
* Registro de legajos escolares, fecha de nacimiento, diagnóstico de base y nivel de autonomía DUA.
* En la pestaña **`Acceso y Seguridad`**, el administrador puede redefinir el método de ingreso del estudiante (asignar/modificar el PIN numérico de 4 dígitos o cambiar a Ingreso Asistido con supervisor) en caso de olvido o cambio de pauta terapéutica.

#### D. Padrón Familiar y Pase de Invitaciones (`/admin/family`, `/admin/invitations`)
* Vinculación formal de madres, padres o tutores con sus respectivos estudiantes.
* Emisión y reenvío de enlaces de invitación digital con token único de 7 días.

#### E. Auditoría, Aprobación y Rechazo de Informes Escolares (`/admin/reports`)
* **Lectura Integral Obligatoria:** En la tabla de informes pendientes, la única acción disponible es el ícono de la lupa **`Ver`** (🔍). No existen botones de aprobación a ciegas; el directivo debe ingresar al detalle del informe para revisarlo en su totalidad.
* **Aprobación Institucional:** Al presionar **`Aprobar Informe`**, el servidor aplica la firma digital, genera el archivo PDF inmutable bajo formato A4 con membrete oficial y despacha un evento en tiempo real (SignalR) al portal de la familia avisando que el informe está disponible.
* **Rechazo Pedagógico:** Al presionar **`Rechazar con Observaciones`**, se despliega un cuadro modal obligatorio donde el directivo fundamenta qué aspectos corregir; el informe regresa a la bandeja del docente en estado observado.

#### F. Mantenimiento de Catálogos Maestros y Usuarios (`/admin/catalogs`, `/admin/users`)
* Configuración de tablas maestras: Tipos de Discapacidad, Niveles de Autonomía, Áreas de Habilidad, Categorías de Actividades y Cargos Institucionales.
* Gestión transversal de cuentas de usuario: Desactivación, reactivación y blanqueo de contraseñas.

---

### 5.2. Módulo II: Portal del Profesional Docente y Terapeuta (`/pro`)

El portal profesional es el centro de intervención pedagógica. Desde aquí los docentes integradores, fonoaudiólogos y terapeutas planifican, adaptan y evalúan el proceso de aprendizaje.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                              PORTAL DEL PROFESIONAL                                    │
├──────────────┬─────────────────────────────────────────────────────────────────────────┤
│ MENÚ         │ PANEL "MI AULA" (Pedro Martínez - Terapeuta Ocupacional)                │
│ • Dashboard  │ Selector de Aula: [ Aula Pedro Integradora                           ▼ ]│
│ • Mi Aula    │                                                                         │
│ • Actividades│ ┌───────────────────────────┐         ┌───────────────────────────┐     │
│ • Roadmap    │ │ María García              │         │ Juan López                │     │
│ • Reportes   │ │ Nivel en Camino: 4 (60%)  │         │ Nivel en Camino: 2 (En cur│     │
│ • Evaluaciones │ Estado: Al día ✅         │         │ Estado: En Práctica 🔄    │     │
│ • Calendario │ └───────────────────────────┘         └───────────────────────────┘     │
│ • Mensajes   │                                                                         │
│              │ 🔔 ALERTA EN VIVO: Ana Martínez (4 intentos en "Emparejar Formas")      │
└──────────────┴─────────────────────────────────────────────────────────────────────────┘
```

#### A. Panel "Mi Aula" (`/pro/persons`)
* Selector dinámico reactivo de aulas: Permite filtrar entre grupos asignados o ver la totalidad de alumnos.
* Tarjetas de seguimiento individual: Exclaman el avatar, el estado de actividad reciente y el nivel alcanzado en "Mi Camino".
* Al hacer clic en un estudiante se accede a su **Ficha Integral** con pestañas de *Perfil Funcional*, *Diagnósticos CIF*, *Asignaciones*, *Roadmap* y *Reportes*.

#### B. Asistente de Registro Unificado (Wizard de 3 Pasos)
Permite al docente enrolar un nuevo caso en una única transacción atómica:
* **Paso 1 (Alumno):** Datos personales, DNI, fecha de nacimiento y diagnóstico de base.
* **Paso 2 (Familia):** Nombre del tutor, parentesco, teléfono y correo electrónico.
* **Paso 3 (Aula y Acceso):** Asignación de aula y selección de método de login (PIN de 4 dígitos o Asistido). Al finalizar, el sistema crea las entidades, establece sus vínculos relacionales e inicializa el Roadmap de 10 estaciones de forma automática.

#### C. Banco de Actividades Pedagógicas y ARASAAC (`/pro/activities`)
InclusiON cuenta con 5 plantillas interactivas diseñadas bajo pautas DUA:
1. **Selección de Figuras (`SELECT_FIGURE`):** El alumno debe seleccionar el pictograma o figura correcta ante una consigna entre distractores.
2. **Suma Visual (`VISUAL_SUM`):** Iniciación al cálculo matemático mediante elementos concretos contables.
3. **Emparejar Imagen-Palabra (`MATCH_IMAGE_WORD`):** Vinculación de un concepto gráfico con su texto mediante pulsaciones simples.
4. **Ordenar Secuencias (`ORDER_SEQUENCE`):** Ordenamiento lógico o cronológico de secuencias cotidianas (ej: higiene, vestimenta) mediante flechas táctiles.
5. **Completar Letras (`COMPLETE_LETTER`):** Fortalecimiento de la lectoescritura completando letras faltantes guiadas por un pictograma.
* **Buscador de Pictogramas ARASAAC:** Integrado directamente en los formularios de creación, permite incorporar imágenes normalizadas de libre uso internacional con un clic.

#### D. Búsqueda Semántica de Actividades por IA Local (ONNX)
* El buscador del banco de actividades ejecuta un modelo local de embeddings vectoriales.
* Si el docente tipea: *"ejercicios para aprender a lavarse los dientes"*, el motor vectorial recuperará actividades vinculadas a higiene bucal aunque en el título no figure la palabra exacta buscada, optimizando los tiempos de planificación.

#### E. Gestión de "Mi Camino" (Roadmap Gamificado de 10 Estaciones)
* El profesional puede supervisar la hoja de ruta de cada estudiante, alterar el orden de las actividades, ajustar el umbral de superación (fijado por defecto en **60%**) o ejecutar un **desbloqueo manual extraordinario** cuando considere que el alumno ha adquirido la destreza por mediación presencial.

#### F. Alerta Pedagógica Temprana en Vivo (CB-07 / HU-21)
* **Principio Fundamental de No Frustración:** Si un alumno comete errores reiterados, la plataforma **nunca bloquea la pantalla, ni expulsa al alumno, ni emite sonidos estridentes**.
* **Alerta Silenciosa al Docente:** Al acumular **4 intentos fallidos consecutivos** ($\le 59\%$), el servidor despacha un aviso instantáneo (SignalR) al portal del profesional:
  > 🔔 *"Alerta de Apoyo Pedagógico: Ana Martínez ha alcanzado 4 intentos en la actividad 'Emparejar Formas'. La actividad continúa abierta pero se sugiere acompañamiento presencial."*
* Esto posibilita que el docente se acerque físicamente a la mesa del alumno para brindarle andamiaje pedagógico de manera discreta y respetuosa.

#### G. Redacción y Envío de Informes de Progreso (`/pro/reports`)
* El docente genera borradores de informe consolidando el porcentaje de logros, evolución por área y sugerencias para el hogar.
* Al pulsar **`Enviar a Revisión` (`Submit`)**, el informe pasa a la bandeja del Administrador para su firma final.

#### H. Agenda Escolar y Mensajería Directa (`/pro/calendar`, `/pro/messages`)
* Programación de turnos terapéuticos y clases de apoyo con prevención de fechas pasadas.
* Mensajería formal interna tipo WhatsApp Web que centraliza las consultas familiares en un canal profesional seguro.

---

### 5.3. Módulo III: Portal del Estudiante con Discapacidad (`/app`)

El entorno del estudiante está concebido con principios de máxima claridad cognitiva, botones amplios, refuerzo positivo visual/sonoro y ausencia total de sobrecarga distractora.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        PORTAL DEL ESTUDIANTE CON DISCAPACIDAD                          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  👋 ¡HOLA, MARÍA!                                                🎨 [ Menú de Apoyos ] │
│                                                                                        │
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │                                MI CAMINO (ROADMAP)                               │  │
│  │                                                                                  │  │
│  │   [ Estación 1 ] ───► [ Estación 2 ] ───► [ Estación 3 ] ───► 🔒 [ Estación 4 ] │  │
│  │        ✅                  ✅                  ▶                      Bloqueada  │  │
│  │    Completada          Completada          En Curso                              │  │
│  │      (100%)              (80%)             (Jugar)                               │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                        │
│  [ 🗣️ Comunicación (SAAC) ]                              [ 🏆 Mis Logros ]             │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### A. Recorrido por "Mi Camino" (Sendero de 10 Estaciones)
* El estudiante visualiza su trayectoria formativa en un mapa interactivo semejante a un sendero de aventuras:
  * **Estaciones con Tilde Verde (`✓ Completada`):** Desafíos ya superados que pueden volverse a jugar libremente para afianzar conocimientos.
  * **Estación con Flecha Azul (`▶ En Curso`):** Desafío actual listo para jugar.
  * **Estaciones con Candado (`🔒 Bloqueada`):** Niveles sucesivos que se habilitan progresivamente.

#### B. Dinámica de Juego y Cuidado Emocional
Al resolver los ejercicios en los jugadores interactivos:
* 🌟 **Si alcanza 60% o más (Nivel Aprobado):**
  * La pantalla despliega una medalla brillante, confeti festivo y aplausos suaves.
  * Al presionar el botón grande verde **`✓ Finalizar`**, el sistema registra el éxito y **desbloquea de forma automática la siguiente estación**.
* 💪 **Si obtiene menos de 60% (Nivel en Proceso):**
  * La pantalla brinda un mensaje de aliento afectuoso (ej: *"¡Buen intento! Cada vez lo haces mejor"*).
  * Dispone de dos opciones claras:
    1. **`🔄 Intentar de nuevo`**: Reinicia el ejercicio limpio en la misma pantalla sin salir.
    2. **`✓ Finalizar`**: Regresa a Mi Camino, conservando la estación activa para continuar en otro momento.
* ❤️ **Cuidado Emocional al 4to Intento:**
  * Si se alcanzan 4 intentos fallidos, el sistema exhibe con calidez: *"Tu docente se acercará para acompañarte. Puedes seguir practicando cuando quieras"*. La actividad **nunca se bloquea**.

#### C. Tablero de Comunicación Aumentativa y Alternativa (SAAC / `/app/talk`)
* Presionando el botón **`Comunicación`**, se despliega un tablero de pictogramas con necesidades esenciales organizadas por categorías (Higiene, Alimentación, Emociones, Pedidos de Ayuda).
* Al pulsar un pictograma (ej: *"Quiero agua"* o *"Necesito ayuda"*), el sintetizador de voz (TTS) del dispositivo pronuncia la frase con claridad, permitiendo la expresión inmediata del alumno en el aula o el hogar.

---

### 5.4. Módulo IV: Portal de la Familia y Tutores Legales (`/family`)

El portal de familias otorga a madres, padres y tutores una ventana transparente y en tiempo real hacia la trayectoria educativa de sus hijos, garantizando el derecho a la información consagrado en la Ley de Educación Nacional.

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                              PORTAL DE LA FAMILIA                                      │
├──────────────┬─────────────────────────────────────────────────────────────────────────┤
│ MENÚ         │ SEGUIMIENTO PEDAGÓGICO DE MARÍA GARCÍA                                  │
│ • Inicio     │ ┌─────────────────────────┐         ┌─────────────────────────┐         │
│ • Progreso   │ │ Nivel en Mi Camino      │         │ Informes Oficiales      │         │
│ • Reportes   │ │ Estación 4 de 10 ⭐     │         │ 1 Nuevo Informe Aprobado│         │
│ • Calendario │ │ Estado: Al día          │         │ [ 📄 Descargar PDF A4 ] │         │
│ • Mensajes   │ └─────────────────────────┘         └─────────────────────────┘         │
│              │                                                                         │
│              │ 📅 Próximo Evento: Clase de Apoyo Fonoaudiológico (Jueves 10:00 hs)     │
│              │ 💬 Mensajería: 1 mensaje sin leer del Terapeuta Pedro Martínez          │
└──────────────┴─────────────────────────────────────────────────────────────────────────┘
```

#### A. Tablero de Progreso sin Tecnicismos Clínicos (`/family/progress`)
* Exhibe el avance del alumno en "Mi Camino", ilustrando cuántos niveles ha completado y qué habilidades específicas (motricidad, lenguaje, cálculo) se están ejercitando.
* Recomienda actividades concretas para realizar en familia como refuerzo lúdico del hogar.

#### B. Consulta y Descarga del Boletín Oficial en PDF A4 (`/family/reports`)
* Listado de informes pedagógicos emitidos por los docentes que ya cuentan con la aprobación formal directiva.
* **Descarga Inmediata de Documento Oficial:** Al presionar **`Descargar PDF`**, el sistema genera un archivo vectorial A4 con membrete escolar, evolución de objetivos, comentarios docentes y firma institucional. Este documento posee plena validez formal ante Obras Sociales, Juntas Evaluadoras de Discapacidad y legajos médicos.
* **Auditoría de Lectura Automática:** Al momento exacto en que el familiar abre o descarga el informe, el sistema actualiza de forma automática el registro de auditoría (`IsReadByFamily = true`), reflejando de inmediato en el tablero directivo que la familia tomó conocimiento del informe.

#### C. Calendario Institucional y Mensajería Directa (`/family/calendar`, `/family/messages`)
* Notificación anticipada de jornadas escolares, reuniones de seguimiento y turnos terapéuticos.
* Canal de comunicación directo, respetuoso y profesional con el equipo docente, resguardando la privacidad y evitando el empleo de redes sociales personales.

---

## 6. SIMULACIONES PASO A PASO DE FLUJOS REALES DE NEGOCIO

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│               CICLO INSTITUCIONAL INTEGRADO DE PUNTA A PUNTA                           │
│                                                                                        │
│  [1. ADMIN] ────► Homologa al docente y verifica legajos escolares                     │
│       │                                                                                │
│  [2. DOCENTE] ──► Crea el aula, enrola alumnos/familias y adapta actividades DUA       │
│       │                                                                                │
│  [3. ALUMNO] ───► Ingresa con PIN/Asistido, recorre Mi Camino y juega en tablet        │
│       │                                                                                │
│  [2. DOCENTE] ──► Monitorea alerta temprana en vivo, redacta y envía informe           │
│       │                                                                                │
│  [1. ADMIN] ────► Audita en detalle y aprueba informe (genera PDF A4 firmado)          │
│       │                                                                                │
│  [4. FAMILIA] ──► Consulta avance, descarga PDF oficial y confirma lectura             │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 6.1. Simulación 1: Administrador (Admisión Docente, Configuración y Aval Directivo)

* **Actor Simulado:** Admin Sistema (Directora Institucional).
* **Credenciales:** `admin@test.com` / `Admin123!`
* **URL de Entrada:** `http://localhost:4200/admin-login`

#### 🔹 Paso 1: Ingreso y Evaluación del Estado Institucional
1. Ingrese a `/admin-login`, digite `admin@test.com`, contraseña `Admin123!` y presione **`Iniciar Sesión`**.
2. Aterriza en el **Tablero de Control** (`/admin/dashboard`).
3. **Diagnóstico visual:** Observa la tarjeta naranja **`Pendientes de Revisión`** con 4 informes aguardando firma y el índice de lectura familiar en verde.

#### 🔹 Paso 2: Auditoría Integral y Aprobación de Informes Pedagógicos (`/admin/reports`)
1. Haga clic directamente sobre la tarjeta **`Pendientes de Revisión`**.
2. El sistema lo traslada a la grilla de informes filtrada automáticamente por el estado `Submitted` (Pendiente).
3. Localice el informe del estudiante **María García**, confeccionado por el terapeuta **Pedro Martínez**.
4. Observe que en la columna de acciones **no existen botones de aprobación precipitada**, sino únicamente el ícono de la lupa **`Ver`** (🔍).
5. Haga clic en **`Ver`** (🔍) para abrir la vista de lectura integral.
6. Revise los gráficos de aciertos, evolución de objetivos DUA y sugerencias terapéuticas.
7. En la barra superior, presione el botón verde **`✓ Aprobar Informe`**.
8. **Resultado:** El servidor aplica la firma digital, genera el archivo PDF A4 inmutable y despacha una notificación en vivo a la madre de la alumna. El contador del tablero descuenta una unidad.

#### 🔹 Paso 3: Aprobación de Nómina Docente (`/admin/professionals`)
1. En el menú lateral, seleccione **`Profesionales`**.
2. Encuentre al docente con estado **`Pendiente`** que solicitó registro público.
3. Haga clic en **`Validar / Evaluar`** y constate su número de matrícula profesional.
4. Presione **`Aprobar Solicitud`**. El sistema activa la cuenta y le despacha un correo de bienvenida habilitante.

---

### 6.2. Simulación 2: Profesional (Mi Aula, Wizard 3 Pasos, ARASAAC, IA y Alerta Temprana)

* **Actor Simulado:** Pedro Martínez (Terapeuta Ocupacional).
* **Credenciales:** `profesional@test.com` / `Prof123!`
* **URL de Entrada:** `http://localhost:4200/admin-login`

#### 🔹 Paso 1: Ingreso a "Mi Aula" (`/pro/persons`)
1. Ingrese con sus credenciales docentes y pulse en **`Mi Aula`** en el menú lateral.
2. En el **Selector de Aula**, elija *"Aula Pedro Integradora"*. La grilla exhibe las fichas de sus alumnos (*María García*, *Juan López*, *Ana Martínez*).

#### 🔹 Paso 2: Alta con el Asistente de Registro Unificado (Wizard)
1. Presione el botón verde superior **`+ Registro Unificado`** (`/pro/persons/new`).
2. **Paso 1 (Alumno):** Nombre: *Mateo*, Apellido: *Díaz*, DNI: *33333333*, Diagnóstico: *Trastorno del Espectro Autista (TEA)*. Presione `Siguiente`.
3. **Paso 2 (Familia):** Nombre Tutor: *Luis Díaz*, Parentesco: *Padre*, Correo: `luistutor@test.com`, Teléfono: `12345678`. Presione `Siguiente`.
4. **Paso 3 (Aula y Acceso):** Aula: *"Aula Pedro Integradora"*, Método de Login: **`PIN Numérico`** (asigne `1234`). Presione **`Finalizar y Crear Registro`**.
5. **Resultado:** En una sola operación atómica se crea la ficha escolar, la cuenta del tutor, el vínculo de tutela y el Roadmap de 10 estaciones con el Nivel 1 desbloqueado.

#### 🔹 Paso 3: Diseño de Actividad con ARASAAC (`/pro/activities/new`)
1. Ingrese a **`Actividades`** y presione **`+ Nueva Actividad`**.
2. Título: *"Reconociendo Frutas Saludables"*. Área: *Comunicación y Lenguaje*. Plantilla: *Selección de Figuras (`SELECT_FIGURE`)*.
3. En el buscador integrado de **ARASAAC**, escriba *"Manzana"*. Seleccione el pictograma oficial de la manzana roja como respuesta correcta.
4. Añada distractores buscando *"Pelota"* y *"Auto"*. Presione **`Guardar Actividad`**.

#### 🔹 Paso 4: Búsqueda Semántica con IA Local (ONNX)
1. Regrese al catálogo de actividades (`/pro/activities`).
2. En la barra superior de búsqueda semántica escriba en lenguaje natural:
   > *"ejercicios para identificar alimentos sanos"*
3. Presione Enter. El motor de embeddings vectoriales ONNX local recuperará la actividad recién creada en los primeros puestos por afinidad semántica.

#### 🔹 Paso 5: Alerta Pedagógica Silenciosa en Vivo (CB-07 / SignalR)
1. Suponga que la alumna **Ana Martínez** está resolviendo una actividad en el aula y comete 4 intentos fallidos consecutivos ($\le 59\%$).
2. **Respuesta del sistema:** En la barra superior del portal docente suena un timbre suave y surge la notificación emergente:
   > 🔔 **Alerta de Apoyo Pedagógico:** *"Ana Martínez ha alcanzado 4 intentos en 'Emparejar Formas'. La actividad continúa abierta pero se sugiere acompañamiento presencial."*
3. El docente se acerca a la mesa de Ana para asistirla sin exponerla ni interrumpir al resto de los estudiantes.

---

### 6.3. Simulación 3: Alumno (Ingreso Adaptativo, Recorrido Lúdico y SAAC)

* **Actor Simulado:** María García (Alumna - Acceso por PIN).
* **URL de Entrada:** `http://localhost:4200/login`

#### 🔹 Recorrido de Juego y Superación en la Tablet
1. María accede a `/login` en su tablet y pulsa la tarjeta grande verde: **`"Soy un alumno"`**.
2. Escribe *"María"* y toca su tarjeta identificatoria con fotografía.
3. Se abre el teclado táctil accesible. María pulsa con su dedo: **`1`** ➔ **`2`** ➔ **`3`** ➔ **`4`**.
4. La sesión se abre automáticamente en su portal: `/app`.
5. Presiona el botón grande **`"Mi Camino"`** (`/app/roadmap`).
6. Observa las Estaciones 1, 2 y 3 con tilde verde (`✓ Completada`) y pulsa sobre la **Estación 4** (`▶ En Curso`).
7. **Resolución:** Escucha la consigna auditiva: *"Toca el dibujo que representa la MANZANA"*. María toca el pictograma de la manzana. El borde se ilumina de verde con aplausos festivos.
8. **Resultado Aprobado (100%):** Aparece la medalla dorada. María pulsa **`✓ Finalizar`**. El sistema regresa a Mi Camino: la Estación 4 luce su tilde verde y **la Estación 5 se desbloquea automáticamente** eliminando el candado.
9. **Uso del SAAC:** En el recreo, María pulsa **`Comunicación`** (`/app/talk`), toca el pictograma **`"Quiero agua"`** y el sintetizador pronuncia la frase en voz alta para solicitar agua a su docente.

---

### 6.4. Simulación 4: Familia (Activación, Monitoreo, Descarga de PDF y Mensajería)

* **Actor Simulado:** Rosa Sánchez (Madre de María García).
* **Credenciales:** `familia@test.com` / `Familia123!`
* **URL de Entrada:** `http://localhost:4200/login`

#### 🔹 Paso 1: Ingreso al Portal Familiar
1. Rosa ingresa a `/login`, pulsa la tarjeta violeta **`"Soy familia"`**, escribe `familia@test.com`, contraseña `Familia123!` y presiona **`Ingresar`**.
2. Aterriza en su panel de inicio (`/family`).

#### 🔹 Paso 2: Seguimiento del Progreso Escolar (`/family/progress`)
1. Visualiza que su hija avanzó a la **Estación 5 de 10** en "Mi Camino", consolidando logros en comunicación y lógica.
2. Consulta las dinámicas sugeridas para estimular el vocabulario en el hogar.

#### 🔹 Paso 3: Descarga del Informe Oficial en PDF A4 (`/family/reports`)
1. Accede a la pestaña **`Reportes`**.
2. Observa el *"Informe de Progreso Pedagógico - 1° Trimestre"* avalado por la dirección escolar.
3. Presiona el botón azul **`📄 Descargar PDF`**.
4. Obtiene el boletín oficial vectorial A4 con membrete, diagnósticos y firmas institucionales para presentar ante su Obra Social.
5. **Auditoría transparente:** En el instante en que Rosa descarga el archivo, el backend actualiza `IsReadByFamily = true`, notificando a la escuela que el informe fue recibido y leído.

#### 🔹 Paso 4: Mensajería con el Docente (`/family/messages`)
1. Hace clic en **`Mensajes`**, presiona **`+ Nuevo Mensaje`** y selecciona al terapeuta **Pedro Martínez**.
2. Escribe una consulta sobre qué pictogramas de secuencias imprimir para el hogar. Pedro responderá en su horario laboral dentro de la plataforma.

---

## 7. RESOLUCIÓN DE PROBLEMAS FRECUENTES (FAQ / TROUBLESHOOTING)

### ❓ Olvidé mi contraseña de acceso (Directivos, Docentes y Familias)
* En la pantalla de login correspondiente (`/admin-login` o `/login`), haga clic en **`¿Olvidaste tu contraseña?`**.
* Ingrese su correo institucional. Recibirá un enlace temporal para registrar una nueva clave de acceso de forma segura.

### ❓ El estudiante olvidó o confundió su código PIN
* El alumno no debe probar combinaciones al azar para evitar sensaciones de frustración.
* Su docente a cargo o el administrador de la escuela pueden ingresar al legajo del alumno (`/pro/persons` o `/admin/persons`), dirigirse a la pestaña **`Acceso y Seguridad`** y consultar o cambiar su PIN de 4 dígitos en segundos.

### ❓ La interfaz se ve pequeña o con contraste insuficiente en la tablet
* Presione el botón de accesibilidad flotante (o el atajo de teclado **`Alt + A`**).
* Seleccione el perfil **`Alto Contraste`** o aumente la escala de tamaño al **`125%`** o **`150%`**. La pantalla se reajustará al instante sin interrumpir la actividad.

### ❓ No se reproduce el audio de las consignas en la tablet escolar
* Verifique que el volumen del dispositivo esté activado y no se encuentre en modo silencioso.
* Si el navegador web solicita permisos para reproducir sonido o utilizar síntesis de voz la primera vez que se ingresa, presione **`Permitir`**.

### ❓ ¿Qué ocurre si un estudiante falla 4 veces seguidas en una actividad?
* La plataforma **nunca expulsará al alumno, ni bloqueará la pantalla, ni mostrará carteles punitivos**.
* El sistema emitirá un aviso silencioso y discreto en la pantalla del docente para que se acerque a brindar apoyo pedagógico personalizado, permitiendo que el estudiante continúe jugando a su propio ritmo.

### ❓ ¿Cómo obtengo soporte técnico de infraestructura?
* Para solicitudes técnicas, copias de seguridad de la base de datos o mantenimiento de red, contacte al administrador de sistemas institucional a través de la casilla oficial: **`soporte@inclusion.edu.ar`**.

---

## 8. GLOSARIO GENERAL DEL SISTEMA

### 8.1. Actores y Perfiles de Usuario
* **Administrador Institucional:** Máxima autoridad directiva y pedagógica de la sede escolar. Administra nóminas, homologa solicitudes docentes, matricula estudiantes y familias, audita y firma informes oficiales y supervisa KPIs.
* **Profesional (Docente / Terapeuta):** Especialista habilitado (maestro de grado, docente de apoyo a la inclusión, terapeuta ocupacional, psicopedagogo o fonoaudiólogo) que interviene en el aula, crea actividades DUA, adapta senderos y redacta reportes.
* **Persona con Discapacidad (Estudiante / Alumno):** Destinatario central de la plataforma. Resuelve actividades lúdicas adaptadas, interactúa con el tablero SAAC y avanza en su sendero de aprendizaje.
* **Representante Familiar (Tutor Legal):** Madre, padre o tutor a cargo del estudiante. Monitorea el progreso pedagógico, descarga informes oficiales firmados y se comunica con la escuela.

### 8.2. Conceptos Pedagógicos y Terapéuticos
* **DUA (Diseño Universal para el Aprendizaje):** Modelo de enseñanza que proporciona múltiples formas de representación, expresión y compromiso para garantizar el aprendizaje accesible de todo el alumnado.
* **SAAC / CAA (Comunicación Aumentativa y Alternativa):** Conjunto de herramientas, símbolos y sintetizadores de voz que complementan o sustituyen el habla en personas con dificultades de comunicación verbal.
* **ARASAAC:** Catálogo gráfico internacional de libre distribución que provee pictogramas normalizados para la accesibilidad cognitiva y la comunicación aumentativa.
* **CIF / ICF (Clasificación Internacional del Funcionamiento):** Marco de la Organización Mundial de la Salud (OMS) que clasifica la discapacidad considerando factores corporales, actividades, participación y barreras contextuales.
* **Área de Habilidad:** Dominio formativo específico trabajado en la plataforma (Comunicación y Lenguaje, Lógica-Matemática, Motricidad Fina, Conducta y Autonomía).
* **Diagnóstico Funcional:** Registro estructurado realizado por el profesional que documenta las capacidades, apoyos requeridos y estrategias pedagógicas de cada estudiante.
* **Zona de Desarrollo Próximo (ZDP):** Espacio pedagógico entre lo que el estudiante resuelve por sí solo y lo que alcanza mediante el andamiaje presencial de su docente.

### 8.3. Conceptos Operativos de la Plataforma
* **Mi Camino (Roadmap):** Hoja de ruta secuencial gamificada estructurada en 10 estaciones de aprendizaje que el estudiante recorre progresivamente.
* **Umbral de Desbloqueo:** Porcentaje mínimo de aciertos (configurado por defecto en el **60%**) que habilita automáticamente la apertura de la siguiente estación del sendero.
* **Player (Jugador Interactivo):** Componente de software accesible que presenta la dinámica de una actividad (Selección de figuras, Suma visual, Emparejar, Secuencias y Completar letras).
* **Alerta Temprana en Vivo (CB-07):** Notificación automática en tiempo real enviada a la pantalla del docente cuando un alumno acumula 4 intentos fallidos en una misma actividad, sin interrumpir la experiencia del menor.
* **Asistente de Registro Unificado (Wizard):** Flujo de 3 pasos que inscribe simultáneamente al alumno, su familiar responsable y su configuración de acceso en una sola operación atómica.
* **Boletín / Informe Pedagógico:** Documento inmutable emitido por el docente y visado con firma digital por la dirección escolar, exportable en PDF A4 para obras sociales o legajos.
* **Confirmación de Lectura Familiar (`IsReadByFamily`):** Mecanismo de auditoría que registra de forma automática la fecha y hora en que un tutor legal descarga o abre un informe aprobado.

### 8.4. Conceptos Técnicos y de Seguridad
* **Soft-Delete (Borrado Lógico):** Desactivación lógica de registros mediante banderas de estado (`IsActive = false`), impidiendo la pérdida irreversible de historias clínicas o legajos escolares.
* **AES-256-GCM:** Estándar de cifrado simétrico robusto empleado para proteger la confidencialidad de datos sensibles de salud de menores conforme a la Ley 25.326.
* **RBAC (Role-Based Access Control):** Control de acceso basado en roles que restringe la navegación y el consumo de endpoints de acuerdo a las facultades de cada usuario.
* **SignalR:** Tecnología de comunicación bidireccional en tiempo real entre el servidor ASP.NET Core y las interfaces Angular para alertas inmediatas.
* **ONNX Runtime:** Motor local de ejecución de modelos de inteligencia artificial para búsqueda semántica vectorial sin transmitir datos a proveedores externos de nube.
* **QuestPDF:** Biblioteca de renderizado que compila de forma transaccional documentos PDF vectoriales a partir del código C#.
* **WCAG 2.2:** Pautas internacionales del consorcio W3C que certifican la accesibilidad del contenido web para personas con discapacidad visual, auditiva, motriz o cognitiva.

---

```text
========================================================================================
                          FIN DEL MANUAL DE USUARIO OFICIAL
                         InclusiON — Edición Consolidada 2026
           "Construyendo puentes digitales hacia una educación verdaderamente inclusiva"
========================================================================================
```
