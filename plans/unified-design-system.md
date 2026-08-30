# Papyrus Unified Design System — "Royal Pharaonic" Specification

**Status:** Planning / Approved-for-implementation
**Scope:** All three frontends — Admin SPA, Blog, Portfolio
**Owner:** Design System Working Group
**Version:** 1.0

---

## 1. Design Direction Statement

**The Papyrus brand is a modern archival luxury house — not a theme park.**

The unified visual language is built on a single, deliberate point of view: **"The Royal Archive."** Every surface reads as a curated, museum-grade record — obsidian and basalt stone, gilded accents, and the tactile grain of papyrus. The aesthetic is **refined editorial/archival luxury**: restrained, architectural, and precise. It is the opposite of generic SaaS or generic "tech startup" styling.

### Differentiation

- **Architectural, not soft.** Zero-radius corners everywhere. Surfaces are cut stone, not rounded plastic. This is the single most recognizable signature of the brand and is non-negotiable across all three apps.
- **Gilded restraint.** Royal Gold (`#D4AF37`) is used as a *signal*, never as a flood. It appears on active states, focus rings, key CTAs, and the logo — never as a background wash. When gold is a background, it is always paired with dark obsidian text for contrast.
- **Material honesty.** Obsidian (`#0F0F13`) and Basalt (`#1A1A1F`) are the dark foundation; Sand (`#E5D3B3`) and Alabaster (`#EDEDED`) are the light foundation. The papyrus texture is a *material memory* — a low-opacity grain that grounds every surface without distracting.
- **One language, two registers.** The Admin SPA is the *workshop* (denser, more data-dense, tighter type, more borders). The public sites are the *gallery* (more whitespace, larger display type, more dramatic hero). Both share the same tokens, the same zero-radius language, the same gold signal, and the same texture — so a user moving between them feels continuity, not a rebrand.

### Non-negotiable invariants

1. Zero-radius surfaces (`--radius: 0`) everywhere.
2. Royal Gold as the single accent signal.
3. Papyrus texture as a low-opacity tiled background (screen in dark, multiply in light).
4. `currentColor`-driven SVG logo (ornamental seal/medallion).
5. Full light + dark themes in **all three** apps.

---

## 2. Unified Color System

### 2.1 Canonical Brand Tokens (source of truth)

| Token | Hex | OKLCH | Role |
| --- | --- | --- | --- |
| Obsidian | `#0F0F13` | `oklch(0.138 0.012 285)` | Dark background / deepest surface |
| Basalt | `#1A1A1F` | `oklch(0.184 0.012 285)` | Dark elevated surface |
| Basalt Hover | `#24242B` | `oklch(0.20 0.014 285)` | Dark surface hover |
| Royal Gold | `#D4AF37` | `oklch(0.748 0.145 88)` | Primary accent (signal) |
| Sand | `#E5D3B3` | `oklch(0.85 0.058 83)` | Accent-light / secondary text on dark |
| Alabaster | `#EDEDED` | `oklch(0.94 0 0)` | Foreground text on dark |
| Parchment | `#F5F1E8` | `oklch(0.955 0.012 90)` | Light background |
| Parchment Elevated | `#EDE7D9` | `oklch(0.92 0.014 90)` | Light elevated surface |
| Parchment Surface | `#FFFDF7` | `oklch(0.99 0.006 90)` | Light card surface |
| Deep Gold (light accent) | `#9A7410` | `oklch(0.55 0.12 80)` | Light-theme accent (readable on parchment) |
| Deep Gold Text | `#5A4520` | `oklch(0.42 0.07 80)` | Light-theme secondary text |

> **Note on light-theme gold:** Royal Gold `#D4AF37` fails contrast on parchment. The light theme uses **Deep Gold** (`#9A7410`) for accent and text roles, while reserving Royal Gold for the logo and decorative glows. This preserves brand identity while meeting WCAG AA.

### 2.2 Canonical Semantic Tokens

