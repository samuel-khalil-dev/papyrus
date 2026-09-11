# Information Architecture

## Site map

### Portfolio (root domain)
- `/` - single long-scroll landing page: Hero, Featured work, About, Selected writing, Contact/links
- `/projects/{slug}` - case-study project page
- `/now` - current focus page
- `/cv` - generated resume/CV view and download
- Availability status indicator on the Hero (open to work / not looking)

### Blog (subdomain)
- `/` - post index, filterable by category/tag
- `/posts/{slug}` - post detail
- `/categories/{slug}`, `/tags/{slug}` - archive views
- `/feed.xml` - syndication feed

### Admin (authenticated; two-factor protected once M3 ships)
- `/` - dashboard
- `/content/posts`, `/content/projects` - CRUD with localization and AI-assist tabs
- `/site/theme`, `/site/localization`, `/site/navigation`, `/site/settings`
- `/site/ai-providers` - AI provider profile management
- `/media` - media library
- `/security` - two-factor (M3) and audit log (M30) views

## Navigation and indexing rules
- Portfolio and Blog never share a navigation instance.
- The Admin subdomain is excluded from search indexing entirely: `robots.txt` disallow and a page-level `noindex` header/meta tag on every Admin route, with no exceptions.
- Locale-prefixed routes apply identically across both public sites; right-to-left locales additionally mirror layout direction site-wide, not just text alignment.
