# Papyrus — Engineering & Product Roadmap

**Status:** Active, finalized edition. Supersedes all prior roadmap documents.
**Scope:** Personal brand platform - Portfolio (landing/root domain), Blog (subdomain), Admin (control plane for content, theme, localization, navigation, AI configuration). Treated as a long-running, continuously evolving product.

---

## 1. Product Definition

| Property | Decision |
|---|---|
| Root domain | Portfolio - landing/home experience |
| Subdomain | Blog - long-form, read-only |
| Admin | Authenticated, two-factor-protected (M3) control plane for content, theme, localization, navigation, AI configuration |
| Visual identity | Black-and-gold, Pharaonic motifs as restrained accents; self-hosted fonts; direction-agnostic (LTR/RTL) by design |
| Architectural style | .NET modular monolith, Clean Architecture, Ports & Adapters |
| Scaling posture | No service extraction planned; verified by infrastructure monitoring, not assumed |
| Languages | English and Romanian at launch; Arabic (right-to-left) is an intended future locale |

---

## 2. Reference Architecture

```
Cloudflare -> Caddy (reverse proxy, TLS)
      |
Presentation      Api.Admin - Web.Blog - Web.Portfolio - Admin.UI
      |
Application       Use cases, application-owned ports, resolvers (theme, AI provider)
      |
Core              Entities, invariants, multi-tenancy contracts
      ^
Infrastructure    EF Core adapters, migrations, interceptors, DbContext
```

*Target topology: Cloudflare and Caddy are planned (M2/M13); today services run directly via Docker Compose with no reverse proxy or CDN.*

---

## 3. Roadmap

### How to read this roadmap: two tracking schemas

Papyrus tracks delivery through **two complementary schemas**. They are not
competing roadmaps; they operate at different altitudes and reconcile against
each other. Confusing them is the most common source of apparent contradiction
in this repository, so the relationship is stated explicitly here.

| Schema | Scope | Status | Primary audience |
| --- | --- | --- | --- |
| **Phases 0–7** — Platform Bootstrap & Hardening | Structural foundation: security, layering, contracts, delivery pipeline, test depth, observability, release readiness | **Completed / verified baseline** (Phase 7 release tag intentionally open) | Contributors, reviewers, auditors |
| **Milestones M0–M32** — Product Feature Evolution | Visible product capability: routing, theming, localization, content, AI, growth | **Target sequence.** M0 is complete; M1–M32 have not started as a tracked sequence | Product planning, roadmap review |

