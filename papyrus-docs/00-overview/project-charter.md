# Project Charter

## Purpose
Papyrus gives its owner a permanent, self-controlled platform for professional presentation (Portfolio) and technical writing (Blog), administered through a single control plane (Admin), under a Pharaonic-inspired visual identity.

## Objectives
1. Present a credible, modern professional profile supporting career development in software development, DevOps, and IT support roles.
2. Publish long-form technical writing on an independent, self-owned platform.
3. Centralize all public-facing configuration in one authenticated control plane.
4. Maintain the platform indefinitely as an evolving product.
5. Serve as a visible engineering credibility artifact in its own right — its architecture and discipline are part of what it communicates.

## Non-objectives
- Multi-tenant SaaS operation (multi-tenancy-ready entities exist for architectural safety only).
- Independent service extraction or a distributed-systems architecture.
- Team-scale process overhead.

## Stakeholders
- Owner/maintainer: sole developer, product owner, and Admin user.
- Site visitors: recruiters, peers, readers.

## Success criteria
- Portfolio functions as a complete landing page at the root domain; Blog at its subdomain.
- Every piece of public content and presentation is editable from Admin without a deployment.
- The platform's own infrastructure is monitored, backed up, and resilient enough to be trusted as a long-term home for the owner's professional record.
- The platform continues to receive new milestones after initial launch.

## Reference documentation
- [changelog.md](changelog.md) — recorded release and phase history of the implemented system.
- [feature-matrix.md](feature-matrix.md) — Implemented / Deferred / Verification-pending status of the current baseline.
- [limitations.md](limitations.md) — deliberate boundaries and operational work still required.
