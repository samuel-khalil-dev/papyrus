# M28 — Newsletter & Syndication

## Objective
Extend reach beyond direct visits via email subscription and automated cross-posting.

## Background
Content currently reaches readers only through direct visits, search, and RSS/Atom.

## Scope

### In scope
- Introduce NewsletterSubscriber with double opt-in, using the M14 shared email infrastructure.
- Send email notification to subscribers on new-post publish.
- Implement automated cross-posting to defined external targets.
- Support hard deletion of a subscriber's data on an erasure request.

### Out of scope
- A full marketing-email platform.

## Domain changes
- NewsletterSubscriber: email, confirmed flag, subscribed date, unsubscribe token; supports hard delete.

## API changes
- New endpoints for subscribe/confirm/unsubscribe/erase.

## UI/Presentation changes
- Subscription form on the Blog.
- Confirmation and unsubscribe landing pages.

## Non-functional requirements
- Subscriber emails must never be exposed publicly.

## Risks & mitigations
- Unconfirmed emails could be treated as active subscribers.
- Mitigation: enforce double opt-in before treating a subscriber as active.

## Definition of Done
- [ ] A visitor can subscribe, confirm, unsubscribe, and request erasure without an account.
- [ ] Publishing triggers notification to confirmed subscribers only.

## Test plan
- Integration test for the opt-in/confirm/unsubscribe/erase flow.
- Integration test verifying notification targets confirmed subscribers only.
