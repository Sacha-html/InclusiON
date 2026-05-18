using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Workers;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Agents;

public class JobExecutorTests
{
    readonly IBackgroundJobRepository _repo = Substitute.For<IBackgroundJobRepository>();

    static BackgroundJob Job(int typeId = 1, int retryCount = 0, int maxRetries = 3) => new()
    {
        Id = 1,
        JobTypeId = typeId,
        StatusId = 2,
        Payload = "{}",
        RetryCount = retryCount,
        MaxRetries = maxRetries
    };

    [Fact]
    public async Task CompletesJob_WhenHandlerSucceeds()
    {
        var handler = Substitute.For<IJobHandler>();
        handler.JobTypeId.Returns(1);
        var sut = new JobExecutor([handler], _repo, NullLogger<JobExecutor>.Instance);

        await sut.ExecuteAsync(Job(), default);

        await _repo.Received(1).CompleteAsync(1, default);
        await _repo.DidNotReceiveWithAnyArgs().FailAsync(default, default!, default);
        await _repo.DidNotReceiveWithAnyArgs().RetryAsync(default, default!, default);
    }

    [Fact]
    public async Task Retries_WhenRetryCountBelowMax()
    {
        var handler = Substitute.For<IJobHandler>();
        handler.JobTypeId.Returns(1);
        handler.When(x => x.HandleAsync(Arg.Any<BackgroundJob>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("fail"));
        var sut = new JobExecutor([handler], _repo, NullLogger<JobExecutor>.Instance);

        await sut.ExecuteAsync(Job(retryCount: 1, maxRetries: 3), default);

        await _repo.Received(1).RetryAsync(1, Arg.Any<string>(), default);
        await _repo.DidNotReceive().FailAsync(Arg.Any<int>(), Arg.Any<string>(), default);
    }

    [Fact]
    public async Task Fails_WhenRetryCountExhausted()
    {
        var handler = Substitute.For<IJobHandler>();
        handler.JobTypeId.Returns(1);
        handler.When(x => x.HandleAsync(Arg.Any<BackgroundJob>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("fail"));
        var sut = new JobExecutor([handler], _repo, NullLogger<JobExecutor>.Instance);

        await sut.ExecuteAsync(Job(retryCount: 3, maxRetries: 3), default);

        await _repo.Received(1).FailAsync(1, Arg.Any<string>(), default);
        await _repo.DidNotReceive().RetryAsync(Arg.Any<int>(), Arg.Any<string>(), default);
    }

    [Fact]
    public async Task Fails_WhenNoHandlerFound()
    {
        var sut = new JobExecutor([], _repo, NullLogger<JobExecutor>.Instance);

        await sut.ExecuteAsync(Job(typeId: 99), default);

        await _repo.Received(1).FailAsync(1, Arg.Is<string>(s => s.Contains("99")), default);
    }
}