These are the **canonical** semantic roles. Both the Admin (shadcn) and Public (papyrus.css) naming schemes map onto them (see §2.3).

| Semantic Role | Dark Value | Light Value |
| --- | --- | --- |
| `background` | Obsidian `#0F0F13` | Parchment `#F5F1E8` |
| `surface` (elevated) | Basalt `#1A1A1F` | Parchment Elevated `#EDE7D9` |
| `card` | Basalt `#1A1A1F` | Parchment Surface `#FFFDF7` |
| `surface-hover` | Basalt Hover `#24242B` | `#F3EBDD` |
| `border` | `oklch(0.39 0.07 88)` (gold-tinted) | `oklch(0.82 0.03 80)` |
| `input` | `oklch(0.32 0.045 88)` | `oklch(0.88 0.02 80)` |
| `text` (foreground) | Alabaster `#EDEDED` | Basalt `#1A1A1F` |
| `text-secondary` | Sand `#E5D3B3` | Deep Gold Text `#5A4520` |
| `text-muted` | `#A99D88` | `#6F6658` |
| `accent` (primary) | Royal Gold `#D4AF37` | Deep Gold `#9A7410` |
| `accent-light` | Sand `#E5D3B3` | Deep Gold Text `#5A4520` |
| `accent-foreground` | Obsidian `#0F0F13` | Parchment `#F5F1E8` |
| `accent-glow` | `rgba(212,175,55,0.18)` | `rgba(154,116,16,0.20)` |
| `ring` | Royal Gold `#D4AF37` | Deep Gold `#9A7410` |
| `glass` | `rgba(15,15,19,0.94)` | `rgba(245,241,232,0.94)` |
| `glass-border` | `rgba(212,175,55,0.28)` | `rgba(122,91,11,0.28)` |
| `success` | `#00b894` | `#16794F` |
| `warning` | `#fdcb6e` | `#8A5A00` |
| `danger` | `#e17055` | `#B33A2B` |
| `muted` (fill) | `oklch(0.225 0.016 285)` | `oklch(0.94 0.01 90)` |
| `muted-foreground` | `oklch(0.68 0.025 83)` | `oklch(0.45 0.02 80)` |

### 2.3 Naming Coherence — Mapping Admin ↔ Public ↔ Canonical

The Admin SPA uses shadcn OKLCH names (`--color-background/card/primary/...`). The public sites use `--color-bg/bg-elevated/accent/...`. To keep them coherent **without a forced rename**, we define an explicit mapping table. Both schemes are **aliases of the same canonical semantic roles**.

| Canonical Role | Admin (shadcn) | Public (papyrus.css) |
| --- | --- | --- |
| background | `--color-background` | `--color-bg` |
| surface (elevated) | `--color-surface` | `--color-bg-elevated` |
| card | `--color-card` | `--color-bg-surface` |
| surface-hover | *(add `--color-surface-hover`)* | `--color-bg-surface-hover` |
| border | `--color-border` | `--color-glass-border` (decorative) / `--color-border` (structural) |
| input | `--color-input` | *(add `--color-input`)* |
| text | `--color-foreground` | `--color-text` |
| text-secondary | `--color-secondary-foreground` | `--color-text-secondary` |
| text-muted | `--color-muted-foreground` | `--color-text-muted` |
| accent | `--color-primary` | `--color-accent` |
| accent-light | `--color-secondary` | `--color-accent-light` |
| accent-foreground | `--color-primary-foreground` | `--color-bg` (on gold) |
| accent-glow | *(add `--color-accent-glow`)* | `--color-accent-glow` |
| ring | `--color-ring` | *(add `--color-ring`)* |
| glass | *(add `--color-glass`)* | `--color-glass` |
| glass-border | *(add `--color-glass-border`)* | `--color-glass-border` |
| success | `--color-success` | `--color-success` |
| warning | `--color-warning` | `--color-warning` |
| danger | `--color-destructive` | `--color-danger` |
| muted | `--color-muted` | *(add `--color-muted`)* |
| muted-foreground | `--color-muted-foreground` | `--color-text-muted` |

