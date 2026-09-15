using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InclusiON.DTOs.Requests.Persons;
using Xunit;

namespace InclusiON.Tests.Unit.Requests;

public class UpdatePersonFunctionalProfileRequestValidationTests
{
    [Fact]
    public void AcceptsValuesAtTheConfiguredLimits()
    {
        var request = new UpdatePersonFunctionalProfileRequest
        {
            InterestsAndMotivators = new string('i', 500),
            LearningStyle = new string('l', 250),
            AvailableResources = new string('r', 255),
            AdditionalTherapies = new string('t', 500),
        };

        Validate(request).Should().BeEmpty();
    }

    [Theory]
    [InlineData(nameof(UpdatePersonFunctionalProfileRequest.InterestsAndMotivators), 501)]
    [InlineData(nameof(UpdatePersonFunctionalProfileRequest.LearningStyle), 251)]
    [InlineData(nameof(UpdatePersonFunctionalProfileRequest.AvailableResources), 256)]
    [InlineData(nameof(UpdatePersonFunctionalProfileRequest.AdditionalTherapies), 501)]
    public void RejectsValuesAboveTheConfiguredLimit(string propertyName, int length)
    {
        var request = new UpdatePersonFunctionalProfileRequest();
        typeof(UpdatePersonFunctionalProfileRequest).GetProperty(propertyName)!
            .SetValue(request, new string('x', length));

        Validate(request).Should().Contain(error => error.MemberNames.Contains(propertyName));
    }

    private static IReadOnlyCollection<ValidationResult> Validate(UpdatePersonFunctionalProfileRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }
}
