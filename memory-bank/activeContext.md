# Active Context

## Current UI bug-fix task — completed 2026-09-03

Fixed cross-frontend logo and background rendering, public footer spacing, and favicon wiring. Shared and Admin background assets now have explicit 1024px dimensions and gold fills; background CSS uses one cover-sized, non-repeating image with theme-compatible blending. The public shell is a flex column with growing main content, and both public hosts receive SVG and ICO favicon links. The Admin SPA receives a base-aware public SVG favicon and its source-asset empty states now use Vite imports so production paths resolve correctly. Removed the duplicate Admin secondary foreground token.

Verification passed: `npm run build --prefix src/Papyrus.Admin.UI` and `dotnet build .\\Papyrus.sln --nologo --no-incremental` completed successfully; `git --no-pager diff --check` passed.

## Current Admin SPA Royal Pharaonic design-system implementation — completed 2026-09-02

The standalone Admin SPA now follows `plans/unified-design-system.md` within the requested scope. Admin-only changes unify Cinzel/Inter/JetBrains Mono typography, canonical dark and light OKLCH semantic tokens, zero-radius surfaces, glass shell treatment, gold-tinted borders, semantic buttons and status badges, theme-aware texture blending, motion timing, page reveals, and reduced-motion behavior. The theme provider now writes both the `.dark` class and `data-theme` attribute so light mode resolves correctly. The Admin Papyrus logo remains `currentColor`-driven.

Verification passed: `npm run build --prefix src/Papyrus.Admin.UI` and `npm run lint --prefix src/Papyrus.Admin.UI` completed successfully. `git --no-pager diff --check` also passed. Public site source, shared SVG assets, and `docs/assets/` were not modified.

### Active focus and next steps

- Active focus: retain the Admin SPA's unified Royal Pharaonic visual system while preserving existing routing, forms, data fetching, and editor behavior.
- Next steps: manually check the light/dark toggle, font loading, and responsive shell in a browser; future Admin UI changes should keep semantic tokens and zero-radius surfaces.

## Current UI/UX theming fix — completed 2026-09-02

The public Blog and Portfolio light/dark theming and SVG rendering regression is fixed. The shared stylesheet now defines a complete light token set under `[data-theme="light"]` with `color-scheme: light`, including the requested warm background/surface/text/accent palette, semantic status colors, orb tokens, and subtle grid color. The light-mode background texture now uses multiply blending with slightly increased opacity instead of the dark-mode screen treatment. `Papyrus_Logo.svg` now uses `fill="currentColor"`, allowing the hero logo to inherit `var(--color-accent)` in either theme. SVG assets also retain their previously added `viewBox` attributes, and the obsolete `Papyrus_Background_2.svg` asset is fully removed with no dangling references.

Verification for this fix passed: `dotnet build .\Papyrus.sln --nologo --no-incremental` completed with 0 warnings and 0 errors, and `git diff --check` passed. Only the shared public CSS/SVG source changes were present for this fix; this memory-bank update records them without modifying source code.

### Active focus and next steps

- Active focus: preserve the shared public Blog/Portfolio theme tokens and theme-aware SVG asset behavior while continuing release-readiness work.
- Next steps: manually check the unified public Royal Pharaonic finishing layer in Blog and Portfolio in both themes; future public UI changes should validate SVG `viewBox`/`currentColor` behavior and shared CSS behavior.

## Current task/state

The audit-and-reconstruction effort is complete for the current scope. Phases 0–6 are complete; Phases 7–8 are not started. Papyrus is a .NET 10 modular monolith with 11 solution projects, a standalone React Admin SPA, PostgreSQL 17, Aspire, and Docker Compose. The Phase 4 test-backfill verification has a 0-warning/0-error build and 70 passing non-integration .NET tests; PostgreSQL-backed integration tests require the test database infrastructure.

## Task Outcomes

1. **Discovery/gap analysis (Step 1):** Established the exact project layout, stack, tenant model, API surface, persistence ownership, telemetry design, deployment topology, phase status, and verification baseline from the completed audit.
2. **G1:** Corrected Phase 6 roadmap status in `docs/project/roadmap.md`.
3. **G2:** Removed confirmed-empty retired Admin Razor Pages shells under `src/Papyrus.Api.Admin/Pages/`.
4. **G3:** Removed confirmed-empty obsolete Admin-local Contracts under `src/Papyrus.Api.Admin/Contracts/`.
5. **G4:** Regenerated `src/Papyrus.Admin.UI/src/api/schema.d.ts` from `docs/api/openapi.json` and changed `src/Papyrus.Admin.UI/package.json` `generate-api` to port 5205.
6. **G10:** Pinned CI Trivy in `.github/workflows/ci.yml` to the immutable v0.33.1 commit.
7. **Architecture reconstruction:** Rebuilt C4 context, container, and component documents plus overview, release gates, and ADR index under `docs/architecture/`.
8. **API reconstruction:** Rebuilt OpenAPI 3.1 `docs/api/openapi.json` and `docs/api/README.md`, including versioned operations, security, tenant header, and schemas.
9. **Diátaxis reconstruction:** Rebuilt `docs/guides/` and `docs/runbooks/` with tutorial, how-to, reference, explanation, deployment, telemetry, alert, and incident guidance.
10. **Memory-bank reconstruction:** Read all existing relative `memory-bank/` files and overwrote all seven requested files with current truth. No `.memory-bank/` directory was created.
11. **Verification:** `dotnet restore .\Papyrus.sln --nologo` passed. `dotnet build .\Papyrus.sln --nologo --no-incremental` passed with 0 warnings and 0 errors. The filtered non-integration run passed 70 tests. The integration inventory contains 14 PostgreSQL tests, producing 84 unique .NET tests in CI when the PostgreSQL service is available; integration tests remain excluded locally because PostgreSQL test infrastructure is unavailable. The separate Playwright inventory contains 5 tests.

