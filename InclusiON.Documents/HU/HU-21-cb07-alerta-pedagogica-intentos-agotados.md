# HU-21 — Caso Borde CB-07: Alerta Pedagógica por Agotamiento de Intentos, Impacto en Dashboard y Acciones desde la Campana

| Campo | Contenido |
|---|---|
| **ID Historia / Issue** | HU-21 (Jira: `IN-335`) |
| **Tipo de Issue** | Historia de Usuario / Regla de Negocio Pedagógica |
| **Épica** | Motor Adaptativo (MDA), Gamificación de Trayectoria & Monitoreo Docente |
| **Título** | CB-07: Gestión de Agotamiento de Intentos (4 errores) con Aviso Pedagógico, Notificación en Tiempo Real, Impacto en Dashboard y Acciones Rápidas en Campana |
| **Prioridad** | Alta |
| **Estimación** | 8 Story Points |
| **Estado** | Ready for Sprint / Backlog |
| **Componentes** | `InclusiON.Server` (API / Handlers / SignalR / BackgroundJobs), `InclusiON.Client` (AAC Players / NotificationBell / Dashboard) |

---

## 1. Descripción de la Historia de Usuario

**Como** docente / profesional a cargo del seguimiento del alumno  
**Quiero** que cuando un estudiante agote sus intentos permitidos (4 errores consecutivos o `MaxAttempts` alcanzados sin superar el puntaje mínimo de aprobación) el sistema active un aviso pedagógico en lugar de penalizarlo o bloquear su trayectoria indefinidamente,  
**Para** recibir una alerta en tiempo real en mi campana de notificaciones, visualizar el impacto en el contador de frustración del Dashboard y poder intervenir presencialmente decidiendo si reactivar sus intentos, editar la dificultad/pistas de la actividad o darla de baja.

---

## 2. Contexto Pedagógico y Caso Borde (CB-07)

* **Enfoque Anti-Punitivo:** InclusiON no califica con notas rojas ni bloquea permanentemente la trayectoria del alumno.
* **Problema Detectado en Pruebas Manuales:** Al fallar reiteradamente en un ejercicio del Roadmap/Player, no se dispara ningún evento; el alumno queda sin retroalimentación de apoyo, la docente no se entera, la campana de notificaciones no suena/no muestra alertas, el contador del Dashboard permanece en 0 y no existen atajos de resolución rápida.

---

## 3. Criterios de Aceptación (Gherkin & Checklist)

### CA-01: Detección y Contención en el Reproductor del Alumno (4 Errores / Intentos Agotados)
- [ ] **Escenario 1.1: Agotamiento de intentos sin castigo**
  - **Dado** que un alumno está realizando un ejercicio con límite de intentos (`MaxAttempts = 4` o umbral de 4 fallos consecutivos)
  - **Cuando** comete el 4.° error sin alcanzar el porcentaje mínimo de aprobación (ej. 60%)
  - **Entonces** el reproductor finaliza amigablemente mostrando un mensaje contenedor y motivacional (ej. *"¡Hiciste un gran esfuerzo! Tu profe se acercará a ayudarte"*).
  - **Y** el mapa/actividad NO muestra candados punitivos ni mensajes de reprobación ("nota roja").
  - **Y** se envía al backend el reporte de intento indicando `result = 'Fallido'`, `frustrationLevel = 4` y `errorCount = 4`.

---

### CA-02: Emisión y Recepción de la Alerta Pedagógica en Tiempo Real (SignalR / Push)
- [ ] **Escenario 2.1: Notificación instantánea a la docente a cargo**
  - **Dado** que el backend recibe la confirmación del agotamiento de intentos del alumno
  - **Cuando** el handler procesa la respuesta
  - **Entonces** se encola y despacha un evento en tiempo real vía SignalR (`NotificationPayload`) al profesional vinculado al alumno.
  - **Y** la campana de notificaciones (`NotificationBellComponent`) en la pantalla de la docente:
    1. Incrementa de inmediato el badge de no leídas (+1).
    2. Muestra un toast emergente: *"Alerta pedagógica: [Nombre Alumno] necesita apoyo en [Nombre Actividad]"*.
    3. Agrega el elemento al listado de la campana con ícono de advertencia/apoyo y metadatos (`assignmentId`, `activityId`, `studentId`, tipo `'frustration_support'`).

---

### CA-03: Impacto en las Métricas y Dashboard Docente
- [ ] **Escenario 3.1: Actualización del contador de Alertas de Frustración**
  - **Dado** que la docente ingresa a su Dashboard (`/pro/dashboard`)
  - **Cuando** se consulta el resumen semanal o la métrica del aula
  - **Entonces** la tarjeta **"Alertas de frustración"** refleja el nuevo caso sumando +1.
