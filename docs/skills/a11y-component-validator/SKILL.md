---
name: a11y-component-validator
description: Valida accesibilidad de componentes Angular (labels, ARIA, roles, focus, estructura). Analiza templates HTML y detecta violations de WCAG 2.1 en componentes.
---

## Qué hago

1. **Análisis de templates Angular**
   - Leo archivos `.html` de componentes en `src/app/views/`
   - Identifico patrones problemáticos: labels huérfanos, falta de aria, roles incorrectos, focus issues

2. **Detección de violations comunes**
   - Labels sin asociación (`for`/`id` o `aria-labelledby`)
   - Botones/links sin texto accesible (falta `aria-label` o contenido visible)
   - Roles incorrectos (`tab` sin `role="tablist"`, buttons con `role="button"` en tabs)
   - Imágenes sin `alt` o con `alt=""` sin `aria-hidden`
   - Tablas sin headers (`th`) o sin `scope`
   - Formularios sin labels visibles o `aria-label`
   - Elementos interactivos sin `tabindex` o sin `aria-disabled`
   - Badges/contadores sin `aria-label` para screen readers
   - Inputs con `readonly` sin `aria-readonly="true"`

3. **Verificación de accesibilidad CSS**
   - Verifica que los componentes usen las variables `--a11y-*` del sistema de temas
   - Detecta hardcoded colors que no respetan el sistema de accesibilidad

4. **Sugerencias de fix**
   - Para cada issue, sugiero el código correcto
   - Prioriza fixes que impactan más usuarios (keyboard navigation, screen readers)

## Cuándo usarme

Usá esta skill cuando:
- Creés o modifiques componentes en `src/app/views/`
- El usuario pida validar accesibilidad de una pantalla específica
- Revises cambios antes de hacer commit
- Necesités hacer un audit completo de accesibilidad

## Formato de salida

Reporto en markdown con:
- Tabla de issues por componente: archivo, línea, issue, severidad, sugerencia
- Severidades: 🔴 CRITICAL (blocker), 🟠 HIGH (impacta muchos usuarios), 🟡 MEDIUM, 🟢 LOW
- Al final, resumen: total de issues por severidad

## Ejemplo de output

```
## Reporte de Accesibilidad

| Archivo | Línea | Issue | Severidad | Sugerencia |
|---------|-------|-------|-----------|------------|
| person-detail.component.html | 17 | Tab con role="button" | 🔴 CRITICAL | Cambiar a role="tab" |
| professional-functional-profile.ts | 44 | Input sin id en label | 🟠 HIGH | Agregar id="..." y for="..." |

### Resumen
- 🔴 CRITICAL: 1
- 🟠 HIGH: 1
- 🟡 MEDIUM: 0
- 🟢 LOW: 0
```