# Diagrama de Actores del Negocio — InclusiON

**Artefacto:** 05 — Actores del Negocio  
**Práctica Profesionalizante II — Institución Cervantes**  
**Última actualización:** 2026-05-31

---

## Actores identificados

| Actor del negocio | Rol en el sistema | Qué hace hoy sin sistema | Qué hará con el sistema |
|---|---|---|---|
| **Persona con Discapacidad (PCD)** | `person` | Realiza actividades terapéuticas guiadas en papel o materiales físicos. El profesional registra manualmente los resultados. | Accede al **portal AAC** desde cualquier dispositivo. Visualiza sus actividades pendientes con pictogramas y audio. Ejecuta actividades interactivas (selección, emparejamiento, secuencias). Ve su progreso de forma simplificada. |
| **Profesional (docente/terapeuta)** | `professional` | Diseña actividades terapéuticas a mano. Lleva el seguimiento en cuadernos o planillas. Comparte avances con la familia por WhatsApp o email informal. | Crea el catálogo de actividades del sistema, organiza el roadmap de cada persona, asigna actividades, consulta resultados con métricas (% éxito, frustración, tiempo), genera reportes de progreso formales y se comunica vía mensajería interna. |
| **Familiar / Representante** | `family` | Recibe información verbal o por WhatsApp del profesional. No tiene acceso a datos estructurados del progreso. | Accede al **portal familiar** para ver reportes de progreso aprobados, consultar diagnósticos y comunicarse con el profesional a través del sistema. En algunos casos puede supervisar el login de la persona. |
| **Admin Institucional (Equipo Directivo)** | `admin` | Gestiona la nómina de profesionales y personas en planillas o sistemas separados. Aprueba altas manualmente. | Gestiona usuarios del establecimiento (profesionales, estudiantes, tutores familiares). Configura la sede escolar, parametriza catálogos estándar, aprueba registros y supervisa los tableros y métricas de la institución. |

---

## Notas de alcance

- La Persona con Discapacidad **no opera el sistema en nombre de otros** — solo ve y ejecuta lo que le fue asignado.
- El Familiar **no tiene acceso al panel de profesionales** — no puede crear actividades ni ver datos de otras personas.
- El Admin Institucional opera con el rol técnico `admin` para la gestión y configuración integral de la institución escolar.
- Un profesional puede pertenecer a la institución y tener **múltiples personas** asignadas.

---

## Diagrama UML de referencia

> El diagrama de casos de uso detallado está en [`Diagrams/IN-187-actores-negocio.puml`](../Diagrams/IN-187-actores-negocio.puml).

```
Persona PCD         → Portal AAC: ejecutar actividades, ver progreso propio
Profesional         → Panel pro: actividades, roadmap, asignaciones, resultados, reportes, mensajes
Familiar            → Portal familiar: reportes, diagnósticos, mensajes
Admin Institucional → Panel admin: usuarios de su institución, sede, catálogos, aprobaciones, métricas
```
