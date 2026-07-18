---
name: scaffold-action-creator
description: "Creates and extends commands (Actions) and Event Handlers (Triggers) for the Scaffold Visual Scripting system in Gear Engine. Activate this when the user asks to create a visual scripting command, a node, or a trigger."
---

# Scaffold Action Creator

This skill provides the exact specifications and templates for creating custom Visual Scripting Commands and Event Handlers for the **Scaffold** system in Gear Engine. 

When the user asks you to create a new command, action, or node for the visual scripting system, you MUST follow these guidelines.

## 1. Creating a New Command (Action)

Scaffold commands dictate actions that the Flowchart executes. All actions in Gear Engine must inherit from `GearEngine.Core.Actions.ActionBase`.

### Requirements
- **Namespace:** Wrap the class in the `Scaffold` namespace so it natively integrates with the visual scripting search menus.
- **Attributes:** 
  - `[CommandInfo("Category Name", "Command Name", "Command Description")]` (This groups the command in the Scaffold Editor).
  - `[AddComponentMenu("")]` (Hides it from the default Unity component menu).
  - `[Serializable]`
- **Inheritance:** Inherit from `ActionBase` (located in `GearEngine.Core.Actions`).
- **Variables:** Use Scaffold Data structs (e.g., `StringData`, `FloatData`, `IntegerData`, `BooleanData`) with `[SerializeField]` to expose them to the Unity Inspector. This allows Game Designers to pass variables from the Flowchart.
- **Execution:** Override `public override void OnEnter()`. 
  - **CRITICAL:** You MUST call `Continue();` when the command finishes execution, or the flowchart will hang forever.
- **Dependency Injection:** If the command needs a service (e.g., `IAnalyticsService`), do NOT use Singletons. Resolve the dependency using VContainer from the host's LifetimeScope: `host.GetComponentInParent<LifetimeScope>()?.Container.Resolve<IMyService>()`.

### Template for Actions

```csharp
using System;
using UnityEngine;
using GearEngine.Core.Actions;
using VContainer;
using VContainer.Unity;

namespace Scaffold
{
    [CommandInfo("Custom Category", 
                 "Custom Command", 
                 "Does a custom action using the Scaffold system.")]
    [AddComponentMenu("")]
    [Serializable]
    public class CustomCommand : ActionBase
    {
        [Tooltip("A string variable exposed to the Inspector.")]
        [SerializeField] protected StringData myStringData = new StringData("");

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(myStringData.Value))
            {
                Continue();
                return;
            }

            // Example of Dependency Injection via host
            var scope = host.GetComponentInParent<LifetimeScope>();
            if (scope != null)
            {
                var service = scope.Container.Resolve<ISomeService>();
                service.DoSomething(myStringData.Value);
            }

            // You MUST call Continue() to pass execution to the next Block.
            Continue();
        }

        // Optional: Custom text shown in the Block node UI
        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(myStringData.Value)) return "Error: No data";
            return $"Doing action with: {myStringData.Value}";
        }

        // Optional: Custom color for the command in the block UI
        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255); // Example Pinkish Color
        }
    }
}
```

## 2. Creating an Event Handler (Trigger)

Event Handlers sit at the top of a Block and dictate when the Block starts executing (e.g., `On Start`, `On Collision`, `On Tag Event`).

### Requirements
- **Namespace:** `Scaffold`
- **Attributes:**
  - `[EventHandlerInfo("Category Name", "Event Name", "Event Description")]`
  - `[AddComponentMenu("")]`
- **Inheritance:** Inherit from `Scaffold.EventHandler`.
- **Execution:** When the condition is met, call `ExecuteBlock();` to start the Flowchart execution.

### Template for Event Handlers

```csharp
using UnityEngine;

namespace Scaffold
{
    [EventHandlerInfo("Custom Category",
                      "Custom Event",
                      "The block will execute when this custom event occurs.")]
    [AddComponentMenu("")]
    public class CustomEventReceived : EventHandler 
    {
        [Tooltip("Exposed variable to filter events")]
        [SerializeField] protected string filterName = "";

        // Example method that gets called by an external system or Unity Event
        public void OnCustomEventTriggered(string incomingName)
        {
            if (this.filterName == incomingName)
            {
                // Trigger the Flowchart Block to start
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            return filterName;
        }
    }
}
```

## 3. Placement
Place new actions and event handlers inside the `Assets/GearEngine/Scripts/Game/GearEngine/Core/Actions/ScaffoldActions/` directory, grouped by category folders (e.g., `UI`, `Audio`, `Analytics`).
