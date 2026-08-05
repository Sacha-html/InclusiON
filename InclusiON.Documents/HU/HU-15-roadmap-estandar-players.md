# HU-15 — Roadmap Estándar y Corrección de Players

| Campo | Contenido |
|---|---|
| ID | HU-15 |
| Épica | Plan de Trabajo (Roadmap) / Resolución de Actividades |
| Título | Roadmap estándar de 10 niveles con contenido real por tipo de juego |
| Prioridad | Crítica |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 10 |
| Estado | Completada |
| Códigos Jira | IN-200, IN-201, IN-202, IN-203, IN-204, IN-205 |

---

## Historia de Usuario

**Como** alumno nuevo en el sistema
**Quiero** que al registrarme reciba automáticamente un plan de actividades de 10 niveles listos para jugar
**Para** poder empezar a practicar sin que el profesional tenga que configurar nada manualmente

---

## Descripción funcional

Cuando el profesional registra a un alumno nuevo, el sistema ejecuta `RoadmapInitializer` que:

1. Crea un `PersonRoadmap` con un área de Comunicación
2. Agrega 10 `PersonRoadmapActivity` ordenadas secuencialmente
3. Desbloquea el Nivel 1 automáticamente (`IsUnlocked = true`)
4. Cada actividad ya tiene su `ContentJson` completo y listo para el player del frontend

Al iniciar el servidor (arranque), `DatabaseSeeder.PatchStandardActivitiesContentAsync` verifica si alguna actividad existente tiene `ContentJson = '{}'` y la actualiza automáticamente, sin afectar actividades ya con contenido.

### Los 10 niveles estándar

| Nivel | Actividad | Tipo de Player |
|-------|-----------|----------------|
| 1 | Señala el animal | OPTION_SELECT |
| 2 | Ordena la secuencia | ORDER_SEQUENCE |
| 3 | Muchos o pocos | OPTION_SELECT |
| 4 | Encuentra el igual | PICTOGRAM_SELECT |
| 5 | ¿Qué viene después? | ORDER_SEQUENCE |
| 6 | Conciencia fonológica | OPTION_SELECT |
| 7 | Clasifica por categoría | CLASSIFY |
| 8 | Une la imagen con la palabra | MATCH_IMAGE_WORD |
| 9 | Lectura global | GLOBAL_READING |
| 10 | Encuentra el intruso | OPTION_SELECT |

---

## Criterios de Aceptación

- [x] Al crear un alumno nuevo, se genera automáticamente un roadmap de 10 niveles
- [x] El Nivel 1 está desbloqueado; los niveles 2-10 están bloqueados
- [x] Cada actividad tiene `ContentJson` con datos reales (no `'{}'`)
- [x] El tipo de player de cada actividad coincide con uno implementado en el `PLAYER_REGISTRY` del frontend
- [x] Al completar el Nivel N con ≥60% de éxito, el Nivel N+1 se desbloquea automáticamente
- [x] Alumnos existentes con `ContentJson = '{}'` reciben el contenido correcto en el próximo arranque del servidor
- [x] El parche es idempotente: si ya tiene contenido, no lo sobreescribe
- [x] Si un tipo de actividad no tiene player implementado (ej: `SOUND_RECOGNITION`), el sistema NO asigna ese tipo en el roadmap estándar

## Casos de Borde

| Caso | Comportamiento esperado |
|------|------------------------|
| Alumno ya tenía roadmap al arrancar | El parche no lo modifica; solo actúa sobre `ContentJson = '{}'` |
| `ContentJson` ya tiene datos válidos | No se sobreescribe |
| Tipo de actividad sin player (SOUND_RECOGNITION) | No aparece en el roadmap estándar; se usa OPTION_SELECT como fallback |
| Alumno completa nivel con < 60% de éxito | El siguiente nivel permanece bloqueado; puede reintentar |
| Servidor se reinicia múltiples veces | El parche se ejecuta pero no modifica nada (idempotente) |
| Alumno completa el Nivel 10 | No se desbloquea ningún nivel adicional; el roadmap muestra estado "completado" |

---

## Fix complementario — `withViewTransitions()` en desarrollo (IN-205)

**Problema:** Angular 20 lanza `InvalidStateError: Failed to execute startViewTransition` al combinar `withViewTransitions()` con Hot Module Replacement (HMR) en modo desarrollo — ambos intentan controlar el ciclo de rendering simultáneamente.

**Solución:** Se deshabilita `withViewTransitions()` en entornos `!environment.production` dentro de `app.config.ts`. En producción se mantiene activo.

---

## Implementación técnica

**Backend:**
- `InclusiON.Infrastructure/Services/RoadmapInitializer.cs` — inicializa roadmap al crear alumno
- `InclusiON.Data/Seeders/DatabaseSeeder.cs` — método `PatchStandardActivitiesContentAsync`

**Frontend:**
- `app.config.ts` — `withViewTransitions()` deshabilitado en dev
- `ActivityPlayerShellComponent` — usa `PLAYER_REGISTRY` para resolver el componente por `TemplateTypeCode`