**Coherence rule:** The Admin SPA keeps shadcn names (so `bg-primary`, `text-foreground`, etc. keep working) but **adds** the missing aliases (`--color-surface-hover`, `--color-accent-glow`, `--color-glass`, `--color-glass-border`, `--color-success`, `--color-warning`, `--color-danger`) so the two systems share identical values. The public CSS keeps its names. The mapping table above is the single source of truth for "what means what."

---

## 3. Unified Typography

### 3.1 Decision: Reconcile the Admin divergence

The Admin SPA currently uses **DM Sans / Space Grotesk**; the public sites use **Cinzel / Inter / JetBrains Mono**. **We unify on the public stack** because it is the stronger brand expression and already deployed on both public hosts:

- **Display:** `Cinzel` (serif, engraved/inscribed feel — perfect for the archival luxury direction)
- **Body:** `Inter` (neutral, highly legible, works at small sizes in dense admin tables)
- **Mono:** `JetBrains Mono` (code, IDs, tenant slugs, timestamps)

**Rationale:** Cinzel gives the display type the "inscribed on stone" character that DM Sans/Space Grotesk cannot. Inter is a superior body face for dense data UIs than DM Sans. JetBrains Mono is already the code face. This removes the divergence and gives all three apps one voice.

### 3.2 Font Stacks

```css
--font-sans: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
--font-display: 'Cinzel', Georgia, serif;
--font-mono: 'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace;
```

### 3.3 Type Scale & Usage

| Role | Font | Size | Weight | Letter-spacing | Usage |
| --- | --- | --- | --- | --- | --- |
| Hero title | Display (Cinzel) | `clamp(2rem, 5vw, 3rem)` | 700 | `-0.03em` | Public hero, page titles |
| Display H1 | Display | `2.25rem` | 700 | `-0.02em` | Admin page titles |
| Display H2 | Display | `1.5rem` | 600 | `-0.02em` | Section headings |
| Display H3 | Display | `1.25rem` | 600 | `-0.01em` | Card titles |
| Body | Sans (Inter) | `1rem` | 400 | `0` | Paragraphs, prose |
| Body small | Sans | `0.875rem` | 400 | `0` | Secondary text, table cells |
| Label | Sans | `0.875rem` | 600 | `0` | Form labels, nav |
| Eyebrow | Display | `0.75rem` | 600 | `0.1em` (uppercase) | Hero eyebrow, section kickers |
| Code | Mono | `0.875rem` | 400 | `0` | Code, IDs, tenant slugs |
| Caption | Sans | `0.75rem` | 500 | `0` | Meta, timestamps, badges |

**Base size:** `14px` on mobile, `16px` at `≥768px` (matches public sites; Admin inherits the same responsive base).

**Line-height:** body `1.6`; headings `1.1–1.3`; code `1.5`.

**Admin-specific note:** The Admin SPA's `--font-display` is used for the brand wordmark and page titles. After the change, `font-display` resolves to Cinzel. The `font-black` weight used in the Sidebar brand (`font-black`) must be relaxed to `font-bold` (700) because Cinzel only ships 500/600/700.

---

## 4. Unified Component & Surface Language

### 4.1 Radius

**Zero everywhere.** `--radius: 0rem` (Admin) and `--radius-sm/md/lg/xl: 0` (public). No exceptions. This is the brand signature.

### 4.2 Borders

- Structural borders: `1px solid var(--color-border)` (Admin) / `1px solid var(--color-glass-border)` (public decorative).
- Gold-tinted borders on interactive/active elements: `border-color: var(--color-accent)`.
- Hover on cards: border transitions to accent gold (no shadow in public; Admin may use a subtle gold ring).

### 4.3 Shadows

