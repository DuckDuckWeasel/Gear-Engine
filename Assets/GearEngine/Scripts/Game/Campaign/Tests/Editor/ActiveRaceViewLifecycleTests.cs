using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ActiveRaceViewLifecycleTests
    {
        private sealed class TestActiveRaceView : ActiveRaceView
        {
            public void InvokeOnOpen() => OnOpen();

            public void InvokeOnClose() => OnClose();
        }

        [Test]
        public void OnClose_DisablesTrackAndHudRoots()
        {
            GameObject trackGo = new GameObject("Track");
            trackGo.AddComponent<UnityEngine.Splines.SplineContainer>();
            TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
            GameObject hudGo = new GameObject("Hud");
            hudGo.AddComponent<RaceHudViewComponent>();
            GameObject root = new GameObject("Root");
            TestActiveRaceView view = root.AddComponent<TestActiveRaceView>();

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("hud").objectReferenceValue = hudGo.GetComponent<RaceHudViewComponent>();
            so.ApplyModifiedPropertiesWithoutUndo();

            trackGo.SetActive(true);
            hudGo.SetActive(true);
            view.InvokeOnClose();

            Assert.That(trackGo.activeSelf, Is.False);
            Assert.That(hudGo.activeSelf, Is.False);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(trackGo);
            Object.DestroyImmediate(hudGo);
        }

        [Test]
        public void OnOpen_EnablesTrackAndHudRoots()
        {
            GameObject trackGo = new GameObject("Track");
            trackGo.AddComponent<UnityEngine.Splines.SplineContainer>();
            TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
            GameObject hudGo = new GameObject("Hud");
            hudGo.AddComponent<RaceHudViewComponent>();
            GameObject root = new GameObject("Root");
            TestActiveRaceView view = root.AddComponent<TestActiveRaceView>();

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("hud").objectReferenceValue = hudGo.GetComponent<RaceHudViewComponent>();
            so.ApplyModifiedPropertiesWithoutUndo();

            trackGo.SetActive(false);
            hudGo.SetActive(false);
            view.InvokeOnOpen();

            Assert.That(trackGo.activeSelf, Is.True);
            Assert.That(hudGo.activeSelf, Is.True);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(trackGo);
            Object.DestroyImmediate(hudGo);
        }
    }
}
