using Microsoft.Extensions.DependencyInjection;
using Papyrus.Application.Content;

namespace Papyrus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPapyrusApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ContentCommandHandlers>();
        services.AddScoped<ContentQueryHandlers>();
        services.AddScoped<TaxonomyCommandHandlers>();
        services.AddScoped<ContentLifecycleHandlers>();
        services.AddScoped<PublicContentQueryHandlers>();
        return services;
    }
}
