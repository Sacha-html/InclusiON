using InclusiON.Domain.Models;
using DomainActivityResponse = InclusiON.Domain.Models.ActivityResponse;

namespace InclusiON.DTOs.Responses.Activities
{
    public class ActivityAssignmentResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public string TemplateTypeCode { get; set; } = string.Empty;
        public string ContentJson { get; set; } = string.Empty;
        public Guid PersonId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsEvaluationActivity { get; set; }
        public List<ActivityAttemptResponse> Responses { get; set; } = [];

        public static ActivityAssignmentResponse From(ActivityAssignment a) => new()
        {
            Id                  = a.Id,
            ActivityId          = a.ActivityId,
            ActivityTitle       = a.Activity?.Title ?? string.Empty,
            TemplateTypeCode    = a.Activity?.Content?.TemplateType?.Code ?? string.Empty,
            ContentJson         = a.Activity?.Content?.ContentJson ?? string.Empty,
            PersonId            = a.PersonId,
            Status              = a.Status?.Name ?? a.StatusId.ToString(),
            AssignedAt          = a.AssignedAt,
            DueDate             = a.DueDate,
            IsEvaluationActivity = a.IsEvaluationActivity,
            Responses           = a.Responses
                                    .Select(ActivityAttemptResponse.From)
                                    .OrderByDescending(r => r.StartedAt)
                                    .ToList(),
        };
    }

    public class ActivityAttemptResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? TimeSpentSeconds { get; set; }
        public string? Result { get; set; }
        public decimal? SuccessPercentage { get; set; }
        public int AttemptCount { get; set; }
        public bool RequiredSupport { get; set; }
        public int? FrustrationLevel { get; set; }

        public static ActivityAttemptResponse From(DomainActivityResponse r) => new()
        {
            Id                = r.Id,
            StartedAt         = r.StartedAt,
            CompletedAt       = r.CompletedAt,
            TimeSpentSeconds  = r.TimeSpentSeconds,
            Result            = r.Result?.ToString(),
            SuccessPercentage = r.SuccessPercentage,
            AttemptCount      = r.AttemptCount,
            RequiredSupport   = r.RequiredSupport,
            FrustrationLevel  = r.FrustrationLevel,
        };
    }
}
