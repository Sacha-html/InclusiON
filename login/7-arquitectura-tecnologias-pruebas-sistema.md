# 📘 Contexto Maestro: Arquitectura, Ambiente Tecnológico, Pruebas del Sistema y Reglas de Negocio — InclusiON

**Marco Académico:** Práctica Profesionalizante II / III — Institución Cervantes — Analista de Sistemas  
**Ubicación:** `d:\git\login\7-arquitectura-tecnologias-pruebas-sistema.md`  
**Fecha de consolidación:** Septiembre 2026  
**Proyecto:** InclusiON — Plataforma de Gestión y Aprendizaje para la Inclusión Educativa  

---

## 📑 Índice de Contenidos

1. [Equipo de Proyecto y Roles](#1-equipo-de-proyecto-y-roles)
2. [Arquitectura del Sistema: Diagrama de Despliegue](#2-arquitectura-del-sistema-diagrama-de-despliegue)
3. [Ambiente de Implementación (Stack Tecnológico y Herramientas)](#3-ambiente-de-implementación-stack-tecnológico-y-herramientas)
4. [Dashboards Analíticos y Componentes de Visualización (Sprints 6 y 7)](#4-dashboards-analíticos-y-componentes-de-visualización-sprints-6-y-7)
5. [Prueba del Sistema: Jerarquía, Casos y Métricas de QA](#5-prueba-del-sistema-jerarquía-casos-y-métricas-de-qa)
6. [Catálogo Oficial de Reglas de Negocio y Casos de Borde (CB)](#6-catálogo-oficial-de-reglas-de-negocio-y-casos-de-borde-cb)

---

## 1. Equipo de Proyecto y Roles

El equipo opera bajo el marco ágil Scrum con distribución estricta de responsabilidades funcionales, técnicas y de calidad:

| Integrante | Rol Oficial en el Proyecto | Responsabilidades Principales |
|---|---|---|
| **Mariano Decalli** | **Facilitador (Scrum Master), Analista de Procesos y QA Lead** | Gobernanza metodológica Scrum, facilitación de ceremonias, DoR/DoD en Jira, diseño de matrices de prueba y coordinación de UAT con POs. |
| **Sacha Del Barrio** | **Analista Funcional, Calidad (QA) y Accesibilidad Cognitiva Lead** | Relevamiento de procesos con escuelas especiales, especificación de requerimientos, auditoría de accesibilidad universal (WCAG 2.1/2.2 AA y AAA), diseño de casos de prueba pedagógicos y diseño de la alerta temprana CB-07. |
| **Germán Cochis** | **Frontend Lead, Diseñador UX/UI y Accesibilidad Visual** | Desarrollo de la SPA en Angular 20, maquetación adaptativa SCSS, empaquetado móvil Android (Capacitor 8), implementación de los componentes de gráficos (torta y barras) y suites E2E Playwright de UI. |
| **Fernando Aparicio** | **Backend Lead, Lógica de Negocio y QA Automatización** | Arquitectura limpia ASP.NET Core 10, persistencia en PostgreSQL + `pgvector`, endpoints de analítica, generación de informes QuestPDF, canal en vivo SignalR y suites de pruebas unitarias xUnit/Moq. |
| **Candelaria Ferreyra<br>Catalina Vettorazzi** | **Product Owners (Referentes de Educación Especial)** | Definición de objetivos pedagógicos, priorización del Product Backlog, validación clínica/formativa de actividades y homologación final en UAT. |
| **Prof. González<br>Prof. Ferrando** | **Docentes Evaluadores Académicos** | Supervisión metodológica y aprobación curricular de las Prácticas Profesionalizantes. |

> ⚠️ *Nota de gobernanza:* Mirko Ivo Wlk no posee asignaciones activas de desarrollo ni de calidad en el proyecto; sus tareas fueron absorbidas y completadas por Germán Cochis y Fernando Aparicio.

---

## 2. Arquitectura del Sistema: Diagrama de Despliegue

La solución adopta una arquitectura cliente-servidor distribuida, orientada a la privacidad, la accesibilidad universal y la alta disponibilidad en entornos escolares:

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                      1. DISPOSITIVOS CLIENTES (PUNTOS DE ACCESO)                        │
├──────────────────────────────┬──────────────────────────────┬───────────────────────────┤
│   Estación Docente / Laptop  │   Tablet Táctil del Alumno   │  Smartphone / Tablet      │
│   • Portal Profesional       │   • Portal Adaptativo (AAC)  │    de la Familia          │
│   • Panel "Mi Aula" en vivo  │   • Modo "Mi Camino" (Juego) │   • Portal Familiar       │
│   • Torta y Barras de Niveles│   • Lector de pantalla (TTS) │   • Informes oficiales PDF│
│   • Alerta de frustración    │   • App Android (APK nativa) │   • Mensajería directa    │
└──────────────┬───────────────┴──────────────┬───────────────┴─────────────┬─────────────┘
               │ HTTPS (Cifrado bancario)     │ HTTPS / WSS en vivo         │ HTTPS
               ▼                              ▼                             ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│               2. SERVIDOR CENTRAL INSTITUCIONAL (DOCKER / LOCAL O CLOUD)                │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│  [Servidor Web Nginx Alpine]                                                            │
│  • Distribución ultrarrápida de vistas Angular 20, compresión de imágenes y sonidos     │
│  ─────────────────────────────────────────────────────────────────────────────────────  │
│  [Servidor de Aplicación: ASP.NET Core 10 Web API]                                     │
│  • Controladores REST, Clean Architecture, CQRS (MediatR), FluentValidation             │
│  • Canal en Tiempo Real SignalR Core (Alertas docentes instantáneas de aula)           │
│  • Motor de Reportes QuestPDF (Boletines e informes oficiales firmados)                 │
│  • Motor de Inferencia Local ONNX (Recomendación privada de ejercicios)                 │
│  ─────────────────────────────────────────────────────────────────────────────────────  │
│  [Almacenamiento Seguro: PostgreSQL 17 + Extensión pgvector]                            │
│  • Aislamiento multi-tenant estricto por escuela (InstitutionAccessFilter)              │
│  • Cifrado AES-256-GCM para diagnósticos de salud y legajos de menores (Ley 25.326)     │
│  • Búsqueda vectorial local (costo cero de licencias, sin fuga a nubes comerciales de IA│
└──────────────────────────────────────────┬──────────────────────────────────────────────┘
                                           │
                    ┌──────────────────────┴──────────────────────┐
                    ▼                                             ▼
┌──────────────────────────────────────┐      ┌────────────────────────────────────────┐
│  3. Catálogo ARASAAC (Externo)       │      │  4. Servidor SMTP Institucional        │
│  • Miles de pictogramas libres (CAA) │      │  • Invitaciones digitales y avisos     │
└──────────────────────────────────────┘      └────────────────────────────────────────┘
```

---

## 3. Ambiente de Implementación (Stack Tecnológico y Herramientas)

### 3.1. Herramientas de Gestión, Entorno y Versionado (Definición Sprint 0)

| Herramienta | Área / Rol | Uso Concreto en InclusiON |
|---|---|---|
| **GitHub** | Control de versiones | Repositorios (`InclusiON.Server`, `InclusiON.Client`, `InclusiON.Documents`), ramas protegidas y *Pull Requests*. |
| **Jira** | Gestión ágil Scrum | Product Backlog, tableros de sprint, historias de usuario, DoR/DoD y métricas burndown. |
| **Visual Studio** | IDE Backend | Desarrollo en C# .NET 10, pruebas unitarias y migraciones de Entity Framework Core. |
| **Visual Studio Code** | Editor Frontend | Desarrollo en Angular 20, TypeScript, estilos SCSS dinámicos y documentación en Markdown. |
| **Figma** | Diseño UX/UI | Prototipado de pantallas accesibles y verificación de contrastes bajo norma WCAG 2.1. |
| **Teams / WhatsApp** | Comunicación | Coordinación diaria (*Daily Scrum*), planificación y resolución ágil de impedimentos. |
| **Word / Excel / Markdown** | Documentación | Requisitos funcionales, actas de ceremonias ágiles y tablas de métricas de avance. |

### 3.2. Lenguajes de Programación
* **C# 13 (.NET 10):** Lógica de negocio robusta, APIs REST de alto desempeño, validaciones y cifrado criptográfico.
* **TypeScript 5.8:** Reactividad fluida en pantalla, tipado estricto y manejo de estado en Angular 20.
* **SCSS / CSS3 Dinámico:** Motor de accesibilidad visual; permite alternar al instante entre **7 perfiles visuales** (dislexia, baja visión, daltonismo) y **2 modos de luz**.
* **SQL (PostgreSQL):** Modelado relacional institucional, integridad referencial y almacenamiento histórico de evaluaciones.

### 3.3. Base de Datos y Persistencia
* **PostgreSQL 17:** Motor relacional open source, transaccional (ACID), con aislamiento multi-tenant por colegio y costo cero de licencias.
* **Entity Framework Core 10:** ORM empresarial para consultas LINQ optimizadas y migraciones versionadas.
* **Extensión pgvector:** Base de datos vectorial interna para emparejar necesidades pedagógicas con actividades sin enviar datos de alumnos a empresas externas.
* **Cifrado AES-256-GCM:** Cifrado de campo para diagnósticos y legajos confidenciales.

### 3.4. Tecnologías Frontend (Web y Móvil)
* **Angular 20 Standalone:** Arquitectura sin módulos (`NgModules`), Signals y navegación instantánea SPA.
* **Capacitor 8:** Compilación de la aplicación nativa Android (`InclusiON.apk`) con modo pantalla completa escolar.
* **CoreUI & Bootstrap 5:** Sistema de diseño responsivo accesible.
* **Web Speech API & Web Audio API:** Síntesis de voz (TTS) nativa en español para lectura automática de pictogramas y refuerzos sonoros.

### 3.5. Tecnologías Backend y Servicios
* **ASP.NET Core 10 Web API:** Servicios REST desacoplados bajo Clean Architecture y CQRS (MediatR).
* **SignalR Core:** WebSockets para notificar en vivo en la pantalla de la maestra si un alumno acumula fallos reiterados.
* **QuestPDF:** Motor de renderizado vectorial de boletines e informes oficiales en PDF.
* **ONNX Runtime:** Motor de inferencia local de machine learning.

### 3.6. Infraestructura
* **Docker & Docker Compose:** Empaquetamiento homogéneo para instalar en cualquier servidor escolar.
* **Nginx Alpine:** Servidor web reverso y compresión de activos estáticos.

---

## 4. Dashboards Analíticos y Componentes de Visualización (Sprints 6 y 7)

En la interfaz del profesional y dirección no se utilizan radares genéricos; la visualización se basa en dos componentes protagonistas de alto contraste:

```
┌─────────────────────────────────────────────────────────────┬─────────────────────────────────────────────────────────────┐
│       1. GRÁFICO DE TORTA DE ALTO CONTRASTE                 │       2. HISTOGRAMA DE BARRAS POR NIVELES (1 AL 10)         │
│          (HighContrastPieChartComponent)                    │          (LevelHistogramChartComponent)                     │
│                                                             │                                                             │
│                      [   Lengua   ]                         │   Alumnos                                                   │
│                  . - ~ ~ ~ ~ ~ ~ - .                        │     ▲                                                       │
│              . '         35%         ' .                    │     │   █           █ = Superaron con éxito                 │
│            /                             \                  │     │   █   █       ▒ = Estancados (requieren apoyo)        │
│           |  Matem.   ( 142 Sesiones )    | Cognitiva       │     │   █   █   █                                           │
│           |   25%     ( 78% Éxito    )    |   20%           │     │   █   █   █   ▒                                       │
│            \                             /                  │     │   █   ▒   █   ▒   █                                   │
│              . '       Comunicación  ' .                    │     └───┴───┴───┴───┴───┴───┴───┴───┴───┴───►               │
│                  ' - . _ _ _ _ . - '                        │        N1  N2  N3  N4  N5  N6  N7  N8  N9 N10               │
│                         20%                                 │                                                             │
│ • Estilo Donut interactivo con resumen central.             │ • Segmentación por color: Azules (superaron con éxito)      │
│ • Muestra volumen de sesiones y % de aciertos formativos.   │   vs. Naranjas/Ámbar (alumnos estancados con dificultad).   │
│ • Ratio de contraste verificado > 7:1 (WCAG AAA).           │ • Alerta visual inmediata del cuello de botella pedagógico. │
└─────────────────────────────────────────────────────────────┴─────────────────────────────────────────────────────────────┘
```

* **`HighContrastPieChartComponent`:** Torta interactiva para comparar el tiempo dedicado a cada área curricular (Lengua, Matemáticas, Cognitiva, Comunicación) y la distribución de estados de reportes directivos (`ReportStatusPieChart`).
* **`LevelHistogramChartComponent`:** Histograma de los 10 hitos del sendero "Mi Camino", destacando en ámbar a los estudiantes estancados según la regla **CB-07** para que el docente intervenga oportunamente.

---

## 5. Prueba del Sistema: Jerarquía, Casos y Métricas de QA

### 5.1. Jerarquía de Lotes de Prueba
1. **Pruebas Unitarias (.NET xUnit / Angular Karma):** Aislamiento de validadores, algoritmos de cálculo y reglas de negocio.
2. **Pruebas de Integración (ASP.NET TestHost + PostgreSQL):** Aislamiento multi-tenant, persistencia, generación de PDF con QuestPDF y eventos SignalR.
3. **Pruebas de Sistema y E2E (Playwright - 28 suites):** Flujos completos desde la UI de los 4 portales hasta la base de datos.
4. **Pruebas de Aceptación (UAT y Accesibilidad):** Validación de campo en tablets escolares Android con `InclusiON.apk` y auditoría automatizada con `axe-core` bajo WCAG 2.1/2.2 AA y AAA.

### 5.2. Casos de Prueba Clave
* **CP-AUTH-01:** Login adaptativo por PIN de 4 dígitos (con bloqueo tras 5 intentos fallidos y auto-envío).
* **CP-ACT-02:** Detección de frustración docente en tiempo real vía SignalR ante 3 fallos consecutivos (Regla CB-07).
* **CP-DASH-03:** Renderizado del histograma de 10 niveles y gráfico de torta de materias en el panel "Mi Aula".
* **CP-A11Y-04:** Auditoría automatizada con `axe-core` sobre los 4 portales (0 violaciones críticas/graves).
* **CP-REP-05:** Descarga de informe oficial en PDF y verificación de cifrado AES-256 en diagnósticos médicos.

### 5.3. Métricas de Cierre y Dictamen
* **Casos Totales Ejecutados:** **433 verificaciones**.
* **Tasa de Aprobación al Cierre:** **100.0%** (430 aprobados iniciales, 3 incidentes subsanados y re-verificados).
* **Cobertura de Código:** Backend 88.4%, Frontend 83.1%, Reglas de negocio 100%.
* **Dictamen:** **APROBADO PARA PRODUCCIÓN Y DEFENSA ACADÉMICA** (Firmado por Decalli, Del Barrio, Ferreyra y Vettorazzi).

---

## 6. Catálogo Oficial de Reglas de Negocio y Casos de Borde (CB)

El comportamiento de la plataforma está estrictamente regido por el catálogo oficial de reglas de negocio del repositorio:

| Código | Tipo / Módulo | Regla en Lenguaje de Negocio | Implementación Técnica y Resultado de Prueba |
|---|---|---|---|
| **CB-01** | Casos Borde / Reportes | **Bloqueo preventivo de baja por reportes pendientes:** No se puede dar de baja a un profesional si posee informes en estado `Draft` o `Submitted`. | Error `409 Conflict` (`ErrorCode.HasPendingReports`). Debe resolver los informes antes del cese. |
| **CB-02** | Casos Borde / Personal | **Bloqueo preventivo de baja por alumnos a cargo:** No se puede dar de baja a un profesional si tiene alumnos asignados activamente o si es supervisor exclusivo de login asistido. | Error `409 Conflict`. Exige reasignar los alumnos a otro colega activo para evitar desamparo. |
| **CB-05** | Casos Borde / Progreso | **Rejugabilidad y conservación de logros:** El alumno puede repetir actividades ya ganadas para afianzar seguridad. | Se almacena el nuevo intento en el historial clínico sin sobrescribir ni decrementar el hito alcanzado. |
| **CB-07** | Casos Borde / Aula | **Detección silenciosa de frustración y estancamiento:** Si un alumno comete 3 fallos consecutivos o agota sus intentos en un nivel, no recibe sonidos punitivos. | La tablet pausa amigablemente la actividad y emite una alerta SignalR en vivo al panel docente, marcándolo en el histograma de barras. |
| **CB-14 / V-38** | Reglas / Reportes | **Rechazo de reporte con motivo obligatorio:** Al rechazar un informe enviado, el directivo debe fundamentar el motivo. | `PATCH /api/reports/{id}/reject` exige `AdminComment` obligatorio (`422 Unprocessable Entity` si está vacío) y devuelve el informe a `Draft`. |
| **CB-20** | Reglas / Aprendizaje | **Separación tajante de ambientes:** El sendero gamificado "Mi Camino" (exploración personal libre) está estrictamente aislado de "Mis Tareas del Día". | Vistas, rutas y tablas desacopladas; las tareas diarias no alteran el sendero secuencial de 10 niveles. |
| **CB-21** | Reglas / Aprendizaje | **Desbloqueo formativo por umbral de dominio:** Para habilitar el siguiente hito, el alumno debe alcanzar un puntaje formativo (por defecto ≥ 60%). | `ScorePercent >= UnlockThresholdPercent` desbloquea el siguiente nodo sin aplicar notas rojas punitivas. |
| **HU-10 / CB-08** | Motor Adaptativo | **Ajuste automático de dificultad con clamping:** El motor sube nivel ante rachas de éxito y reduce distractores ante fallos continuos. | Restricción estricta dentro del intervalo `[MinDifficultyLevel, MaxDifficultyLevel]`. Si `Min = Max`, produce un no-op seguro. |
| **V-08 / IN-310** | Accesibilidad / Auth | **Métodos de login adaptativos coherentes:** El login visual alfanumérico fue **discontinuado** (`400 Bad Request`). | Para alumnos rigen exclusivamente **PIN numérico de 4 dígitos** o **Login Asistido** supervisado por docente o familiar autorizado. |
| **HU-IN-173** | Seguridad de Datos | **Cifrado de diagnósticos de salud y secreto profesional:** Diagnósticos CIF/CIE-10 y notas sensibles jamás se guardan en texto plano. | Cifrado transparente con algoritmo **AES-256-GCM** (`[Encrypted]`) y aislamiento institucional multi-tenant. |
