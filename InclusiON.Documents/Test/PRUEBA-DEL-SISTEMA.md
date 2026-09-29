# PRUEBA DEL SISTEMA — INCLUSION

**Institución Cervantes — Carrera de Analista de Sistemas**  
**Cátedra:** Prácticas Profesionalizantes  
**Proyecto:** InclusiON — Plataforma de Gestión y Aprendizaje para la Inclusión Educativa  
**Enfoque de Redacción:** Calidad de Software, Ingeniería de Pruebas y Accesibilidad Universal  
**Versión del Documento:** 1.0 (Documento Oficial de Aseguramiento de la Calidad y Testing)  

---

## 1. Introducción y Marco General de Pruebas

El presente documento formaliza el plan, diseño, ejecución y control del **Aseguramiento de la Calidad de Software (QA & Testing)** para la plataforma **InclusiON**.

Dado que InclusiON es un ecosistema socio-educativo y terapéutico destinado a escuelas especiales y centros de rehabilitación, el proceso de pruebas no se limita a la verificación funcional y técnica tradicional de una aplicación comercial. Requiere un marco de validación riguroso con especial foco en:
1. **Accesibilidad Universal y Ergonomía Cognitiva:** Cumplimiento estricto de las directrices WCAG 2.1 / 2.2 niveles AA y AAA, garantizando interfaces operables por estudiantes con parálisis cerebral, baja visión, hipoacusia, dislexia o trastorno del espectro autista (TEA).
2. **Seguridad y Confidencialidad de Datos Sensibles:** Cumplimiento de la Ley Nacional de Protección de Datos Personales N° 25.326 respecto al almacenamiento cifrado de legajos clínicos y diagnósticos de menores.
3. **Fidelidad del Motor de Adaptación Pedagógica:** Verificación matemática de los algoritmos de ajuste automático de dificultad, umbrales de superación y emisión de alertas de frustración docente en tiempo real.

---

## 2. Lotes de Prueba por Niveles de Jerarquía de Verificación

Para garantizar una cobertura integral, las pruebas se estructuran siguiendo la **Pirámide de Calidad de Software**, preparando lotes de datos específicos para cada nivel jerárquico:

```
                    ┌─────────────────────────┐
                    │    4. Aceptación (UAT)   │  Docentes PO, Alumnos, Tablets
                    ├─────────────────────────┤
                    │    3. Sistema (E2E)     │  Playwright (28 suites), Flujos Completos
                    ├─────────────────────────┤
                    │    2. Integración       │  APIs, PostgreSQL + pgvector, SignalR, PDF
                    ├─────────────────────────┤
                    │    1. Pruebas Unitarias │  xUnit, Moq, Karma, Reglas CB-07/20/21
                    └─────────────────────────┘
```

### 2.1. Nivel 1: Pruebas Unitarias (Component & Unit Testing)
* **Objetivo:** Verificar de forma aislada e independiente cada método, función, validador y regla de negocio del sistema, sin dependencias de red ni de bases de datos.
* **Alcance Técnico:**
  * *Backend (.NET 10):* Clases de dominio, comandos y queries CQRS (MediatR), validadores FluentValidation, cálculo de puntajes y umbrales de avance.
  * *Frontend (Angular 20):* Pipes de accesibilidad, servicios de estado con Signals, componentes de cálculo de porcentajes para gráficos de torta y barras.
