# Agent Review Playbook

**Current:** this is the concrete review checklist an agent (or a human) runs against a diff before approval, ported verbatim from `docs/review/copilot-review-instructions.md`.

Focused guidance for AI coding agents performing a **review** in the Papyrus repository. Use together with [AGENTS.md](../../AGENTS.md) for architecture boundaries, build/test commands, and conventions. Review language: **English**. Prioritize findings as 🔴 CRITICAL (block merge), 🟡 IMPORTANT (discuss), 🟢 SUGGESTION (non-blocking).

## Verify before reviewing

Never trust a claim about what runs or passes. Run the gates yourself where feasible:

```powershell
dotnet build Papyrus.sln --configuration Release --no-restore /p:TreatWarningsAsErrors=true
dotnet test Papyrus.sln --filter "FullyQualifiedName!~Papyrus.Tests.Integration"
npm run build --prefix src/Papyrus.Admin.UI
npm run lint --prefix src/Papyrus.Admin.UI
```

Zero warning/error is a hard gate. If a gate cannot run (e.g. integration tests need the Postgres service), say so explicitly rather than assuming it passes.

## Security review priorities (maps to OWASP Top 10 2025)

Consult [docs/security/threat-model.md](../security/threat-model.md), [policy](../security/policy.md), and [risk-acceptance](../security/risk-acceptance.md).

- **A01 Broken access control / IDOR:** every Admin endpoint must authenticate via `X-Admin-Api-Key` and resolve tenant ownership. Verify the `AdminOpenApiDocumentTransformer` still guards every documented path (regression: a one-arg security-scheme constructor previously serialized empty `[{}]` auth).
- **A05 Injection:** parameterized EF/LINQ everywhere (no SQL string building), Markdown rendered only through the **safe renderer**, uploads validated as raster media (magic-byte checked, 5 MB cap); no `innerHTML`/`v-html`/XSS-by-sanitization-bypass in the React admin SPA.
- **A04 Cryptographic / secrets:** no keys, connection strings, cookies, or raw PII in code, logs, or commit. Admin auth must stay constant-time (`AdminApiKey.MatchesAny`, no early-exit comparison).
- **A09 Logging:** audit events logger-backed, never sampled; redaction at the sink seam; confirm sensitive values still excluded.
- **A07 Auth/sec session:** multi-key rotation semantics preserved (`AdminApi:ApiKeys` add/overlap/remove); production still fails fast on missing or `change-me` key.
- **Tenant isolation (ADR 0007): no RLS.** Application-level filtering + `EnforceTenantBoundary()` is the only enforcement layer. Flag any change that weakens that boundary (e.g. un-filtered query, cross-tenant association, missing soft-delete predicate).

## Architecture review priorities

- Respect accepted ADRs in [docs/architecture/decisions/README.md](../architecture/decisions/README.md); proposing to violate one requires an ADR change, not a silent code change.
- Modular monolith boundaries: Core/Application/Contracts/Infrastructure/Admin responsibilities must not blur. Public hosts (Blog/Portfolio) must stay read-only and must not call migration APIs.
- CQRS discipline in `Papyrus.Application`: commands/query separation, bounded no-tracking projections; flag any new N+1 or missing `AsNoTracking()`.

## Correctness / regression checkpoints for this repo

- **EF migrations:** never edit/delete an applied migration; name-diff and model-snapshot drift are CI-checked. Minority risk: a change to the EF model that does not add a migration.
- **OpenAPI drift:** `docs/api/openapi.json` and Admin UI `schema.d.ts` are generated and diff-checked. Flag hand-edits or out-of-sync regenerated files.
- **Soft deletes:** confirm deletes still route through the interceptor and filters exclude soft-deleted rows; flag hard deletes outside the owner.

## Review checklist

### Code quality

- [ ] Descriptive names, single-responsibility, no magic numbers/strings
- [ ] Error handling fails safe, no silent catches, no stack traces to client in prod
- [ ] No dead code, commented-out code, or drift between code and docs

### Security

- [ ] Endpoints authenticated + tenant/ownership checked
- [ ] Parameterized queries; safe markdown; validated media uploads
- [ ] No secrets/PII in commits, config, or logs
- [ ] Constant-time key comparison preserved

### Testing

- [ ] New behavior covered (unit/contract/integration) and named well
- [ ] CTests cover edge cases and error paths; no tests that always pass

### Performance

- [ ] No N+1, missing-`AsNoTracking`, or unbounded projections
- [ ] Feeds/sitemaps/search still bounded and tenant-aware

### Docs

- [ ] Code ↔ [feature matrix](../00-overview/feature-matrix.md) / [limitations](../00-overview/limitations.md) in sync
- [ ] Changelog and release-gate status updated when behavior changes

## Report format

Produce a short structured report: summary verdict, 🔴/🟡/🟢 findings with file:line references, and for each finding a concrete suggested fix. End with a verified-gates list (which commands actually ran and passed vs. skipped).