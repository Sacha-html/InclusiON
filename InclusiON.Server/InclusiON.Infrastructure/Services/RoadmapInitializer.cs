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
                    Title = "¿Qué dice aquí?",
                    Desc = "Lectura global de palabras cotidianas asociadas a su pictograma.",
                    CatId = 1, // Lectoescritura
                    TemplateCode = "GLOBAL_READING",
                    Instructions = "Lee la palabra y elige el dibujo correcto",
                    ContentJson = """{"instruction":"Lee la palabra y elige el dibujo correcto","word":"GATO","items":[{"id":"gato","pictogramId":9214,"label":"Gato"},{"id":"perro","pictogramId":9217,"label":"Perro"},{"id":"pajaro","pictogramId":9174,"label":"Pájaro"}],"correctItemId":"gato"}"""
                },
                new {
                    Seq = 2,
                    Title = "¿Cómo se siente el niño?",
                    Desc = "Identificación de emociones básicas a partir de una situación visual.",
                    CatId = 3, // Habilidades Socioemocionales
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "Mira el dibujo del niño llorando. ¿Qué emoción está sintiendo?",
                    ContentJson = """{"instruction":"Mira el dibujo del niño llorando. ¿Qué emoción está sintiendo?","question":"¿Qué emoción está sintiendo el niño?","questionPictogramId":24200,"options":[{"id":"triste","text":"Tristeza","pictogramId":24200},{"id":"alegre","text":"Alegría","pictogramId":24194},{"id":"enojado","text":"Enojo","pictogramId":24198}],"correctOptionId":"triste"}"""
                },
                new {
                    Seq = 3,
                    Title = "Armando la palabra Sol",
                    Desc = "Construcción guiada de palabras simples letra por letra.",
                    CatId = 1, // Lectoescritura
                    TemplateCode = "BUILD_WORD",
                    Instructions = "Completa las letras que faltan para formar la palabra SOL",
                    ContentJson = """{"instruction":"Completa las letras que faltan para formar la palabra SOL","word":"SOL","hiddenIndices":[0,1,2],"options":[["S","M","P"],["O","A","E"],["L","R","T"]]}"""
                },
                new {
                    Seq = 4,
                    Title = "Contando las manzanas",
                    Desc = "Conteo visual y numeración sumando elementos ilustrados.",
                    CatId = 2, // Numeración y Matemática
                    TemplateCode = "NUMERATION",
                    Instructions = "¿Cuántas manzanas hay en total?",
                    ContentJson = """{"instruction":"¿Cuántas manzanas hay en total?","operandA":3,"operandB":2,"pictogramId":2462,"options":[{"id":"opt1","value":4},{"id":"opt2","value":5},{"id":"opt3","value":6}]}"""
                },
                new {
                    Seq = 5,
                    Title = "Pasos para lavarse las manos",
                    Desc = "Secuencia cronológica de hábitos de higiene personal paso a paso.",
                    CatId = 7, // Autonomía y Vida Diaria
                    TemplateCode = "ORDER_SEQUENCE",
                    Instructions = "Ordena los pasos correctos para lavarse las manos",
                    ContentJson = """{"instruction":"Ordena los pasos correctos para lavarse las manos","items":[{"id":"1","label":"Mojarse las manos","pictogramId":32442,"correctPosition":0},{"id":"2","label":"Ponerse jabón","pictogramId":32443,"correctPosition":1},{"id":"3","label":"Enjuagarse con agua","pictogramId":32444,"correctPosition":2},{"id":"4","label":"Secarse con toalla","pictogramId":32445,"correctPosition":3}]}"""
                },
                new {
                    Seq = 6,
                    Title = "Tengo hambre",
                    Desc = "Selección de pictograma de necesidad básica de alimentación.",
                    CatId = 4, // Comunicación y Lenguaje
                    TemplateCode = "PICTOGRAM_SELECT",
                    Instructions = "Si tienes hambre, ¿qué pictograma debes presionar para comunicarte?",
                    ContentJson = """{"instruction":"Si tienes hambre, ¿qué pictograma debes presionar para comunicarte?","correctItemId":"comer","items":[{"id":"comer","label":"Quiero comer","pictogramId":28667},{"id":"dormir","label":"Quiero dormir","pictogramId":32448},{"id":"jugar","label":"Quiero jugar","pictogramId":32449}]}"""
                },
                new {
                    Seq = 7,
                    Title = "Armando la palabra Casa",
                    Desc = "Construcción guiada de la palabra CASA completando las vocales faltantes.",
                    CatId = 1, // Lectoescritura
                    TemplateCode = "BUILD_WORD",
                    Instructions = "Completa las letras que faltan para formar la palabra CASA",
                    ContentJson = """{"instruction":"Completa las letras que faltan para formar la palabra CASA","word":"CASA","hiddenIndices":[1,3],"options":[["A","E","O"],["A","I","U"]]}"""
                },
                new {
                    Seq = 8,
                    Title = "¿Para qué sirve?",
                    Desc = "Asociación funcional de objetos de la vida cotidiana.",
                    CatId = 8, // Estimulación Cognitiva
                    TemplateCode = "OPTION_SELECT",
                    Instructions = "¿Para qué sirve una cuchara?",
                    ContentJson = """{"instruction":"¿Para qué sirve una cuchara?","question":"¿Para qué usamos la cuchara?","questionPictogramId":2452,"options":[{"id":"comer","text":"Para comer sopa o yogur","pictogramId":28667},{"id":"dibujar","text":"Para pintar en la hoja","pictogramId":2600},{"id":"dormir","text":"Para acostarse en la cama","pictogramId":32448}],"correctOptionId":"comer"}"""
                },
                new {
                    Seq = 9,
                    Title = "Contando los lápices",
                    Desc = "Conteo y suma visual de útiles escolares.",
                    CatId = 2, // Numeración y Matemática
                    TemplateCode = "NUMERATION",
                    Instructions = "¿Cuántos lápices hay en la cartuchera?",
                    ContentJson = """{"instruction":"¿Cuántos lápices hay en la cartuchera?","operandA":4,"operandB":2,"pictogramId":3028,"options":[{"id":"opt1","value":5},{"id":"opt2","value":6},{"id":"opt3","value":7}]}"""
                },
                new {
                    Seq = 10,
                    Title = "Ropa de verano e invierno",
                    Desc = "Clasificación y emparejamiento de prendas de vestir según la estación climática.",
                    CatId = 7, // Autonomía y Vida Diaria
                    TemplateCode = "CLASSIFY",
                    Instructions = "Empareja cada prenda con la estación del año correspondiente",
                    ContentJson = """{"instruction":"Empareja cada prenda con la estación del año correspondiente","pairs":[{"id":"1","label":"Invierno (Gorro)","pictogramId":5992},{"id":"2","label":"Invierno (Bufanda)","pictogramId":5996},{"id":"3","label":"Verano (Malla)","pictogramId":6010},{"id":"4","label":"Verano (Gorra de sol)","pictogramId":6015}]}"""
                }
            };

            foreach (var def in activitiesDefinitions)
            {
                var act = await _context.Activities.FirstOrDefaultAsync(a => a.Title == def.Title || a.RoadmapOrder == def.Seq, cancellationToken);
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
                        RoadmapOrder = def.Seq,
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
                    act.RoadmapOrder = def.Seq;

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
