# M12 — Navigation & Site Settings

## Objective
Make navigation and low-churn site configuration admin-editable.

## Background
Navigation and settings are currently hardcoded.

## Scope

### In scope
- Introduce NavigationItem and SiteSetting aggregates.
- Build Admin navigation management (add/remove/reorder, scoped per site).
- Build an Admin settings form.
- Update both sites' navigation to be data-driven.

### Out of scope
- Drag-and-drop reordering (ordinal field with up/down controls suffices for v1).

## Domain changes
- NavigationItem: label, target, owning site, order, visibility.
- SiteSetting: key-value store.

## API changes
- New Admin endpoint groups for navigation and settings.

## UI/Presentation changes
- Admin navigation and settings screens.
- Data-driven public navigation components.

## Non-functional requirements
- Changes must apply without redeploy.

## Risks & mitigations
- Easier to introduce a broken link once admin-editable.
- Mitigation: basic URL validation in the Admin form.

## Definition of Done
- [ ] An admin-added link appears live on the correct site.
- [ ] Reordering/removal reflects immediately.

## Test plan
- Integration tests for both adapters.
- End-to-end test verifying correct site scoping.
