# Generic UI Effects Guide

## Objective

Provide a project-owned, UI-first material library for high-value, self-animated visual effects.

## Design Goals

The library is organized around four reusable effect groups:

| Category | Included capabilities | Design direction |
| --- | --- | --- |
| Color and presentation | Glow, outlines, HSV, gradients, contrast, pixelation, posterization | Use `Coffee.UIEffects` presets for tone, color, edge, and gradation filters. |
| Transitions | Fade, burn, dissolve, patterns, radial clipping | Use `Coffee.UIEffects` transitions; add the project-owned dissolve loop for ambient use. |
| UV motion | Texture scroll, wave, twist, rotation, zoom, fish-eye, pinch, shake | Use `Gear/UI/LoopEffects` for self-animated source-texture motion. |
| Ambient feedback | Hologram, glitch, flicker, shine, and ghosting | Use self-animated material presets for ambient feedback. |

## Capability Coverage Matrix

The project-owned loop shader uses no effect keywords and only the two standard UGUI clipping variants. This keeps the library predictable for UI use while the installed UIEffect package supplies optimized static filters.

| Capability family | Coverage | Decision |
| --- | --- | --- |
| Glow, tint, blend, HSV, contrast, negative, greyscale, posterize | `Coffee.UIEffects` tone and color filters | Reuse the installed package instead of duplicating mature filters. |
| Outer/inner outlines, shadow, alpha outline | `Coffee.UIEffects` edge and shadow filters; `P_UIE_GlowingOutlineLoop` | Reuse native rendering support; add an animated generic preset. |
| Blur, pixelation, RGB shift, edge detection, motion blur | `Coffee.UIEffects` sampling filters; `M_UIE_PixelatePulse`, `M_UIE_ChromaticDrift` | Cover common UI feedback. Motion-blur ghosting is excluded because it is expensive and poor for static UI. |
| Gradients, color ramps, color swap | `Coffee.UIEffects` gradation, HSV, and color filters | Preserve UI batching and avoid a second color-management pipeline. |
| Fade, burn, dissolve, shiny, pattern, melt, blaze | `Coffee.UIEffects` transitions plus five animated `P_UIE_*` loop presets | Reuse the existing transition implementation. |
| Texture scroll, wave, zoom, twist, rotate, fish-eye, pinch, shake | `Gear/UI/LoopEffects` materials | Implement as source-UV animation because a native UIEffect detail overlay cannot scroll the source image itself. |
| Hologram, glitch, shine, flicker | `M_UIE_HologramScan`, `M_UIE_Glitch`, `M_UIE_Shine`, and `P_UIE_HologramMatrix` | Implement the highest-value ambient feedback effects. |
| Radial clip, fill amount, masking | UGUI `Image.fillAmount`, `Mask`, and `RectMask2D` | Retain engine-native controls; a shader replacement would be less accessible. |
| Wind, grass, hand-drawn, billboard, fog, atlas controls | Not adopted | These are sprite/mesh presentation features, not generic UGUI material effects. |
| Multi-target color replacement and ghost trails | Not adopted | Require additional texture samples or bespoke art direction; defer until a concrete generic use case exists. |

## Ownership Boundary

`Gear/UI/LoopEffects` is a project-owned shader. The library contains only project-owned shader code, materials, presets, and documentation.

## Implemented Effect Modes

| Mode | Best use | Self-animation control |
| --- | --- | --- |
| Texture scroll | Repeating menu backgrounds and ribbons | Direction and speed |
| Wave | Water, banners, heat ambience | Strength, frequency, and speed |
| Shine | Rewards, cards, CTA accents | Sweep angle, width, and speed |
| Scanline | Holograms and tech UI | Line frequency and speed |
| Glitch | Error states and cyber feedback | Strength, band frequency, and speed |
| Radial pulse | Targeting and attention rings | Center, radius, and speed |
| Vortex | Portals and magical backdrops | Center, strength, and speed |
| Border pulse | Selection and focus states | Width, frequency, and speed |
| Pixelate | Retro transitions and disabled previews | Pixel size |
| Dissolve | Ambient reveal and decay loops | Threshold, softness, and speed |
| Zoom pulse | Breathing backgrounds and emphasis | Center, strength, frequency, and speed |
| Shake | Impact and warning feedback | Strength, frequency, and speed |
| Fisheye | Portals and stylized idle motion | Center, strength, and speed |
| Chromatic drift | Digital or spectral feedback | Direction, strength, and speed |
| Aurora flow | Ambient color ribbons | Color pair, frequency, strength, and speed |
| Flicker | Electrical and degraded display feedback | Frequency, intensity, tint, and speed |
| Ghost trail | Directional motion echo | Direction, trail width, and tint |
| Heat distortion | Fire, exhaust, and atmospheric motion | Frequency, strength, and speed |

## Material Preset Catalog

