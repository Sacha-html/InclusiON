# 🌿 Flujo de Trabajo Git y Estructura de Ramas — InclusiON

Este documento define la estructura oficial de ramas y las **buenas prácticas** para el desarrollo en equipo entre **Sacha** y **Fernando (FAparicio)**.

---

## 1. Arquitectura de Ramas

```text
main  <──────── (Cuando hagan un release / entrega formal con Tag de versión)
  ▲
develop <────── (Pull Request de Sacha y FAparicio cuando terminen sus tareas)
  ▲
  ├── Sacha (Espacio de trabajo de Sacha)
  └── FAparicio (Espacio de trabajo de Fernando)
```

### 📌 Responsabilidad de cada rama

| Rama | Responsable | Propósito | Reglas |
| :--- | :--- | :--- | :--- |
| **`main`** | Equipo | Código en **Producción / Entregas Formales**. | 🚫 **Bloqueada a commits directos.** Solo recibe merges desde `develop` cuando se cierra una versión o entrega evaluativa. Cada merge lleva un tag (ej. `v1.0.0`). |
| **`develop`** | Equipo | Código de **Integración activa**. Contiene todo el código estable que ya fue revisado. | 🚫 **Bloqueada a commits directos.** Solo recibe cambios a través de **Pull Requests (PR)** aprobados desde `Sacha` o `FAparicio`. |
| **`Sacha`** | Sacha | Rama personal de trabajo diario para Sacha. | ✅ Commits diarios y pruebas locales. Se sincroniza con `develop` antes de abrir Pull Request. |
| **`FAparicio`** | Fernando | Rama personal de trabajo diario para Fernando. | ✅ Commits diarios y pruebas locales. Se sincroniza con `develop` antes de abrir Pull Request. |

---

## 2. Buenas Prácticas Fundamentales

### 🛡️ Regla 1: Nadie hace push directo a `develop` ni a `main`
Todo cambio que quiera ingresar a `develop` debe pasar por un **Pull Request en GitHub**. Esto permite:
1. Que tu compañero pueda revisar qué archivos tocaste.
2. Evitar que se rompa el backend o el frontend en la rama compartida.
3. Dejar un historial claro de qué tarea se integró y cuándo.

### 🔄 Regla 2: Sincronizarte con `develop` ANTES de abrir un PR
Para evitar conflictos en GitHub, antes de solicitar que tu rama se una a `develop`, siempre debes traer lo que tu compañero ya integró:
```bash
# Estando en tu rama (ej. Sacha):
git checkout Sacha
git fetch origin
git merge origin/develop
```
Si hay algún conflicto, se resuelve localmente en tu computadora, se prueba que compile y recién ahí se hace `push` y se abre el Pull Request.

### 🧪 Regla 3: Verificar que el proyecto compile antes de commitear
Antes de hacer commit y push:
- **Backend (.NET):** Asegurarse de que compile sin errores (`dotnet build`).
- **Frontend (Angular):** Asegurarse de que compile (`ng build` o sin errores de TypeScript).

### 🏷️ Regla 4: Estandarizar los mensajes de Commit (Conventional Commits)
Mantener el formato que ya utiliza el proyecto:
- `feat(modulo): nueva funcionalidad implementada`
- `fix(modulo): corrección de un error o caso de borde`
- `docs(hu): actualización de historias de usuario o documentación`
- `refactor(modulo): mejora de código sin cambiar comportamiento`
- `chore(db): scripts, dependencias o configuración`

---

## 3. Guía Paso a Paso para el Trabajo Diario

### 👤 Rutina para Sacha

#### 1. Iniciar el día de trabajo
```bash
git checkout Sacha
git pull origin Sacha
# Traer novedades que Fernando haya integrado a develop:
git fetch origin
git merge origin/develop
```

#### 2. Desarrollar y guardar cambios localmente
```bash
git status
git add .
git commit -m "feat(evaluations): agregar filtro de alumnos por sala"
```

#### 3. Subir avances a GitHub
```bash
git push origin Sacha
```

#### 4. Cuando terminaste la tarea (Abrir Pull Request)
1. Ve a GitHub: [https://github.com/Sacha-html/InclusiON](https://github.com/Sacha-html/InclusiON)
2. Crea un **New Pull Request**:
   - **Base:** `develop`  ⬅️  **Compare:** `Sacha`
3. Fernando revisa el código y le da **Approve / Merge**.
4. ¡Listo! La tarea ya forma parte de `develop`.

---

### 👤 Rutina para Fernando (FAparicio)

#### 1. Iniciar el día de trabajo
```bash
git checkout FAparicio
git pull origin FAparicio
# Traer novedades que Sacha haya integrado a develop:
git fetch origin
git merge origin/develop
```

#### 2. Desarrollar y guardar cambios
```bash
git add .
git commit -m "feat(activities): nuevo endpoint de catalogo"
git push origin FAparicio
```

#### 3. Cuando terminó la tarea (Abrir Pull Request)
1. Ve a GitHub.
2. Crea un **New Pull Request**:
   - **Base:** `develop`  ⬅️  **Compare:** `FAparicio`
3. Sacha revisa y aprueba el **Merge**.

---

## 4. Cómo Hacer una Entrega Formal / Release a `main`

Cuando finaliza un Sprint o se debe presentar una entrega formal:

1. Asegurarse de que `develop` esté completamente probado y funcional.
2. En GitHub, crear un **Pull Request**:
   - **Base:** `main`  ⬅️  **Compare:** `develop`
3. Título del PR: `Release v1.0.0 - Entrega Parcial / Hito X`.
4. Aprobar el Merge en `main`.
5. Crear el Tag de la versión:
   ```bash
   git checkout main
   git pull origin main
   git tag -a v1.0.0 -m "Versión 1.0.0 - Entrega Formal"
   git push origin v1.0.0
   ```

---

## 5. Resumen de Comandos Frecuentes

| Acción | Comando |
| :--- | :--- |
| Ver en qué rama estás | `git branch` |
| Cambiar a tu rama | `git checkout Sacha` (o `git checkout FAparicio`) |
| Guardar cambios | `git add .` && `git commit -m "tipo(alcance): descripcion"` |
| Subir a tu rama remota | `git push origin <tu-rama>` |
| Actualizarte con develop | `git fetch origin` seguido de `git merge origin/develop` |
