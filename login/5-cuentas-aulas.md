# 🏫 Cuentas de Prueba Masivas por Aula (Alumnos 1 al 39 y Tutores 1 al 39)

## Concepto de Negocio
Para realizar pruebas de carga, pruebas funcionales de múltiples aulas en paralelo o demostraciones masivas del sistema InclusiON, se cuenta con una serie de cuentas secuenciales asignadas a las diferentes aulas de los docentes (Pedro Martínez, Sofía Gutiérrez y Sacha Del Barrio).

---

## 🔑 Patrón de Credenciales de Alumnos (1 al 39)

- **Formato de Email:** `student{N}@inclusion.com` *(donde `{N}` es del 1 al 39)*
  - Ejemplos: `student1@inclusion.com`, `student2@inclusion.com`, ..., `student39@inclusion.com`
- **Contraseña Estándar:** `Student123!`
- **Código PIN de Acceso:** `1234`
- **Método de Acceso:** PIN / Visual
- **Pantalla de Login:** `http://localhost:4200/login` (Pestaña "Estudiante")

---

## 🔑 Patrón de Credenciales de Tutores (1 al 39)

- **Formato de Email:** `tutor{N}@inclusion.com` *(donde `{N}` es del 1 al 39)*
  - Ejemplos: `tutor1@inclusion.com`, `tutor2@inclusion.com`, ..., `tutor39@inclusion.com`
- **Contraseña Estándar:** `Tutor123!`
- **Código PIN:** *No aplica*
- **Parentesco Registrado:** Tutor/a
- **Alumno Vinculado:** `student{N}@inclusion.com` (Tutor 1 vinculado a Alumno 1, etc.)
- **Pantalla de Login:** `http://localhost:4200/login` (Pestaña "Familia")

---

## 📋 Tabla Rápida de Muestra (Primeros 10)

| N° | Email Alumno | Contraseña Alumno | PIN Alumno | Email Tutor | Contraseña Tutor |
|:---:|---|---|:---:|---|---|
| 1 | `student1@inclusion.com` | `Student123!` | **1234** | `tutor1@inclusion.com` | `Tutor123!` |
| 2 | `student2@inclusion.com` | `Student123!` | **1234** | `tutor2@inclusion.com` | `Tutor123!` |
| 3 | `student3@inclusion.com` | `Student123!` | **1234** | `tutor3@inclusion.com` | `Tutor123!` |
| 4 | `student4@inclusion.com` | `Student123!` | **1234** | `tutor4@inclusion.com` | `Tutor123!` |
| 5 | `student5@inclusion.com` | `Student123!` | **1234** | `tutor5@inclusion.com` | `Tutor123!` |
| 6 | `student6@inclusion.com` | `Student123!` | **1234** | `tutor6@inclusion.com` | `Tutor123!` |
| 7 | `student7@inclusion.com` | `Student123!` | **1234** | `tutor7@inclusion.com` | `Tutor123!` |
| 8 | `student8@inclusion.com` | `Student123!` | **1234** | `tutor8@inclusion.com` | `Tutor123!` |
| 9 | `student9@inclusion.com` | `Student123!` | **1234** | `tutor9@inclusion.com` | `Tutor123!` |
| 10 | `student10@inclusion.com` | `Student123!` | **1234** | `tutor10@inclusion.com` | `Tutor123!` |
