using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Infrastructure.Services;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Services;

public class NotificationServiceTests : DbContextTestBase
{
    private readonly NotificationsRepository _repository;
    private readonly IRealTimeNotifier _realTimeNotifier = Substitute.For<IRealTimeNotifier>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _repository = new NotificationsRepository(Db);
        _encryption.Encrypt(Arg.Any<string>()).Returns(ci => $"ENC_{ci.Arg<string>()}");
        _dateTime.UtcNow.Returns(new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc));

        _sut = new NotificationService(
            _repository,
            _realTimeNotifier,
            _encryption,
            _dateTime,
            NullLogger<NotificationService>.Instance);
    }

    [Fact]
    public async Task NotifyAsync_PersistsToDatabaseAndPushesOnceViaSignalR()
    {
        var userId = Guid.NewGuid();

        await _sut.NotifyAsync(new NotificationMessage(
            UserId: userId,
            Type: NotificationTypes.Calendar,
            EventCode: NotificationEventCodes.CalendarEventCreated,
            Title: "Nuevo evento en calendario: Tutoría",
            Message: "Prof agendó una Tutoría para joan joan.",
            ActionUrl: "/family/calendar"));

        var stored = await _repository.GetByUserIdAsync(userId);
        stored.Should().HaveCount(1);
        stored[0].Title.Should().Be("Nuevo evento en calendario: Tutoría");
        stored[0].IsRead.Should().BeFalse();
        stored[0].ActionUrl.Should().Be("/family/calendar");

        await _realTimeNotifier.Received(1).SendNotificationAsync(
            userId.ToString(),
            Arg.Is<NotificationPushPayload>(p =>
                p.Type == NotificationTypes.Calendar &&
                p.Title == "Nuevo evento en calendario: Tutoría" &&
                p.Id == $"ENC_{stored[0].Id}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyManyAsync_BlocksCalendarNotificationsForAdminRole_IN217()
    {
        var adminRoleId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var familyUserId = Guid.NewGuid();

        Db.Roles.Add(new IdentityRole<Guid> { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" });
        Db.UserRoles.Add(new IdentityUserRole<Guid> { RoleId = adminRoleId, UserId = adminUserId });
        await Db.SaveChangesAsync();

        await _sut.NotifyManyAsync([
            new NotificationMessage(adminUserId, NotificationTypes.Calendar, NotificationEventCodes.CalendarEventCreated, "Tutoría", "Mensaje", "/family/calendar"),
            new NotificationMessage(familyUserId, NotificationTypes.Calendar, NotificationEventCodes.CalendarEventCreated, "Tutoría", "Mensaje", "/family/calendar")
        ]);

        (await _repository.GetByUserIdAsync(adminUserId)).Should().BeEmpty();
        (await _repository.GetByUserIdAsync(familyUserId)).Should().HaveCount(1);
        await _realTimeNotifier.DidNotReceive().SendNotificationAsync(
            adminUserId.ToString(), Arg.Any<NotificationPushPayload>(), Arg.Any<CancellationToken>());
        await _realTimeNotifier.Received(1).SendNotificationAsync(
            familyUserId.ToString(), Arg.Any<NotificationPushPayload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Repository_StrictlyIsolatesNotificationsBetweenUsers()
    {
        var profUserId = Guid.NewGuid();
        var joanTutorUserId = Guid.NewGuid();

        await _sut.NotifyAsync(new NotificationMessage(
            profUserId,
            NotificationTypes.Report,
            NotificationEventCodes.ReportRejected,
            "Reporte rechazado",
            "Tu reporte \"Informe - Ricardo Jose\" requiere correcciones.",
            "/pro/reports/abc/edit"));

        var profNotifs = await _repository.GetByUserIdAsync(profUserId);
        var joanNotifs = await _repository.GetByUserIdAsync(joanTutorUserId);

        profNotifs.Should().HaveCount(1);
        joanNotifs.Should().BeEmpty();

        // Joan cannot mark as read or dismiss Prof's notification
        var marked = await _repository.MarkAsReadAsync(profNotifs[0].Id, joanTutorUserId, DateTime.UtcNow);
        var dismissed = await _repository.DismissAsync(profNotifs[0].Id, joanTutorUserId);

        marked.Should().BeFalse();
        dismissed.Should().BeFalse();
        (await _repository.GetUnreadCountAsync(profUserId)).Should().Be(1);
    }
}
