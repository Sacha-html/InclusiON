# 🎒 Perfiles: Alumnos y Estudiantes con Discapacidad

## Concepto de Negocio
Los estudiantes acceden al sistema a través del portal amigable e intuitivo de **InclusiON** (`/login` pestaña "Estudiante"). 
El sistema soporta diferentes métodos de acceso adaptados a las capacidades motrices y cognitivas de cada estudiante:
- **Acceso por PIN (Código Numérico de 4 dígitos):** El alumno hace clic en su tarjeta visual/avatar e ingresa su código PIN.
- **Acceso Asistido:** Pensado para alumnos que requieren mediación pedagógica. No requiere que el alumno memorice un PIN; un profesional docente/supervisor autoriza el ingreso en el aula.
- **Acceso Estándar:** Mediante usuario y contraseña tradicional.

---

## 🔑 Listado Completo de Estudiantes

| Alumno/a | DNI | Correo Electrónico | Contraseña | Método de Acceso | Código PIN | Supervisor Asistido | Tutor / Familia Vinculado |
|---|---|---|---|---|:---:|---|---|
| **María García** | — | `maria@test.com` | `Maria123!` | **PIN** | `1234` | — | Rosa Sánchez (`familia@test.com`) |
| **Juan López** | — | `juan@test.com` | `Juan123!` | **PIN** | `1234` | — | Miguel Fernández (`tutor@test.com`) |
| **Carlos Rodríguez** | — | `carlos@test.com` | `Carlos123!` | **PIN** | `5678` | — | Roberto Rodríguez (`carlostu@test.com`) |
| **Tomás Pérez** | 11111111 | `tomas@test.com` | `Student123!` | **PIN** | `1234` | — | Carlos Pérez (`carlostutor@test.com`) |
| **Sofía Rodríguez** | 22222222 | `sofia@test.com` | `Student123!` | **PIN** | `1234` | — | Ana Rodríguez (`anatutor@test.com`) |
| **Mateo Díaz** | 33333333 | `mateo@test.com` | `Student123!` | **PIN** | `1234` | — | Luis Díaz (`luistutor@test.com`) |
| **Valentina Silva** | 44444444 | `valentina@test.com` | `Student123!` | **PIN** | `1234` | — | Elena Silva (`elenatutor@test.com`) |
| **Ana Martínez** | — | `ana@test.com` | *(Sin pass)* | **Asistido** | *(Sin PIN)* | Pedro Martínez (`profesional@test.com`) | Patricia Martínez (`anatu@test.com`) |
| **Benjamín Castro** | 55555555 | `benjamin@test.com` | `Student123!` | **Asistido** | *(Sin PIN)* | Pedro Martínez (`profesional@test.com`) | Jorge Castro (`jorgetutor@test.com`) |

---

## 🖥️ ¿Cómo ingresa un Estudiante?

1. Dirigirse a `http://localhost:4200/login`.
2. Seleccionar la pestaña **"Estudiante"** (o ingresar por el selector de perfiles visuales).
3. Seleccionar la tarjeta correspondiente al alumno:
   - Si tiene método **PIN**: Se abrirá un teclado numérico táctil accesible. Escribir el PIN (`1234` o `5678` según corresponda).
   - Si tiene método **Asistido**: La pantalla solicitará la confirmación del supervisor asignado (Pedro Martínez).
