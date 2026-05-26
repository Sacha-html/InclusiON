using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class GetAdminUsersQueryHandlerTests
    {
        private readonly IRawDbExecutor _db = Substitute.For<IRawDbExecutor>();

        private GetAdminUsersQueryHandler BuildSut() => new(_db);

        private static GetAdminUsersQuery DefaultQuery() =>
            new(Page: 1, PageSize: 10, Search: null, Role: null, IsActive: null,
                SortBy: null, SortDirection: "DESC");

        [Fact]
        public async Task ReturnsPagedResponse_WithCorrectTotals()
        {
            _db.ExecuteScalarAsync<int>(
                    Arg.Any<string>(),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
                .Returns(25);

            _db.QueryAsync(
                    Arg.Any<string>(),
                    Arg.Any<Func<System.Data.IDataReader, AdminUserListItemResponse>>(),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
                .Returns(new List<AdminUserListItemResponse>
                {
                    new() { Email = "admin@test.com", Role = "GlobalAdmin" }
                });

            var result = await BuildSut().HandleAsync(DefaultQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(25);
            result.Data.TotalPages.Should().Be(3);
            result.Data.Data.Should().HaveCount(1);
            result.Data.Data[0].Email.Should().Be("admin@test.com");
        }

        [Fact]
        public async Task EmptyResult_ReturnsZeroTotals()
        {
            _db.ExecuteScalarAsync<int>(
                    Arg.Any<string>(),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
                .Returns(0);

            _db.QueryAsync(
                    Arg.Any<string>(),
                    Arg.Any<Func<System.Data.IDataReader, AdminUserListItemResponse>>(),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
                .Returns(new List<AdminUserListItemResponse>());

            var result = await BuildSut().HandleAsync(DefaultQuery(), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(0);
            result.Data.TotalPages.Should().Be(0);
            result.Data.Data.Should().BeEmpty();
        }
    }
}
