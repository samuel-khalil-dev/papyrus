# API Standards

## Contract-first development
The Admin API is OpenAPI contract-first; CI fails on drift between implementation and the published contract. The Admin UI's TypeScript client is generated from it.

## Versioning
Public contract under `/api/v1`. A breaking change requires a new version segment.

## Endpoint organization
Feature-grouped Minimal API endpoint classes via `MapGroup`, split further past ~300 lines.

## Error handling
RFC 7807 `ProblemDetails` uniformly across every endpoint, including the AI-assist and AI-provider-configuration endpoint groups introduced by M25-M27.

## Authentication and authorization

**Current:** the Admin API is authenticated via a static API key (`X-Admin-Api-Key`) and is tenant-aware. **Planned (M3):** two-factor authentication will be added in addition to the key.

## AI-assist endpoints specifically
Streamed (Server-Sent Events) responses for generative actions; every AI-assist request is metered and recorded as an `AiUsageRecord` (M27) before or as part of streaming the response, so cost tracking cannot be bypassed by a request that never completes normally.

## Public read API
Blog and Portfolio render server-side via direct in-process Application calls. A separate, read-only public API surface is documented publicly (M22) as a design showcase.