**Phases 0–7 answer "is the platform sound?"** They are complete or near-complete
and are verified by the numbers in
[Actual Progress](#actual-progress-as-of-last-verified-state) below: build
warnings, test counts, migration ownership, load-test p95 figures, and release
gate status.

**Milestones M0–M32 answer "what does the product become next?"** They describe
the intended feature trajectory. Section 4 gives the canonical ordering, and each
milestone is detailed in `05-milestones/`.

### How the two schemas overlap

The schemas are not sequential-disjoint. Three milestones in the M-sequence have
*adjacent* work that landed during the bootstrap phases. The overlap is real but
partial in every case, so each entry below states only what the repository can
evidence today and what is still open against the milestone's own scope.

- **M5 (Infrastructure & Resource Monitoring)** — the bootstrap baseline built the
  **application** observability layer: a shared `Papyrus` W3C `ActivitySource`,
  logger-backed audit events, tenant-safe redaction and sampling, the dashboard
  definition in `docs/observability/dashboards/`, and the K6 load evidence. None of
  M5's own in-scope items are delivered: `docker-compose.yml` sets no
  per-container resource limits, and there is no host/per-container monitoring
  agent, no threshold-based alerting, and no private resource dashboard. M5 is
  therefore **entirely open**; see
  `05-milestones/m5-infrastructure-and-resource-monitoring.md`.
- **M6 (SEO & Discoverability)** — RSS syndication
  (`Web.Shared/Syndication/SiteFeed.cs`), sitemap generation, canonical URLs, and
  Open Graph metadata in the shared layout shipped during Phase 4. Still open
  against `05-milestones/m6-seo-and-discoverability.md`: JSON-LD structured data
  (`Person` on Portfolio, `BlogPosting` on Blog — no `application/ld+json` block
  exists today) and the Admin indexing exclusion (`robots.txt` disallow plus a
  per-route `noindex`; neither `robots.txt` nor `noindex` exists today).
- **M8 (Brand & Motion Implementation)** — the shared stylesheet
  (`Web.Shared/wwwroot/css/papyrus.css`) defines the design tokens
  (`--font-sans`, `--font-display`, `--font-mono`) that the black-and-gold
  Portfolio and Blog surfaces already consume, together with entrance keyframes
  and a `prefers-reduced-motion` path, and `Web.Shared/wwwroot/js/papyrus.js`
  wires scroll-triggered `.reveal-on-scroll` sections. The design **spec** lives in
  `docs/03-design-system/`. Still open against
  `05-milestones/m8-brand-and-motion-implementation.md`: self-hosted typefaces
  (no `@font-face` and no font asset is tracked), the hero scroll-unroll entrance,
  the accent shimmer and card-lift interactions, and the milestone's rollout
  verification (third-party font-request check, Lighthouse, accessibility).

### M0 as the permanent structural contract

M0 (Architectural Guardrails) is the one milestone that is **not** merely a
feature. It formalizes the structural boundaries that the bootstrap phases
already established, converting a convention into an enforced contract:

- Architecture fitness tests (`src/Papyrus.Tests/Architecture/`) assert that
  Core does not depend on Infrastructure or web frameworks, that public
  contracts do not leak Application/Infrastructure internals, that Web.Shared
  stays presentation-only, that public delivery layers cannot depend on the
  Admin write layer, and that Application does not depend on delivery layers.
- Migration ownership tests assert that Infrastructure owns the `DbContext` and
  migrations, and that only Admin startup authorizes schema migration.
- The `Application → Infrastructure` inversion is recorded in
  `02-architecture/decisions/adr-0002-application-owned-ports-with-infrastructure-adapters.md`.

Because these tests run in CI, M0's guardrails now govern **every future M1–M32
milestone**: new feature work is required to preserve the same boundaries rather
than re-litigate them per feature. **M0 is complete: all four Definition-of-Done
items are met.** The feature-scoped Admin endpoint grouping is delivered by
`src/Papyrus.Api.Admin/Endpoints/`; every Admin error response conforms to RFC 7807
`ProblemDetails`; and the Presentation-to-Infrastructure reference audit produced
[ADR-0010](02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md),
which scopes the boundary and enforces it with three fitness rules. M1 Routing
Truth is the next milestone in the sequence.

**Reading rule:** when the phase tracker and the milestone sequence disagree,
the phase tracker is authoritative for *what is built and verified today*, and
the milestone sequence is authoritative for *what is planned next*. Neither
invalidates the other.

### Foundation
M0 Architectural Guardrails · M1 Routing Truth · M2 Reverse Proxy & TLS Termination · M3 Admin Two-Factor Authentication · M4 Backup & Disaster Recovery · M5 Infrastructure & Resource Monitoring · M6 SEO & Discoverability · M7 Contact, Compliance & Trust · M8 Brand & Motion Implementation · M9 Case-Study Project Pages

### Admin Control Plane
M10 Theming System · M11 Localization Foundation (with RTL support) · M12 Navigation & Site Settings

### Infrastructure Fit
M13 Edge & CDN Layer (Cloudflare) · M14 Transactional Email Infrastructure

### Content Platform Maturity
M15 Content Reading Experience · M16 Community Layer · M17 Content Workflow & Versioning · M18 Site-Wide Search · M19 Media & Visual Craft

### Personal Brand & Credibility
M20 Career & Credibility Modules · M21 Uses & Now Page, Availability Status · M22 GitHub Credibility Integration · M23 Visitor Experience Toggle · M24 Navigation & Interaction Polish

### AI Platform
M25 AI Provider Configuration · M26 AI-Assisted Authoring · M27 AI Usage & Cost Guardrails

### Growth & Automation
M28 Newsletter & Syndication · M29 Progressive Web App · M30 Platform Observability & Trust · M31 Feature Flag Platform · M32 Repo Openness Decision

Each milestone is detailed in full in `05-milestones/`.

---

## 4. Delivery Sequence

M0 -> M1 -> M2 -> M3 -> M4 -> M5 -> M6 -> M7 -> M8 -> M9 -> M10 -> M11 -> M12 -> M13 -> M14 -> M15 -> M16 -> M17 -> M18 -> M19 -> M20 -> M21 -> M22 -> M23 -> M24 -> M25 -> M26 -> M27 -> M28 -> M29 -> M30 -> M31 -> M32

---

## 5. Governance

Each milestone is tracked as a GitHub Milestone; each deliverable within it as an Issue. Every new aggregate ships as one complete vertical slice per pull request. Every new external dependency is recorded in `06-operations/vendor-dependency-map.md`.

---

## Actual Progress (as of last verified state)

This section records what has genuinely been shipped, separated from the M0–M32
forward plan above so a reader can compare "what the roadmap says" with "what's
actually built". It is the evidence half of the two-schema model described in
[How to read this roadmap](#how-to-read-this-roadmap-two-tracking-schemas): the
phases below are the *completed / verified baseline*, while the M0–M32 sequence
in Section 3 is the *target sequence*. Where a milestone (notably M5, M6, and
M8) has adjacent work that already shipped, Section 3 states exactly how much and
what is still open, so nothing is double-counted here.

Phases 0–6 are **complete**; Phase 7 (release readiness and
GitHub showcase) is **in progress** with only roadmap item 4 (tagging a release)
still open; Phase 8 is a decision gate and not started.

### Phases and subtasks shipped

- **Phase 0–5:** audit closure and guardrails, tenant isolation and admin
  hardening, request/media/HTTP security, domain/application layering,
  REST contracts and projections, and containers/Aspire/Compose/CI-CD are all
  complete.
- **Phase 6 (S30.0–S30.12):** architecture/layer tests, PostgreSQL integration
  harness and depth, handler/migration-drift tests, Playwright browser smoke,
  K6 load evidence, telemetry core (shared `Papyrus` W3C ActivitySource,
  structured audit events), tenant-safe redaction/cardinality/sampling,
  dashboards and alert families, performance budgets, runbooks, and the
  Postgres RLS decision ADR are all done.
- **Phase 7:** items 1–3 complete — the public documentation set, five
  sanitized showcase assets under `docs/assets` (blog, portfolio, admin,
  health, Aspire), and the contribution/disclosure/license/changelog/
  feature-matrix documentation are published. Item 4 (release tag) is
  intentionally open: it is authorized only after all release gates and
  rollback drills pass.

### Key verified figures

- **Tests:** 116 unique .NET tests in CI (100 provider-independent /
  non-integration tests plus 16 PostgreSQL-backed integration tests), plus a
  separate 5-test Playwright browser smoke suite. The warnings-as-errors build
  is 0 warnings / 0 errors.
- **Migrations:** five EF Core migrations applied; the Admin API is the sole
  migration/seed owner and public hosts never migrate.
- **S30.5 K6 load evidence (2026-08-29):** public run HTTP p95 23.574 ms,
  payload p95 9,030 bytes, 0% failed, 100% checks; admin run HTTP p95
  21.660 ms, payload p95 9,256 bytes, 0% failed, 100% checks, 0 5xx.
- **S30.9 production-like re-measurement (2026-09-03):** public HTTP p95
  32.79 ms, payload p95 16,887 bytes; admin HTTP p95 19.35 ms, payload p95
  19,827 bytes; all within the explicit budgets.
- **S30.12 RLS decision:** PostgreSQL Row-Level Security was evaluated and
  **declined**; application-level tenant filtering (EF global filters plus
  `EnforceTenantBoundary()`) remains the sole enforcement layer. Recorded in
  `02-architecture/decisions/adr-0004-retain-multitenant-entities.md`.

### Implementation alignment note (2026-09-08)

The `Application → Infrastructure` dependency was inverted to align with
ADR-0002 (application-owned ports with Infrastructure adapters):
`Papyrus.Application` owns the persistence ports
(`IPublicContentStore`, `IContentCommandStore`, `IContentQueryStore`,
`ITaxonomyCommandStore`, `IContentLifecycleStore`) and thin facades;
`Papyrus.Infrastructure` implements them with EF Core adapters (`Ef*Store`).
The application layer no longer depends on `PapyrusDbContext`.

### Release-gate status (as of last verified state)

Tracked per-gate in `04-engineering-standards/release-gates.md`:

- **W6 (deployment/operations):** evidence attached (2026-09-02 local release
  drill — PASS).
- **W7 (observability/alerting):** partial — code/config evidence attached;
  production collector verification pending.
- **W8 (release documentation/CI):** evidence attached (2026-09-03 gitleaks
  scan — no leaks found).
- **O4 (telemetry/audit evidence):** partial — local artifact/config evidence;
  production telemetry verification pending.
- **O7 (restore/load drills):** closed (2026-08-29).
- **O8 (performance budgets):** evidence attached (2026-09-03 re-measurement).
- **O9 (release artifacts/sanitized publication):** evidence attached
  (2026-09-03 gitleaks scan).
- **O10 (architecture evidence):** evidence attached (S30.0 architecture
  tests).
