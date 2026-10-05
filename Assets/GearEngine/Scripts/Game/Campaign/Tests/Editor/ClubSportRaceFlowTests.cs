using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.Campaign.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Tracks;

namespace GearEngine.Campaign.Tests.Editor
{
    /// <summary>Runs the real campaign composition and UI buttons; it never sets race completion or score.</summary>
    public sealed class ClubSportRaceFlowTests
    {
        private const string k_folder = "Artifacts/VisualTests/ClubSportProduction";

        [UnityTest]
        [Category("Visual")]
        public IEnumerator TwoConsecutiveRaces_RestoreTheHudAcrossEveryPhase()
        {
            EditorSceneManager.OpenScene("Assets/GearEngine/Scenes/Main Scene.unity");
            yield return new EnterPlayMode();
            yield return WaitFor<MainView>(60);
            for (int race = 1; race <= 2; race++)
            {
                MainView home = Active<MainView>();
                yield return new WaitForSecondsRealtime(1);
                MainViewModel main = (MainViewModel)typeof(MainView).GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(home);
                string requestedTrack = race == 1 ? "Circle" : "Infinity";
                for (int attempt = 0; attempt < 12 && !main.Track.Track.name.Contains(requestedTrack); attempt++)
                {
                    Click(home, "nextTrackButton");
                    yield return new WaitForSecondsRealtime(.6f);
                }
                Assert.That(main.Track.Track.name, Does.Contain(requestedTrack), "Requested campaign track must be selected.");
                Directory.CreateDirectory(k_folder);
                File.WriteAllText(k_folder + "/Race" + race + "Track.txt", main.Track.Track.name);
                Click(home, "playButton");
                yield return WaitFor<SetupView>(15);
                yield return new WaitForSecondsRealtime(1);
                Click(Active<SetupView>(), "raceButton");
                yield return WaitFor<ActiveRaceView>(15);
                ActiveRaceView raceView = Active<ActiveRaceView>();
                ClubSportHudView hud = raceView.GetComponentInChildren<ClubSportHudView>();
                Assert.That(hud, Is.Not.Null);
                Assert.That(hud.State, Is.Not.Null);
                Assert.That(hud.ControlsDisabled, Is.True);
                yield return new WaitForSecondsRealtime(4);
                yield return Capture("Race" + race + "Tall", 390, 780);
                yield return Capture("Race" + race + "Short", 390, 635);
                yield return Capture("Race" + race + "Narrow", 320, 680);
                yield return WaitFor<ResultPopupView>(150);
                ResultPopupView result = Active<ResultPopupView>();
                TMP_Text resultScore = (TMP_Text)new SerializedObject(result).FindProperty("scoreText").objectReferenceValue;
                TMP_Text total = hud.transform.Find("Content/Feedback/BankedTotal").GetComponent<TMP_Text>();
                Assert.That(total.text.Replace(",", ""), Is.EqualTo(resultScore.text));
                yield return Capture("Race" + race + "Results", 390, 780);
                Click(result, "continueButton");
                yield return WaitFor<ReceivedRewardsView>(20);
                yield return new WaitForSecondsRealtime(1);
                yield return Capture("Race" + race + "Rewards", 390, 780);
                float rewardDeadline = Time.realtimeSinceStartup + 20;
                while (Active<RoguelikeView>() == null && Active<MainView>() == null && Time.realtimeSinceStartup < rewardDeadline)
                {
                    ReceivedRewardsView rewards = Active<ReceivedRewardsView>();
                    if (rewards != null) { Click(rewards, "continueButton"); }
                    yield return new WaitForSecondsRealtime(.5f);
                }
                bool hasUpgrade = Active<RoguelikeView>() != null;
                File.WriteAllText(k_folder + "/Race" + race + "RewardRoute.txt", hasUpgrade ? "Rewards > Upgrade > Selection" : "Rewards > Selection (no gear reward)");
                if (hasUpgrade)
                {
                    yield return Capture("Race" + race + "Upgrade", 390, 780);
                    Click(Active<RoguelikeView>(), "continueButton");
                }
                yield return WaitFor<MainView>(20);
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        [Category("Visual")]
        public IEnumerator ProductionRace_PortraitFraming()
        {
            EditorSceneManager.OpenScene("Assets/GearEngine/Scenes/Main Scene.unity");
            yield return new EnterPlayMode();
            yield return WaitFor<MainView>(60);
            yield return new WaitForSecondsRealtime(1);
            Click(Active<MainView>(), "playButton");
            yield return WaitFor<SetupView>(15);
            yield return new WaitForSecondsRealtime(1);
            Click(Active<SetupView>(), "raceButton");
            yield return WaitFor<ActiveRaceView>(15);
            yield return new WaitForSecondsRealtime(4);
            yield return Capture("Framing", 390, 780);
            yield return new ExitPlayMode();
        }

        private static T Active<T>() where T : Component => Object.FindObjectsByType<T>()
            .FirstOrDefault(item => item.gameObject.activeInHierarchy && item.transform.lossyScale.x > .01f);

        private static IEnumerator WaitFor<T>(float timeout) where T : Component
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (Active<T>() == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(Active<T>(), Is.Not.Null, "Campaign did not reach " + typeof(T).Name);
            yield return new WaitForSecondsRealtime(.5f);
        }

        private static void Click(Component view, string field)
        {
            Assert.That(view, Is.Not.Null);
            Button button = (Button)new SerializedObject(view).FindProperty(field).objectReferenceValue;
            Assert.That(button.isActiveAndEnabled && button.interactable, Is.True, field + " unavailable");
            button.onClick.Invoke();
        }

        private static IEnumerator Capture(string name, int width, int height)
        {
            Directory.CreateDirectory(k_folder);
            ActiveRaceView beforeRace = Active<ActiveRaceView>();
            if (beforeRace != null)
            {
                Component beforeTrack = (Component)new SerializedObject(beforeRace).FindProperty("track").objectReferenceValue;
                File.WriteAllText(k_folder + "/" + name + "Before.txt", $"Screen {Screen.width}x{Screen.height} track={beforeTrack.transform.position} scale={beforeTrack.transform.lossyScale} canvas={beforeRace.transform.lossyScale} rect={((RectTransform)beforeRace.transform).rect}");
            }
            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            RenderTexture target = new RenderTexture(width, height, 24);
            RenderTexture previous = camera.targetTexture;
            RenderTexture active = RenderTexture.active;
            Canvas[] canvases = Object.FindObjectsByType<Canvas>().Where(canvas => canvas.isRootCanvas).ToArray();
            RenderMode[] modes = canvases.Select(canvas => canvas.renderMode).ToArray();
            Camera[] cameras = canvases.Select(canvas => canvas.worldCamera).ToArray();
            float[] distances = canvases.Select(canvas => canvas.planeDistance).ToArray();
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;

                for (int i = 0; i < canvases.Length; i++)
                {
                    if (modes[i] == RenderMode.ScreenSpaceOverlay) { canvases[i].renderMode = RenderMode.ScreenSpaceCamera; canvases[i].worldCamera = camera; canvases[i].planeDistance = 1f; }
                }

                Canvas.ForceUpdateCanvases();
                yield return null;
                yield return null;
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (modes[i] == RenderMode.ScreenSpaceOverlay) { canvases[i].renderMode = RenderMode.ScreenSpaceCamera; canvases[i].worldCamera = camera; canvases[i].planeDistance = 1f; }
                }

                Canvas.ForceUpdateCanvases();
                BoardViewComponent sharedBoard = Active<BoardViewComponent>();
                if (Active<ActiveRaceView>() != null)
                {
                    Assert.That(sharedBoard, Is.Not.Null, "Race board must remain active.");
                    Assert.That(sharedBoard.GetBoardSpaceRoot().lossyScale.x, Is.GreaterThan(.001f), "Board must not collapse during view/canvas transitions.");
                }
                string diagnostics = $"Camera pos={camera.transform.position} near={camera.nearClipPlane} far={camera.farClipPlane} mask={camera.cullingMask}\n" + string.Join("\n", canvases.Select(canvas => $"Canvas {canvas.name} mode={canvas.renderMode} plane={canvas.planeDistance} order={canvas.sortingOrder} rect={((RectTransform)canvas.transform).rect}"));
                ActiveRaceView race = Active<ActiveRaceView>();
                if (race != null)
                {
                    SerializedObject fields = new SerializedObject(race);
                    Component trackComponent = (Component)fields.FindProperty("track").objectReferenceValue;
                    Renderer road = trackComponent.transform.Find("Track/Path").GetComponent<Renderer>();
                    Renderer roadEdge = trackComponent.transform.Find("Track/RoadEdge").GetComponent<Renderer>();
                    AssertRoadEdgeContrast(road, roadEdge);
                    AssertThemeSurfaceZonesVisible(trackComponent, camera);
                    SplinePropGenerator generator = trackComponent.GetComponentInChildren<global::GearEngine.CarSimulation.Tracks.SplinePropGenerator>();
                    Transform generated = generator.transform.Find("Generated Props");
                    Assert.That(generated, Is.Not.Null);
                    foreach (Renderer prop in generated.GetComponentsInChildren<Renderer>())
                    {
                        Assert.That(prop.bounds.size.x, Is.LessThan(road.bounds.size.x * .5f), "Responsive framing must not magnify props across the road.");
                    }
                    ClubSportHudView hud = race.GetComponentInChildren<ClubSportHudView>();
                    Rect region = hud.Layout.RoadAnchors();
                    Rect canvasRect = ((RectTransform)race.transform).rect;
                    foreach (RectTransform marker in race.transform.Cast<Transform>().OfType<RectTransform>().Where(item => item.name == "RacePositionMarker" && item.gameObject.activeSelf))
                    {
                        float bottom = marker.anchoredPosition.y - marker.pivot.y * marker.rect.height;
                        float top = bottom + marker.rect.height;
                        Assert.That(bottom, Is.GreaterThanOrEqualTo(canvasRect.yMin + region.yMin * canvasRect.height - 1f));
                        Assert.That(top, Is.LessThanOrEqualTo(canvasRect.yMin + region.yMax * canvasRect.height + 1f), "Position markers must not cover the feedback strip.");
                    }
                    Vector3 minimum = camera.WorldToViewportPoint(road.bounds.min);
                    Vector3 maximum = camera.WorldToViewportPoint(road.bounds.max);
                    Assert.That(minimum.x, Is.GreaterThanOrEqualTo(region.xMin - .01f));
                    Assert.That(maximum.x, Is.LessThanOrEqualTo(region.xMax + .01f));
                    Assert.That(minimum.y, Is.GreaterThanOrEqualTo(region.yMin - .03f));
                    Assert.That(maximum.y, Is.LessThanOrEqualTo(region.yMax + .03f));
                    foreach (string field in new[] { "track", "board" })
                    {
                        Component item = (Component)fields.FindProperty(field).objectReferenceValue;
                        diagnostics += $"\n{field} root={item.transform.position} scale={item.transform.lossyScale}";
                        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
                        {
                            diagnostics += $"\n{renderer.name} bounds={renderer.bounds} enabled={renderer.enabled} order={renderer.sortingOrder} queue={renderer.sharedMaterial?.renderQueue} shader={renderer.sharedMaterial?.shader.name}";
                        }

                        foreach (RectTransform rect in item.GetComponentsInChildren<RectTransform>())
                        {
                            diagnostics += $"\n{rect.name} pos={rect.position} scale={rect.lossyScale} size={rect.rect.size}";
                        }
                    }
                }
                File.WriteAllText(k_folder + "/" + name + ".txt", diagnostics);
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                string path = k_folder + "/" + name + ".png";
                File.WriteAllBytes(path, image.EncodeToPNG());
                string testName = name == "Framing"
                    ? "GearEngine.Campaign.Tests.Editor.ClubSportRaceFlowTests.ProductionRace_PortraitFraming"
                    : "GearEngine.Campaign.Tests.Editor.ClubSportRaceFlowTests.TwoConsecutiveRaces_RestoreTheHudAcrossEveryPhase";
                File.WriteAllText(path + ".evidence.json", "{\"test\":\"" + testName + "\",\"artifact\":\"" + name + ".png\",\"scenario\":\"" + name + "\",\"criteria\":[\"HUD readable and separate from road\",\"Shared board visible after reopening\",\"Four distinct car liveries\",\"Rounded full-color PULSE and TURBO controls\",\"Compact rounded lap panel\",\"Persistent SCORE heading\"]}");
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; }
                camera.targetTexture = previous; RenderTexture.active = active;
                Object.DestroyImmediate(target); Object.DestroyImmediate(image);
            }
        }

