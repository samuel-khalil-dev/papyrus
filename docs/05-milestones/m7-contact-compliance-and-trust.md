# M7 — Contact, Compliance & Trust

## Objective
Provide a functioning contact channel and satisfy EU data-protection and accessibility expectations.

## Background
No contact mechanism, consent notice, privacy documentation, or accessibility statement exists today.

## Scope

### In scope
- Contact form with Cloudflare Turnstile spam protection, delivered by email.
- Cookie/consent notice and a published privacy policy.
- A published Accessibility Statement page (standard targeted, known limitations, reporting contact).
- Privacy-respecting, self-hosted analytics.
- Branded 404/500 error pages.
- Support for a data-subject erasure request against any stored visitor data.

### Out of scope
- Full CRM-style message management.

## Domain changes
- An optional lightweight ContactMessage record if messages are stored rather than only emailed; supports hard deletion on request.

## API changes
- New rate-limited, Turnstile-protected contact submission endpoint; an admin-triggered erasure endpoint for stored visitor data.

## UI/Presentation changes
- Contact section on Portfolio.
- Cookie consent banner.
- Privacy policy and accessibility statement pages.
- Branded error pages.

## Non-functional requirements
- Consent requested before any non-essential cookie is set.

## Risks & mitigations
- Spam bypassing protections.
- Mitigation: Cloudflare Turnstile plus endpoint-level rate limiting.

## Definition of Done
- [ ] A test message is received by email.
- [ ] Consent notice, privacy policy, and accessibility statement all exist and are linked from the footer.
- [ ] 404/500 pages match the brand identity.
- [ ] An admin can erase a stored visitor's data on request.

## Test plan
- End-to-end test submitting the form and verifying delivery.
- Automated rate-limit test.
- Manual verification of the erasure action's completeness.
