using System.Collections;
using System.IO;
using System.Linq;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Tracks;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class HomePresentationTests
    {
        [TestCase(1.35f, 3, 4)]
        [TestCase(0.9f, 0, 1)]
        public void HomeStats_SeparatesSavedStarsFromBestTime(float timeMultiplier, int stars, int position)
        {
            TrackDefinition track = AssetDatabase.FindAssets("t:TrackDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<TrackDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .First(candidate => candidate.HasConfiguredTiers);
            Services.TrackProgressModel progress = new Services.TrackProgressModel();
            progress.RecordBestTime(track.name, track.TimeToBeatSeconds * timeMultiplier);
            progress.RecordEarnedStars(track.name, stars);
            progress.RecordEarnedStars(track.name, 0);
            TrackStatsViewModel model = new TrackStatsViewModel(track, progress);
            Assert.That(model.Standings.PlayerPosition, Is.EqualTo(position));
            Assert.That(model.EarnedStars, Is.EqualTo(stars));
            Assert.That(model.StarTargetScores, Is.EqualTo(track.Tiers.OrderBy(tier => tier.TargetScore).Select(tier => tier.TargetScore)));
        }

        [UnityTest]
        public IEnumerator HomeBestTimes_ShowsSavedAndUnracedStandings()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            TrackDefinition track = AssetDatabase.FindAssets("t:TrackDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<TrackDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .First(candidate => candidate.HasConfiguredTiers);
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/Main View.prefab"));
            instance.SetActive(true);
            TrackStatsViewComponent stats = instance.GetComponentInChildren<TrackStatsViewComponent>();
            ResultStandingsView standings = instance.GetComponentInChildren<ResultStandingsView>();
            SerializedObject serializedStats = new SerializedObject(stats);
            TMP_Text lapsLabel = (TMP_Text)serializedStats.FindProperty("targetLapsLabel").objectReferenceValue;
            TMP_Text targetLabel = (TMP_Text)serializedStats.FindProperty("targetTimeLabel").objectReferenceValue;
            Image firstEarnedStar = (Image)serializedStats.FindProperty("earnedStars").GetArrayElementAtIndex(0).objectReferenceValue;
            RectTransform panel = (RectTransform)standings.transform.parent;
            float expandedHeight = panel.sizeDelta.y;
            float rowSpacing = new SerializedObject(standings).FindProperty("rowSpacing").floatValue;
            RectTransform standingsRect = (RectTransform)standings.transform;
            RectTransform trackViewport = (RectTransform)instance.transform.Find("TrackViewport");
            typeof(MainView).GetMethod(
                    "ConfigureTrackPreviewLayout",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(instance.GetComponent<MainView>(), null);
            Assert.That(expandedHeight, Is.EqualTo(760f).Within(0.1f),
                "The best-times panel must end directly below its four standings rows.");
            Assert.That(standingsRect.anchoredPosition.y, Is.EqualTo(-140f).Within(0.1f),
                "The standings rows must sit directly below the Best Times heading.");
            Assert.That(trackViewport.anchorMin, Is.EqualTo(new Vector2(0.06f, 0.48f)),
                "The home track viewport must use the enlarged lower-left bounds.");
            Assert.That(trackViewport.anchorMax, Is.EqualTo(new Vector2(0.94f, 0.76f)),
                "The home track viewport must use the enlarged upper-right bounds.");
            foreach (bool saved in new[] { false, true })
            {
                Services.TrackProgressModel progress = new Services.TrackProgressModel();
                if (saved)
                {
                    progress.RecordBestTime(track.name, track.TimeToBeatSeconds * 1.18f);
                    progress.RecordEarnedStars(track.name, 2);
                }
                TrackStatsViewModel model = new TrackStatsViewModel(track, progress);
                stats.Bind(model);
                yield return new WaitForSecondsRealtime(2f);
                RectTransform starsContainer = (RectTransform)firstEarnedStar.transform.parent;
                Assert.That(starsContainer.parent, Is.SameAs(instance.transform),
                    "Earned stars must be in the track header, outside the best-times panel.");
                Assert.That(starsContainer.anchorMin, Is.EqualTo(new Vector2(0.5f, 1f)));
                Assert.That(starsContainer.anchoredPosition.y, Is.EqualTo(-450f).Within(0.1f),
                    "Earned stars must sit below the track name and above the track preview.");
                TMP_Text[] targetScores = starsContainer.GetComponentsInChildren<TMP_Text>(true);
                Assert.That(targetScores.Select(label => label.text),
                    Is.EquivalentTo(model.StarTargetScores.Select(score => score.ToString())),
                    "Each star must show the score required for that track.");
                Assert.That(lapsLabel.gameObject.activeSelf, Is.False, "Home must not show lap metadata.");
                Assert.That(targetLabel.gameObject.activeSelf, Is.False, "Home must not show target-time metadata.");
                Assert.That(standings.DisplayedPlayerPosition, Is.EqualTo(saved ? 3 : 4));
                Assert.That(standings.IsAnimating, Is.False);
                ResultStandingRowView[] visibleRows = standings.GetComponentsInChildren<ResultStandingRowView>();
                Assert.That(visibleRows.Length, Is.EqualTo(3));
                Assert.That(visibleRows.Any(row => row.Entry.IsPlayer), Is.EqualTo(saved),
                    "YOU must remain hidden until the saved time reaches the top three.");
                float expectedHeight = expandedHeight - rowSpacing;
                Assert.That(panel.sizeDelta.y, Is.EqualTo(expectedHeight).Within(0.1f),
                    "The standings panel must end after the last visible row.");
                Assert.That(model.Standings.Player.FormattedTime, saved ? Does.Not.Contain("--") : Is.EqualTo("--:--.--"));
                foreach (int height in new[] { 2280, 1680 })
                {
                    yield return Capture(instance, saved ? "HomeSavedProgress" : "HomeUnraced", height);
                }
            }
            Object.Destroy(instance);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator HomePreview_RestoresSharedTrackWhenLeavingOrInterrupted()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            GameObject camera = new GameObject("PreviewCamera", typeof(Camera));
            camera.tag = "MainCamera";
            GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TrackViewComponent trackView = track.AddComponent<global::GearEngine.CarSimulation.Tracks.TrackViewComponent>();
            track.transform.position = new Vector3(1, 2, 3);
            Vector3 position = track.transform.position;
            Vector3 scale = track.transform.localScale;
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/Main View.prefab"));
            SerializedObject main = new SerializedObject(instance.GetComponent<MainView>());
            main.FindProperty("track").objectReferenceValue = trackView;
            main.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject anchor = new SerializedObject(instance.GetComponentInChildren<global::GearEngine.FrustumFit.FrustumFitAnchor>(true));
            anchor.FindProperty("targetTransform").objectReferenceValue = track.transform;
            anchor.ApplyModifiedPropertiesWithoutUndo();
            IView view = instance.GetComponent<IView>();
            view.Open();
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(track.transform.position, Is.Not.EqualTo(position), "Home should fit the preview.");
            view.Close();
            Assert.That(track.transform.position, Is.EqualTo(position));
            Assert.That(track.transform.localScale, Is.EqualTo(scale));
            view.Open();
            yield return null;
            view.Hide();
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(track.transform.position, Is.EqualTo(position), "An interrupted opening must not keep writing the shared track pose.");
            Assert.That(track.transform.localScale, Is.EqualTo(scale));
            Object.Destroy(instance);
            Object.Destroy(track);
            Object.Destroy(camera);
            yield return new ExitPlayMode();
        }

        [Test]
        public void GearPopup_BindsRarityStars()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/ItemPopup View.prefab");
            ItemSlotView slot = prefab.GetComponentInChildren<ItemSlotView>(true);
            Assert.That(new SerializedObject(slot).FindProperty("rarityStars").arraySize, Is.EqualTo(5), "Gear rarity stars must be data-bound, not a two-star design sample.");
        }

        [UnityTest]
        public IEnumerator GearCard_UsesItemRarityInsteadOfTwoStarDesignSample()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/Item_View.prefab");
            GameObject canvasObject = new GameObject(
                "GearCardCaptureCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                instance.transform.SetParent(canvasObject.transform, false);
                RectTransform cardRect = (RectTransform)instance.transform;
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localScale = Vector3.one * 2.2f;
                ItemSlotView slot = instance.GetComponent<ItemSlotView>();
                GearItemData gear = AssetDatabase.LoadAssetAtPath<GearItem>(
                    "Assets/GearEngine/Scriptables/GeneratedGears/QuantumLink/QuantumLink_Tier4_Config.asset").CreateRuntimeData();
                slot.Bind(new ItemSlotViewModel(gear, null));

                SerializedObject serialized = new SerializedObject(slot);
                SerializedProperty stars = serialized.FindProperty("rarityStars");
                Sprite filled = (Sprite)serialized.FindProperty("filledRarityStar").objectReferenceValue;

                Assert.That(stars.arraySize, Is.EqualTo(5), "Grid cards must represent all five rarity levels.");
                Assert.That(filled, Is.Not.Null, "Grid cards must have a configured filled rarity star.");
                Assert.That(Enumerable.Range(0, stars.arraySize)
                    .Count(i => ((Image)stars.GetArrayElementAtIndex(i).objectReferenceValue).sprite == filled), Is.EqualTo(4),
                    "An epic grid card must show four filled stars, matching its popup.");
                yield return Capture(canvasObject, "GearCardQuantumLink", 1680);
            }
            finally
            {
                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(canvasObject);
            }
        }

        [UnityTest]
        public IEnumerator GearPopup_ShowsSelectedCardOnOpenAndReopen()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/ItemPopup View.prefab"));
            ItemSlotView slot = instance.GetComponentInChildren<ItemSlotView>(true);
            SerializedObject serialized = new SerializedObject(slot);
            TMP_Text name = (TMP_Text)serialized.FindProperty("nameLabel").objectReferenceValue;
            Image icon = (Image)serialized.FindProperty("iconImage").objectReferenceValue;
            GearItemData gear = AssetDatabase.LoadAssetAtPath<GearItem>(
                "Assets/GearEngine/Scriptables/GeneratedGears/QuantumLink/QuantumLink_Tier4_Config.asset").CreateRuntimeData();
            GearItemData second = AssetDatabase.LoadAssetAtPath<GearItem>(
                "Assets/GearEngine/Scriptables/GeneratedGears/QuantumLink/QuantumLink_Tier2_Config.asset").CreateRuntimeData();
            ItemPopupViewModel model = new ItemPopupViewModel(new[] { new ItemSlotViewModel(gear, null), new ItemSlotViewModel(second, null) }, 0, _ => System.Threading.Tasks.Task.FromResult(true), "Select", false);
            model.Bind(new RecordingNavigation());
            instance.GetComponent<ItemPopupView>().Bind(model);
            IView view = instance.GetComponent<IView>();
            for (int open = 0; open < 2; open++)
            {
                view.Open();
                yield return new WaitForSecondsRealtime(3f);
                Assert.That(name.gameObject.activeInHierarchy, Is.True, "Selected gear title is inactive.");
                Assert.That(icon.gameObject.activeInHierarchy, Is.True, "Selected gear artwork is inactive.");
                Assert.That(icon.canvasRenderer.GetAlpha(), Is.GreaterThan(0.9f), "Selected gear artwork remains transparent.");
                Assert.That(name.canvasRenderer.GetAlpha(), Is.GreaterThan(0.9f), "Selected gear title remains transparent.");
                Assert.That(name.text, Is.EqualTo(gear.Name));
                Assert.That(name.color.grayscale, Is.LessThan(0.35f), "Popup titles must contrast with the rarity card background.");
                Assert.That(icon.sprite, Is.SameAs(gear.Icon));
                Assert.That(name.transform.lossyScale.x, Is.GreaterThan(0.1f), "Selected gear title remains hidden.");
                Assert.That(name.transform.lossyScale.z, Is.GreaterThan(0.1f), "Selected gear title has a degenerate transform.");
                Assert.That(icon.transform.lossyScale.x, Is.GreaterThan(0.1f), "Selected gear artwork remains hidden.");
                SerializedProperty stars = new SerializedObject(slot).FindProperty("rarityStars");
                Assert.That(stars.arraySize, Is.EqualTo(5));
                Sprite filled = (Sprite)new SerializedObject(slot).FindProperty("filledRarityStar").objectReferenceValue;
                Assert.That(Enumerable.Range(0, 5).Count(i => ((Image)stars.GetArrayElementAtIndex(i).objectReferenceValue).sprite == filled), Is.EqualTo(4));
                foreach (int height in new[] { 2280, 1680 })
                {
                    yield return Capture(instance, "GearPopupQuantumLink", height);
                }
                model.Next();
                yield return null;
                Assert.That(name.text, Is.EqualTo(second.Name));
                Assert.That(Enumerable.Range(0, 5).Count(i => ((Image)stars.GetArrayElementAtIndex(i).objectReferenceValue).sprite == filled), Is.EqualTo(1));
                model.Previous();
                yield return null;
                Assert.That(name.text, Is.EqualTo(gear.Name));
                Assert.That(name.color.grayscale, Is.LessThan(0.35f), "Popup titles must contrast with the rarity card background.");
                view.Close();
            }
            Object.Destroy(instance);
            yield return new ExitPlayMode();
        }
        private static IEnumerator Capture(GameObject instance, string scenario, int height)
        {
            const int width = 1080;
            string output = Path.GetFullPath("Artifacts/VisualTests/HomeStandings");
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
                string criteria = scenario.StartsWith("Home", System.StringComparison.Ordinal)
                    ? "[\"Runtime ViewModel bindings\",\"Track name shown without laps or target\",\"Rendered text inside viewport\",\"Standings panel fits three rows\",\"Unranked player hidden until reaching the top three\"]"
                    : "[\"Runtime ViewModel bindings\",\"Rendered text inside viewport\",\"Animation restart\",\"Selected gear card visibility and rarity\"]";
                File.WriteAllText(Path.Combine(output, file + ".evidence.json"),
                    "{\"test\":\"" + NUnit.Framework.TestContext.CurrentContext.Test.FullName + "\"," +
                    $"\"artifact\":\"{file}\",\"scenario\":\"{scenario}\"," +
                    $"\"criteria\":{criteria}}}");
                ItemSlotView card = instance.GetComponentInChildren<ItemSlotView>();
                if (card != null)
                {
                    TMP_Text title = (TMP_Text)new SerializedObject(card).FindProperty("nameLabel").objectReferenceValue;
                    Bounds bounds = title.textBounds;
                    Vector3 min = camera.WorldToViewportPoint(title.transform.TransformPoint(bounds.min));
                    Vector3 max = camera.WorldToViewportPoint(title.transform.TransformPoint(bounds.max));
                    int titlePixels = 0;
                    for (int y = Mathf.Max(0, (int)(min.y * height)); y < Mathf.Min(height, (int)(max.y * height)); y++)
                    {
                        for (int x = Mathf.Max(0, (int)(min.x * width)); x < Mathf.Min(width, (int)(max.x * width)); x++)
                        {
                            Color pixel = image.GetPixel(x, y);
                            Color ink = title.color;
                            if (Mathf.Abs(pixel.r - ink.r) + Mathf.Abs(pixel.g - ink.g) + Mathf.Abs(pixel.b - ink.b) < 0.12f)
                            {
                                titlePixels++;
                            }
                        }
                    }
                    Assert.That(titlePixels, Is.GreaterThan(30), "Gear title must actually render on the card, not only have bound text.");
                }

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
