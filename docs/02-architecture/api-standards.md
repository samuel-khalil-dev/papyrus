# API Standards

## Contract-first development
The Admin API is OpenAPI contract-first; CI fails on drift between implementation and the published contract. The Admin UI's TypeScript client is generated from it.

## Versioning
Public contract under `/api/v1`. A breaking change requires a new version segment.

## Endpoint organisation
Feature-grouped Minimal API endpoint classes via `MapGroup`, split further past ~300 lines.

## Error handling
RFC 7807 `ProblemDetails` uniformly across every endpoint, including the AI-assist and AI-provider-configuration endpoint groups introduced by M25-M27.

## Authentication and authorization

**Current:** the Admin API is authenticated via a static API key (`X-Admin-Api-Key`) and is tenant-aware. **Planned (M3):** two-factor authentication will be added in addition to the key.

## AI-assist endpoints specifically
Streamed (Server-Sent Events) responses for generative actions; every AI-assist request is metered and recorded as an `AiUsageRecord` (M27) before or as part of streaming the response, so cost tracking cannot be bypassed by a request that never completes normally.

## Public read API
Blog and Portfolio render server-side via direct in-process Application calls. A separate, read-only public API surface is documented publicly (M22) as a design showcase.

## Current Endpoint Inventory

**Current:** the checked-in [OpenAPI 3.1.1 document](../api/openapi.json) is the canonical reference for the 23 documented operations in the versioned admin REST API under `/api/v1`. It is kept aligned with the runtime document served at `/openapi/v1.json` and is used by CI for OpenAPI drift checks.

**Current:** every `/api/v1` operation requires the `AdminApiKey` security scheme (`X-Admin-Api-Key` header), as described under [Authentication and authorization](#authentication-and-authorization) above. Every documented path accepts the optional `X-Tenant-Id` header. It selects the tenant scope and defaults to `main`; valid values match `^[a-z0-9][a-z0-9-]{0,62}$`. The API applies the same per-IP `admin-api` rate limit as the runtime endpoints (120 requests per minute).

### DTO schemas

**Current:** the specification covers the request and response DTOs used by the versioned routes, including `PageResponse<T>` expanded for categories, tags, projects, and blog posts. It also includes the nested category/tag/project/post summary schemas, validation problem details, and media upload/list result schemas. The documented request models are `CategoryUpsertRequest`, `TagUpsertRequest`, `ProjectUpsertRequest`, and `BlogPostUpsertRequest`.

### Operations

**Current:** the contract currently documents these 23 operations:

| Resource | Read | Write |
| --- | --- | --- |
| Categories | `GET /api/v1/categories`, `GET /api/v1/categories/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Tags | `GET /api/v1/tags`, `GET /api/v1/tags/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Projects | `GET /api/v1/projects`, `GET /api/v1/projects/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Blog posts | `GET /api/v1/blog-posts`, `GET /api/v1/blog-posts/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Media | `GET /api/v1/media` | `POST /api/v1/media`, `DELETE /api/v1/media/{fileName}` |

For parameters, validation rules, response shapes, and status codes, use the canonical OpenAPI JSON. The running Admin API exposes the same contract at `/openapi/v1.json`.

### Compatibility

**Current:** the legacy `/api` routes remain compatibility behaviour. They are intentionally not represented as a second published contract; their bounded pagination and compatibility behaviour are covered by tests and the architecture/security documentation.

### Contract rules

**Current:**
- Delivery boundaries expose DTOs, never EF entities.
- New API changes are additive or versioned and require contract verification.
- The published contract is strict OpenAPI 3.1 JSON and must remain valid JSON.
- Admin requests use the `AdminApiKey` scheme and `X-Admin-Api-Key` header.
- Tenant scope is resolved by the application and persistence boundaries using `X-Tenant-Id`.