- **Admin:** `shadow-sm` on cards/buttons; `shadow-md` on hover; `shadow-2xl` on modals/command palette. Shadows are subtle and neutral.
- **Public:** `--shadow-sm/md/lg/xl` tokens already defined; use `--shadow-md` on cards, `--shadow-lg` on featured cards, `--shadow-xl` on modals. Gold glow (`--color-accent-glow`) is reserved for the logo and hero, not general cards.

### 4.4 Glass Treatment

- Sticky header/footer: `backdrop-filter: blur(16px) saturate(180%)` with `background: var(--color-glass)` and `border-bottom: 1px solid var(--color-glass-border)`.
- Admin Topbar/Sidebar: `bg-background/80` + `backdrop-blur` (already present) — align the blur to `blur(16px) saturate(180%)` and use `--color-glass` semantics.

### 4.5 Hover States

- Interactive elements: `transition: color/border/background 150ms cubic-bezier(0.4,0,0.2,1)`.
- Cards: `translateY(-2px)` + gold border on hover (public featured card); Admin cards use gold border + subtle shadow.
- Nav links: gold underline grows `width: 0 → 100%` on hover (public pattern; reuse in Admin nav).

### 4.6 Focus / Ring

- **Admin:** `focus-visible:ring-2 focus-visible:ring-ring` with `ring-offset` = background. Ring color = Royal Gold (dark) / Deep Gold (light).
- **Public:** `outline: 2px solid var(--color-accent)` on `:focus-visible` (already present for cards/links). Standardize all focusable elements to this.
- Inputs: `border-color: var(--color-accent)` + `box-shadow: 0 0 0 3px var(--color-accent-glow)` on focus.

### 4.7 Status Badges

- **Admin** `StatusBadge`: `rounded-none border px-2.5 py-1 text-xs font-semibold`. Published = `border-primary/60 text-primary`; Draft = `border-secondary/60 text-secondary`. Add success/warning/danger variants using `--color-success/warning/danger`.
- **Public** badges: `border-radius: 999px` currently — **change to `0`** to match the zero-radius language. Use `--color-accent-glow` background + `--color-accent-light` text.

### 4.8 Buttons

- **Admin** `Button`: `rounded-none border border-primary bg-primary px-4 py-2 text-sm font-medium text-primary-foreground shadow-sm transition-all hover:bg-primary/90 hover:shadow-md focus-visible:ring-2 focus-visible:ring-ring`. Add `variant` prop: `default` (gold), `outline` (transparent + gold border), `ghost` (transparent, gold text on hover), `destructive` (danger).
- **Public** `.btn-primary`: `background: var(--color-accent); border-color: var(--color-accent); color: var(--color-bg); border-radius: 0`. Hover → `--color-accent-light`. `.btn-link`: gold text.

### 4.9 Cards

- **Admin** `Card`: `rounded-none border border-border bg-card text-foreground shadow-sm`. `CardTitle` uses `font-display` (Cinzel) `text-2xl font-semibold`.
- **Public** `.card-item` / `.featured-card`: `border-radius: 0`, `background: var(--color-bg-surface)`, `border: 1px solid var(--color-glass-border)`. Hover → gold border.

### 4.10 Tables

- **Admin** `Table`: `w-full text-sm`. `TableRow`: `border-b border-border hover:bg-muted`. `TableHead`: `h-12 px-4 text-left font-medium`. Keep zero-radius (tables have no radius anyway).
- **Public:** no native table component; if tables appear in prose, style with `border-bottom: 1px solid var(--color-glass-border)` and `font-family: var(--font-sans)`.

### 4.11 Forms

- **Admin** `Input`/`Textarea`/`Select`: `rounded-none border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:ring-2 focus-visible:ring-ring`. Change `rounded-md` → `rounded-none`.
- **Public** `.filter-bar__input` and any form fields: `border-radius: 0`, `border: 1px solid var(--color-glass-border)`, focus → gold border + `--color-accent-glow` ring.
- Labels: `font-size: 0.875rem; font-weight: 600`.

