using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Papyrus.Api.Admin.Authentication;

internal static class AdminAuthenticationExtensions
{
    public static IServiceCollection AddAdminApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorization(options =>
        {
            // DefaultPolicy applies only where RequireAuthorization is used (the /api group).
            // Health endpoints stay anonymous so Aspire WaitFor can observe readiness.
            options.DefaultPolicy = new AuthorizationPolicyBuilder(ApiKeyAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
