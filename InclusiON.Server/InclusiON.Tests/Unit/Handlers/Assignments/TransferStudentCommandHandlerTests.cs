using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses.Assignments;
using InclusiON.Application.Auditing;

namespace InclusiON.Tests.Unit.Handlers.Assignments
{
    public class TransferStudentCommandHandlerTests
    {
        private readonly IAssignmentsRepository _assignmentsRepo = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository _professionalsRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IActivityAssignmentRepository _activityAssignmentRepo = Substitute.For<IActivityAssignmentRepository>();
        private readonly IReportsRepository _reportsRepo = Substitute.For<IReportsRepository>();
        private readonly IAccessAuditLogger _auditLogger = Substitute.For<IAccessAuditLogger>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

        private TransferStudentCommandHandler BuildSut() =>
            new(_assignmentsRepo, _professionalsRepo, _personsRepo, _activityAssignmentRepo, _reportsRepo, _auditLogger, _dateTime, _unitOfWork);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid FromProfId = Guid.NewGuid();
        private static readonly Guid ToProfId = Guid.NewGuid();
        private static readonly Guid AdminUserId = Guid.NewGuid();

        private static TransferStudentCommand Cmd() =>
            new(PersonId, FromProfId, ToProfId, AdminUserId, "Admin");

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsError()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task HandleAsync_FromProfessionalNotFound_ReturnsError()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability());
            _professionalsRepo.GetByIdAsync(FromProfId, Arg.Any<CancellationToken>())
                              .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        [Fact]
        public async Task HandleAsync_ToProfessionalNotFound_ReturnsError()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability());
            _professionalsRepo.GetByIdAsync(FromProfId, Arg.Any<CancellationToken>())
                              .Returns(new Professional());
            _professionalsRepo.GetByIdAsync(ToProfId, Arg.Any<CancellationToken>())
                              .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        [Fact]
        public async Task HandleAsync_ToProfessionalNotApproved_ReturnsError()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability());
            _professionalsRepo.GetByIdAsync(FromProfId, Arg.Any<CancellationToken>())
                              .Returns(new Professional());
            _professionalsRepo.GetByIdAsync(ToProfId, Arg.Any<CancellationToken>())
                              .Returns(new Professional { Status = ProfessionalStatusEnum.Pending });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        [Fact]
        public async Task HandleAsync_LinkNotFound_ReturnsError()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new PersonWithDisability());
            _professionalsRepo.GetByIdAsync(FromProfId, Arg.Any<CancellationToken>())
                              .Returns(new Professional());
            _professionalsRepo.GetByIdAsync(ToProfId, Arg.Any<CancellationToken>())
                              .Returns(new Professional { Status = ProfessionalStatusEnum.Approved });
            _assignmentsRepo.GetAssignmentAsync(FromProfId, PersonId, Arg.Any<CancellationToken>())
                            .Returns((ProfessionalPerson?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_PerformsTransferAndReassigns()
        {
            var person = new PersonWithDisability { FirstName = "Juan", LastName = "Perez" };
            var fromProf = new Professional { FirstName = "Dr.", LastName = "Mendez" };
            var toProf = new Professional { Status = ProfessionalStatusEnum.Approved, FirstName = "Dra.", LastName = "Gomez" };
            var oldLink = new ProfessionalPerson { PersonId = PersonId, ProfessionalId = FromProfId, IsActive = true, IsPrimaryProfessional = true };

            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);
            _professionalsRepo.GetByIdAsync(FromProfId, Arg.Any<CancellationToken>()).Returns(fromProf);
            _professionalsRepo.GetByIdAsync(ToProfId, Arg.Any<CancellationToken>()).Returns(toProf);
            _assignmentsRepo.GetAssignmentAsync(FromProfId, PersonId, Arg.Any<CancellationToken>()).Returns(oldLink);
            _assignmentsRepo.GetAssignmentAsync(ToProfId, PersonId, Arg.Any<CancellationToken>()).Returns((ProfessionalPerson?)null);

            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);

            var activity1 = new ActivityAssignment { AssignedByProfessionalId = FromProfId, StatusId = AssignmentStatuses.Pendiente };
            var activity2 = new ActivityAssignment { AssignedByProfessionalId = FromProfId, StatusId = AssignmentStatuses.EnProgreso };
            var activity3 = new ActivityAssignment { AssignedByProfessionalId = FromProfId, StatusId = AssignmentStatuses.Completada }; // Should not reassign completed

            _activityAssignmentRepo.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                                   .Returns(new List<ActivityAssignment> { activity1, activity2, activity3 });

            var report1 = new Report { ProfessionalId = FromProfId, Status = ReportStatus.Draft };
            var report2 = new Report { ProfessionalId = FromProfId, Status = ReportStatus.Submitted };
            var report3 = new Report { ProfessionalId = FromProfId, Status = ReportStatus.Approved }; // Should not reassign approved

            _reportsRepo.GetPagedAsync(
                page: 1, pageSize: 999, search: null, personId: PersonId.ToString(), professionalId: FromProfId.ToString(),
                reportTypeId: null, isActive: true, onlyDeactivatedProfessionals: null, status: null, dateFrom: null, dateTo: null,
                sortBy: null, sortDirection: "ASC", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<Report> { Data = new List<Report> { report1, report2, report3 } });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            oldLink.IsActive.Should().BeFalse();

            // Active activities reassigned
            activity1.AssignedByProfessionalId.Should().Be(ToProfId);
            activity2.AssignedByProfessionalId.Should().Be(ToProfId);
            activity3.AssignedByProfessionalId.Should().Be(FromProfId); // remains unchanged

            // Reports reassigned (reassigned reports should have called ReassignReportAsync)
            await _reportsRepo.Received(1).ReassignReportAsync(report1, ToProfId, now, Arg.Any<CancellationToken>());
            await _reportsRepo.Received(1).ReassignReportAsync(report2, ToProfId, now, Arg.Any<CancellationToken>());
            await _reportsRepo.DidNotReceive().ReassignReportAsync(report3, ToProfId, now, Arg.Any<CancellationToken>());

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _auditLogger.Received(1).LogAsync(Arg.Any<AccessAuditEntry>(), Arg.Any<CancellationToken>());
        }
    }
}