## Current architecture facts

- Scoped `TenantContext` resolves `X-Tenant-Id`, defaults to `main`, validates `^[a-z0-9][a-z0-9-]{0,62}$`, and is enforced by EF filters plus `EnforceTenantBoundary()`.
- API-key authentication reads `X-Admin-Api-Key`, uses constant-time multi-key comparison, and fails fast in production for missing or `change-me` configuration.
- `/api/v1` is documented/authenticated; `/api` is legacy/excluded. Rate limiting is per IP at 120/min.
- `Papyrus.Api.Admin` is the sole migrator/seed owner. Five migrations are applied. RLS is not used.
- Telemetry uses `Papyrus` ActivitySource, W3C propagation, structured audit events, fail-closed redaction, cardinality protection, and diagnostic-only sampling. OTLP is gated by `OTEL_EXPORTER_OTLP_ENDPOINT`.

## Phase 7.1 documentation publication

Published public-facing Diátaxis documentation under `docs/`: architecture
overview, sanitized threat-model summary, local setup and seed/demo guide,
versioned API reference additions, deployment guidance, and a truthful
limitations page. Updated repository and section indexes, removed development
credential literals from published Markdown, and corrected relative links in
ADR 0007. No source code, dependencies, secrets, or migrations were changed.

## Phase 7.2 showcase assets

Captured sanitized local screenshots for the public Blog, public Portfolio,
React Admin UI, health probe, and .NET Aspire dashboard under `docs/assets/`.
The captures use seeded demo content; the Aspire image masks environment URLs
and operational details. Assets are referenced from `README.md`,
`docs/architecture/public-overview.md`, and `docs/guides/local-setup.md`.
Static screenshots were used instead of GIFs because the requested showcase
surfaces are adequately represented without recording an interaction flow.
Visual inspection found no API keys, connection strings, private URLs, email
addresses, or other personal data in the committed assets.

## Self-Healing Log

During this task, an initial asset-edit command was sent through the default cmd wrapper and returned `The system cannot find the file specified`; the retry also exposed cmd quote parsing and created an unintended `')` file. The stray file was removed, and the SVG edits were completed with targeted filesystem edits. No unintended file remains.

During the shared public design-system implementation, an initial CSS patch was rejected atomically because its context did not match. A later finishing-layer patch temporarily corrupted a stylesheet comment header; the malformed header was repaired and the stylesheet now has valid comment structure. No Admin UI or `docs/assets/` files were changed by this task.

Verification completed after repair: `dotnet restore .\\Papyrus.sln --nologo`, `dotnet build .\\Papyrus.sln --nologo --no-incremental` (0 warnings, 0 errors), and `git --no-pager diff --check` all passed.

No self-healing issue was encountered during the public theming fix. The build and diff checks passed on the first recorded verification run.

No unresolved code self-healing issue remains for this reconstruction. The first all-file memory-bank patch was rejected because it contained a duplicated patch header; the same content was reapplied with the valid file-oriented patch format and verified by rereading all seven files. During the broader effort, malformed patches, documentation-link parsing, OpenAPI JSON rewriting, CI YAML validation, Aspire port/key wiring, EF model drift, npm package resolution, retry-strategy transaction handling, K6 container networking, and media-volume ownership issues were diagnosed and resolved. Final verification identified only unavailable PostgreSQL test infrastructure: the non-integration suite is green, while integration execution is blocked by missing Compose environment variables and no local database listener.

During the test-count reconciliation, the first documentation patch was rejected atomically because it contained a duplicated `*** Begin Patch` header. No files were changed by that failed patch; a corrected documentation-only patch was applied and verified with a repository-wide stale-count search and `git diff --check`.

During the Phase 4 security audit, two exploratory file reads returned ENOENT because `ContentCommandHandlers.cs` and `ServiceDefaultsExtensions.cs` were initially guessed paths. No project files were changed by the failed reads. The locations were resolved by listing `src/Papyrus.Application/` and `src/Papyrus.ServiceDefaults/`; the relevant implementations are `ContentCommands.cs` and `Extensions.cs`.

