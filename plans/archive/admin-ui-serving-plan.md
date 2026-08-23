# Plan: Serve the New React Admin UI

## Root Cause

The React admin SPA in [`src/Papyrus.Admin.UI/`](src/Papyrus.Admin.UI/README.md:1) is an **orphaned frontend project**. It is never built, hosted, or routed to by any part of the .NET solution:

- [`Papyrus.Api.Admin/Program.cs`](src/Papyrus.Api.Admin/Program.cs:109) only calls `app.MapRazorPages()`, so `/admin` is always resolved by the Razor Pages UI.
- The SPA is not referenced in `Papyrus.sln`, `Papyrus.Api.Admin.csproj`, or `Papyrus.AppHost/Program.cs`.
- No static-file provider or SPA fallback points at the React `dist/` output.
- The SPA's `dist/` is git-ignored and never produced by `dotnet build`.

The old UI appears because it is the **only** UI actually served.

## Recommended Approach: Option A — Serve the built SPA from the backend

Single admin origin: the React SPA is built to `dist/` and served by `Papyrus.Api.Admin` under `/admin`, keeping the existing `/api/v1` endpoints as the same-origin data layer.

### Why this fits the existing design

- The SPA calls `baseUrl: '/api/v1'` (same-origin) — works perfectly when served from the admin host.
- The SPA uses **hash routing** (`#dashboard`, `#posts`, `#projects`, `#media`), so no server-side route handling is needed beyond serving `index.html` at `/admin`.
- The backend already exposes the full `/api/v1` surface the SPA consumes.

---

## Implementation Steps

### Step 1 — Build the SPA and make `dist/` available to the admin host

**Local dev:**

- Run `npm ci && npm run build` in `src/Papyrus.Admin.UI` to produce `dist/`.

**Docker (`Dockerfile.admin`):**

- Add a Node build stage before the .NET build stage:
  - `FROM node:22 AS ui-build`
  - `WORKDIR /ui`
  - `COPY src/Papyrus.Admin.UI/package*.json ./`
  - `RUN npm ci`
  - `COPY src/Papyrus.Admin.UI/ ./`
  - `RUN npm run build`
- In the .NET build stage, `COPY --from=ui-build /ui/dist ./Papyrus.Admin.UI/wwwroot/admin` (or a dedicated path).
- In the runtime stage, ensure the SPA `dist` is copied into the published output.

### Step 2 — Serve the SPA from `Papyrus.Api.Admin/Program.cs`

Add, after `app.MapRazorPages()` (line 109) and `app.MapAdminApi()` (line 108):

```csharp
// Serve the React admin SPA from its build output.
var spaRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin");
if (Directory.Exists(spaRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(spaRoot),
        RequestPath = "/admin"
    });
    app.MapFallbackToFile("/admin/{**path:nonfile}", "admin/index.html");
}
```

> Note: Because the SPA uses hash routing, only `/admin` strictly needs to return `index.html`. The fallback above is defensive for any non-file sub-path.

### Step 3 — Reconcile the authentication model (DECISION REQUIRED)

There is a **conflict** between the two UIs' auth models:

| | Razor UI (current) | React SPA (new) |
| --- | --- | --- |
| Auth mechanism | Cookie session (`Papyrus.AdminSession`) | API key header (`X-Admin-Api-Key`) |
| Gate | [`AdminCookieGateExtensions.cs`](src/Papyrus.Api.Admin/Authentication/AdminCookieGateExtensions.cs:33) redirects unauthenticated `/admin` → `/admin/login` | SPA stores key in browser, sends on each `/api/v1` call |
| Login page | Razor `Pages/Admin/Login.cshtml` | None (key entered in SPA) |

If the SPA is served at `/admin`, the cookie gate will redirect unauthenticated users to the **Razor** login page — which is inconsistent with the SPA's API-key flow.

