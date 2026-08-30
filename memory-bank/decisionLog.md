# Decision Log

## ADR 0001 — Modular monolith boundaries

Keep domain, application, contracts, infrastructure, and delivery concerns in one deployable modular monolith, preserving clear dependency boundaries without premature service extraction.

## ADR 0002 — Tenant enforcement

Use `TenantContext`, EF global filters, save-boundary enforcement, composite tenant relationships, and tenant-partitioned media for application-level tenant isolation.

## ADR 0004 — REST and contracts

Expose explicit versioned REST contracts under `/api/v1`, with DTO projections and pagination envelopes. Keep `/api` as excluded legacy compatibility surface. The committed OpenAPI 3.1 document is the frontend generation source.

## ADR 0005 — Migration ownership

`Papyrus.Api.Admin` is the sole startup migrator and seed owner. Infrastructure supplies migration capability; public applications never migrate. Applied migration files remain immutable.

## ADR 0006 — Retire admin Razor pages

Retire legacy admin Razor pages, browser sessions, cookie authentication, login throttling, and Razor antiforgery wiring. The React SPA at `/admin` and header API-key authentication are the sole admin experience.

## ADR 0007 — Decline PostgreSQL RLS

PostgreSQL row-level security is declined. EF filters, save-boundary enforcement, composite relationships, and tenant-scoped media remain the sole enforcement layer; no RLS policy or migration is introduced.

## Managed API-key rotation

Use reloadable `AdminApi:ApiKeys` configuration with constant-time comparison across candidates. Retain `AdminApi:ApiKey` as a compatibility alias. Rotation is add, overlap, client migration, then remove; secrets remain deployment-owned.

## Audit code-fix decisions

- **G1:** Corrected Phase 6 roadmap status in `docs/project/roadmap.md`.
- **G2:** Removed confirmed-empty retired Admin Razor Pages shells under `src/Papyrus.Api.Admin/Pages/`.
- **G3:** Removed confirmed-empty obsolete Admin-local Contracts under `src/Papyrus.Api.Admin/Contracts/`; REST contracts remain in `Papyrus.Contracts`.
- **G4:** Regenerated `src/Papyrus.Admin.UI/src/api/schema.d.ts` from `docs/api/openapi.json` and changed `src/Papyrus.Admin.UI/package.json` `generate-api` to target port 5205.
- **G10:** Pinned CI Trivy in `.github/workflows/ci.yml` to the immutable commit for `aquasecurity/trivy-action` v0.33.1.

# Archify documentation decision

- Use Archify `architecture` specifications with `quality_profile: showcase`, dark/light-capable standalone HTML, and inline SVG for the backend boundary map and React Admin SPA component tree.

## Phase 3 frontend design decisions

- **Admin Royal Pharaonic system:** Keep the Admin SPA on shadcn semantic variable names while adding canonical surface, hover, glow, glass, and status aliases. Use a `:root` light palette with Deep Gold and a `.dark` Obsidian palette with Royal Gold; synchronize both `.dark` and `data-theme` from the existing theme provider. Preserve zero-radius primitives and the currentColor Papyrus logo.

- **React compiler:** Enable `babel-plugin-react-compiler` through Vite's React plugin. Manual memoization is not retained where it only targets render optimization; debounce state/effect remains because it implements observable time-based behavior.

- **Public visual system:** Keep the shared Razor public hosts on a dark Royal Pharaonic palette: Obsidian `#0F0F13`, Basalt `#1A1A1F`, Royal Gold `#D4AF37`, Sand `#E5D3B3`, and Alabaster `#EDEDED`. Use Cinzel for display typography, Inter for body copy, JetBrains Mono for code, and zero-radius surfaces.
- **Public light theme and SVG rendering:** Adopt a dedicated warm light palette for the shared Blog/Portfolio hosts rather than reusing dark `:root` tokens: background `#F5F1E8`, elevated surface `#EDE7D9`, surface `#FFFDF7`, hover surface `#F3EBDD`, text `#1A1A1F`, secondary text `#5A4520`, muted text `#6F6658`, accent `#9A7410`, and accent-light `#7A5B0B`, supplemented by semantic status, orb, and grid tokens. Use SVG `currentColor` for the Papyrus logo so CSS theme tokens control its accent color, and use multiply blending for the light-mode background texture.
- **Shared public finishing layer:** Keep the gallery register architectural with a 720px repeating papyrus texture, subtle gold atmosphere only in the dark public theme, zero-radius controls/cards/tags, canonical focus rings, and restrained hero load reveals with reduced-motion handling. Preserve the existing shared Razor layout and theme bootstrap because it already supplies the required `data-theme`, fonts, background host, and theme-switch load order.
- **Primitive policy:** Use Radix for Dialog, AlertDialog, Tabs, and DropdownMenu where accessibility, focus management, keyboard navigation, or state behavior adds value. Retain the native Select because the current form contract requires `multiple` selection and native mobile controls. Provide standard React Hook Form-backed Form primitives.
- **Tokens:** Use semantic shadcn CSS variables (`background`, `foreground`, `border`, `input`, `ring`, `muted`) for global fields and controls rather than raw slate or accent colors, preserving the existing slate visual language and dark mode.

## Phase 7.2 showcase asset decision

- Store public showcase captures in `docs/assets/` and use only local seeded demo
  data. Exclude credentials, connection strings, private URLs, personal data,
  and operational environment details; the Aspire capture masks its URL/source
  area. Prefer static PNG captures because the five requested surfaces do not
  require a GIF to communicate their documented state.
