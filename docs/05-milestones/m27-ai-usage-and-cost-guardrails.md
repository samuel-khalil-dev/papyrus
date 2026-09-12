# M27 — AI Usage & Cost Guardrails

## Objective
Track AI usage cost and enforce a configurable budget cap, given that M25/M26 introduce metered spend for the first time on this platform.

## Background
Nothing currently tracks token usage or cost, and an unexpected usage spike could produce a surprise bill with no warning.

## Scope

### In scope
- Introduce AiUsageRecord, populated per AI-assist request with tokens consumed and estimated cost.
- Build an Admin usage view showing running cost by day/month and by capability.
- Implement a configurable monthly budget cap that gracefully disables AI-assist actions (not the platform) once reached.
- Ensure usage is recorded even for requests that stream and are abandoned mid-response, not only fully completed ones.

### Out of scope
- Per-user usage attribution (single-admin platform; not applicable).

## Domain changes
- AiUsageRecord aggregate.

## API changes
- New Admin endpoint for usage reporting; the existing content-assist endpoints record usage as part of each call.

## UI/Presentation changes
- Admin usage dashboard with the configurable budget-cap setting.

## Non-functional requirements
- Usage recording must not be bypassable by a request that errors or disconnects before completing.

## Risks & mitigations
- A request that streams partially and disconnects could be recorded as zero usage despite consuming tokens.
- Mitigation: usage is recorded based on the provider's actual reported token count for the call, captured before or during streaming, not only on full completion.

## Definition of Done
- [ ] Every AI-assist request produces a corresponding AiUsageRecord.
- [ ] The Admin usage view accurately reflects cumulative cost.
- [ ] AI-assist actions disable gracefully once the configured budget cap is reached, with a clear message, not a silent failure.

## Test plan
- Integration test verifying usage is recorded even for an aborted streaming request.
- Test verifying the budget cap disables actions correctly and resets appropriately.
