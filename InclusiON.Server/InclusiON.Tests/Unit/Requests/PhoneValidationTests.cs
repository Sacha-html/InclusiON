using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InclusiON.DTOs.Requests.Family;
using InclusiON.DTOs.Requests.Institutions;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Requests.Professionals;
using Xunit;

namespace InclusiON.Tests.Unit.Requests
{
    public class PhoneValidationTests
    {
        private static IReadOnlyCollection<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results;
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData("1144556677")]
        [InlineData(null)]
        public void CreatePersonWithTutorRequest_ValidPhone_PassesValidation(string? phone)
        {
            var request = new CreatePersonWithTutorRequest
            {
                Student = new CreatePersonRequest
                {
                    FirstName = "Juan",
                    LastName = "Pérez",
                    BirthDate = DateTime.UtcNow.AddYears(-15)
                },
                TutorFirstName = "María",
                TutorLastName = "Pérez",
                TutorEmail = "maria@test.com",
                TutorRelationship = "Madre",
                ProfessionalId = Guid.NewGuid(),
                TutorPhone = phone
            };

            var errors = Validate(request);
            errors.Should().NotContain(e => e.MemberNames.Contains(nameof(CreatePersonWithTutorRequest.TutorPhone)));
        }

        [Theory]
        [InlineData("abcde")]
        [InlineData("1234a")]
        [InlineData("+5491112345678")]
        [InlineData("11 1234-5678")]
        [InlineData("phone#123")]
        public void CreatePersonWithTutorRequest_InvalidPhone_FailsValidation(string invalidPhone)
        {
            var request = new CreatePersonWithTutorRequest
            {
                Student = new CreatePersonRequest
                {
                    FirstName = "Juan",
                    LastName = "Pérez",
                    BirthDate = DateTime.UtcNow.AddYears(-15)
                },
                TutorFirstName = "María",
                TutorLastName = "Pérez",
                TutorEmail = "maria@test.com",
                TutorRelationship = "Madre",
                ProfessionalId = Guid.NewGuid(),
                TutorPhone = invalidPhone
            };

            var errors = Validate(request);
            errors.Should().Contain(e => e.MemberNames.Contains(nameof(CreatePersonWithTutorRequest.TutorPhone)));
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData(null)]
        public void ProfessionalRequests_ValidPhone_PassesValidation(string? phone)
        {
            var createReq = new CreateProfessionalRequest
            {
                FirstName = "Ana",
                LastName = "Gómez",
                Email = "ana@test.com",
                DocumentNumber = "12345678",
                SpecialtyId = 1,
                Phone = phone
            };
            Validate(createReq).Should().NotContain(e => e.MemberNames.Contains(nameof(CreateProfessionalRequest.Phone)));

            var registerReq = new RegisterProfessionalRequest
            {
                FirstName = "Ana",
                LastName = "Gómez",
                Email = "ana@test.com",
                DocumentNumber = "12345678",
                Specialty = "Fonoaudiología",
                Phone = phone
            };
            Validate(registerReq).Should().NotContain(e => e.MemberNames.Contains(nameof(RegisterProfessionalRequest.Phone)));

            var updateReq = new UpdateProfessionalRequest { Phone = phone };
            Validate(updateReq).Should().NotContain(e => e.MemberNames.Contains(nameof(UpdateProfessionalRequest.Phone)));
        }

        [Theory]
        [InlineData("abcde")]
        [InlineData("1234x")]
        [InlineData("+54 11")]
        [InlineData("123-456")]
        public void ProfessionalRequests_InvalidPhone_FailsValidation(string invalidPhone)
        {
            var createReq = new CreateProfessionalRequest
            {
                FirstName = "Ana",
                LastName = "Gómez",
                Email = "ana@test.com",
                DocumentNumber = "12345678",
                SpecialtyId = 1,
                Phone = invalidPhone
            };
            Validate(createReq).Should().Contain(e => e.MemberNames.Contains(nameof(CreateProfessionalRequest.Phone)));

            var registerReq = new RegisterProfessionalRequest
            {
                FirstName = "Ana",
                LastName = "Gómez",
                Email = "ana@test.com",
                DocumentNumber = "12345678",
                Specialty = "Fonoaudiología",
                Phone = invalidPhone
            };
            Validate(registerReq).Should().Contain(e => e.MemberNames.Contains(nameof(RegisterProfessionalRequest.Phone)));

            var updateReq = new UpdateProfessionalRequest { Phone = invalidPhone };
            Validate(updateReq).Should().Contain(e => e.MemberNames.Contains(nameof(UpdateProfessionalRequest.Phone)));
        }

        [Theory]
        [InlineData("abcde")]
        [InlineData("1234-5678")]
        [InlineData("phone")]
        public void FamilyAndInstitutionRequests_InvalidPhone_FailsValidation(string invalidPhone)
        {
            var familyCreate = new CreateFamilyRequest
            {
                FirstName = "Carlos",
                LastName = "López",
                Email = "carlos@test.com",
                Relationship = "Padre",
                PersonId = Guid.NewGuid(),
                Phone = invalidPhone
            };
            Validate(familyCreate).Should().Contain(e => e.MemberNames.Contains(nameof(CreateFamilyRequest.Phone)));

            var familyUpdate = new UpdateFamilyRequest
            {
                FirstName = "Carlos",
                LastName = "López",
                Email = "carlos@test.com",
                Phone = invalidPhone
            };
            Validate(familyUpdate).Should().Contain(e => e.MemberNames.Contains(nameof(UpdateFamilyRequest.Phone)));

            var instCreate = new CreateInstitutionRequest
            {
                Name = "Instituto A",
                Email = "inst@test.com",
                Phone = invalidPhone
            };
            Validate(instCreate).Should().Contain(e => e.MemberNames.Contains(nameof(CreateInstitutionRequest.Phone)));

            var instUpdate = new UpdateInstitutionRequest
            {
                Name = "Instituto A",
                Email = "inst@test.com",
                Phone = invalidPhone
            };
            Validate(instUpdate).Should().Contain(e => e.MemberNames.Contains(nameof(UpdateInstitutionRequest.Phone)));
        }
    }
}
