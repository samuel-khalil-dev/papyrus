# ADR 0006: Retire the Admin Razor Pages

## Status

Accepted.

## Context

The admin UI is served by the React SPA in [`src/Papyrus.Admin.UI/`](../../../src/Papyrus.Admin.UI/package.json:1), integrated as the primary `/admin` experience. The SPA authenticates every `/api/v1` call with the `X-Admin-Api-Key` header and `X-Tenant-Id` header ([`api.ts`](../../../src/Papyrus.Admin.UI/src/lib/api.ts:9)); it never uses a cookie session.

The Razor admin pages in [`src/Papyrus.Api.Admin/Pages/`](../../../src/Papyrus.Api.Admin/Pages/Index.cshtml:1) are legacy code retained "for compatibility" when the SPA was integrated:

- The legacy dashboard was moved off `/admin` to `/admin/legacy`.
- The child pages (`/admin/posts`, `/admin/projects`, `/admin/media`, and the Edit pages) are not linked from the SPA and are only reachable by direct URL.
- The login/logout pages and the cookie-gate session model (`Papyrus.AdminKey` cookie, `IAdminSessionStore`, `IAdminLoginThrottle`, `AdminCookieGateMiddleware`) exist solely to support the cookie-based Razor flow, which the SPA does not use.

The SPA shell is already exempted from the cookie gate, and all data operations are protected by API-key authorization on `/api/v1`. The cookie-gate session model is therefore dead code for the current admin surface.

## Decision

Retire the admin Razor pages and the cookie-gate session model completely, consolidating admin authentication on the SPA's `X-Admin-Api-Key` model.

Specifically:

1. Remove the Razor Pages pipeline: `AddRazorPages` and `app.MapRazorPages()`.
2. Delete the `Pages/` directory (all admin Razor pages, the shared layout, `_ViewImports`, `_ViewStart`).
3. Remove the cookie-gate middleware and its session/throttle services: `UseAdminCookieGate`, `AdminCookieGateExtensions`, `IAdminSessionStore`, `InMemoryAdminSessionStore`, `IAdminLoginThrottle`, `InMemoryAdminLoginThrottle`, `AdminSessionOptions`, `AdminSecurityOptions`.
4. Remove the cookie helpers from `AdminApiKey` (cookie append/delete/options); keep the header name, configuration key, and constant-time `Matches`.
5. Remove the antiforgery registration and the global `AutoValidateAntiforgeryToken` filter, which exist only for cookie-authenticated Razor form posts.
6. Replace the Razor `UseExceptionHandler("/Error")` with a minimal API exception handler.
7. Retain the shared `AdminContentSupport` helpers because slug/tag behavior is covered by API-facing tests and shared test infrastructure; remove no shared content behavior.
8. Remove the `AdminSession` configuration section from `appsettings.json`.
9. Delete the now-obsolete `AdminSessionTests.cs` and update stale documentation.
10. Archive the superseded `plans/admin-ui-serving-plan.md`.

## Consequences

- **Positive:** Removes a large body of dead code and a second, conflicting authentication model. The admin surface has a single auth path (API key), reducing the attack surface and the maintenance burden. The `Papyrus.AdminKey` cookie and its session store/throttle are eliminated.
- **Negative:** The cookie-based login page and session flow are gone. Anyone who relied on the cookie-gate flow (e.g., a browser session that persists across the SPA) must use the SPA's API-key entry. The `AdminSessionTests.cs` suite is removed; API-key matching remains covered by `AdminApiTests.cs`.
- **Security note:** The SPA shell at `/admin` is publicly reachable (as it already is today), but every data operation on `/api/v1` remains protected by the API-key authorization policy. No data exposure is introduced.
- **Documentation:** `README.md`, `docs/architecture/overview.md`, `docs/security/threat-model.md`, `docs/security/audit-register.md`, `docs/project/roadmap.md`, ADR 0003, and the memory bank were updated to reflect the SPA-first, API-key-only model.

## Alternatives considered

- **Keep the Razor pages for compatibility.** Rejected: they are dead code for the primary admin experience, add a second auth model, and increase the attack surface.
- **Keep only login/logout + cookie gate.** Rejected: the SPA does not use the cookie, so the gate and login/logout pages serve no live purpose.
- **Rework the SPA to use cookie auth (plan sub-option A2).** Rejected: contradicts the SPA's existing `X-Admin-Api-Key` contract and adds rework with no benefit.
