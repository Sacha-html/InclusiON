---
name: wcag-validator
description: Valida contrastes WCAG 2.1 AA/AAA en el sistema de temas de accesibilidad
user_invocable: true
---

Lee `src/scss/_accessibility-themes.scss` y valida todos los pares de colores foreground/background para cada combinacion de `data-color-mode` + `data-a11y-profile`.

Calcula el ratio de contraste WCAG 2.1:
- Hex -> RGB -> linealizar -> luminancia -> ratio
- Texto normal: AAA >= 7:1, AA >= 4.5:1
- Texto grande: AAA >= 4.5:1, AA >= 3:1
- UI elements: AA >= 3:1

Pares a evaluar: text/bg, text/surface, text-secondary/bg, primary-text/primary, link/bg, sidebar-*/sidebar-bg, header-*/header-bg, dropdown-*/dropdown-bg, success|danger|warning/bg, role-*/fondo.

Reporta solo failures y pares en el limite (< 5:1). Para cada failure sugiere un color alternativo que mantenga el hue y alcance el ratio requerido.

Formato: tabla markdown por perfil con columnas Par, Foreground, Background, Ratio, Estado, Sugerencia. Resumen final con totales.