* **Preparación del Lote de Pruebas (Test Fixtures & Mocks):**
  * **Lote A — Desbloqueo Formativo por Umbral (Regla CB-21):** Casos de prueba con puntajes inferiores al umbral (< 60%, mantiene estado `Locked`) y superiores (≥ 60%, transiciona a `Completed` y desbloquea el siguiente nodo del sendero) sin aplicar calificaciones punitivas.
  * **Lote B — Detección Silenciosa de Frustración (Regla CB-07 / HU-21):** Secuencias de 3 equivocaciones consecutivas o agotamiento de intentos (`MaxAttempts`), verificando el disparo booleano de `TriggerFrustrationAlert = true` y la emisión del evento en tiempo real sin mostrar mensajes de error en la tablet del estudiante.
  * **Lote C — Calibración del Motor Adaptativo (HU-10 / Reglas V-30 a V-34 / CB-08):** Evaluación de rachas de aciertos (`ConsecutiveSuccessToUpgrade`) para elevar dificultad y rachas de fallos (`ConsecutiveFailuresToDowngrade`) para reducir alternativas distractoras, respetando siempre el clamping estricto entre `MinDifficultyLevel` y `MaxDifficultyLevel`.
  * **Lote D — Separación Tajante de Ambientes (Regla CB-20):** Verificación de aislamiento de vistas: las actividades del sendero gamificado "Mi Camino" jamás se mezclan ni contaminan con las asignaciones de aula en "Mis Tareas del Día".
  * **Lote E — Rejugabilidad y Conservación de Logros (Regla CB-05):** Casos donde un alumno repite un nivel ya ganado; se verifica que el nuevo intento se almacena en el historial clínico sin sobrescribir ni decrementar el récord previo.
  * **Lote F — Bloqueos Preventivos de Baja / Hard Stop (Reglas CB-01 y CB-02):** Intentos de dar de baja a profesionales que poseen reportes pendientes (`Draft`/`Submitted`) o alumnos a cargo, verificando el rechazo con código `409 Conflict` / `HasPendingReports`.
  * **Lote G — Rechazo de Reportes con Motivo Obligatorio (Reglas CB-14 / V-38):** Solicitudes de rechazo de informes pedagógicos sin incluir el campo `AdminComment`, verificando el retorno de error de validación `422 Unprocessable Entity`.
  * **Lote H — Validación de Formatos y Autonomía (Reglas V-01 a V-08):** DNI duplicado, matrículas repetidas y asignación de método de login autónomo a personas con nivel de autonomía que exige supervisión clínica.

### 2.2. Nivel 2: Pruebas de Integración (Integration Testing)
* **Objetivo:** Comprobar la correcta interacción entre múltiples componentes de software: controladores REST, base de datos relacional y vectorial, canal de mensajería en tiempo real y motor de reportes.
* **Alcance Técnico:**
  * Interacción entre Entity Framework Core 10 y PostgreSQL 17 (`inclusion_test`).
  * Filtro global multi-tenant (`InstitutionAccessFilter`) para verificar que ninguna consulta devuelva datos de otra escuela.
  * Canal de WebSockets mediante SignalR Core para alertas en vivo.
  * Generación de archivos PDF vectoriales con QuestPDF y persistencia de trazas.
* **Preparación del Lote de Pruebas (Dataset de Base de Datos de Prueba):**
  * **Base de Datos Sembrada (`seed-data.sql`):**
    * 2 Instituciones escolares distintas (ID 1: "Escuela Especial Dr. René Favaloro", ID 2: "Centro Terapéutico Solar").
    * 4 Aulas configuradas (Sala Azul, Sala Amarilla, etc.).
    * 10 Alumnos registrados con sus respectivos legajos y diagnósticos CIE-10/CIF.
    * 4 Docentes y 4 Representantes Familiares con cuentas vinculadas.
    * Catálogo de 10 niveles del sendero pedagógico y banco de 50 actividades precargadas.

### 2.3. Nivel 3: Pruebas de Sistema y End-to-End (E2E Testing)
* **Objetivo:** Simular la experiencia integral del usuario final recorriendo flujos de negocio completos desde la interfaz gráfica web y móvil hasta la persistencia final en el servidor.
* **Alcance Técnico:**
  * Automatización mediante **Playwright** estructurado en **28 suites de prueba**.
  * Verificación de persistencia, navegación sin recarga de página (SPA) y sincronización de eventos entre portales.
* **Preparación del Lote de Pruebas (Perfiles de Usuario Simulados):**
  * **Usuario Docente:** `docente.prueba@escuelainclusion.edu.ar` (con permisos de administración de aula y visualización de dashboards).
  * **Usuario Alumno:** Alumno "Lucas Benítez" (acceso mediante PIN `1234`, perfil de dislexia activado).
  * **Usuario Familiar:** `familiar.lucas@correo.com` (con vínculo formal y permisos de lectura de informes aprobados).
  * **Usuario Administrador:** `admin@escuelainclusion.edu.ar` (gestión de catálogos y aprobación institucional de reportes).

