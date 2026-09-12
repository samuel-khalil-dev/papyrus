# Domain Model Reference

## Content aggregates (Foundation)
BlogPost/BlogPostTag, Project/ProjectTag, ContentCategory/ContentTag - as previously specified.

## Presentation & configuration aggregates
- **SiteTheme** (M10): token set, active flag, fallback-safe default, resolved via `IThemeResolver`.
- **Locale** (M11): code, display name, enabled flag, default flag, **text direction (LTR/RTL)**.
- **ContentTranslation** (M11): content discriminator, content id, locale id, translated fields, fallback resolution.
- **NavigationItem** (M12): label, target, owning site, order, visibility.
- **SiteSetting** (M12): key-value store, including the availability-status flag (M21).

## Content lifecycle
- **ContentRevision** (M17): content discriminator, content id, snapshot, timestamp, author, and an `AiAssisted` flag recording whether AI drafting contributed to the saved version.

## Media
- **MediaAsset** (M19): file reference, responsive variants, dimensions, required alt text, optimization metadata.

## Credibility aggregates
SkillEntry, TimelineEntry, Testimonial, Certification (M20).

## Growth aggregates
- **NewsletterSubscriber** (M28): email, confirmed flag, subscribed date, unsubscribe token. Supports hard deletion on a data-subject erasure request.
- **FeatureFlag** (M31): key, enabled flag, description, scope.

## AI platform aggregates
- **AiProviderProfile** (M25): name, provider type (OpenRouter/OpenAI/Anthropic/self-hosted), base URL, encrypted API key, active flag.
- **AiCapabilityModelOverride** (M25): child of `AiProviderProfile` - capability, model identifier, temperature, max tokens.
- **AiUsageRecord** (M27): timestamp, capability, profile reference, tokens consumed, estimated cost - used to enforce the configurable monthly budget cap.

## Security note
`AiProviderProfile.ApiKey` is encrypted at rest via the ASP.NET Core Data Protection API and is explicitly excluded from the Admin audit log (M30) and the self-service content export (M30) - it must never appear in either output, masked or otherwise, beyond a display-only masked form in the Admin UI itself.