- [ ] **Escenario 3.2: Registro en el Modal de Detalle de Frustración**
  - **Dado** que la docente hace clic en *"Ver detalle"* de la tarjeta de alertas de frustración
  - **Cuando** se abre el modal (`openFrustrationDetails()`)
  - **Entonces** se lista la fila correspondiente con:
    - Nombre del alumno
    - Nombre y categoría de la actividad
    - Cantidad de errores (>= 4)
    - Motivo: *"Agotamiento de intentos sin alcanzar puntaje mínimo (CB-07)"*
    - Fecha y hora exacta de la incidencia

---

### CA-04: Menú de Acciones Rápidas al Cliquear la Notificación en la Campana
- [ ] **Escenario 4.1: Despliegue de opciones contextuales de intervención**
  - **Dado** que la docente tiene la notificación de alerta de apoyo en el dropdown de la campana
  - **Cuando** hace clic sobre dicha notificación
  - **Entonces** el sistema NO realiza una navegación genérica pasiva a `/pro/persons`, sino que abre inmediatamente un modal o diálogo interactivo de **"Intervención Pedagógica"** enfocado en ese alumno y actividad.
- [ ] **Escenario 4.2: Ejecución de las 3 opciones de resolución**
  - El modal presenta tres acciones claras y accesibles:
    1. **Reactivar intentos:** Restablece el contador de intentos de la asignación (`MaxAttempts` renovados o nuevo intento limpio habilitado) para que el alumno pueda reintentar con el apoyo presencial de la docente.
    2. **Editar actividad / configuración:** Abre directamente la edición de parámetros de la actividad en el Roadmap (ajustar nivel de dificultad, habilitar pistas visuales `showHints = true`, modificar tiempo límite o reducir umbral de aprobación).
    3. **Dar de baja / desasignar:** Permite desasignar o marcar la actividad como eximida/cancelada para que el alumno no quede bloqueado y pueda continuar con otra área de su plan de aprendizaje.
  - **Y** tras seleccionar y confirmar cualquiera de las 3 opciones, la notificación se marca como atendida/resuelta y se actualiza el estado en el Dashboard y Roadmap.

---

## 4. Diseño Técnico e Implementación Sugerida

### Backend (`InclusiON.Server`)
1. **`CompleteActivityResponseCommandHandler.cs`**:
   - Evaluar si la actividad agotó los intentos (`MaxAttempts` alcanzado o 4 errores registrados).
   - Crear el registro correspondiente en `ActivitySessions` asociando `StudentId`, `ActivityId`, `ErrorCount = 4`, `SuccessRate` y `DateCompleted`.
   - Encolar job de notificación `JobTypes.Push` con `NotificationPayload`:
     ```json
     {
       "UserId": "guid-profesional",
       "Title": "Alerta de apoyo pedagógico",
       "Message": "[Alumno] completó sus intentos en [Actividad] y requiere intervención.",
       "ActionUrl": "/#/pro/persons?action=intervene&assignmentId=...&studentId=...",
       "Type": "frustration_support"
     }
     ```
2. **`AnalyticsController.cs` & `GetWeeklyProgressQueryHandler.cs`**:
   - Asegurar que la sesión con 4 errores sea contabilizada en `GetFrustrationDetails` y en `FrustrationAlerts`.

### Frontend (`InclusiON.Client`)
1. **`player-base.component.ts`**:
   - Monitorear la cantidad de errores cometidos durante la sesión del ejercicio.
   - Si se alcanzan 4 fallos o se agota el límite, computar `frustrationLevel: 4` y pasar `errorCount: 4` en el payload de `completeResponse`.
2. **`notification-bell.component.ts`**:
   - Detectar notificaciones con `type === 'frustration_support'` o parámetros de intervención.
   - En `onNotificationClick(notif)`: disparar la apertura del modal contextual `PedagogicalInterventionModalComponent` en lugar de sólo navegar.
3. **Nuevo Componente `PedagogicalInterventionModalComponent`**:
   - Exponer botones:
     - `[Reactivar Intentos]` → llama a `activitiesService.reactivateAssignment(id)` o `resetAttempts()`.
     - `[Editar Actividad]` → redirige a la pestaña Roadmap del alumno con el modal de configuración de la actividad abierto.
     - `[Dar de Baja]` → invoca la baja/cancelación de la asignación.

---

## 5. Criterios de Prueba (Definition of Done - DoD)
- [ ] Pruebas unitarias en Backend para `CompleteActivityResponseCommandHandler` ante 4 errores.
- [ ] Pruebas unitarias en Frontend para `NotificationBellComponent` verificando el evento clic y apertura del modal.
- [ ] Validación E2E manual: Alumno falla 4 veces → Llega notificación SignalR en tiempo real a la campana del docente → Sube contador de frustración en Dashboard → Clic en campana despliega [Reactivar] [Editar] [Dar de baja] funcionando correctamente.
