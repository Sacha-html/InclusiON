# Módulo 6 — Trayectoria Pedagógica (Roadmap)

Este módulo describe la trayectoria de aprendizaje adaptativa ("Mi Camino"). A diferencia de un armado manual en blanco, **el Roadmap nace a partir de una Plantilla Oficial Predeterminada de 10 niveles/hitos pedagógicos** graduados bajo el Diseño Universal para el Aprendizaje (DUA) y principios anti-frustración, la cual se inicializa de forma automática y puede ser supervisada, enriquecida y calibrada por el equipo docente.

---

## CU-23: Inicialización Automática y Supervisión de la Trayectoria (Roadmap)

| Campo | Detalle |
|---|---|
| **Actor principal** | Sistema (inicialización) / Profesional Docente (supervisión y notas) |
| **Actores secundarios** | Estudiante (receptor) |
| **HU de referencia** | HU-05 / HU-IN-110–111 |
| **Prioridad** | Crítica |
| **Precondiciones** | El estudiante se encuentra registrado formalmente y asignado a un profesional o aula institucional. |

**Flujo principal**
1. Al momento de formalizar la asignación del estudiante, el sistema inicializa automáticamente su Roadmap oficial basándose en la **Plantilla Estandarizada DUA de 10 Actividades**:
   - Nivel 1: *Armando el vaso* (Motricidad y Coordinación - Rompecabezas).
   - Nivel 2: *Necesito ir al baño* (Comunicación y Lenguaje - Selección pictográfica CAA).
   - Nivel 3: *¿Quién se ríe?* (Habilidades Socioemocionales - Discriminación de emociones).
   - Nivel 4: *Buscando la MESA* (Lectoescritura - Lectura global).
   - Nivel 5: *Contando galletas* (Numeración y Matemática - Conteo visual asistido).
   - Nivel 6: *Me sirvo agua* (Autonomía y Vida Diaria - Secuencia de pasos).
   - Nivel 7: *¿Dónde hacemos pis?* (Habilidades Socioemocionales - Comprensión y contexto).
   - Nivel 8: *A ordenar la cocina* (Numeración y Matemática - Clasificación por categorías).
   - Nivel 9: *Las letras de MESA* (Lectoescritura - Conciencia fonológica).
   - Nivel 10: *La rutina de la mañana* (Autonomía y Vida Diaria - Secuencia temporal completa).
2. El sistema configura automáticamente el **Nivel 1 como Desbloqueado**, generando su correspondiente asignación de actividad en estado `Pendiente`.
3. Los niveles subsiguientes (2 al 10) quedan vinculados en secuencia con umbral de avance automático.
4. El Profesional accede a la ficha del estudiante y selecciona la solapa **"Roadmap"**.
5. El sistema despliega el mapa completo de la trayectoria, detallando el estado de cada nodo (Desbloqueado, Pendiente, Completado), las métricas del radar de habilidades y las notas clínicas.
6. El docente puede registrar y actualizar anotaciones pedagógicas globales sobre el desempeño del alumno.

**Postcondiciones**
- El alumno cuenta de inmediato con su sendero de aprendizaje activo en el portal adaptativo (`/app/roadmap`), sin requerir configuración manual previa del docente.
- El profesional dispone del panel de control para dar seguimiento al progreso.

---

## CU-24: Incorporar o Personalizar Actividades en la Trayectoria

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 / HU-IN-113–114 |
| **Prioridad** | Alta |
| **Precondiciones** | El estudiante cuenta con su Roadmap inicializado. |

**Flujo principal**
1. El Profesional accede a la pestaña Roadmap del estudiante y selecciona el área curricular a enriquecer.
2. Pulsa la opción **"Agregar actividad"**.
3. El sistema lista el catálogo de actividades disponibles (tanto estándar oficiales como plantillas personalizadas creadas por el docente).
4. El docente selecciona la actividad deseada y calibra los parámetros didácticos:
   - Orden secuencial en el área.
   - Umbral de desbloqueo exigido (porcentaje mínimo de precisión para habilitar la siguiente).
   - Límite de tiempo opcional e intentos máximos.
   - Nivel de dificultad y habilitación de pistas visuales/auditivas.
5. El sistema incorpora el nuevo nodo al final de la secuencia del área correspondiente.

**Flujos alternativos**
- **4a. Actividad ya presente en el área:** El sistema advierte la duplicidad y bloquea la inclusión repetida en la misma secuencia.

**Postcondiciones**
- La nueva actividad se integra armónicamente a la trayectoria del estudiante respetando la jerarquía secuencial configurada.

---

## CU-25: Reordenar Secuencia Pedagógica de Actividades

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 / HU-IN-115 |
| **Prioridad** | Media |
| **Precondiciones** | El área contiene múltiples actividades y las que se desean reorganizar **no registran respuestas ni sesiones evaluadas**. |

