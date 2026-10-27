# CAPÍTULO 10: PRUEBAS DEL SISTEMA — INCLUSION

**Institución:** Instituto Técnico Superior Córdoba / Institución Cervantes  
**Carrera:** Tecnicatura Superior en Análisis de Sistemas / Práctica Profesionalizante  
**Proyecto:** InclusiON — Plataforma de Gestión y Aprendizaje para la Inclusión Educativa  
**Enfoque:** Aseguramiento de la Calidad (QA), Verificación Formal, Seguridad Defensiva y Accesibilidad Universal  
**Versión del Documento:** 2.0 (Revisión Final Post-Auditoría Académica)  

---

## 10.1. Introducción y Marco Metodológico de Verificación

El presente capítulo formaliza la estrategia, diseño, ejecución, control y resultados del proceso de **Aseguramiento de la Calidad de Software (QA & Testing)** implementado en la plataforma **InclusiON**.

Dada la naturaleza socio-educativa, médica y asistiva del sistema, el marco de verificación trasciende las pruebas funcionales tradicionales de una aplicación web estándar, estructurándose bajo tres pilares de rigor obligatorio:
1. **Accesibilidad Universal y Ergonomía Cognitiva:** Cumplimiento verificable de las directrices **WCAG 2.1 y 2.2 niveles AA y AAA**, asegurando que la interfaz sea operable por personas con parálisis cerebral, baja visión, hipoacusia, dislexia o condición del espectro autista (TEA).
2. **Seguridad Defensiva y Protección de Datos Sensibles (Ley N° 25.326):** Verificación de aislamiento multi-tenant estricto entre instituciones, almacenamiento criptográfico de diagnósticos médicos (AES-256-GCM) y mecanismos contra fuerza bruta y claves débiles en accesos de estudiantes.
3. **Fidelidad del Motor de Adaptación Pedagógica:** Validación matemática de los algoritmos de ajuste dinámico de dificultad, umbrales de promoción sin calificación punitiva y despacho idempotente de alertas de frustración docente en tiempo real (SignalR).

---

## 10.2. Lotes de Prueba por Niveles de Jerarquía de Verificación

Siguiendo la **Pirámide de Pruebas de Software (Mike Cohn)**, las 433 verificaciones del sistema se distribuyen en cuatro niveles jerárquicos complementarios:

```
                            ┌───────────────────────────────┐
                            │    Nivel 4: Aceptación (UAT)   │  18 flujos (Validación docente en tablets)
                            ├───────────────────────────────┤
                            │    Nivel 3: Sistema y E2E     │  94 pruebas (Playwright en 28 suites)
                            ├───────────────────────────────┤
                            │    Nivel 2: Integración       │  52 pruebas (PostgreSQL, SignalR, QuestPDF)
                            ├───────────────────────────────┤
                            │    Nivel 1: Pruebas Unitarias │  234 pruebas (xUnit .NET 10 + Karma Angular)
                            └───────────────────────────────┘
```

### 10.2.1. Nivel 1: Pruebas Unitarias (Unit Testing)
* **Objetivo:** Aislar y verificar exhaustivamente la lógica atómica de funciones, validadores, mapeadores y reglas de negocio, sin dependencias de infraestructura (mocking de repositorios y contexto HTTP).
* **Alcance:**
  * **Backend (.NET 10 / xUnit + Moq):** Comandos y queries CQRS (MediatR), validadores FluentValidation, cálculo de umbrales de promoción y bloqueo de acciones inconsistentes.
  * **Frontend (Angular 20 / Karma + Jasmine):** Pipes de accesibilidad, servicios de estado reactivo con Signals, directivas de alto contraste y controladores de interfaz táctil.
* **Lotes de Prueba Específicos:**
  * *Lote A (Regla CB-21):* Verificación del umbral de superación formativa (puntajes < 60% retienen el nodo en `Locked` sin penalización punitiva; puntajes ≥ 60% habilitan el siguiente nodo).
  * *Lote B (Regla CB-07 / HU-21):* Detección de frustración al acumular exactamente 3 fallos consecutivos en una actividad, activando la bandera booleana `TriggerFrustrationAlert = true`.
  * *Lote C (Reglas CB-01 y CB-02):* Rechazo de baja de profesionales con alumnos asignados o reportes en estado `Draft`/`Submitted` (`409 Conflict`).
  * *Lote D (Regla CB-14):* Rechazo de solicitud de desaprobación de reportes pedagógicos que omitan el motivo formal (`AdminComment`).

