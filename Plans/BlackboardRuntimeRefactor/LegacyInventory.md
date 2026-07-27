# Legacy Blackboard inventory and behavior-parity baseline

This inventory is the Milestone 1 baseline for the breaking Blackboard runtime
replacement. It records the source surface that must be migrated or deliberately
removed. It is generated from the repository state at the start of
`codex/blackboard-runtime-refactor`; update it when the migration changes the
remaining surface.

## Runtime ownership

| Legacy type | Current ownership | Required replacement |
| --- | --- | --- |
| `Blackboard : MonoBehaviour` | Discovers sibling Blocks and Variables, owns lifecycle, execution, messages, and persistence access. | Plain `Blackboard` plus optional `BlackboardBehaviour`. |
| `Node : MonoBehaviour` / `Block : Node` | Mixes graph metadata, serialized behavior, coroutine execution, and feedback. | `BlockDefinition`, authoring metadata, and plain `Block`. |
| `Command : MonoBehaviour` | Component execution unit and coroutine host. | `IAction` in a plain `ActionListDefinition`. |
| `InvokeActionCommand : Command` | Serialized nested action list and per-frame composite owner. | Plain `ActionListDefinition` and runtime action list. |
| `EventHandler : MonoBehaviour` | Component-owned trigger and block link. | Plain trigger definition/runtime; relay only for Unity callbacks. |
| `Variable : MonoBehaviour` | Component cell resolved through Blackboard/GameObject. | Typed definition and runtime cell in explicit stores. |
| `GlobalVariables` | Creates a hidden Blackboard GameObject through `ScaffoldManager.Instance`. | Injected global variable store. |

## Discoverable action implementations (273)

These are all C# files carrying `CommandInfoAttribute`, which is the authoritative
legacy add-menu/discoverability surface. Abstract helpers without `CommandInfo` are
listed separately after the action list.

