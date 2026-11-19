namespace InclusiON.Domain.Enums;

public static class NotificationTypes
{
    public const string Calendar   = "calendar";
    public const string Activity   = "activity";
    public const string Report     = "report";
    public const string Message    = "message";
    public const string Assignment = "assignment";
    public const string Alert      = "alert";
    public const string System     = "system";
}

public static class NotificationEventCodes
{
    // Calendario
    public const string CalendarEventCreated = "calendar.created";
    public const string CalendarEventUpdated = "calendar.updated";
    public const string CalendarEventDeleted = "calendar.deleted";

    // Actividades y Roadmap
    public const string ActivityAssigned     = "activity.assigned";
    public const string RoadmapUnlocked      = "roadmap.unlocked";
    public const string ActivityCompleted    = "activity.completed";
    public const string ActivityAttemptAlert = "activity.attempt_alert";
    public const string FrustrationAlert     = "adaptive.frustration_alert";

    // Reportes
    public const string ReportSubmitted      = "report.submitted";
    public const string ReportApproved       = "report.approved";
    public const string ReportRejected       = "report.rejected";
    public const string ReportReassigned     = "report.reassigned";

    // Mensajería
    public const string MessageReceived      = "message.received";
    public const string MessageReplied       = "message.replied";

    // Asignaciones y Transferencias
    public const string StudentAssigned      = "assignment.student_assigned";
    public const string StudentUnassigned    = "assignment.student_unassigned";
    public const string StudentTransferred   = "assignment.student_transferred";

    // Estudiantes
    public const string StudentCreated       = "person.created";
    public const string StudentDeactivated   = "person.deactivated";
    public const string StudentReactivated   = "person.reactivated";

    // Familiares e Invitaciones
    public const string InvitationAccepted   = "invitation.accepted";
    public const string FamilyLinked         = "family.linked";
    public const string FamilyUnlinked       = "family.unlinked";
    public const string FamilyDeactivated    = "family.deactivated";
    public const string StudentSuspendedCb03 = "family.student_suspended_cb03";

    // Profesionales
    public const string ProfessionalRegistered = "professional.registered";
    public const string ProfessionalApproved   = "professional.approved";
    public const string ProfessionalsSuspended = "professional.suspended_inactive";

    // Seguridad
    public const string PinAccountLocked     = "security.pin_locked";
}
