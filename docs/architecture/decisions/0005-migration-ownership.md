# ADR 0005: One Controlled Migration Owner

## Decision

Only a controlled admin migration job or admin deployment identity applies EF migrations. Public hosts never migrate. Schema changes use generated, reviewed migrations with backup, expand/contract sequencing, lock review, and restore rehearsal.

## Consequences

Deployment orchestration must gate readiness on migration completion. A database rollback is a recovery procedure, not a routine down-migration.
