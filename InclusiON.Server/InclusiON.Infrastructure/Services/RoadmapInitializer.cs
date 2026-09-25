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

        private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private sealed record StandardActivityDefinition(
            int Sequence, string Key, string Title, string Description, string Category,
            string TemplateCode, string Instructions, string ContentJson);

        private static readonly StandardActivityDefinition[] Definitions =
        {
            new(1, "ROADMAP_STANDARD_01_ARMANDO_EL_VASO", "Armando el vaso", "Rompecabezas básico de un vaso de agua.", "Motricidad y Coordinación", "PUZZLE", "Armá el vaso", "{\"instruction\":\"Armá el vaso\",\"pictogramId\":2610,\"label\":\"Vaso de agua\",\"rows\":2,\"cols\":1,\"showGhostGuide\":true}"),
            new(2, "ROADMAP_STANDARD_02_NECESITO_IR_AL_BANO", "Necesito ir al baño", "Comunicación de una necesidad básica mediante pictogramas.", "Comunicación y Lenguaje", "PICTOGRAM_SELECT", "Elegí el pictograma del baño", "{\"instruction\":\"Elegí el pictograma del baño\",\"correctItemId\":\"bano\",\"items\":[{\"id\":\"bano\",\"label\":\"Baño\",\"pictogramId\":6929},{\"id\":\"pelota\",\"label\":\"Pelota\",\"pictogramId\":3241}]}"),
            // SOUND_RECOGNITION currently falls back to OPTION_SELECT: no real audio is played,
            // so this activity evaluates the association between laughter and emotion through visual options.
            new(3, "ROADMAP_STANDARD_03_QUIEN_SE_RIE", "¿Quién se ríe?", "Discriminación auditiva de una emoción básica.", "Habilidades Socioemocionales", "SOUND_RECOGNITION", "Escuchá y elegí la cara alegre", "{\"instruction\":\"Escuchá el sonido y elegí la cara alegre\",\"question\":\"¿Quién se ríe?\",\"options\":[{\"id\":\"alegre\",\"text\":\"La cara alegre\",\"pictogramId\":8582},{\"id\":\"triste\",\"text\":\"La cara triste\",\"pictogramId\":16371}],\"correctOptionId\":\"alegre\"}"),
            new(4, "ROADMAP_STANDARD_04_BUSCANDO_LA_MESA", "Buscando la MESA", "Lectura global de la palabra MESA.", "Lectoescritura", "GLOBAL_READING", "Leé MESA y elegí el pictograma correcto", "{\"instruction\":\"Leé MESA y elegí el pictograma correcto\",\"word\":\"MESA\",\"items\":[{\"id\":\"mesa\",\"pictogramId\":3129,\"label\":\"Mesa\"},{\"id\":\"silla\",\"pictogramId\":3155,\"label\":\"Silla\"},{\"id\":\"cama\",\"pictogramId\":25900,\"label\":\"Cama\"}],\"correctItemId\":\"mesa\"}"),
            new(5, "ROADMAP_STANDARD_05_CONTANDO_GALLETAS", "Contando galletas", "Conteo visual de tres galletas.", "Numeración y Matemática", "NUMERATION", "Contá las galletas y elegí el número correcto", "{\"instruction\":\"Contá las galletas y elegí el número correcto\",\"operandA\":3,\"operandB\":0,\"pictogramId\":8312,\"options\":[{\"id\":\"dos\",\"value\":2},{\"id\":\"tres\",\"value\":3},{\"id\":\"cuatro\",\"value\":4}]}"),
            new(6, "ROADMAP_STANDARD_06_ME_SIRVO_AGUA", "Me sirvo agua", "Secuencia de acciones para servirse agua.", "Autonomía y Vida Diaria", "ORDER_SEQUENCE", "Ordená los pasos para servirte agua", "{\"instruction\":\"Ordená los pasos para servirte agua\",\"items\":[{\"id\":\"1\",\"label\":\"Agarrar el vaso\",\"pictogramId\":2610,\"correctPosition\":0},{\"id\":\"2\",\"label\":\"Abrir la canilla\",\"pictogramId\":11737,\"correctPosition\":1},{\"id\":\"3\",\"label\":\"Tomar agua\",\"pictogramId\":6061,\"correctPosition\":2}]}"),
            new(7, "ROADMAP_STANDARD_07_DONDE_HACEMOS_PIS", "¿Dónde hacemos pis?", "Comprensión del contexto de una necesidad básica.", "Habilidades Socioemocionales", "OPTION_SELECT", "Elegí dónde hacemos pis", "{\"instruction\":\"Si tenés ganas de hacer pis, ¿adónde vas?\",\"question\":\"¿Dónde hacemos pis?\",\"options\":[{\"id\":\"cama\",\"text\":\"La cama\",\"pictogramId\":25900},{\"id\":\"bano\",\"text\":\"El baño\",\"pictogramId\":6929},{\"id\":\"cocina\",\"text\":\"La cocina\",\"pictogramId\":10752}],\"correctOptionId\":\"bano\"}"),
            new(8, "ROADMAP_STANDARD_08_ORDENAR_LA_COCINA", "A ordenar la cocina", "Clasificación de alimentos y utensilios.", "Numeración y Matemática", "CLASSIFY", "Uní cada imagen con su grupo", "{\"instruction\":\"Uní cada imagen con su grupo\",\"pairs\":[{\"id\":\"galleta\",\"label\":\"Alimentos: galleta\",\"pictogramId\":8312},{\"id\":\"manzana\",\"label\":\"Alimentos: manzana\",\"pictogramId\":2462},{\"id\":\"plato\",\"label\":\"Utensilios: plato\",\"pictogramId\":16857},{\"id\":\"cuchara\",\"label\":\"Utensilios: cuchara\",\"pictogramId\":2362}]}"),
            new(9, "ROADMAP_STANDARD_09_LETRAS_DE_MESA", "Las letras de MESA", "Conciencia fonológica para completar M _ S _.", "Lectoescritura", "BUILD_WORD", "Completá las letras de MESA", "{\"instruction\":\"Completá las letras de MESA\",\"word\":\"MESA\",\"hiddenIndices\":[1,3],\"options\":[[\"E\",\"A\",\"O\"],[\"A\",\"E\",\"I\"]]}"),
            new(10, "ROADMAP_STANDARD_10_RUTINA_DE_LA_MANANA", "La rutina de la mañana", "Secuencia de cuatro acciones de la rutina matutina.", "Autonomía y Vida Diaria", "ORDER_SEQUENCE", "Ordená la rutina de la mañana", "{\"instruction\":\"Ordená la rutina de la mañana\",\"items\":[{\"id\":\"1\",\"label\":\"Despertarse\",\"pictogramId\":8989,\"correctPosition\":0},{\"id\":\"2\",\"label\":\"Ir al baño\",\"pictogramId\":6929,\"correctPosition\":1},{\"id\":\"3\",\"label\":\"Lavarse los dientes\",\"pictogramId\":6971,\"correctPosition\":2},{\"id\":\"4\",\"label\":\"Desayunar\",\"pictogramId\":28667,\"correctPosition\":3}]}"),
        };

        public async Task<List<Activity>> EnsureStandardActivitiesAsync(CancellationToken cancellationToken = default)
        {
            var skillArea = await _context.SkillAreas.FirstOrDefaultAsync(sa => sa.Name == "Trayectoria", cancellationToken);
            if (skillArea == null) return new List<Activity>();

            var templateTypes = await _context.ActivityTemplateTypes
                .Where(t => Definitions.Select(d => d.TemplateCode).Contains(t.Code))
                .ToDictionaryAsync(t => t.Code, cancellationToken);
            var missingTemplate = Definitions.Select(d => d.TemplateCode).Distinct().FirstOrDefault(code => !templateTypes.ContainsKey(code));
            if (missingTemplate != null)
                throw new InvalidOperationException($"Missing activity template type with code '{missingTemplate}'.");

            var categoryNames = Definitions.Select(d => d.Category).Distinct().ToArray();
            var categories = await _context.ActivityCategories
                .Where(c => categoryNames.Contains(c.Name))
                .ToDictionaryAsync(c => c.Name, cancellationToken);
            var missingCategory = categoryNames.FirstOrDefault(name => !categories.ContainsKey(name));
            if (missingCategory != null)
                throw new InvalidOperationException($"Missing activity category '{missingCategory}'.");

            var keys = Definitions.Select(d => d.Key).ToArray();
            var activities = await _context.Activities.Where(a => a.StandardKey != null && keys.Contains(a.StandardKey)).ToListAsync(cancellationToken);
            var result = new List<Activity>(Definitions.Length);
            foreach (var def in Definitions)
            {
                var activity = activities.FirstOrDefault(a => a.StandardKey == def.Key);
                if (activity == null)
                {
                    activity = new Activity { StandardKey = def.Key, CreatedAt = DateTime.UtcNow, CreatedBy = SystemUserId };
                    _context.Activities.Add(activity);
                }

                activity.Title = def.Title;
                activity.Description = def.Description;
                activity.Instructions = def.Instructions;
                activity.CategoryId = categories[def.Category].Id;
                activity.SkillAreaId = skillArea.Id;
                activity.ProfessionalId = null;
                activity.HasVisualSupport = true;
                activity.HasAudioSupport = true;
                activity.UsesEasyReading = true;
                activity.UsesPictograms = true;
                activity.RequiresSupervision = false;
                activity.IsStandardActivity = true;
                activity.IsTemplate = true;
                activity.RoadmapOrder = def.Sequence;
                activity.ComplexityLevel = 1;
                activity.EstimatedDurationMinutes = 2;

                var content = await _context.ActivityContents.FirstOrDefaultAsync(c => c.ActivityId == activity.Id, cancellationToken);
                if (content == null)
                {
                    content = new ActivityContent { Activity = activity, CreatedAt = DateTime.UtcNow, CreatedBy = SystemUserId };
                    _context.ActivityContents.Add(content);
                }
                content.TemplateTypeId = templateTypes[def.TemplateCode].Id;
                content.ContentJson = def.ContentJson;
                result.Add(activity);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return result.OrderBy(a => a.RoadmapOrder).ToList();
        }

        public async Task InitializeStudentRoadmapAsync(Guid studentId, Guid? supervisorUserId = null, CancellationToken cancellationToken = default)
        {
            // Do not even touch the global catalog for students who already have a roadmap.
            var hasRoadmap = await _context.PersonRoadmaps.AnyAsync(r => r.PersonId == studentId, cancellationToken);
            if (hasRoadmap) return;

            var standardActivities = await EnsureStandardActivitiesAsync(cancellationToken);

            var skillArea = await _context.SkillAreas.FirstOrDefaultAsync(sa => sa.Name == "Trayectoria", cancellationToken);
            if (skillArea == null) return;

            Professional? profEntity = null;
            if (supervisorUserId.HasValue && supervisorUserId.Value != Guid.Empty)
            {
                profEntity = await _context.Professionals.FirstOrDefaultAsync(
                    p => p.UserId == supervisorUserId.Value,
                    cancellationToken);
            }

            // PersonRoadmap and ActivityAssignment require a real professional.
            // Standard activities remain global when none exists, but the personalized
            // roadmap must be deferred until a professional can own it.
            if (profEntity == null) return;

            // 4. Crear el PersonRoadmap
            var roadmap = new PersonRoadmap
            {
                PersonId = studentId,
                CreatedByProfessionalId = profEntity.Id,
                Notes = "Trayectoria anti-frustración preconfigurada.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemUserId
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
                CreatedBy = SystemUserId
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
                    CreatedBy = SystemUserId
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
                        CreatedBy = SystemUserId
                    };
                    _context.ActivityAssignments.Add(assignment);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<int> RepairAssignedStudentRoadmapsAsync(CancellationToken cancellationToken = default)
        {
            var assignments = await _context.ProfessionalPersons
                .Where(pp => pp.IsActive && !_context.PersonRoadmaps.Any(r => r.PersonId == pp.PersonId))
                .Select(pp => new
                {
                    pp.PersonId,
                    ProfessionalUserId = pp.Professional.UserId,
                    pp.IsPrimaryProfessional,
                    pp.AssignedAt,
                    pp.ProfessionalId
                })
                .OrderByDescending(pp => pp.IsPrimaryProfessional)
                .ThenBy(pp => pp.AssignedAt)
                .ThenBy(pp => pp.ProfessionalId)
                .ToListAsync(cancellationToken);

            var repaired = 0;
            foreach (var assignment in assignments.GroupBy(a => a.PersonId).Select(g => g.First()))
            {
                await InitializeStudentRoadmapAsync(
                    assignment.PersonId,
                    assignment.ProfessionalUserId,
                    cancellationToken);
                repaired++;
            }

            return repaired;
        }
    }
}
