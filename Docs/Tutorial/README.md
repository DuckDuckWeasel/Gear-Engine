# Scaffold Tutorial Plugin

This module provides a decoupled, data-driven tutorial system integrated with
Scaffold Visual Scripting and Scaffold LiveOps for progress persistence.

## 1. What We Have Built (Current Setup)

The architecture is composed of three main layers:

### A. Data & Configuration Layer
- **`TutorialSO`**: A ScriptableObject that defines the identity and rules of a specific tutorial (e.g., "First Battle", "Upgrade Item").
- **`TutorialWrapper`**: A ScriptableObject container that aggregates all available tutorials into lists (`StartTutorials` and `BattleTutorials`). It acts as the central database of tutorials for the game.
- **`TutorialRequirement`**: An abstract base class for defining conditions. You create subclasses for specific game triggers (e.g., `ReachLevelRequirement`, `DefeatEnemyRequirement`).

### B. Controller & Execution Layer
- **`TutorialController`** / **`TutorialProgressController`**: The core engines that check if a tutorial should start, evaluate its requirements, and manage its lifecycle. They use standard C# events (`Action`) instead of Zenject Signals, adhering to Scaffold's architecture.

### C. Integration Layer
- **LiveOps / GameApi**: The `CompleteTutorialOptimisticHandler` intercepts `CompleteTutorialRequest` to immediately simulate success locally (Optimistic update) while the server processes the state change in the background.
- **Scaffold Visual Scripting integration**:
  - `ScaffoldTutorialAdapter`: Bridges plain Blackboard block-start events to the tutorial controller from the optional `BlackboardBehaviour` wrapper.
  - `TutorialProgressControllerVariable`: A serializable plain-C# compatibility value for actions that need an explicit controller reference.
  - `WaitTutorialRequirementCommand`: A managed action definition that schedules requirement polling through the Blackboard execution context.

---

## 2. How to Configure a Guided Step-by-Step Tutorial

Here is the exact workflow to create and wire up a brand new tutorial (e.g., "Welcome to the Hub").

### Step 1: Define Your Triggers / Requirements
If your tutorial depends on a specific game state (e.g., "Player reached the Hub screen"), create a requirement script if it doesn't exist:
```csharp
[CreateAssetMenu(menuName = "Tutorial/Requirements/Screen State")]
public class ScreenStateRequirement : TutorialRequirement
{
    public string targetScreen;
    // Implement IsMet() to check if the current screen == targetScreen
}
```
*Create this ScriptableObject in your project folder.*

### Step 2: Create the Tutorial Data (TutorialSO)
1. Right-click in your Project view -> **Create > Tutorial > TutorialSO**.
2. Name it `TUT_WelcomeHub`.
3. Give it an ID (e.g., `welcome_hub_01`).
4. Assign your Requirements (from Step 1) to its requirement list if it needs prerequisites.

### Step 3: Register the Tutorial
1. Locate your main **`TutorialWrapper`** ScriptableObject asset.
2. Add your `TUT_WelcomeHub` to the **Start Tutorials** (or Battle Tutorials) list.
*This makes the system aware that this tutorial exists and should be checked for initialization.*

### Step 4: Create the Visual Flow
1. Add `BlackboardBehaviour` to the scene or UI prefab, or create a
   `BlackboardDefinitionAsset` for a reusable flow.
2. Choose the Direct or ScriptableObject source in the Blackboard inspector and open
   the managed Blackboard window.
3. Add a managed Block for the first step, such as `Show Dialogue`.
4. Add the required managed action definitions, such as Say, Portrait, and UI focus.
   These are serialized data; the editor does not add Block or action components.

### Step 5: Wait for Player Actions (The "Wait" Command)
When you need the player to actually do something (like click a specific button) before the tutorial advances:
1. In the managed Block, add **Tutorial > Wait Tutorial Requirement**.
2. Drag and drop the specific `TutorialRequirement` ScriptableObject that represents the action you are waiting for (e.g., a `ButtonClickedRequirement` for the "Upgrade" button).
3. The plain Blackboard runtime keeps the action running through its injected
   scheduler until the requirement succeeds, then resumes the Block.

### Step 6: Complete the Tutorial
At the final managed Blackboard Block:
1. Use the managed **Invoke Method** action, or a game-specific action, to call
   `CompleteTutorial()` on the `TutorialProgressController`.
2. The Controller will dispatch the `CompleteTutorialRequest` to the **GameApi**.
3. Our `CompleteTutorialOptimisticHandler` instantly validates it on the client, grants rewards if any, and updates the UI without waiting for the server roundtrip.
