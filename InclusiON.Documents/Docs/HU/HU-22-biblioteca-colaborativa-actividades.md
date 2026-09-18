# HU-22 — Biblioteca Colaborativa de Actividades Multi-Especialidad con Adaptación Local (Anti-Duplicación) y Filtro Propias / Compartidas / Editadas

| Campo | Contenido |
|---|---|
| **ID Historia / Issue** | HU-22 (Jira: `IN-340`) |
| **Tipo de Issue** | Historia de Usuario / Regla de Negocio Pedagógica |
| **Épica** | Banco de Recursos Pedagógicos, Red Colaborativa & Gamificación |
| **Título** | Biblioteca Colaborativa de Actividades de Profesionales: Adaptación Curricular sin Polución Comunitaria (Anti-Duplicación) y Filtros Propias / Compartidas / Editadas |
| **Prioridad** | Alta |
| **Estimación** | 8 Story Points |
| **Estado** | Ready for Sprint / Backlog |
| **Componentes** | `InclusiON.Server` (API / Handlers / Repositories), `InclusiON.Client` (Professional Activities / Filters / Modals) |

---

## 1. Descripción de la Historia de Usuario

**Como** profesional de inclusión (docente, psicopedagogo, terapeuta ocupacional o fonoaudiólogo)  
**Quiero** disponer de una biblioteca colaborativa donde se compartan todas las actividades creadas por cualquier profesional sin importar su especialidad, y poder tomar cualquier actividad para editarla y adaptarla a las necesidades de mis alumnos,  
**Para** enriquecer mi práctica pedagógica colaborando con la comunidad, garantizando que si adapto una actividad ajena quede guardada únicamente en mi listado personal (evitando ver la misma actividad mil veces duplicada en la biblioteca compartida), y pudiendo filtrar con precisión entre actividades **Propias**, **Compartidas** y **Editadas**.

---

## 2. Reglas de Negocio Pedagógicas y de Plataforma

### RN-01: Colaboración Abierta Multi-Especialidad
* Toda actividad creada desde cero por cualquier profesional de la plataforma (sea docente de grado, DAI, psicopedagogo, fonoaudiólogo, etc.) es visible y utilizable por cualquier otro colega de InclusiON.
* No existen barreras por especialidad: la inclusión y estimulación cognitiva es interdisciplinaria.

### RN-02: Exclusión Tajante de los 10 Niveles del Roadmap
* Las 10 actividades base del Roadmap oficial (`RoadmapOrder != null`, pertenecientes al área *Trayectoria* y al recorrido gamificado *"Mi Camino"*) **NO forman parte de esta biblioteca**.
* La biblioteca comunitaria contiene **exclusivamente** actividades diseñadas por profesionales.

### RN-03: Adaptación Local Anti-Polución (Regla de No Duplicación Comunitaria)
* Cuando el Profesional B encuentra una actividad compartida creada por el Profesional A, puede:
  1. **Asignarla directamente** a sus estudiantes sin modificaciones.
  2. **Editarla para adaptarla** (adecuación curricular: modificar consignas, cambiar pictogramas, ajustar tiempo o complejidad).
* **Regla Crítica:** Al guardar la versión adaptada/editada:
  - **NO se publica en el listado compartido comunitario**.
  - Queda guardada **exclusivamente en el listado privado del profesional que la editó** (Profesional B).
  - La actividad original del Profesional A permanece intacta en el listado compartido para toda la comunidad.
  - **Objetivo:** Evitar la polución del catálogo comunitario (impedir que la misma actividad aparezca repetida decenas o cientos de veces por pequeñas variaciones).

### RN-04: Triple Filtro de Origen en la Gestión de Actividades
El listado de actividades del profesional contará con un selector de origen con 3 opciones excluyentes:
1. **`Propias`**: Actividades creadas originalmente desde cero por el profesional logueado. Estas son las que se comparten a la comunidad.
2. **`Compartidas`**: Actividades originales creadas por otros profesionales de InclusiON disponibles para utilizar o adaptar.
3. **`Editadas`**: Actividades que fueron tomadas de otros profesionales y adaptadas por el profesional logueado. Visibles únicamente para él.

