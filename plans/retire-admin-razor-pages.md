# Plan: Retire the Admin Razor Pages

## Goal

Remove the legacy admin Razor pages and the cookie-gate session model from `Papyrus.Api.Admin`, consolidating admin authentication on the SPA's `X-Admin-Api-Key` model. The React SPA in `src/Papyrus.Admin.UI` is the sole admin UI.

## Background

The SPA is the primary `/admin` surface and authenticates every `/api/v1` call with `X-Admin-Api-Key` + `X-Tenant-Id` headers ([`api.ts`](src/Papyrus.Admin.UI/src/lib/api.ts:9)). The Razor pages and cookie-gate session model are dead code for the current admin experience. See [ADR 0006](docs/architecture/decisions/0006-retire-admin-razor-pages.md).

## Scope

**Delete (Razor pages):**

- `src/Papyrus.Api.Admin/Pages/` — entire directory (Index, Admin/Index, Admin/Login, Admin/Logout, Admin/Posts/*, Admin/Projects/*, Admin/Media/*, Shared/_Layout, _ViewImports, _ViewStart)
- `src/Papyrus.Api.Admin/wwwroot/css/admin.css` — Razor-only stylesheet

**Delete (cookie-gate session model):**

- `src/Papyrus.Api.Admin/Authentication/AdminCookieGateExtensions.cs`
- `src/Papyrus.Api.Admin/Authentication/IAdminSessionStore.cs`
- `src/Papyrus.Api.Admin/Authentication/InMemoryAdminSessionStore.cs`
- `src/Papyrus.Api.Admin/Authentication/IAdminLoginThrottle.cs`
- `src/Papyrus.Api.Admin/Authentication/InMemoryAdminLoginThrottle.cs`
- `src/Papyrus.Api.Admin/Authentication/AdminSessionOptions.cs`
- `src/Papyrus.Api.Admin/Authentication/AdminSecurityOptions.cs`
- `src/Papyrus.Tests/AdminSessionTests.cs`

**Modify:**

- `src/Papyrus.Api.Admin/Program.cs` — remove `AddRazorPages`, `AddAntiforgery`, `UseAdminCookieGate`, `MapRazorPages`; replace `UseExceptionHandler("/Error")` with a minimal API handler; remove `AdminSession` config binding.
- `src/Papyrus.Api.Admin/Authentication/AdminApiKey.cs` — remove cookie helpers (`AppendCookie`, `DeleteCookie`, `CreateCookieOptions`, `CookieName`); keep `HeaderName`, `ConfigurationKey`, `Matches`.
- `src/Papyrus.Api.Admin/Authentication/AdminAuthenticationExtensions.cs` — remove `IAdminSessionStore`/`IAdminLoginThrottle`/`AdminSessionOptions`/`AdminSecurityOptions` registrations.
- `src/Papyrus.Api.Admin/AdminUi/AdminContentSupport.cs` — retain shared slug/tag/media helpers because tests and API support code use them; no shared content behavior is deleted.
- `src/Papyrus.Api.Admin/appsettings.json` — remove the `AdminApi:Session` section.

**Docs:**

- `docs/architecture/overview.md` — update admin description to SPA + API-key.
- `docs/security/threat-model.md` — update T-02/T-03 to reflect API-key-only auth; remove cookie-session controls.
- `docs/security/audit-register.md` — update W-NEW-04/W-NEW-05 references to the Razor media page and session store.
- `docs/project/roadmap.md` — remove Razor/client synchronization references.
- `docs/architecture/decisions/0003-admin-session-and-media-policy.md` — add a note that the session portion is superseded by ADR 0006.
- `plans/admin-ui-serving-plan.md` — archive (move to `plans/archive/` or delete).

## Execution Order

1. **Program.cs** — remove Razor/antiforgery/cookie-gate wiring; add minimal exception handler.
2. **Authentication** — delete cookie-gate + session/throttle files; trim `AdminApiKey`; update `AdminAuthenticationExtensions`.
3. **AdminContentSupport** — verify shared helpers remain available to API support code and tests.
4. **Delete** `Pages/` and `wwwroot/css/admin.css`.
5. **appsettings.json** — remove `AdminApi.Session`.
6. **Tests** — delete `AdminSessionTests.cs`; verify `AdminApiTests.cs` still covers API-key auth.
7. **Docs** — update all stale references.
8. **Archive** the superseded serving plan.
9. **Verify** — `dotnet build Papyrus.sln` (zero warnings/errors); run tests; confirm `/admin` serves the SPA and `/api/v1` rejects unauthenticated calls.

## Verification

- `dotnet build Papyrus.sln` → zero warnings/errors.
- `dotnet test` → all remaining tests pass.
- Navigate to `/admin` → React SPA renders.
- Unauthenticated `/api/v1` call → rejected (401/403).
- Authenticated `/api/v1` call with `X-Admin-Api-Key` → succeeds.
- No route remains for `/admin/login`, `/admin/logout`, `/admin/legacy`, `/admin/posts`, `/admin/projects`, `/admin/media`.

## Risks

- **Exception handler:** `UseExceptionHandler("/Error")` currently points at a Razor route; must be replaced or unhandled exceptions will surface raw. Use a minimal API handler returning a problem-details JSON response.
- **Shared helper:** `AdminContentSupport.IsMediaReferencedAsync` is used by the API endpoint — do not delete the whole class.
- **Docs drift:** several docs reference the Razor UI; update them in the same change to avoid stale documentation.
