# Gear Engine Brand Kit

- Version: 1.1
- Last verified: 2026-09-10
- Status: working digital brand standard; current logo masters are raster and still require production-vector and rights review

## Purpose

Gear Engine combines readable engineering, arcade speed, and racing-comic energy. The brand should help a player or portfolio visitor understand the product in this order:

1. Build a mechanical system.
2. Race and observe the result.
3. Improve the next run.

The system is intended for the game, its portfolio case study, press materials, submission pages, and product-facing documentation. It does not authorize a visual redesign of the Unity application.

## Source of truth

Use the following hierarchy:

1. This document defines identity roles, application rules, UI hierarchy, accessibility targets, and production boundaries.
2. [`Artifacts/Marketing/Brand/BrandTokens.json`](../Artifacts/Marketing/Brand/BrandTokens.json) is authoritative for machine-readable color, typography, spacing, radius, motion, and accessibility values.
3. [`Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandKit.html`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandKit.html) is the browser presentation of this system.
4. [`Docs/Marketing/GearEngineBrandKitClient.md`](Marketing/GearEngineBrandKitClient.md) is the earlier client-facing direction that established the palette, typography, voice, and splash-image invariant.

If a hex value differs between prose and `BrandTokens.json`, the JSON token value wins. If asset status is unclear, treat the asset as unresolved rather than inventing a production role.

## Brand relationship

| Entity | Type | Relationship | Brand role |
| --- | --- | --- | --- |
| Gear Engine | Product/game | Portfolio project | Owns the racing wordmark, game illustration, palette, typography, UI language, and gameplay evidence. |
| Leonardo Lycan | Personal portfolio | Owner/endorser | May appear as a restrained attribution or footer lockup. It is not the game logo. |
| GORn 2026 | Submission/event | Distribution context | May frame the project submission but does not replace the Gear Engine identity. |

Do not fuse these marks into a new combined logo. Keep ownership and endorsement visually separate.

## Brand idea

**Build. Race. Improve.**

This line is the shortest expression of the product loop and the preferred high-level brand promise. It should lead to factual gameplay evidence rather than unsupported claims.

Core attributes:

- **Mechanical:** interlocking parts, visible cause and effect, purposeful detail.
- **Fast:** racing-line motion and concise actions, without constant animation.
- **Readable:** strong hierarchy, calm surfaces, legible metrics, and explicit states.
- **Competitive:** lap, result, and improvement language without esports claims.
- **Optimistic:** energetic color and forward motion rather than aggression or dystopian styling.

Avoid generic neon technology, photoreal motorsport, luxury-car cues, gear clip art, excessive chrome, and unverified feature claims.

## Identity assets

### Primary marketing wordmark

[`T_GearEngineLogoRacingFinal.png`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/Media/T_GearEngineLogoRacingFinal.png) is the default current wordmark for portfolio, press, and marketing surfaces.

Preserve these approved characteristics:

- exact words `GEAR ENGINE`;
- white letter faces with irregular black racing outlines;
- stacked, forward-leaning composition;
- red racing stroke and red track accents;
- checkered finish at the right edge;
- the existing `R`, `I`, and `N`-to-`G` cuts.

Do not redraw, recolor, stretch, rotate, crop, add glow, replace the lettering, or separate the checkered finish from the lockup.

Current file status:

- format: PNG RGBA;
- dimensions: 1446 × 645 px;
- lifecycle: current approved raster application;
- limitation: no verified production vector master or complete rights/provenance package was found.

### Game illustration and icon source

[`GearEngineSplashLogo.png`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/Media/GearEngineSplashLogo.png) is the approved red-car racing illustration used as the game icon/splash invariant. Use it for square or illustrative placements where the full wordmark would be too small.

The underlying image is 1024 × 1024 and JPEG-encoded despite its `.png` filename. Preserve the original; do not repeatedly recompress it. A correctly named and losslessly packaged production master remains a handoff task.

### Portfolio endorsement

[`T_LeonardoLycanLockup.png`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/Media/T_LeonardoLycanLockup.png) identifies the portfolio owner. Use it only in attribution areas such as a footer, case-study masthead, or credits panel. It must remain secondary to the Gear Engine identity.

### Non-primary variants

- `T_GearEngineLogoRacingConsolidated.png` is a retained alternate application, not the default.
- `GearEngineRouteMark.svg` and its raster exports belong to earlier route exploration. They must not silently replace the current racing wordmark.
- Concept images and experimental wordmarks remain concept evidence even when packaged beside current assets.

## Logo placement

- On dark surfaces, use the current wordmark unchanged with enough surrounding space to preserve its irregular outline and checkered finish.
- Keep clear space equal to at least 8% of the rendered wordmark width on every side for digital layouts. This is a layout rule, not a substitute for a future vector-master construction grid.
- Use the full wordmark at 220 CSS px wide or larger. Below that width, use the square game illustration or a future approved small-size mark.
- Place the logo on Asphalt, Pit Wall, or a calm image region. Do not place it over dense gameplay controls or high-frequency track detail.
- Use one Gear Engine mark per composition unless a repeated mark has an explicit navigational purpose.
- Keep the Leonardo Lycan endorsement smaller and spatially separate.

