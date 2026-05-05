# Módulo 4 — Actividades

---

## CU-17: Crear actividad desde plantilla dinámica

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-02 |
| **Prioridad** | Crítica |

**Precondiciones**
- El Profesional está autenticado.
- Existen áreas de habilidad y tipos de plantilla cargados en los catálogos.

**Flujo principal**
1. El Profesional accede a su catálogo de actividades y selecciona "Nueva actividad".
2. **Paso 1 — Área:** Selecciona el área de habilidad (Comunicación, Alfabetización, Lógica-Matemática, Conducta, Motricidad, etc.).
3. **Paso 2 — Plantilla:** El sistema filtra y muestra solo las plantillas disponibles para el área elegida. El Profesional selecciona una.
4. **Paso 3 — Contenido:** El sistema genera el formulario dinámico según la plantilla elegida:
   - *Selección de figuras:* instrucción con audio, opciones con pictogramas, respuesta correcta.
   - *Suma visual:* cantidades representadas con pictogramas, botones numéricos y distractores.
   - *Emparejar imagen-palabra:* pares de pictograma + palabra.
   - *Ordenar secuencia:* cards arrastrables en el orden correcto.
   - *Completar letra:* pictograma + palabra con espacio en blanco + letras posibles.
5. **Paso 4 — Metadatos:** Título, nivel de complejidad (1 a 5), duración estimada y si requiere supervisión.
6. El sistema guarda la actividad en el catálogo personal del Profesional.

**Flujos alternativos**
- **4a. Imágenes:** El Profesional puede buscar e integrar pictogramas de ARASAAC directamente desde el formulario.
- **6a. Guardado incompleto:** Si faltan campos obligatorios según la plantilla, el sistema muestra errores inline y bloquea el guardado.

**Postcondiciones**
- La actividad queda en el catálogo del Profesional con el área y la plantilla asociadas.
- Solo el Profesional creador puede editarla.

---

## CU-18: Editar actividad propia

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional (creador de la actividad) |
| **Actores secundarios** | — |
| **HU de referencia** | HU-02 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.
- La actividad fue creada por ese Profesional (no es una actividad estándar del sistema).
- La actividad no tiene asignaciones activas en ningún roadmap.

**Flujo principal**
1. El Profesional accede al catálogo, busca la actividad y selecciona "Editar".
2. El sistema carga el formulario con los datos actuales.
3. El Profesional modifica los campos deseados.
4. El sistema valida y guarda los cambios.

**Flujos alternativos**
- **1a. Actividad con asignaciones activas:** El sistema muestra aviso "Esta actividad tiene asignaciones activas. Los cambios se reflejarán en los roadmaps existentes." y solicita confirmación.
- **1b. Actividad estándar del sistema:** El sistema no muestra la opción de editar (solo lectura).
- **1c. Actividad de otro Profesional:** El sistema devuelve `403 Forbidden`.

**Postcondiciones**
- Los cambios quedan guardados. Si hay asignaciones activas, el contenido actualizado se usa en la próxima ejecución.

---

## CU-19: Desactivar actividad

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional (creador) |
| **Actores secundarios** | — |
| **HU de referencia** | HU-02 |
| **Prioridad** | Media |

**Precondiciones**
- El Profesional está autenticado.
- La actividad fue creada por ese Profesional.
- La actividad **no** tiene asignaciones activas en ningún roadmap.

**Flujo principal**
1. El Profesional selecciona "Desactivar" en la actividad.
2. El sistema verifica que no existan asignaciones activas.
3. El sistema aplica soft-delete: la actividad queda inactiva y deja de aparecer en catálogos.

**Flujos alternativos**
- **2a. Actividad con asignaciones activas:** El sistema bloquea la operación y muestra "No se puede desactivar una actividad con asignaciones activas".

**Postcondiciones**
- La actividad no aparece en catálogos ni en el wizard de creación de roadmap.
- El historial de respuestas asociadas se conserva.

---

## CU-20: Consultar catálogo de actividades

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-02 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.

**Flujo principal**
1. El Profesional accede a la sección Actividades.
2. El sistema muestra las actividades propias del Profesional y las actividades estándar del sistema.
3. El Profesional puede filtrar por: área de habilidad, tipo de plantilla, nivel de complejidad (1-5), estado (activa/inactiva).
4. El sistema muestra cada actividad con: color del área, estrellas de complejidad y duración estimada.
5. El Profesional selecciona una actividad para ver su detalle completo.

**Flujos alternativos**
- **3a. Sin resultados:** El sistema muestra estado vacío con acción sugerida "Crear tu primera actividad".

**Postcondiciones**
- El Profesional puede usar las actividades listadas para armar roadmaps.
