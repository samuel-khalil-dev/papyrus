using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Papyrus.Web.Shared;

public static class PapyrusWebServiceCollectionExtensions
{
    public static IServiceCollection AddPapyrusWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteOptions>(configuration.GetSection(SiteOptions.SectionName));
        return services;
    }
}
