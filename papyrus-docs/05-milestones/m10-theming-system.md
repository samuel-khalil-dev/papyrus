# M10 — Theming System

## Objective
Move theme control from fixed code defaults into an admin-editable, safely-resolved configuration.

## Background
M8 ships a fixed default theme in code; this makes it data-driven per the admin-controls-everything principle.

## Scope

### In scope
- Introduce the SiteTheme aggregate (full vertical slice).
- Implement IThemeResolver with a hardcoded safe fallback.
- Build the Admin theme editor within a bounded palette.
- Render tokens as server-side CSS custom properties.

### Out of scope
- Visitor-facing theme toggle (M23).

## Domain changes
- SiteTheme aggregate: entity, configuration, migration, ports, EfThemeStore adapter.

## API changes
- New Admin API endpoint group for theme read/update.

## UI/Presentation changes
- Admin theme editor screen.
- Public layouts consume resolved tokens server-side.

## Non-functional requirements
- Theme resolution must never fail to produce a renderable page.

## Risks & mitigations
- A missing/invalid theme record could break rendering.
- Mitigation: IThemeResolver enforces an always-valid fallback, tested explicitly.

## Definition of Done
- [ ] Changing the accent value in Admin reflects on both public sites without redeploy.
- [ ] A missing/corrupted record falls back safely.

## Test plan
- Unit test for IThemeResolver covering valid/missing/invalid cases.
- Integration test for the adapter.
- End-to-end test for propagation to both sites.
