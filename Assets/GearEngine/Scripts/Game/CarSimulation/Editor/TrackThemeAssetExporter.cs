using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Tracks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GearEngine.CarSimulation.Editor
{
    public static class TrackThemeAssetExporter
    {
        private sealed class ThemeSpec
        {
            public string Name { get; set; }
            public string RepresentativeTrack { get; set; }
            public string BackgroundModelPath { get; set; }
            public string GroundTexturePath { get; set; }
            public string[] PropModelPaths { get; set; }
            public string[] PropTexturePaths { get; set; }
            public string[] PreparedPropPrefabPaths { get; set; }
            public Color GroundTint { get; set; }
            public Color RoadColor { get; set; }
            public Color PropTint { get; set; }
            public Color SkyColor { get; set; }
            public float PropHeight { get; set; }
            public bool UseDefaultProps { get; set; }
        }

        private static string MaterialsFolder => "Assets/GearEngine/Art/Materials/TrackThemes";
        private static string PrefabsFolder => "Assets/GearEngine/Prefabs/Tracks/Themes";
        private static string ThemesFolder => "Assets/GearEngine/Data/Track/Themes";
        private static string TracksFolder => "Assets/GearEngine/Data/Track/Tracks";
        private static string TrackViewPrefabPath => "Assets/GearEngine/Prefabs/Tracks/TrackViewComponent.prefab";
        private static string ConePrefabPath => "Assets/PROMETEO - Car Controller/Prefabs/Cone.prefab";

        [MenuItem("Tools/Gear Engine/Track Themes/Build And Export")]
        public static void BuildAndExport()
        {
            try
            {
                EnsureOutputFolders();
                IReadOnlyList<ThemeSpec> specs = CreateThemeSpecs();
                Dictionary<string, TrackThemeDefinition> themes = BuildThemeAssets(specs);
                AssignThemesToTracks(themes);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ExportVisuals(specs);
                ExportUnityPackage();
                Debug.Log("[TrackThemes] Built four themes, assigned all tracks, and exported visual evidence.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TrackThemes] Export failed: {exception.Message}\n{exception.StackTrace}");
                throw;
            }
        }

        private static IReadOnlyList<ThemeSpec> CreateThemeSpecs()
        {
            return new[]
            {
                new ThemeSpec
                {
                    Name = "Desert",
                    RepresentativeTrack = "Figure8Track",
                    PropModelPaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Cactus/FBX/cactus_mesh.fbx",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Rock3/FBX/rock3_mesh.fbx",
                    },
                    PropTexturePaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Cactus/Textures/T_Cactus_BaseColor.png",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Rock3/Texture/T_Rock3_BaseColor.png",
                    },
                    PreparedPropPrefabPaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Cactus/Prefab/cactus_mesh.prefab",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Cactus/Prefab/cactus_mesh Variant 1.prefab",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Rock3/Prefab/rock3_mesh.prefab",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Desert/Rock3/Prefab/rock3_mesh Variant 1.prefab",
                    },
                    GroundTint = new Color(1f, 0.86f, 0.58f),
                    RoadColor = new Color(0.07f, 0.05f, 0.03f),
                    PropTint = Color.white,
                    SkyColor = new Color(0.96f, 0.79f, 0.49f),
                    PropHeight = 6f,
                },
                new ThemeSpec
                {
                    Name = "Forest",
                    RepresentativeTrack = "RoundedSquareTrack",
                    BackgroundModelPath = "Assets/GearEngine/Art/ModelsTextures/Background/Forest/FBX/background_mesh.fbx",
                    GroundTexturePath = "Assets/GearEngine/Art/ModelsTextures/Background/Forest/Texture/T_Forest_BaseColor.png",
                    PropModelPaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Forest/Tree/FBX/tree_mesh.fbx",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Forest/Tree2/FBX/tree2_mesh.fbx",
                    },
                    PropTexturePaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Forest/Tree/Textures/T_Tree_BaseColor.png",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Forest/Tree2/Texture/T_Tree2_BaseColor.png",
                    },
                    GroundTint = new Color(0.70f, 0.88f, 0.57f),
                    RoadColor = new Color(0.10f, 0.19f, 0.13f),
                    PropTint = Color.white,
                    SkyColor = new Color(0.62f, 0.82f, 0.65f),
                    PropHeight = 8f,
                },
                new ThemeSpec
                {
                    Name = "Mountain",
                    RepresentativeTrack = "HairpinTrack",
                    BackgroundModelPath = "Assets/GearEngine/Art/ModelsTextures/Background/Rock/FBX/background_mesh.fbx",
                    GroundTexturePath = "Assets/GearEngine/Art/ModelsTextures/Background/Rock/Textures/T_Rock_BaseColor.png",
                    PropModelPaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Rock/Rock/FBX/rock_mesh.fbx",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Rock/Rock2/FBX/rock2_mesh.fbx",
                    },
                    PropTexturePaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Rock/Rock/Texture/T_Rock_BaseColor.png",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Rock/Rock2/Texture/T_Rock2_BaseColor.png",
                    },
                    GroundTint = new Color(0.72f, 0.70f, 0.67f),
                    RoadColor = new Color(0.17f, 0.19f, 0.22f),
                    PropTint = Color.white,
                    SkyColor = new Color(0.65f, 0.68f, 0.73f),
                    PropHeight = 5f,
                },
                new ThemeSpec
                {
                    Name = "Glacial",
                    RepresentativeTrack = "StarTrack",
                    BackgroundModelPath = "Assets/GearEngine/Art/ModelsTextures/Background/Glacial/FBX/background_mesh.fbx",
                    GroundTexturePath = "Assets/GearEngine/Art/ModelsTextures/Background/Glacial/Textures/T_Glacial_BaseColor.png",
                    PropModelPaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Glacial/Crystal/FBX/crystal_mesh.fbx",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Glacial/Crystal2/FBX/crystal2_mesh.fbx",
                    },
                    PropTexturePaths = new[]
                    {
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Glacial/Crystal/Texture/T_Crystal_BaseColor.png",
                        "Assets/GearEngine/Art/ModelsTextures/AssetsBackground/Glacial/Crystal2/Texture/T_Crystal2_BaseColor.png",
                    },
                    GroundTint = new Color(0.79f, 0.94f, 1f),
                    RoadColor = new Color(0.07f, 0.24f, 0.33f),
                    PropTint = new Color(0.82f, 0.96f, 1f),
                    SkyColor = new Color(0.70f, 0.88f, 0.96f),
                    PropHeight = 6f,
                },
            };
        }

        private static void EnsureOutputFolders()
        {
            EnsureAssetFolder(MaterialsFolder);
            EnsureAssetFolder(PrefabsFolder);
            EnsureAssetFolder(ThemesFolder);
            Directory.CreateDirectory(GetArtifactFolder("VisualTests/TrackThemes"));
            Directory.CreateDirectory(GetArtifactFolder("Exports/TrackThemes"));
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private static Dictionary<string, TrackThemeDefinition> BuildThemeAssets(IReadOnlyList<ThemeSpec> specs)
        {
            Dictionary<string, TrackThemeDefinition> themes = new Dictionary<string, TrackThemeDefinition>(StringComparer.Ordinal);
            GameObject conePrefab = CreateOrUpdatePreparedPrefab(
                ConePrefabPath,
                $"{PrefabsFolder}/PFB_ThemeConeObstacle.prefab",
                1.2f);
            foreach (ThemeSpec spec in specs)
            {
                Material groundMaterial = CreateOrUpdateMaterial(
                    $"{MaterialsFolder}/M_Theme{spec.Name}Ground.mat",
                    spec.GroundTexturePath,
                    spec.GroundTint,
                    0.12f,
                    spec.Name == "Desert");
                Material roadMaterial = CreateOrUpdateMaterial(
                    $"{MaterialsFolder}/M_Theme{spec.Name}Road.mat",
                    null,
                    spec.RoadColor,
                    0.32f,
                    true);
                Material environmentMaterial = CreateOrUpdateMaterial(
                    $"{MaterialsFolder}/M_Theme{spec.Name}Environment.mat",
                    spec.GroundTexturePath,
                    Color.white,
                    0.08f);

                GameObject environmentPrefab = BuildEnvironmentPrefab(spec, environmentMaterial);
                List<GameObject> propPrefabs = BuildPropPrefabs(spec);
                DeleteUnusedGeneratedProps(spec);
                themes.Add(
                    spec.Name,
                    CreateOrUpdateTheme(spec, environmentPrefab, roadMaterial, groundMaterial, propPrefabs, conePrefab));
            }

            return themes;
        }

        private static GameObject BuildEnvironmentPrefab(ThemeSpec spec, Material environmentMaterial)
        {
            string prefabPath = $"{PrefabsFolder}/PFB_Theme{spec.Name}Environment.prefab";
            if (string.IsNullOrEmpty(spec.BackgroundModelPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
                return null;
            }

            return CreateOrUpdateModelPrefab(
                spec.BackgroundModelPath,
                environmentMaterial,
                prefabPath,
                240f,
                true);
        }

        private static List<GameObject> BuildPropPrefabs(ThemeSpec spec)
        {
            if (spec.PreparedPropPrefabPaths != null && spec.PreparedPropPrefabPaths.Length > 0)
            {
                List<GameObject> preparedPrefabs = new List<GameObject>();
                for (int index = 0; index < spec.PreparedPropPrefabPaths.Length; index++)
                {
                    preparedPrefabs.Add(CreateOrUpdatePreparedPrefab(
                        spec.PreparedPropPrefabPaths[index],
                        $"{PrefabsFolder}/PFB_Theme{spec.Name}Prop{index + 1}.prefab",
                        spec.PropHeight * (1f - index * 0.08f)));
                }

                return preparedPrefabs;
            }

            List<GameObject> prefabs = new List<GameObject>();
            for (int index = 0; index < spec.PropModelPaths.Length; index++)
            {
                Material material = CreateOrUpdateMaterial(
                    $"{MaterialsFolder}/M_Theme{spec.Name}Prop{index + 1}.mat",
                    spec.PropTexturePaths[index],
                    spec.PropTint,
                    spec.Name == "Glacial" ? 0.65f : 0.18f);
                prefabs.Add(CreateOrUpdateModelPrefab(
                    spec.PropModelPaths[index],
                    material,
                    $"{PrefabsFolder}/PFB_Theme{spec.Name}Prop{index + 1}.prefab",
                    spec.PropHeight * (1f - index * 0.12f),
                    false));
            }

            return prefabs;
        }

        private static GameObject CreateOrUpdatePreparedPrefab(
            string sourcePath,
            string prefabPath,
            float targetSize)
        {
            GameObject source = LoadRequiredAsset<GameObject>(sourcePath);
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            try
            {
                GameObject model = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (model == null)
                {
                    throw new InvalidOperationException($"Could not instantiate prefab '{sourcePath}'.");
                }

                model.transform.SetParent(root.transform, false);
                NormalizePreparedModel(root, model, targetSize);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void NormalizePreparedModel(GameObject root, GameObject model, float targetSize)
        {
            Bounds bounds = CalculateBounds(root);
            float sourceSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (sourceSize <= Mathf.Epsilon)
            {
                throw new InvalidOperationException($"Prefab '{model.name}' has no renderer bounds.");
            }

            model.transform.localScale *= targetSize / sourceSize;
            bounds = CalculateBounds(root);
            model.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        }

        private static void DeleteUnusedGeneratedProps(ThemeSpec spec)
        {
            if (!spec.UseDefaultProps)
            {
                return;
            }

            for (int index = 0; index < spec.PropModelPaths.Length; index++)
            {
                AssetDatabase.DeleteAsset($"{PrefabsFolder}/PFB_Theme{spec.Name}Prop{index + 1}.prefab");
            }
        }

        private static Material CreateOrUpdateMaterial(
            string assetPath,
            string texturePath,
            Color color,
            float smoothness,
            bool unlit = false)
        {
            string shaderName = unlit
                ? "Universal Render Pipeline/Unlit"
                : "Universal Render Pipeline/Lit";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{shaderName}' was not found.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else
            {
                material.shader = shader;
            }

            Texture2D texture = string.IsNullOrEmpty(texturePath)
                ? null
                : LoadRequiredAsset<Texture2D>(texturePath);
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateOrUpdateModelPrefab(
            string modelPath,
            Material material,
            string prefabPath,
            float targetSize,
            bool normalizeHorizontal)
        {
            GameObject source = LoadRequiredAsset<GameObject>(modelPath);
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            try
            {
                GameObject model = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (model == null)
                {
                    throw new InvalidOperationException($"Could not instantiate model '{modelPath}'.");
                }

                model.transform.SetParent(root.transform, false);
                ApplyMaterial(model, material);
                NormalizeModel(root, model, targetSize, normalizeHorizontal);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ApplyMaterial(GameObject root, Material material)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++)
                {
                    materials[index] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static void NormalizeModel(
            GameObject root,
            GameObject model,
            float targetSize,
            bool normalizeHorizontal)
        {
            Bounds bounds = CalculateBounds(root);
            float sourceSize = normalizeHorizontal
                ? Mathf.Max(bounds.size.x, bounds.size.z)
                : bounds.size.y;
            if (sourceSize <= Mathf.Epsilon)
            {
                throw new InvalidOperationException($"Model '{model.name}' has no renderer bounds.");
            }

            model.transform.localScale *= targetSize / sourceSize;
            bounds = CalculateBounds(root);
            float targetY = normalizeHorizontal ? -6f : 0f;
            model.transform.position += new Vector3(
                -bounds.center.x,
                targetY - bounds.min.y,
                -bounds.center.z);
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static TrackThemeDefinition CreateOrUpdateTheme(
            ThemeSpec spec,
            GameObject environmentPrefab,
            Material roadMaterial,
            Material groundMaterial,
            IReadOnlyList<GameObject> propPrefabs,
            GameObject conePrefab)
        {
            string assetPath = $"{ThemesFolder}/{spec.Name}TrackTheme.asset";
            TrackThemeDefinition theme = AssetDatabase.LoadAssetAtPath<TrackThemeDefinition>(assetPath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();
                AssetDatabase.CreateAsset(theme, assetPath);
            }

            SerializedObject serializedTheme = new SerializedObject(theme);
            serializedTheme.FindProperty("displayName").stringValue = spec.Name;
            serializedTheme.FindProperty("environmentPrefab").objectReferenceValue = environmentPrefab;
            serializedTheme.FindProperty("environmentLocalPosition").vector3Value = Vector3.zero;
            serializedTheme.FindProperty("environmentLocalEulerAngles").vector3Value = Vector3.zero;
            serializedTheme.FindProperty("environmentLocalScale").vector3Value = Vector3.one;
            serializedTheme.FindProperty("roadMaterial").objectReferenceValue = roadMaterial;
            serializedTheme.FindProperty("groundMaterial").objectReferenceValue = groundMaterial;
            serializedTheme.FindProperty("hideBaseGround").boolValue = false;
            serializedTheme.FindProperty("useDefaultProps").boolValue = spec.UseDefaultProps;
            PopulatePropRules(serializedTheme.FindProperty("propRules"), propPrefabs, conePrefab);
            serializedTheme.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            return theme;
        }

        private static void PopulatePropRules(
            SerializedProperty property,
            IReadOnlyList<GameObject> prefabs,
            GameObject conePrefab)
        {
            property.arraySize = 0;
            if (prefabs.Count == 0)
            {
                return;
            }

            SplinePropGenerator.All edgeRule = new SplinePropGenerator.All
            {
                prefabs = prefabs.ToList(),
                chancePerSegment = 0.92f,
                minAmount = 3,
                maxAmount = 5,
                randomOffsetRange = new Vector2(0.5f, 1.5f),
                randomYRotation = true,
                minSeparation = 5f,
                minimumSplineClearance = 8.5f,
                side = SplinePropGenerator.PlacementSide.Both,
                leftDistance = 11f,
                rightDistance = 11f,
            };
            AddManagedReference(property, edgeRule);

            SplinePropGenerator.Inside insideRule = new SplinePropGenerator.Inside
            {
                prefabs = prefabs.ToList(),
                chancePerSegment = 0.55f,
                minAmount = 2,
                maxAmount = 4,
                randomOffsetRange = new Vector2(1f, 2f),
                randomYRotation = true,
                minSeparation = 5f,
                minimumSplineClearance = 8.5f,
                maxDistance = 28f,
                minSplineDistance = 12f,
            };
            AddManagedReference(property, insideRule);

            SplinePropGenerator.Straights coneRule = new SplinePropGenerator.Straights
            {
                prefabs = new List<GameObject> { conePrefab },
                chancePerSegment = 0.28f,
                minAmount = 1,
                maxAmount = 2,
                randomOffsetRange = new Vector2(0.2f, 0.75f),
                randomYRotation = true,
                minSeparation = 8f,
                minimumSplineClearance = 3.8f,
                curvatureThreshold = 8f,
                side = SplinePropGenerator.PlacementSide.Both,
                leftDistance = 4.8f,
                rightDistance = 4.8f,
            };
            AddManagedReference(property, coneRule);
        }

        private static void AddManagedReference(SerializedProperty property, object value)
        {
            int index = property.arraySize;
            property.InsertArrayElementAtIndex(index);
            property.GetArrayElementAtIndex(index).managedReferenceValue = value;
        }

        private static void AssignThemesToTracks(IReadOnlyDictionary<string, TrackThemeDefinition> themes)
        {
            IReadOnlyDictionary<string, string> assignments = CreateTrackAssignments();
            foreach ((string trackName, string themeName) in assignments)
            {
                string trackPath = $"{TracksFolder}/{trackName}.asset";
                TrackDefinition track = LoadRequiredAsset<TrackDefinition>(trackPath);
                SerializedObject serializedTrack = new SerializedObject(track);
                serializedTrack.FindProperty("theme").objectReferenceValue = themes[themeName];
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(track);
            }

            string[] trackGuids = AssetDatabase.FindAssets("t:TrackDefinition", new[] { TracksFolder });
            if (trackGuids.Length != assignments.Count)
            {
                throw new InvalidOperationException(
                    $"Assigned {assignments.Count} tracks, but {trackGuids.Length} TrackDefinition assets exist.");
            }
        }

        private static IReadOnlyDictionary<string, string> CreateTrackAssignments()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CircleTrack"] = "Desert",
                ["DShapeTrack"] = "Desert",
                ["Figure8Track"] = "Desert",
                ["OvalTrack"] = "Desert",
                ["InfinityTrack"] = "Forest",
                ["KidneyTrack"] = "Forest",
                ["PeanutTrack"] = "Forest",
                ["RoundedSquareTrack"] = "Forest",
                ["HairpinTrack"] = "Mountain",
                ["LShapeTrack"] = "Mountain",
                ["SCurveTrack"] = "Mountain",
                ["StarTrack"] = "Glacial",
                ["TriangleTrack"] = "Glacial",
                ["ZigZagTrack"] = "Glacial",
            };
        }

        private static void ExportVisuals(IReadOnlyList<ThemeSpec> specs)
        {
            for (int index = 0; index < specs.Count; index++)
            {
                ExportThemeVisual(specs[index], index);
            }
        }

        private static void ExportThemeVisual(ThemeSpec spec, int seedOffset)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            UnityEngine.Random.InitState(8300 + seedOffset);

            GameObject trackViewPrefab = LoadRequiredAsset<GameObject>(TrackViewPrefabPath);
            TrackDefinition track = LoadRequiredAsset<TrackDefinition>(
                $"{TracksFolder}/{spec.RepresentativeTrack}.asset");
            if (track.Theme == null || !string.Equals(track.Theme.DisplayName, spec.Name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Representative track '{track.name}' is not using the expected '{spec.Name}' theme.");
            }

            GameObject trackObject = PrefabUtility.InstantiatePrefab(trackViewPrefab) as GameObject;
            if (trackObject == null)
            {
                throw new InvalidOperationException("Track view prefab could not be instantiated.");
            }

            TrackViewComponent trackView = trackObject.GetComponent<TrackViewComponent>();
            trackView.InitializeTrack(track);
            ValidateAppliedTheme(trackObject, track.Theme);
            trackView.GenerateProps();

            Bounds bounds = CalculateTrackBounds(trackObject);
            Camera camera = CreateCamera(bounds, spec.SkyColor);
            CreateDirectionalLight();
            string outputPath = Path.Combine(
                GetArtifactFolder("VisualTests/TrackThemes"),
                $"{spec.Name}{track.GetDisplayName()}1200x900.png");
            RenderCamera(camera, outputPath, 1200, 900);
        }

        private static void ValidateAppliedTheme(GameObject trackObject, TrackThemeDefinition theme)
        {
            MeshRenderer roadRenderer = trackObject.transform.Find("Track/Path")?.GetComponent<MeshRenderer>();
            MeshRenderer groundRenderer = trackObject.transform.Find("Floor")?.GetComponent<MeshRenderer>();
            if (roadRenderer == null || roadRenderer.sharedMaterial != theme.RoadMaterial)
            {
                throw new InvalidOperationException($"Theme '{theme.DisplayName}' did not apply its road material.");
            }

            if (groundRenderer == null || groundRenderer.sharedMaterial != theme.GroundMaterial)
            {
                throw new InvalidOperationException($"Theme '{theme.DisplayName}' did not apply its ground material.");
            }
        }

        private static Bounds CalculateTrackBounds(GameObject trackObject)
        {
            Transform path = trackObject.transform.Find("Track/Path");
            Renderer roadRenderer = path == null ? null : path.GetComponent<Renderer>();
            return roadRenderer == null ? CalculateBounds(trackObject) : roadRenderer.bounds;
        }

        private static Camera CreateCamera(Bounds bounds, Color backgroundColor)
        {
            GameObject cameraObject = new GameObject("ThemePreviewCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.allowHDR = false;
            camera.allowMSAA = true;

            float radius = Mathf.Max(70f, Mathf.Max(bounds.extents.x, bounds.extents.z));
            Vector3 focus = new Vector3(bounds.center.x, 0f, bounds.center.z);
            camera.transform.position = focus + new Vector3(0f, radius * 1.8f, -radius * 1.35f);
            camera.transform.LookAt(focus + Vector3.up * 1.5f);
            return camera;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = new GameObject("ThemePreviewLight");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = new Color(1f, 0.95f, 0.86f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.68f);
        }

        private static void RenderCamera(Camera camera, string outputPath, int width, int height)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
            };
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                Debug.Log($"[TrackThemes] Visual exported: {outputPath}");
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void ExportUnityPackage()
        {
            string outputPath = Path.Combine(
                GetArtifactFolder("Exports/TrackThemes"),
                "GearEngineTrackThemes.unitypackage");
            string[] assetPaths =
            {
                MaterialsFolder,
                PrefabsFolder,
                ThemesFolder,
                TracksFolder,
                "Assets/GearEngine/Art/ModelsTextures/Background",
                "Assets/GearEngine/Art/ModelsTextures/AssetsBackground",
                "Assets/GearEngine/Scripts/Game/CarSimulation/Definitions/TrackDefinition.cs",
                "Assets/GearEngine/Scripts/Game/CarSimulation/Definitions/TrackThemeDefinition.cs",
                "Assets/GearEngine/Scripts/Game/CarSimulation/Tracks/SplinePropGenerator.cs",
                "Assets/GearEngine/Scripts/Game/CarSimulation/Tracks/TrackViewComponent.cs",
            };
            AssetDatabase.ExportPackage(
                assetPaths,
                outputPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log($"[TrackThemes] Unity package exported: {outputPath}");
        }

        private static string GetArtifactFolder(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve the Unity project root.");
            return Path.Combine(projectRoot, "Artifacts", relativePath);
        }

        private static T LoadRequiredAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Required asset was not found at '{assetPath}'.");
            }

            return asset;
        }
    }
}