During Phase 7.1 verification, the first Markdown link-check command reported
false positives because it scanned generated `node_modules` documentation,
treated source links with `:line` suffixes as filesystem paths, and passed an
empty directory path for the root README. The command did not modify files.
The checker was narrowed to repository documentation and corrected to strip
line anchors; the remaining ADR 0007 relative links were fixed. An initial
multi-file repair patch was rejected because one expected context line did not
match; a corrected focused patch applied successfully.

## Phase 4 Security Audit

Completed 2026-09-02. Tenant isolation, admin API authorization, API-key authentication and rotation, rate limiting, media storage/serving, Markdown sanitization, security headers, ProblemDetails, and SPA key handling were reviewed. No genuine High or Critical vulnerability was identified, so no application source fix was required. Existing tenant-predicate restatements in lifecycle handlers and adversarial tenant tests were verified. The SPA `localStorage` API-key exposure remains a documented Low risk accepted within the requested no-auth-flow-redesign scope. Required verification passed: `dotnet restore .\Papyrus.sln --nologo` and `dotnet build .\Papyrus.sln --nologo --no-incremental` with 0 warnings and 0 errors.

The audit confirmed that tenant selection is an authenticated-admin boundary: the API key authenticates the admin surface, while the validated tenant header selects the administrative tenant context. Public media URLs remain intentionally readable, but the path guard prevents serving a path for a tenant other than the request context and generated names prevent traversal.

During the Archify documentation task, an initial cleanup patch attempted to delete a non-existent visual-check contact-sheet sidecar and therefore failed atomically; the generated HTML and existing JSON sidecars remained intact. The failed deletion was not a source-code or project-configuration change.

During Phase 3 public UI implementation, two initial multi-file patches were rejected atomically: the first used escaped newline text in CSS context, and the second included a duplicated patch header. No source files were changed by either failed patch; subsequent edits are being split into focused, valid patches.

During Phase 2 backend refinement, the first non-integration test run failed while Minimal API endpoints were being initialized because handler parameters were inferred as request-body parameters (`UNKNOWN`) rather than services. The root cause was that Minimal API metadata did not infer the registered handler classes as services in these lambdas. The handlers were explicitly annotated with `[FromServices]`; the solution build remained green and the verification rerun passed.

The subsequent test rerun exposed a test-host composition gap: `AdminApiTests` built a minimal host without calling `AddPapyrusApplication`, so the newly required handler services were absent. The test host was aligned with production composition by registering the application module; no runtime fallback or service locator was reintroduced.

A later tenant-isolation test failure showed that lifecycle handlers used `IgnoreQueryFilters()` without restating the tenant predicate, allowing a cross-tenant entity to be tracked before `SaveChangesAsync` rejected it. The root cause was the filter bypass needed for restore operations lacking an explicit tenant boundary. Each lifecycle lookup now includes `x.TenantId == db.TenantId`; cross-tenant delete/restore requests return not-found without tracking or saving the foreign entity.

## Phase 3 frontend refinement

- Enabled the React 19 compiler in `src/Papyrus.Admin.UI/vite.config.ts` through `@vitejs/plugin-react` and added the pinned compiler package to the SPA dev dependencies.
- Removed unnecessary `useCallback` wrappers from `src/Papyrus.Admin.UI/src/hooks/useAuth.ts` and `src/Papyrus.Admin.UI/src/hooks/useTenant.ts`. No other manual `useMemo`, `useCallback`, or `React.memo` usages were present in application source. `useDebounce` intentionally retains effect/state timing semantics because compiler memoization cannot replace a temporal debounce.
- Completed Radix implementations for Dialog, AlertDialog, Tabs, and DropdownMenu. Native Select remains an intentional design-system exception because it preserves mobile/keyboard behavior and the existing multiple-select React Hook Form contract. Form now exposes the standard provider/controller/item/label/message primitives.
- Migrated SPA controls and global field styles to semantic shadcn CSS variables while preserving slate and dark-mode behavior. TanStack Query mutations remain unchanged; React Actions were not introduced because React Query owns cache invalidation and mutation state.
- SPA lint and production build both pass with zero reported errors. No self-healing entry was needed for this phase.

During the Phase 4 test backfill, the first non-integration test run failed because a newly added theory case expected the valid category values `slug` and `name` to be rejected. The test data was corrected to contain only invalid empty slug/name cases; the test suite was then rerun.

## Phase 1–2 Royal Pharaonic Admin UI overhaul

- Updated the standalone React Admin SPA with an Obsidian/Basalt/Royal Gold/Sand/Alabaster OKLCH token system, zero-radius primitives, and a 5% repeating papyrus texture.
- Added Phosphor icons with regular weights, an inline `PapyrusLogo`, semantic empty-state watermark treatment, and token-based admin shell styling. Razor Pages projects were not modified.
- Verification passed: `npm run build --prefix src/Papyrus.Admin.UI` and `npm run lint --prefix src/Papyrus.Admin.UI`.

During initial icon migration verification, the first build failed because renamed Phosphor imports left legacy JSX identifiers and Phosphor does not export `Activity`. The imports were aliased to equivalent Phosphor icons (`Pulse`, `Folder`, `TrendUp`, and regular icon equivalents), the stale Lucide dependency was removed, and the build/lint rerun passed.
