# Architecture Decision Records

This directory records the architecture decisions (ADRs) for Papyrus. Each ADR
captures a decision, its context, and its consequences. The ADRs are the
authoritative record of the architectural choices that shape the modular
monolith.

## Index

| ADR | Title | Status |
| --- | --- | --- |
| [0001](0001-modular-monolith-boundaries.md) | Preserve Modular Monolith Boundaries | Accepted |
| [0002](0002-tenant-enforcement.md) | Tenant Enforcement Before Multi-Tenant Release | Accepted |
| [0003](0003-admin-session-and-media-policy.md) | Harden Admin Sessions and Media | Superseded for admin auth by ADR 0006; media portions active |
| [0004](0004-rest-and-contracts.md) | Versioned REST and Explicit Contracts | Accepted |
| [0005](0005-migration-ownership.md) | One Controlled Migration Owner | Accepted |
| [0006](0006-retire-admin-razor-pages.md) | Retire the Admin Razor Pages | Accepted |
| [0007](0007-postgres-rls-decision.md) | Postgres Row-Level Security Decision | Accepted (decline RLS) |

## How to add an ADR

1. Create a new file `NNNN-short-title.md` in this directory.
2. Follow the existing structure: **Status**, **Context**, **Decision**,
   **Consequences**, and (where relevant) **Alternatives considered**.
3. Add the entry to the index table above.
4. Link the ADR from the change record and the release gates
   ([`release-gates.md`](../release-gates.md)).

## Related

- [Architecture overview](../overview.md)
- [C4 System Context](../c4/context.md)
- [C4 Containers](../c4/container.md)
- [C4 Components](../c4/component.md)