# M26 — AI-Assisted Authoring

## Objective
Add editor-integrated AI actions that accelerate drafting without ever auto-publishing generated text.

## Background
Content is currently authored entirely manually in the Tiptap editor with no assistance for outlining, rewriting, summarizing, or translation drafting.

## Scope

### In scope
- Implement IContentAssistant actions: generate outline, continue writing, rewrite/improve selection, summarize to excerpt, generate SEO meta description, suggest tags, draft translation (feeding into M11's translation tab).
- Build a custom Tiptap toolbar (not a paid vendor AI extension) calling a streaming Admin API endpoint.
- Every action inserts editable, reviewable text; no action publishes or saves content directly.
- Flag ContentRevision entries as AiAssisted when AI-generated text contributed to a saved version.

### Out of scope
- Fully autonomous post generation from a prompt alone (explicitly rejected to protect authorial voice and credibility).

## Domain changes
- No new aggregates; consumes M25's provider layer and M17's ContentRevision.AiAssisted flag.

## API changes
- New streaming (SSE) Admin API endpoint group for content-assist actions.

## UI/Presentation changes
- Custom AI toolbar integrated into the existing Tiptap editor in Admin.UI.

## Non-functional requirements
- Streaming responses must render incrementally in the editor, not block on full completion.

## Risks & mitigations
- Undisclosed AI-generated career claims (in translation or rewriting) could introduce factual inaccuracy about credentials.
- Mitigation: all AI output is inserted as editable draft text requiring explicit admin review before save, with extra scrutiny called out for career-related content in the admin workflow guidance.

## Definition of Done
- [ ] All seven actions function against the active AI provider profile.
- [ ] No action saves or publishes content without an explicit admin save action.
- [ ] A saved revision correctly reflects the AiAssisted flag when applicable.

## Test plan
- End-to-end test for each action's streaming behavior.
- Test verifying no action can save content without an explicit user action.
- Verification that ContentRevision.AiAssisted is set correctly.
