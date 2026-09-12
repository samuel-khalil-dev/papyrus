# M17 — Content Workflow & Versioning

## Objective
Introduce a proper content lifecycle and change history.

## Background
Posts/projects have no distinct lifecycle state or version history.

## Scope

### In scope
- Add draft/scheduled/published states.
- Introduce ContentRevision to snapshot every save, including an AiAssisted flag.
- Build scheduled-publish execution.
- Build an Admin revision history view with rollback.

### Out of scope
- Real-time collaborative editing.

## Domain changes
- Extend BlogPost/Project with status and publish-at fields.
- ContentRevision: discriminator, content id, snapshot, timestamp, author, AiAssisted flag.

## API changes
- New endpoints for revision listing/retrieval/rollback.

## UI/Presentation changes
- Admin content list shows status.
- Admin revision history panel.

## Non-functional requirements
- Scheduled publishing must execute reliably without the admin being online.

## Risks & mitigations
- A silent scheduling failure leaves content unpublished.
- Mitigation: logged execution/failure plus a manual publish override always available.

## Definition of Done
- [ ] Draft content is never publicly visible.
- [ ] Scheduled content publishes automatically on time.
- [ ] A prior revision can be viewed and restored.

## Test plan
- Unit tests for status transitions.
- Integration test for scheduled-publish execution.
- End-to-end test for view/rollback.
