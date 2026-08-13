using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services
{
    public class RoadmapInitializer : IRoadmapInitializer
    {
        private readonly AppDbContext _context;

        public RoadmapInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeStudentRoadmapAsync(Guid studentId, Guid? supervisorUserId = null, CancellationToken cancellationToken = default)
        {
            // 1. Verificar si el alumno ya tiene un roadmap
            var hasRoadmap = await _context.PersonRoadmaps.AnyAsync(r => r.PersonId == studentId, cancellationToken);
            if (hasRoadmap) return;

            // 2. Obtener o crear el SkillArea "Trayectoria"
            var skillArea = await _context.SkillAreas.FirstOrDefaultAsync(sa => sa.Name == "Trayectoria", cancellationToken);
            if (skillArea == null)
            {
                skillArea = new SkillArea
                {
                    Name = "Trayectoria",
                    Description = "Camino de aprendizaje estándar anti-frustración.",
                    Icon = "map",
                    Color = "#673AB7",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                };
                _context.SkillAreas.Add(skillArea);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 3. Obtener o crear las 10 actividades estándar
            Professional? profEntity = null;
            if (supervisorUserId.HasValue)
            {
                profEntity = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == supervisorUserId.Value, cancellationToken);
            }
            profEntity ??= await _context.Professionals.FirstOrDefaultAsync(cancellationToken);

            var defaultProfId = profEntity?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000000200");
            var standardActivities = new List<Activity>();
            var activitiesDefinitions = new[]
            {
                new {
                    Seq = 1,
                    Title = "Rompecabezas de 2 piezas",
                    Desc = "Unir la mitad de una imagen de un objeto cotidiano con su otra mitad.",
                    CatId = 8,
                    TemplateCode = "CLASSIFY",
                    Instructions = "Arrastra y une las dos piezas para formar la imagen completa.",
                    ContentJson = "{\"instruction\":\"Arrastra y une las dos piezas para formar la imagen completa.\",\"pairs\":[{\"id\":\"manzana\",\"label\":\"Manzana\",\"pictogramId\":2462},{\"id\":\"pelota\",\"label\":\"Pelota\",\"pictogramId\":3241}]}"
                },
                new {
                    Seq = 2,
                    Title = "Mi rutina visual",
                    Desc = "Ordenar 3 pictogramas cronológicos (ej: Despertar, Comer, Ir al colegio).",
                    CatId = 3,
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Ordena los pictogramas según lo que haces primero en el día.",
                    ContentJson = "{\"instruction\":\"Ordena los pictogramas según lo que haces primero en el día.\",\"items\":[{\"id\":\"1\",\"label\":\"Cepillarse los dientes\",\"pictogramId\":30086,\"correctPosition\":0},{\"id\":\"2\",\"label\":\"Desayunar\",\"pictogramId\":28667,\"correctPosition\":1},{\"id\":\"3\",\"label\":\"Ir al colegio\",\"pictogramId\":32446,\"correctPosition\":2}]}"
                },
                new {
                    Seq = 3,
                    Title = "Concepto 'Muchos / Pocos'",
                    Desc = "Tocar el grupo que tiene más frutas.",
                    CatId = 2,
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "Toca el grupo que tiene más frutas.",
                    ContentJson = "{\"instruction\":\"Toca el grupo que tiene más frutas.\",\"question\":\"¿Dónde hay más fruta?\",\"options\":[{\"id\":\"muchas\",\"text\":\"Muchas frutas\",\"pictogramId\":28339},{\"id\":\"pocas\",\"text\":\"Menos fruta\",\"pictogramId\":3247}],\"correctOptionId\":\"muchas\"}"
                },
                new {
                    Seq = 4,
                    Title = "Explotar burbujas",
                    Desc = "Reventar la burbuja que tiene la estrella de mar adentro.",
                    CatId = 5,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "Reventá la burbuja que tenga la estrella de mar adentro.",
                    ContentJson = "{\"instruction\":\"Reventá la burbuja que tenga la estrella de mar adentro.\",\"correctItemId\":\"estrella\",\"items\":[{\"id\":\"auto\",\"label\":\"Auto\",\"pictogramId\":2339},{\"id\":\"estrella\",\"label\":\"Estrella de mar\",\"pictogramId\":3310},{\"id\":\"silla\",\"label\":\"Silla\",\"pictogramId\":3155}]}"
                },
                new {
                    Seq = 5,
                    Title = "¿Dónde va cada cosa?",
                    Desc = "Asociación Funcional Directa",
                    CatId = 2,
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "¿Qué va aquí?",
                    ContentJson = "{\"instruction\":\"¿Qué va aquí?\",\"question\":\"¿Qué va aquí?\",\"questionPictogramId\":25900,\"options\":[{\"id\":\"almohada\",\"text\":\"Almohada\",\"pictogramId\":2250},{\"id\":\"pelota\",\"text\":\"Pelota\",\"pictogramId\":3241}],\"correctOptionId\":\"almohada\"}"
                },
                new {
                    Seq = 6,
                    Title = "Conciencia fonológica",
                    Desc = "Escuchar el sonido de una vocal y seleccionar el animal que empieza igual (ej: 'A' -> Araña).",
                    CatId = 1,
                    TemplateCode = "SOUND_RECOGNITION",
                    Instructions = "Escucha el sonido y selecciona el dibujo que comience con esa letra.",
                    ContentJson = "{\"instruction\":\"Escucha el sonido y selecciona el dibujo que comience con esa letra.\",\"question\":\"Sonido de la vocal A\",\"options\":[{\"id\":\"1\",\"text\":\"Araña\",\"pictogramId\":3028},{\"id\":\"2\",\"text\":\"Perro\",\"pictogramId\":9217},{\"id\":\"3\",\"text\":\"Gato\",\"pictogramId\":9214}],\"correctOptionId\":\"1\"}"
                },
                new {
                    Seq = 7,
                    Title = "Colorear libre",
                    Desc = "Un lienzo digital con colores predefinidos donde solo deben rellenar formas grandes.",
                    CatId = 6,
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "Elige tus colores favoritos y rellena el dibujo libremente.",
                    ContentJson = "{\"instruction\":\"Elige tus colores favoritos y rellena el dibujo libremente.\",\"question\":\"Colorea la Estrella\",\"options\":[{\"id\":\"amarillo\",\"text\":\"Amarillo\",\"pictogramId\":2854},{\"id\":\"azul\",\"text\":\"Azul\",\"pictogramId\":2854},{\"id\":\"rojo\",\"text\":\"Rojo\",\"pictogramId\":2854}],\"correctOptionId\":\"amarillo\"}"
                },
                new {
                    Seq = 8,
                    Title = "Vestirse para el frío",
                    Desc = "Arrastrar una bufanda y un abrigo hacia un personaje.",
                    CatId = 7,
                    TemplateCode = "CLASSIFY",
                    Instructions = "Arrastra la bufanda y el abrigo para abrigar al personaje.",
                    ContentJson = "{\"instruction\":\"Arrastra la bufanda y el abrigo para abrigar al personaje.\",\"pairs\":[{\"id\":\"1\",\"label\":\"Bufanda\",\"pictogramId\":5996},{\"id\":\"2\",\"label\":\"Abrigo\",\"pictogramId\":5992}]}"
                },
                new {
                    Seq = 9,
                    Title = "Clasificación por tamaño",
                    Desc = "Ordenar 3 pelotas de 'Grande' a 'Pequeño'.",
                    CatId = 2,
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Ordena las pelotas de fútbol de la más grande a la más pequeña.",
                    ContentJson = "{\"instruction\":\"Ordena las pelotas de fútbol de la más grande a la más pequeña.\",\"items\":[{\"id\":\"1\",\"label\":\"Grande\",\"pictogramId\":3270,\"correctPosition\":0},{\"id\":\"2\",\"label\":\"Mediano\",\"pictogramId\":3272,\"correctPosition\":1},{\"id\":\"3\",\"label\":\"Pequeño\",\"pictogramId\":3271,\"correctPosition\":2}]}"
                },
                new {
                    Seq = 10,
                    Title = "Encuentra el intruso",
                    Desc = "Identificar qué objeto no pertenece a la categoría (ej: 3 frutas y 1 zapato).",
                    CatId = 8,
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "Toca el objeto que no pertenece al grupo.",
                    ContentJson = "{\"instruction\":\"Toca el objeto que no pertenece al grupo.\",\"question\":\"¿Cuál es el intruso?\",\"options\":[{\"id\":\"manzana\",\"text\":\"Manzana\",\"pictogramId\":2479},{\"id\":\"pera\",\"text\":\"Pera\",\"pictogramId\":2483},{\"id\":\"banana\",\"text\":\"Banana\",\"pictogramId\":2480},{\"id\":\"zapato\",\"text\":\"Zapato\",\"pictogramId\":3113}],\"correctOptionId\":\"zapato\"}"
                }
            };

            foreach (var def in activitiesDefinitions)
            {
                var act = await _context.Activities.FirstOrDefaultAsync(a => a.Title == def.Title && a.IsStandardActivity, cancellationToken);
                var templateType = await _context.Set<ActivityTemplateType>().FirstOrDefaultAsync(t => t.Code == def.TemplateCode, cancellationToken);

                if (act == null)
                {
                    act = new Activity
                    {
                        Title = def.Title,
                        Description = def.Desc,
                        Instructions = def.Instructions,
                        CategoryId = def.CatId,
                        SkillAreaId = skillArea.Id,
                        ProfessionalId = defaultProfId,
                        HasVisualSupport = true,
                        HasAudioSupport = true,
                        UsesEasyReading = true,
                        UsesPictograms = true,
                        RequiresSupervision = false,
                        IsStandardActivity = true,
                        ComplexityLevel = 1,
                        EstimatedDurationMinutes = 2,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    };

                    _context.Activities.Add(act);
                    await _context.SaveChangesAsync(cancellationToken);

                    var content = new ActivityContent
                    {
                        ActivityId = act.Id,
                        TemplateTypeId = templateType?.Id ?? 1,
                        ContentJson = def.ContentJson,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    };
                    _context.Set<ActivityContent>().Add(content);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var existingContent = await _context.Set<ActivityContent>().FirstOrDefaultAsync(c => c.ActivityId == act.Id, cancellationToken);
                    if (existingContent != null)
                    {
                        existingContent.ContentJson = def.ContentJson;
                        if (templateType != null)
                        {
                            existingContent.TemplateTypeId = templateType.Id;
                        }
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
                standardActivities.Add(act);
            }

            // 4. Crear el PersonRoadmap
            var roadmap = new PersonRoadmap
            {
                PersonId = studentId,
                CreatedByProfessionalId = defaultProfId,
                Notes = "Trayectoria anti-frustración preconfigurada.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };
            _context.PersonRoadmaps.Add(roadmap);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Crear el PersonRoadmapArea
            var roadmapArea = new PersonRoadmapArea
            {
                PersonRoadmapId = roadmap.Id,
                SkillAreaId = skillArea.Id,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };
            _context.PersonRoadmapAreas.Add(roadmapArea);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Agregar las 10 actividades al área
            for (int i = 0; i < standardActivities.Count; i++)
            {
                var seq = i + 1;
                var isUnlocked = seq == 1;

                var roadmapActivity = new PersonRoadmapActivity
                {
                    PersonRoadmapAreaId = roadmapArea.Id,
                    ActivityId = standardActivities[i].Id,
                    SequenceOrder = seq,
                    IsUnlocked = isUnlocked,
                    UnlockedAt = isUnlocked ? DateTime.UtcNow : null,
                    UnlockThresholdPercent = 0, // Fail-safe: 0% requerido para desbloquear la siguiente
                    ShowHints = true,
                    DifficultyLevel = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                };
                _context.PersonRoadmapActivities.Add(roadmapActivity);
                await _context.SaveChangesAsync(cancellationToken);

                // Si es el Nivel 1, crear la asignación correspondiente
                if (isUnlocked)
                {
                    var assignment = new ActivityAssignment
                    {
                        ActivityId = standardActivities[i].Id,
                        PersonId = studentId,
                        AssignedByProfessionalId = roadmap.CreatedByProfessionalId,
                        AssignedAt = DateTime.UtcNow,
                        StatusId = AssignmentStatuses.Pendiente,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    };
                    _context.ActivityAssignments.Add(assignment);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
