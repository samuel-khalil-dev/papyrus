# Progress

| Phase | Scope | Status |
| --- | --- | --- |
| 0 | Audit closure and guardrails | Complete |
| 1 | Tenant isolation and admin session hardening | Complete |
| 2 | Request integrity, media, and HTTP security | Complete |
| 3 | Domain/application layering and persistence correctness | Complete |
| 4 | REST contracts, projections, and frontend synchronization | Complete |
| 5 | Containers, Aspire, Compose, and CI/CD | Complete |
| 6 | Observability, test depth, and performance budgets | Complete |
| 7 | Release readiness and showcase | In progress — release tag and operational gates (W8, O9, O8, W7, O4) open; CI/OpenAPI verification complete |
| 8 | Measured future scale decision gate | Not started |

## Verification baseline

- 70 non-integration .NET tests pass locally. CI adds 16 PostgreSQL-backed integration tests for 86 unique .NET tests; those integration tests are excluded locally because no local PostgreSQL listener is available (they require `postgres:17-alpine`). The separate Playwright browser suite currently contains 5 tests.
- Solution build passes with 0 warnings and 0 errors; restore also passes.
- Five EF Core migrations are applied.
- Phase 7 verification baseline (commit `2b54b25`): `dotnet restore` + `dotnet build .\Papyrus.sln --nologo --no-incremental` = 0 warnings/0 errors; Admin UI `npm ci` + `npm run build` = success; `dotnet test` = 70 pass / 16 fail locally (the 16 are CI-only PostgreSQL integration tests, not a defect).

## Dependabot integration and ignore-file hardening milestone — COMPLETE (2026-09-07)

- Added `.github/dependabot.yml` with Dependabot version 2, a weekly schedule, and `open-pull-requests-limit: 10` for three ecosystems: `nuget` (directory `/`), `npm` (directories `/src/Papyrus.Admin.UI` and `/tests/e2e`), and `github-actions` (directory `/`).
- Hardened `.gitignore`: added compiled binaries (`*.dll`, `*.exe`, `*.pdb`), TS/Node caches (`*.tsbuildinfo`, `.vite/`, `.eslintcache`), secret key/cert files (`*.snk`, `*.pfx`, `*.p12`, `*.key`), package-manager logs, and transient/editor files (`*.orig`, `*.rej`, `*.bak`, `*.tmp`, `*.temp`, `*.swp`).
- Hardened `.dockerignore`: added `**/node_modules/`, `**/dist/`, `coverage/`, `artifacts/`, `.idea/`, `*.tsbuildinfo`, `.vite/`, `.eslintcache`, `*.nupkg`/`*.snupkg`, `packages/`, `*.log`/`logs/`, local DB files, OS files, and non-build dirs `plans/` and `ops/`.
- Updated `.gitattributes`: kept `* text=auto` and added explicit LF/CRLF handling for config/ignore files and source extensions, plus additional binary types.
- A tracked-file safety check (`git ls-files`) confirmed no tracked files were affected by the new ignore rules.
- All changes are repository-hygiene/CI-tooling only; no application source, dependencies, secrets, or migrations were modified.

## CI workflow hardening milestone — COMPLETE (2026-09-05)

- Bumped GitHub Actions dependencies across `ci.yml`, `openapi-schema.yml`, `perf-baseline.yml`, and `release.yml`: `actions/checkout@v7.0.1`, `actions/setup-dotnet@v6.0.0`, `actions/setup-node@v7.0.0` (Node 24), `actions/upload-artifact@v7.0.1`, pinned `gitleaks` (v3.0.0), `trivy-action` (v0.36.0), `docker/login-action` (v4.6.0), `docker/setup-buildx-action` (v4.3.0), `anchore/sbom-action` (v0.24.2), and `grafana/setup-k6-action` (v1.2.1, k6 0.45.0). Added `packages: write` permission to `ci.yml`.
- Updated Docker login/setup-buildx actions to v4 in `release.yml`.
- Switched OpenAPI normalization to `python3` and made the OpenAPI contract comparison ignore CRLF/LF line-ending differences in `ci.yml` and `openapi-schema.yml`.
- Resolved leftover merge-conflict markers in `memory-bank/decisionLog.md` and `memory-bank/progress.md` (commit `f4cf021`).
- All changes are CI-only; no application source, dependencies, secrets, or migrations were modified.

