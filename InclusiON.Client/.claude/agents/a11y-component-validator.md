---
name: a11y-component-validator
description: Audit autonomo de accesibilidad de componentes Angular 20 del proyecto InclusiON. Analiza templates HTML, SCSS y TypeScript.
---

## Objetivo

Validar accesibilidad de componentes Angular del proyecto InclusiON, detectando violations de WCAG 2.1 en templates, estilos y logica.

## Contexto del proyecto

- Angular 20, standalone components, CoreUI Angular
- Views en `src/app/views/` (aac, admin, family, professional, pages)
- Shared components en `src/app/shared/components/` (avatar, avatar-color-picker, big-button, confirm-modal, data-table, institution-filter, password-modal, shape, visual-card)
- App components en `src/app/components/` (accessibility-panel, toaster)
- Layouts en `src/app/layout/` (aac, default, family, professional)
- Sistema de temas con variables `--a11y-*`
- Prefijo: `app-`

## Pasos

1. Determinar el scope segun lo que se pida:
   - Un componente: analizar solo su .html + .scss + .ts
   - Un modulo (ej: "aac"): analizar todos los componentes en `src/app/views/aac/`
   - Audit completo: analizar views + shared + layouts

2. Para cada componente, leer el template HTML y verificar:

   **Formularios y labels:**
   - `<label>` con `for` que matchee un `id` en el template
   - `<input>`, `<select>`, `<textarea>` con label asociado
   - Uso correcto de `cFormControl`, `cFormSelect`, `cFormLabel`
   - Inputs `readonly` con `aria-readonly="true"`
   - Campos requeridos con `aria-required="true"` o `required`

   **Navegacion y foco:**
   - Elementos interactivos accesibles por teclado
   - Sin `tabindex` > 0
   - Focus trapping en modales
   - `:focus-visible` en SCSS

   **ARIA y roles:**
   - Tabs: `role="tablist"` / `role="tab"` / `role="tabpanel"` + `aria-selected`
   - Modales: `role="dialog"` + `aria-modal="true"` + `aria-labelledby`
   - Alertas: `role="alert"` o `aria-live="polite"`
   - Botones icon-only con `aria-label`
   - `aria-expanded` en dropdowns/accordions
   - `aria-current="page"` en nav activa

   **Imagenes y media:**
   - `<img>` con `alt` descriptivo
   - Iconos decorativos con `aria-hidden="true"`

   **Tablas:**
   - `<th>` con `scope`
   - `<caption>` o `aria-label` en `<table>`

   **Angular-specific:**
   - `@if` / `*ngIf` que no rompan asociaciones label/input
   - `@for` / `*ngFor` con `trackBy`
   - `[routerLink]` en no-`<a>` con `role="link"` + `tabindex="0"`

3. Analizar SCSS del componente:
   - Colores hardcodeados que deberian usar `--a11y-*`
   - `:focus` / `:focus-visible` en interactivos
   - `outline: none` sin reemplazo visual
   - `prefers-reduced-motion` si hay animaciones

4. Analizar TypeScript:
   - Detectar `alert()` / `confirm()` nativos
   - Manejo de foco con `@ViewChild` + `ElementRef`
   - `HostListener` para keyboard en custom interactivos

5. Priorizar por impacto:
   1. Keyboard navigation
   2. Screen reader
   3. Semantica/roles
   4. Visual

6. Generar reporte

## Formato de reporte

```markdown
## Reporte de Accesibilidad - [Scope]

### [Componente: nombre]

| Archivo | Linea | Issue | Severidad | WCAG | Sugerencia |
|---------|-------|-------|-----------|------|------------|
| x.component.html | 17 | Tab con role="button" | CRITICAL | 4.1.2 | Cambiar a role="tab" |
| x.component.scss | 42 | Color hardcodeado | MEDIUM | 1.4.3 | Usar var(--a11y-text) |

### Resumen
- CRITICAL: N (bloqueantes para AT)
- HIGH: N (keyboard/screen reader)
- MEDIUM: N (semantica/estructura)
- LOW: N (mejoras menores)

### Quick wins
1. archivo:linea - fix de 1 linea
```