---

## 3. Criterios de Aceptación (Gherkin & Checklist)

### CA-01: Filtro de Origen Triple en la Interfaz Docente
- [ ] **Escenario 1.1: Visualización del selector de origen**
  - **Dado** que el profesional ingresa a la sección de Actividades (`/pro/activities`)
  - **Cuando** observa la barra de filtros
  - **Entonces** visualiza el selector de origen con las opciones: `Todas`, `Propias`, `Compartidas` y `Editadas`.
- [ ] **Escenario 1.2: Filtrado por "Propias"**
  - **Dado** que se selecciona el filtro `Propias`
  - **Entonces** se muestran únicamente las actividades creadas por el profesional en sesión que no sean adaptaciones de terceros (`ProfessionalId == Me` y `OriginalActivityId == null`).
- [ ] **Escenario 1.3: Filtrado por "Compartidas"**
  - **Dado** que se selecciona el filtro `Compartidas`
  - **Entonces** se muestran las actividades originales creadas por otros profesionales (`ProfessionalId != Me` y `OriginalActivityId == null`).
  - **Y** se muestra claramente el nombre del colega autor y su especialidad.
- [ ] **Escenario 1.4: Filtrado por "Editadas"**
  - **Dado** que se selecciona el filtro `Editadas`
  - **Entonces** se muestran únicamente las actividades que el profesional actual tomó de un colega y adaptó (`ProfessionalId == Me` y `OriginalActivityId != null`).

---

### CA-02: Edición y Adaptación sin Polución Comunitaria (Anti-Spam)
- [ ] **Escenario 2.1: Profesional adapta actividad de un colega**
  - **Dado** que el Profesional B visualiza una actividad compartida del Profesional A
  - **Cuando** hace clic en la acción **"Editar / Adaptar"**
  - **Entonces** el sistema abre el formulario de edición precargado con el contenido original
  - **Y** al presionar **"Guardar Adaptación"**:
    1. Se crea un nuevo registro de `Activity` asignado al Profesional B.
    2. Se vincula `OriginalActivityId = IdDeLaActividadDeA`.
    3. Esta nueva actividad **NO tiene visibilidad comunitaria** (`IsCommunityShared = false`).
    4. Aparece de inmediato bajo el filtro **"Editadas"** del Profesional B.
    5. La actividad original del Profesional A sigue figurando en **"Compartidas"** sin sufrir cambios.
- [ ] **Escenario 2.2: Ausencia de duplicados en "Compartidas"**
  - **Dado** que 10 profesionales distintos editan y adaptan la misma actividad creada por el Profesional A
  - **Cuando** cualquier usuario consulta la sección **"Compartidas"**
  - **Entonces** la actividad se visualiza **exactamente 1 sola vez** (la versión original de A).
  - **Y** ninguna de las 10 adaptaciones privadas aparece en el listado compartido.

---

### CA-03: Exclusión Absoluta de Niveles del Roadmap
- [ ] **Escenario 3.1: Validación de exclusión de los 10 niveles**
  - **Dado** que en la base de datos existen las 10 actividades del Roadmap oficial (`RoadmapOrder` del 1 al 10)
  - **Cuando** se consulta cualquiera de los filtros (`Propias`, `Compartidas`, `Editadas` o `Todas`)
  - **Entonces** ninguna actividad con `RoadmapOrder IS NOT NULL` es devuelta en la respuesta.

---

### CA-04: Asignación Directa y Prueba Previa
- [ ] **Escenario 4.1: Asignación sin necesidad de editar**
  - **Dado** que una actividad compartida satisface plenamente las necesidades de un alumno
  - **Cuando** el profesional presiona **"Asignar"**
  - **Entonces** puede asignarla directamente a su estudiante o grupo sin requerir adaptarla ni clonarla.
