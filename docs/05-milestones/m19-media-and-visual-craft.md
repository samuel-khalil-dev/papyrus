# M19 — Media & Visual Craft

## Objective
Formalize media handling for responsive, optimized imagery.

## Background
Media is handled as basic uploads without responsive variants or gallery support.

## Scope

### In scope
- Introduce MediaAsset with responsive variant generation.
- Build a project screenshot gallery with lightbox.
- Support embedded video.
- Enforce mandatory alt text at the schema level.

### Out of scope
- A full digital-asset-management system.

## Domain changes
- MediaAsset: original reference, variants, dimensions, alt text (required), optimization metadata.

## API changes
- New endpoints for upload, variant status, metadata management.

## UI/Presentation changes
- Admin media library updated.
- Public gallery/lightbox component.
- Video embed component.

## Non-functional requirements
- Responsive variants must be served per device.

## Risks & mitigations
- Variant generation could be resource-intensive at upload time.
- Mitigation: process variants asynchronously.

## Definition of Done
- [ ] Uploading without alt text is rejected.
- [ ] Gallery/lightbox works on desktop and mobile.

## Test plan
- Integration test for the adapter and variant pipeline.
- Automated check that alt text is enforced end to end.
