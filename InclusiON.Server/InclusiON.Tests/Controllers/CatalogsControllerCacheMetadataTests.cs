using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Api.Controllers;
using Xunit;

namespace InclusiON.Tests.Controllers;

public class CatalogsControllerCacheMetadataTests
{
    [Fact]
    public void GetDisabilityTypes_DisablesHttpAndOutputCaching()
    {
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetDisabilityTypes));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>().Single();
        var outputCache = method.GetCustomAttributes(typeof(OutputCacheAttribute), inherit: true)
            .Cast<OutputCacheAttribute>().Single();

        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
        outputCache.NoStore.Should().BeTrue();
    }

    [Fact]
    public void GetSpecialties_DisablesHttpResponseCaching()
    {
        var controllerResponseCache = typeof(CatalogsController).GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>()
            .Single();
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetSpecialties));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>()
            .Single();

        controllerResponseCache.Duration.Should().Be(300);
        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
    }

    [Fact]
    public void GetSkillAreas_DisablesHttpAndOutputCaching()
    {
        var controllerResponseCache = typeof(CatalogsController).GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>()
            .Single();
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetSkillAreas));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>()
            .Single();
        var outputCache = method.GetCustomAttributes(typeof(OutputCacheAttribute), inherit: true)
            .Cast<OutputCacheAttribute>()
            .Single();

        controllerResponseCache.Duration.Should().Be(300);
        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
        outputCache.NoStore.Should().BeTrue();
    }

    [Fact]
    public void GetAutonomyLevels_DisablesHttpAndOutputCaching()
    {
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetAutonomyLevels));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>().Single();
        var outputCache = method.GetCustomAttributes(typeof(OutputCacheAttribute), inherit: true)
            .Cast<OutputCacheAttribute>().Single();

        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
        outputCache.NoStore.Should().BeTrue();
    }

    [Fact]
    public void GetActivityCategories_DisablesHttpAndOutputCaching()
    {
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetActivityCategories));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>().Single();
        var outputCache = method.GetCustomAttributes(typeof(OutputCacheAttribute), inherit: true)
            .Cast<OutputCacheAttribute>().Single();

        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
        outputCache.NoStore.Should().BeTrue();
    }

    [Fact]
    public void GetActivityTemplateTypes_DisablesHttpAndOutputCaching()
    {
        var method = typeof(CatalogsController).GetMethod(nameof(CatalogsController.GetActivityTemplateTypes));
        var responseCache = method!.GetCustomAttributes(typeof(ResponseCacheAttribute), inherit: true)
            .Cast<ResponseCacheAttribute>().Single();
        var outputCache = method.GetCustomAttributes(typeof(OutputCacheAttribute), inherit: true)
            .Cast<OutputCacheAttribute>().Single();

        responseCache.NoStore.Should().BeTrue();
        responseCache.Location.Should().Be(ResponseCacheLocation.None);
        outputCache.NoStore.Should().BeTrue();
    }
}
