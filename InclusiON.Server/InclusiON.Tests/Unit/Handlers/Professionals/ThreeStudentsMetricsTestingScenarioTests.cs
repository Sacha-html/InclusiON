using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    /// <summary>
    /// Escenario de prueba solicitado por el usuario:
    /// Tres alumnos (María, Juan, Ana) realizan actividades asignadas por el profesional.
    /// Se simulan aciertos (alta precisión) y errores (baja precisión / frustración alta)
    /// para comprobar el cálculo exacto de métricas (TotalCompletadas, PromedioÉxito, AlertasFrustración).
    /// </summary>
    public class ThreeStudentsMetricsTestingScenarioTests
    {
        private readonly IAssignmentsRepository _assignmentsRepo = Substitute.For<IAssignmentsRepository>();
        private readonly IActivityAssignmentRepository _activityAssignmentRepo = Substitute.For<IActivityAssignmentRepository>();
        private readonly IRoadmapRepository _roadmapRepo = Substitute.For<IRoadmapRepository>();
        private readonly IProfessionalsRepository _profRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IBackgroundJobRepository _backgroundJobRepo = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfessionalId = Guid.NewGuid();
        private static readonly Guid Student1Id = Guid.NewGuid(); // María
        private static readonly Guid Student2Id = Guid.NewGuid(); // Juan
        private static readonly Guid Student3Id = Guid.NewGuid(); // Ana

        private static readonly DateTime Now = new(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);

        public ThreeStudentsMetricsTestingScenarioTests()
        {
            _dateTime.UtcNow.Returns(Now);
            _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted_string");
        }

        [Fact]
        public async Task ThreeStudents_CompletingActivitiesWithSuccessesAndErrors_CalculatesMetricsCorrectly()
        {
            // Arrange — 3 alumnos asignados al profesional
            var studentsList = new List<ProfessionalPerson>
            {
                new() { ProfessionalId = ProfessionalId, PersonId = Student1Id, IsActive = true },
                new() { ProfessionalId = ProfessionalId, PersonId = Student2Id, IsActive = true },
                new() { ProfessionalId = ProfessionalId, PersonId = Student3Id, IsActive = true }
            };

            _assignmentsRepo.GetPersonsByProfessionalIdAsync(ProfessionalId, Arg.Any<CancellationToken>())
                .Returns(studentsList);

            // Actividades completadas por los tres alumnos:
            // Alumno 1 (María - Aciertos): 95% y 85%
            // Alumno 2 (Juan - Desempeño medio/Error): 60% y 40% (Frustración 4 -> Alerta)
            // Alumno 3 (Ana - Error/Recuperación): 25% (Frustración 5 -> Alerta) y 75%
            var responsesMap = new Dictionary<Guid, List<ActivityResponse>>
            {
                {
                    Student1Id, new List<ActivityResponse>
                    {
                        new() { Id = 1, CompletedAt = Now.AddDays(-1), SuccessPercentage = 95m, FrustrationLevel = 1, Result = ActivityResponseResult.Exito },
                        new() { Id = 2, CompletedAt = Now.AddDays(-2), SuccessPercentage = 85m, FrustrationLevel = 1, Result = ActivityResponseResult.Exito }
                    }
                },
                {
                    Student2Id, new List<ActivityResponse>
                    {
                        new() { Id = 3, CompletedAt = Now.AddDays(-1), SuccessPercentage = 60m, FrustrationLevel = 3, Result = ActivityResponseResult.Parcial },
                        new() { Id = 4, CompletedAt = Now.AddDays(-3), SuccessPercentage = 40m, FrustrationLevel = 4, Result = ActivityResponseResult.Fallido } // Alerta 1
                    }
                },
                {
                    Student3Id, new List<ActivityResponse>
                    {
                        new() { Id = 5, CompletedAt = Now.AddDays(-2), SuccessPercentage = 25m, FrustrationLevel = 5, Result = ActivityResponseResult.Fallido }, // Alerta 2
                        new() { Id = 6, CompletedAt = Now.AddDays(-4), SuccessPercentage = 75m, FrustrationLevel = 2, Result = ActivityResponseResult.Parcial }
                    }
                }
            };

            _activityAssignmentRepo.GetRecentCompletedResponsesByPersonIdsAsync(
                Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 3), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(responsesMap);

            var queryHandler = new GetWeeklyProgressQueryHandler(_assignmentsRepo, _activityAssignmentRepo, _dateTime);

            // Act
            var result = await queryHandler.HandleAsync(new GetWeeklyProgressQuery(ProfessionalId), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.PersonCount.Should().Be(3);
            result.Data.TotalCompleted.Should().Be(6);

            // Promedio: (95 + 85 + 60 + 40 + 25 + 75) / 6 = 380 / 6 = 63.333... -> Redondeado a 0 decimales = 63%
            result.Data.AvgSuccess.Should().Be(63m);

            // Alertas de frustración: Juan (nivel 4) y Ana (nivel 5) -> Total 2 alertas
            result.Data.FrustrationAlerts.Should().Be(2);
        }

        [Theory]
        [InlineData(-10, 10, 2, "El porcentaje de éxito debe estar entre 0 y 100.")]
        [InlineData(150, 10, 2, "El porcentaje de éxito debe estar entre 0 y 100.")]
        [InlineData(80, -5, 2, "El tiempo transcurrido no puede ser negativo.")]
        [InlineData(80, 10, 0, "El nivel de frustración debe estar entre 1 y 5.")]
        [InlineData(80, 10, 6, "El nivel de frustración debe estar entre 1 y 5.")]
        public async Task CompleteActivityResponse_WithInvalidInputs_ReturnsValidationError(
            decimal successPercentage, int timeSpentSeconds, int frustrationLevel, string expectedErrorMessage)
        {
            // Arrange
            var assignmentId = 10;
            var responseId = 100;
            var assignment = new ActivityAssignment { Id = assignmentId, PersonId = Student1Id };
            var response = new ActivityResponse { Id = responseId, AssignmentId = assignmentId, CompletedAt = null };

            _activityAssignmentRepo.GetByIdAsync(assignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
            _activityAssignmentRepo.GetResponseByIdAsync(responseId, Arg.Any<CancellationToken>()).Returns(response);

            var handler = new CompleteActivityResponseCommandHandler(
                _activityAssignmentRepo, _roadmapRepo, _profRepo, _backgroundJobRepo, _unitOfWork, _dateTime, _encryption);

            var command = new CompleteActivityResponseCommand(
                assignmentId, responseId, Student1Id, successPercentage, timeSpentSeconds, false, frustrationLevel, null, null);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public async Task CompleteActivityResponse_WithValidSuccessAndErrors_StoresCorrectResultStatus()
        {
            // Test mapping of percentages to Result enum:
            // >= 80 -> Exito
            // >= 50 -> Parcial
            // < 50  -> Fallido

            var handler = new CompleteActivityResponseCommandHandler(
                _activityAssignmentRepo, _roadmapRepo, _profRepo, _backgroundJobRepo, _unitOfWork, _dateTime, _encryption);

            // Case 1: Exito (85%)
            await TestSingleCompletionResult(handler, 85m, ActivityResponseResult.Exito);

            // Case 2: Parcial (65%)
            await TestSingleCompletionResult(handler, 65m, ActivityResponseResult.Parcial);

            // Case 3: Fallido / Error (35%)
            await TestSingleCompletionResult(handler, 35m, ActivityResponseResult.Fallido);
        }

        private async Task TestSingleCompletionResult(
            CompleteActivityResponseCommandHandler handler, decimal successPercentage, ActivityResponseResult expectedResult)
        {
            var assignmentId = 1;
            var responseId = 1;
            var assignment = new ActivityAssignment { Id = assignmentId, PersonId = Student1Id };
            var response = new ActivityResponse { Id = responseId, AssignmentId = assignmentId, CompletedAt = null };

            _activityAssignmentRepo.GetByIdAsync(assignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
            _activityAssignmentRepo.GetResponseByIdAsync(responseId, Arg.Any<CancellationToken>()).Returns(response);

            var command = new CompleteActivityResponseCommand(
                assignmentId, responseId, Student1Id, successPercentage, 60, false, 2, "pattern", "obs");

            var result = await handler.HandleAsync(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            response.Result.Should().Be(expectedResult);
        }
    }
}
