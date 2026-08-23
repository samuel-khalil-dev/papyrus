# W/O Findings Risk Acceptance

## Decision

- **Decision date:** 2026-08-25
- **Decision owner:** Project owner
- **Tracked release gates:** W6-W8, O4, and O7-O10, plus deployment portions
  of O-NEW-04 and O-NEW-05.
- **Decision:** W1-W5, O1-O3, O5, and O6 are removed from this risk-acceptance
  record because the current implementation verifies that they are no longer
  threats. W6-W8, O4, and O7-O10 remain explicit operational or release-gate
  verification items and are not waived by this document.

The source audit supplied identifiers without original prose or evidence. The
repository does not fabricate those artifacts. The current code and test
evidence for the resolved items is recorded below; the retained operational
items keep concrete release-gate steps in [`audit-register.md`](audit-register.md).

## Resolved findings removed from acceptance

These identifiers are historical dispositions, not accepted risks or open
findings:

- **W1-W5:** explicit modular boundaries and migration ownership;
  tenant/query/relationship/save/media isolation; green security controls;
  antiforgery, secure cookies, rate limiting, and validated media. Evidence:
  [`0001-modular-monolith-boundaries.md`](../architecture/decisions/0001-modular-monolith-boundaries.md:5),
  [`PapyrusDbContext`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32),
  [`InitializeDatabaseAsync()`](../../src/Papyrus.Infrastructure/DependencyInjection.cs:47),
  [`Program.cs`](../../src/Papyrus.Api.Admin/Program.cs:29), and the focused
  tenant/media/security tests.
- **O1-O3:** versioned DTO contracts, no-tracking projections, tenant/live
  public filtering, and bounded feed/sitemap source queries are implemented in
  [`AdminApiEndpoints`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:14),
  [`PublicContentQueryHandlers`](../../src/Papyrus.Application/Content/PublicContentQueries.cs:8),
  and [`TenantIsolationTests`](../../src/Papyrus.Tests/TenantIsolationTests.cs:35).
- **O5-O6:** projection SQL and PostgreSQL execution evidence show bounded,
  non-N+1 read paths, while all runtime images are multi-stage and non-root.
Evidence: [`projection-evidence.md`](../performance/projection-evidence.md:66),
  [`Dockerfile.admin`](../../Dockerfile.admin:20), and the CI/release workflows.

## Rationale and boundaries

This governance record is limited to release verification conditions. It does not:

- represent unavailable source audit evidence as present;
- waive the P0 production-blocker requirements for C1-C4;
- assert that any planned operational verification has passed; or
- authorize a production release by itself.

W6-W8, O4, and O7-O10 are `Mitigated; release gate`, not accepted residual
defects. The required deployment, monitoring, backup/restore, network,
performance, architecture, and publication-artifact evidence must be attached
to the release approval.

## Revisit conditions

This decision must be revisited when any of the following occurs:

1. The original audit report or evidence becomes available; it must then be
   attached to each applicable register row and the findings re-evaluated.
2. A public or production release is proposed; all applicable W/O verification
   gates must be completed and recorded before release approval.
3. New evidence changes the severity, owner, remediation, or acceptance
   criteria for any W/O finding.

## Approval

**Approved by:** Project owner  
**Date:** 2026-08-25
