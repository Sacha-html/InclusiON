# SPRINT 0 — Arranque y Preparación del Entorno

## Objetivo del Sprint 0
El **Sprint 0** tiene como propósito preparar el entorno de trabajo, seleccionar las herramientas tecnológicas y definir la organización del equipo de desarrollo para iniciar el proyecto **InclusiON** en las mejores condiciones posibles.
Durante esta etapa, el equipo estableció los lineamientos iniciales que permitirán planificar, ejecutar y controlar los sprints posteriores bajo la metodología ágil Scrum.

---

## 1. Elección y Prueba de Herramientas (IN-15, IN-16)

Con el fin de garantizar una comunicación eficiente y una gestión organizada del proyecto, se seleccionaron y probaron las siguientes herramientas:

| Área | Herramienta | Descripción / Uso |
| :--- | :--- | :--- |
| **Comunicación interna** | Microsoft Teams / WhatsApp | Comunicación sincrónica y asincrónica entre los integrantes del equipo. Permite compartir avances, coordinar reuniones y resolver dudas. |
| **Gestión de tareas y versiones** | GitHub / Jira | Repositorio central del código, control de versiones mediante ramas y seguimiento del backlog de tareas. Facilita la colaboración y trazabilidad. |
| **Entorno de desarrollo** | Visual Studio Code / Visual Studio | IDEs utilizados según el módulo (Frontend en Angular/TypeScript y Backend en C# .NET respectivamente). |
| **Diseño web y Prototipado** | Figma / HTML5 / SCSS / Bootstrap 5 / CoreUI | Diseño de la interfaz visual accesible, paletas de colores y experiencia de usuario inclusiva. |
| **Programación Frontend** | Angular 17+ (TypeScript) | Desarrollo de la SPA cliente: componentes reutilizables, reactividad con Signals/RxJS y perfiles visuales de accesibilidad. |
| **Programación Backend** | C# (.NET 8/10 / ASP.NET Web API) | Implementación de controladores REST, arquitectura limpia (Clean Architecture / CQRS con MediatR) y lógica de negocio. |
| **Base de Datos** | SQL Server / PostgreSQL (Entity Framework Core) | Almacenamiento estructurado de información de usuarios, roles, diagnósticos, actividades, sesiones analíticas y mensajería. |
| **Gestión de documentación y Análisis** | Markdown / Word / Excel / Jira | Elaboración de especificaciones funcionales, diagramas de procesos, historias de usuario, actas de ceremonias e informes. |

---

## 2. Revisión de Roles del Equipo (IN-14)

El equipo de trabajo adoptó la estructura recomendada por el marco Scrum con roles específicos de análisis y desarrollo:

| Rol | Nombre | Responsabilidades |
| :--- | :--- | :--- |
| **Product Owner (PO)** | Ferreyra Candelaria<br>Vettorazzi Catalina | Define los requisitos funcionales macro y prioriza el Product Backlog. Supervisa que el producto responda a los objetivos de inclusión educativa. |
| **Analista Funcional / Scrum Master** | Decali Mariano | Relevamiento y análisis de requerimientos, modelado de procesos de negocio, redacción y refinamiento de historias de usuario, criterios de aceptación y facilitación de ceremonias Scrum. |
| **Equipo de Desarrollo (Dev Team)** | Cochis German<br>Del Barrio Sacha | Implementación técnica integral: arquitectura backend (.NET), frontend interactivo y accesible (Angular), bases de datos, APIs REST, pruebas unitarias y documentación técnica. |

---

## 3. Elección de la Plataforma Tecnológica (IN-19, IN-20)

El sistema **InclusiON** está concebido bajo una arquitectura cliente-servidor distribuida, diseñada para garantizar accesibilidad universal, alta disponibilidad, seguridad y aislamiento de datos:

* **Frontend:** Single Page Application (SPA) en **Angular (TypeScript)** con soporte modular, maquetación adaptativa, consumo reactivo de servicios HTTP y soporte para sintetizador de voz (Web Speech API) y Web Audio API.
* **Backend:** **ASP.NET Core Web API en C#**, implementado con patrones de diseño empresariales (CQRS, MediatR, FluentValidation, Repository Pattern) para centralizar la lógica de negocio y seguridad.
* **Base de Datos y Persistencia:** **Entity Framework Core** con migraciones automatizadas, aislamiento multi-tenant por institución (`InstitutionAccessFilter`) y encriptación de datos sensibles.
* **Seguridad y Autenticación:** Tokens JWT (JSON Web Tokens) con políticas de autorización basadas en roles (RBAC) y permisos por módulo, soportando múltiples métodos de login (Estándar, PIN de 4 dígitos, Asistido y Familiar).
* **Comunicación en Tiempo Real:** **SignalR Core** para mensajería instantánea interna y notificaciones push.

---

## 4. Definición de Ceremonias Scrum (IN-18)

A partir del Sprint 1, el equipo acordó el ciclo formal de ceremonias Scrum para asegurar entregas incrementales de valor y mejora continua:

1. **Planificación del Sprint (Sprint Planning):** Definición de objetivos, estimación y selección de historias de usuario del Product Backlog analizadas previamente.
2. **Reuniones Diarias (Daily Scrum):** Sincronización breve para responder a las 3 preguntas clave: qué se hizo, qué se hará hoy y qué impedimentos existen.
3. **Revisión del Sprint (Sprint Review):** Demostración del incremento de software funcional al Product Owner y validación funcional contra criterios de aceptación.
4. **Retrospectiva del Sprint (Sprint Retrospective):** Análisis interno del equipo sobre aciertos, fricciones operativas y definición de compromisos de mejora.
5. **Refinamiento del Backlog (Backlog Refinement):** Relevamiento de procesos, desglose continuo, redacción técnica y funcional de historias de usuario (HU) liderada por el analista.

---

## 5. Tareas y Trazabilidad Jira (IN-2)

| Código | Tarea / Historia | Responsable(s) | Estado |
| :--- | :--- | :--- | :---: |
| **IN-14** | Definición de roles del equipo y asignación funcional | Decali Mariano | ✅ Completada |
| **IN-15** | Elección y prueba de herramientas de desarrollo y gestión | Sacha Del Barrio / Decali Mariano | ✅ Completada |
| **IN-16** | Creación de repositorios GitHub (`InclusiON.Server`, `InclusiON.Client`, `InclusiON.Documents`) | Sacha Del Barrio | ✅ Completada |
| **IN-17** | Elaboración, análisis funcional y estimación del Product Backlog inicial | Decali Mariano / German Cochis | ✅ Completada |
| **IN-18** | Definición de ceremonias Scrum, DoR y DoD | Decali Mariano | ✅ Completada |
| **IN-19** | Selección y validación de la plataforma tecnológica base | Sacha Del Barrio | ✅ Completada |
| **IN-20** | Diseño del modelo de datos base inicial y análisis de entidad-relación | German Cochis / Decali Mariano | ✅ Completada |

---

## 6. Resultados del Sprint 0

Durante el **Sprint 0** se alcanzaron el 100% de los objetivos planteados:
* ✅ Conformación del equipo con roles de Análisis Funcional, Scrum Master y Desarrollo claramente definidos.
* ✅ Selección, instalación y validación del stack tecnológico integral (.NET + Angular + EF Core + SQL).
* ✅ Creación y configuración de repositorios Git centralizados con branches protegidas y estándares de commit.
* ✅ Elaboración del Product Backlog inicial organizado en 12 épicas de negocio con análisis de procesos.
* ✅ Acuerdos de equipo sobre ceremonias ágiles, Definition of Ready (DoR) y Definition of Done (DoD).