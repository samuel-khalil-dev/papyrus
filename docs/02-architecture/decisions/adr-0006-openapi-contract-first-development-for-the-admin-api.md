# ADR-0006: OpenAPI contract-first development for the Admin API

**Status:** Accepted

## Context
Without an enforced contract, implementation and consumers can silently drift.

## Decision
The Admin API publishes an OpenAPI document verified against the implementation in CI; the Admin UI client is generated from it.

## Consequences
Positive: drift caught in CI, not production. Negative: contract changes require regenerating the client, treated as a required step.
