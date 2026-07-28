using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class CreateReportCommandHandler : ICommandHandler<CreateReportCommand, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateReportCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public CreateReportCommandHandler(
            IReportsRepository repository,
            IPersonsRepository personsRepository,
            IProfessionalsRepository professionalsRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateReportCommandHandler> logger,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _personsRepository = personsRepository;
            _professionalsRepository = professionalsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(
            CreateReportCommand command,
            CancellationToken cancellationToken)
        {
            var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (person == null)
            {
                return ApiResponse<ReportResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    "Persona no encontrada.");
            }

            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional == null)
            {
                return ApiResponse<ReportResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    "Profesional no encontrado.");
            }

            var report = new Report
            {
                PersonId = command.PersonId,
                ProfessionalId = command.ProfessionalId,
                Title = command.Title,
                Content = command.Content,
                ReportTypeId = command.ReportTypeId,
                ReportDate = command.ReportDate,
                PeriodStartDate = command.PeriodStartDate,
                PeriodEndDate = command.PeriodEndDate,
                AchievedGoals = command.AchievedGoals,
                AreasToReinforce = command.AreasToReinforce,
                FutureRecommendations = command.FutureRecommendations,
                NextObjectives = command.NextObjectives,
                Status = ReportStatus.Draft,
                IsActive = true,
                CreatedAt = _dateTime.UtcNow,
                UpdatedAt = _dateTime.UtcNow
            };

            var created = await _repository.CreateAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Report created: {ReportId} for person {PersonId} by professional {ProfessionalId}",
                created.Id, created.PersonId, created.ProfessionalId);

            var response = ReportResponse.MapToResponse(created);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(created.Id.ToString()));
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s)
            => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}