        private static void AssertRoadEdgeContrast(Renderer road, Renderer roadEdge)
        {
            Assert.That(road, Is.Not.Null);
            Assert.That(roadEdge, Is.Not.Null);
            Assert.That(road.sharedMaterial, Is.Not.Null);
            Assert.That(road.sharedMaterial.HasProperty("_BaseColor"), Is.True);

            MaterialPropertyBlock edgeProperties = new MaterialPropertyBlock();
            roadEdge.GetPropertyBlock(edgeProperties);
            Color roadColor = road.sharedMaterial.GetColor("_BaseColor");
            Color edgeColor = edgeProperties.GetColor("_BaseColor");
            Color.RGBToHSV(roadColor, out _, out _, out float roadValue);
            Color.RGBToHSV(edgeColor, out _, out _, out float edgeValue);
            Assert.That(edgeValue - roadValue, Is.GreaterThanOrEqualTo(0.25f),
                "The road edge must remain visibly separated from the biome background.");
        }

        private static void AssertThemeSurfaceZonesVisible(Component trackComponent, Camera camera)
        {
            TrackViewComponent trackView = trackComponent as TrackViewComponent;
            Assert.That(trackView, Is.Not.Null);
            Assert.That(trackView.ActiveTheme, Is.Not.Null);

            foreach (TrackThemeDefinition.SurfaceZone zone in trackView.ActiveTheme.SurfaceZones)
            {
                Transform patch = trackComponent.transform.Find($"Track Theme/Track Effects/{zone.Name}");
                Assert.That(patch, Is.Not.Null, $"The {zone.Name} surface must survive the race transition.");
                Assert.That(patch.gameObject.activeInHierarchy, Is.True);
                Renderer renderer = patch.GetComponent<Renderer>();
                Assert.That(renderer, Is.Not.Null);
                Assert.That(renderer.enabled, Is.True);

                Vector3 viewportPosition = camera.WorldToViewportPoint(renderer.bounds.center);
                Assert.That(viewportPosition.z, Is.GreaterThan(0f));
                Assert.That(viewportPosition.x, Is.InRange(0f, 1f));
                Assert.That(viewportPosition.y, Is.InRange(0f, 1f));
            }
        }
    }
}