**Sub-option A1 (recommended, minimal rework):** Exempt the SPA static shell from the cookie gate and rely on API-key auth for all `/api/v1` data calls.

- Modify `RequiresAdminCookie` in [`AdminCookieGateExtensions.cs`](src/Papyrus.Api.Admin/Authentication/AdminCookieGateExtensions.cs:33) to skip the gate for static SPA asset requests (e.g., requests for `index.html` and hashed asset files), while `/api/v1` remains protected by `RequireAuthorization()` + API-key auth.
- The SPA shell is publicly reachable, but **all data operations require the API key** — no data exposure.
- This matches the SPA's existing design with zero SPA rework.

**Sub-option A2 (more work, changes SPA design):** Keep the cookie gate and rework the SPA to use cookie-based session auth instead of API-key headers. Not recommended — contradicts the SPA's current architecture and the `X-Admin-Api-Key` contract.

### Step 4 — Retire the Razor admin pages (optional)

If the React SPA fully supersedes the Razor UI:

- Remove/disable `app.MapRazorPages()` in [`Program.cs`](src/Papyrus.Api.Admin/Program.cs:109) and the `AddRazorPages` registration (line 43).
- Delete or archive the Razor admin pages under `Pages/Admin/`.
- Keep the Razor login/logout only if the cookie gate remains (only relevant under sub-option A2).

> Recommendation: For a first cut, keep the Razor pages in place (non-breaking) and only add the SPA serving. Retire Razor pages in a follow-up once the SPA is confirmed working.

### Step 5 — Verify

1. `npm ci && npm run build` in `src/Papyrus.Admin.UI` → confirm `dist/` is produced.
2. `dotnet build Papyrus.sln` → zero warnings/errors.
3. Run the app (AppHost or `Papyrus.Api.Admin` directly).
4. Navigate to `/admin` → confirm the **React** UI renders (not the Razor dashboard).
5. Confirm `/api/v1` calls from the SPA succeed with the API key.
6. Confirm unauthenticated access to `/api/v1` is still rejected.

---

## Alternative: Option B — Run the SPA as a separate Aspire resource (local dev only)

Register `Papyrus.Admin.UI` in [`Papyrus.AppHost/Program.cs`](src/Papyrus.AppHost/Program.cs:1) as an `AddNpmApp` resource running `npm run dev`, add the project reference in [`Papyrus.AppHost.csproj`](src/Papyrus.AppHost/Papyrus.AppHost.csproj:1), and configure the Vite dev server to proxy `/api/v1` to the admin API. The SPA is reached at its own URL.

- Pros: Fast HMR for local development; no build step needed.
- Cons: Does not solve production serving; two origins (SPA + API) require CORS/proxy config; diverges from the single-origin model.

**Recommendation:** Use Option A for the actual fix (works in dev and prod). Option B can be layered on later purely as a dev convenience if desired.

---

## Files to Change (Option A)

| File | Change |
| --- | --- |
| [`src/Papyrus.Api.Admin/Program.cs`](src/Papyrus.Api.Admin/Program.cs:109) | Add SPA static-file provider + fallback after `MapRazorPages` |
| [`src/Papyrus.Api.Admin/Authentication/AdminCookieGateExtensions.cs`](src/Papyrus.Api.Admin/Authentication/AdminCookieGateExtensions.cs:33) | Exempt SPA static assets from the cookie gate (sub-option A1) |
| [`Dockerfile.admin`](Dockerfile.admin:1) | Add Node build stage; copy SPA `dist/` into runtime image |
| `src/Papyrus.Admin.UI/` | No changes required (already builds to `dist/`) |

## Open Decision for the User

1. **Approach**: Option A (serve from backend) vs Option B (Aspire dev resource) vs both.
2. **Auth model**: Sub-option A1 (exempt SPA shell from cookie gate, keep API-key auth) vs A2 (rework SPA to cookie auth).
3. **Razor retirement**: Keep Razor pages for now (non-breaking) vs remove them immediately.
