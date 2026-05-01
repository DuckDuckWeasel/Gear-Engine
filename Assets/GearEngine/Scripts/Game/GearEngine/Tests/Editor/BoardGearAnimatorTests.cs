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
        public void Track_DrivesRotationTowardStaggeredTarget_FromMotorDistance()
        {
            var layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            layout.StaggeredRotationOffset = 30f;

            var slot0 = new GameObject("Slot0").transform;
            var slot1 = new GameObject("Slot1").transform;
            var slot2 = new GameObject("Slot2").transform;
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

            var host = new GameObject("AnimatorHost");
            var animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(GetSlot, layout, "motor");

            var motorRoot = new GameObject("MotorRoot");
            var motorVisual = new GameObject("MotorVisual");
            motorVisual.transform.SetParent(motorRoot.transform, false);
            var motorGearView = motorRoot.AddComponent<GearView>();
            motorGearView.WireTestReferences(motorVisual.transform);
            motorGearView.ApplyConfig(new GearItemData { RelativeScaleMultiplier = 1f, Id = "motor" });

            var gearRoot = new GameObject("GearRoot");
            var gearVisual = new GameObject("GearVisual");
            gearVisual.transform.SetParent(gearRoot.transform, false);
            var gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisual.transform);
            gearView.ApplyConfig(new GearItemData { RelativeScaleMultiplier = 1f, Id = "other" });

            var motorNode = new FakeGridNode
            {
                Position = new Vector2Int(1, 0),
                CurrentRotation = 0f,
                ConfigData = new GearItemData { Id = "motor", MaxCharge = 0f },
            };

            var node = new FakeGridNode
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

            float z = gearVisual.transform.localEulerAngles.z;
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(z, 30f)), 5f);

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
            var layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            var host = new GameObject("AnimatorHost");
            var animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(_ => new GameObject("S").transform, layout, null);

            var gearRoot = new GameObject("GearRoot");
            var gv = new GameObject("GV");
            gv.transform.SetParent(gearRoot.transform, false);
            var gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gv.transform);

            var node = new FakeGridNode();
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
            var layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            var host = new GameObject("AnimatorHost");
            var animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(_ => new GameObject("S").transform, layout, null);

            var gearRoot = new GameObject("GearRoot");
            var gv = new GameObject("GV");
            gv.transform.SetParent(gearRoot.transform, false);
            var gearView = gearRoot.AddComponent<GearView>();
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
