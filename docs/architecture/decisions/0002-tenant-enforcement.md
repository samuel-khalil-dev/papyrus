# ADR 0002: Tenant Enforcement Before Multi-Tenant Release

## Decision

Treat the current tenant column as incomplete. Before any multi-tenant release, resolve tenant context from a trusted boundary, enforce it in application and persistence paths, use tenant-scoped constraints/indexes, and evaluate PostgreSQL row-level security as defense in depth.

## Rejected shortcut

Hard-coding `main`, trusting a request tenant header, or relying only on an EF query filter is not an isolation strategy.

## Consequences

Existing data needs an inventory and controlled backfill. Every read, write, relationship, media, feed, seed, and test path must carry the tenant context.
