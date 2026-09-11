# M11 — Localization Foundation

## Objective
Enable content localization with right-to-left layout support built in from the start, administered from Admin.

## Background
No localization capability exists today. Arabic is an intended future locale, so text direction must be a first-class part of the Locale model, not a later addition.

## Scope

### In scope
- Introduce Locale (including a text-direction attribute) and ContentTranslation aggregates.
- Build an Admin translation interface per content item.
- Implement locale-aware routing with fallback to the default locale.
- Apply the root dir attribute from the resolved locale, server-side.
- Ship Romanian as the first complete additional translation.
- Verify shared layout components render correctly under a simulated RTL locale, even before Arabic content exists.

### Out of scope
- Actual Arabic content/translation (a later, separate content milestone once this foundation and design-system RTL work are both proven).

## Domain changes
- Locale: code, display name, enabled, default flag, text direction.
- ContentTranslation: content discriminator, content id, locale id, translated fields, fallback logic.

## API changes
- New Admin endpoint groups for locale and translation management.

## UI/Presentation changes
- Admin language management screen.
- Per-content translation tab.
- Public language switcher and locale-prefixed, direction-aware routing.

## Non-functional requirements
- Fallback must be deterministic; layout must mirror correctly under a simulated RTL test locale.

## Risks & mitigations
- Building RTL groundwork without ever testing it under a real RTL locale risks it being broken when Arabic actually ships.
- Mitigation: a synthetic RTL test locale (not user-facing) is used to exercise the layout mirroring in this milestone's own test plan.

## Definition of Done
- [ ] Romanian round-trips end-to-end.
- [ ] Untranslated content falls back cleanly.
- [ ] A synthetic RTL test locale renders all shared components correctly mirrored.

## Test plan
- Unit tests for fallback logic.
- Integration test for the adapter.
- Visual/automated check of shared components under a synthetic RTL locale.