## CI verification and OpenAPI contract milestone — COMPLETE

- Verification of the `cb312eb` CI/OpenAPI fixes is complete and requires no further code changes:
  - CI workflow review: `ci.yml` and `release.yml` both use `fetch-depth: 0` and have `security-events: write`; no defects block the gitleaks SARIF upload or the 16 PostgreSQL integration tests.
  - Local build: `dotnet build .\Papyrus.sln --nologo --no-incremental` → 0 warnings, 0 errors.
  - Non-integration tests: 70 passed, 0 failed.
  - OpenAPI contract: `docs/api/openapi.json` is byte-for-byte identical to the runtime-generated contract (no drift); `schema.d.ts` has no content drift (only cosmetic key-ordering from `openapi-typescript` 7.13.0 vs declared `^7.9.1`).
  - Remaining open release gates (W8/O9 secret-scan artifact, O8 re-measurement, W7/O4 production verification) are execution steps requiring an actual CI run and production verification — not code defects.
  - Non-blocking observation: consider pinning `openapi-typescript` to an exact version for byte-stable `schema.d.ts` regeneration (recorded in `decisionLog.md`).

## CI verification and OpenAPI contract milestone

- Installed and authenticated the GitHub CLI (`gh`) to inspect the CI run on the pushed commit `cb312eb`.
- Fixed the gitleaks secret-scan failure by adding `fetch-depth: 0` to the checkout step in both `ci.yml` and `release.yml` (shallow clone prevented gitleaks from resolving the parent commit). The redacted SARIF artifact was already uploading successfully and no secrets were found.
- Fixed the OpenAPI contract drift by correcting the runtime code: excluded the `/admin` Admin SPA routes from the OpenAPI, fixed the `AdminApiKey` security serialization (two-arg `OpenApiSecuritySchemeReference` + top-level `document.Security`), and restored the `X-Tenant-Id` parameter component with path-level refs.
- Regenerated `docs/api/openapi.json` (now exactly matches the runtime document) and `src/Papyrus.Admin.UI/src/api/schema.d.ts`.
- Verified with `dotnet build` (0 warnings/0 errors, warnings-as-errors), Admin UI production build, 70 passing non-integration tests, and `git --no-pager diff --check`.

## UI bug-fix milestone

- Completed logo/background asset rendering fixes, non-repeating theme-aware backgrounds, sticky public footer layout, Admin/public favicon links, production-safe Admin background imports, and duplicate token cleanup.
- Verified with the Admin production build, solution build (0 warnings/0 errors), and `git --no-pager diff --check`.

## Reconstruction status

- Admin SPA Royal Pharaonic design-system implementation is complete: Cinzel/Inter/JetBrains Mono typography, canonical dark/light tokens, theme attribute wiring, zero-radius UI primitives, glass shell surfaces, semantic button and badge variants, tokenized texture/motion, responsive feature surfaces, and currentColor logo behavior are in place.
- Admin verification passed with `npm run build --prefix src/Papyrus.Admin.UI`, `npm run lint --prefix src/Papyrus.Admin.UI`, and `git --no-pager diff --check`.

- Public Blog/Portfolio UI/UX theming fix is complete: light-mode tokens and `color-scheme: light` were added, the light background texture was made visible with multiply blending, and the shared SVG logo now inherits the active theme accent through `currentColor`.
- SVG assets have the required `viewBox` attributes, and obsolete `Papyrus_Background_2.svg` was fully removed with no dangling references.
- The theming fix was verified by a 0-warning/0-error solution build and a passing `git diff --check`.
- Shared public design-system finishing layer is implemented: the papyrus tile is unified at 720px with dark screen/light multiply blending, dark gold orbs/grid are enabled, canonical input/ring/muted/border aliases are present, all public surfaces are zero-radius, shared controls use gold-tinted focus rings, and hero page-load reveals respect reduced motion.
- C4 architecture documentation was reconstructed under `docs/architecture/`, including context, container, component, overview, release gates, and ADR index.
- OpenAPI 3.1 documentation was reconstructed under `docs/api/`, including `openapi.json` and README.
- Diátaxis guides and runbooks were reconstructed under `docs/guides/` and `docs/runbooks/`.
- Phase 7.1 public documentation was published: architecture explanation,
  threat-model summary, local setup/demo tutorial, API reference enhancements,
  deployment how-to, and limitations reference.
