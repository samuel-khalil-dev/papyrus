using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Papyrus.Api.Admin.Authentication;

/// The per-IP request budget for the Admin API, registered here so that the
/// policy name and its limits have a single definition. The policy is applied to
/// both the versioned and the legacy unversioned route groups, and it is
/// configured rather than duplicated in tests so a change to the limit is a
/// change to the behaviour under test.
internal static class AdminRateLimitingExtensions
{
    public const string AdminApiPolicyName = "admin-api";

    public static int PermitLimit { get; } = 120;

    public static TimeSpan Window { get; } = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddAdminApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AdminApiPolicyName, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = PermitLimit,
                    Window = Window,
                    // No queueing: an over-budget request is rejected immediately
                    // rather than parked, so a client cannot hold a connection open.
                    QueueLimit = 0,
                }));
        });

        return services;
    }
}
