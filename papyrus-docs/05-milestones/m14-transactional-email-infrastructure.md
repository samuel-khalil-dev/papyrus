# M14 — Transactional Email Infrastructure

## Objective
Provide a real, deliverable email foundation for the contact form (M7) and newsletter (M28), which otherwise assume delivery works without specifying how.

## Background
Neither M7 nor M28 specifies an actual email provider or domain-authentication setup.

## Scope

### In scope
- Select and configure a transactional email provider (or self-hosted equivalent).
- Configure SPF, DKIM, and DMARC records for the sending domain.
- Provide a shared email-sending port/adapter reused by both the contact form and newsletter features.

### Out of scope
- Marketing-email features beyond transactional/notification delivery.

## Domain changes
- No new aggregates; a shared `IEmailSender` port and adapter.

## API changes
- No new public API changes; consumed internally by M7 and M28.

## UI/Presentation changes
- No UI changes.

## Non-functional requirements
- Deliverability must be verified, not assumed - a message landing in spam is effectively a failed feature.

## Risks & mitigations
- Poor domain reputation or missing authentication records could cause messages to land in spam.
- Mitigation: SPF/DKIM/DMARC configured and verified with a deliverability test before this milestone is considered done.

## Definition of Done
- [ ] A test message sent through the shared email port is delivered to a real inbox, not spam.
- [ ] SPF/DKIM/DMARC all pass verification for the sending domain.

## Test plan
- Deliverability test against at least one major email provider.
- Integration test for the shared email-sending port.
