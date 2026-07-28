---
description: Audit all accessibility profiles in the InclusiON project for WCAG 2.1 AA/AAA compliance.
agent: explore
---

Read InclusiON.Client/src/scss/_accessibility-themes.scss (read in chunks using offset/limit — it's large). Identify all accessibility profiles defined by selectors like [data-color-mode="..."], [data-a11y-profile="..."], or any block overriding --a11y-* variables.

For each profile, extract these text/background color pairs:

--a11y-text / --a11y-bg — body text
--a11y-text / --a11y-surface — cards
--a11y-text / --a11y-bg-secondary — secondary backgrounds
--a11y-text-secondary / --a11y-bg — secondary text
--a11y-text-muted / --a11y-bg — muted text
--a11y-primary-text / --a11y-primary — button text
--a11y-sidebar-text / --a11y-sidebar-bg — sidebar text
--a11y-sidebar-nav-link / --a11y-sidebar-bg — sidebar links
--a11y-sidebar-icon / --a11y-sidebar-bg — sidebar icons
--a11y-sidebar-text-muted / --a11y-sidebar-bg — sidebar muted
--a11y-header-text / --a11y-header-bg — header
--a11y-panel-accent / #263238 — accessibility panel title (always dark bg)
--a11y-link / --a11y-bg — inline links
--a11y-success / --a11y-bg — success text
--a11y-danger / --a11y-bg — error text
--a11y-warning / --a11y-bg — warning text

Calculate WCAG contrast ratio: linearize each RGB channel (c ≤ 0.04045 → c/12.92, else → ((c+0.055)/1.055)^2.4), then L = 0.2126R + 0.7152G + 0.0722B, ratio = (L_light + 0.05) / (L_dark + 0.05).

Classify: ✅ AAA ≥ 7:1 | ⚠️ AA ≥ 4.5:1 | ❌ FAIL < 4.5:1

Also check accessibility-panel.component.scss for hardcoded --a11y-panel-* overrides per vision profile.

Report per profile as a markdown table. At the end, list only failures with current ratio, required ratio, and a suggested replacement color.