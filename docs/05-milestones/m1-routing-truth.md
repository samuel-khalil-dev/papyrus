# M1 — Routing Truth

## Objective
Make Portfolio-as-root / Blog-as-subdomain an explicit, enforced application-level configuration.

## Background
SiteOptions/SiteUrl do not currently encode which site owns which host.

## Scope

### In scope
- Define explicit host configuration for Portfolio (root) and Blog (subdomain).
- Update Aspire AppHost and docker-compose bindings.
- Add a test asserting correct host resolution.

### Out of scope
- The actual internet-facing TLS/routing layer (see M2).

## Domain changes
- No new aggregates; configuration-only.

## API changes
- No breaking API changes.

## UI/Presentation changes
- No new screens.

## Non-functional requirements
- Must work identically in local, containerized, and production environments.

## Risks & mitigations
- Misconfigured host mapping could make a site unreachable.
- Mitigation: automated resolution test plus manual per-environment verification.

## Definition of Done
- [x] Root domain resolves exclusively to Portfolio in all environments.
- [x] Blog reachable only via subdomain in all environments.
- [x] An automated test enforces the rule.

## Test plan
- Integration test for site resolution per host.
- Manual verification per environment.

## Status — shipped 2026-09-28

The configuration this milestone describes already existed: both public
`appsettings.json` files carry a complete `Site` block (`Kind`,
`PublicBaseUrl`, `BlogBaseUrl`, `PortfolioBaseUrl`) and `docker-compose.yml`
maps `BLOG_PUBLIC_BASE_URL` / `PORTFOLIO_PUBLIC_BASE_URL` per host. What was
missing was enforcement — `AllowedHosts` was `"*"` in both public hosts, so any
Host header was served, and `Site:Kind` was read only by two `.cshtml` files to
pick a label. The milestone was therefore closed by making the existing
configuration authoritative rather than by adding new configuration.

### What was added

- `SiteHostValidator` in `Papyrus.Web.Shared`, a pure static type so the rules
  are testable without booting a host. It supplies the derived `AllowedHosts`
  list and the startup validation.
- `UsePapyrusSiteHostPolicy` on `WebApplicationBuilder`, called once from each
  public host's `Program.cs` with the site that host actually serves, so the
  policy is defined in one place rather than repeated per host.

### Rules enforced

1. **Identity** — `Site:PublicBaseUrl` must name the same host as the matching
   role base URL (`Site:BlogBaseUrl` for the blog, `Site:PortfolioBaseUrl` for
   the portfolio). A host cannot advertise the other site's canonical URLs.
2. **Kind** — `Site:Kind` must match the site the process serves. It is what the
   shared layout and About content branch on.
3. **Distinctness** — the two public hosts must resolve to different canonical
   hosts, so one host is never both apex and subdomain.

Violations of 1 and 2 fail startup with a message naming the offending key,
mirroring the Admin API key check and `MediaRootResolver`. Rule 3 is enforced by
`SiteHostValidatorTests.The_two_public_hosts_resolve_to_different_canonical_hosts`,
not at startup, because a process cannot see the other host's configuration;
this is a genuine limit of in-process enforcement, not a covered case.

### AllowedHosts derivation and the loopback exemption

`AddHostFiltering` is already called by `ConfigureWebHostDefaults`, so the
middleware was always present — it was configured wide open. The allowlist is
derived from the bare host of `Site:PublicBaseUrl` (ports are stripped, because
`HostFilteringOptions.AllowedHosts` ignores them) plus the loopback literals
`localhost`, `127.0.0.1`, and `[::1]`.

**Loopback is always allowed, and that is a real trade-off rather than a free
win.** `ci.yml` (two jobs), `perf-baseline.yml`, and `release.yml` all start the
hosts with `ASPNETCORE_URLS=http://127.0.0.1:<port>` and poll `127.0.0.1`, as do
the Aspire AppHost health checks and the local `dotnet run` path — all while the
configured public base URLs are real domains. Without the exemption every one of
those probes would be refused with a 400.

The exemption is currently **unconditional**, and that is a known limitation
rather than a safe default. `deployment-guide.md` records that Papyrus runs
under Docker Compose with no reverse proxy or CDN in front of it, so a
co-located process or any peer on the compose network can present
`Host: 127.0.0.1` and be served. The exposure is bounded — the public hosts are
read-only, hold no secrets, and `SiteUrl.ResolvePublicBase` refuses to derive
absolute URLs from `Request.Host`, so the classic host-header poisoning sink is
absent — but the allowlist is not a complete defence in the current topology.
Scoping the exemption to an explicit configuration flag is the identified
follow-up, tracked with the **M2 reverse proxy** milestone, which is also where
trusted-proxy handling for the rate limiter (threat T-06a) gets settled.

When no public base URL is configured at all, the allowlist degrades to `"*"`
and the pre-milestone permissive behaviour is preserved, because
`SiteUrl.ResolvePublicBase` already has a safe relative-URL fallback for that
case.

A **malformed** public base URL degrades the same way, and that is a
classification worth stating: a typo such as `https//blog.example.com` or a
bare `blog.example.com` is not an empty configuration, but `Uri.TryCreate`
rejects it, the allowlist silently becomes `"*"`, and the host starts cleanly
with no warning. `docker-compose.yml` uses `:?` on `BLOG_PUBLIC_BASE_URL`, which
checks only that the variable is non-empty, not that it is a well-formed
absolute URL. This is inconsistent with the fail-loud treatment a `Site:Kind`
mismatch receives. The validator must distinguish "unset" from "unparseable".

**Owner:** this follow-up is now carried by
[M2 — Reverse Proxy & TLS Termination](m2-reverse-proxy-and-tls-termination.md),
as an added Definition-of-Done item, rather than being left as an unassigned
candidate. It is the same configuration surface and the same fail-loud
principle, so it lands with the proxy work that also depends on this file being
configured correctly.

### Deployment-visible behaviour change

A deployment whose `Site:PublicBaseUrl` disagrees with the Host header its
traffic actually arrives with will now receive **400 on every request** rather
than serving the site under the wrong host. That is the intended fail-loud
outcome. Align `Site:PublicBaseUrl` with the real public host before deploying.
An unset `Site:Kind` mismatch also now fails startup.

### Verification

The 400 is the entire security deliverable of this milestone, so it is proven by
a real request rather than by a source-text assertion.
`PublicHostFilterTests` boots the Blog pipeline against `TestServer` and sends
requests carrying foreign Host headers — `evil.example.com`, `example.com`,
`blog.example.com.evil.test`, and `notblog.example.com` — each of which must be
refused with **400**. A 404 would not be sufficient evidence, since it would
also indicate a host that passed the filter and merely missed a route. The
canonical host, an uppercase variant, and both loopback literals are required
*not* to be rejected, which is what pins the loopback exemption in place.

Runtime behaviour was additionally confirmed against a locally started host: a
request with `Host: evil.example.com` returned **400**, while `Host: 127.0.0.1`
and `Host: localhost` reached the application and returned the database health
result rather than a host rejection, and all three CI probe URLs returned 200
over loopback.

A wiring assertion over both public hosts remains as a second layer, guarding
against a deleted or mis-kinded call. It proves the call site exists, not that
the filter behaves; the request-level tests are what prove the behaviour.

### Scope note on AppHost and Compose

`docker-compose.yml` already supplies `Site__PublicBaseUrl` per host, and the
Aspire AppHost sets no `Site__*` values, so each host reads its own
`appsettings.json`. Neither needed a change; adding redundant configuration
would have created a second source of truth for the host mapping.

