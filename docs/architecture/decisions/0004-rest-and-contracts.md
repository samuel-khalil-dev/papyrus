# ADR 0004: Versioned REST and Explicit Contracts

## Decision

Use versioned REST with explicit DTOs, OpenAPI, standardized error responses, and contract tests. Separate command and query handlers, but keep one PostgreSQL database initially. Defer GraphQL and gRPC until measured requirements justify them.

## Consequences

Razor pages and future clients must consume stable contracts rather than EF entities. Additive API evolution and compatibility windows become release requirements.