---

## 5. Unified Background & Texture Treatment

### 5.1 Papyrus Texture

- **Source:** `Papyrus_Background.svg` (exists in both `docs/assets/` and `src/Papyrus.Web.Shared/wwwroot/images/`; Admin has a copy in `src/Papyrus.Admin.UI/src/assets/`).
- **Tiling:** `background-repeat: repeat`.
- **Dark theme:** `opacity: 0.05`, `mix-blend-mode: screen`, tile size `720px` (Admin) / `450px` (public). **Unify tile size to `720px`** for consistency.
- **Light theme:** `opacity: 0.075`, `mix-blend-mode: multiply`.
- **Placement:** fixed, `inset: 0`, `z-index: -1` (Admin `body::before`) / `z-index: 0` (public `.site-background::before`), `pointer-events: none`.

### 5.2 Orbs (public only)

- `.site-background__orb--one` / `--two`: blurred radial glows, `filter: blur(80px)`, `animation: orbFloat 20s ease-in-out infinite alternate`.
- **Dark:** `--orb-color-1/2: transparent` (currently disabled). **Recommendation:** enable subtle gold orbs in dark — `--orb-color-1: rgba(212,175,55,0.06)`, `--orb-color-2: rgba(212,175,55,0.04)`.
- **Light:** keep orbs transparent or very subtle `rgba(154,116,16,0.05)`.
- Admin does **not** use orbs (denser workshop aesthetic) — keep it texture-only.

### 5.3 Grid (public only)

- `.site-background__grid`: 60px grid lines with radial mask.
- **Dark:** `--grid-color: transparent` (disabled). **Recommendation:** enable `rgba(212,175,55,0.05)`.
- **Light:** `--grid-color: rgba(122,91,11,0.07)` (already set).
- Admin does **not** use the grid.

### 5.4 Admin background

- `body::before` already applies the papyrus texture at `720px/0.05/screen`. Add a light-theme override: `[data-theme="light"] body::before { opacity: 0.075; mix-blend-mode: multiply; }`.

---

## 6. Motion & Micro-interactions

### 6.1 Timing & Easing Tokens

```css
--transition-fast: 150ms cubic-bezier(0.4, 0, 0.2, 1);
--transition-base: 250ms cubic-bezier(0.4, 0, 0.2, 1);
--transition-slow: 400ms cubic-bezier(0.4, 0, 0.2, 1);
```

These already exist in public `papyrus.css`. **Add the same tokens to Admin** `index.css` so both systems share identical motion timing.

### 6.2 Page-Load Reveals

- **Public:** hero elements fade/slide in on load (`@keyframes fade-in` / `slide-in`, `opacity 0→1`, `translateY(-0.5rem)`). Respect `prefers-reduced-motion` (disable animations).
- **Admin:** use `@starting-style` / Tailwind v4 `--animate-fade-in` for modal/dialog/command-palette entry. Keep reveals subtle (150–250ms).

### 6.3 Hover Transitions

- All interactive elements transition `color`, `border-color`, `background-color`, `transform` at `--transition-fast`.
- Card hover lift: `translateY(-2px)` at `--transition-base`.
- Nav underline: `width` transition at `--transition-base`.

### 6.4 Theme Switch

- Body transitions `background` and `color` at `--transition-slow` (public already does this). Add the same to Admin `body`.

### 6.5 Reduced Motion

- Wrap all animations in `@media (prefers-reduced-motion: reduce)` → disable transforms/animations, keep opacity transitions only.

---

## 7. Concrete Implementation Checklist

### 7.1 Admin SPA — `src/Papyrus.Admin.UI`

