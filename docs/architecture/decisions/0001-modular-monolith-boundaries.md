# ADR 0001: Preserve Modular Monolith Boundaries

## Decision

Keep Papyrus as a modular monolith with separate admin, blog, and portfolio hosts sharing core, infrastructure, and PostgreSQL. Add an application layer before considering service extraction.

## Rules

- Core owns domain policy and has no web or persistence dependency.
- Application owns use cases, authorization decisions, DTO mapping, and ports.
- Infrastructure owns EF Core, PostgreSQL, migrations, outbox, and adapters.
- Admin owns writes and authentication; public hosts are read-only.
- Shared web code is presentation-only.

## Consequences

This preserves local simplicity and transaction boundaries. It requires architecture tests and disciplined dependency review. Extraction is deferred until measured load, deployment, or team ownership justifies distributed complexity.
