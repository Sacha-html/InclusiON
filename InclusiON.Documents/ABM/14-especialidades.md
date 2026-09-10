# Catálogo de especialidades

Especialidades se administra desde Administración > Catálogos > Especialidades. El endpoint público de lectura devuelve únicamente registros activos.

Los profesionales nuevos reciben `SpecialtyId`, con una FK opcional a `Specialties`; la respuesta conserva `Specialty` como texto de compatibilidad. La migración `20260909000000_AddSpecialtiesCatalog` normaliza los valores históricos conocidos, incluyendo variantes sin acento. Los valores no reconocidos conservan el texto original y quedan sin FK para no perder información; esos registros deben ser revisados manualmente antes de asignar una especialidad del catálogo.

La baja es lógica y se rechaza cuando existen profesionales asociados. `Otro` no forma parte del seed ni de los formularios.
