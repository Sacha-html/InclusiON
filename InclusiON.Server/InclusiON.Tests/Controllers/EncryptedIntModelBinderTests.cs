using FluentAssertions;
using System.Globalization;
using InclusiON.Api.ModelBinders;
using InclusiON.Data.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace InclusiON.Tests.Controllers;

public class EncryptedIntModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_EncryptedToken_ResolvesInternalId()
    {
        EncryptionAccessor.Initialize(value => value, value => value == "token" ? "42" : value);
        var context = CreateContext("ENC:dG9rZW4");

        await new EncryptedIntModelBinder().BindModelAsync(context);

        context.Result.Model.Should().Be(42);
        context.ModelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task BindModelAsync_InvalidToken_AddsModelErrorWithoutResolvingAnId()
    {
        EncryptionAccessor.Initialize(value => value, _ => throw new FormatException("invalid token"));
        var context = CreateContext("not-an-encrypted-id");

        await new EncryptedIntModelBinder().BindModelAsync(context);

        context.Result.IsModelSet.Should().BeFalse();
        context.ModelState.IsValid.Should().BeFalse();
        context.ModelState["id"]!.Errors.Should().ContainSingle();
    }

    private static DefaultModelBindingContext CreateContext(string value)
    {
        var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int));
        var context = new DefaultModelBindingContext
        {
            ModelMetadata = metadata,
            ModelName = "id",
            ModelState = new ModelStateDictionary(),
            ValueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(new Dictionary<string, StringValues> { ["id"] = value }),
                CultureInfo.InvariantCulture),
        };
        return context;
    }
}
