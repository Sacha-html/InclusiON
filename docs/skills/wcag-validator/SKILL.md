---
name: wcag-validator
description: Valida que los temas de accesibilidad del proyecto InclusiON cumplan con WCAG 2.1 AA/AAA. Analiza contrastes, identifica failures y sugiere colores alternativos.
---

## Qué hago

1. **Lectura de temas de accesibilidad**
   - Leo `src/scss/_accessibility-themes.scss` completo (en chunks si es necesario)
   - Identifico todos los perfiles definidos (selectores como `[data-color-mode]`, `[data-a11y-profile]`, etc.)

2. **Análisis de pares de colores**
   - Para cada perfil, extraigo todos los pares text/background:
     - `--a11y-text / --a11y-bg`
     - `--a11y-text / --a11y-surface`
     - `--a11y-text-secondary / --a11y-bg`
     - `--a11y-primary-text / --a11y-primary`
     - `--a11y-link / --a11y-bg`
     - `--a11y-sidebar-* / --a11y-sidebar-bg`
     - `--a11y-header-* / --a11y-header-bg`
     - `--a11y-success/danger/warning / --a11y-bg`

3. **Cálculo de contraste WCAG**
   - Convierto colores hex a RGB
   - Linealizo cada canal RGB (c ≤ 0.04045 → c/12.92, sino → ((c+0.055)/1.055)^2.4)
   - Calculo luminancia: L = 0.2126R + 0.7152G + 0.0722B
   - Ratio = (L_light + 0.05) / (L_dark + 0.05)

4. **Clasificación**
   - ✅ AAA ≥ 7:1
   - ⚠️ AA ≥ 4.5:1
   - ❌ FAIL < 4.5:1

5. **Reporte de failures**
   - Lista solo los pares que fallan
   - Incluye ratio actual y color sugerido

## Cuándo usarme

Usá esta skill cuando:
- Modifiques `_accessibility-themes.scss`
- Añadas nuevos perfiles de accesibilidad
- Revises cambios antes de hacer commit
- El usuario pida validar WCAG

## Formato de salida

Reporto en markdown con:
- Tabla por perfil mostrando cada par y su estado
- Al final, lista de failures con: perfil, par de colores, ratio actual, ratio requerido, color sugerido