- `Core/Actions/ScaffoldActions/Analytics/SendAnalyticsEvent.cs`
- `Core/Actions/ScaffoldActions/Animation/CrossfadeAnim.cs`
- `Core/Actions/ScaffoldActions/Animation/SetAnimSpeed.cs`
- `Core/Actions/ScaffoldActions/AssertCommand.cs`
- `Core/Actions/ScaffoldActions/Audio/BroAudioPlay.cs`
- `Core/Actions/ScaffoldActions/Audio/BroAudioSetVolume.cs`
- `Core/Actions/ScaffoldActions/Audio/BroAudioStop.cs`
- `Core/Actions/ScaffoldActions/Break.cs`
- `Core/Actions/ScaffoldActions/Call.cs`
- `Core/Actions/ScaffoldActions/CallStringMethod.cs`
- `Core/Actions/ScaffoldActions/Camera/CameraZoom.cs`
- `Core/Actions/ScaffoldActions/Camera/FlashScreen.cs`
- `Core/Actions/ScaffoldActions/ClearMenu.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandAdd.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandAddAll.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandClear.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandContains.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandContainsAll.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandContainsAny.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandCopy.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandCount.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandElement.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandExclusive.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandFind.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandInsert.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandIntersection.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandOccurrences.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandRemove.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandRemoveAllOf.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandRemoveAt.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandReserve.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandResize.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandReverse.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandShuffle.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandSort.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionCommandUnique.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionRandom.cs`
- `Core/Actions/ScaffoldActions/Collection/CollectionRandomBag.cs`
- `Core/Actions/ScaffoldActions/Collection/ForEach.cs`
- `Core/Actions/ScaffoldActions/Collection/GameObjectFind.cs`
- `Core/Actions/ScaffoldActions/Collection/Physics2DCast.cs`
- `Core/Actions/ScaffoldActions/Collection/Physics2DOverlap.cs`
- `Core/Actions/ScaffoldActions/Collection/PhysicsCast.cs`
- `Core/Actions/ScaffoldActions/Collection/PhysicsOverlap.cs`
- `Core/Actions/ScaffoldActions/Comment.cs`
- `Core/Actions/ScaffoldActions/ControlStage.cs`
- `Core/Actions/ScaffoldActions/Conversation.cs`
- `Core/Actions/ScaffoldActions/DebugBreak.cs`
- `Core/Actions/ScaffoldActions/DebugLog.cs`
- `Core/Actions/ScaffoldActions/DeleteSaveKey.cs`
- `Core/Actions/ScaffoldActions/Destroy.cs`
- `Core/Actions/ScaffoldActions/DestroyOnLoad.cs`
- `Core/Actions/ScaffoldActions/Else.cs`
- `Core/Actions/ScaffoldActions/ElseIf.cs`
- `Core/Actions/ScaffoldActions/End.cs`
- `Core/Actions/ScaffoldActions/ExecuteLua.cs`
- `Core/Actions/ScaffoldActions/FadeScreen.cs`
- `Core/Actions/ScaffoldActions/FadeSprite.cs`
- `Core/Actions/ScaffoldActions/FadeToView.cs`
- `Core/Actions/ScaffoldActions/FadeUI.cs`
- `Core/Actions/ScaffoldActions/FromString.cs`
- `Core/Actions/ScaffoldActions/Fullscreen.cs`
- `Core/Actions/ScaffoldActions/GameObject/SetComponentEnabled.cs`
- `Core/Actions/ScaffoldActions/GetText.cs`
- `Core/Actions/ScaffoldActions/GetToggleState.cs`
- `Core/Actions/ScaffoldActions/If.cs`
- `Core/Actions/ScaffoldActions/Input/GetAxis.cs`
- `Core/Actions/ScaffoldActions/Input/GetKey.cs`
- `Core/Actions/ScaffoldActions/Input/GetMousePosition.cs`
- `Core/Actions/ScaffoldActions/InvokeEvent.cs`
- `Core/Actions/ScaffoldActions/InvokeMethod.cs`
- `Core/Actions/ScaffoldActions/Jump.cs`
- `Core/Actions/ScaffoldActions/Label.cs`
- `Core/Actions/ScaffoldActions/LeanTween/MoveLean.cs`
- `Core/Actions/ScaffoldActions/LeanTween/RotateLean.cs`
- `Core/Actions/ScaffoldActions/LeanTween/ScaleLean.cs`
- `Core/Actions/ScaffoldActions/LeanTween/StopTweensLean.cs`
- `Core/Actions/ScaffoldActions/LoadScene.cs`
- `Core/Actions/ScaffoldActions/LoadVariable.cs`
- `Core/Actions/ScaffoldActions/LookFrom.cs`
- `Core/Actions/ScaffoldActions/LookTo.cs`
- `Core/Actions/ScaffoldActions/LoopRange.cs`
- `Core/Actions/ScaffoldActions/LuaElseIf.cs`
- `Core/Actions/ScaffoldActions/LuaIf.cs`
- `Core/Actions/ScaffoldActions/Math/Abs.cs`
- `Core/Actions/ScaffoldActions/Math/Clamp.cs`
- `Core/Actions/ScaffoldActions/Math/Curve.cs`
- `Core/Actions/ScaffoldActions/Math/Exp.cs`
- `Core/Actions/ScaffoldActions/Math/Inv.cs`
- `Core/Actions/ScaffoldActions/Math/InvLerp.cs`
- `Core/Actions/ScaffoldActions/Math/Lerp.cs`
- `Core/Actions/ScaffoldActions/Math/Log.cs`
- `Core/Actions/ScaffoldActions/Math/Map.cs`
- `Core/Actions/ScaffoldActions/Math/MinMax.cs`
- `Core/Actions/ScaffoldActions/Math/Neg.cs`
- `Core/Actions/ScaffoldActions/Math/Pow.cs`
- `Core/Actions/ScaffoldActions/Math/Round.cs`
- `Core/Actions/ScaffoldActions/Math/Sign.cs`
- `Core/Actions/ScaffoldActions/Math/Sqrt.cs`
- `Core/Actions/ScaffoldActions/Math/ToInt.cs`
- `Core/Actions/ScaffoldActions/Math/Trig.cs`
- `Core/Actions/ScaffoldActions/Menu.cs`
- `Core/Actions/ScaffoldActions/MenuShuffle.cs`
- `Core/Actions/ScaffoldActions/MenuTimer.cs`
- `Core/Actions/ScaffoldActions/MoveAdd.cs`
- `Core/Actions/ScaffoldActions/MoveFrom.cs`
- `Core/Actions/ScaffoldActions/MoveTo.cs`
- `Core/Actions/ScaffoldActions/MoveToView.cs`
- `Core/Actions/ScaffoldActions/OpenURL.cs`
- `Core/Actions/ScaffoldActions/Particles/PlayParticles.cs`
- `Core/Actions/ScaffoldActions/Particles/SpawnParticles.cs`
- `Core/Actions/ScaffoldActions/PerformInterruption.cs`
- `Core/Actions/ScaffoldActions/Physics/Raycast.cs`
- `Core/Actions/ScaffoldActions/Physics/Raycast2D.cs`
- `Core/Actions/ScaffoldActions/PlayAnimState.cs`
- `Core/Actions/ScaffoldActions/PlayUsfxrSound.cs`
- `Core/Actions/ScaffoldActions/Portrait.cs`
- `Core/Actions/ScaffoldActions/Priority/ScaffoldPriorityCount.cs`
- `Core/Actions/ScaffoldActions/Priority/ScaffoldPriorityDecrease.cs`
- `Core/Actions/ScaffoldActions/Priority/ScaffoldPriorityIncrease.cs`
- `Core/Actions/ScaffoldActions/Priority/ScaffoldPriorityReset.cs`
- `Core/Actions/ScaffoldActions/Property/AnimatorProperty.cs`
- `Core/Actions/ScaffoldActions/Property/CollectionProperty.cs`
- `Core/Actions/ScaffoldActions/Property/Collider2DProperty.cs`
- `Core/Actions/ScaffoldActions/Property/ColliderProperty.cs`
- `Core/Actions/ScaffoldActions/Property/Collision2DProperty.cs`
- `Core/Actions/ScaffoldActions/Property/CollisionProperty.cs`
- `Core/Actions/ScaffoldActions/Property/ColorProperty.cs`
- `Core/Actions/ScaffoldActions/Property/ControllerColliderHitProperty.cs`
- `Core/Actions/ScaffoldActions/Property/GameObjectProperty.cs`
- `Core/Actions/ScaffoldActions/Property/MaterialProperty.cs`
- `Core/Actions/ScaffoldActions/Property/Matrix4x4Property.cs`
- `Core/Actions/ScaffoldActions/Property/QuaternionProperty.cs`
- `Core/Actions/ScaffoldActions/Property/Rigidbody2DProperty.cs`
- `Core/Actions/ScaffoldActions/Property/RigidbodyProperty.cs`
- `Core/Actions/ScaffoldActions/Property/SpriteProperty.cs`
- `Core/Actions/ScaffoldActions/Property/TextureProperty.cs`
- `Core/Actions/ScaffoldActions/Property/TransformProperty.cs`
- `Core/Actions/ScaffoldActions/Property/Vector2Property.cs`
- `Core/Actions/ScaffoldActions/Property/Vector3Property.cs`
- `Core/Actions/ScaffoldActions/Property/Vector4Property.cs`
- `Core/Actions/ScaffoldActions/PunchPosition.cs`
- `Core/Actions/ScaffoldActions/PunchRotation.cs`
- `Core/Actions/ScaffoldActions/PunchScale.cs`
- `Core/Actions/ScaffoldActions/Quit.cs`
- `Core/Actions/ScaffoldActions/RandomFloat.cs`
- `Core/Actions/ScaffoldActions/RandomInteger.cs`
- `Core/Actions/ScaffoldActions/ReadTextFile.cs`
- `Core/Actions/ScaffoldActions/Renderers/Blink.cs`
- `Core/Actions/ScaffoldActions/Renderers/PlayVideo.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetFog.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetGlobalShader.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetLight.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetMaterial.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetShaderProperty.cs`
- `Core/Actions/ScaffoldActions/Renderers/SetSkybox.cs`
- `Core/Actions/ScaffoldActions/Reset.cs`
- `Core/Actions/ScaffoldActions/ResetAnimTrigger.cs`
- `Core/Actions/ScaffoldActions/ReturnActionStatus.cs`
- `Core/Actions/ScaffoldActions/Rigidbody/AddForce.cs`
- `Core/Actions/ScaffoldActions/Rigidbody/AddTorque.cs`
- `Core/Actions/ScaffoldActions/Rigidbody/SetVelocity.cs`
- `Core/Actions/ScaffoldActions/Rigidbody2D/AddForce2D.cs`
- `Core/Actions/ScaffoldActions/Rigidbody2D/AddTorque2D.cs`
- `Core/Actions/ScaffoldActions/Rigidbody2D/StopMotionRigidBody2D.cs`
- `Core/Actions/ScaffoldActions/RotateAdd.cs`
- `Core/Actions/ScaffoldActions/RotateFrom.cs`
- `Core/Actions/ScaffoldActions/RotateTo.cs`
- `Core/Actions/ScaffoldActions/SavePoint.cs`
- `Core/Actions/ScaffoldActions/SaveVariable.cs`
- `Core/Actions/ScaffoldActions/Say.cs`
- `Core/Actions/ScaffoldActions/ScaleAdd.cs`
- `Core/Actions/ScaffoldActions/ScaleFrom.cs`
- `Core/Actions/ScaffoldActions/ScaleTo.cs`
- `Core/Actions/ScaffoldActions/Scene/ReloadScene.cs`
- `Core/Actions/ScaffoldActions/Scene/UnloadScene.cs`
- `Core/Actions/ScaffoldActions/SendMessage.cs`
- `Core/Actions/ScaffoldActions/SetActive.cs`
- `Core/Actions/ScaffoldActions/SetAnimBool.cs`
- `Core/Actions/ScaffoldActions/SetAnimFloat.cs`
- `Core/Actions/ScaffoldActions/SetAnimInteger.cs`
- `Core/Actions/ScaffoldActions/SetAnimTrigger.cs`
- `Core/Actions/ScaffoldActions/SetAudioPitch.cs`
- `Core/Actions/ScaffoldActions/SetAudioVolume.cs`
- `Core/Actions/ScaffoldActions/SetClickable2D.cs`
- `Core/Actions/ScaffoldActions/SetCollider.cs`
- `Core/Actions/ScaffoldActions/SetDraggable2D.cs`
- `Core/Actions/ScaffoldActions/SetInteractable.cs`
- `Core/Actions/ScaffoldActions/SetLanguage.cs`
- `Core/Actions/ScaffoldActions/SetLayerOrder.cs`
- `Core/Actions/ScaffoldActions/SetMenuDialog.cs`
- `Core/Actions/ScaffoldActions/SetMouseCursor.cs`
- `Core/Actions/ScaffoldActions/SetSaveProfile.cs`
- `Core/Actions/ScaffoldActions/SetSayDialog.cs`
- `Core/Actions/ScaffoldActions/SetSliderValue.cs`
- `Core/Actions/ScaffoldActions/SetSprite.cs`
- `Core/Actions/ScaffoldActions/SetSpriteOrder.cs`
- `Core/Actions/ScaffoldActions/SetText.cs`
- `Core/Actions/ScaffoldActions/SetToggleState.cs`
- `Core/Actions/ScaffoldActions/SetUIImage.cs`
- `Core/Actions/ScaffoldActions/SetVariable.cs`
- `Core/Actions/ScaffoldActions/ShakeCamera.cs`
- `Core/Actions/ScaffoldActions/ShakePosition.cs`
- `Core/Actions/ScaffoldActions/ShakeRotation.cs`
- `Core/Actions/ScaffoldActions/ShakeScale.cs`
- `Core/Actions/ScaffoldActions/ShowSprite.cs`
- `Core/Actions/ScaffoldActions/SpawnObject.cs`
- `Core/Actions/ScaffoldActions/Sprite/PlaySpriteSheet.cs`
- `Core/Actions/ScaffoldActions/StartSwipe.cs`
- `Core/Actions/ScaffoldActions/Stop.cs`
- `Core/Actions/ScaffoldActions/StopAmbiance.cs`
- `Core/Actions/ScaffoldActions/StopBlackboard.cs`
- `Core/Actions/ScaffoldActions/StopBlock.cs`
- `Core/Actions/ScaffoldActions/StopMusic.cs`
- `Core/Actions/ScaffoldActions/StopSwipe.cs`
- `Core/Actions/ScaffoldActions/StopTween.cs`
- `Core/Actions/ScaffoldActions/StopTweens.cs`
- `Core/Actions/ScaffoldActions/Tags/TagEvent.cs`
- `Core/Actions/ScaffoldActions/ThrowException.cs`
- `Core/Actions/ScaffoldActions/Time/FreezeFrame.cs`
- `Core/Actions/ScaffoldActions/Time/SetTimeScale.cs`
- `Core/Actions/ScaffoldActions/ToString.cs`
- `Core/Actions/ScaffoldActions/Transform/AutoRotate.cs`
- `Core/Actions/ScaffoldActions/Transform/Billboard.cs`
- `Core/Actions/ScaffoldActions/Transform/FollowTarget.cs`
- `Core/Actions/ScaffoldActions/Transform/GetPosition.cs`
- `Core/Actions/ScaffoldActions/Transform/GetRotation.cs`
- `Core/Actions/ScaffoldActions/Transform/LookAt.cs`
- `Core/Actions/ScaffoldActions/Transform/MatchTransform.cs`
- `Core/Actions/ScaffoldActions/Transform/PulseScale.cs`
- `Core/Actions/ScaffoldActions/Transform/RotateAround.cs`
- `Core/Actions/ScaffoldActions/Transform/SetParent.cs`
- `Core/Actions/ScaffoldActions/Transform/SetPosition.cs`
- `Core/Actions/ScaffoldActions/Transform/SetRotation.cs`
- `Core/Actions/ScaffoldActions/Transform/SetScale.cs`
- `Core/Actions/ScaffoldActions/Transform/SquashAndStretch.cs`
- `Core/Actions/ScaffoldActions/Transform/Wiggle.cs`
- `Core/Actions/ScaffoldActions/Tutorial/ClearUIFocus.cs`
- `Core/Actions/ScaffoldActions/Tutorial/ShowUIFocus.cs`
- `Core/Actions/ScaffoldActions/UI/CountTo.cs`
- `Core/Actions/ScaffoldActions/UI/CrossfadeGraphic.cs`
- `Core/Actions/ScaffoldActions/UI/FadeText.cs`
- `Core/Actions/ScaffoldActions/UI/SetCanvasGroupRaycast.cs`
- `Core/Actions/ScaffoldActions/UI/SetFontSize.cs`
- `Core/Actions/ScaffoldActions/UI/SetGraphicColor.cs`
- `Core/Actions/ScaffoldActions/UI/SetRaycastTarget.cs`
- `Core/Actions/ScaffoldActions/UI/SetTextColor.cs`
- `Core/Actions/ScaffoldActions/UI/TriggerFader.cs`
- `Core/Actions/ScaffoldActions/UI/UpdateProgressBar.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ApplyUIEffectPreset.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ApplyUILoopMaterial.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ClearAllUIEffects.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ClearUIEffect.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ClearUIEffectsByTarget.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ClearUILoopMaterial.cs`
- `Core/Actions/ScaffoldActions/UIEffects/ControlUIEffectTweener.cs`
- `Core/Actions/ScaffoldActions/UIEffects/CycleUIEffectPreset.cs`
- `Core/Actions/ScaffoldActions/UIEffects/SetUIEffectEnabled.cs`
- `Core/Actions/ScaffoldActions/UIEffects/SetUIEffectIntensity.cs`
- `Core/Actions/ScaffoldActions/Vector3/Vector3Arithmetic.cs`
- `Core/Actions/ScaffoldActions/Vector3/Vector3Fields.cs`
- `Core/Actions/ScaffoldActions/Vector3/Vector3Normalise.cs`
- `Core/Actions/ScaffoldActions/Vector3/Vector3ToVector2.cs`
- `Core/Actions/ScaffoldActions/Wait.cs`
- `Core/Actions/ScaffoldActions/WaitFrames.cs`
- `Core/Actions/ScaffoldActions/While.cs`
- `Core/Actions/ScaffoldActions/Write.cs`
- `Presentation/UI/Tags/Input/InvokeActionCommand.cs`
- `Presentation/UI/Tags/Input/PushToScaffoldVariable.cs`
- `Presentation/UI/Tags/Input/WaitForTargetClickAction.cs`
- `Presentation/UI/Tags/Input/WaitForTargetDropAction.cs`
- `Presentation/UI/Tags/Input/WaitForTargetDropAtIndexAction.cs`
- `Presentation/UI/Tags/Input/WaitForTargetPointerEnterAction.cs`

