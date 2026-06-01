# Automatizaciones del Sistema — InclusiON

**Encuentro 5 — Evidencia de procesamiento automático**  
**Práctica Profesionalizante II — Institución Cervantes**

> Este documento lista los procesos que el sistema ejecuta **automáticamente**, sin intervención manual del usuario. Cada uno demuestra que el sistema toma decisiones de negocio, no solo guarda datos.

---

## 1. Motor de Dificultad Adaptativa (MDA)

**Descripción:** Después de cada intento completado, el sistema analiza el desempeño de la persona y ajusta automáticamente el nivel de dificultad y el tiempo límite de la actividad.

**Cuándo se activa:** Al recibir `POST /api/activity-assignments/{id}/responses/{responseId}/complete`

**Qué decide el sistema:**

| Condición | Acción automática |
|---|---|
| `consecutiveSuccess >= consecutiveSuccessToUpgrade` | Sube `difficultyLevel` en 1 (dentro del rango max configurado) |
| `consecutiveFails >= consecutiveFailuresToDowngrade` | Baja `difficultyLevel` en 1 (sin bajar del mínimo configurado) |
| `frustrationLevel >= frustrationThreshold` | Baja dificultad agresivamente y genera alerta al profesional |
| Siempre | Registra el ajuste en `AdaptiveAdjustmentLog` con antes/después y motivo |

**Por qué es relevante:** El sistema no solo guarda un resultado — analiza el patrón de respuestas y toma una decisión clínica automatizada sobre cómo debe continuar el tratamiento.

**Historia de usuario origen:** [HU-10](../HU/HU-10-motor-adaptativo.md)  
**Endpoint del pipeline:** `CompleteActivityResponseCommandHandler.cs` → `AdaptiveEngineService`

---

## 2. Desbloqueo Automático de Actividades (Roadmap)

**Descripción:** Al completar una actividad del roadmap, el sistema evalúa si el porcentaje de éxito supera el umbral definido. Si lo supera, la siguiente actividad en la secuencia se desbloquea automáticamente sin que el profesional intervenga.

**Cuándo se activa:** Al finalizar un intento exitoso

**Qué decide el sistema:**
- Calcula `successPercentage` del intento
- Compara con `PersonRoadmapActivity.UnlockThresholdPercent` de la siguiente actividad
- Si `successPercentage >= unlockThresholdPercent` → `nextActivity.IsUnlocked = true`, registra `UnlockedAt`

**Por qué es relevante:** El sistema gestiona la progresión del plan terapéutico de forma autónoma — el profesional diseña la secuencia, el sistema decide cuándo avanzar.

**Historia de usuario origen:** [HU-05](../HU/HU-05-roadmap.md)

---

## 3. Suspensión Automática de Profesionales Inactivos

**Descripción:** Un proceso automático detecta profesionales que no iniciaron sesión en más de 90 días y cambia su estado a `Suspended`, revocando el acceso.

**Cuándo se activa:** Job programado — `POST /api/professionals/suspend-inactive`

**Qué decide el sistema:**
- Consulta todos los profesionales con `Status = Approved`
- Compara `User.LastLoginDate` con la fecha actual
- Si la diferencia > 90 días → `Status = Suspended`, registra en `ProfessionalStatusHistory`

**Por qué es relevante:** El sistema aplica una política de seguridad de forma automática, sin necesidad de que el admin revise manualmente cada cuenta.

**Historia de usuario origen:** [HU-11](../HU/HU-11-gestion-usuarios.md)

---

## 4. Generación de Embedding Semántico de Actividades

**Descripción:** Al crear o editar una actividad, el sistema genera automáticamente un vector semántico usando IA (pgvector + modelo de embeddings). Este vector se usa para la búsqueda inteligente por similitud.

**Cuándo se activa:** Al crear (`POST /api/activities`) o editar (`PUT /api/activities/{id}`) una actividad

**Qué hace el sistema:**
- Extrae el texto descriptivo de la actividad
- Genera un vector de 1536 dimensiones usando el modelo de embeddings
- Persiste el vector en `ActivityEmbedding` (1:1 con Activity)
- Las búsquedas posteriores (`GET /api/activities/search?text=...`) usan distancia coseno para encontrar actividades similares

**Por qué es relevante:** El sistema transforma contenido educativo en conocimiento estructurado que permite recomendaciones inteligentes.

**Historia de usuario origen:** [HU-10](../HU/HU-10-motor-adaptativo.md) · [HU-02](../HU/HU-02-actividades-templates.md)

---

## 5. Cambio de Contraseña Obligatorio en Primer Login

**Descripción:** Cuando un admin crea un profesional o familiar, se genera una contraseña temporal. En el primer login, el sistema detecta `MustChangePassword = true` y fuerza el cambio antes de permitir cualquier otra acción.

**Cuándo se activa:** En cualquier request autenticado si `User.MustChangePassword = true`

**Qué hace el sistema:**
- El middleware de autenticación verifica el flag en el JWT
- Si está activo, rechaza cualquier endpoint que no sea `PUT /api/auth/change-password`
- Al cambiar la contraseña con éxito, el flag se desactiva automáticamente

**Por qué es relevante:** El sistema impone una política de seguridad de credenciales sin depender de que el usuario recuerde hacerlo.

**Historia de usuario origen:** [HU-01](../HU/HU-01-catalogos-configuracion.md)

---

## Resumen para demostración en Encuentro 5

| # | Automatización | Demo posible | Relevancia clínica |
|---|---|---|---|
| 1 | Motor Adaptativo | ✅ Completar actividad → ver `AdaptiveAdjustmentLog` | Alta — ajusta tratamiento |
| 2 | Desbloqueo automático roadmap | ✅ Completar con ≥ umbral → siguiente actividad desbloqueada | Alta — progresión terapéutica |
| 3 | Suspensión por inactividad | ✅ Llamar al job → profesional queda Suspended | Media — seguridad |
| 4 | Embedding semántico | ✅ Crear actividad → buscar similar → resultado relevante | Media — eficiencia profesional |
| 5 | Primer login forzado | ✅ Login con cuenta nueva → sistema fuerza cambio | Media — seguridad |

**Automatización recomendada para demostrar al docente:** Motor Adaptativo (#1) — demuestra que el sistema toma decisiones clínicas basadas en datos reales del paciente.
