using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Editor
{
    internal static class ClubSportCarLiveryAssets
    {
        internal const string k_folder = "Assets/GearEngine/Art/Textures/ClubSport";
        private const string k_sourcePath = "Assets/PROMETEO - Car Controller/Textures/PCC_TextureAtlas.png";
        private static readonly string[] s_names =
        {
            "T_ClubSportCarCoral",
            "T_ClubSportCarCream",
            "T_ClubSportCarTeal",
            "T_ClubSportCarBurgundy"
        };
        private static readonly Color[] s_colors =
        {
            new Color32(227, 93, 80, 255),
            new Color32(241, 215, 134, 255),
            new Color32(47, 127, 134, 255),
            new Color32(138, 73, 96, 255)
        };

        internal static Texture2D[] Build()
        {
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(k_sourcePath);
            if (source == null)
            {
                throw new InvalidOperationException("Club Sport car liveries require the production car base-color texture.");
            }

            Directory.CreateDirectory(k_folder);
            AssetDatabase.Refresh();
            Texture2D[] liveries = new Texture2D[s_colors.Length];
            for (int i = 0; i < liveries.Length; i++)
            {
                liveries[i] = BuildLivery(source, s_colors[i], s_names[i]);
            }

            return liveries;
        }

        private static Texture2D BuildLivery(Texture2D source, Color paint, string name)
        {
            string path = k_folder + "/" + name + ".asset";
            RenderTexture previous = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(
                source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            try
            {
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                result.ReadPixels(new Rect(0f, 0f, source.width, source.height), 0, 0);
                Color[] pixels = result.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color pixel = pixels[i];
                    if (pixel.r > 0.3f && pixel.r > pixel.g * 2f && pixel.r > pixel.b * 2f)
                    {
                        pixels[i] = new Color(
                            paint.r * pixel.r,
                            paint.g * pixel.r,
                            paint.b * pixel.r,
                            pixel.a);
                    }
                }

                result.SetPixels(pixels);
                result.Apply(false, false);
                Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(result, path);
                    return result;
                }

                EditorUtility.CopySerialized(result, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(result);
                return existing;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
        }
    }
}
