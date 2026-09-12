# M30 — Platform Observability & Trust

## Objective
Make the platform's own operational health and change history visible.

## Background
Existing health checks and audit-capable interceptors are not currently surfaced to the public or the admin.

## Scope

### In scope
- Build a public status/uptime page from existing health-check data, scoped to coarse-grained signals only.
- Surface an Admin audit log from existing change-tracking interceptor data, explicitly excluding AiProviderProfile.ApiKey and any other secret field.
- Implement self-service content export for backup/portability, with the same secret-exclusion guarantee.

### Out of scope
- Building new health-check infrastructure.

## Domain changes
- No new aggregates for the status page; an audit-log read-projection with an explicit secret-field exclusion list.

## API changes
- New public status endpoint.
- New Admin endpoints for audit log retrieval and content export.

## UI/Presentation changes
- Public status page.
- Admin audit log view.
- Admin content export action.

## Non-functional requirements
- The status page must expose only coarse-grained availability signals.

## Risks & mitigations
- Exposing detailed operational data publicly, or leaking a secret through the audit log/export, is a real security concern.
- Mitigation: a named, reviewed exclusion list covering every secret-holding field, tested explicitly.

## Definition of Done
- [ ] The status page reflects service health without sensitive detail.
- [ ] Admin can view an audit log and export content.
- [ ] Automated verification that no secret field appears in either output.

## Test plan
- Automated test verifying status reflects simulated healthy/unhealthy states.
- Automated test confirming secret exclusion from audit log and export.
