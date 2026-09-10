---
title: Gear Engine One-Page Website Concept
audience: client-service
status: generated-concept-iteration-2
date: 2026-08-29
project: Gear Engine
---

# Gear Engine — One-Page Website Concept

One cinematic scrolling page presents the game fantasy, explains the configure-to-race loop, proves the relationship between machine and result, and closes with a single playable-prototype action. The macro layout follows the immersive rhythm of the [Dota 2 homepage](https://www.dota2.com/home) while all brand, copy, mechanics, imagery, and calls to action remain specific to Gear Engine.

> **Evidence label — GeneratedConcept:** all three mockups are design exploration. They are not gameplay captures, final key art, release claims, or proof of unverified features.

## Direction Decision

**Pop-Art Workshop is the approved baseline.** Pit-Lane Editorial and Race Broadcast vary that same direction without changing the one-page architecture or brand foundation. The discarded Night Circuit direction has been removed from the project and evidence manifest.

| Direction | Strongest quality | Main risk | Recommended use |
| --- | --- | --- | --- |
| **1 — Pop-Art Workshop** | Clearest balance of game fantasy, mechanics, and approved brand kit | Needs disciplined spacing so the comic treatment does not become noisy | Approved baseline |
| **2 — Pit-Lane Editorial** | Strongest premium composition and best hero-loop staging | The cream editorial field can feel less immediately digital without motion | Premium production candidate |
| **3 — Race Broadcast** | Best expression of observe → understand → improve | Dense telemetry can imply unsupported precision if not labeled as illustrative | Systems-led campaign variant |

## Version 1 — Pop-Art Workshop

![GeneratedConcept — Gear Engine Pop-Art Workshop one-page website](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/WebsiteConcepts/T_GearEngineWebsiteConceptPopArtWorkshop.png>)

The approved baseline uses cream fields, black ink outlines, halftone texture, cyan speed strokes, and torn-paper transitions. It is the most direct expression of the current brand kit and the easiest direction to adapt to mobile.

**Hero loop:** the car crosses the checkered line on a stable wide camera; crowd flags, wheel rotation, dust, and speed strokes animate while the left copy field remains still.

## Version 2 — Pit-Lane Editorial

![GeneratedConcept — Gear Engine Pit-Lane Editorial one-page website](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/WebsiteConcepts/T_GearEngineWebsiteConceptPitLaneEditorial.png>)

This variation turns the same system into a premium motorsport magazine. Larger asymmetric cuts, a pit-lane environment, diagonal red marks, and more negative space strengthen the launch-page character without abandoning the bright identity.

**Hero loop:** the car exits a dark pit tunnel into daylight. The background and wet-floor reflections move continuously, but the cream headline field remains optically locked.

## Version 3 — Race Broadcast

![GeneratedConcept — Gear Engine Race Broadcast one-page website](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Artifacts/Marketing/Evidence/WebsiteConcepts/T_GearEngineWebsiteConceptRaceBroadcast.png>)

This variation makes the race itself the readout for the machine. Broadcast lower-thirds, numbered stages, route consequences, and readable graphs foreground the game's optimization fantasy.

**Hero loop:** the car runs through a broad sunlit curve while flags, dust, timing ticks, and the track move. The headline and primary CTA remain static; scoreboard values may update only once per loop.

## Shared One-Page Grid

The desktop page uses a centered **1180 px content grid**, a minimum **24 px outer gutter**, and full-bleed media bands behind the grid. The Dota reference contributes the macro rhythm: overlay navigation, a full-viewport hero, an overlapping card strip, immersive narrative sections, a final wide call to action, and a compact footer.

| Order | Section | Desktop behavior | Gear Engine content |
| ---: | --- | --- | --- |
| 1 | Header | 88–96 px high, transparent over hero, solid Asphalt after scroll | Text-only wordmark; `GAME`, `HOW IT PLAYS`, `MEDIA`; `PLAY THE PROTOTYPE` |
| 2 | Hero | 85–100 svh, full-bleed image or muted video, copy aligned to the lower-left grid | Red number-77 car; mechanics-to-speed promise; one primary CTA |
| 3 | Core-loop strip | Three equal cards overlap the hero boundary by 48–72 px | `CONFIGURE`, `RACE`, `UPGRADE` |
| 4 | Build section | 70/30 or 55/45 media/text split with generous vertical space | Cream gear board, causal links, short mechanic explanation |
| 5 | Race section | Full-bleed contrasting band; copy and proof swap sides | Red car, track, readable telemetry, cause-and-effect message |
| 6 | Progression section | Centered statement followed by three reward cards | Unlock, synergy, refine-and-repeat progression |
| 7 | Final CTA | 360–440 px cinematic band | One promise and one `PLAY THE PROTOTYPE` action |
| 8 | Footer | 144–180 px black band | Leonardo Lycan lockup and `leonardolycan.com` only |

## Copy System

### Header

- Wordmark: `GEAR ENGINE`
- Navigation: `GAME` · `HOW IT PLAYS` · `MEDIA`
- Primary action: `PLAY THE PROTOTYPE`

### Hero — Pop-Art Workshop or Pit-Lane Editorial

- Headline: `BUILD A SMARTER MACHINE. PROVE IT AT SPEED.`
- Support: `Configure. Race. Observe. Improve.`

### Hero — Race Broadcast

- Headline: `BUILD THE SYSTEM. READ THE RACE.`
- Support: `Every run shows you what to change next.`

### Narrative Sections

1. `BUILD THE MACHINE` — Place gears and connect effects. Every position changes the system.
2. `WATCH CAUSE BECOME EFFECT` — Run the race and read what the machine produced.
3. `CHOOSE. REBUILD. GO AGAIN.` — Earn a new option, reconfigure the board, and improve the next run.

Copy must remain direct and evidence-based. Do not claim a release date, store availability, player count, awards, platform certification, or features that are not present in the current evidence manifest.

## Visual and Brand Rules

- Use the splash red car as the canonical public identity reference.
- Use a text-only Gear Engine wordmark until a deterministic final wordmark is approved.
- Do not use the retired orange-and-blue gear mark or any generic settings-style gear as the logo.
- Use Oswald Variable for short uppercase display text and Inter Variable for body copy and controls.
- Keep primary actions Victory Yellow with Asphalt text; use Track Blue for secondary information.
- Preserve complete gameplay frames when they are used as proof. Generated promotional scenes must be labeled `GeneratedConcept` in source records and review materials.
- Keep one strong chromatic accent per action. Racing Red and Track Blue must not be combined as ordinary text.
- Maintain WCAG AA contrast, 44 × 44 px minimum interactive targets, a 3 px Victory Yellow keyboard focus ring, and reduced-motion behavior.

## Video and Motion Direction

The motion system is divided into one cinematic media zone and several lightweight interface-motion zones. The site does not use scroll-jacking, autoplay audio, or continuous animation in every section.

### Hero Video Loop

| Property | Production contract |
| --- | --- |
| Duration | 6–8 seconds with a seamless visual return to the opening frame |
| Camera | Stable wide camera; action occupies the right 55–65% so the copy field never shifts |
| Motion | Car travel, wheel rotation, dust or spray, crowd flags, track streaks, and restrained environment parallax |
| Static layer | Wordmark, navigation, headline, support copy, and CTA remain HTML above the video |
| Audio | None; video is always muted and uses `playsinline` |
| Delivery | WebM primary, MP4 fallback, factual poster image, `preload="metadata"`, and an initial target below 4 MB |
| Playback | Autoplay only when the hero is visible; pause when offscreen or when the document is hidden |
| Fallback | Poster only for reduced motion, data saving, failed playback, or narrow devices when the video crop loses the car |

The loop must be promotional art, not disguised gameplay. Authentic gameplay footage may appear later in a separate media block with normal controls and an evidence label.

### Motion Map

| Section | Motion | Trigger | Limit |
| --- | --- | --- | --- |
| Header | Transparent-to-Asphalt background and compact spacing | First 64 px of scroll | 180–220 ms; no bouncing |
| Hero | Muted loop behind static HTML copy | Visible viewport | One loop source; no camera shake under copy |
| Core loop | Cards rise 12 px and arrows appear in sequence | First entry | One 420–600 ms sequence; never auto-cycle |
| Build the machine | Connector paths draw once; affected gears rotate 8–12 degrees | First entry or explicit interaction | No perpetual gear spinning |
| Race readout | Graph lines draw and values count once | First entry | Values must be illustrative or sourced from verified data |
| Reward cards | Small lift, border highlight, and detail reveal | Hover or keyboard focus | Maximum 4 px lift; no parallax |
| Final CTA | Checkered or speed-line field drifts slowly | Section visible | Decorative only; stop after one pass on low-power devices |
| Footer | None | — | Logo and URL remain stable |

### Variant-Specific Hero Shots

1. **Pop-Art Workshop:** the car crosses the finish line; use crowd flags and comic speed strokes to hide the loop seam.
2. **Pit-Lane Editorial:** the car leaves a shaded tunnel into daylight; match the first and last frames on the tunnel's darkest beat.
3. **Race Broadcast:** the car follows a wide curve; timing ticks reset at the loop seam while the physical camera remains stable.

The gear board is the product's visual center of gravity. It appears before decorative rewards and shows a legible chain rather than unrelated icons. Racing imagery communicates consequence, not direct steering controls that the current product evidence does not verify.

### Accessibility and Performance

- `prefers-reduced-motion: reduce` replaces video with the poster and disables reveal transforms, graph drawing, parallax, and number animation.
- Text and controls never render inside the video asset; they remain selectable, responsive HTML.
- Motion does not delay content visibility or block keyboard navigation.
- The page reserves the hero aspect ratio before media loads to prevent layout shift.
- Lazy-load media below the hero and use responsive image sources for the tall concept art.

## Responsive Behavior

- **Desktop ≥ 1200 px:** 1180 px centered grid; three-card loop strip; alternating wide media/text bands.
- **Tablet 768–1199 px:** 24–40 px gutters; hero copy uses 55–65% width; cards become a horizontal snap row or a two-plus-one grid.
- **Mobile < 768 px:** single-column flow; header collapses to wordmark, menu, and one CTA; copy moves below the hero focal point; all cards stack; no section requires horizontal scrolling.
- Keep the red car visible at every crop. Never crop gameplay UI used as evidence.
- Decorative parallax and speed-line motion stop when `prefers-reduced-motion: reduce` is active.

## Footer Contract

The Valve/Dota trademark block shown in the reference is intentionally excluded. The Gear Engine footer contains only:

- the approved Leonardo Lycan lockup;
- the exact URL `leonardolycan.com`;
- optional copyright text only after the responsible legal entity and year are confirmed.

Do not add Valve, Dota, Steam, platform badges, social icons, partner marks, or invented legal copy.

## Production Acceptance Criteria

- One continuous page; no separate news, heroes, esports, login, or language routes.
- The macro grid remains recognizably inspired by the Dota homepage rhythm without copying its art or identity.
- The hero exposes the game promise and playable-prototype CTA in the first viewport.
- The hero loop respects the 6–8 second, muted, stable-copy, poster-fallback, and reduced-motion contract.
- The configure → race → observe → upgrade loop is understandable without reading long copy.
- The gear board remains more prominent than generic racing spectacle.
- The supplied Leonardo Lycan lockup and `leonardolycan.com` close the page.
- Desktop, tablet, mobile, keyboard, reduced-motion, and contrast checks pass before publication.

## Final Image Prompts

### Version 1 — Pop-Art Workshop

`High-fidelity tall desktop one-page Gear Engine launch site; Dota-inspired macro grid only; bright daytime pop-art hero with canonical red number-77 car; headline “BUILD A SMARTER MACHINE. PROVE IT AT SPEED.”; overlapping CONFIGURE/RACE/UPGRADE cards; cream gear-board editorial section; red/cyan cause-and-effect telemetry band; reward-card progression; final workshop CTA; cream, Asphalt, Racing Red, Victory Yellow, Track Blue, and Grass Green palette; text-only Gear Engine wordmark; Leonardo Lycan lockup and leonardolycan.com in a compact black footer; no Dota, Valve, Steam, generic gear logo, fake awards, release claims, or watermark.`

### Version 2 — Pit-Lane Editorial

`High-fidelity tall desktop one-page Gear Engine launch site derived from the approved Pop-Art Workshop baseline; premium motorsport magazine composition; asymmetric cream and black editorial cuts; red number-77 car exiting a dark pit tunnel into daylight as a loopable hero frame; stable left copy field; overlapping core-loop cards; chain-reaction gear-board section; dark telemetry band; reward cards; final workshop CTA; text-only Gear Engine wordmark; Leonardo Lycan lockup and leonardolycan.com footer; no standalone gear logo, Dota, Valve, Steam, release claims, fake awards, or watermark.`

### Version 3 — Race Broadcast

`High-fidelity tall desktop one-page Gear Engine launch site derived from the approved Pop-Art Workshop baseline; bright race-day broadcast and strategy-board direction; red number-77 car on a broad sunlit curve as a loopable hero frame; stable headline “BUILD THE SYSTEM. READ THE RACE.”; numbered core-loop cards; causal route callouts; full-width race-readout telemetry band; reward choices; final CTA “TUNE IT. RUN IT. BEAT IT.”; text-only Gear Engine wordmark; Leonardo Lycan lockup and leonardolycan.com footer; no standalone gear logo, Dota, Valve, Steam, release claims, fake awards, or watermark.`

## Source References

- [Gear Engine Brand Kit](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Docs/Marketing/GearEngineBrandKitClient.md>)
- [Game Concept and Creative Brief](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Docs/Marketing/GameConceptCreativeBriefClient.md>)
- [Game Evidence Library](<file:///Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Docs/Marketing/GameEvidenceLibraryClient.md>)
- [Current Dota 2 homepage reference](https://www.dota2.com/home)