## Color system

| Token | Hex | Role | Safe default text |
| --- | --- | --- | --- |
| Asphalt | `#0B0F14` | Page background, deepest field, dark text on bright actions | Race White or Smoke |
| Pit Wall | `#151C24` | Cards, panels, navigation, secondary surfaces | Race White or Smoke |
| Pit Wall Elevated | `#202A35` | Hovered/raised surface and nested content | Race White or Smoke |
| Race White | `#FFF8E8` | Primary text and light reading field | Asphalt when used as a background |
| Smoke | `#C8D0D8` | Secondary text, metadata, subdued borders | Asphalt when used as a background |
| Racing Red | `#EF3E2F` | Brand motion, race emphasis, destructive/error accent | Asphalt |
| Victory Yellow | `#FFD43B` | Primary action, focus, key callout | Asphalt |
| Track Blue | `#18AEEA` | Link, secondary action, informational accent | Asphalt |
| Grass Green | `#43B649` | Success and verified-ready state | Asphalt |

### Contrast and allocation

- Race White on Asphalt is 18.16:1; Smoke on Asphalt is 12.33:1.
- Race White on Pit Wall is 16.22:1; Smoke on Pit Wall is 11.01:1.
- Asphalt on Victory Yellow is 13.48:1.
- Asphalt on Track Blue is 7.58:1.
- Asphalt on Grass Green is 7.35:1.
- Asphalt on Racing Red is 4.94:1.
- Race White on Racing Red is 3.67:1 and must not be used for normal-size text.
- Race White on Track Blue, Victory Yellow, or Grass Green also fails normal-text contrast. Use Asphalt text on those surfaces.

Use one dominant chromatic action per state. Yellow is the normal primary action; blue is secondary/informational; red is racing emphasis or destructive/error meaning; green confirms success. Never encode status with color alone.

## Typography

| Role | Typeface | Default use |
| --- | --- | --- |
| Display | Oswald Variable, 200–700 | Product headings, race metrics, section labels, short calls to action |
| Body | Inter Variable, 100–900 | Instructions, descriptions, controls, tables, and long-form reading |
| Display fallback | Arial Narrow, sans-serif | Environments where Oswald is unavailable |
| Body fallback | Arial, sans-serif | Environments where Inter is unavailable |

Rules:

- Reserve uppercase Oswald for short headings, labels, and metrics.
- Use sentence-case Inter for instructions and body copy.
- Keep digital body copy at 16 px or larger.
- Do not use the racing wordmark lettering as a general-purpose typeface.
- Avoid condensed text for paragraphs, error explanations, or accessibility instructions.

The web press-kit package includes both variable fonts in its adjacent `Fonts` directory. Their presence in the press kit does not mean they are licensed or packaged for every runtime/application target; verify distribution rights before release.

## Layout system

- Maximum long-form content width: 1180 px.
- Minimum outer spacing: 24 px on expanded and medium layouts; 18 px on compact layouts.
- Use a clear sequence: orientation → identity → rules → practical application → assets.
- Prefer one primary reading column. Use two or three columns only for comparable items such as colors, roles, or do/don't guidance.
- Use Pit Wall cards with restrained borders; not every paragraph needs a card.
- Keep expressive red/yellow/blue accents near actions and proof, leaving documentation surfaces calm.
- Screenshots retain their original aspect ratio and enough interface context to prove the feature.

## UI and action hierarchy

Every state names one intended next action.

| Context | Primary | Secondary | Tertiary/utility | Do not show equally |
| --- | --- | --- | --- | --- |
| Brand-kit entry | Download brand tokens | Return to press kit | In-page section navigation | Multiple equally filled download/back buttons |
| Portfolio project hero | Play or view the current build | Watch gameplay | Open press/brand details | Concept-map or ranking links as if shipped |
| Game setup | Race when configuration is valid | Edit/remove selected part | Storage/help | Garage, storage, and race with equal weight |
| Result | Continue the loop | Inspect details or upgrade | Share/press utility | Unrelated configuration actions |
| Error/recovery | Retry or return to safe state | View details | Copy diagnostic | Auto-dismissed error messaging |

Control rules:

- Primary: Victory Yellow fill, Asphalt text, explicit verb.
- Secondary: Track Blue fill with Asphalt text, or a Race White outline on dark surfaces.
- Destructive: Racing Red fill with Asphalt text plus a destructive verb/icon; never color alone.
- Disabled: keep visible only when it teaches the sequence, and state what enables it.
- Interactive targets: at least 44 × 44 CSS px for the portfolio/press-kit experience.
- Focus: 3 px Victory Yellow outline with visible offset; focused elements must not be obscured by sticky navigation.

