---
name: a11y-component-validator
description: Valida accesibilidad WCAG 2.1 de componentes Angular del proyecto
user_invocable: true
---

Valida accesibilidad del scope indicado (componente, modulo o audit completo).

Analiza 3 capas por componente:

**HTML:** labels/for/id, aria-label, roles (tablist/tab/tabpanel, dialog, alert), aria-expanded, aria-current, img alt, th scope, tabindex, focus trapping en modales, routerLink en no-<a>, @if/@for que rompen asociaciones.

**SCSS:** colores hardcodeados (deben usar --a11y-*), outline:none sin reemplazo, falta de :focus-visible, falta de prefers-reduced-motion.

**TypeScript:** alert()/confirm() nativos, manejo de foco con ViewChild, HostListener para keyboard.

Scope:
- Un componente: su .html + .scss + .ts
- Un modulo (ej: "aac"): todos en src/app/views/aac/
- Audit completo: views + shared/components + components + layout

Priorizar: keyboard > screen reader > semantica > visual.

Formato: tabla por componente (Archivo, Linea, Issue, Severidad, WCAG, Sugerencia). Resumen de severidades. Seccion "Quick wins" con fixes de 1 linea.
