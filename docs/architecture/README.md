# Architecture

Papyrus is a **modular monolith** (.NET 10, EF Core 10.0.11, React 19 admin SPA,
Aspire orchestration, PostgreSQL 17 via Docker Compose). It runs three web hosts
over one shared database: the Admin host owns writes, authentication, media,
migrations, and seed; Blog and Portfolio are read-only public hosts.

The architecture is documented with the **C4 Model**, which describes a system
at four levels of abstraction. Papyrus documents the first three levels.

## C4 Model structure

| Level | View | Question it answers | File |
| --- | --- | --- | --- |
| 1 | **System Context** | What does Papyrus do, and who/what does it interact with? | [`c4/context.md`](c4/context.md) |
| 2 | **Containers** | What are the high-level technology components, and how do they communicate? | [`c4/container.md`](c4/container.md) |
| 3 | **Components** | What are the key building blocks inside the Admin API? | [`c4/component.md`](c4/component.md) |

Each view uses Mermaid diagrams and the C4 naming conventions (Person, System,
Container, Component).

## Documents

- [Public architecture overview](public-overview.md) — accessible explanation
  of the runtime, boundaries, request flow, and trade-offs.
- [Architecture overview](overview.md) — module responsibilities, runtime shape,
  and security boundaries.
- [Release gates](release-gates.md) — mandatory CI/release verification controls.
- [ADRs](decisions/README.md) — decisions about boundaries, tenants,
  sessions/media, REST, migration ownership, and Postgres RLS.

## C4 levels covered

- **Level 1 — System Context** ([`c4/context.md`](c4/context.md)): the Papyrus
  system, its users (Admin, Public Visitor), and external systems (PostgreSQL
  17, OTLP collector, browser).
- **Level 2 — Containers** ([`c4/container.md`](c4/container.md)): the Admin
  SPA (React 19), Admin API (Minimal API), Blog (Razor), Portfolio (Razor),
  PostgreSQL, and the Aspire orchestrator, with their HTTP/SQL/OTLP
  interactions.
- **Level 3 — Components** ([`c4/component.md`](c4/component.md)): the Admin
  API's internal components — `ApiKeyAuthenticationHandler`,
  `AdminAuditMiddleware`, `AdminApiEndpoints`, the Application CQRS handlers,
  `PapyrusDbContext` + `TenantContext` + `PapyrusSaveChangesInterceptor`,
  `LocalMediaService`, and `TelemetryModule`.

The modular monolith remains the default until measured scale, deployment, or
ownership constraints justify extraction (ADR 0001).
