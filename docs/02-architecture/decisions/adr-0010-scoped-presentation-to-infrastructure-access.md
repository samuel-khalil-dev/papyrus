# ADR-0010: Scoped Presentation-to-Infrastructure access

**Status:** Accepted

## Context
ADR-0002 inverted the `Application → Infrastructure` dependency but said nothing about Presentation. The architecture overview stated that a Presentation project may reference Infrastructure only inside its composition root, without defining where a composition root ends, so the rule was unjudgeable and nothing enforced it.

The M0 audit established the actual shape. In `Papyrus.Api.Admin`, every Infrastructure reference sits in one of three places: the entry point, the feature-scoped endpoint layer under `Endpoints/`, and the Admin support layer under `AdminUi/`. Nothing is scattered into unrelated Presentation code. The Admin endpoint handlers and `AdminContentSupport` inject `PapyrusDbContext` directly, so those reads, the tag-link writes, and the media-reference check bypass the Application ports. In the read-only public hosts, the only Infrastructure data-access reference is `PapyrusDbContext`, injected into the `/feed.xml` and `/sitemap.xml` handler registrations in the entry point; page content goes through Application. No public host references an `Ef*Store` adapter. `ITenantContext` lives in `Papyrus.Infrastructure.MultiTenancy` and is consumed by Presentation.

Extraction behind the existing ports was evaluated and cannot close the gap as the ports stand. `IContentQueryStore` exposes only unpaged `List<ContentListItem>` reads and `ITaxonomyCommandStore` only save operations, while the endpoints need paged projections with category, tag, and post includes, tag-link mutation, and a media-reference existence check. Closing it by extraction means a materially new port surface, which is a refactor sized for its own milestone and reopens the shape ADR-0002 set.

## Decision
- A composition root is an entry point plus the code that exists to wire and dispatch requests. Presentation code outside it must not reference Infrastructure.
- The read-only public hosts (`Papyrus.Web.Blog`, `Papyrus.Web.Portfolio`) and `Papyrus.Web.Shared` may reference Infrastructure only from their entry point, and may not reference the `Ef*Store` adapters at all. A `PapyrusDbContext` parameter on the feed and sitemap handler registrations is an accepted, recorded exception: it is read-only and is the current state of a bounded public read.
- For `Papyrus.Api.Admin`, the composition root is the entry point plus `Endpoints/` and `AdminUi/`. Presentation code added elsewhere in that project must not reference Infrastructure. `ITenantContext` is exempt wherever it is consumed, because it is a cross-cutting request-scoped contract rather than a data-access type.
- The boundary is enforced by NetArchTest and reflection rules in `LayerDependencyTests` as a ratchet. The shape this audit accepted is the baseline; widening it requires a superseding ADR, not a test edit.
- Extracting the Admin read paths and the feed and sitemap queries behind Application ports is follow-on work. It is not a gate on the guardrail milestone.

## Consequences
Positive: a public read-only host cannot gain write capability, which is the risk that matters; new Admin Presentation code cannot reach `PapyrusDbContext` without a deliberate decision; the boundary is machine-checked rather than asserted, and was verified to fail on an introduced violation. Negative: Admin still reads and writes through `PapyrusDbContext` directly, so the Application ports understate the real data-access surface and ADR-0002 is not yet true end to end; the public hosts keep a recorded `PapyrusDbContext` dependency. Accepted deliberately: both are narrower than a full extraction and are bounded by a machine-checked boundary in the meantime.
