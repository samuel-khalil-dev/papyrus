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

    private static readonly string[] PublicDeliveryAssemblies =
    [
        "Papyrus.Web.Blog",
        "Papyrus.Web.Portfolio"
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

    // ADR-0010 scopes Presentation access to Infrastructure. These rules make the
    // scope a ratchet: the shape the audit accepted becomes the baseline, and it may
    // not widen without a superseding ADR. Negated assertions ("nothing outside these
    // types") are expressed with reflection because NetArchTest's predicate list has
    // no negative namespace filter.

    [Fact]
    public void Public_hosts_do_not_depend_on_infrastructure_outside_their_entry_point()
    {
        foreach (var assemblyName in PublicDeliveryAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            var offenders = InfrastructureReferences(assembly)
                .Where(entry => !IsEntryPoint(entry.Type))
                .Select(entry => $"{entry.Type.FullName} -> {entry.Dependency.FullName}")
                .ToArray();

            Assert.True(offenders.Length == 0,
                $"{assemblyName} reaches Infrastructure outside its entry point: {string.Join(", ", offenders)}");
        }
    }

    [Fact]
    public void Public_hosts_cannot_reach_infrastructure_write_adapters()
    {
        // PapyrusDbContext in a public host entry point is a recorded exception
        // (ADR-0010): the feed and sitemap handlers read through it. The EF write
        // adapters are not, so no public host can obtain write capability from
        // Infrastructure at all.
        var forbidden = new[]
        {
            "Papyrus.Infrastructure.Content.EfContentCommandStore",
            "Papyrus.Infrastructure.Content.EfContentLifecycleStore",
            "Papyrus.Infrastructure.Content.EfContentQueryStore",
            "Papyrus.Infrastructure.Content.EfPublicContentStore",
            "Papyrus.Infrastructure.Content.EfTaxonomyCommandStore"
        };

        foreach (var assemblyName in WebAssemblies[1..])
        {
            var assembly = Assembly.Load(assemblyName);
            var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();

            Assert.True(result.IsSuccessful, result.FailingTypeNames is null
                ? $"{assemblyName} contains a forbidden write-adapter dependency."
                : string.Join(Environment.NewLine, result.FailingTypeNames));
        }
    }

    [Fact]
    public void Admin_presentation_code_outside_the_composition_root_does_not_reach_infrastructure()
    {
        var assembly = Assembly.Load("Papyrus.Api.Admin");
        var offenders = InfrastructureReferences(assembly)
            .Where(entry => !IsAdminCompositionRoot(entry.Type))
            .Where(entry => entry.Dependency.FullName != "Papyrus.Infrastructure.MultiTenancy.ITenantContext")
            .Select(entry => $"{entry.Type.FullName} -> {entry.Dependency.FullName}")
            .ToArray();

        Assert.True(offenders.Length == 0,
            $"Papyrus.Api.Admin reaches Infrastructure outside its composition root: {string.Join(", ", offenders)}");
    }

    // Top-level statements compile to a global-namespace Program plus nested
    // compiler-generated closure types that inherit its references.
    private static bool IsEntryPoint(Type type) =>
        type.FullName == "Program"
        || (type.FullName?.StartsWith("Program+", StringComparison.Ordinal) ?? false);

    private static bool IsAdminCompositionRoot(Type type) =>
        IsEntryPoint(type)
        || type.Namespace == "Papyrus.Api.Admin.Endpoints"
        || type.Namespace == "Papyrus.Api.Admin.AdminUi";

    private static IEnumerable<(Type Type, Type Dependency)> InfrastructureReferences(Assembly assembly) =>
        assembly.GetTypes()
            .SelectMany(type => ReferencedTypes(type)
                .Where(dependency => dependency.FullName?.StartsWith("Papyrus.Infrastructure", StringComparison.Ordinal) == true)
                .Select(dependency => (Type: type, Dependency: dependency)))
            .Distinct();

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        if (type.BaseType is not null)
        {
            yield return type.BaseType;
        }

        foreach (var contract in type.GetInterfaces())
        {
            yield return contract;
        }

        const BindingFlags members = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(members))
        {
            yield return field.FieldType;
        }

        foreach (var property in type.GetProperties(members))
        {
            yield return property.PropertyType;
        }

        foreach (var method in type.GetMethods(members))
        {
            yield return method.ReturnType;

            foreach (var parameter in method.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }
    }
}
