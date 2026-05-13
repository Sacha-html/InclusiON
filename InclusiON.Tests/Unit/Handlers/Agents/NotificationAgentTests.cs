using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Agents;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Agents;

public class NotificationAgentTests
{
    readonly IRealTimeNotifier _notifier = Substitute.For<IRealTimeNotifier>();
    readonly IEmailService _emailService = Substitute.For<IEmailService>();
    readonly ILogger<NotificationAgent> _logger = NullLogger<NotificationAgent>.Instance;

    NotificationAgent BuildSut() => new(_notifier, _emailService, _logger);

    static BackgroundJob CreateJob(NotificationPayload p) => new()
    {
        Id = 1,
        JobTypeId = JobTypes.Push,
        StatusId = BackgroundJobStatuses.Running,
        Payload = JsonSerializer.Serialize(p),
        RetryCount = 0,
        MaxRetries = 3
    };

    [Fact]
    public async Task SendsViaSignalR()
    {
        var payload = new NotificationPayload { UserId = "user1", Title = "Test", Message = "Hello" };
        var sut = BuildSut();

        await sut.HandleAsync(CreateJob(payload), default);

        await _notifier.Received(1).NotifyUserAsync("user1", "Test", "Hello", null, default);
    }

    [Fact]
    public async Task SendsEmailFallback_WhenConfigured()
    {
        var payload = new NotificationPayload
        {
            UserId = "user1",
            Title = "Test",
            Message = "Hello",
            Email = "a@b.com",
            SendEmailFallback = true
        };
        _emailService.SendTemplatedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string?>>(), Arg.Any<CancellationToken>()).Returns(true);
        var sut = BuildSut();

        await sut.HandleAsync(CreateJob(payload), default);

        await _emailService.Received(1)
            .SendTemplatedEmailAsync("a@b.com", "Test", "notification",
                Arg.Is<Dictionary<string, string?>>(d => d["{{Title}}"] == "Test"), default);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenFallbackDisabled()
    {
        var payload = new NotificationPayload
        {
            UserId = "user1",
            Title = "Test",
            Message = "Hello",
            SendEmailFallback = false
        };
        var sut = BuildSut();

        await sut.HandleAsync(CreateJob(payload), default);

        await _emailService.DidNotReceiveWithAnyArgs()
            .SendTemplatedEmailAsync(default!, default!, default!, default!, default);
    }

    [Fact]
    public async Task DoesNotSendEmail_WhenEmailIsNull()
    {
        var payload = new NotificationPayload
        {
            UserId = "user1",
            Title = "Test",
            Message = "Hello",
            SendEmailFallback = true,
            Email = null
        };
        var sut = BuildSut();

        await sut.HandleAsync(CreateJob(payload), default);

        await _emailService.DidNotReceiveWithAnyArgs()
            .SendTemplatedEmailAsync(default!, default!, default!, default!, default);
    }

    [Fact]
    public async Task Throws_OnInvalidPayload()
    {
        var job = new BackgroundJob
        {
            Id = 1,
            JobTypeId = JobTypes.Push,
            StatusId = BackgroundJobStatuses.Running,
            Payload = "not-json"
        };
        var sut = BuildSut();

        await Assert.ThrowsAsync<JsonException>(() => sut.HandleAsync(job, default));
    }
}
