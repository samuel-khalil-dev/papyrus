# ADR-0007: Pluggable AI provider architecture via resolver and factory

**Status:** Accepted

## Context
Hardcoding one AI vendor/model would require a redeploy to change providers, contradicting the admin-controls-everything principle, and would create vendor lock-in for a cost-sensitive, evolving capability.

## Decision
AI capabilities are accessed through an `IContentAssistant`/`ILlmClient` port. An `AiProviderProfile` aggregate holds admin-configured provider/model settings; an `IAiProviderResolver` resolves the active, connection-tested profile; an `LlmClientFactory` returns the matching adapter (OpenRouter, OpenAI, Anthropic, self-hosted). This mirrors the existing `IThemeResolver` pattern exactly.

## Consequences
Positive: provider/model switchable from Admin without a redeploy; no vendor lock-in; consistent with an already-established resolver pattern. Negative: an extra layer of indirection versus calling one SDK directly; accepted because the configurability was an explicit requirement, not a hypothetical.