### 10.2.2. Nivel 2: Pruebas de Integración (Integration Testing)
* **Objetivo:** Comprobar la interacción acoplada entre capas: controladores REST, persistencia relacional/vectorial, mensajería en tiempo real y generación documental.
* **Alcance Técnico:**
  * Interacción entre Entity Framework Core 10 y PostgreSQL 17 (`inclusion_test`).
  * Filtro global multi-tenant (`InstitutionAccessFilter`) garantizando que ninguna consulta devuelva datos de otra institución escolar.
  * Hub de WebSockets (SignalR Core) para sincronización docente-alumno.
  * Motor de composición vectorial QuestPDF para reportes clínicos y pedagógicos.
* **Lote de Datos Sembrado (`seed-data.sql`):**
  * 2 Instituciones independientes (ID 1: "Escuela Especial René Favaloro", ID 2: "Centro Terapéutico Solar").
  * 4 Aulas configuradas (Sala Azul, Sala Amarilla, Taller Laboral, Apoyo Pedagógico).
  * 10 Alumnos registrados con diagnósticos CIE-10/CIF cifrados en base de datos.
  * 4 Docentes/Terapeutas y 4 Representantes Familiares con cuentas vinculadas.

### 10.2.3. Nivel 3: Pruebas de Sistema y End-to-End (E2E Testing)
* **Objetivo:** Simular recorridos completos de usuarios reales en un navegador headless mediante **Playwright**, verificando la integridad de datos desde la UI hasta la base de datos sin recarga de página.
* **Estructura:** **28 suites de prueba** que cubren 94 casos automatizados en los cuatro portales del sistema (Administrador, Profesional/Docente, Familiar y Alumno).

### 10.2.4. Nivel 4: Pruebas de Aceptación del Usuario (UAT) y Accesibilidad Universal
* **Objetivo:** Validar la usabilidad de campo en dispositivos físicos reales y certificar la eliminación de barreras digitales bajo estándares internacionales.
* **Hardware y Software:** Tablets Android físicas (Samsung Galaxy Tab A8 y Lenovo Tab M10) con la aplicación nativa (`InclusiON.apk`), lectores de pantalla TalkBack y pulsadores físicos asistivos.
* **Auditoría de Accesibilidad:** Escaneo automatizado de 35 vistas mediante el motor **axe-core** integrado a Playwright, complementado con auditorías manuales de ratio fotométrico y navegación por teclado.

---

## 10.3. Matriz de Roles y Segregación de Funciones en QA

Para asegurar la objetividad e independencia de criterio, el equipo técnico distribuyó las tareas de testing según las especialidades de sus integrantes:

| Integrante | Rol en Pruebas | Responsabilidades Asignadas |
|---|---|---|
| **Mariano Decalli** | **Líder de Calidad y Procesos (QA Lead)** | Planificación del plan maestro de testing, elaboración y control de las matrices de trazabilidad, gestión de defectos en Jira y redacción de los dictámenes de homologación de calidad. |
| **Sacha Del Barrio** | **Especialista en Accesibilidad y QA Funcional (A11y Lead)** | Auditoría de normas WCAG 2.1/2.2 AA y AAA, validación fotométrica de ratios de contraste, pruebas de navegación por teclado/pulsador, y testeo funcional de los flujos del alumno en tablets físicas. |
| **Fernando Aparicio** | **Especialista en QA Técnico y Backend (Backend QA Lead)** | Automatización de pruebas unitarias en .NET 10 (xUnit), pruebas de integración de base de datos PostgreSQL, verificación de seguridad en endpoints (JWT/Argon2id) y pruebas de carga en generación PDF con QuestPDF. |
| **Germán Cochis** | **Especialista en QA Frontend y Mobile (Frontend QA Lead)** | Automatización de pruebas de componentes en Angular 20, desarrollo y mantenimiento de las 28 suites E2E con Playwright, y pruebas de compatibilidad en dispositivos móviles Android (Capacitor). |
| **Candelaria Ferreyra<br>Catalina Vettorazzi** | **Referentes Pedagógicos (Product Owners / UAT)** | Validación de aceptación funcional en contexto áulico simulado, evaluación de pertinencia pedagógica de consignas y aprobación de formatos de boletines e informes escolares. |

