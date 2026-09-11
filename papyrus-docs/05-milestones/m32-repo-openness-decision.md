# M32 — Repo Openness Decision

## Objective
Deliberately decide, and act on, whether the Papyrus repository (or a sanitized subset) becomes public, turning the engineering work itself into a visible credibility artifact.

## Background
The repository is currently private, but a substantial part of this roadmap's value - the architecture discipline, CI/CD rigor, and ADRs - is exactly what a hiring manager would want to see. This should only proceed once secret-handling is proven solid.

## Scope

### In scope
- Audit git history and current configuration for any secret that would be exposed by making the repository public.
- Decide between full repository openness versus a sanitized public mirror that omits operational specifics.
- Execute the chosen option.
- Update documentation to reflect the repository's public status and any redacted operational detail.

### Out of scope
- Open-sourcing under a contribution-accepting model (this remains a personal, single-maintainer project regardless of visibility).

## Domain changes
- No new aggregates; this is a repository and documentation action.

## API changes
- No API changes.

## UI/Presentation changes
- No application UI changes.

## Non-functional requirements
- This milestone must not proceed until M25's secret-handling (encryption, exclusion from audit/export) and a full git-history secret scan both pass.

## Risks & mitigations
- A historical commit could contain a secret that predates the current secret-handling standard.
- Mitigation: a full git-history secret scan (not just current-state gitleaks) is a required precondition, with history rewritten or the repository kept as a fresh public mirror if any historical secret is found.

## Definition of Done
- [ ] A full secret-history scan has passed with no findings.
- [ ] The repository (or its public mirror) is live and reflects the decision made.
- [ ] Documentation reflects the current visibility status.

## Test plan
- Full git-history secret scan, not limited to the current HEAD.
- Manual review of the decision's execution before making anything public.
