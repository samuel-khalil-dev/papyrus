# Porting Spec — Step 3: Critical Omissions into papyrus-docs/

Implementation spec for the agent executing step 3 of the reconciliation report
(`1789126488475-docs-vs-papyrus-docs-report.md`, Section 7 and the per-file
`merge` dispositions in Section 6).

## Critical Constraint — Read Before Starting

This spec does **not** contain the source content to port, only where it lives and
where it goes. Unlike the C1–C6 corrections (which fixed claims I could quote exactly
from the report), the files below contain real operational detail — threat entries,
telemetry field names, exact commands — that must be read directly from `docs/` and
carried over **faithfully**, not paraphrased, summarized, or reconstructed from memory
of the report's description of them.

**Rules for this task:**
1. Open and read each source file in full before writing anything to its target.
2. Port content accurately — exact field names, exact command strings, exact IDs
   (e.g. `T-01`, `EventId 1001`) must match the source verbatim. Do not "clean up"
   or rephrase technical identifiers.
3. Where the source file's content conflicts with something already corrected in
   `papyrus-docs/` under the C1–C6 pass, the **already-corrected papyrus-docs/ text
   wins** — do not reintroduce a stale claim by porting it uncritically. If such a
   conflict is found, flag it in the PR description rather than silently resolving it.
4. Every ported section is written using the same Current/Planned labeling
   convention established in the C1–C6 pass. Content from `docs/` is, by definition,
   describing the **current** implemented system unless the source itself says
   otherwise.
5. Do not delete, move, or modify any source file in `docs/` as part of this task.
   This is a copy/port operation only. Retirement of old files is step 5, not this
   step.
6. If a target file does not yet exist in `papyrus-docs/`, create it following the
   existing numbering and naming convention of its section (e.g. a new file under
   `02-architecture/` continues that folder's existing style).

---

## 3.1 — OpenAPI Contract & Endpoint Inventory

**Source:** `docs/api/README.md`, `docs/api/openapi.json` (read for reference only —
do not copy or move the JSON file itself; it is CI-diffed and must stay in place).

**Target:** `02-architecture/api-standards.md`

**Port:** The real endpoint inventory (report cites 23 operations), the `/api/v1`
versioning as actually implemented, rate-limit values, the `X-Tenant-Id` header
schema/regex, and the API-key authentication scheme exactly as implemented (already
corrected under C1 — do not re-describe auth here, just link back to the corrected
section).

**Add as a new subsection**, e.g. `## Current Endpoint Inventory`, distinct from the
existing conventions-only content already in this file.

---

## 3.2 — C4 Architecture Diagrams

**Source:** `docs/architecture/c4/context.md`, `container.md`, `component.md`

**Target:** New file `02-architecture/c4-diagrams.md`

**Port:** All three levels in full, including any Mermaid source or diagram
references. Reference this new file from `02-architecture/architecture-overview.md`'s
existing content (add a pointer, do not duplicate the diagrams inline).

---

## 3.3 — Threat Register, Risk Acceptance, Security Policy

**Source:** `docs/security/threat-model.md` (T-01 through T-08 with per-threat
status/gates), `docs/security/risk-acceptance.md`, `docs/security/policy.md`,
`docs/security/threat-model-summary.md`

**Target:** New file `04-engineering-standards/threat-register.md`

**Port:**
- The full T-01–T-08 threat register with each threat's current status and its
  associated gate, exactly as recorded.
- The risk-acceptance record (W1–W5/O1–O3/O5/O6 accepted items and retained gates),
  as its own subsection.
- Fold `policy.md`'s disclosure pointer ("no disclosure in public issues," pointer to
  root `SECURITY.md`) into `04-engineering-standards/security-baseline.md` as a short
  closing note, rather than a separate file.
- The sanitized public summary (`threat-model-summary.md`) becomes a short public-facing
  excerpt at the top of the new file, clearly marked as the public-safe version, with
  the full register below it.

---

## 3.4 — Telemetry, Alerting, and Observability Contracts

**Source:** `docs/runbooks/telemetry-runbook.md`, `docs/runbooks/alerts.md`,
`docs/runbooks/observability-production-checklist.md`

**Target:** `06-operations/observability-and-monitoring.md` (already corrected under
C4 — extend it, do not overwrite the corrected C4 text)

**Port:**
- The emission/redaction/cardinality/sampling contract from the telemetry runbook,
  including the exact config keys (`Telemetry:SamplingRate`, `Telemetry:CardinalityLimit`)
  and the `EventId 1001` never-sampled rule.
- All nine live alert families from `alerts.md`, with their trigger conditions.
- The W7/O4 verification checklist from `observability-production-checklist.md`,
  including its reference to `ops/dashboards/papyrus-dashboard.json` (link, do not
  copy the dashboard JSON itself).

---

## 3.5 — Incident Runbook

**Source:** `docs/runbooks/incident-runbook.md`

**Target:** `06-operations/runbooks.md`

**Port:** The SEV process, the key-rotation procedure via `AdminApi:ApiKeys`, and the
drill-record convention, added as new sections alongside the existing runbook entries
already in this file. Do not remove the existing planned-milestone runbook entries
(e.g. the M5 disk-alert runbook) — both current and planned runbooks coexist here,
each correctly labeled.

---

## 3.6 — Changelog, Feature Matrix, Known Limitations

**Source:** `docs/project/changelog.md`, `docs/project/feature-matrix.md`,
`docs/project/limitations.md`

