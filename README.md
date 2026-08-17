# 🌟 InclusiON
> **Institución Cervantes — Analista de Sistemas — Prácticas Profesionalizantes 2025/2026**
---
**InclusiON** es una plataforma web integral diseñada para fortalecer y facilitar la **inclusión educativa de personas con discapacidad**. Actúa como un ecosistema digital accesible que conecta de forma directa a **profesionales** (docentes, terapeutas, psicopedagogos, psicólogos), **personas con discapacidad (estudiantes)**, **familias** e **instituciones educativas**.
---
## 🚀 Funcionalidades Principales
- 🎨 **Accesibilidad Visual Adaptativa**: Interfaz adaptada con 7 perfiles visuales accesibles (temas de alto contraste, tipografía adaptada para dislexia, modo oscuro) y métodos de login adaptativos (PIN y Asistido).
- 🎯 **Planes de Trabajo y "Mi Camino" (Roadmap Gamificado)**: Creación y asignación de itinerarios educativos con progresión pedagógica y ajuste automático según desempeño.
- 📊 **Monitoreo de Progreso y Dashboards Analíticos**: Paneles interactivos con métricas clave (KPIs), gráficos de evolución por aula y alertas tempranas de estancamiento o frustración.
- 📄 **Exportación Dinámica a PDF en Formato A4**: Descarga instantánea de reportes y dashboards en formato estándar A4 (portrait/landscape) con sanitización de estilos y alta resolución.
- 💬 **Mensajería Multirrol en Tiempo Real**: Sistema de chat directo entre Administradores, Profesionales y Familias con notificaciones push (SignalR), ordenamiento dinámico por actividad reciente, insignias numéricas de no leídos y redirección contextual desde la campana de notificaciones.
- 🧠 **Búsqueda Semántica**: Implementación de inteligencia artificial local mediante modelos ONNX para búsquedas semánticas de actividades pedagógicas.
- 🤝 **Gestión Unificada de Aulas y Tutores**: Asistente de registro transaccional unificado (Alumno + Tutor + Aula) y vinculación familiar directa.
---
## 📂 Estructura del Monorepo
El repositorio está organizado en las siguientes subcarpetas principales:
```text
InclusiON/
├── InclusiON.Client/       # Frontend en Angular 20 Standalone (Interfaz de Usuario)
├── InclusiON.Server/       # Backend Web API en .NET 10 & PostgreSQL con EF Core
├── InclusiON.Documents/    # Documentación del proyecto (DER, Procesos, HUs, Changelog)
└── demo-repository/        # Repositorio de demostración del sistema
```
