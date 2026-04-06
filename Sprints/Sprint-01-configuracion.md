# Sprint 1 — Infraestructura y Configuración (IN-21 a IN-35)

**Épica:** IN-3 - Configuración del Sistema

**Período:** 

**Objetivo:** Establecer el núcleo de la infraestructura del sistema (Core Settings), desarrollando la base de datos fundamental para el aislamiento y protección del tenant institucional.

---

## Tareas

| Código | Task | Épica | Estado | Notas |
|--------|------|-------|--------|-------|
| IN-21 | Alta de institución | IN-3 | ✅ | InstitutionsController POST |
| IN-22 | Consulta de instituciones | IN-3 | ✅ | InstitutionsController GET |
| IN-23 | Edición de institución | IN-3 | ✅ | InstitutionsController PUT |
| IN-24 | Consulta de roles | IN-3 | ✅ | RolesController GET |
| IN-25 | Asignación de permisos por módulo | IN-3 | ✅ | RolesController PUT permissions |
| IN-26 | Creación de administradores institucionales | IN-3 | ✅ | AdminInstitutionsController |
| IN-27 | Asignación de instituciones a admins | IN-3 | ✅ | AdminInstitutionsController POST |
| IN-28 | Filtrado de datos por institución | IN-3 | ✅ | InstitutionAccessFilter |
| IN-29 | Enforcement de aislamiento por institución | IN-3 | ✅ | InstitutionAccessFilter |
| IN-30 | Confirmación al guardar permisos | IN-3 | ✅ | Frontend confirm modal |
| IN-31 | Revocación de tokens al cambiar permisos | IN-3 | ✅ | RolesController línea 183-202 |
| IN-32 | Invalidación de caché de permisos | IN-3 | ✅ | RolesController línea 181 |
| IN-33 | Consulta de catálogos (6 tipos) | IN-3 | ✅ | CatalogsController GET |
| IN-34 | Alta de items en catálogo | IN-3 | ✅ | CatalogAdminController POST |
| IN-35 | Edición de items en catálogo | IN-3 | ✅ | CatalogAdminController PUT |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 15 |
| Completadas | 15 |

---

## Épicas padre

- **IN-3:** Configuración del Sistema