## Adaptive behavior

### Expanded: 960 px and wider

- Keep hero content and the primary asset preview visible together.
- Use multi-column comparison grids for colors, asset roles, and UI guidance.
- Keep a sticky in-page navigation bar for orientation.

### Medium: 640–959 px

- Collapse three-column areas to two columns.
- Preserve the same section order and action labels.
- Let navigation scroll within its own row rather than forcing page-wide horizontal overflow.

### Compact: below 640 px

- Use one content column.
- Stack primary and secondary actions at full width.
- Keep the primary action first in visual and focus order.
- Scale the full wordmark down only to its 220 px minimum; use the square illustration for smaller placements.
- Never create page-level horizontal scrolling.

## Imagery and evidence

Use explicit labels:

- **Current build:** unaltered capture from the current working build.
- **Historical:** prior implementation or superseded presentation.
- **Concept:** proposed experience, not available gameplay.
- **Promotional composite:** brand-led assembly that may combine current and conceptual material.

Rules:

- Never crop away controls needed to understand a current-build claim.
- Do not present concept maps, rankings, or reward screens as implemented.
- Do not alter gameplay outcomes, currencies, or telemetry in factual evidence.
- Use native video controls, factual poster frames, captions where needed, and no autoplay with sound.
- Avoid decorative motion that competes with the primary action.

## Voice and messaging

Voice is direct, energetic, and based on cause and effect.

Preferred verbs:

- build;
- configure;
- race;
- observe;
- improve.

Preferred structure:

1. Name the action.
2. Name what changes.
3. Show factual proof.

Avoid claims about launch status, player traction, certification, platform availability, ranking, maps, or live-service completeness unless current evidence supports them.

## Accessibility baseline

The web and portfolio experience targets WCAG 2.2 AA, but static inspection alone is not a conformance claim.

- Normal text: at least 4.5:1 contrast.
- Large text and essential non-text boundaries: at least 3:1.
- Text must reflow without page-level horizontal scrolling at equivalent compact widths.
- All interactive controls must work by keyboard and show visible focus.
- Focused elements must remain visible when sticky navigation is present.
- Touch targets should be at least 44 × 44 CSS px; the WCAG 2.2 minimum target criterion is a floor, not the preferred touch size.
- Respect `prefers-reduced-motion` and remove nonessential transform/scroll animation.
- Provide meaningful alternative text for identity and gameplay images.
- Do not use color as the only signal for selection, status, error, or success.

Reference: [Web Content Accessibility Guidelines (WCAG) 2.2](https://www.w3.org/TR/WCAG22/).

## Do and do not

| Do | Do not |
| --- | --- |
| Use the current racing wordmark for marketing identity. | Replace it with an exploratory route mark. |
| Use the square racing illustration when the wordmark is too small. | Compress the full wordmark into an unreadable icon. |
| Keep one primary action per context. | Give every available link equal visual weight. |
| Use Asphalt text on Yellow, Blue, Green, and Red fills. | Use Race White body text on bright brand colors. |
| Label current, historical, concept, and composite evidence. | Present conceptual features as shipped gameplay. |
| Keep the Leonardo Lycan lockup secondary. | Fuse the portfolio endorsement into the game logo. |
| Preserve original raster masters. | Auto-trace or redraw them as if they were approved vectors. |

## Asset handoff

Browser-ready kit:

- [`BrandKit.html`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandKit.html)
- [`BrandTokens.json`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandTokens.json)
- [`Fonts`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/Fonts)
- [`Media`](../Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/Media)

Before production release:

- [ ] Rebuild and approve the racing wordmark as a deterministic vector master.
- [ ] Resolve the `.png`/JPEG mismatch for the game illustration without recompressing the source.
- [ ] Record owner, provenance, approval date, and modification rights for each primary asset.
- [ ] Produce approved monochrome, reversed, small-size, and app-icon masters.
- [ ] Define a formal construction grid, clear space, and minimum sizes from the vector master.
- [ ] Verify font distribution rights for each target.
- [ ] Test compact, medium, expanded, keyboard, touch, zoom/reflow, contrast, and reduced-motion behavior.
- [ ] Commit and review the current brand-kit artifacts before treating them as release masters.

## Change record

### 1.1 — 2026-09-10

- Separated the Gear Engine product identity from the Leonardo Lycan portfolio endorsement.
- Established the current racing wordmark as the default marketing mark and the red-car illustration as the compact/illustrative invariant.
- Added contrast-safe color pairings, contextual action hierarchy, responsive behavior, evidence labels, accessibility targets, and production gaps.
- Aligned the browser presentation with this Markdown guide and the existing token file.

### 1.0 — 2026-08-29

- Established the game-specific palette, Oswald/Inter typography, direct voice, splash-image invariant, and basic layout/accessibility rules.
