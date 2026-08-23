using System.Reflection;
using NetArchTest.Rules;

namespace Papyrus.Tests.Architecture;

public sealed class LayerDependencyTests
{
    private static readonly string[] WebAssemblies =
    [
        "Papyrus.Api.Admin",
        "Papyrus.Web.Blog",
        "Papyrus.Web.Portfolio",
        "Papyrus.Web.Shared"
    ];

    [Fact]
    public void Core_does_not_depend_on_infrastructure_or_web_frameworks()
    {
        var result = Types.InAssembly(typeof(Papyrus.Core.Content.BlogPost).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Papyrus.Infrastructure",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "SixLabors.ImageSharp",
                "Microsoft.Extensions.Configuration")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null
            ? "Core contains a forbidden dependency."
            : string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void Core_does_not_reference_delivery_or_application_layers()
    {
        var result = Types.InAssembly(typeof(Papyrus.Core.Content.BlogPost).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Papyrus.Application", "Papyrus.Api.Admin", "Papyrus.Web")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null
            ? "Core contains a forbidden delivery dependency."
            : string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void Public_contracts_do_not_depend_on_application_or_infrastructure_internals()
    {
        var result = Types.InAssembly(typeof(Papyrus.Contracts.Content.PublicCategory).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Papyrus.Application", "Papyrus.Infrastructure", "Papyrus.Api.Admin")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null
            ? "Contracts contains a forbidden internal dependency."
            : string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void Web_shared_is_presentation_only()
    {
        var result = Types.InAssembly(typeof(Papyrus.Web.Shared.MediaUrl).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Papyrus.Infrastructure", "Papyrus.Application", "Papyrus.Api.Admin")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null
            ? "Web.Shared contains a forbidden state-owning dependency."
            : string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void Public_delivery_layers_cannot_depend_on_admin_write_layer()
    {
        foreach (var assemblyName in WebAssemblies[1..])
        {
            var assembly = Assembly.Load(assemblyName);
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Papyrus.Api.Admin")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assemblyName} references Papyrus.Api.Admin.");
        }
    }

    [Fact]
    public void Application_does_not_depend_on_delivery_layers()
    {
        var result = Types.InAssembly(typeof(Papyrus.Application.Content.ContentCommandHandlers).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Papyrus.Api.Admin", "Papyrus.Web.Blog", "Papyrus.Web.Portfolio", "Papyrus.Web.Shared")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null
            ? "Application contains a forbidden delivery dependency."
            : string.Join(Environment.NewLine, result.FailingTypeNames));
    }
}