**Target:** Three new files:
- `00-overview/changelog.md` — port the full Keep-a-Changelog history verbatim.
- `00-overview/feature-matrix.md` — port the Implemented/Deferred/Verification-pending
  matrix exactly as recorded, resolving the 84→86 test-count drift (C10 in the report)
  in favor of **86** (70 provider-independent + 16 PostgreSQL integration) — the newer,
  correct count — everywhere it appears in the ported content.
- `00-overview/limitations.md` — port the boundary list (not full SaaS, no RLS as a
  second layer, API-key-only auth today, CI-only integration tests) verbatim, making
  sure it stays consistent with the already-corrected C1/C5 text elsewhere.

Reference all three from `00-overview/project-charter.md`.

---

## 3.7 — Migration Ownership Decision Record

**Source:** `docs/architecture/decisions/0005-migration-ownership.md`

**Target:** Already correctly described in `06-operations/deployment-guide.md` under
C3. Add one line there linking back to this source ADR by its `docs/` path, so the
decision's full rationale remains reachable rather than only its conclusion.

---

## 3.8 — RLS-Decline Decision (Full Mechanics)

**Source:** `docs/architecture/decisions/0002-tenant-enforcement.md`,
`docs/architecture/decisions/0007-postgres-rls-decision.md`

**Target:** `02-architecture/decisions/adr-0004-retain-multitenant-entities.md`
(already renamed and partially updated under C5)

**Port:** The EF Core global-filter and `EnforceTenantBoundary()` mechanics from
`0002`, and the RLS evaluation history and rationale from `0007`, as supporting detail
under the section already added in the C1–C6 pass. Do not create a duplicate separate
ADR — this content extends the existing corrected ADR-0004.

---

## 3.9 — Media Upload Security Policy

**Source:** `docs/architecture/decisions/0003-admin-session-and-media-policy.md`
(media-policy portion only — the auth portion of this file is superseded history, not
part of this port)

**Target:** `04-engineering-standards/security-baseline.md`

**Port:** The SVG-rejection rule, magic-byte validation, non-executable upload
serving, and the 5 MB cap, as a new subsection `## Media Upload Security`.

---

## 3.10 — Agent Review Playbook

**Source:** `docs/review/copilot-review-instructions.md`

**Target:** New file `07-governance/agent-review-playbook.md`

**Port:** The full OWASP-mapped review checklist verbatim. Reference it from
`07-governance/contribution-workflow.md` and from `07-governance/definition-of-done.md`
as the concrete checklist an agent (or a human) runs against a diff before approval.

---

## 3.11 — Concrete Setup, Deployment, and Contributing Commands

**Source:** `docs/guides/local-setup.md`, `docs/guides/deployment.md` (commands only —
topology claims are already corrected under C2/C3, do not reintroduce old topology
claims), `docs/guides/contributing.md`, `docs/guides/tutorials/getting-started.md`

**Target:**
- `06-operations/environments.md` — add the real run commands
  (`dotnet run --project src/Papyrus.AppHost`, `npm ci --prefix src/Papyrus.Admin.UI`),
  actual ports, and seed-data steps.
- `06-operations/deployment-guide.md` — add exact environment variable names, volumes,
  and the non-root `APP_UID` detail, consistent with the already-corrected C2/C3/C6
  topology and secrets text.
- `07-governance/contribution-workflow.md` — add the `dotnet ef migrations add`
  command convention and the Conventional Commits detail.
- New file `01-product/getting-started-tutorial.md` — port the full tutorial.

---

## Assets

**Source:** `docs/assets/blog.png`, `portfolio.png`, `admin.png`, `health.png`,
`aspire.png`, `Papyrus_Logo.svg`, `Papyrus_Background.svg`

**Action:** Copy (not move) into a new `papyrus-docs/assets/` folder. Reference the
screenshots from wherever `papyrus-docs/` now describes the public-facing sites
(e.g. `01-product/vision-and-positioning.md` or the new C4/getting-started files), and
reference the two brand SVGs from `03-design-system/design-tokens.md` as the actual
source-of-truth brand assets, replacing any purely descriptive color-only framing
there with a pointer to the real files.

---

## Explicitly Out of Scope for This Step

- Do not touch `docs/architecture/release-gates.md`, `docs/security/audit-register.md`,
  `docs/security/gitleaks-sarif-*.sarif`, or any `docs/performance/*` evidence file —
  these are report-flagged **archive**, not **merge**, and are handled in step 5, not
  this step.
- Do not repoint any external reference (`AGENTS.md`, root `README.md`, `SECURITY.md`,
  CI workflows, `memory-bank/`, `ops/dashboards/`, `plans/`) — that is step 4.
- Do not delete anything from `docs/` — that is step 5, and only after steps 3 and 4
  are both complete and merged.

---

## Verification After Porting

- Every new/extended file uses the Current/Planned labeling convention.
- Grep the newly ported content for "84" test-count references — zero remaining;
  all read "86".
- Grep for any topology or auth claim that contradicts the already-merged C1–C6
  corrections — zero conflicts introduced by this port.
- Confirm `docs/` itself is completely unmodified (`git diff --stat docs/` shows no
  changes) — this step only writes to `papyrus-docs/`.
- Open as a single PR per top-level subsection (3.1–3.11) if the diff is large, or as
  one PR with clearly separated commits per subsection if preferred — do not merge
  without review, consistent with prior practice.
