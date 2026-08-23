# Design: Wire the React Admin UI into Aspire (Dev-Only)

**Status:** Implemented
**Date:** 2026-08-26
**Related:** [`plans/archive/admin-ui-serving-plan.md`](plans/archive/admin-ui-serving-plan.md) (superseded primary path)

## 1. Context & Problem

The React admin SPA (`Papyrus.Admin.UI`) is currently **not** a resource in the Aspire orchestration graph. It is served **same-origin from `Papyrus.Api.Admin` at `/admin`** — the built Vite output is placed in `wwwroot/admin` (published) or the sibling UI's local `dist` (dev), and `Dockerfile.admin` builds it in a Node stage.

This is a deliberate, documented decision (Option A in the archived plan). It keeps a single origin and solves production serving. However, it degrades the **local frontend dev loop**: every UI change requires a rebuild (or a separate watch process), and there is no Vite HMR as a first-class experience.

This design adds the SPA to Aspire **as a dev-only convenience** (the archived plan's "Option B, layered on later purely as a dev convenience"). It does **not** change the production serving model.

## 2. Scope

**In scope:**

- Register `Papyrus.Admin.UI` as an `AddNpmApp` resource in the AppHost.
- Add a Vite dev-server proxy so the SPA can call the admin API during local dev.
- Keep production serving unchanged (backend still serves the built SPA at `/admin`).

**Out of scope:**

- Changing production serving / Dockerfile.admin.
- CORS changes (dev proxy avoids cross-origin).
- Any change to the single-origin model.

## 3. Proposed Changes

### 3.1 `src/Papyrus.AppHost/Papyrus.AppHost.csproj`

Add the Node.js hosting integration package:

```xml
<PackageReference Include="Aspire.Hosting.NodeJs" Version="9.5.2" />
```

### 3.2 `src/Papyrus.AppHost/Program.cs`

Register the SPA as a dev resource, referencing the admin API so the proxy target is discovered:

```csharp
var adminUi = builder.AddNpmApp("admin-ui", "../Papyrus.Admin.UI", "dev")
    .WithReference(adminApi)
    .WithHttpEndpoint(port: 5173, targetPort: 5173, name: "http")
    .WithEnvironment("VITE_API_BASE_URL", adminApi.GetEndpoint("http"));
```

Notes:

- `AddNpmApp` runs `npm run dev` (Vite) in the UI directory.
- `WithReference(adminApi)` exposes the admin API endpoint to the SPA via the standard `services__admin-api__http__0` env var, which the Vite proxy can consume.
- The endpoint is pinned to 5173 for a stable dev URL; Aspire still surfaces it in the dashboard.

### 3.3 `src/Papyrus.Admin.UI/vite.config.ts`

Add a dev-server proxy forwarding API calls to the admin API. The target should come from the Aspire-injected env var when present, falling back to a local default:

```ts
const apiTarget = process.env.services__admin-api__http__0
  ?? process.env.VITE_API_BASE_URL
  ?? 'http://localhost:5000';

export default defineConfig({
  base: '/admin/',
  plugins: [react(), tailwindcss()],
  resolve: { alias: { '@': './src' } },
  server: {
    port: 5173,
    proxy: {
      '/api/v1': { target: apiTarget, changeOrigin: true },
    },
  },
});
```

## 4. Dev Flow After Change

1. `dotnet run --project src/Papyrus.AppHost` starts Postgres, admin-api, portfolio, blog, and the Vite dev server.
2. The SPA is available at `http://localhost:5173/admin/` with full HMR.
3. API calls to `/api/v1/*` are proxied to `admin-api` by Vite.
4. Production is unaffected: the built SPA is still served from `admin-api` at `/admin`.

## 5. Trade-offs

| Aspect | Current (single-origin) | With dev-only AddNpmApp |
| -------- | ------------------------ | -------------------------- |
| Production serving | SPA from admin-api `/admin` | Unchanged |
| Dev HMR | Requires rebuild/watch | First-class Vite HMR |
| Origins in dev | Single | Two (proxied by Vite) |
| Orchestration complexity | Low | Slightly higher |
| Port management | Hardcoded | Aspire-managed (pinned 5173) |

## 6. Risks & Mitigations

- **Two origins in dev** — mitigated by the Vite proxy; no CORS needed.
- **Env var coupling** — the proxy target reads the Aspire-injected var with a local fallback, so the UI still works standalone.
- **Package availability** — `Aspire.Hosting.NodeJs` has no 13.x package on NuGet; the latest published version, 9.5.2, restores successfully with the existing Aspire AppHost SDK 13.5.2 and is pinned explicitly.

## 7. Open Questions

1. Should the dev port stay pinned at `5173`, or let Aspire allocate it dynamically?
2. Should the proxy forward the full `/api/v1` prefix or a broader path?
3. Is a separate `dev` script needed, or reuse the existing `dev: vite` script?

## 8. Acceptance Criteria

- [x] `dotnet run` in AppHost starts the Vite dev server as a dashboard resource.
- [x] SPA loads at `/admin` with HMR working.
- [x] API calls proxy to `admin-api` without CORS errors.
- [x] Production build/serving path is unchanged.
