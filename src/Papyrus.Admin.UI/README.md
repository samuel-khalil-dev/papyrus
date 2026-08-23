# Papyrus Admin UI

Vite + React 19 + TypeScript strict-mode scaffold for the Papyrus admin SPA.

## Local development

Install dependencies with `npm install`, then run `npm run dev`. The API client
uses `/api/v1` and injects `X-Admin-Api-Key` and `X-Tenant-Id` from the browser
store (`dev-admin-key` / `main` defaults).

To serve the production bundle from `Papyrus.Api.Admin`, run `npm run build`.
The backend discovers the local `dist/` directory automatically; the admin
Dockerfile builds and publishes the same bundle under `/admin`.

The `generate-api` script targets the local OpenAPI document. It intentionally
has not been run during scaffolding because the backend may not be running; the
checked-in `src/api/schema.d.ts` is a valid placeholder until generation.