| File | Change |
| --- | --- |
| `index.html` | Replace font links: load `Cinzel:wght@500;600;700`, `Inter:wght@400;500;600;700`, `JetBrains+Mono:wght@400;500`. Add `<meta name="color-scheme" content="dark light" />`. |
| `src/index.css` | (1) Update `@theme`: `--font-sans: 'Inter', ...`, `--font-display: 'Cinzel', ...`, add `--font-mono: 'JetBrains Mono', ...`. (2) Add `--transition-fast/base/slow` tokens. (3) Add a **light theme** block: `:root` (light) + `.dark` (dark) split — move current dark values under `.dark`, add light values under `:root` (Parchment bg, Deep Gold accent, etc.). (4) Add missing aliases: `--color-surface-hover`, `--color-accent-glow`, `--color-glass`, `--color-glass-border`, `--color-success`, `--color-warning`, `--color-danger`. (5) Add light-theme texture override `[data-theme="light"] body::before`. (6) Add `body { transition: background var(--transition-slow), color var(--transition-slow); }`. |
| `components/ui/button.tsx` | Change `rounded-md` → `rounded-none` (already `rounded-none`). Add `variant` prop (`default`/`outline`/`ghost`/`destructive`). |
| `components/ui/card.tsx` | `CardTitle` → use `font-display` (Cinzel). Keep `rounded-none`. |
| `components/ui/badge.tsx` | Keep `rounded-none`. Add success/warning/danger variants. |
| `components/ui/table.tsx` | Keep as-is (already zero-radius, correct borders). Optionally add `font-display` to `TableHead`. |
| `components/ui/input.tsx` | Change `rounded-md` → `rounded-none`. |
| `components/ui/textarea.tsx`, `select.tsx`, `dialog.tsx`, `alert-dialog.tsx`, `dropdown-menu.tsx`, `tabs.tsx`, `form.tsx`, `label.tsx`, `separator.tsx`, `skeleton.tsx`, `sonner.tsx`, `toast.tsx` | Audit for `rounded-*` classes → replace with `rounded-none`. Align focus rings to `ring-ring` (gold). |
| `components/layout/Sidebar.tsx` | Brand wordmark `font-black` → `font-bold` (Cinzel max 700). Keep `text-primary` logo. |
| `components/layout/Topbar.tsx` | Align glass blur to `blur(16px) saturate(180%)`. Theme toggle already present — ensure it toggles `.dark` class (works with new light theme). |
| `components/layout/AdminLayout.tsx` | No change required (uses `bg-background text-foreground`). |
| `components/shared/PapyrusLogo.tsx` | Keep `currentColor` + `viewBox="0 0 100 100"`. Optionally align to the 1024 medallion path from `docs/assets/Papyrus_Logo.svg` for brand fidelity. |
| `components/shared/status-badge.tsx` | Add success/warning/danger variants using new tokens. |
| `components/shared/theme-provider.tsx` | Already supports `light`/`dark`/`system`. No change required. |

### 7.2 Public Sites — `src/Papyrus.Web.Shared`

| File | Change |
| --- | --- |
| `wwwroot/css/papyrus.css` | (1) Add `--color-input`, `--color-ring`, `--color-muted` aliases to both themes. (2) Change all `border-radius: 999px` (badges, tags, filter tags) → `0`. (3) Unify papyrus texture tile size to `720px`. (4) Enable subtle gold orbs/grid in dark theme. (5) Standardize `:focus-visible` to `outline: 2px solid var(--color-accent)` on all interactive elements. (6) Ensure `.btn`, `button`, `input`, `select`, `textarea` all `border-radius: 0`. |
| `Pages/Shared/_PapyrusLayout.cshtml` | Font link already loads Cinzel/Inter/JetBrains Mono — no change. Verify `data-theme` default and `papyrus-theme.js` load order. |
| `wwwroot/js/papyrus-theme.js` | Already handles light/dark via `data-theme` + `papyrus-theme` localStorage. No change required. |

### 7.3 SVG Assets

