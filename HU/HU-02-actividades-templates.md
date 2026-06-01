# HU-02 — Sistema de Actividades con Templates Dinámicos

| Campo | Contenido |
|---|---|
| ID | HU-02 |
| Épica | Gestión de Actividades |
| Título | Sistema de Actividades con Templates Dinámicos |
| Prioridad | Crítica |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 6 |
| Estado | Completada |

**Proceso relacionado:** 09, 10

---

## Historia de Usuario

**Como** profesional
**Quiero** crear, editar y gestionar actividades educativas eligiendo un tipo de plantilla y completando el contenido según su estructura
**Para** tener un catálogo reutilizable de actividades adaptadas a cada área de habilidad sin necesidad de conocimientos técnicos

---

## Descripción funcional

El profesional diseña actividades educativas mediante un proceso guiado de 4 pasos:

1. **Elegir área de habilidad** — Selecciona de las áreas disponibles (Comunicación, Alfabetización, Lógica-Matemática, Conducta, Motricidad, etc.)

2. **Elegir tipo de plantilla** — El sistema ofrece plantillas según el área seleccionada. Cada plantilla define qué tipo de actividad es: selección de figuras, suma visual, emparejar imagen-palabra, ordenar secuencia, completar letra. Cada plantilla tiene una estructura de contenido propia.

3. **Completar el contenido** — El sistema genera un formulario dinámico según la plantilla elegida. El profesional completa los campos: textos, opciones, imágenes (pictogramas ARASAAC), respuestas correctas, distractores, etc.

4. **Definir metadatos** — Título de la actividad, nivel de complejidad (1 a 5), duración estimada y si requiere supervisión.

Las actividades creadas quedan en el catálogo personal del profesional. También existen actividades estándar del sistema que cualquier profesional puede usar pero no modificar.

---

## Criterios de Aceptación

- [x] El profesional puede crear actividades paso a paso sin conocimientos técnicos
- [x] El formulario de contenido se adapta dinámicamente según la plantilla seleccionada
- [ ] Se pueden buscar e integrar pictogramas de ARASAAC desde el formulario
- [x] El profesional puede ver su catálogo de actividades y filtrar por área, plantilla o complejidad
- [x] Las actividades estándar del sistema son visibles pero no editables por el profesional
- [x] Solo el creador de una actividad puede editarla
- [ ] No se puede desactivar una actividad que tenga asignaciones activas
- [x] El catálogo muestra el área con su color distintivo, la complejidad con estrellas y la duración estimada