Supporting action contracts and bases that must be replaced or adapted:
`IAction`, `IActionWithStatus`, `IActionProgressProvider`,
`IInterruptibleAction`, `ActionBase`, `IMonoBehaviourConsumer`,
`IBlackboardConsumer`, `ICommandContextConsumer`, `ActionWrapper`,
`InvokeActionCompositeTask`, `WaitForInputActionBase`,
`UIEffectActionBase`, `CollectionBaseCommand`,
`BaseUnaryMathCommand`, `BaseLeanTweenCommand`, and `iTweenCommand`.

Migration batches are fixed as:

1. Pure, variable, collection, math, status, and flow actions.
2. Actions with explicit Unity references or synchronous Unity API access.
3. Scheduled, tween, input-wait, dialog, scene, save, analytics, and other
   service-backed actions.

## Variable component types (29)

- `AnimatorVariable.cs`
- `AudioClipVariable.cs`
- `AudioSourceVariable.cs`
- `BooleanVariable.cs`
- `ButtonVariable.cs`
- `CharacterVariable.cs`
- `CollectionVariable.cs`
- `Collider2DVariable.cs`
- `ColliderVariable.cs`
- `Collision2DVariable.cs`
- `CollisionVariable.cs`
- `ColorVariable.cs`
- `ControllerColliderHitVariable.cs`
- `FloatVariable.cs`
- `GameObjectVariable.cs`
- `IntegerVariable.cs`
- `MaterialVariable.cs`
- `Matrix4x4Variable.cs`
- `ObjectVariable.cs`
- `QuaternionVariable.cs`
- `Rigidbody2DVariable.cs`
- `RigidbodyVariable.cs`
- `SpriteVariable.cs`
- `StringVariable.cs`
- `TextureVariable.cs`
- `TransformVariable.cs`
- `Vector2Variable.cs`
- `Vector3Variable.cs`
- `Vector4Variable.cs`

