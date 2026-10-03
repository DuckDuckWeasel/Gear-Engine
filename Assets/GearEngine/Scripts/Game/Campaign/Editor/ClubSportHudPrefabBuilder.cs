using System;
using GearEngine.Campaign.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Editor
{
    /// <summary>One-time authoring utility. All generated references and layout are serialized into production prefabs.</summary>
    public static class ClubSportHudPrefabBuilder
    {
        public const string k_prefabPath = "Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab";
        private const string k_racePath = "Assets/GearEngine/Prefabs/Campaign/Race View.prefab";
        private static readonly Color s_cream = new Color32(248, 239, 220, 255);
        private static readonly Color s_ink = new Color32(53, 65, 74, 255);
        private static readonly Color s_coral = new Color32(255, 102, 105, 255);
        private static readonly Color s_gold = new Color32(255, 213, 64, 255);
        private static TMP_FontAsset s_font, s_mono, s_italic, s_display;
        private static ClubSportHudView s_view;
        private static ClubSportHudLayout s_layout;
        private static readonly Rect s_full = new Rect(0, 0, 1, 1);

        public static void RebuildRoundedPanel()
        {
            ClubSportHudAssets.Shape("RoundedPanel", true);
            AssetDatabase.SaveAssets();
            Debug.Log("Club Sport rounded panel regenerated.");
        }

        public static void Build()
        {
            Texture2D[] carLiveries = ClubSportCarLiveryAssets.Build();
            s_font = ClubSportHudAssets.Font("ClubSportSansBlack");
            s_mono = ClubSportHudAssets.Font("ClubSportMonoBold");
            s_italic = ClubSportHudAssets.Font("ClubSportSansBlackItalic");
            s_display = ClubSportHudAssets.Font("ClubSportDisplayBlack");
            RectTransform root = Element(null, "ClubSportHud", s_full);
            s_view = root.gameObject.AddComponent<ClubSportHudView>();
            s_layout = root.gameObject.AddComponent<ClubSportHudLayout>();
            Ref(s_view, "layout", s_layout);
            RectTransform content = Element(root, "Content", new Rect(0, 0, 0, 0));
            content.pivot = Vector2.zero;
            Ref(s_layout, "content", content);
            Header(content);
            Feedback(content);
            Dashboard(content);
            RectTransform board = Panel(content, "BoardArea", s_full, new Color32(251, 248, 239, 255));
            Panel(board, "Separator", new Rect(0, .99f, 1, .01f), new Color32(233, 227, 211, 255));
            Ref(s_layout, "boardArea", board);
            Ref(s_layout, "roadArea", Element(content, "RoadArea", s_full));
            s_layout.ApplySize(new Vector2(390, 780));
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, k_prefabPath);
            Object.DestroyImmediate(root.gameObject);
            WireCarLiveries(carLiveries);
            WireRace();
            AssetDatabase.SaveAssets();
            Debug.Log("Club Sport production HUD authored and wired.");
        }

        private static void Header(RectTransform content)
        {
            RectTransform header = Panel(content, "Header", s_full, s_cream);
            Ref(s_layout, "header", header);
            Panel(header, "PositionShadow", new Rect(.041f, .10f, .255f, .76f), s_ink, true);
            RectTransform position = Panel(header, "Position", new Rect(.034f, .14f, .255f, .76f), s_coral, true);
            Label(position, "Title", "POSITION", new Rect(.16f, .66f, .79f, .30f), 8, s_cream, s_italic).alignment = TextAlignmentOptions.MidlineLeft;
            Ref(s_view, "positionText", Label(position, "Value", "1<size=50%>/4</size>", new Rect(.1f, .02f, .8f, .71f), 32, s_cream, s_italic));
            RectTransform time = Element(header, "RaceTime", new Rect(.317f, .09f, .364f, .82f));
            Label(time, "Title", "RACE TIME", new Rect(0, .59f, 1, .32f), 10, s_ink).characterSpacing = 4;
            Ref(s_view, "timeText", Label(time, "Value", "00:00.00", new Rect(0, .09f, 1, .44f), 16, s_ink, s_mono));
            RectTransform laps = Panel(header, "Laps", new Rect(.729f, .14f, .235f, .76f), s_ink);
            laps.GetComponent<Image>().sprite = ClubSportHudAssets.Shape("RoundedPanel");
            laps.GetComponent<Image>().type = Image.Type.Sliced;
            Label(laps, "Title", "LAPS", new Rect(0, .65f, 1, .28f), 9, s_cream).characterSpacing = 4;
            Ref(s_view, "lapsText", Label(laps, "Value", "1<size=60%>/4</size>", new Rect(0, .04f, 1, .57f), 26, s_cream));
            Panel(header, "Separator", new Rect(0, 0, 1, .035f), s_ink);
        }

        private static void Feedback(RectTransform content)
        {
            RectTransform strip = Panel(content, "Feedback", s_full, s_cream);
            Ref(s_layout, "feedback", strip);
            Label(strip, "ScoreTitle", "SCORE", new Rect(.035f, .12f, .105f, .76f), 8, s_ink);
            RectTransform combo = Element(strip, "ActiveCombo", new Rect(.14f, .12f, .205f, .76f));
            Ref(s_view, "combo", combo.gameObject.AddComponent<CanvasGroup>());
            Ref(s_view, "pointsText", Label(combo, "Points", "+0", new Rect(0, 0, .56f, 1), 15, s_ink));
            RectTransform badge = Panel(combo, "Multiplier", new Rect(.52f, .06f, .48f, .88f), s_ink, true);
            Ref(s_view, "multiplierBadge", badge.GetComponent<Image>());
            TMP_Text multiplier = Label(badge, "Value", "1x", s_full, 18, s_cream);
            multiplier.fontStyle = FontStyles.Bold;
            multiplier.fontWeight = FontWeight.Bold;
            Ref(s_view, "multiplierText", multiplier);
            TMP_Text total = Label(strip, "BankedTotal", "0", new Rect(.37f, 0, .26f, 1), 31, s_gold);
            total.enableAutoSizing = true; total.fontSizeMin = 23; total.fontSizeMax = 31;
            Ref(s_view, "totalText", total);
            ClubSportStarView[] stars = new ClubSportStarView[3];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Star(strip, i, new Rect(.634f + i * .05f, .26f, .048f, .48f));
            }

            Refs(s_view, "stars", stars);
            RectTransform overtake = Element(strip, "Overtake", new Rect(.79f, .22f, .19f, .56f));
            CanvasGroup group = overtake.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            Ref(s_view, "overtake", group);
            Panel(overtake, "Accent", new Rect(0, 0, .03f, 1), s_coral);
            Label(overtake, "Title", "OVERTAKE", new Rect(.06f, 0, .94f, 1), 8, s_ink);
            Panel(strip, "Separator", new Rect(0, 0, 1, .025f), new Color32(220, 215, 197, 255));
        }

        private static ClubSportStarView Star(RectTransform parent, int index, Rect bounds)
        {
            RectTransform root = Element(parent, "Star" + index, bounds);
            ClubSportStarView star = root.gameObject.AddComponent<ClubSportStarView>();
            Sprite shape = ClubSportHudAssets.Shape("Star");
            Image glow = Panel(root, "Glow", new Rect(-.2f, -.2f, 1.4f, 1.4f), new Color(1, .8f, .3f, 0)).GetComponent<Image>();
            glow.sprite = shape; glow.preserveAspect = true;
            Image empty = Panel(root, "Empty", s_full, new Color32(215, 209, 187, 255)).GetComponent<Image>();
            empty.sprite = shape; empty.preserveAspect = true;
            Image fill = Panel(root, "Gold", s_full, s_gold).GetComponent<Image>();
            fill.sprite = shape; fill.preserveAspect = true; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 0;
            Image outline = Panel(root, "Outline", s_full, s_ink).GetComponent<Image>();
            outline.sprite = ClubSportHudAssets.Shape("StarOutline"); outline.preserveAspect = true;
            RectTransform[] sparks = new RectTransform[6];
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i] = Panel(root, "Spark" + i, new Rect(.5f, .5f, 0, 0), new Color(1, .75f, .2f, 0));
                sparks[i].sizeDelta = new Vector2(2, 3);
            }
            Ref(star, "fill", fill); Ref(star, "glow", glow); Refs(star, "sparks", sparks);
            return star;
        }

        private static void Dashboard(RectTransform content)
        {
            RectTransform dash = Panel(content, "Dashboard", s_full, s_cream);
            Ref(s_layout, "dashboard", dash);
            Panel(dash, "Trim", new Rect(0, .96f, 1, .04f), s_coral);
            Panel(dash, "InsetHairline", new Rect(.036f, .915f, .928f, .01f), new Color32(234, 223, 201, 255));
            TMP_Text speed = Label(dash, "SpeedValue", "0", new Rect(.25f, .40f, .5f, .52f), 48, s_ink, s_display);
            speed.fontStyle = FontStyles.Italic; speed.characterSpacing = -2; speed.alignment = TextAlignmentOptions.MidlineGeoAligned;
            Label(dash, "SpeedLabel", "SPEED", new Rect(.25f, .30f, .5f, .13f), 9, s_ink).characterSpacing = 6;
            RectTransform rpm = Element(dash, "Rpm", new Rect(.30f, .055f, .40f, .22f));
            RectTransform gear = Panel(rpm, "Transmission", new Rect(0, 0, .141f, 1), s_coral);
            Label(gear, "Value", "1", s_full, 19, s_cream);
            for (int i = 0; i < 14; i++)
            {
                Rect bounds = new Rect(.19f + i * .057f, 0, .05f, 1);
                Panel(rpm, "Empty" + i, bounds, new Color32(191, 191, 174, 255), true);
                Image fill = Panel(rpm, "Segment" + i, bounds, s_coral, true).GetComponent<Image>();
                fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0;
            }
            Label(dash, "RpmLabel", "RPM", new Rect(.30f, .005f, .40f, .07f), 6, s_ink);
            Control(dash, "Pulse", "PULSE", s_ink, true);
            Control(dash, "Turbo", "TURBO", s_coral, false);
        }

        private static void Control(RectTransform parent, string name, string text, Color color, bool left)
        {
            RectTransform rect = Panel(parent, name, new Rect(left ? 0 : 1, .50f, 0, 0), color);
            rect.GetComponent<Image>().sprite = ClubSportHudAssets.Shape("RoundedPanel");
            rect.GetComponent<Image>().type = Image.Type.Sliced;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.interactable = false;
            ColorBlock colors = button.colors;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            Outline border = rect.gameObject.AddComponent<Outline>();
            border.effectColor = new Color32(53, 65, 74, 64);
            border.effectDistance = new Vector2(1f, -1f);
            border.useGraphicAlpha = true;
            Shadow depth = rect.gameObject.AddComponent<Shadow>();
            depth.effectColor = left ? new Color32(36, 45, 52, 220) : new Color32(190, 65, 70, 220);
            depth.effectDistance = new Vector2(0, -4);
            depth.useGraphicAlpha = true;
            rect.GetComponent<Image>().raycastTarget = true;
            Label(rect, "Title", text, new Rect(.34f, .18f, .61f, .64f), 9, s_cream);
            Image icon = Panel(rect, "Icon", new Rect(.06f, .25f, .26f, .5f), s_cream).GetComponent<Image>();
            icon.sprite = ClubSportHudAssets.Shape(left ? "ButtonGear" : "ButtonTurbo");
            icon.preserveAspect = true;
            Ref(s_view, left ? "pulseButton" : "turboButton", button);
            Ref(s_layout, left ? "pulseButton" : "turboButton", rect);
        }

        private static void WireCarLiveries(Texture2D[] liveries)
        {
            const string carPath = "Assets/GearEngine/Prefabs/Tracks/CarView.prefab";
            GameObject car = PrefabUtility.LoadPrefabContents(carPath);
            try
            {
                ClubSportCarLiveryView view = car.GetComponent<ClubSportCarLiveryView>();
                if (view == null)
                {
                    view = car.AddComponent<ClubSportCarLiveryView>();
                }

                Refs(view, "bodyTextures", liveries);
                PrefabUtility.SaveAsPrefabAsset(car, carPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(car);
            }
        }

        private static void WireRace()
        {
            GameObject race = PrefabUtility.LoadPrefabContents(k_racePath);
            try
            {
                Transform old = race.transform.Find("ClubSportHud");
                if (old != null)
                {
                    Object.DestroyImmediate(old.gameObject);
                }

                GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(k_prefabPath), race.transform);
                hud.name = "ClubSportHud";
                Transform legacy = race.transform.Find("Container");
                if (legacy != null)
                {
                    legacy.gameObject.SetActive(false);
                }

                ActiveRaceView raceView = race.GetComponent<ActiveRaceView>();
                Ref(raceView, "clubSportHud", hud.GetComponent<ClubSportHudView>());
                Ref(raceView, "currentVelocityText", hud.transform.Find("Content/Dashboard/SpeedValue").GetComponent<TMP_Text>());
                Ref(raceView, "currentGearText", hud.transform.Find("Content/Dashboard/Rpm/Transmission/Value").GetComponent<TMP_Text>());
                Image[] segments = new Image[14];
                for (int i = 0; i < segments.Length; i++)
                {
                    segments[i] = hud.transform.Find("Content/Dashboard/Rpm/Segment" + i).GetComponent<Image>();
                }

                Refs(raceView, "rpmSegments", segments);
                PrefabUtility.SaveAsPrefabAsset(race, k_racePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(race); }
        }

        internal static RectTransform Element(Transform parent, string name, Rect bounds)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5;
            rect.SetParent(parent, false);
            rect.anchorMin = bounds.min; rect.anchorMax = bounds.max;
            rect.sizeDelta = rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private static RectTransform Panel(Transform parent, string name, Rect bounds, Color color, bool slanted = false)
        {
            RectTransform rect = Element(parent, name, bounds);
            Image image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
            if (slanted)
            {
                image.sprite = ClubSportHudAssets.Shape("SlantedPanel");
            }

            return rect;
        }

        private static TMP_Text Label(Transform parent, string name, string text, Rect bounds, float size, Color color, TMP_FontAsset typeface = null)
        {
            TMP_Text label = Element(parent, name, bounds).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = typeface != null ? typeface : s_font; label.text = text; label.fontSize = size; label.color = color;
            label.alignment = TextAlignmentOptions.Midline; label.textWrappingMode = TextWrappingModes.NoWrap; label.raycastTarget = false;
            return label;
        }

        internal static void Ref(Object target, string field, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field) ?? throw new InvalidOperationException(field);
            property.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Refs(Object target, string field, Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target); SerializedProperty array = serialized.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