---

## 10.4. Matriz Maestra de las 433 Verificaciones (Evidencia Formal de Ejecución)

En respuesta al requerimiento de auditoría académica, la siguiente matriz desglosa formalmente las **433 verificaciones ejecutadas**, distinguiendo entre casos planificados, ejecutados, aprobados en primera instancia, fallidos, re-evaluados tras la aplicación del parche correctivo y aprobados finales:

| Nivel de Prueba | Módulo / Subsistema Auditado | Herramienta / Runner de Ejecución | Planificadas | Ejecutadas | Aprobadas (1ª Ronda) | Fallidas (1ª Ronda) | Repetidas tras Fix (Re-test) | Aprobadas Finales | Tasa de Éxito Final |
|---|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Nivel 1: Unitaria** | Backend: Dominio, CQRS, Reglas CB y Validadores | xUnit + Moq (.NET 10) | 148 | 148 | 148 | 0 | 0 | 148 | **100.0 %** |
| **Nivel 1: Unitaria** | Frontend: Servicios, Signals, Pipes y Directivas | Karma + Jasmine (Angular 20) | 86 | 86 | 86 | 0 | 0 | 86 | **100.0 %** |
| **Nivel 2: Integración** | Repositorios, EF Core, Multi-Tenant y PostgreSQL | xUnit + Testcontainers (PostgreSQL 17) | 28 | 28 | 28 | 0 | 0 | 28 | **100.0 %** |
| **Nivel 2: Integración** | WebSockets en Vivo (SignalR) y Alertas de Aula | xUnit + SignalR Client Test Fixture | 12 | 12 | 11 | 1 *(BUG-01)* | 1 | 12 | **100.0 %** |
| **Nivel 2: Integración** | Motor de Generación Documental en PDF | xUnit + QuestPDF Engine Runner | 12 | 12 | 11 | 1 *(BUG-03)* | 1 | 12 | **100.0 %** |
| **Nivel 3: Sistema/E2E** | Suite Autenticación, Roles y Seguridad JWT | Playwright Test (Headless Chromium) | 24 | 24 | 24 | 0 | 0 | 24 | **100.0 %** |
| **Nivel 3: Sistema/E2E** | Suite Sendero Alumno "Mi Camino" y Gamificación | Playwright Test (Emulación Touch Tablet) | 32 | 32 | 32 | 0 | 0 | 32 | **100.0 %** |
| **Nivel 3: Sistema/E2E** | Suite Dashboards Analíticos y Reportes de Aula | Playwright Test (Desktop & Tablet) | 20 | 20 | 19 | 1 *(BUG-02)* | 1 | 20 | **100.0 %** |
| **Nivel 3: Sistema/E2E** | Suite Aprobación Maker-Checker de Informes PPI | Playwright Test (Multi-rol concurrente) | 18 | 18 | 18 | 0 | 0 | 18 | **100.0 %** |
| **Nivel 4: Accesibilidad** | Auditoría WCAG 2.1/2.2 AA y AAA en Vistas Clave | @axe-core/playwright (35 vistas web) | 35 | 35 | 35 | 0 | 0 | 35 | **100.0 %** |
| **Nivel 4: Aceptación** | Flujos de Negocio de Campo con Docentes (UAT) | Pruebas Manuales en Tablets Android Físicas | 18 | 18 | 18 | 0 | 0 | 18 | **100.0 %** |
| **TOTAL CONSOLIDADO** | **Ecosistema Global InclusiON** | **Suite Integral de Calidad** | **433** | **433** | **430** | **3** | **3** | **433** | **100.0 %** |

> **Nota Metodológica sobre la Tasa de Éxito:**  
> En la primera ronda de ejecución, la tasa de aprobación directa fue de **99.31 % (430 aprobadas sobre 433)**. Los 3 casos fallidos correspondieron a los defectos de desarrollo identificados como `BUG-QA-01`, `BUG-QA-02` y `BUG-QA-03`. Tras implementar las soluciones técnicas correspondientes y ejecutar las pruebas de regresión (*re-testing*), dichos casos alcanzaron el estado de **Aprobado**, consolidando la tasa de éxito final del **100.0 %**.

---