### 2.4. Nivel 4: Pruebas de Aceptación del Usuario (UAT) y Accesibilidad Universal
* **Objetivo:** Validar que el producto resuelva las necesidades reales del aula especial y que la interacción sea natural, inclusiva y libre de barreras tecnológicas.
* **Alcance Técnico:**
  * Pruebas de campo con docentes, psicopedagogas y directivos (Product Owners).
  * Pruebas en hardware objetivo: Tablets táctiles Android (Samsung Galaxy Tab A8 / Lenovo Tab M10) con la aplicación nativa instalada mediante el archivo compilado `InclusiON.apk`.
  * Auditoría de accesibilidad con motor automatizado **axe-core** y pruebas manuales con teclado y lectores de pantalla.
* **Preparación del Lote de Pruebas (Perfiles de Accesibilidad Adaptativa):**
  * **Lote P1 (Baja Visión):** Tema de Ultra Alto Contraste (Amarillo sobre Negro, ratio > 7:1) y fuente *Atkinson Hyperlegible* en tamaño grande.
  * **Lote P2 (Dislexia):** Fuente *Lexend*, espaciado tipográfico interlinear ampliado y botones con guías de pictograma ARASAAC.
  * **Lote P3 (Cognitivo / TEA):** Modo despejado sin distracciones visuales, límite de 2 opciones por pantalla y síntesis de voz automática en español.

---

## 3. Responsables de Testing (Equipo de Calidad)

El equipo de proyecto distribuye las responsabilidades de verificación según las competencias técnicas y funcionales de cada integrante, asegurando segregación de tareas e independencia de criterio:

| Integrante | Rol en Pruebas | Responsabilidades Principales |
|---|---|---|
| **Mariano Decalli** | **Líder de Calidad y Procesos (QA Lead)** | • Planificación integral del plan de pruebas del proyecto.<br>• Diseño y control de las matrices de Casos de Prueba.<br>• Coordinación y facilitación de las sesiones de UAT con los Product Owners.<br>• Control de criterios *Definition of Ready* (DoR) y *Definition of Done* (DoD) en Jira.<br>• Elaboración del informe final de cierre y dictamen de calidad. |
| **Sacha Del Barrio** | **Especialista en QA Funcional y Accesibilidad (A11y Lead)** | • Diseño y ejecución de casos de prueba de accesibilidad universal (WCAG 2.1/2.2).<br>• Verificación de ratios de contraste y ergonomía visual de los 7 perfiles inclusivos.<br>• Validación funcional de los flujos del alumno ("Mi Camino") y login adaptativo (PIN y Asistido).<br>• Pruebas de usabilidad pedagógica y síntesis de voz (Web Speech API). |
| **Fernando Aparicio** | **Especialista en QA Técnico y Backend (Backend QA Lead)** | • Automatización de pruebas unitarias en .NET 10 con xUnit y Moq.<br>• Ejecución de pruebas de integración de base de datos PostgreSQL y `pgvector`.<br>• Verificación de seguridad de endpoints, tokens JWT y aislamiento multi-tenant.<br>• Pruebas de estrés y volumen en generación de informes PDF con QuestPDF. |
| **Germán Cochis** | **Especialista en QA Frontend y Mobile (Frontend QA Lead)** | • Automatización de pruebas de componentes y vistas en Angular 20.<br>• Ejecución y mantenimiento de suites E2E con Playwright.<br>• Pruebas de compatibilidad y rendimiento en tablets Android reales (Capacitor APK).<br>• Verificación de reactividad y renderizado de gráficos de torta y barras. |
| **Candelaria Ferreyra<br>Catalina Vettorazzi** | **Referentes Pedagógicos (Product Owners / UAT)** | • Validación final de aceptación en entorno de simulación de aula especial.<br>• Evaluación de pertinencia terapéutica del banco de actividades.<br>• Homologación y aprobación de los formatos oficiales de informes escolares. |
| **Prof. González<br>Prof. Ferrando** | **Comité Evaluador Académico** | • Supervisión metodológica de la cobertura de pruebas y rigurosidad del proceso. |