- [ ] **Escenario 4.2: Previsualización en vivo**
  - **Dado** que el profesional desea evaluar una actividad compartida
  - **Cuando** presiona **"Previsualizar"**
  - **Entonces** se lanza el reproductor en modo demo sin registrar puntuaciones ni frustración en el historial del alumno.

---

## 4. Diseño Técnico e Implementación Sugerida

### Backend (`InclusiON.Server`)

1. **Modelo de Datos (`Activity.cs`)**:
   ```csharp
   // Identificador de la actividad original de la cual se deriva la adaptación
   public int? OriginalActivityId { get; set; }
   public virtual Activity? OriginalActivity { get; set; }
   ```
2. **Consultas en Repositorio (`ActivitiesRepository.cs`)**:
   ```csharp
   // Exclusión obligatoria de niveles del roadmap
   query = query.Where(a => a.RoadmapOrder == null && !a.IsStandardActivity);

   switch (originFilter?.ToLowerInvariant())
   {
       case "propias":
           // Creadas por mí desde cero
           query = query.Where(a => a.ProfessionalId == currentProfId && a.OriginalActivityId == null);
           break;

       case "compartidas":
           // Creadas por otros colegas (solo originales, nunca adaptaciones)
           query = query.Where(a => a.ProfessionalId != currentProfId && a.OriginalActivityId == null && a.IsActive);
           break;

       case "editadas":
           // Adaptadas por mí a partir de la creación de otro profesional
           query = query.Where(a => a.ProfessionalId == currentProfId && a.OriginalActivityId != null);
           break;

       default:
           // Vista combinada para el profesional: sus propias + sus editadas + las compartidas originales
           query = query.Where(a => 
               (a.ProfessionalId == currentProfId) || 
               (a.ProfessionalId != currentProfId && a.OriginalActivityId == null && a.IsActive));
           break;
   }
   ```
3. **Comando de Adaptación / Guardado**:
   * Si el usuario edita una actividad donde `ProfessionalId != currentProfId`, el backend automáticamente bifurca el guardado como una nueva entidad con `OriginalActivityId = idOriginal` y `ProfessionalId = currentProfId`.

---

### Frontend (`InclusiON.Client`)

1. **Filtro en `list.component.html` & `list.component.ts`**:
   * Reemplazar o enriquecer el filtro de origen actual:
     ```html
     <select cSelect [(ngModel)]="originFilter" (change)="onFilterChange()">
       <option value="">Todas las actividades</option>
       <option value="propias">Mis creaciones (Propias)</option>
       <option value="compartidas">Comunidad (Compartidas)</option>
       <option value="editadas">Mis adaptaciones (Editadas)</option>
     </select>
     ```
2. **Badges Identificadores en Tabla**:
   * `Propias`: Badge Primario (Azul) - *"Propia"*
   * `Compartidas`: Badge Info (Cyan/Violeta) - *"Compartida por [Nombre]"*
   * `Editadas`: Badge Secundario (Amarillo/Naranja) - *"Adaptada de [Nombre Original]"*

---

## 5. Criterios de Aceptación y Pruebas (Definition of Done - DoD)
- [ ] Prueba unitaria que verifique que una actividad editada (`OriginalActivityId != null`) **nunca** sea devuelta en la consulta de `compartidas`.
- [ ] Prueba unitaria de exclusión de actividades de Roadmap (`RoadmapOrder != null`).
- [ ] Validación de la interfaz: el selector de filtros actualiza la grilla dinámicamente según `Propias`, `Compartidas` y `Editadas`.
- [ ] Prueba funcional de ciclo de vida:
  1. Profesional A crea "Juego de Frutas".
  2. Profesional B lo ve en "Compartidas".
  3. Profesional B hace clic en "Editar / Adaptar", cambia una fruta y guarda.
  4. Profesional B ve la versión adaptada en "Editadas".
  5. Profesional C entra a "Compartidas" y solo ve la versión original de A una única vez.