In addition to the typed component cells, the migration covers `VariableDataSource`,
all `*Data` and `*DataMulti` wrappers, `VariableValueReference`,
`VariableReference`, `AnyVariableAndDataPair`, collection values, and all
`VariableValueSO<T>` asset types. Unity object value types remain supported as
explicit references.

## Trigger and event-handler source files (28)

- `BlackboardEnabled.cs`
- `ButtonClicked.cs`
- `Drag/DragCancelled.cs`
- `Drag/DragCompleted.cs`
- `Drag/DragEntered.cs`
- `Drag/DragExited.cs`
- `Drag/DragStarted.cs`
- `EndEdit.cs`
- `GameStarted.cs`
- `KeyPressed.cs`
- `MessageReceived.cs`
- `MonoBehaviour/AnimatorState.cs`
- `MonoBehaviour/ApplicationState.cs`
- `MonoBehaviour/BasePhysicsEventHandler.cs`
- `MonoBehaviour/CharacterControllerCollide.cs`
- `MonoBehaviour/Collision.cs`
- `MonoBehaviour/Collision2D.cs`
- `MonoBehaviour/Mouse.cs`
- `MonoBehaviour/Particle.cs`
- `MonoBehaviour/Render.cs`
- `MonoBehaviour/TagFilteredEventHandler.cs`
- `MonoBehaviour/TransformChanged.cs`
- `MonoBehaviour/Trigger.cs`
- `MonoBehaviour/Trigger2D.cs`
- `MonoBehaviour/UpdateTick.cs`
- `ObjectClicked.cs`
- `SavePointLoaded.cs`
- `ToggleChanged.cs`