---

## 4. Proceso de Testing

El proceso de pruebas se ejecuta de manera continua e incremental a lo largo de cada sprint ágil, siguiendo cuatro fases ordenadas y sistemáticas:

```
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│ 1. Especificación   │ ──► │ 2. Planificación    │ ──► │ 3. Ejecución         │ ──► │ 4. Documentación    │
│ de Casos de Prueba  │     │ del Testing         │     │ del Testing         │     │ y Cierre            │
└─────────────────────┘     └─────────────────────┘     └─────────────────────┘     └─────────────────────┘
```

---

### 4.1. Especificación de Casos de Prueba

Los casos de prueba se redactan formalmente especificando precondiciones, pasos cronológicos, datos de entrada y resultados esperados. A continuación se presentan los casos representativos de los subsistemas neurálgicos de InclusiON:

#### Caso de Prueba CP-AUTH-01: Autenticación Adaptativa de Alumno mediante PIN de 4 Dígitos
* **Identificador:** CP-AUTH-01
* **Módulo / Subsistema:** Autenticación e Inclusión (Épica IN-1)
* **Nivel de Jerarquía:** Sistema / E2E
* **Prioridad:** Crítica (Alta)
* **Precondición:** El alumno "Lucas Benítez" está registrado con PIN configurado `1234` y estado `Activo`.
* **Datos de Entrada:** Identificador o selección de avatar + PIN: `1234`.
* **Pasos de Ejecución:**
  1. Ingresar a la pantalla de inicio de InclusiON y seleccionar la solapa "Portal Alumno".
  2. Seleccionar el avatar del estudiante "Lucas".
  3. En el teclado numérico de alto contraste con botones grandes, presionar los dígitos `1` - `2` - `3` - `4`.
  4. Presionar el botón "Ingresar" o aguardar el auto-envío al cuarto dígito.
* **Resultado Esperado:** 
  * El sistema emite un sonido armónico de confirmación.
  * Se genera el token de sesión con rol `Student` y permisos limitados al entorno de juego.
  * La pantalla redirige de inmediato al Sendero Pedagógico ("Mi Camino") mostrando el mapa de niveles.
  * Se prohíbe el acceso a cualquier barra de direcciones o menú administrativo.

---

#### Caso de Prueba CP-ACT-02: Detección y Notificación de Alerta de Frustración en Aula (Regla CB-07)
* **Identificador:** CP-ACT-02
* **Módulo / Subsistema:** Motor de Dificultad Adaptativa y Alertas en Tiempo Real (Épica IN-16)
* **Nivel de Jerarquía:** Integración y E2E
* **Prioridad:** Crítica (Alta)
* **Precondición:** La docente tiene abierta la sesión en su laptop en el panel "Mi Aula". El alumno está en su tablet resolviendo una actividad del Nivel 4.
* **Datos de Entrada:** 3 intentos fallidos sucesivos en la misma consigna de emparejamiento.
* **Pasos de Ejecución:**
  1. El alumno selecciona una opción incorrecta (Fallo 1) → el sistema ofrece refuerzo sonoro suave.
  2. El alumno selecciona nuevamente una opción incorrecta (Fallo 2) → el sistema resalta la consigna.
  3. El alumno comete un tercer fallo consecutivo (Fallo 3).
* **Resultado Esperado:**
  * En la tablet del alumno: La actividad se pausa suavemente mostrando un pictograma de descanso y la locución: *"¡Hiciste un gran esfuerzo! Esperemos juntos a la seño"*.
  * En el servidor backend: Se procesa la regla de negocio **CB-07**, registrando la sesión como `BloqueadaPorFrustracion` y emitiendo un mensaje por el hub de SignalR.
  * En la laptop de la docente: Aparece una alerta visual destacada en color ámbar en la tarjeta del alumno con un aviso sonoro no estridente: *"Lucas Benítez requiere apoyo en Nivel 4 - Actividad 12"*.

---

