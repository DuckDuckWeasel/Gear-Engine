# Mobile Game Tutorials and FTUE Research

## Purpose and Scope

This document synthesizes research and practitioner evidence about mobile-game tutorials and First-Time User Experience (FTUE). It is intended to guide a future export built from Gear Engine foundations, but it deliberately does **not** assume that the new game will retain the current gear-board-and-race loop.

The practical objective is not “make players finish a tutorial.” It is to help new players reach an independent, satisfying execution of the core loop, understand why repeating it is valuable, and leave with a clear reason to return.

### Evidence Labels

| Label | Meaning | How it is used here |
| --- | --- | --- |
| **Research / platform guidance** | Peer-reviewed work or official platform guidance. | Supports general principles and measurement discipline. |
| **Industry synthesis** | Structured analysis across commercial games. | Identifies recurring genre practices, not universal rules. |
| **Practitioner case** | A postmortem or professional observation from a specific title. | Supplies hypotheses and examples; its results are not benchmarks. |
| **Project evidence** | Direct reading of the current Gear Engine codebase. | Describes current capability and implementation risks only. |
| **Recommendation** | An inference for the future export. | Must be validated through playtests and controlled experiments. |

## Research Synthesis

### What onboarding must accomplish

FTUE includes more than a tutorial: it is the first sequence of play, framing, feature access, progression, and help that makes a player comfortable participating. The first minutes of F2P mobile play have high churn, making this a product-design and measurement problem rather than merely an instructional-writing task. **[Research]** [Petersen et al., CHI PLAY 2017](https://vbn.aau.dk/en/publications/evaluating-the-onboarding-phase-of-free-toplay-mobile-games-a-mix/)

An effective FTUE should establish four outcomes:

1. The player can perform the minimum viable interaction.
2. The player sees the complete core-loop consequence, not only a setup screen.
3. The player attributes success to their own action rather than to an opaque scripted outcome.
4. The player sees the next near-term goal or reward without being diverted into non-essential systems.

Apple’s guidance aligns with this: teach the core loop in short, sequential objectives; let players demonstrate competence; get to self-directed play quickly; and defer non-essential systems. **[Platform guidance]** [Apple — Onboarding for Games](https://developer.apple.com/app-store/onboarding-for-games/)

### Patterns that consistently deserve preference

| Pattern | Why it helps | Design application |
| --- | --- | --- |
| **Time-to-first-play** | A new player evaluates the promise of a mobile game quickly. | Launch directly into a safe playable situation; do not put account, lore, menus, settings, or monetization before the first meaningful input. |
| **Teach through action** | A player remembers an executed action better than a detached explanation. | Present one verb, give a visible target, wait for the action, then show its consequence. Use text only to clarify intent or a non-obvious rule. |
| **Progressive disclosure** | Front-loading mechanics creates cognitive load before motivation exists. | Teach the core action in session one; introduce modifiers, optimization, meta systems, and advanced tools only when the player needs them. |
| **Constrained teaching, then agency** | A hard gate can prevent an unfamiliar input from being missed, but permanent hand-holding removes ownership. | Restrict input only for the exact first use of an essential or dangerous action. Immediately provide a small unguided repetition. |
| **Failure-safe challenge** | A completely automatic sequence feels empty, while a true early game-over can feel unfair. | Create a low-risk choice with readable stakes; allow a recoverable miss, hint, retry, or generous correction. |
| **Contextual replayable help** | Players forget mechanics introduced before they become useful. | Provide a replayable “How to play” entry point and unobtrusive, trigger-based reminders. |
| **Feature staging** | A large menu implies work and creates decision paralysis. | Reveal the next relevant system after a successful core-loop moment, with a clear benefit and one purposeful destination. |

The guidance above is supported by Apple’s recommendation to teach one clear step at a time, demonstrate competency, use multiple short tutorials for multiple systems, and make help replayable. **[Platform guidance]** [Apple — Onboarding for Games](https://developer.apple.com/app-store/onboarding-for-games/)

Liquid & Grit’s teardown of 26 casual, core, and casino titles reaches a complementary conclusion: stronger FTUEs get players playing rapidly, teach primarily through gameplay, and foreground distinctive mechanics, progression, or rewards. **[Industry synthesis]** [Liquid & Grit — FTUE Toolkit](https://www.liquidandgrit.com/first-time-user-experience-ftue-toolkit/)

### Useful trade-offs, not absolute rules

**Guided versus free interaction.** The first activation of an unfamiliar gesture may be gated by a dimmed overlay, hand animation, or target highlight. After it succeeds, the player should repeat it in an open state. A casual-mobile tutorial case study illustrates a common implementation: restrict unrelated input only while teaching the specific drag or tap, then reveal its score or timer consequence. **[Practitioner case]** [Sommer — Tutorial Design for Casual Mobile Games](https://medium.com/%40csommer828/tutorial-design-for-casual-mobile-games-c077f5d145d8)

**Challenge versus friction.** Casual does not mean consequence-free. A player should make a real, legible choice and feel that success came from it, while the first failure remains recoverable. Pascal Luban argues for an apparent but safe risk rather than a pure “press the highlighted button” sequence. **[Practitioner opinion]** [Luban — Best Practices for a Successful FTUE](https://www.linkedin.com/pulse/best-practices-successful-ftue-pascal-luban)

**Tutorial length.** There is no reusable number of seconds that guarantees retention. A 2023 practitioner postmortem reports that one title improved results after replacing a 5–6 minute tutorial with an immediate playable level and about 30 seconds of interactive guidance; the reported lift belongs to that game, audience, creative, and acquisition mix. **[Practitioner case]** [Hellmich — Optimizing the FTUE](https://www.linkedin.com/pulse/optimizing-first-time-user-experience-yannik-hellmich-xuicc)

**Forced menu tutorials.** They are appropriate only when a newly unlocked menu is required for the next enjoyable action. Practitioner testing described by James Varma found that unguided menu introductions can be missed, but that does not justify forcing every menu visit. Gate the minimum required interaction, state its player benefit, and return to play. **[Practitioner opinion]** [Varma — Game Onboarding Insights](https://www.linkedin.com/pulse/one-tutorial-step-forward-some-insights-game-onboarding-james-varma-keojc)

### Anti-patterns to avoid

- Explaining controls, meta, currencies, social systems, and monetization before the player has played the advertised core interaction.
- Showing a tooltip without a required action or visible feedback.
- Teaching a feature long before its first useful moment.
- Replacing meaningful choice with a long sequence of forced taps.
- Making a first failure terminal, costly, or ambiguous.
- Showing rating, notifications, consent prompts beyond required platform consent, subscriptions, ads, or IAP before initial competence.
- Treating tutorial completion as proof of comprehension; a player can advance while confused.
- Relying solely on text, color, sound, or a tiny target. Use concise language, contrast, animation, input tolerance, and non-audio feedback together.

### Retention, rewards, and ethics

The first reward should confirm the loop: reward the action that the export wants repeated, and clearly show what it unlocks or improves. Do not introduce premium currency or an IAP until the player has experienced the ordinary value of the system it affects; Apple gives the same sequencing guidance for consumable purchases. **[Platform guidance]** [Apple — Onboarding for Games](https://developer.apple.com/app-store/onboarding-for-games/)

Open loops—an unfinished but understandable next goal—can give a player a reason to return, such as a newly unlocked challenge, upgrade, collection item, or next track. They should be transparent and player-beneficial, never disguise a timer, scarcity mechanic, or paid bypass as instruction. **[Practitioner opinion]** [Luban — Best Practices for a Successful FTUE](https://www.linkedin.com/pulse/best-practices-successful-ftue-pascal-luban)

## Hyper-Casual and Hybrid-Casual Decision Guide

| Dimension | Hyper-casual priority | Hybrid-casual priority |
| --- | --- | --- |
| Session-one objective | Prove the single repeatable interaction is immediately satisfying. | Prove the core interaction and establish the first progression promise. |
| Tutorial duration | Usually one to a few interaction beats; continue playing almost immediately. | Still short and playable, but may include a first reward, unlock, or brief meta transition. |
| Allowed complexity | One primary verb and one immediate success condition; defer almost everything else. | One primary verb plus one light strategic choice or modifier if it is central to the game’s appeal. |
| Progression reveal | A next challenge or cosmetic/score objective is enough. | Show a clear near-term upgrade, collection, build, or level goal after first success. |
| Ads / IAP timing | Do not interrupt the first competence loop; validate creative and retention before tuning ad cadence. | Delay monetization education until ordinary value is understood; make any starter offer optional and value-led. |
| Primary success signals | First interaction, first successful loop, retries, first-session depth, D1 retention. | The hyper-casual signals plus first reward claim, first meta action, tutorial-to-core-loop conversion, and later retention. |

**Recommendation:** if the export’s market model is not selected, build a shared short playable onboarding path first. Keep progression and monetization beats configuration-driven so a hyper-casual version can omit them and a hybrid-casual version can introduce them after initial competence.

## Reusable FTUE Blueprint

This is a sequencing template, not a literal script. Replace every placeholder with the future export’s core action, objective, and feedback.

| Beat | Player experience | Tutorial behavior | Success evidence |
| --- | --- | --- | --- |
| 0. Promise match | The launch immediately resembles the advertised fantasy and interaction. | Load a safe playable state with no menu detour. | `first_open` and scene-ready timing. |
| 1. Playable hook | The player performs **one core action**. | Show one clear target, optional gesture animation, and a short action-oriented prompt. Gate only unrelated input if needed. | `first_interaction` and input latency. |
| 2. Consequence | The action visibly changes the game state. | Use distinct visual, sound, and haptic feedback where supported; state the immediate objective. | `tutorial_step_completed:core_action`. |
| 3. First competence | The player repeats the action with less or no guidance. | Remove the overlay; use a recoverable challenge with a lightweight hint on hesitation. | `first_independent_success`. |
| 4. Core-loop closure | The player sees setup → action → outcome → reward/progress. | Summarize only the result that motivates the next loop. | `first_core_loop_completed`. |
| 5. Next goal | The player knows what to do next and why. | Reveal one appropriate challenge, upgrade, collection, or score goal. Do not open every menu. | `next_goal_presented`, first voluntary repeat. |
| 6. Contextual education | A secondary mechanic appears when it becomes useful. | Pause only if the action is essential; otherwise use a dismissible cue and replayable help. | Feature-specific entered/completed events. |
| 7. Exit and return | The player can stop without losing orientation. | Preserve progress, show the next actionable goal on return, and offer “How to play” from settings or the relevant screen. | Resume and return-session conversion. |

### Accessibility and resilience requirements

- Keep prompts short, localized, and action-first; never depend on a language-specific gesture alone.
- Do not encode the next action only in color; retain contrast and a non-color shape, animation, or label cue.
- Make tutorial targets forgiving on small screens and support different aspect ratios, safe areas, and touch precision.
- Respect reduced-motion, haptic, sound, and text-size preferences when those options exist in the export.
- Make every tutorial step idempotent and resumable after app suspension, reload, network loss, or an interrupted scene transition.
- Include skip behavior only after the player has a safe way to replay the information; emit an explicit skip event rather than treating it as completion.

## Measurement and Experimentation

### Event contract

Use immutable tutorial and step identifiers plus a variant identifier. The minimum event funnel is:

| Event | Required properties | Purpose |
| --- | --- | --- |
| `first_open` | install cohort, platform, build, acquisition source where available | Establish cohort entry. |
| `ftue_ready` | load duration, tutorial variant | Find technical first-play friction. |
| `tutorial_started` | tutorial ID, variant, entry context | Confirm exposure. |
| `tutorial_step_entered` | tutorial ID, step ID, ordinal, variant | Measure step reach. |
| `tutorial_step_completed` | tutorial ID, step ID, elapsed time, retries, hint count | Measure comprehension and effort. |
| `tutorial_step_abandoned` | tutorial ID, step ID, exit reason if known | Locate stuck or interrupting steps. |
| `first_independent_success` | core-loop ID, retries, elapsed time | Distinguish compliance from competence. |
| `first_core_loop_completed` | outcome, reward, elapsed time | Confirm the full loop was experienced. |
| `tutorial_completed` / `tutorial_skipped` | tutorial ID, elapsed time, final step | Keep completion and skipping analytically distinct. |
| `session_end` | session length, furthest FTUE state | Connect funnel behavior to engagement. |
| retention events | D1 and later cohort markers | Measure downstream value, not only tutorial conversion. |

Segment every view by tutorial variant, platform/device tier, locale, app version, and acquisition source where privacy and consent allow. Treat D1 retention and independent core-loop completion as primary outcome metrics; use tutorial completion, duration, hints, and retries as diagnostic metrics.

### Experiment protocol

1. Write the hypothesis in player terms, for example: “A gesture animation will reduce hesitation on the first drag without reducing independent success.”
2. Change one meaningful variable per controlled variant: prompt format, amount of gating, sequencing, target presentation, safety net, or help timing.
3. Randomly assign eligible new players and preserve the assigned variant through the whole FTUE.
4. Predefine the primary metric, guardrails (crash rate, time-to-play, accidental input), cohort window, and decision threshold.
5. Pair quantitative funnels with moderated playtests or short in-game feedback to explain why a step fails.
6. Ship the winning learning, then repeat. Roblox’s onboarding guidance explicitly frames tutorial steps as a funnel and recommends A/B testing individual steps to locate leaks. **[Platform guidance]** [Roblox — Onboarding](https://github.com/Roblox/creator-docs/blob/main/content/en-us/production/game-design/onboarding.md)

Tencent’s 2024 GDC session is a useful framing model: attraction, a motivating goal, and tutorial effectiveness should be evaluated together. **[Industry presentation]** [GDC Vault — Start Right, Start Fun](https://gdcvault.com/play/1034824/Start-Right-Start-Fun-Unveiling)

## Gear Engine Technical Assessment

### Confirmed current capability

| Component | Confirmed responsibility | Reuse value for a future export |
| --- | --- | --- |
| `TutorialSO` | Stores a tutorial ID, a `TutorialProgressController` prefab, a next-tutorial link, and unlock references. | A data asset can represent authored tutorial identity and sequencing. |
| `TutorialWrapper` | Stores the tutorial catalog plus `StartTutorials` and `BattleTutorials`; resolves an asset by ID. | Provides a central catalog and entry-point grouping. |
| `TutorialController` | Starts, loads, completes, chains, and emits analytics for an active tutorial controller. | Provides a lifecycle orchestration starting point. |
| `TutorialProgressController` | Raises started, named-step, and completed events. | Supports event-level funnel instrumentation. |
| Tutorial analytics events | Emit `tutorial_started`, `tutorial_step_reached`, and `tutorial_completed`, including tutorial ID and completion skip state. | Provides a partial analytics vocabulary. |
| `CompleteTutorialOptimisticHandler` | Produces an immediate success response for a completion request and defines a validation seam. | Can become the optimistic/reconcile boundary once a real request path exists. |
| `TutorialFocusService` and `FocusPresetSO` | Builds a dimmed overlay, input blocking, target focus, indicator, and optional UI effect. | Provides visual guidance suitable for the first essential action. |
| `TutorialLoadingEventBinder` | Maps tutorial-loading state to the global loading presentation. | Provides a project-level presentation bridge during long tutorial transitions. |

### Current constraints and risks

1. **Progress is volatile.** `TutorialController` uses an in-memory `HashSet<string>` for completed IDs and an in-memory current tutorial ID. It does not itself load or store durable progress, so app restart/resume behavior is not production-ready. **[Project evidence]** `Assets/Packages/com.scaffold.tutorial/Runtime/Controllers/TutorialController.cs`
2. **The visible completion path is local.** `CompleteTutorialAsync` updates local state and chains the next tutorial. Although `CompleteTutorialOptimisticHandler` exists, the controller code does not visibly dispatch `CompleteTutorialRequest`; the handler’s server validation method is empty. Confirm or implement the actual GameApi request and reconciliation path before relying on server persistence. **[Project evidence]** `Assets/Packages/com.scaffold.tutorial/Runtime/Controllers/TutorialController.cs`, `Assets/Packages/com.scaffold.tutorial/Runtime/CloudCode/CompleteTutorialOptimisticHandler.cs`
3. **Tutorial views are directly instantiated.** The controller instantiates the configured progress-controller GameObject and then injects it. This couples tutorial execution to a prefab and scene lifetime, and it offers no explicit cleanup of the previous instance after replacement. **[Project evidence]** `Assets/Packages/com.scaffold.tutorial/Runtime/Controllers/TutorialController.cs`
4. **Focus service has two ownership paths.** `TutorialFocusInstaller` registers `TutorialFocusService` with VContainer, while the static `TutorialFocusService.Instance` can allocate a new service and `DontDestroyOnLoad` canvas itself. This static fallback bypasses the DI lifecycle and can make ownership, tests, and teardown ambiguous. **[Project evidence]** `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Highlight/TutorialFocusService.cs`, `TutorialFocusInstaller.cs`
5. **No production authored flow was found.** The repository contains focus-test assets and a test tutorial scene, but the assessment found no completed export-ready tutorial asset flow. Treat the package as infrastructure, not as validated onboarding content. **[Project evidence]** `Assets/GearEngine/Data/Gear/Tag/Tutorial/`, `Assets/GearEngine/Scenes/Test/TestTutorialScene.unity`

### Architecture and pattern assessment

- **Data-driven / Command-style authoring is present:** `TutorialSO` and the visual-scripting integration separate authored sequence data from controller lifecycle.
- **Observer is present:** progress controller events feed tutorial lifecycle and analytics.
- **State ownership is incomplete:** tutorial status is currently controller-local rather than a persisted profile state service.
- **Singleton anti-pattern risk is present:** `TutorialFocusService.Instance` bypasses the registered VContainer service. A future export should inject an `ITutorialFocusService`-style contract into the tutorial presenter and let the composition root own its lifetime.
- **Factory/lifetime boundary is missing:** direct `Object.Instantiate` in the controller should eventually be replaced or wrapped by an explicitly owned tutorial-presentation factory, especially when a future export spans scene changes.

## Export-Agnostic Adaptation Sequence

No code change is made by this research task. When a new export has a defined core loop, implement the following sequence.

1. **Define success before authoring.** Name the minimum independent core-loop success event and the first voluntary repeat. Do not define the tutorial as “all prompts shown.”
2. **Create configuration-owned steps.** Assign stable tutorial ID, step ID, entry condition, success condition, allowed-input policy, focus presentation, fallback hint, skip rule, and next-step rule. Keep gameplay conditions outside UI presentation code.
3. **Use gated interactive beats.** For each essential unknown action, focus the target, permit the action, wait for a gameplay event, then remove the gate. Follow it with an unguided repetition.
4. **Persist and reconcile progress.** Add a profile-owned tutorial-progress service with durable completed, in-progress, skipped, variant, and last-step state. Make completion idempotent, optimistic where appropriate, and reconciled against the authoritative backend.
5. **Expand the event funnel.** Preserve existing start/step/complete events, add entered, abandoned, independent-success, skip, variant, elapsed-time, hint, and retry fields, and validate the event schema before acquisition testing.
6. **Resolve focus-service ownership.** Use DI rather than the static fallback; create and clean the overlay at the appropriate view or scope lifetime, and test transition, resume, and target-destruction behavior.
7. **Add contextual help.** Provide replayable help and condition-based reminders after the FTUE instead of replaying the full tutorial by default.
8. **Validate variants.** Run a small usability test first, then controlled production experiments using the measurement contract above. Tune one variable at a time.

### Acceptance Scenarios for the Future Implementation

- A first-time player reaches the first independent core-loop success without seeing a non-essential menu, ad, IAP, or long-form exposition.
- Suspending or restarting during any step resumes safely without duplicate rewards, duplicated overlays, or lost progress.
- Skipping, completing, retrying, and abandoning a tutorial produce distinct analytics outcomes.
- A missing focus target or scene transition fails safely, clears input restrictions, logs an actionable error, and leaves the game playable.
- The same configuration can omit hybrid-casual meta beats for a hyper-casual export without changing core tutorial code.
- A controlled variant remains stable for one player across all FTUE events and can be filtered by platform and app version.

## Source Notes

The requested [Deconstructor of Fun](https://www.deconstructoroffun.com/) site was checked as a potential market-context source. Its public homepage did not expose directly relevant, indexable tutorial or FTUE material during this research pass, so it is not used to support tutorial-design claims. The research instead relies on directly relevant official, academic, industry-synthesis, and clearly labelled practitioner sources above.
