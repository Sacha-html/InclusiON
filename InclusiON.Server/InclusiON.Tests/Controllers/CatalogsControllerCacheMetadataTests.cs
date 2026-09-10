using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Controllers;
using Xunit;

namespace InclusiON.Tests.Controllers;

public class CatalogsControllerCacheMetadataTests
{
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
}
