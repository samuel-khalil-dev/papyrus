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

**Loopback is always allowed, and that is load-bearing.** `ci.yml` (two jobs),
`perf-baseline.yml`, and `release.yml` all start the hosts with
`ASPNETCORE_URLS=http://127.0.0.1:<port>` and poll `127.0.0.1`, as do the
Aspire AppHost health checks and the local `dotnet run` path — all while the
configured public base URLs are real domains. Without the exemption every one of
those probes would be refused with a 400.

When no public base URL is configured at all, the allowlist degrades to `"*"`
and the pre-milestone permissive behaviour is preserved, because
`SiteUrl.ResolvePublicBase` already has a safe relative-URL fallback for that
case.

### Deployment-visible behaviour change

A deployment whose `Site:PublicBaseUrl` disagrees with the Host header its
traffic actually arrives with will now receive **400 on every request** rather
than serving the site under the wrong host. That is the intended fail-loud
outcome. Align `Site:PublicBaseUrl` with the real public host before deploying.
An unset `Site:Kind` mismatch also now fails startup.

### Verification

Runtime behaviour was confirmed against a locally started host rather than
inferred from the unit tests: a request with `Host: evil.example.com` returned
**400**, while `Host: 127.0.0.1` and `Host: localhost` reached the application
and returned the database health result rather than a host rejection.

### Scope note on AppHost and Compose

`docker-compose.yml` already supplies `Site__PublicBaseUrl` per host, and the
Aspire AppHost sets no `Site__*` values, so each host reads its own
`appsettings.json`. Neither needed a change; adding redundant configuration
would have created a second source of truth for the host mapping.

