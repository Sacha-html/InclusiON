# Alta de Actividad — Versión Mínima (MVP)

**HU:** IN-105  
**Estado:** 🔧 Entidad + migraciones listas, sin handlers ni FE

---

## Qué se recorta respecto a la versión completa

| Funcionalidad completa | MVP mínimo |
|------------------------|------------|
| Wizard de 4 pasos | Formulario de 1 página con secciones |
| Búsqueda de pictogramas ARASAAC | Sin ARASAAC — campo URL opcional por ítem |
| Actividades estándar del sistema (biblioteca global) | Solo actividades propias del profesional |
| Duración estimada y flag de supervisión | No incluidos |
| Vista previa del player antes de guardar | No incluida |

---

## Formulario mínimo

### Sección 1 — Datos generales

| Campo | Tipo | Requerido |
|-------|------|-----------|
| Título | texto libre | ✅ |
| Área de habilidad | select (catálogo) | ✅ |
| Tipo de template | select (catálogo, filtrado por área) | ✅ |
| Complejidad | número 1–5 | ✅ (default: 1) |
| Descripción | textarea | No |

### Sección 2 — Contenido (dinámico según template)

El formulario de contenido cambia al seleccionar el tipo de template. Ver detalle por template abajo.

---

## Contenido mínimo por template

### Template 1 — Selección de figuras

El estudiante ve una pregunta y elige la opción correcta entre varias.

```
Pregunta:        [texto]
Opciones:        mín. 2, máx. 4
  - Opción 1:    [etiqueta]  [¿correcta? checkbox]
  - Opción 2:    [etiqueta]  [¿correcta? checkbox]
  + Agregar opción
```

Validaciones: exactamente 1 opción correcta, mínimo 2 opciones.

---

### Template 2 — Suma visual

El estudiante suma dos cantidades y elige el resultado correcto.

```
Operando 1:    [número]
Operando 2:    [número]
Distractores:  [número, número]   (opciones incorrectas para el player)
```

Validaciones: operandos > 0, los distractores ≠ resultado real.

---

### Template 3 — Emparejar imagen-palabra

El estudiante conecta cada palabra con su imagen correspondiente.

```
Pares:   mín. 2, máx. 6
  - Par 1:  Palabra [texto]   URL imagen [opcional]
  - Par 2:  Palabra [texto]   URL imagen [opcional]
  + Agregar par
```

Validaciones: mínimo 2 pares, todas las palabras distintas.

---

### Template 4 — Ordenar secuencia

El estudiante ordena ítems en la secuencia correcta.

```
Ítems en orden correcto:   mín. 3, máx. 6
  1. [texto]
  2. [texto]
  3. [texto]
  + Agregar ítem
```

El orden de ingreso define el orden correcto.

---

### Template 5 — Completar letra

El estudiante completa una letra faltante en una palabra.

```
Palabra completa:     [texto]       (ej: CASA)
Posición del blank:   [número]      (ej: 2 → C_SA)
Opciones:             mín. 2, máx. 4
  - Opción 1:   [letra]   [¿correcta? checkbox]
  - Opción 2:   [letra]   [¿correcta? checkbox]
  + Agregar opción
```

Validaciones: posición válida dentro de la palabra, exactamente 1 opción correcta.

---

## Estructura JSON del campo `Content` (jsonb)

El campo `Content` en la entidad `Activity` almacena el contenido dinámico como JSONB. Estructura por template:

```json
// Template 1 — Selección de figuras
{
  "question": "¿Cuál es el perro?",
  "options": [
    { "label": "Gato", "imageUrl": null, "isCorrect": false },
    { "label": "Perro", "imageUrl": null, "isCorrect": true }
  ]
}

// Template 2 — Suma visual
{
  "operand1": 3,
  "operand2": 4,
  "distractors": [5, 8]
}

// Template 3 — Emparejar imagen-palabra
{
  "pairs": [
    { "word": "Manzana", "imageUrl": null },
    { "word": "Pelota",  "imageUrl": null }
  ]
}

// Template 4 — Ordenar secuencia
{
  "items": ["Primero", "Segundo", "Tercero"]
}

// Template 5 — Completar letra
{
  "word": "CASA",
  "blankIndex": 1,
  "options": [
    { "letter": "A", "isCorrect": true },
    { "letter": "E", "isCorrect": false }
  ]
}
```

---

## Backend mínimo

```
POST   /api/activities          CreateActivityCommand
GET    /api/activities          GetActivitiesQuery (paginado, filtro por área)
GET    /api/activities/{id}     GetActivityByIdQuery
PUT    /api/activities/{id}     UpdateActivityCommand
PUT    /api/activities/{id}/deactivate   DeactivateActivityCommand
```

Validaciones server-side:
- El profesional solo puede editar/desactivar actividades propias
- No desactivar si tiene asignaciones activas
- Validar estructura del JSON `Content` según `TemplateTypeId`

---

## Frontend mínimo

```
/pro/activities              → listado paginado (mis actividades)
/pro/activities/new          → formulario de alta
/pro/activities/:id/edit     → formulario de edición
```

Componentes:
- `ActivityFormComponent` — formulario general + sección de contenido dinámica
- `ActivityContentSeleccionFigurasComponent`
- `ActivityContentSumaVisualComponent`
- `ActivityContentEmparejarComponent`
- `ActivityContentOrdenarComponent`
- `ActivityContentCompletarLetraComponent`

El componente de contenido activo se intercambia con `@if` sobre el `templateTypeId` seleccionado.

---

## Lo que queda para Post-MVP

- Búsqueda e integración de pictogramas ARASAAC (IN-106)
- Biblioteca de actividades estándar del sistema
- Vista previa del player antes de guardar
- Duración estimada y flag de supervisión
- Búsqueda semántica de actividades (IN-135)
