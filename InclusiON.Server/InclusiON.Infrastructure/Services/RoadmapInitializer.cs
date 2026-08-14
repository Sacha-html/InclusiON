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
                    OldTitles = new[] { "Rompecabezas de 2 piezas" },
                    Desc = "Une la mitad de la imagen para completar el objeto cotidiano.",
                    CatId = 8,
                    TemplateCode = "CLASSIFY",
                    Instructions = "Arrastra y une la mitad de la imagen para completar el objeto cotidiano.",
                    ContentJson = """{"pairs":[{"id":1,"label":"Taza (Derecha)","pictogramId":"pic_taza_der"},{"id":2,"label":"Taza (Izquierda)","pictogramId":"pic_taza_izq"}]}"""
                },
                new {
                    Seq = 2,
                    Title = "Mi rutina visual",
                    OldTitles = new[] { "Mi rutina visual" },
                    Desc = "Ordena los pasos de tu rutina diaria.",
                    CatId = 3,
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Ordena los pasos de tu rutina diaria.",
                    ContentJson = """{"items":[{"id":1,"label":"Despertar","pictogramId":"pic_despertar","correctPosition":1},{"id":2,"label":"Comer","pictogramId":"pic_comer","correctPosition":2},{"id":3,"label":"Jugar","pictogramId":"pic_jugar","correctPosition":3}]}"""
                },
                new {
                    Seq = 3,
                    Title = "Concepto Muchos / Pocos",
                    OldTitles = new[] { "Concepto 'Muchos / Pocos'", "Concepto Muchos / Pocos" },
                    Desc = "¿Dónde hay muchas manzanas?",
                    CatId = 2,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "¿Dónde hay muchas manzanas?",
                    ContentJson = """{"correctItemId":2,"items":[{"id":1,"pictogramId":"pic_una_manzana","label":"Pocas (1)"},{"id":2,"pictogramId":"pic_muchas_manzanas","label":"Muchas (8)"}]}"""
                },
                new {
                    Seq = 4,
                    Title = "Secuencia de acción (Camino Visual)",
                    OldTitles = new[] { "Explotar burbujas", "Secuencia de acción (Camino Visual)" },
                    Desc = "Toca las burbujas en orden para terminar el camino.",
                    CatId = 5,
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Toca las burbujas en orden para terminar el camino.",
                    ContentJson = """{"items":[{"id":1,"label":"Burbuja 1","pictogramId":"pic_burbuja_1","correctPosition":1},{"id":2,"label":"Burbuja 2","pictogramId":"pic_burbuja_2","correctPosition":2},{"id":3,"label":"Burbuja 3","pictogramId":"pic_burbuja_3","correctPosition":3}]}"""
                },
                new {
                    Seq = 5,
                    Title = "Asociación Funcional Cotidiana",
                    OldTitles = new[] { "¿Dónde va cada cosa?", "¿Qué quieres hacer?", "Asociación Funcional Cotidiana" },
                    Desc = "Contexto: Cama. ¿Qué objeto va en la cama?",
                    CatId = 8,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "Contexto: Cama. ¿Qué objeto va en la cama?",
                    ContentJson = """{"correctItemId":1,"items":[{"id":1,"pictogramId":"pic_almohada","label":"Almohada"},{"id":2,"pictogramId":"pic_pelota","label":"Pelota de fútbol"}]}"""
                },
                new {
                    Seq = 6,
                    Title = "Reconocimiento Fonológico",
                    OldTitles = new[] { "Conciencia fonológica", "Reconocimiento Fonológico" },
                    Desc = "¿Qué animal empieza con la letra A?",
                    CatId = 1,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "¿Qué animal empieza con la letra A?",
                    ContentJson = """{"correctItemId":3,"items":[{"id":1,"pictogramId":"pic_perro","label":"Perro"},{"id":2,"pictogramId":"pic_gato","label":"Gato"},{"id":3,"pictogramId":"pic_arana","label":"Araña"}]}"""
                },
                new {
                    Seq = 7,
                    Title = "Identificación de Formas Básicas",
                    OldTitles = new[] { "Colorear libre", "Identificación de Formas Básicas" },
                    Desc = "¿Cuál es el círculo?",
                    CatId = 2,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "¿Cuál es el círculo?",
                    ContentJson = """{"correctItemId":3,"items":[{"id":1,"pictogramId":"pic_cuadrado","label":"Cuadrado"},{"id":2,"pictogramId":"pic_triangulo","label":"Triángulo"},{"id":3,"pictogramId":"pic_circulo","label":"Círculo"}]}"""
                },
                new {
                    Seq = 8,
                    Title = "Vestirse para el frío",
                    OldTitles = new[] { "Vestirse para el frío" },
                    Desc = "Guarda la ropa de invierno en el armario.",
                    CatId = 7,
                    TemplateCode = "CLASSIFY",
                    Instructions = "Guarda la ropa de invierno en el armario.",
                    ContentJson = """{"pairs":[{"id":1,"label":"Invierno","pictogramId":"pic_bufanda"},{"id":2,"label":"Invierno","pictogramId":"pic_gorro"}]}"""
                },
                new {
                    Seq = 9,
                    Title = "Seriación de Tamaños",
                    OldTitles = new[] { "Clasificación por tamaño", "Seriación de Tamaños" },
                    Desc = "Ordena las pelotas de la más pequeña a la más grande.",
                    CatId = 2,
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Ordena las pelotas de la más pequeña a la más grande.",
                    ContentJson = """{"items":[{"id":1,"label":"Pequeña","pictogramId":"pic_pelota_chica","correctPosition":1},{"id":2,"label":"Mediana","pictogramId":"pic_pelota_mediana","correctPosition":2},{"id":3,"label":"Grande","pictogramId":"pic_pelota_grande","correctPosition":3}]}"""
                },
                new {
                    Seq = 10,
                    Title = "Encuentra el intruso",
                    OldTitles = new[] { "Encuentra el intruso" },
                    Desc = "¿Qué objeto no pertenece a este grupo de frutas?",
                    CatId = 8,
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "¿Qué objeto no pertenece a este grupo de frutas?",
                    ContentJson = """{"correctItemId":4,"items":[{"id":1,"pictogramId":"pic_manzana","label":"Manzana"},{"id":2,"pictogramId":"pic_pera","label":"Pera"},{"id":3,"pictogramId":"pic_banana","label":"Banana"},{"id":4,"pictogramId":"pic_zapato","label":"Zapato"}]}"""
                }
            };

            foreach (var def in activitiesDefinitions)
            {
                var act = await _context.Activities.FirstOrDefaultAsync(a => a.Title == def.Title || def.OldTitles.Contains(a.Title), cancellationToken);
                var templateType = await _context.Set<ActivityTemplateType>().FirstOrDefaultAsync(t => t.Code == def.TemplateCode, cancellationToken);
                var templateTypeId = templateType?.Id ?? 1;

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
                        IsTemplate = true,
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
                        TemplateTypeId = templateTypeId,
                        ContentJson = def.ContentJson,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    };
                    _context.Set<ActivityContent>().Add(content);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    act.Title = def.Title;
                    act.Description = def.Desc;
                    act.Instructions = def.Instructions;
                    act.CategoryId = def.CatId;
                    act.SkillAreaId = skillArea.Id;
                    act.IsStandardActivity = true;
                    act.IsTemplate = true;

                    var existingContent = await _context.Set<ActivityContent>().FirstOrDefaultAsync(c => c.ActivityId == act.Id, cancellationToken);
                    if (existingContent != null)
                    {
                        existingContent.ContentJson = def.ContentJson;
                        existingContent.TemplateTypeId = templateTypeId;
                    }
                    else
                    {
                        var content = new ActivityContent
                        {
                            ActivityId = act.Id,
                            TemplateTypeId = templateTypeId,
                            ContentJson = def.ContentJson,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
                        };
                        _context.Set<ActivityContent>().Add(content);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
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
