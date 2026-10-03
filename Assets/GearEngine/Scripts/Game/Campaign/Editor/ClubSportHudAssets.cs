using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Campaign.Editor
{
    internal static class ClubSportHudAssets
    {
        internal const string k_folder = "Assets/GearEngine/Art/UI/ClubSport";
        internal static TMP_FontAsset Font(string name)
        {
            Directory.CreateDirectory(k_folder + "/Fonts");
            string path = k_folder + "/Fonts/" + name;
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path + ".asset");
            if (existing != null)
            {
                return existing;
            }

            File.Copy("Assets/_Dev/ClubSport/Review/Fonts/" + name + ".ttf", path + ".ttf", true);
            AssetDatabase.ImportAsset(path + ".ttf", ImportAssetOptions.ForceSynchronousImport);
            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(path + ".ttf"));
            font.name = name;
            if (!font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 /:,.+×-", out string missing))
            {
                throw new InvalidOperationException("Missing HUD characters: " + missing);
            }

            font.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font, path + ".asset");
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (Texture2D atlas in font.atlasTextures)
            {
                AssetDatabase.AddObjectToAsset(atlas, font);
            }

            return font;
        }

        internal static Sprite Shape(string name, bool forceRebuild = false)
        {
            Directory.CreateDirectory(k_folder);
            string path = k_folder + "/T_" + name + ".png";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null && !forceRebuild)
            {
                return existing;
            }

            const int size = 256;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            Vector2[] star = StarVertices();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2((x + .5f) / size, (y + .5f) / size);
                    bool inside = name.StartsWith("Star")
                        ? Inside(point, star)
                        : name == "RoundedPanel"
                            ? InsideRoundedRectangle(point, 18f / size)
                            : point.x > .16f * point.y && point.x < .84f + .16f * point.y;
                    if (name == "ButtonGear")
                    {
                        Vector2 offset = (point - Vector2.one * .5f) * 32f;
                        inside = offset.magnitude > 5.5f && offset.magnitude < 10.5f;
                        for (int tooth = 0; tooth < 8; tooth++)
                        {
                            float angle = tooth * Mathf.PI / 4f;
                            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                            float along = Vector2.Dot(offset, direction);
                            float across = Mathf.Abs(offset.x * direction.y - offset.y * direction.x);
                            inside |= along > 9f && along < 14f && across < 2f;
                        }
                    }
                    if (name == "ButtonTurbo")
                    {
                        Vector2 p = new Vector2(point.x * 32, (1 - point.y) * 32);
                        inside = Inside(p, new[] { new Vector2(17, 2), new Vector2(19, 9), new Vector2(24, 14), new Vector2(26, 20), new Vector2(25, 25), new Vector2(21, 29), new Vector2(16, 30), new Vector2(11, 29), new Vector2(7, 25), new Vector2(6, 20), new Vector2(8, 14), new Vector2(11, 10), new Vector2(11, 14), new Vector2(14, 16), new Vector2(17, 10) });
                        inside &= !Inside(p, new[] { new Vector2(17, 17), new Vector2(18, 21), new Vector2(21, 25), new Vector2(19, 29), new Vector2(13, 29), new Vector2(11, 25), new Vector2(14, 19), new Vector2(14, 22) });
                    }
                    if (name == "StarOutline")
                    {
                        inside &= !Inside((point - Vector2.one * .5f) / .76f + Vector2.one * .5f, star);
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, inside ? (byte)255 : (byte)0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Vector2[] StarVertices()
        {
            Vector2[] points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? .49f : .225f;
                points[i] = Vector2.one * .5f + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return points;
        }

        private static bool Inside(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i], b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static bool InsideRoundedRectangle(Vector2 point, float radius)
        {
            if (point.x >= radius && point.x <= 1f - radius || point.y >= radius && point.y <= 1f - radius)
            {
                return true;
            }

            Vector2 corner = new Vector2(point.x < .5f ? radius : 1f - radius, point.y < .5f ? radius : 1f - radius);
            return Vector2.SqrMagnitude(point - corner) <= radius * radius;
        }
    }
}