| Material | Effect |
| --- | --- |
| `M_UIE_BackgroundScrollRight`, `M_UIE_BackgroundScrollLeft` | Horizontal looping background scroll |
| `M_UIE_BackgroundScrollUp`, `M_UIE_BackgroundScrollDown` | Vertical looping background scroll |
| `M_UIE_Wave` | Two-axis wave distortion |
| `M_UIE_Shine` | Diagonal highlight sweep |
| `M_UIE_HologramScan` | Cyan moving scanlines |
| `M_UIE_Glitch` | Horizontal digital displacement |
| `M_UIE_RadialPulse` | Expanding center pulse |
| `M_UIE_Vortex` | Oscillating vortex distortion |
| `M_UIE_BorderPulse` | Animated perimeter glow |
| `M_UIE_PixelatePulse` | Animated pixel-grid resolution |
| `M_UIE_DissolveLoop` | Procedural dissolve with a colored edge |
| `M_UIE_ZoomPulse` | Breathing zoom loop |
| `M_UIE_Shake` | Deterministic impact shake |
| `M_UIE_FisheyePulse` | Breathing lens distortion |
| `M_UIE_ChromaticDrift` | Moving red/blue channel separation |
| `M_UIE_AuroraFlow` | Animated two-color aurora overlay |
| `M_UIE_Flicker` | Deterministic tinted flicker |
| `M_UIE_GhostTrail` | Directional two-sample ghost trail |
| `M_UIE_HeatDistortion` | Layered heat-wave UV distortion |

## Effect Configuration Catalog

Every `E_UIE_*` asset is a `MaterialUIEffectPreset`. It applies its native UIEffect settings or its matching `M_UIE_*` material through one execution path. Assign these presets to `UI Effects > Apply Effect`.

| Configuration | Material |
| --- | --- |
| `E_UIE_AuroraFlow` through `E_UIE_ZoomPulse` | Matching `M_UIE_*` material with the same suffix |

## Native UIEffect Preset Catalog

These assets use the installed `Coffee.UIEffects.UIEffectPreset` type. They complement the material library where an existing UGUI `UIEffect` component is already the preferred integration point.

| Preset | Effect |
| --- | --- |
| `P_UIE_NeonSweep`, `P_UIE_NeonSweepReverse` | Forward and reverse animated edge sweeps |
| `P_UIE_HologramMatrix` | Animated hologram detail overlay |
| `P_UIE_DiamondFlow`, `P_UIE_StarDrift`, `P_UIE_StripeFlow` | Scrolling decorative detail patterns |
| `P_UIE_BlazeTransitionLoop` | Animated blaze transition |
| `P_UIE_ShinyTransitionLoop` | Animated diagonal highlight transition |
| `P_UIE_PatternDissolveLoop` | Moving pattern-driven dissolve |
| `P_UIE_GlowingOutlineLoop` | Animated glowing outline with a supporting shadow |
| `P_UIE_MeltLoop` | Animated melt transition |

Every catalog entry is an `E_UIE_*` `MaterialUIEffectPreset`. It applies either its native UIEffect settings (including native looping) or its animated material through the same execution path.

| Catalog entry | Native UIEffect behavior |
| --- | --- |
| `E_UIE_NeonSweep`, `E_UIE_NeonSweepReverse` | Matching `P_UIE_NeonSweep*` preset |
| `E_UIE_HologramMatrix`, `E_UIE_DiamondFlow`, `E_UIE_StarDrift`, `E_UIE_StripeFlow` | Matching `P_UIE_*` preset |
| `E_UIE_BlazeTransitionLoop`, `E_UIE_ShinyTransitionLoop`, `E_UIE_PatternDissolveLoop`, `E_UIE_GlowingOutlineLoop`, `E_UIE_MeltLoop` | Matching `P_UIE_*` preset |

## Usage Notes

- Apply the materials in `Assets/3rdParty/UIEffect/UIEffectPresets/UIEffects` to a UGUI `RawImage` or `Image`.
- The library is not applied automatically to project scenes or prefabs. Select an `E_UIE_*` preset in `UI Effects > Apply Effect` to use an effect.
- Use a repeat-wrapped texture for the four background-scroll materials. `RawImage` is the most reliable choice for tiled backgrounds, especially when a sprite atlas is in use.
- Use `Coffee.UIEffects` for static color/outline/transition composition and `Gear/UI/LoopEffects` when the source texture itself must animate.
- Keep one loop effect per UI element. This avoids stacking unnecessary UI draw calls and makes effect ownership clear.

## Applying a Loop Material

Use the `UI Effects > Apply Effect` visual-scripting command. Assign a target GameObject containing an `Image`, `RawImage`, or TextMeshPro UGUI graphic, then assign an `E_UIE_*` preset. The same preset can also be selected from the UIEffect inspector dropdown.

For a blackboard-driven example, create an **Object** variable, assign `E_UIE_BackgroundScrollRight` as its value, and select that variable in **Configuration** on the action. Changing the Object variable to another `E_UIE_*` asset changes the applied effect without editing the action.

The command adds `UILoopMaterialEffect`, assigns the material, and automatically disables an enabled native `UIEffect` component on the same object. This is required because both components own the UGUI material. Use `UI Effects > Clear Loop Material` to restore the original material and re-enable the native component. `UI Effects > Apply Loop Material` remains available for existing visual-scripting graphs.

## Project Integration

This library is intentionally generic. No scene, prefab, runtime component, gameplay system, or application screen in this project references these materials or the shader. A future integration must name the exact target UI element and retain this library as a reusable dependency rather than embedding effect logic in feature code.
