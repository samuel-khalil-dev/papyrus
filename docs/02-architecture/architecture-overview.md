# Architecture Overview

## Architectural style
Papyrus is a .NET modular monolith built on Clean Architecture with a Ports & Adapters boundary between Application and Infrastructure.

## C4 architecture diagrams
**Current:** the C4 model views of the implemented system — Level 1 System Context, Level 2 Containers, and Level 3 Components (Admin API), including Mermaid sources — live in [c4-diagrams.md](c4-diagrams.md). They are not duplicated inline here.

## Layers

```
Presentation      Api.Admin - Web.Blog - Web.Portfolio - Admin.UI
      |
Application       Use cases, application-owned ports (command/query stores per aggregate)
      |
Core              Entities, invariants, multi-tenancy contracts
      ^
Infrastructure    EF Core adapters, migrations, interceptors, DbContext
```

### Core
Entities, value objects, and invariants only. Every entity implements `IEntity`; isolated entities implement `ITenantEntity`.

### Application
Use cases and the ports they require, one per aggregate, split into command and query interfaces. Never references Infrastructure or EF Core types directly.

### Infrastructure
Implements Application's ports as EF Core adapters; owns the DbContext, entity configurations, migrations, and `SaveChanges` interceptors.

### Presentation
- `Papyrus.Api.Admin` - authenticated via a static API key (`X-Admin-Api-Key`); two-factor protected once M3 ships; write API and media handling.
- `Papyrus.Admin.UI` - React SPA.
- `Papyrus.Web.Blog`, `Papyrus.Web.Portfolio` - server-rendered Razor Pages calling Application in-process.

A Presentation project may reference Infrastructure only inside its composition
root, where a composition root is an entry point plus the code that exists to
wire and dispatch requests. The scope is defined and enforced by
[ADR-0010](decisions/adr-0010-scoped-presentation-to-infrastructure-access.md):
the read-only public hosts reach Infrastructure only from their entry point and
never reference the `Ef*Store` adapters, and Admin's endpoint and Admin support
layers are its composition root. New Presentation code outside those boundaries
cannot reach `PapyrusDbContext` without a superseding ADR. The Admin endpoint
handlers still read and write through `PapyrusDbContext` directly rather than the
Application ports, so [ADR-0002](decisions/adr-0002-application-owned-ports-with-infrastructure-adapters.md)
is not yet true end to end; extraction behind the ports is recorded as follow-on
work in ADR-0010.

## Edge and network layer

**Planned (M2, M13):** a reverse proxy (Caddy) will terminate TLS and route each configured host (Portfolio, Blog, Admin) to its container. Cloudflare will sit in front of the reverse proxy for CDN caching, DDoS mitigation, and bot protection (Turnstile), reducing load on the underlying VPS. Today services run directly via Docker Compose with no reverse proxy or CDN in front. See `decisions/adr-0009-reverse-proxy-and-edge-layer-caddy-behind-cloudflare.md`.

## Resolver pattern (applied consistently)
Any admin-editable, safety-critical configuration - theme, AI provider, feature flags - is resolved through a dedicated `I<X>Resolver` that always returns a valid, safe configuration, falling back gracefully rather than failing the request. This pattern is applied uniformly to `IThemeResolver`, `IAiProviderResolver`, and any future resolver, rather than reinvented per feature.

## Dependency rule
Enforced by architecture fitness tests (NetArchTest) in CI, not convention alone.
