using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Papyrus.Tests;

/// <summary>
/// An authentication scheme that authenticates every request as a fixed test
/// identity.
/// <para>
/// The real <c>ApiKeyAuthenticationHandler</c> is exercised at the HTTP layer
/// by <c>CompositionRootTests</c>. The InMemory-backed endpoint tests here only
/// need the pipeline to be authorized, so sharing one handler keeps a second,
/// divergent copy from drifting.
/// </para>
/// </summary>
internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "Test");
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
