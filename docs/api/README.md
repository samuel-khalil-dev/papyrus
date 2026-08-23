# API reference

The generated [OpenAPI document](openapi.json) is the canonical checked-in
reference for the versioned admin REST API under `/api/v1`. It describes the
explicit DTO contracts, pagination envelopes, validation responses, and
authentication requirements.

## Compatibility

The legacy `/api` routes remain compatibility behavior. They are intentionally
not represented as a second published contract; their bounded pagination and
compatibility behavior are covered by tests and the architecture/security
documentation.

## Contract rules

- Delivery boundaries expose DTOs, never EF entities.
- New API changes are additive or versioned and require contract verification.
- Admin requests use `X-Admin-Api-Key`.
- Tenant scope is resolved by the application and persistence boundaries.
