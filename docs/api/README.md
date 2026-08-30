# API reference

The checked-in [OpenAPI 3.1.1 document](openapi.json) is the canonical
reference for the 23 documented operations in the versioned admin REST API
under `/api/v1`. It is kept aligned with the runtime document served at
`/openapi/v1.json` and is used by CI for OpenAPI drift checks.

## Authentication and tenant scope

- Every `/api/v1` operation requires the `AdminApiKey` security scheme.
- `AdminApiKey` is an API-key scheme in the `X-Admin-Api-Key` header.
- Every documented path accepts the optional `X-Tenant-Id` header. It selects
  the tenant scope and defaults to `main`; valid values match
  `^[a-z0-9][a-z0-9-]{0,62}$`.
- The API applies the same per-IP `admin-api` rate limit as the runtime
  endpoints (120 requests per minute).

## DTO schemas

The specification covers the request and response DTOs used by the versioned
routes, including `PageResponse<T>` expanded for categories, tags, projects,
and blog posts. It also includes the nested category/tag/project/post summary
schemas, validation problem details, and media upload/list result schemas.
The documented request models are `CategoryUpsertRequest`,
`TagUpsertRequest`, `ProjectUpsertRequest`, and `BlogPostUpsertRequest`.

## Operations

The contract currently documents these 23 operations:

| Resource | Read | Write |
| --- | --- | --- |
| Categories | `GET /api/v1/categories`, `GET /api/v1/categories/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Tags | `GET /api/v1/tags`, `GET /api/v1/tags/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Projects | `GET /api/v1/projects`, `GET /api/v1/projects/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Blog posts | `GET /api/v1/blog-posts`, `GET /api/v1/blog-posts/{id}` | `POST`, `PUT`, and `DELETE` on the same resource paths |
| Media | `GET /api/v1/media` | `POST /api/v1/media`, `DELETE /api/v1/media/{fileName}` |

For parameters, validation rules, response shapes, and status codes, use the
[canonical OpenAPI JSON](openapi.json). The running Admin API exposes the same
contract at `/openapi/v1.json`.

## Compatibility

The legacy `/api` routes remain compatibility behavior. They are intentionally
not represented as a second published contract; their bounded pagination and
compatibility behavior are covered by tests and the architecture/security
documentation.

## Contract rules

- Delivery boundaries expose DTOs, never EF entities.
- New API changes are additive or versioned and require contract verification.
- The published contract is strict OpenAPI 3.1 JSON and must remain valid JSON.
- Admin requests use the `AdminApiKey` scheme and `X-Admin-Api-Key` header.
- Tenant scope is resolved by the application and persistence boundaries using
  `X-Tenant-Id`.