## 10.5. Evidencias Objetivas de Estabilidad, Cobertura y Accesibilidad

Para fundamentar técnicamente las conclusiones de estabilidad y calidad sobresaliente, se adjuntan los registros y métricas empíricas obtenidas:

### 10.5.1. Cobertura de Código Automatizada (Code Coverage)
El análisis cuantitativo de cobertura fue compilado mediante la herramienta **Coverlet (XPlat Code Coverage)** para el backend y **Karma-Coverage (Istanbul)** para el frontend:

| Componente del Sistema | Métrica Evaluada | Resultado Obtenido | Umbral Mínimo Exigido | Estado |
|---|---|:---:|:---:|:---:|
| **Backend — Lógica de Dominio y Casos de Uso** | Cobertura de Líneas | **88.4 %** | ≥ 80.0 % | Conforme |
| **Backend — Validadores y Reglas de Negocio (CB)** | Cobertura de Ramas (*Branch*) | **100.0 %** | 100.0 % | Conforme |
| **Frontend — Componentes y Servicios Críticos** | Cobertura de Sentencias | **83.1 %** | ≥ 75.0 % | Conforme |
| **Frontend — Directivas y Utilidades de Accesibilidad** | Cobertura de Ramas (*Branch*) | **92.5 %** | ≥ 85.0 % | Conforme |

### 10.5.2. Evidencias de Accesibilidad Universal (WCAG 2.1 / 2.2)
La verificación de accesibilidad no constituye una apreciación subjetiva, sino que está sustentada en auditorías normativas de software:
1. **Auditoría Automatizada axe-core (`@axe-core/playwright`):**
   * Total de vistas escaneadas: 35 páginas del sistema.
   * Reglas evaluadas: `wcag2a`, `wcag2aa`, `wcag21aa`, `wcag22aa`, `wcag2aaa`.
   * **Violaciones Críticas (*Critical*):** 0
   * **Violaciones Graves (*Serious*):** 0
   * **Violaciones Menores / Sugerencias:** 0 residuales.
2. **Auditoría Google Lighthouse (Versión 12.0):**
   * Portal Alumno: **100 / 100**
   * Portal Docente: **100 / 100**
   * Portal Familiar: **100 / 100**
   * Portal Administrador: **100 / 100**
3. **Medición Fotométrica de Ratios de Contraste (Colour Contrast Analyser):**
   * Modo Predeterminado (Texto general): **8.6:1** (Excede el umbral de 4.5:1 exigido por WCAG AA).
   * Modo Ultra Alto Contraste (Amarillo `#FFE600` sobre Fondo Negro Puro `#000000`): **14.1:1** (Supera ampliamente el estándar de 7:1 exigido por WCAG AAA).
4. **Navegabilidad Asistiva:**
   * 100 % de los controles interactivos poseen foco visible inequívoco (`outline: 3px solid #FFE600; outline-offset: 2px`).
   * Compatibilidad auditada con lectores de pantalla mediante etiquetas `aria-label`, atributos semánticos `role` y anuncios dinámicos vía `aria-live="polite"` en alertas de aula.

---

## 10.6. Seguridad en el Acceso de Estudiantes (Justificación de PIN y Mitigación de Fuerza Bruta)

El panel evaluador observó el uso del PIN `1234` en el caso representativo CP-AUTH-01 y requirió la justificación de las defensas contra claves débiles e intentos repetidos de acceso.

### 10.6.1. Naturaleza del PIN «1234» en las Pruebas
El PIN `1234` utilizado en la documentación de prueba **no constituye una clave predeterminada del sistema en producción**. Corresponde estrictamente a un *Test Fixture* determinístico sembrado en la base de datos de pruebas (`seed-data.sql` / `login/credenciales.csv`) con el único propósito de permitir la ejecución repetible y desatendida de las suites de prueba E2E de Playwright en el pipeline de Integración Continua (CI/CD).