#### Caso de Prueba CP-DASH-03: Renderizado y Fidelidad del Histograma de Niveles y Torta de Categorías
* **Identificador:** CP-DASH-03
* **Módulo / Subsistema:** Dashboards Analíticos de Aula (Épica IN-7 / Sprint 7)
* **Nivel de Jerarquía:** Componente Frontend y E2E
* **Prioridad:** Alta
* **Precondición:** El aula "Sala Amarilla" registra 12 alumnos con historiales de actividades en base de datos (7 superaron Nivel 3, 3 estancados en Nivel 4 por regla CB-07, 2 en Nivel 6).
* **Datos de Entrada:** Consulta al endpoint `/api/analytics/classroom/1/levels-distribution`.
* **Pasos de Ejecución:**
  1. Iniciar sesión como docente y acceder a la pestaña "Rendimiento y Métricas".
  2. Seleccionar el aula "Sala Amarilla".
  3. Inspeccionar el componente `LevelHistogramChartComponent`.
  4. Inspeccionar el componente `HighContrastPieChartComponent`.
* **Resultado Esperado:**
  * El histograma muestra 10 columnas correspondientes a los 10 niveles del roadmap.
  * La barra del Nivel 4 muestra claramente el segmento ámbar de 3 alumnos en estado "Estancado / Requiere Apoyo", diferenciado del azul de "Superado".
  * El gráfico de torta de alto contraste distribuye correctamente el volumen de sesiones por materia (Lengua 35%, Matemáticas 25%, Cognitiva 20%, Comunicación 20%) con tooltip de texto legible y ratio de contraste verificado > 7:1.

---

#### Caso de Prueba CP-A11Y-04: Verificación Automatizada de Accesibilidad Universal (WCAG 2.1 AAA)
* **Identificador:** CP-A11Y-04
* **Módulo / Subsistema:** Motor de Accesibilidad y Vistas Inclusivas
* **Nivel de Jerarquía:** Sistema / Accesibilidad
* **Prioridad:** Crítica (Alta)
* **Precondición:** Servidor web levantado con el build de producción de Angular.
* **Datos de Entrada:** Ejecución del comando de prueba `npx playwright test tests/frontend/accessibility.spec.ts`.
* **Pasos de Ejecución:**
  1. El script recorre los 4 portales (Alumno, Docente, Familia, Admin).
  2. Inyecta la librería `axe-core` en cada vista clave.
  3. Valida: contraste de color en tema claro y tema oscuro, atributos `aria-label` en botones con iconos, etiquetas `<label>` vinculadas a controles de formulario mediante `for/id`, y jerarquía correcta de títulos (`<h1>` a `<h6>`).
* **Resultado Esperado:**
  * Cero violaciones de severidad Crítica (*Critical*) o Grave (*Serious*).
  * 100% de los elementos interactivos cuentan con foco visible (*focus indicator*) para navegación por teclado o pulsador asistivo.
  * Todas las imágenes y pictogramas contienen texto alternativo accesible.

---

#### Caso de Prueba CP-REP-05: Emisión de Informe Pedagógico en PDF y Validación de Cifrado
* **Identificador:** CP-REP-05
* **Módulo / Subsistema:** Informes Formales de Evolución (Épica IN-15 / QuestPDF)
* **Nivel de Jerarquía:** Integración
* **Prioridad:** Alta
* **Precondición:** El profesional redactó y firmó digitalmente el informe semestral del alumno. El director institucional aprobó el documento en la cola de revisión.
* **Datos de Entrada:** Solicitud de descarga en `/api/reports/104/download-pdf`.
* **Pasos de Ejecución:**
  1. El familiar inicia sesión en su portal y hace clic en "Descargar Informe Aprobado".
  2. El servidor ejecuta el motor `QuestPDF` compilando datos del legajo, gráficos y notas.
* **Resultado Esperado:**
  * Se genera un documento PDF válido con diseño institucional estandarizado.
  * El archivo incluye membrete, número de registro formal y firma del profesional.
  * La base de datos almacena el diagnóstico médico bajo cifrado de campo (AES-256), impidiendo su lectura en texto plano mediante consultas directas SQL.

---

### 4.2. Planificación del Testing

