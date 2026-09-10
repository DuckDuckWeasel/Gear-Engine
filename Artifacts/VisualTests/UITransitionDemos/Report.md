# UI Transition Demo Visual Verification

## Scope

The verification covers the generated `UITransitionsGallery` and
`UITransitionsDestination` scenes plus a midpoint sample of the Square transition
preset. Captures use a 1920 x 1080 game view.

## Evidence

- `Gallery.png` verifies the gallery hierarchy, checkerboard preview, preset label,
  and four controls.
- `Destination.png` verifies the companion scene's alternate visual theme and return
  control.
- `SquareMidpoint.png` verifies that the Square transition shader renders across the
  full-screen transition surface at a partial transition rate.

## Result

PASS. Both scene layouts are readable, controls remain within the safe content area,
and the full-screen transition surface covers the intended viewport.

No Unity Test Framework result file was produced. This task adds sample scene layout
and UI wiring, so verification used focused compilation, serialized-reference checks,
Unity log analysis, and deterministic visual captures rather than a new automated
test fixture.
