# Papyrus Documentation

Authoritative reference for the Papyrus platform: a personal brand system composed of a Portfolio (landing/root domain), a Blog (subdomain), and an Admin control plane. Follows the Diátaxis framework.

## Organization

| Section | Purpose |
|---|---|
| `00-overview/` | Project charter and glossary |
| `01-product/` | Vision, information architecture, brand identity |
| `02-architecture/` | Architecture, domain model, API standards, ADRs |
| `03-design-system/` | Visual, motion, and layout-direction specification |
| `04-engineering-standards/` | Coding, testing, CI/CD, security, performance standards |
| `05-milestones/` | One document per roadmap milestone (M0-M32) |
| `06-operations/` | Deployment, environments, runbooks, observability, vendor dependency map |
| `07-governance/` | Definition of done, contribution workflow, documentation maintenance, continuity |

## Source of truth
`Papyrus_Roadmap.md` defines *what* is being built and *when*. This set defines *how* and *why*.

## Start here

| Topic | Location |
| --- | --- |
| Architecture overview | [`02-architecture/architecture-overview.md`](02-architecture/architecture-overview.md) |
| Architecture decisions (ADRs) | [`02-architecture/decisions/`](02-architecture/decisions/) |
| API reference | [`02-architecture/api-standards.md`](02-architecture/api-standards.md) and [`api/openapi.json`](api/openapi.json) |
| Security baseline and threat register | [`04-engineering-standards/security-baseline.md`](04-engineering-standards/security-baseline.md) |
| Getting started | [`01-product/getting-started-tutorial.md`](01-product/getting-started-tutorial.md) |
| Operations and runbooks | [`06-operations/runbooks.md`](06-operations/runbooks.md) |
| Roadmap | [`Papyrus_Roadmap.md`](Papyrus_Roadmap.md) |

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
- Generated artifacts, such as [`api/openapi.json`](api/openapi.json) and captured SQL,
  are retained when they provide reviewable evidence for the current state.
- Update links when moving a document and keep claims aligned with the verified
  implementation state.

## Note on this revision
This is the finalized, complete edition of the documentation, incorporating security, infrastructure, AI-platform, localization-direction, and compliance scope agreed after the initial architecture pass. It supersedes every prior roadmap or documentation artifact produced earlier in this project's history.