| File | Change |
| --- | --- |
| `docs/assets/Papyrus_Logo.svg` | Source of truth for the medallion. Keep `viewBox="0 0 1024 1024"`, `currentColor`-driven. |
| `docs/assets/Papyrus_Background.svg` | Source of truth for the texture. Keep as-is. |
| `src/Papyrus.Web.Shared/wwwroot/images/Papyrus_Background.svg` | Public texture copy. Keep in sync with `docs/assets`. |
| `src/Papyrus.Admin.UI/src/assets/Papyrus_Background.svg` | Admin texture copy. Keep in sync. |

---

## 8. Verification Plan

### 8.1 Admin SPA

```bash
cd src/Papyrus.Admin.UI
npm run build
npm run lint
```

- Build must succeed with 0 errors.
- Lint must pass with 0 errors/warnings.
- Manual check: toggle light/dark via Topbar; verify both themes render with correct tokens; verify zero-radius on all components; verify Cinzel/Inter/JetBrains Mono load.

### 8.2 .NET (Blog + Portfolio + Shared)

```bash
dotnet build .\Papyrus.sln --nologo --no-incremental
```

- Must report `0 Error(s)` / `0 Warning(s)`.
- Manual check: load Blog and Portfolio in both light and dark; verify shared `papyrus.css` changes apply to both hosts (proves the shared design system works).

### 8.3 Git Hygiene

```bash
git diff --check
```

- Must report no whitespace errors.

### 8.4 Cross-App Consistency Checklist

- [ ] Same font stack (Cinzel/Inter/JetBrains Mono) in all three apps.
- [ ] Zero-radius everywhere.
- [ ] Royal Gold accent in dark; Deep Gold accent in light (both apps).
- [ ] Papyrus texture at 720px, screen/dark + multiply/light.
- [ ] Light + dark themes in all three apps.
- [ ] `currentColor` logo in all three apps.
- [ ] Shared `papyrus.css` drives both Blog and Portfolio (no per-host divergence).

---

## Appendix A — Admin Light Theme Token Block (reference)

```css
:root {
  color-scheme: light;
  --radius: 0rem;
  --color-background: oklch(0.955 0.012 90);   /* #F5F1E8 */
  --color-card: oklch(0.99 0.006 90);          /* #FFFDF7 */
  --color-surface: oklch(0.92 0.014 90);       /* #EDE7D9 */
  --color-surface-hover: oklch(0.90 0.016 90); /* #F3EBDD */
  --color-primary: oklch(0.55 0.12 80);        /* #9A7410 Deep Gold */
  --color-primary-foreground: oklch(0.99 0.006 90);
  --color-secondary: oklch(0.42 0.07 80);      /* #5A4520 */
  --color-secondary-foreground: oklch(0.99 0.006 90);
  --color-accent: oklch(0.55 0.12 80);
  --color-accent-foreground: oklch(0.99 0.006 90);
  --color-foreground: oklch(0.184 0.012 285);  /* #1A1A1F */
  --color-border: oklch(0.82 0.03 80);
  --color-input: oklch(0.88 0.02 80);
  --color-ring: oklch(0.55 0.12 80);
  --color-muted: oklch(0.94 0.01 90);
  --color-muted-foreground: oklch(0.45 0.02 80);
  --color-destructive: oklch(0.55 0.2 25);     /* #B33A2B */
  --color-destructive-foreground: oklch(0.99 0 0);
  --color-success: oklch(0.5 0.12 160);        /* #16794F */
  --color-warning: oklch(0.6 0.12 70);         /* #8A5A00 */
  --color-danger: oklch(0.55 0.2 25);
  --color-accent-glow: rgba(154, 116, 16, 0.20);
  --color-glass: rgba(245, 241, 232, 0.94);
  --color-glass-border: rgba(122, 91, 11, 0.28);
}
```

> The existing dark values (currently in `:root, .dark`) move under a `.dark` block unchanged, with the addition of the new aliases (`--color-surface-hover`, `--color-accent-glow`, `--color-glass`, `--color-glass-border`, `--color-success`, `--color-warning`, `--color-danger`).