#### A. Estrategia de Pruebas
La estrategia combina **pruebas continuas automatizadas** en el repositorio para evitar regresiones de código y **pruebas funcionales exploratorias en dispositivos físicos** para validar la experiencia de usuario:
* **Automatización en Integración Continua (CI/CD):** Cada *Pull Request* en GitHub ejecuta automáticamente la suite de pruebas unitarias (.NET y Angular) y las pruebas de sintaxis de accesibilidad. Si un test falla, el merge a la rama `develop` queda bloqueado.
* **Frecuencia por Sprint:** Al inicio de cada sprint se definen los casos de prueba asociados a los criterios de aceptación de cada Historia de Usuario; al promediar el sprint se ejecutan pruebas de integración; y los últimos dos días se congelan cambios para pruebas E2E y UAT con los Product Owners.

#### B. Ambientes de Prueba (Test Environments)

| Ambiente | Propósito | Infraestructura / Configuración |
|---|---|---|
| **Entorno de Desarrollo (Local)** | Pruebas unitarias y de componentes rápidas durante la codificación. | Visual Studio / VS Code, base de datos local SQLite o Docker PostgreSQL, mocks en memoria. |
| **Entorno de Integración / QA (Staging)** | Ejecución de suites E2E Playwright y pruebas de integración de servicios. | Contenedor Docker `inclusion_test` con PostgreSQL 17 + `pgvector`, backend configurado con `ASPNETCORE_ENVIRONMENT=Testing`, servicio SMTP en modo simulado (*mock*). |
| **Entorno Piloto / UAT (Laboratorio Escolar)** | Pruebas de campo con docentes y estudiantes. | Tablets escolares Android físicas (Samsung / Lenovo) con la aplicación instalada (`InclusiON.apk`) conectadas a la red Wi-Fi escolar, interactuando con el servidor central institucional. |

#### C. Criterios de Entrada y Salida (DoR y DoD)
* **Criterios de Entrada (Ready for Test):**
  * Historia de Usuario con criterios de aceptación en formato Gherkin (*Dado... Cuando... Entonces...*).
  * Código commiteado en rama de feature sin errores de compilación.
  * Base de datos de prueba sembrada con los datos requeridos para el escenario.
* **Criterios de Salida (Done / Aprobado):**
  * 100% de los casos de prueba de prioridad Crítica y Alta ejecutados y en estado **Aprobado (Pass)**.
  * Cero defectos abiertos de severidad Bloqueante o Crítica.
  * Cobertura de código superior al 80% en componentes de lógica central y reglas pedagógicas.
  * Auditoría axe-core aprobada sin observaciones graves de accesibilidad.
  * Aprobación formal de los Product Owners (Candelaria Ferreyra y Catalina Vettorazzi).

---

### 4.3. Ejecución del Testing

La ejecución se llevó a cabo combinando corridas automáticas mediante scripts de consola y sesiones presenciales de testeo funcional manual:

#### A. Ejecución Automatizada
* **Pruebas de Backend (.NET 10):**
  ```bash
  dotnet test InclusiON.Server.Tests/InclusiON.Server.Tests.csproj --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
  ```
* **Pruebas E2E y Accesibilidad (Playwright):**
  ```bash
  cd InclusiON.Testing
  npx playwright test tests/e2e/ --reporter=list,html
  npx playwright test tests/frontend/accessibility.spec.ts
  ```

#### B. Ciclo de Gestión de Defectos (Bugs)
Cuando un caso de prueba arroja resultado fallido, se registra de inmediato un ticket de defecto en Jira con la siguiente clasificación de severidad:
1. **Bloqueante (Severity 1):** El sistema se congela, genera pantalla blanca o impide el acceso del alumno o docente (ej. bloqueo del login adaptativo).
2. **Crítica (Severity 2):** Falla en una regla de negocio central (ej. no se dispara la alerta de frustración o se calcula mal un puntaje de nivel).
3. **Mayor (Severity 3):** Falla funcional que admite una alternativa de operación (ej. error en filtro secundario del historial de reportes).
4. **Menor (Severity 4):** Defecto cosmético o ajuste visual menor que no perjudica la usabilidad ni incumple normas de accesibilidad.

---

### 4.4. Documentación del Resultado

#### A. Resumen Ejecutivo de Ejecución de Pruebas

