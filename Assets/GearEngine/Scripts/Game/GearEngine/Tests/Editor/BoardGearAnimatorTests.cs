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

            public GearConfigData ConfigData { get; set; } = new GearConfigData { MaxCharge = 0f };

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

            public void Initialize(Vector2Int position, GearConfigData configData)
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
        public void Track_DrivesRotationTowardStaggeredTarget()
        {
            var layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            layout.StaggeredRotationOffset = 30f;

            var slot0 = new GameObject("Slot0").transform;
            var slot1 = new GameObject("Slot1").transform;
            Transform GetSlot(Vector2Int p) => p == Vector2Int.zero ? slot0 : slot1;

            var host = new GameObject("AnimatorHost");
            var animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(GetSlot, layout);

            var gearRoot = new GameObject("GearRoot");
            var gearVisual = new GameObject("GearVisual");
            gearVisual.transform.SetParent(gearRoot.transform, false);
            var gearView = gearRoot.AddComponent<GearView>();
            gearView.WireTestReferences(gearVisual.transform);
            gearView.ApplyConfig(new GearConfigData { RelativeScaleMultiplier = 1f });

            var node = new FakeGridNode
            {
                Position = Vector2Int.zero,
                CurrentRotation = 0f,
            };

            animator.Track(node, gearView);

            for (int i = 0; i < 30; i++)
            {
                InvokeAnimatorUpdate(animator);
                InvokeGearViewUpdate(gearView);
            }

            float z = gearVisual.transform.localEulerAngles.z;
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(z, 30f)), 5f);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(gearRoot);
            Object.DestroyImmediate(slot0.gameObject);
            Object.DestroyImmediate(slot1.gameObject);
            Object.DestroyImmediate(layout);
        }

        [Test]
        public void Untrack_ThenUpdate_DoesNotThrow()
        {
            var layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            var host = new GameObject("AnimatorHost");
            var animator = host.AddComponent<BoardGearAnimator>();
            animator.Configure(_ => new GameObject("S").transform, layout);

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
            animator.Configure(_ => new GameObject("S").transform, layout);

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