**Flujo principal**
1. El Profesional ingresa a la gestión secuencial de actividades del área.
2. Reorganiza los nodos pendientes según la prioridad pedagógica del periodo (ej. adelantar pautas de autonomía antes que lectoescritura).
3. El sistema valida que las actividades modificadas no contengan intentos ya calificados para no alterar la integridad del historial evolutivo.
4. El sistema recalcula la secuencia numérica y actualiza el orden visual de los nodos.

**Flujos alternativos**
- **2a. Intento de reordenar actividad ya realizada:** El sistema advierte: *"No es posible modificar el orden de actividades que ya cuentan con sesiones finalizadas por el estudiante"*.

**Postcondiciones**
- El nuevo orden pedagógico impacta en tiempo real en la pantalla de "Mi Camino" del estudiante.

---

## CU-26: Desbloqueo Pedagógico Manual de Actividad

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente |
| **Actores secundarios** | Sistema |
| **HU de referencia** | HU-05 / HU-IN-112 |
| **Prioridad** | Alta |
| **Precondiciones** | La actividad se encuentra en estado `Bloqueado` y el profesional tiene asignación activa con el alumno. |

**Flujo principal**
1. El Profesional detecta en el Roadmap una actividad bloqueada que considera pertinente habilitar (por ejemplo, porque el alumno consolidó ese aprendizaje en el aula física o para motivar su participación).
2. Selecciona el nodo y pulsa **"Desbloquear manualmente"**.
3. El sistema omite el requisito del umbral automático previo y genera inmediatamente la asignación de actividad (`ActivityAssignment`) para el estudiante.
4. El nodo pasa a estado **`Desbloqueado / Disponible`** en el portal del alumno.

**Postcondiciones**
- El estudiante puede acceder e interactuar con la actividad inmediatamente desde su tablet o dispositivo.

---

## CU-27: Excluir o Desactivar Actividad de la Trayectoria

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente |
| **Actores secundarios** | — |
| **HU de referencia** | HU-05 / HU-IN-113 |
| **Prioridad** | Media |
| **Precondiciones** | La actividad no cuenta con sesiones finalizadas en la trayectoria del alumno. |

**Flujo principal**
1. Si una actividad de la plantilla oficial no es adecuada para el perfil funcional del estudiante (por ejemplo, discriminación sonora en un alumno con hipoacusia severa), el docente selecciona el nodo en el Roadmap.
2. Elige la opción **"Eliminar del roadmap"**.
3. El sistema constata que no existan métricas de ejecución registradas.
4. El sistema retira la actividad de la trayectoria personal del estudiante y renumera automáticamente las actividades restantes del área.

**Flujos alternativos**
- **3a. Actividad con métricas históricas:** El sistema bloquea el borrado para preservar el legajo pedagógico y sugiere conservarla como completada o desactivar asignaciones pendientes.

**Postcondiciones**
- El nodo desaparece del camino del estudiante sin discontinuar la progresión del resto de los niveles.

---

## CU-28: Recorrer "Mi Camino" e Iniciar Actividades (Portal del Alumno)

| Campo | Detalle |
|---|---|
| **Actor principal** | Estudiante (Persona con Discapacidad) |
| **Actores secundarios** | Sistema (reproductor de actividad y refuerzo positivo) |
| **HU de referencia** | HU-05 / HU-IN-117 |
| **Prioridad** | Crítica |
| **Precondiciones** | El estudiante se encuentra autenticado en el entorno adaptativo (`/app` o `/person`). |

**Flujo principal**
1. El estudiante accede a la vista principal **"Mi Camino"** (`/app/roadmap`).
2. El sistema renderiza un sendero interactivo gamificado (estilo Duolingo) con accesibilidad universal:
   - **Nodo Completado:** Muestra tilde verde (✓), estrellas de dificultad y porcentaje de logro obtenido.
   - **Nodo Desbloqueado:** Destacado con pulso visual brillante y señalización clara, listo para iniciar.
   - **Nodo Bloqueado:** Candado con dificultad graduada, indicando que se habilitará al completar el paso previo.
3. El estudiante toca el nodo desbloqueado.
4. El sistema inicia de forma inmediata el reproductor adaptativo de la actividad correspondiente.
5. Al completar la sesión, el sistema evalúa el porcentaje de éxito frente al umbral; si es superado, desbloquea de inmediato el siguiente nivel mostrando una animación de celebración con refuerzo positivo contingente.

**Postcondiciones**
- Las métricas de tiempo, aciertos y nivel GAS se registran en la bitácora del estudiante.
- El avance queda reflejado tanto en su vista como en el panel de control del docente y la familia.