| Nivel de Prueba | Casos Planificados | Casos Ejecutados | Casos Aprobados (Pass) | Casos Fallidos (Fail) | Tasa de Éxito (%) |
|---|---|---|---|---|---|
| **Pruebas Unitarias (Backend .NET)** | 148 | 148 | 148 | 0 | **100.0%** |
| **Pruebas Unitarias (Frontend Angular)** | 86 | 86 | 86 | 0 | **100.0%** |
| **Pruebas de Integración (APIs / BD / PDF)** | 52 | 52 | 51 | 1 *(Resuelto)* | **100.0%** |
| **Pruebas de Sistema / E2E (Playwright)** | 28 suites (94 tests) | 94 | 92 | 2 *(Resueltos)* | **100.0%** |
| **Pruebas de Accesibilidad (WCAG / axe-core)** | 35 vistas | 35 | 35 | 0 | **100.0%** |
| **Pruebas de Aceptación de Usuario (UAT)** | 18 flujos de negocio | 18 | 18 | 0 | **100.0%** |
| **Total Consolidado** | **433 verificaciones** | **433** | **430** | **3** | **99.3% Inicial / 100% Cierre** |

#### B. Cobertura de Código Alcanzada
* **Lógica de Negocio y Dominio Backend (.NET):** **88.4%** de cobertura de líneas.
* **Componentes y Servicios Críticos Frontend (Angular):** **83.1%** de cobertura de código.
* **Reglas de Negocio y Casos de Borde (CB-01, CB-02, CB-05, CB-07, CB-14, CB-20, CB-21):** **100.0%** de cobertura de caminos lógicos.

#### C. Registro de Defectos Detectados y Acciones Correctivas Aplicadas
Durante los ciclos de prueba se detectaron 3 incidencias relevantes que fueron subsanadas y re-verificadas exitosamente antes del cierre:

1. **BUG-QA-01 (Severidad: Crítica - Resuelto):**
   * *Descripción:* Al acumular el 3er intento fallido en tablets lentas, la señal SignalR de alerta de frustración se emitía dos veces por un rebote de evento táctil.
   * *Acción Correctiva:* Se implementó un operador `debounceTime(400)` en el servicio táctil de Angular y un bloqueo transaccional idempotente en el backend. Re-testeado: Aprobado.
2. **BUG-QA-02 (Severidad: Mayor - Resuelto):**
   * *Descripción:* En el histograma de distribución de niveles, el tooltip no respetaba la relación de contraste en monitores de docentes con brillo bajo.
   * *Acción Correctiva:* Se actualizó la paleta de variables SCSS del componente gráfico fijando fondo negro `#000000` con tipografía amarilla `#FFD700`, superando el ratio de 12:1 (nivel AAA). Re-testeado: Aprobado.
3. **BUG-QA-03 (Severidad: Menor - Resuelto):**
   * *Descripción:* La generación de reportes en PDF fallaba si el nombre de una materia contenía caracteres especiales no estándar en el membrete.
   * *Acción Correctiva:* Se configuró codificación UTF-8 estricta en las fuentes del motor QuestPDF. Re-testeado: Aprobado.

---

## 5. Dictamen Final de Calidad

> ### ✅ DICTAMEN: APTO PARA PRODUCCIÓN Y DEFENSA ACADÉMICA
>
> Sobre la base de los 433 casos de prueba ejecutados a lo largo de todos los niveles de jerarquía, habiéndose verificado el **100% de los flujos críticos aprobados**, con **cero defectos bloqueantes residuales**, una cobertura de pruebas superior a los estándares académicos exigidos y la homologación positiva de accesibilidad universal bajo normativas **WCAG 2.1 / 2.2 AA/AAA**, el equipo de Calidad y Procesos dictamina que la plataforma **InclusiON** se encuentra en estado **ESTABLE, ROBUSTA Y CONFORME** para su puesta en marcha en instituciones de educación especial y para su presentación en las Prácticas Profesionalizantes.
>
> **Firma del Responsable de QA y Procesos:** Mariano Decalli  
> **Firma del Analista Funcional y Accesibilidad:** Sacha Del Barrio  
> **Firma de Product Owners:** Candelaria Ferreyra / Catalina Vettorazzi  
