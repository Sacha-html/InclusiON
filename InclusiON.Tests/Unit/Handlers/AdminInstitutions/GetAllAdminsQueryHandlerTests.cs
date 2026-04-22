using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Handlers;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.AdminInstitutions
{
    public class GetAllAdminsQueryHandlerTests
    {
        private readonly IAdminInstitutionRepository _repository = Substitute.For<IAdminInstitutionRepository>();
        private GetAllAdminsQueryHandler BuildSut() => new(_repository);

        [Fact]
        public async Task HandleAsync_NoAdmins_ReturnsEmptyList()
        {
            _repository.GetAllAdminsWithInstitutionsAsync(Arg.Any<CancellationToken>())
                       .Returns([]);

            var result = await BuildSut().HandleAsync(new GetAllAdminsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_AdminWithNoInstitutions_IsGlobalAdmin()
        {
            var admin = new User { Id = Guid.NewGuid(), Name = "Ana", Surname = "Lopez", Email = "ana@test.com" };
            _repository.GetAllAdminsWithInstitutionsAsync(Arg.Any<CancellationToken>())
                       .Returns([admin]);

            var result = await BuildSut().HandleAsync(new GetAllAdminsQuery(), default);

            result.Data!.Single().IsGlobalAdmin.Should().BeTrue();
            result.Data.Single().Institutions.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_AdminWithInstitutions_IsNotGlobalAdmin()
        {
            var institution = new EducationalInstitution { Id = 1, Name = "Escuela N° 1" };
            var admin = new User { Id = Guid.NewGuid(), Name = "Pedro", Surname = "Gomez", Email = "pedro@test.com" };
            admin.AdminInstitutions.Add(new AdminInstitution
            {
                AdminUserId   = admin.Id,
                InstitutionId = 1,
                Institution   = institution,
                IsActive      = true
            });

            _repository.GetAllAdminsWithInstitutionsAsync(Arg.Any<CancellationToken>())
                       .Returns([admin]);

            var result = await BuildSut().HandleAsync(new GetAllAdminsQuery(), default);

            var dto = result.Data!.Single();
            dto.IsGlobalAdmin.Should().BeFalse();
            dto.Institutions.Should().HaveCount(1);
            dto.Institutions[0].InstitutionName.Should().Be("Escuela N° 1");
        }

        [Fact]
        public async Task HandleAsync_MapsNameAndEmailCorrectly()
        {
            var admin = new User { Id = Guid.NewGuid(), Name = "Luis", Surname = "Martinez", Email = "luis@test.com", IsActive = true };
            _repository.GetAllAdminsWithInstitutionsAsync(Arg.Any<CancellationToken>())
                       .Returns([admin]);

            var result = await BuildSut().HandleAsync(new GetAllAdminsQuery(), default);

            var dto = result.Data!.Single();
            dto.Name.Should().Be("Luis");
            dto.Surname.Should().Be("Martinez");
            dto.Email.Should().Be("luis@test.com");
            dto.IsActive.Should().BeTrue();
        }
    }
}
