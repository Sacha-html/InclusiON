# ABM — Reportes de Progreso

**Actor:** Profesional  
**Justificación:** El Profesional necesita generar reportes formales de avance para comunicar a los familiares, directivos o equipos interdisciplinarios el progreso de la persona con discapacidad. Sin este ABM, el seguimiento queda en observaciones informales y no hay documentación oficial del proceso pedagógico. Los familiares también necesitan acceder a estos reportes para estar informados sobre el trabajo que se realiza con su familiar.

**Entidades:** `Report`

---

## Alta — Reporte de Progreso

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Persona con discapacidad | Referencia | Sí | Debe existir y estar activa; el profesional debe tener asignada a la persona |
| Tipo de reporte | Referencia | Sí | Debe existir y estar activo en el catálogo |
| Título | Texto (200) | Sí | No vacío |
| Contenido | Texto largo | Sí | No vacío |
| Fecha del reporte | Fecha | Sí | No puede ser futura |
| Inicio del período | Fecha | No | Debe ser anterior al fin del período |
| Fin del período | Fecha | No | Debe ser posterior al inicio; no puede ser futura |
| Metas alcanzadas | Texto largo | No | — |
| Áreas a reforzar | Texto largo | No | — |
| Recomendaciones futuras | Texto largo | No | — |
| Próximos objetivos | Texto largo | No | — |

**Validaciones de integridad:**
- Si se ingresan fechas de período, `InicioPeriodo` debe ser anterior a `FinPeriodo`.

**Resultado:** Se crea `Report` con `Activo = true`. El profesional autenticado queda registrado como profesional que lo generó.

---

## Baja — Reporte

**Actor:** Profesional (solo reportes propios)

- Se establece `Activo = false` (baja lógica).
- **Validación:** Una vez que un familiar o directivo ha visto el reporte (acceso registrado en `AccessAudit`), el reporte no debería darse de baja sin justificación. Se muestra advertencia.

---

## Modificación — Reporte

**Actor:** Profesional (solo reportes propios)

Todos los campos son editables excepto `PersonaConDiscapacidad` y el `Profesional` que lo generó.

| Campo | Validaciones |
|-------|--------------|
| Fecha del reporte | No puede ser futura |
| Inicio/fin del período | `Inicio < Fin`; fin no futuro |
| Resto | Texto libre |

---

## Listado — Reportes de una Persona

**Actor:** Profesional / Representante Familiar (solo lee)

| Columna | Descripción |
|---------|-------------|
| Título | Título del reporte |
| Tipo | Del catálogo de tipos de reporte |
| Fecha | Fecha de emisión |
| Período cubierto | Desde — hasta |
| Profesional | Quién lo generó |
| Estado | Activo / Inactivo |

Ordenado por fecha de reporte descendente.

**Filtros:** tipo de reporte, rango de fechas, profesional, estado.  
**Persistencia:** Consulta a `Report` filtrado por `PersonaConDiscapacidadId`.  
- El Profesional ve reportes de personas bajo su cargo.  
- El Familiar ve reportes activos de las personas que representa.  
- El Admin Global/Institucional puede ver todos los reportes.
