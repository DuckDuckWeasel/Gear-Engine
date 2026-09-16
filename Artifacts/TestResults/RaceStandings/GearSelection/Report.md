# Unity Test Report

Generated: 2026-09-16T18:35:37.416210+00:00

## Test intent

Rerun the two existing Roguelike gear-selection tests after supplying the required ad manager and event bus fixtures. Verify loading offers and selecting a gear still adds one item, consumes one roll and returns home in the standalone path.

## Selection

- Project: `/Users/leonardosilva/Documents/MatheusCohen/VictorScreensMigration`
- Platform mode: `edit`
- Selector: `GearEngine.Campaign.Tests.Editor.RoguelikeViewModelTests`

## Outcome

| Platform | Result | Total | Passed | Failed | Skipped | Inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | No XML | 0 | 0 | 1 | 0 | 0 |

## Failures


- `No result file` — Unity did not write NUnit XML.

## Relevant Unity log events


- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(32,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,36): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Assets/GearEngine/Scripts/Game/Campaign/Tests/Editor/RoguelikeViewModelTests.cs(74,77): error CS0234: The type or namespace name 'Ads' does not exist in the namespace 'Scaffold' (are you missing an assembly reference?)
- `EditMode.log` — Scripts have compiler errors.

## Evidence

- NUnit XML: [EditMode.xml](EditMode.xml)
- Editor log: [EditMode.log](EditMode.log)

## Test evidence

No test-declared visual evidence was created during this run.
