using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OM;
using OM.Editor;
using OM.TimelineCreator.Editor;
using OM.TimelineCreator.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OM.TimelineCreator.Tests.Editor
{
    [TestFixture]
    public sealed class OM_TimelineInteractionTests
    {
        private TestEditorWindow window;
        private VisualElement root;

        [SetUp]
        public void SetUp()
        {
            window = ScriptableObject.CreateInstance<TestEditorWindow>();
            window.position = new Rect(100, 100, 640, 480);
            window.ShowUtility();
            root = window.rootVisualElement;
        }

        [TearDown]
        public void TearDown()
        {
            window.Close();
            UnityEngine.Object.DestroyImmediate(window);
        }

        [Test]
        public void NestedChildClick_RoutesToClosestInteractiveAncestor()
        {
            FakeTimeline timeline = CreateTimeline(2);
            OM_Track<FakeClip, FakeTrack>[] tracks = timeline.TracksList.OrderBy(track => track.GetTrackIndex()).ToArray();
            OM_Track<FakeClip, FakeTrack> firstTrack = tracks[0];
            OM_Track<FakeClip, FakeTrack> secondTrack = tracks[1];
            timeline.SelectTrack(secondTrack);
            Vector2 clickPosition = Vector2.one;

            SendMouseDown(firstTrack.TrackClip.Title, clickPosition);
            SendMouseUp(firstTrack.TrackClip.Title, clickPosition);

            Assert.That(timeline.SelectedTrack, Is.SameAs(firstTrack));
            Assert.That(firstTrack.IsSelected, Is.True);
            Assert.That(secondTrack.IsSelected, Is.False);
        }

        [Test]
        public void MouseDownWithoutThreshold_DoesNotStartDrag()
        {
            InteractiveElement interactive = AddInteractiveElement(out Label child);

            SendMouseDown(child, Vector2.one);

            Assert.That(interactive.StartDragCount, Is.Zero);
            Assert.That(interactive.DragCount, Is.Zero);
        }

        [Test]
        public void MovementBeyondThreshold_StartsDragAndSuppressesClick()
        {
            InteractiveElement interactive = AddInteractiveElement(out Label child);
            Vector2 dragPosition = new Vector2(10, 0);

            SendMouseDown(child, Vector2.zero);
            SendMouseMove(child, dragPosition);
            SendMouseUp(child, dragPosition);

            Assert.That(interactive.StartDragCount, Is.EqualTo(1));
            Assert.That(interactive.DragCount, Is.EqualTo(1));
            Assert.That(interactive.EndDragCount, Is.EqualTo(1));
            Assert.That(interactive.ClickCount, Is.Zero);
        }

        [Test]
        public void ClickingSelectedTrack_KeepsSelectionUntilBackgroundClick()
        {
            FakeTimeline timeline = CreateTimeline();
            OM_Track<FakeClip, FakeTrack> track = timeline.TracksList.Single();
            int selectionChanges = 0;
            timeline.OnSelectedTrackChanged += _ => selectionChanges++;

            track.Click(MouseButton.LeftMouse, null, Vector2.zero);
            track.Click(MouseButton.LeftMouse, null, Vector2.zero);

            Assert.That(timeline.SelectedTrack, Is.SameAs(track));
            Assert.That(selectionChanges, Is.EqualTo(1));

            timeline.Body.Click(MouseButton.LeftMouse, null, Vector2.zero);

            Assert.That(timeline.SelectedTrack, Is.Null);
            Assert.That(selectionChanges, Is.EqualTo(2));
        }

        [Test]
        public void SelectionAndDragging_SwapDescriptionAndTimingDetails()
        {
            OM_Track<FakeClip, FakeTrack> track = CreateTimeline().TracksList.Single();
            OM_TrackClip<FakeClip, FakeTrack> trackClip = track.TrackClip;

            AssertSecondaryText(trackClip, DisplayStyle.Flex, DisplayStyle.None);

            track.SetIsSelected(true);
            AssertSecondaryText(trackClip, DisplayStyle.None, DisplayStyle.Flex);

            track.SetIsSelected(false);
            track.SetIsDragging(true);
            AssertSecondaryText(trackClip, DisplayStyle.None, DisplayStyle.Flex);

            track.SetIsDragging(false);
            AssertSecondaryText(trackClip, DisplayStyle.Flex, DisplayStyle.None);
        }

        private InteractiveElement AddInteractiveElement(out Label child)
        {
            root.AddManipulator(new OM_DragControlManipulator());
            InteractiveElement interactive = new InteractiveElement();
            child = new Label("Nested child");
            interactive.Add(child);
            root.Add(interactive);
            return interactive;
        }

        private FakeTimeline CreateTimeline(int clipCount = 1)
        {
            List<FakeClip> clips = Enumerable.Range(0, clipCount)
                .Select(index => new FakeClip
                {
                    ClipName = $"Scale {index + 1}",
                    ClipDescription = "This is a scale action",
                    Duration = 1,
                    OrderIndex = index,
                })
                .ToList();
            FakePlayer player = new FakePlayer();
            player.ClipsManager.PopulateWithClips(clips);
            FakeOwner owner = new FakeOwner(player);
            FakeTimeline timeline = new FakeTimeline(owner, player);
            root.Add(timeline);
            return timeline;
        }

        private static void AssertSecondaryText(
            OM_TrackClip<FakeClip, FakeTrack> trackClip,
            DisplayStyle expectedDescription,
            DisplayStyle expectedDetails)
        {
            Assert.That(trackClip.DescriptionLabel.style.display.value, Is.EqualTo(expectedDescription));
            Assert.That(trackClip.Details.style.display.value, Is.EqualTo(expectedDetails));
        }

        private static void SendMouseDown(VisualElement target, Vector2 position)
        {
            Event systemEvent = new Event
            {
                type = EventType.MouseDown,
                button = (int)MouseButton.LeftMouse,
                mousePosition = position,
            };

            using MouseDownEvent mouseEvent = MouseDownEvent.GetPooled(systemEvent);
            mouseEvent.target = target;
            target.SendEvent(mouseEvent);
        }

        private static void SendMouseMove(VisualElement target, Vector2 position)
        {
            Event systemEvent = new Event
            {
                type = EventType.MouseMove,
                button = (int)MouseButton.LeftMouse,
                mousePosition = position,
            };

            using MouseMoveEvent mouseEvent = MouseMoveEvent.GetPooled(systemEvent);
            mouseEvent.target = target;
            target.SendEvent(mouseEvent);
        }

        private static void SendMouseUp(VisualElement target, Vector2 position)
        {
            Event systemEvent = new Event
            {
                type = EventType.MouseUp,
                button = (int)MouseButton.LeftMouse,
                mousePosition = position,
            };

            using MouseUpEvent mouseEvent = MouseUpEvent.GetPooled(systemEvent);
            mouseEvent.target = target;
            target.SendEvent(mouseEvent);
        }

        private sealed class TestEditorWindow : EditorWindow
        {
        }

        private sealed class InteractiveElement :
            VisualElement,
            IOM_DragControlClickable,
            IOM_DragControlDraggable
        {
            public int ClickCount { get; private set; }

            public int StartDragCount { get; private set; }

            public int DragCount { get; private set; }

            public int EndDragCount { get; private set; }

            public void Click(MouseButton mouseButton, MouseUpEvent mouseUpEvent, Vector2 mousePosition)
            {
                ClickCount++;
            }

            public void StartDrag(Vector2 mousePosition)
            {
                StartDragCount++;
            }

            public void Drag(Vector2 delta, Vector2 mousePosition)
            {
                DragCount++;
            }

            public void EndDrag(Vector2 delta, Vector2 mousePosition)
            {
                EndDragCount++;
            }
        }

        private sealed class FakeClip : OM_ClipBase
        {
        }

        private sealed class FakeTrack : OM_Track<FakeClip, FakeTrack>
        {
            public FakeTrack(OM_Timeline<FakeClip, FakeTrack> timeline, FakeClip clip)
                : base(timeline, clip)
            {
            }

            public override Texture2D GetClipIcon()
            {
                return null;
            }

            protected override void OnDragPerform(DragPerformEvent e)
            {
            }
        }

        private sealed class FakeTimeline : OM_Timeline<FakeClip, FakeTrack>
        {
            public FakeTimeline(IOM_TimelineEditorOwner<FakeClip> owner, IOM_TimelinePlayer<FakeClip> player)
                : base(owner, player)
            {
            }

            public override void OnAddTrackClicked()
            {
            }

            public override FakeTrack CreateTrack(FakeClip clip)
            {
                return new FakeTrack(this, clip);
            }
        }

        private sealed class FakeOwner : IOM_TimelineEditorOwner<FakeClip>
        {
            public FakeOwner(FakePlayer player)
            {
                TimelinePlayer = player;
            }

            public OM_VisualElementsManager VisualElementsManager { get; } = new();

            public UnityEditor.Editor Editor => null;

            public IOM_TimelinePlayer<FakeClip> TimelinePlayer { get; }
        }

        private sealed class FakePlayer : IOM_TimelinePlayer<FakeClip>
        {
            private float timelineDuration = 1;

            public event Action<float> OnElapsedTimeChangedCallback
            {
                add { }
                remove { }
            }

            public event Action OnPlayerValidateCallback;

            public event Action<OM_PlayState> OnPlayStateChanged
            {
                add { }
                remove { }
            }

            public event Action OnTriggerEditorRefresh
            {
                add { }
                remove { }
            }

            public event Action OnClipAddedOrRemoved
            {
                add { }
                remove { }
            }

            public event Action<FakeClip> OnClipAdded
            {
                add { }
                remove { }
            }

            public event Action<FakeClip> OnClipRemoved
            {
                add { }
                remove { }
            }

            public OM_ClipsManager<FakeClip> ClipsManager { get; } = new();

            public float ElapsedTime { get; private set; }

            public int SelectedClipIndex { get; private set; } = -1;

            public void InitPlayerForEditor()
            {
                ClipsManager.InitForEditor();
            }

            public float GetTimelineDuration()
            {
                return timelineDuration;
            }

            public void SetTimelineDuration(float newDuration)
            {
                timelineDuration = newDuration;
            }

            public void SetElapsedTime(float newElapsedTime)
            {
                ElapsedTime = newElapsedTime;
            }

            public void AddClip(FakeClip clipToAdd)
            {
                ClipsManager.AddClip(clipToAdd, this);
            }

            public void RemoveClip(FakeClip clipToRemove)
            {
                ClipsManager.RemoveClip(clipToRemove, this);
            }

            public void DuplicateClip(FakeClip clipToDuplicate)
            {
                ClipsManager.DuplicateClip(clipToDuplicate, this);
            }

            public IEnumerable<FakeClip> GetClips()
            {
                return ClipsManager.GetClips();
            }

            public void RecordUndo(string undoName)
            {
            }

            public void OnValidate()
            {
                OnPlayerValidateCallback?.Invoke();
            }

            public void SetSelectedClipIndex(int index)
            {
                SelectedClipIndex = index;
            }

            public int GetSelectedClipIndex()
            {
                return SelectedClipIndex;
            }
        }
    }
}
