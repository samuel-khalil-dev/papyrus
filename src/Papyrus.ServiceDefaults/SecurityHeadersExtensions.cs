using Microsoft.AspNetCore.Builder;

namespace Papyrus.ServiceDefaults;

public static class SecurityHeadersExtensions
{
    public static WebApplication UsePapyrusSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'; " +
                "img-src 'self' https: data:; script-src 'self'; style-src 'self' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });

        return app;
    }
}