Classification:

- Plain lifecycle/message triggers: BlackboardEnabled, GameStarted, MessageReceived,
  and SavePointLoaded.
- Bindable UI triggers: ButtonClicked, EndEdit, ToggleChanged, ObjectClicked, and drag
  event subscriptions.
- Polled triggers: KeyPressed and UpdateTick.
- Mandatory Unity callback relays: AnimatorState, ApplicationState,
  CharacterControllerCollide, Collision, Collision2D, Mouse, Particle, Render,
  TransformChanged, Trigger, and Trigger2D.
- Filtering, repeat policy, variable writes, and block selection move into plain
  trigger logic even when a relay is required.

## Authoring/editor surface (50 source files)

- `AnyVariableAndDataPairDrawer.cs`
- `BlackboardEditor.cs`
- `BlackboardMenuItems.cs`
- `BlackboardWindow.cs`
- `BlockEditor.cs`
- `BlockInspector.cs`
- `BlockInspectorStyleSheet.cs`
- `BlockReferenceDrawer.cs`
- `CommandEditor.cs`
- `CommandListAdaptor.cs`
- `CustomVariableDrawerLookup.cs`
- `EventHandlerEditor.cs`
- `EventWindow.cs`
- `GenerateVariableHelper.cs`
- `GenerateVariableWindow.cs`
- `InvokeActionEditorSelection.cs`
- `InvokeActionEditorUtility.cs`
- `PopupContent/CommandSelectorPopupWindowContent.cs`
- `PopupContent/EventSelectorPopupWindowContent.cs`
- `PopupContent/VariableSelectPopupWindowContent.cs`
- `VariableEditor.cs`
- `VariableListAdaptor.cs`
- `VariableReferenceDrawer.cs`
- `VariableTypes/AnimatorVariableDrawer.cs`
- `VariableTypes/AudioSourceVariableDrawer.cs`
- `VariableTypes/BooleanVariableDrawer.cs`
- `VariableTypes/ButtonVariableDrawer.cs`
- `VariableTypes/CharacterVariableDrawer.cs`
- `VariableTypes/CollectionVariableDrawer.cs`
- `VariableTypes/Collider2DVariableDrawer.cs`
- `VariableTypes/ColliderVariableDrawer.cs`
- `VariableTypes/ColorVariableDrawer.cs`
- `VariableTypes/FloatVariableDrawer.cs`
- `VariableTypes/GameObjectVariableDrawer.cs`
- `VariableTypes/IntegerVariableDrawer.cs`
- `VariableTypes/MaterialVariableDrawer.cs`
- `VariableTypes/Matrix4x4VariableDrawer.cs`
- `VariableTypes/ObjectVariableDrawer.cs`
- `VariableTypes/QuaternionVariableDrawer.cs`
- `VariableTypes/Rigidbody2DVariableDrawer.cs`
- `VariableTypes/RigidbodyVariableDrawer.cs`
- `VariableTypes/SpriteVariableDrawer.cs`
- `VariableTypes/StringDataMultiDrawer.cs`
- `VariableTypes/StringVariableDrawer.cs`
- `VariableTypes/TextureVariableDrawer.cs`
- `VariableTypes/TransformVariableDrawer.cs`
- `VariableTypes/VariableDataDrawer.cs`
- `VariableTypes/Vector2VariableDrawer.cs`
- `VariableTypes/Vector3VariableDrawer.cs`
- `VariableTypes/Vector4VariableDrawer.cs`