### 10.6.2. Almacenamiento Criptográfico con Argon2id
El sistema jamás almacena PINs ni contraseñas en texto plano. En el componente `Argon2idPinHasher.cs` ([Argon2idPinHasher.cs](file:///d:/git/InclusiON.Server/InclusiON.Infrastructure/Authentication/Argon2idPinHasher.cs)), se implementa el algoritmo **Argon2id**, recomendado internacionalmente por OWASP para credenciales de baja entropía:
* **Costo de Memoria (*Memory Cost*):** 64 MB (65.536 KB), lo que neutraliza ataques de fuerza bruta masivos basados en hardware especializado (GPUs y ASICs).
* **Costo Temporal (*Iterations*):** 3 iteraciones.
* **Paralelismo (*Lanes/Threads*):** 1 hilo.
* **Sal Criptográfica (*Cryptographic Salt*):** 128 bits generados aleatoriamente por `RandomNumberGenerator.Fill(salt)`.
* **Tiempo de Cómputo Intencional:** ~80–150 ms por intento, imperceptible para un estudiante pero computacionalmente prohibitivo para ataques de diccionario *offline*.

### 10.6.3. Protección contra Ataques de Fuerza Bruta (Rate Limiting y Lockout)
InclusiON implementa una arquitectura defensiva en capas para mitigar intentos reiterados de adivinación del PIN de 4 dígitos:

```
Petición HTTP PIN ──► [ Rate Limiter Gateway ] ──► [ PinLoginCommandHandler ] ──► [ SignalR Notifier ]
 (POST /login/pin)     (auth-pin: 5 req / 5 min)      (Lockout al 5° fallo)         (Alerta a Docente y Familia)
```

1. **Limitación de Tasa a Nivel API (`[EnableRateLimiting("auth-pin")]`):**
   * Configurado en `RateLimiterExtensions.cs` ([RateLimiterExtensions.cs](file:///d:/git/InclusiON.Server/InclusiON.Api/Extensions/RateLimiterExtensions.cs#L60-L69)).
   * Aplica una política de **Ventana Deslizante (*Sliding Window*) de 5 minutos**, dividida en 5 segmentos, con un límite máximo estricto de **5 solicitudes permitidas por dirección IP**.
   * Si un atacante supera este umbral, el servidor rechaza inmediatamente la conexión con código HTTP **429 Too Many Requests**, retornando el mensaje: *"Demasiados intentos. Esperá unos minutos antes de reintentar."*
2. **Política de Bloqueo Progresivo de Cuenta (*Account Lockout Policy*):**
   * Administrado en `PinLoginCommandHandler.cs` ([PinLoginCommandHandler.cs](file:///d:/git/InclusiON.Server/InclusiON.Application/UseCases/Auth/Handlers/PinLoginCommandHandler.cs#L22-L106)).
   * El sistema tolera hasta 5 fallos consecutivos de credencial. A cada fallo, la respuesta informa los intentos restantes (`RemainingAttempts`).
   * Al alcanzar el quinto fallo consecutivo, la cuenta del alumno pasa al estado **Bloqueada Temporalmente** (`IsLocked = true`) durante 15 minutos.
3. **Notificación en Tiempo Real de Cuenta Bloqueada:**
   * Al bloquearse la cuenta, el método `NotifyAccountLockedAsync()` despacha de forma instantánea una notificación por WebSockets (SignalR) con prioridad alta:
     * Al portal del representante familiar: *"🔒 Cuenta bloqueada: [Nombre del Alumno] superó los intentos de PIN permitidos y su cuenta quedó bloqueada temporalmente."*
     * Al panel del docente o terapeuta a cargo en el aula, permitiendo la intervención presencial inmediata.

### 10.6.4. Política de Prevención de Claves Débiles y Login Asistido
* **Asignación Supervisada:** El PIN del alumno no es auto-registrado públicamente, sino configurado por el profesional a cargo desde el módulo de configuración de acceso ([UpdatePersonAccessConfigurationCommandHandler.cs](file:///d:/git/InclusiON.Server/InclusiON.Application/UseCases/Persons/Handlers/UpdatePersonAccessConfigurationCommandHandler.cs)).
* **Validación de Entropía:** El validador rechaza cadenas de longitud inválida y listas negras de secuencias triviales (`1234`, `4321`, `0000`, `1111`).
* **Alternativa Inclusiva — Login Asistido (`/api/auth/login/assisted`):**
  Para aquellos estudiantes con discapacidades cognitivas severas o parálisis motriz en quienes recordar o digitar un PIN resulta inviable o riesgoso, InclusiON proporciona el mecanismo de **Acceso Asistido**. En esta modalidad, el alumno jamás maneja contraseñas: el docente o terapeuta supervisor autoriza el ingreso desde su propio dispositivo institucional autenticado, eliminando toda superficie de vulnerabilidad por exposición de credenciales del menor.

---

## 10.7. Registro de Defectos Detectados, Acciones Correctivas y Re-Testing

Durante los ciclos de testing se identificaron 3 defectos que requirieron intervención en el código fuente. A continuación se presentan sus fichas técnicas formales:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ FICHA DE DEFECTO: BUG-QA-01                                                            │
├──────────────────────┬────────────────────────────────┬─────────────────┬──────────────┤
│ Identificador:       │ BUG-QA-01                      │ Severidad:      │ Crítica (S2) │
├──────────────────────┼────────────────────────────────┴─────────────────┴──────────────┤
│ Módulo Afectado:     │ Alertas de Aula en Tiempo Real (SignalR) / Portal Alumno Tablet │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Descripción:         │ Al cometerse el 3er error consecutivo en tablets táctiles de     │
│                      │ rendimiento moderado, el estudiante tendía a presionar la        │
│                      │ pantalla varias veces seguidas antes de la animación de pausa,   │
│                      │ provocando el envío duplicado de la señal de alerta a la docente.│
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Causa Raíz:          │ Falta de desrebote (debounce) en el manejador táctil de Angular  │
│                      │ y ausencia de filtro de idempotencia transaccional en el backend.│
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Solución Aplicada:   │ 1. Frontend: Inclusión de operador `exhaustMap` y bloqueo de     │
│                      │    interfaz inmediata tras el tercer fallo.                      │
│                      │ 2. Backend: Verificación atómica de idempotencia en la sesión     │
│                      │    activa (`SessionId + AttemptNumber`) antes de emitir SignalR. │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Re-Testing / Regres.:│ Suite Playwright simulando ráfaga de 3 toques táctiles en 50 ms. │
│                      │ Resultado: 1 sola notificación emitida y recibida. (APROBADO)    │
└──────────────────────┴──────────────────────────────────────────────────────────────────┘
```

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ FICHA DE DEFECTO: BUG-QA-02                                                            │
├──────────────────────┬────────────────────────────────┬─────────────────┬──────────────┤
│ Identificador:       │ BUG-QA-02                      │ Severidad:      │ Mayor (S3)   │
├──────────────────────┼────────────────────────────────┴─────────────────┴──────────────┤
│ Módulo Afectado:     │ Componentes de Gráficos Analíticos de Aula (Dashboard Docente)   │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Descripción:         │ En monitores de bajo brillo utilizados en escuelas, el texto del │
│                      │ tooltip en el histograma de niveles presentaba contraste de 4.8:1│
│                      │ (aprobaba WCAG AA pero no alcanzaba la meta institucional AAA).  │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Causa Raíz:          │ Uso de la variable general de tooltip en lugar de la directiva   │
│                      │ especializada de ultra alto contraste en elementos SVG dinámicos.│
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Solución Aplicada:   │ Rediseño del componente `HighContrastTooltipDirective`, forzando │
│                      │ fondo negro `#000000`, borde de 2px en `#FFE600` y tipografía    │
│                      │ amarilla `#FFE600` semibold, elevando el ratio medido a 14.1:1.  │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Re-Testing / Regres.:│ Escaneo de contraste automatizado con axe-core y medición con    │
│                      │ Colour Contrast Analyser. Violaciones: 0. (APROBADO)             │
└──────────────────────┴──────────────────────────────────────────────────────────────────┘
```

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ FICHA DE DEFECTO: BUG-QA-03                                                            │
├──────────────────────┬────────────────────────────────┬─────────────────┬──────────────┤
│ Identificador:       │ BUG-QA-03                      │ Severidad:      │ Menor (S4)   │
├──────────────────────┼────────────────────────────────┴─────────────────┴──────────────┤
│ Módulo Afectado:     │ Motor de Generación de Informes PDF (QuestPDF)                  │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Descripción:         │ La exportación de boletines fallaba cuando el legajo contenía    │
│                      │ caracteres diacríticos complejos (acentos, virgulilla «ñ», signos│
│                      │ de interrogación invertidos «¿») en los diagnósticos médicos.   │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Causa Raíz:          │ La tipografía institucional no incrustaba el conjunto extendido  │
│                      │ de caracteres UTF-8 en el compilador vectorial de QuestPDF.      │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Solución Aplicada:   │ Incrustación estricta de fuentes OTF con soporte Unicode latino  │
│                      │ completo y normalización previa del texto mediante `FormC`.      │
├──────────────────────┼──────────────────────────────────────────────────────────────────┤
│ Re-Testing / Regres.:│ Generación automatizada de 20 PDFs con cadenas complejas de texto│
│                      │ médico y pedagógico. Resultado: 100 % generados sin error. (APRO)│
└──────────────────────┴──────────────────────────────────────────────────────────────────┘
```

---

## 10.8. Casos de Prueba Representativos Formalizados

A continuación se documentan en detalle cuatro casos de prueba paradigmáticos que ilustran la rigurosidad operativa aplicada en las distintas capas del sistema:

### Caso de Prueba CP-AUTH-01: Autenticación Adaptativa de Alumno mediante PIN
* **Módulo:** Autenticación e Identificación Adaptativa (IN-1)
* **Nivel:** E2E / Sistema
* **Precondición:** El alumno "Lucas Benítez" se encuentra dado de alta en la institución, con nivel de autonomía que admite PIN de 4 dígitos y clave sembrada en base de datos de pruebas.
* **Datos de Entrada:** Avatar: Alumno Lucas / PIN: `1234`
* **Pasos de Ejecución:**
  1. Ingresar a la URL del portal del estudiante.
  2. Seleccionar el avatar de "Lucas".
  3. En el teclado accesible de alto contraste con botones táctiles de 72x72px, pulsar sucesivamente los dígitos `1`, `2`, `3` y `4`.
* **Resultado Esperado:**
  * El sistema emite un refuerzo auditivo armónico de confirmación (tono suave).
  * Se genera el token de sesión con rol `Student` limitado estrictamente al sendero gamificado.
  * Se redirecciona al componente `RoadmapComponent` ("Mi Camino").
  * Las rutas administrativas (`/admin`, `/pro`, `/family`) quedan bloqueadas por guardias de navegación (`RoleGuard`), retornando desvío seguro.

### Caso de Prueba CP-ACT-02: Detección y Notificación de Alerta de Frustración Docente (Regla CB-07)
* **Módulo:** Motor de Dificultad Adaptativa y Alertas en Tiempo Real (IN-16)
* **Nivel:** Integración y E2E
* **Precondición:** La docente titular se encuentra autenticada en el portal docente en su equipo portátil, con la vista "Mi Aula" activa. El estudiante se encuentra en la tablet resolviendo una actividad pedagógica.
* **Datos de Entrada:** Registro de 3 intentos fallidos consecutivos en la misma consigna interactiva.
* **Pasos de Ejecución:**
  1. El estudiante comete el Fallo 1 → El sistema ofrece refuerzo sonoro positivo de reintento.
  2. El estudiante comete el Fallo 2 → El sistema reduce alternativas distractoras.
  3. El estudiante comete el Fallo 3 consecutivo.
* **Resultado Esperado:**
  * En la tablet del alumno: La consigna se pausa sin ruidos punitivos ni calificaciones rojas, exhibiendo un pictograma tranquilizador de pausa y la locución: *"¡Hiciste un gran esfuerzo! Esperemos juntos a la docente"*.
  * En el servidor backend: Se valida la regla **CB-07**, registrando la sesión como `PausedForSupport` y despachando el evento `FrustrationAlertEvent` por el hub de SignalR.
  * En la pantalla de la docente: Aparece de inmediato una tarjeta destacada en color ámbar en el cuadrante del alumno indicando: *"Lucas Benítez requiere asistencia en Nivel 4 - Actividad 12"*, permitiendo apoyo pedagógico presencial.

### Caso de Prueba CP-A11Y-04: Auditoría Automatizada de Accesibilidad Universal (WCAG 2.1 AAA)
* **Módulo:** Motor de Accesibilidad Universal y Ergonomía Visual
* **Nivel:** Sistema / Calidad
* **Precondición:** Aplicación web compilada en modo optimizado de producción.
* **Comando de Ejecución:** `npx playwright test tests/frontend/accessibility.spec.ts`
* **Pasos de Ejecución:**
  1. La suite Playwright navega programáticamente por los 4 portales (Alumno, Docente, Familia, Administrador).
  2. Inyecta la librería de auditoría **axe-core** en cada vista renderizada.
  3. Evalúa etiquetas `aria-label`, contraste de color, estructura de encabezados semánticos (`<h1>`-`<h6>`), accesibilidad de formularios y foco navegable por teclado.
* **Resultado Esperado:**
  * 0 violaciones de severidad Crítica (*Critical*) o Grave (*Serious*).
  * 100 % de los elementos interactivos cuentan con indicador visible de foco navegable por pulsador o teclado (`:focus-visible`).
  * 100 % de los pictogramas ARASAAC e imágenes cuentan con atributo `alt` o `aria-hidden` según corresponda.

### Caso de Prueba CP-REP-05: Emisión de Informe Pedagógico y Validación de Cifrado Médico (Ley 25.326)
* **Módulo:** Informes Formales de Evolución Escolar (IN-15 / QuestPDF)
* **Nivel:** Integración y Base de Datos
* **Precondición:** El profesional responsable redactó el informe semestral del estudiante y la dirección institucional aprobó el documento en la cola Maker-Checker.
* **Datos de Entrada:** Petición `GET /api/reports/104/download-pdf` con token JWT de familiar autorizado.
* **Pasos de Ejecución:**
  1. El familiar solicita la descarga del informe desde su portal.
  2. El servidor compila los datos del estudiante y renderiza el documento vectorial con QuestPDF.
  3. Se inspecciona la base de datos PostgreSQL mediante consulta SQL directa:
     `SELECT "DiagnosisDetails" FROM "Diagnoses" WHERE "PersonId" = 10;`
* **Resultado Esperado:**
  * El cliente recibe un archivo PDF con formato institucional estandarizado, membrete oficial, firma digital y codificación UTF-8 íntegra.
  * En la base de datos, el contenido del diagnóstico clínico no es legible en texto plano; se comprueba el almacenamiento bajo el prefijo criptográfico `ENC:` con cifrado AES-256-GCM, garantizando el cumplimiento irrestricto de la Ley Nacional de Protección de Datos Personales N° 25.326.

---

## 10.9. Homologación de Campo y Dictamen Final de Calidad

Como fase de cierre previa a la presentación formal, la plataforma InclusiON fue sometida a una sesión de validación de campo (*User Acceptance Testing - UAT*) con **Candelaria Ferreyra** y **Catalina Vettorazzi**, docentes y referentes pedagógicas de educación especial.

Las evaluadoras auditaron la interfaz directamente en las tablets escolares, evaluando la claridad de los pictogramas, la calidez de las locuciones auditivas, la pertinencia de las alertas tempranas de frustración en el aula y la utilidad práctica del formato de informe semestral para su entrega a las familias. La homologación pedagógica resultó **100 % satisfactoria**, confirmando que el sistema se adapta a la realidad operativa del aula inclusiva.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                      DICTAMEN FINAL DE ASEGURAMIENTO DE CALIDAD                        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ ESTADO: APTO PARA PRODUCCIÓN Y DEFENSA DE PRÁCTICAS PROFESIONALIZANTES                 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ Sobre la base de las 433 verificaciones ejecutadas a lo largo de los cuatro niveles de │
│ la pirámide de pruebas, habiéndose alcanzado una tasa de aprobación final del 100.0 %, │
│ con cero defectos residuales abiertos, una cobertura de código del 88.4 % en lógica de │
│ dominio backend y 83.1 % en componentes frontend, y la certificación normativa de     │
│ Accesibilidad Universal bajo estándares WCAG 2.1 y 2.2 niveles AA y AAA mediante      │
│ axe-core y Google Lighthouse (100/100), el Equipo de Calidad y Procesos dictamina que │
│ la plataforma InclusiON se encuentra en estado ESTABLE, ROBUSTA, SEGURA Y CONFORME    │
│ para su implantación institucional y su defensa académica.                             │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ Mariano Decalli — Líder de QA y Procesos                                               │
│ Sacha Del Barrio — Especialista en Accesibilidad y QA Funcional                        │
│ Fernando Aparicio — Especialista en QA Técnico y Backend                               │
│ Germán Cochis — Especialista en QA Frontend y Mobile                                   │
│ Candelaria Ferreyra / Catalina Vettorazzi — Referentes Pedagógicas (Product Owners)    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```
