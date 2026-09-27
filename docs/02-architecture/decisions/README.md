# Architecture Decision Records

The authoritative record of durable architectural decisions for Papyrus. Each ADR
is short, numbered, immutable once accepted, and states a Context, a Decision,
and Consequences. They are **not** implementation plans and they are **not**
status trackers — a decision that is later reversed is superseded by a new ADR,
not edited in place.

All ten decisions below are **Accepted**. The ADR files do not carry decision
dates, so none are listed here rather than inferred.

| ADR | Decision | One-line summary |
| --- | --- | --- |
| [ADR-0001](adr-0001-adopt-a-modular-monolith-over-microservices.md) | Adopt a modular monolith over microservices | One deployable .NET solution, internally split into bounded Clean Architecture modules enforced by fitness tests. |
| [ADR-0002](adr-0002-application-owned-ports-with-infrastructure-adapters.md) | Application-owned ports with Infrastructure adapters | Application defines per-aggregate command/query store ports; Infrastructure implements them as EF Core `Ef*Store` adapters. |
| [ADR-0003](adr-0003-react-admin-razor-public.md) | React for Admin, server-rendered Razor Pages for Portfolio and Blog | Each surface gets the technology its requirements justify; no shared component library because the surfaces are unrelated. |
| [ADR-0004](adr-0004-retain-multitenant-entities.md) | Retain multi-tenancy-ready entity contracts in a single-tenant deployment | `IEntity`/`ITenantEntity` everywhere; **PostgreSQL RLS is evaluated and declined** — application-level filtering is the sole enforcement layer. |
| [ADR-0005](adr-0005-defer-or-reject-independent-service-extraction.md) | Defer or reject independent service extraction | The modular monolith stands; extraction would require measured operational data, verified by the M5 monitoring work. |
| [ADR-0006](adr-0006-openapi-contract-first-development-for-the-admin-api.md) | OpenAPI contract-first development for the Admin API | The Admin API publishes an OpenAPI document diff-checked against a live capture in CI; the Admin UI client is generated from it. |
| [ADR-0007](adr-0007-pluggable-ai-provider-architecture-via-resolver-and-factory.md) | Pluggable AI provider architecture via resolver and factory | An `IContentAssistant`/`ILlmClient` port plus an admin-configured `AiProviderProfile` makes the provider switchable without a redeploy, mirroring `IThemeResolver`. |
| [ADR-0008](adr-0008-rtl-layout-from-inception.md) | Include right-to-left layout support in the localization foundation from inception | `Locale` carries a text-direction attribute and shared components use logical CSS properties from the first line, so RTL is a content exercise, not a redesign. |
| [ADR-0009](adr-0009-reverse-proxy-and-edge-layer-caddy-behind-cloudflare.md) | Reverse proxy and edge layer: Caddy behind Cloudflare | Caddy terminates TLS and routes by host; Cloudflare fronts it for CDN caching, DDoS protection, and Turnstile bot mitigation. |
| [ADR-0010](adr-0010-scoped-presentation-to-infrastructure-access.md) | Scoped Presentation-to-Infrastructure access | A composition root is an entry point plus its request-dispatch code; Presentation code outside it must not reach Infrastructure, and the accepted current shape is a machine-checked ratchet. |

## Frequently cited

| Decision | Cited from |
| --- | --- |
| [ADR-0001](adr-0001-adopt-a-modular-monolith-over-microservices.md) | [Architecture overview](../architecture-overview.md), [release gates](../../04-engineering-standards/release-gates.md), [threat register](../../04-engineering-standards/threat-register.md) |
| [ADR-0002](adr-0002-application-owned-ports-with-infrastructure-adapters.md) | [Release gates](../../04-engineering-standards/release-gates.md), [roadmap](../../Papyrus_Roadmap.md) |
| [ADR-0004](adr-0004-retain-multitenant-entities.md) | [Security baseline](../../04-engineering-standards/security-baseline.md), [agent review playbook](../../07-governance/agent-review-playbook.md), [C4 diagrams](../c4-diagrams.md), [roadmap](../../Papyrus_Roadmap.md) |
| [ADR-0009](adr-0009-reverse-proxy-and-edge-layer-caddy-behind-cloudflare.md) | [Architecture overview](../architecture-overview.md) |
| [ADR-0010](adr-0010-scoped-presentation-to-infrastructure-access.md) | [Architecture overview](../architecture-overview.md), [M0 milestone](../../05-milestones/m0-architectural-guardrails.md), [agent review playbook](../../07-governance/agent-review-playbook.md), `AGENTS.md` |

## How to change a decision

1. Do not edit an accepted ADR. Write a new numbered ADR that supersedes it and
   states the reason.
2. If the decision is architectural and durable, it belongs here. If it is an
   implementation plan, a release gate, or a milestone, it belongs in
   [`04-engineering-standards/`](../../04-engineering-standards/) or
   [`05-milestones/`](../../05-milestones/).
3. Reviewers treat accepted ADRs as binding. Proposing to violate one requires an
   ADR change, not a silent code change — see the
   [agent review playbook](../../07-governance/agent-review-playbook.md).