The rewritten editor must preserve these operations: create/open a Blackboard;
Direct/asset/variable source switching; add/remove/duplicate/reorder Blocks, tracks,
action lists, actions, triggers, and variables; group and ungroup; cross-track moves;
copy/paste; Undo/Redo; selection and navigation; search and filtering; graph
position/zoom/scroll; tint; variable pickers; validation summaries; execution status
and progress feedback; automatic action metadata synchronization; and editor
duplication with regenerated definition IDs.

## Serialized asset references

The initial script-GUID scan found:

| Legacy script | Serialized assets |
| --- | --- |
| Blackboard (`7a334fe2ffb574b3583ff3b18b4792d3`) | `Assets/3rdParty/ScaffoldVisualScripting/Resources/Prefabs/Blackboard.prefab`; `Assets/GearEngine/Scenes/Test/Test Tutorial Scene.unity`; `Assets/GearEngine/Scenes/Test/UIEffectsForEachDemo.unity` |
| Block (`3d3d73aef2cfc4f51abf34ac00241f60`) | The same prefab and two scenes. |
| GameStarted (`d2f6487d21a03404cb21b245f0242e79`) | Blackboard prefab and Test Tutorial Scene. |
| InvokeActionCommand | Test Tutorial Scene and UIEffectsForEachDemo; the concrete GUID is retained in the cutover scan manifest. |
| Typed variables and concrete event handlers | Serialized by their concrete script GUIDs inside the same affected Blackboard assets; the final recursive GUID scan must be empty before deleting legacy scripts. |

