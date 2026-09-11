# M25 — AI Provider Configuration

## Objective
Make the AI provider and model fully configurable from Admin, with no active profile permitted to go live without a verified working connection.

## Background
AI-assisted features require a real LLM provider, but hardcoding one vendor/model would require a redeploy to change and contradicts the admin-controls-everything principle.

## Scope

### In scope
- Introduce the AiProviderProfile aggregate (name, provider type, base URL, encrypted API key, active flag) and its child AiCapabilityModelOverride collection.
- Implement IAiProviderResolver returning the active, connection-verified profile, with graceful feature-disable if none is valid.
- Implement an LlmClientFactory producing the correct adapter (OpenRouter, OpenAI, Anthropic, self-hosted) per the active profile.
- Build the Admin provider management screen, including a mandatory 'Test connection' action that must succeed before a profile can be marked active.
- Amend M30's audit log and export scope to explicitly exclude ApiKey.

### Out of scope
- The actual content-authoring features that consume this layer (M26).

## Domain changes
- AiProviderProfile and AiCapabilityModelOverride aggregates.

## API changes
- New Admin endpoint group for provider CRUD and the test-connection action.

## UI/Presentation changes
- Admin AI provider management screen with masked key display.

## Non-functional requirements
- A profile cannot become active without a successful test-connection pass in the same action.

## Risks & mitigations
- An unverified or broken profile going live would silently break every AI-assist feature at once.
- Mitigation: activation is gated on a successful test-connection call, not just a save.

## Definition of Done
- [ ] A profile cannot be set active without passing test-connection first.
- [ ] The API key is encrypted at rest and never appears unmasked outside initial entry.
- [ ] The key is confirmed absent from audit-log and export output.

## Test plan
- Unit test for IAiProviderResolver's fallback behavior with zero valid profiles.
- Integration test for the activation gate rejecting an unverified profile.
- Automated check confirming ApiKey is absent from audit-log and export output.
