# Papyrus documentation

This directory is the documentation home for Papyrus, a .NET 10 modular
monolith for publishing technical writing, showcasing portfolio work, and
managing content through an authenticated admin surface.

## Start here

| Topic | Description |
| --- | --- |
| [Architecture](architecture/README.md) | System boundaries, data flow, and release gates |
| [Architecture decisions](architecture/decisions/) | Accepted architectural decision records (ADRs) |
| [API reference](api/README.md) | Versioned admin API contract and generated OpenAPI document |
| [Security](security/README.md) | Threat model, audit status, risk disposition, and policy |
| [Performance](performance/README.md) | PostgreSQL projection and query-plan evidence |
| [Guides](guides/README.md) | Local contribution and production deployment guidance |
| [Project status](project/README.md) | Roadmap and changelog |

## Current status

- Phases 0–6 are complete; Phase 7 is release readiness and showcase work.
- The current local baseline is 70 passing non-integration .NET tests and a
  zero-warning, zero-error warnings-as-errors build. PostgreSQL-backed
  integration tests require the test database infrastructure and run in CI.
- Five Playwright browser tests are tracked; they are not part of the local
  non-integration baseline.
- Five EF Core migrations are applied. The admin migration job is the sole
  migration owner.

## Documentation conventions

- ADRs record durable architectural decisions; they are not implementation plans.
- Security documents distinguish controls that are closed in code from release
  gates that still require operational evidence.
- Generated artifacts, such as [OpenAPI](api/openapi.json) and captured SQL,
  are retained when they provide reviewable evidence for the current state.
- Update links when moving a document and keep claims aligned with the verified
  implementation state.