- Phase 7.2 showcase assets were captured under `docs/assets/` for Blog,
  Portfolio, Admin, Health, and Aspire. README and public documentation link to
  the sanitized captures; no GIF was needed for the static showcase surfaces.
- Audit fixes G1–G4 and G10 are complete: roadmap status, empty Admin directories, frontend schema/port synchronization, and CI Trivy pinning.
- This relative `memory-bank/` directory is the current context source; no `.memory-bank/` directory is used.

## Phase 7 release readiness and showcase

- Phase 7.1 (public documentation) and 7.2 (showcase assets) were completed earlier.
- This session finalized the remaining Phase 7 work:
  1. Reconciliated the stale Phase 7 status in `docs/project/roadmap.md` from "Not started" to "In progress", recording items 1–3 complete and item 4 (tag a release) open.
  2. Added a Phase 7 section to `docs/project/changelog.md` under the `[Unreleased]` heading.
  3. Verified `docs/guides/contributing.md`, `SECURITY.md`, `LICENSE`, the README Support section, and `docs/architecture/feature-matrix.md` are complete/truthful — no changes needed.
  4. Fixed `docs/architecture/release-gates.md`: corrected the stale test-count claim (84→86 unique tests, 14→16 integration) and added the previously-missing O9 gate section (Release artifacts and sanitized publication evidence, status: partial, CI secret-scan pending).
- Committed all Phase 7 work in commit `2b54b25` ("docs: finalize Phase 7 release-readiness documentation and assets", 20 files, +147/−40). Working tree is clean.
- The gitleaks CI/release secret-scan wiring is now complete: `security-events: write` was added to the workflow-level `permissions` block in `.github/workflows/release.yml` (line 8). The gitleaks `gitleaks/gitleaks-action@v2` step already existed in both `ci.yml` (line 53, `verify` job) and `release.yml` (line 51, `dependency-scan` job); the release workflow now has the permission needed to upload the redacted SARIF secret-scan artifact. This closes gates W8 (CI secret-scan artifact) and O9 (release artifacts / sanitized publication evidence) at the code level, pending a CI run to produce the artifact.
- Release gate status: W6, O7, O10 closed; W8, O9 code in place (artifact pending a CI run); W7, O4 partial (production verification pending); O8 open (re-measurement pending). No release tag was created — item 4 remains intentionally open.

# Current documentation milestone

- Created and showcase-validated two Archify architecture specifications and delivered standalone HTML artifacts under `docs/architecture/`.
- Browser visual-check was attempted for both artifacts; automated evidence was skipped because Chrome/Chromium was unavailable in the environment.

## Phase 3 frontend refinement

- React 19 compiler enabled for the Admin SPA; unnecessary auth/tenant callback wrappers removed and debounce timing retained for correctness.
- Radix-backed Dialog, AlertDialog, Tabs, and DropdownMenu primitives completed. Native Select formally retained for multiple-select and mobile behavior; Form primitives completed with React Hook Form integration.
- Global field/control styling migrated to semantic shadcn CSS variables with slate/dark-mode support.
- Admin SPA lint and build pass with zero errors.

## Phase 3 public Royal Pharaonic UI overhaul

- Public Portfolio and Blog now share an Obsidian `#0F0F13`, Basalt `#1A1A1F`, Royal Gold `#D4AF37`, Sand `#E5D3B3`, and Alabaster `#EDEDED` design-token layer.
- Shared Razor layout uses Cinzel, Inter, and JetBrains Mono, with a low-opacity repeating papyrus texture and zero-radius public surfaces.
- Portfolio hero uses the gold Papyrus mark; cards use a sharp Basalt grid with a 1px Royal Gold hover border.
- Blog and Portfolio Markdown prose use Cinzel headings, Inter body copy, Basalt code blocks with a gold top edge, and Sand serif italic blockquotes.
- Public-only verification passed: `dotnet restore .\\Papyrus.sln --nologo` and `dotnet build .\\Papyrus.sln --nologo --no-incremental` with 0 warnings and 0 errors.
