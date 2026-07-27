using System.Reflection;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class BoardGearAnimatorTests
    {
        private sealed class FakeGridNode : IGridNode
        {
            public Vector2Int Position { get; set; }

            public float CurrentRotation { get; set; }

            public GearItemData ConfigData { get; set; } = new GearItemData { MaxCharge = 0f };

            public float LocalSpeedMultiplier { get; set; }

            public bool IsActive { get; set; } = true;

            public bool IsInteractable => true;

            public IEventBus EventBus => null!;

            public void Dispose()
            {
            }

            public void SetPosition(Vector2Int position)
            {
                Position = position;
            }

            public void AddAbility(GearAbilitySO ability, float duration = -1f)
            {
            }

            public void RemoveAbility(GearAbilitySO ability)
            {
            }

            public System.Collections.Generic.IEnumerable<GearAbilitySO> GetAbilities() => new System.Collections.Generic.List<GearAbilitySO>(); public void Initialize(Vector2Int position, GearItemData configData)
            {
            }

            public void NodeUpdate(float deltaTime, float speedModifier)
            {
            }

            public void WindDownUpdate(float deltaTime, float speedModifier)
            {
            }

            public void ResetSimulationState()
            {
            }
        }

        private static void InvokeAnimatorUpdate(BoardGearAnimator animator)
        {
            MethodInfo update = typeof(BoardGearAnimator).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(update);
            update.Invoke(animator, null);
        }

        private static void InvokeGearViewUpdate(GearView view)
        {
            MethodInfo update = typeof(GearView).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(update);
            update.Invoke(view, null);
        }

        [Test]
        public void Track_OffsetsOnlyCoreWithoutChangingAdjacentPhase()
        {
            BoardLayoutSO layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            layout.StaggeredRotationOffset = 17f;

            Transform slot0 = new GameObject("Slot0").transform;
            Transform slot1 = new GameObject("Slot1").transform;
            Transform slot2 = new GameObject("Slot2").transform;
            Transform GetSlot(Vector2Int p)
            {
                if (p == Vector2Int.zero)
                {
                    return slot0;
                }

                if (p == new Vector2Int(1, 0))
                {
                    return slot1;
                }

                return slot2;
            }

            GameObject host = new GameObject("AnimatorHost");
            BoardGearAnimator animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(GetSlot, layout, "motor");

            GameObject motorRoot = new GameObject("MotorRoot");
            GameObject motorVisual = new GameObject("MotorVisual");
            motorVisual.transform.SetParent(motorRoot.transform, false);
            GearView motorGearView = motorRoot.AddComponent<GearView>();
            motorGearView.WireTestReferences(motorVisual.transform);
            motorGearView.ApplyConfig(new GearItemData
            {
                RelativeScaleMultiplier = 1f,
                InitialRotationOffset = 30f,
                Id = "motor",
            });

            GameObject gearRoot = new GameObject("GearRoot");
            GameObject gearVisual = new GameObject("GearVisual");
            gearVisual.transform.SetParent(gearRoot.transform, false);
            GearView gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisual.transform);
            gearView.ApplyConfig(new GearItemData { RelativeScaleMultiplier = 1f, Id = "other" });

            FakeGridNode motorNode = new FakeGridNode
            {
                Position = new Vector2Int(1, 0),
                CurrentRotation = 0f,
                ConfigData = new GearItemData
                {
                    Id = "motor",
                    InitialRotationOffset = 30f,
                    MaxCharge = 0f,
                },
            };

            FakeGridNode node = new FakeGridNode
            {
                Position = Vector2Int.zero,
                CurrentRotation = 0f,
                ConfigData = new GearItemData { Id = "other", MaxCharge = 0f },
            };

            animator.Track(motorNode, motorGearView);
            animator.Track(node, gearView);

            for (int i = 0; i < 30; i++)
            {
                InvokeAnimatorUpdate(animator);
                InvokeGearViewUpdate(gearView);
                InvokeGearViewUpdate(motorGearView);
            }

            float coreRotation = motorVisual.transform.localEulerAngles.z;
            float adjacentRotation = gearVisual.transform.localEulerAngles.z;
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(coreRotation, 30f)), 5f);
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(adjacentRotation, 17f)), 5f);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(gearRoot);
            Object.DestroyImmediate(motorRoot);
            Object.DestroyImmediate(slot0.gameObject);
            Object.DestroyImmediate(slot1.gameObject);
            Object.DestroyImmediate(slot2.gameObject);
            Object.DestroyImmediate(layout);
        }

        [Test]
        public void Untrack_ThenUpdate_DoesNotThrow()
        {
            BoardLayoutSO layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            GameObject host = new GameObject("AnimatorHost");
            BoardGearAnimator animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(_ => new GameObject("S").transform, layout, null);

            GameObject gearRoot = new GameObject("GearRoot");
            GameObject gv = new GameObject("GV");
            gv.transform.SetParent(gearRoot.transform, false);
            GearView gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gv.transform);

            FakeGridNode node = new FakeGridNode();
            animator.Track(node, gearView);
            animator.Untrack(node);

            Assert.DoesNotThrow(() => InvokeAnimatorUpdate(animator));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(gearRoot);
            Object.DestroyImmediate(layout);
        }

        [Test]
        public void Clear_RemovesTrackedEntries()
        {
            BoardLayoutSO layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            GameObject host = new GameObject("AnimatorHost");
            BoardGearAnimator animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(_ => new GameObject("S").transform, layout, null);

            GameObject gearRoot = new GameObject("GearRoot");
            GameObject gv = new GameObject("GV");
            gv.transform.SetParent(gearRoot.transform, false);
            GearView gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gv.transform);

            animator.Track(new FakeGridNode(), gearView);
            animator.Clear();

            Assert.DoesNotThrow(() => InvokeAnimatorUpdate(animator));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(gearRoot);
            Object.DestroyImmediate(layout);
        }

    }
}
