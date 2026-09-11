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