Other serialized references that must be updated with the scene rename include build
settings, scene lists, editor builders, tests, documentation, and any string-based
scene lookup. ScriptableObject value assets are data references, not legacy component
hosts, and are retained unless their consumers are replaced.

## Unity lifecycle and callback dependencies (69 declarations)

- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Blackboard.cs:117:        protected virtual void Start()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Blackboard.cs:151:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Blackboard.cs:169:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Block.cs:110:        protected virtual void Awake()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Block.cs:177:        protected virtual void Update()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/Components/Variable.cs:244:        protected virtual void Start()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/BlackboardEnabled.cs:15:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/ButtonClicked.cs:21:        public virtual void Start()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragCancelled.cs:34:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragCancelled.cs:41:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragCompleted.cs:48:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragCompleted.cs:65:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragEntered.cs:44:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragEntered.cs:54:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragExited.cs:44:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragExited.cs:54:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragStarted.cs:33:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/Drag/DragStarted.cs:40:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/EndEdit.cs:19:        protected virtual void Start()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/GameStarted.cs:19:        protected virtual void Start()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/KeyPressed.cs:34:        protected virtual void Update()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MessageReceived.cs:20:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MessageReceived.cs:25:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/AnimatorState.cs:34:        private void OnAnimatorIK(int layer)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/AnimatorState.cs:43:        private void OnAnimatorMove()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/ApplicationState.cs:33:        private void OnApplicationFocus(bool focus)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/ApplicationState.cs:45:        private void OnApplicationPause(bool pause)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/ApplicationState.cs:57:        private void OnApplicationQuit()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/CharacterControllerCollide.cs:21:        private void OnControllerColliderHit(ControllerColliderHit hit)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision.cs:21:        private void OnCollisionEnter(UnityEngine.Collision collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision.cs:26:        private void OnCollisionStay(UnityEngine.Collision collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision.cs:31:        private void OnCollisionExit(UnityEngine.Collision collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision2D.cs:20:        private void OnCollisionEnter2D(UnityEngine.Collision2D collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision2D.cs:25:        private void OnCollisionStay2D(UnityEngine.Collision2D collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Collision2D.cs:30:        private void OnCollisionExit2D(UnityEngine.Collision2D collision)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:35:        private void OnMouseDown()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:40:        private void OnMouseDrag()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:45:        private void OnMouseEnter()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:50:        private void OnMouseExit()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:55:        private void OnMouseOver()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:60:        private void OnMouseUp()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Mouse.cs:65:        private void OnMouseUpAsButton()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Particle.cs:35:        private void OnParticleCollision(GameObject other)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Particle.cs:51:        private void OnParticleTrigger()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Render.cs:60:        private void OnRenderImage(RenderTexture source, RenderTexture destination)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Render.cs:65:        private void OnRenderObject()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Render.cs:81:        private void OnBecameInvisible()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Render.cs:89:        private void OnBecameVisible()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/TransformChanged.cs:30:        private void OnTransformChildrenChanged()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/TransformChanged.cs:38:        private void OnTransformParentChanged()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger.cs:20:        private void OnTriggerEnter(Collider col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger.cs:25:        private void OnTriggerStay(Collider col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger.cs:30:        private void OnTriggerExit(Collider col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger2D.cs:19:        private void OnTriggerEnter2D(Collider2D col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger2D.cs:24:        private void OnTriggerStay2D(Collider2D col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/Trigger2D.cs:29:        private void OnTriggerExit2D(Collider2D col)`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/UpdateTick.cs:28:        private void Update()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/UpdateTick.cs:36:        private void FixedUpdate()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/MonoBehaviour/UpdateTick.cs:44:        private void LateUpdate()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/ObjectClicked.cs:33:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/ObjectClicked.cs:40:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/SavePointLoaded.cs:17:        protected virtual void OnEnable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/SavePointLoaded.cs:22:        protected virtual void OnDisable()`
- `Assets/3rdParty/ScaffoldVisualScripting/Scripts/EventHandlers/ToggleChanged.cs:25:        public virtual void Start()`
- `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input/InvokeActionCommand.cs:484:        private void Update()`
- `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input/TargetClickRelay.cs:32:        public void OnPointerClick(PointerEventData eventData)`
- `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input/TargetClickRelay.cs:37:        public void OnPointerDown(PointerEventData eventData)`
- `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input/WaitForTargetDropAtIndexAction.cs:121:        private void OnPointerExit(ScreenPointerExitEvent _)`
- `Assets/GearEngine/Scripts/Game/GearEngine/Presentation/UI/Tags/Input/WaitForTargetPointerEnterAction.cs:40:        private void OnPointerEnter(ScreenPointerEnterEvent signal)`

Only callbacks Unity must deliver directly remain in components. Blackboard lifecycle
callbacks move to `BlackboardBehaviour`; Block and action-list `Update` calls move
to explicit `Tick`; GameStarted and wait-frame coroutines move to the scheduler;
subscriptions must attach and detach symmetrically.

## Behavior-parity matrix

| Behavior | Legacy source | Characterization evidence | Plain-runtime acceptance |
| --- | --- | --- | --- |
| Sequence and ordered flow | Block + CompositeExecutionRunner | `BlockTrackExecutionTests.SequentialCommands_StillExecuteInOrder` | Pure NUnit executes identical order. |
| If/Else/End and jumps | Block continuation indexes | `BlockTrackExecutionTests.IfElseEnd_WorksInsideNonPrimaryTrack` and Invoke Action flow tests | Stable IDs route within the owning action list/track. |
| Parallel and multi-track | Block + CommandTrack | `MultipleTracks_ExecuteInParallel_WithWaitAll` and `MultipleTracks_ExecuteInSequence_WhenConfigured` | Plain tracks retain launch and await semantics. |
| Selector / Parallel Selector | CompositeExecutionRunner | Selector and Parallel Selector tests in `BlockTrackExecutionTests` and `InvokeActionCommandTests` | Status and early-completion rules remain unchanged. |
| Utility reevaluation | InvokeActionCommand Update | Utility tests in `InvokeActionCommandTests` | `Tick` plus injected time/scheduler drives reevaluation. |
| Random, Shuffle, weights, repeat guard | CompositeExecutionRunner metadata | Random/Shuffle/repeat tests in Block and Invoke Action suites | Definition metadata clones and executes identically. |
| Interruption | InvokeActionCommand generations and interruptible actions | Perform Interruption and stop tests in `InvokeActionCommandTests` | Late completions cannot resume stopped execution. |
| Variable reads/writes | Blackboard component variables | `BlackboardVariableTests`; Milestone 1 variable isolation characterization | Plain cells preserve type, operator, and reference semantics. |
| Messages | Static/Blackboard message dispatch + MessageReceived | Milestone 1 message characterization | Injected event bus dispatches without static state. |
| Startup timing | GameStarted Start coroutine | `EventHandlerTests.GameStarted_YieldsConfiguredFramesBeforeExecutingBlock` | Fake scheduler reproduces timing without Unity lifecycle. |
| Stop/reset | Blackboard and Block component state | Existing Block/Invoke Action stop tests | Plain runtime resets transient state and subscriptions. |
| Execution feedback | Component/editor status state | Existing Invoke Action editor selection/feedback tests | Runtime snapshots are separate from authoring metadata. |

## Baseline acceptance

Milestone 1 is complete when this inventory and the ExecPlan are committed together
with focused characterization tests and their passing NUnit XML, Editor log summary,
and Report.md. Later milestones may update the matrix, but no listed source surface may
silently disappear.
