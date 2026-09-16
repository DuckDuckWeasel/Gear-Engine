using System;
using System.Collections;
using System.IO;
using System.Linq;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using Scaffold.MVVM;
using Scaffold.Navigation;
using Scaffold.Navigation.Contracts;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class PostRaceScreenTests
    {
        [Test]
        public void NavigationSettings_RegisterSeparatePostRaceControllers()
        {
            NavigationSettings settings = AssetDatabase.LoadAssetAtPath<NavigationSettings>("Assets/Navigation/Navigation Settings.asset");
            foreach (Type type in new[] { typeof(ResultPopupViewModel), typeof(ReceivedRewardsViewModel), typeof(RaceProgressViewModel) })
            {
                ViewConfig config = settings.GetViewConfig(type);
                Assert.That(config.ControllerType, Is.EqualTo(type));
                Assert.That(config.Asset.editorAsset, Is.Not.Null);
            }
        }

        [UnityTest]
        public IEnumerator PostRaceScreens_OpenCloseAndRenderBoundData()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            TrackDefinition track = AssetDatabase.FindAssets("t:TrackDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<TrackDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .First(candidate => candidate.HasConfiguredTiers && candidate.Tiers.Count == 3);
            RaceResultModel result = new RaceResultModel(track.TimeToBeatSeconds * 1.18f, 3, track, 5438, track.TimeToBeatSeconds * 1.3f);
            string[] names = { "Campaign_ResultPopupView", "PFB_ReceivedRewardsView", "PFB_RaceProgressView" };
            for (int i = 0; i < names.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/GearEngine/Prefabs/Campaign/{names[i]}.prefab");
                GameObject instance = Object.Instantiate(prefab);
                RecordingNavigation navigation = new RecordingNavigation();
                ViewModel vm = CreateModel(i, result);
                vm.Bind(navigation);
                ViewElement element = instance.GetComponent<ViewElement>();
                element.Bind(vm);
                IView view = instance.GetComponent<IView>();
                view.Open();
                if (i == 0)
                {
                    ResultStandingsView standings = instance.GetComponentInChildren<ResultStandingsView>();
                    Assert.That(standings.DisplayedPlayerPosition, Is.EqualTo(4));
                    yield return Capture(instance, "FourthBeforePromotion", 2280);
                    view.Close();
                    Assert.That(standings.IsAnimating, Is.False, "Closing must stop an unfinished rank animation.");
                    view.Open();
                    Assert.That(standings.DisplayedPlayerPosition, Is.EqualTo(4), "Reopening must reset the starting rank.");
                }
                yield return new WaitForSecondsRealtime(5f);
                view.Close();
                Assert.That(instance.activeSelf, Is.False);
                view.Open();
                yield return new WaitForSecondsRealtime(5f);
                Assert.That(instance.activeSelf, Is.True);
                if (i == 0)
                {
                    ResultStandingsView standings = instance.GetComponentInChildren<ResultStandingsView>();
                    Assert.That(standings.DisplayedPlayerPosition, Is.EqualTo(3));
                    Assert.That(standings.IsAnimating, Is.False);
                    Assert.That(instance.GetComponentsInChildren<ResultStandingRowView>().Length, Is.EqualTo(3));
                    Assert.That(instance.GetComponentsInChildren<Button>().Length, Is.EqualTo(1));
                }
                TMP_Text[] visibleTexts = instance.GetComponentsInChildren<TMP_Text>();
                foreach (TMP_Text text in visibleTexts)
                {
                    float alpha = text.color.a;
                    foreach (CanvasGroup group in text.GetComponentsInParent<CanvasGroup>())
                    {
                        alpha *= group.alpha;
                    }

                    Assert.That(alpha, Is.GreaterThan(0.95f), $"{names[i]}/{text.name} remains hidden after reopening.");
                    Assert.That(text.transform.lossyScale.x, Is.GreaterThan(0f));
                    Assert.That(text.transform.lossyScale.z, Is.GreaterThan(0f), $"{names[i]}/{text.name} has a degenerate text transform.");
                }
                if (i == 1)
                {
                    TMP_Text rewardCount = visibleTexts.Single(text => text.name == "Reward_Count");
                    Assert.That(rewardCount.color.grayscale, Is.LessThan(0.5f), "Reward count must contrast with its white strip.");
                }

                string visibleContent = string.Join(" ", visibleTexts.Select(text => text.text));
                Assert.That(visibleContent, Does.Not.Contain("RACE TIME").And.Not.Contain("LAPS COMPLETED").And.Not.Contain("GOLD EARNED"));
                Assert.That(visibleContent, Does.Contain(i == 0 ? "5438" : i == 1 ? "GOLD" : track.GetDisplayName()));
                foreach (int height in new[] { 2280, 1920, 2400, 1680 })
                {
                    yield return Capture(instance, names[i], height);
                }

                Button button = (Button)new SerializedObject(element).FindProperty("continueButton").objectReferenceValue;
                button.onClick.Invoke();
                if (i == 1)
                {
                    Assert.That(((ReceivedRewardsViewModel)vm).NeedsGearSelection, Is.True);
                    Assert.That(string.Join(" ", instance.GetComponentsInChildren<TMP_Text>().Select(text => text.text)), Does.Contain("UPGRADE"));
                    yield return Capture(instance, "GearReward", 2280);
                }
                button.onClick.Invoke();
                Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1), "Repeated clicks must open one destination.");
                view.Close();
                Object.Destroy(instance);
                yield return null;
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Standings_ShowPoorPlacementWithThreeStarsAndUnchangedThird()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            TrackDefinition track = AssetDatabase.FindAssets("t:TrackDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<TrackDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .First(candidate => candidate.HasConfiguredTiers && candidate.Tiers.Count == 3);
            int maximumScore = track.Tiers.Max(tier => tier.TargetScore);
            foreach (bool fourth in new[] { true, false })
            {
                float raceTime = track.TimeToBeatSeconds * (fourth ? 1.35f : 1.18f);
                RaceResultModel result = new RaceResultModel(raceTime, 3, track, maximumScore, raceTime);
                GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/GearEngine/Prefabs/Campaign/Campaign_ResultPopupView.prefab"));
                ResultPopupViewModel vm = new ResultPopupViewModel(result);
                vm.Bind(new RecordingNavigation());
                instance.GetComponent<ViewElement>().Bind(vm);
                IView view = instance.GetComponent<IView>();
                view.Open();
                ResultStandingsView standings = instance.GetComponentInChildren<ResultStandingsView>();
                Assert.That(standings.IsAnimating, Is.False);
                Assert.That(standings.DisplayedPlayerPosition, Is.EqualTo(fourth ? 4 : 3));
                Assert.That(result.HighestAchievedTier, Is.EqualTo(3));
                Assert.That(instance.GetComponentsInChildren<ResultStandingRowView>().Length, Is.EqualTo(fourth ? 4 : 3));
                yield return new WaitForSecondsRealtime(4f);
                RectTransform header = (RectTransform)instance.transform.Find("Container/Labels_Title&Sub");
                Assert.That(header.anchoredPosition.y, Is.EqualTo(-340f).Within(0.1f), "First opening must settle the header inside the screen.");
                yield return Capture(instance, fourth ? "FourthPlaceThreeStars" : "UnchangedThirdPlace", 2280);
                view.Close();
                Object.Destroy(instance);
                yield return null;
            }
            yield return new ExitPlayMode();
        }

        private static ViewModel CreateModel(int index, RaceResultModel result)
        {
            if (index == 0)
            {
                return new ResultPopupViewModel(result);
            }

            if (index == 1)
            {
                return new ReceivedRewardsViewModel(result);
            }

            return new RaceProgressViewModel(result);
        }

        private static IEnumerator Capture(GameObject instance, string scenario, int height)
        {
            const int width = 1080;
            string output = Path.GetFullPath("Artifacts/VisualTests/RaceStandings/Runtime");
            Directory.CreateDirectory(output);
            GameObject cameraObject = new GameObject("PostRaceCaptureCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.96f, 0.92f);
            RenderTexture texture = new RenderTexture(width, height, 24);
            RenderTexture previous = RenderTexture.active;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = texture;
                Canvas canvas = instance.GetComponent<Canvas>();
                CanvasScaler scaler = instance.GetComponent<CanvasScaler>();
                scaler.enabled = false;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                canvas.scaleFactor = height / 2280f;
                Canvas.ForceUpdateCanvases();
                yield return null;
                yield return null;
                Canvas.ForceUpdateCanvases();
                foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>())
                {
                    text.ForceMeshUpdate(true);
                    Bounds bounds = text.textBounds;
                    Vector3 min = camera.WorldToViewportPoint(text.transform.TransformPoint(bounds.min));
                    Vector3 max = camera.WorldToViewportPoint(text.transform.TransformPoint(bounds.max));
                    Assert.That(min.x, Is.GreaterThanOrEqualTo(0f), $"{scenario}/{text.name} leaves the left edge.");
                    Assert.That(max.x, Is.LessThanOrEqualTo(1f), $"{scenario}/{text.name} leaves the right edge.");
                    Assert.That(min.y, Is.GreaterThanOrEqualTo(0f), $"{scenario}/{text.name} leaves the bottom edge.");
                    Assert.That(max.y, Is.LessThanOrEqualTo(1f), $"{scenario}/{text.name} leaves the top edge.");
                }
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                string file = $"{scenario}{width}x{height}.png";
                File.WriteAllBytes(Path.Combine(output, file), image.EncodeToPNG());
                File.WriteAllText(Path.Combine(output, file + ".evidence.json"),
                    "{\"test\":\"" + NUnit.Framework.TestContext.CurrentContext.Test.FullName + "\"," +
                    $"\"artifact\":\"{file}\",\"scenario\":\"{scenario}\"," +
                    "\"criteria\":[\"Runtime ViewModel bindings\",\"Rendered text inside viewport\",\"Animation restart\",\"Time-based standings and score-only stars\"]}");
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                texture.Release();
                Object.DestroyImmediate(image);
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
