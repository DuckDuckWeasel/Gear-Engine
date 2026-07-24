using System.Reflection;
using Coffee.UIEffectInternal;
using Coffee.UIEffects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    [Category("Unit")]
    public sealed class UIEffectPatternLayerTests
    {
        private GameObject target;
        private Texture2D texture;
        private UIEffectPreset preset;

        [TearDown]
        public void TearDown()
        {
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }

            if (texture != null)
            {
                Object.DestroyImmediate(texture);
            }

            if (preset != null)
            {
                Object.DestroyImmediate(preset);
            }
        }

        [Test]
        public void PresetMigration_LegacyValuesPopulateLayerZero()
        {
            texture = CreateTexture(Color.white);
            preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            preset.m_TransitionTex = texture;
            preset.m_TransitionTexScale = new Vector2(2, 3);
            preset.m_TransitionTexOffset = new Vector2(0.2f, 0.3f);
            preset.m_TransitionTexSpeed = new Vector2(0.4f, 0.5f);
            preset.m_TransitionRotation = 45;
            preset.m_TransitionKeepAspectRatio = false;
            preset.m_TransitionRate = 0.25f;
            preset.m_TransitionReverse = true;
            preset.m_TransitionWidth = 0.6f;
            preset.m_TransitionRange = new MinMax01(0.1f, 0.8f);
            preset.m_TransitionPatternReverse = true;
            preset.m_TransitionAutoPlaySpeed = 1.5f;
            preset.m_TransitionColorFilter = ColorFilter.Replace;
            preset.m_TransitionColor = Color.magenta;
            preset.m_TransitionColorGlow = true;
            preset.m_PatternArea = PatternArea.Edge;
            ResetMigrationState(preset);

            preset.OnAfterDeserialize();

            PatternLayer layer = preset.GetPatternLayer(0);
            Assert.That(layer.m_Enabled, Is.True);
            Assert.That(layer.m_Texture, Is.SameAs(texture));
            Assert.That(layer.m_Opacity, Is.EqualTo(1));
            Assert.That(layer.m_TextureScale, Is.EqualTo(new Vector2(2, 3)));
            Assert.That(layer.m_TextureOffset, Is.EqualTo(new Vector2(0.2f, 0.3f)));
            Assert.That(layer.m_TextureSpeed, Is.EqualTo(new Vector2(0.4f, 0.5f)));
            Assert.That(layer.m_Rotation, Is.EqualTo(45));
            Assert.That(layer.m_KeepAspectRatio, Is.False);
            Assert.That(layer.m_Rate, Is.EqualTo(0.25f));
            Assert.That(layer.m_TextureReverse, Is.True);
            Assert.That(layer.m_Width, Is.EqualTo(0.6f));
            Assert.That(layer.m_Range.min, Is.EqualTo(0.1f));
            Assert.That(layer.m_Range.max, Is.EqualTo(0.8f));
            Assert.That(layer.m_PatternReverse, Is.True);
            Assert.That(layer.m_AutoPlaySpeed, Is.EqualTo(1.5f));
            Assert.That(layer.m_ColorFilter, Is.EqualTo(ColorFilter.Replace));
            Assert.That(layer.m_Color, Is.EqualTo(Color.magenta));
            Assert.That(layer.m_ColorGlow, Is.True);
            Assert.That(layer.m_Area, Is.EqualTo(PatternArea.Edge));
        }

        [Test]
        public void ComponentMigration_LegacyValuesPopulateLayerZero()
        {
            texture = CreateTexture(Color.white);
            target = new GameObject("UIEffectMigrationTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            UIEffect effect = target.AddComponent<UIEffect>();
            effect.transitionTexture = texture;
            effect.transitionTextureScale = new Vector2(2, 3);
            effect.transitionTextureOffset = new Vector2(0.2f, 0.3f);
            effect.transitionTextureSpeed = new Vector2(0.4f, 0.5f);
            effect.transitionRotation = 45;
            effect.transitionKeepAspectRatio = false;
            effect.transitionRate = 0.25f;
            effect.transitionReverse = true;
            effect.transitionWidth = 0.6f;
            effect.transitionRange = new MinMax01(0.1f, 0.8f);
            effect.transitionPatternReverse = true;
            effect.transitionAutoPlaySpeed = 1.5f;
            effect.transitionColorFilter = ColorFilter.Replace;
            effect.transitionColor = Color.magenta;
            effect.transitionColorGlow = true;
            effect.patternArea = PatternArea.Edge;
            ResetMigrationState(effect);

            effect.OnAfterDeserialize();

            PatternLayer layer = effect.GetPatternLayer(0);
            Assert.That(layer.m_Enabled, Is.True);
            Assert.That(layer.m_Texture, Is.SameAs(texture));
            Assert.That(layer.m_Opacity, Is.EqualTo(1));
            Assert.That(layer.m_TextureScale, Is.EqualTo(new Vector2(2, 3)));
            Assert.That(layer.m_TextureOffset, Is.EqualTo(new Vector2(0.2f, 0.3f)));
            Assert.That(layer.m_TextureSpeed, Is.EqualTo(new Vector2(0.4f, 0.5f)));
            Assert.That(layer.m_Rotation, Is.EqualTo(45));
            Assert.That(layer.m_KeepAspectRatio, Is.False);
            Assert.That(layer.m_Rate, Is.EqualTo(0.25f));
            Assert.That(layer.m_TextureReverse, Is.True);
            Assert.That(layer.m_Width, Is.EqualTo(0.6f));
            Assert.That(layer.m_Range.min, Is.EqualTo(0.1f));
            Assert.That(layer.m_Range.max, Is.EqualTo(0.8f));
            Assert.That(layer.m_PatternReverse, Is.True);
            Assert.That(layer.m_AutoPlaySpeed, Is.EqualTo(1.5f));
            Assert.That(layer.m_ColorFilter, Is.EqualTo(ColorFilter.Replace));
            Assert.That(layer.m_Color, Is.EqualTo(Color.magenta));
            Assert.That(layer.m_ColorGlow, Is.True);
            Assert.That(layer.m_Area, Is.EqualTo(PatternArea.Edge));
        }

        [Test]
        public void PresetMigration_RepeatedDeserializePreservesPopulatedLayers()
        {
            preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            PatternLayer expected = new PatternLayer
            {
                m_Enabled = true,
                m_Opacity = 0.35f,
                m_Color = Color.cyan,
            };
            preset.SetPatternLayer(2, expected);
            SetMigrationVersion(preset, 0);

            preset.OnAfterDeserialize();
            preset.OnAfterDeserialize();

            PatternLayer actual = preset.GetPatternLayer(2);
            Assert.That(actual.m_Enabled, Is.True);
            Assert.That(actual.m_Opacity, Is.EqualTo(0.35f));
            Assert.That(actual.m_Color, Is.EqualTo(Color.cyan));
        }

        [Test]
        public void PresetPaths_FourLayersSurviveLoadSaveAndReplica()
        {
            target = new GameObject("UIEffectTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            UIEffect effect = target.AddComponent<UIEffect>();
            preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            preset.m_TransitionFilter = TransitionFilter.Pattern;
            PatternLayer sourceLayer = new PatternLayer
            {
                m_Enabled = true,
                m_Opacity = 0.45f,
                m_TextureScale = new Vector2(4, 5),
                m_Color = Color.yellow,
            };
            preset.SetPatternLayer(3, sourceLayer);

            effect.LoadPreset(preset, false);
            PatternLayer loaded = effect.GetPatternLayer(3);
            Assert.That(loaded.m_Opacity, Is.EqualTo(0.45f));
            Assert.That(loaded.m_TextureScale, Is.EqualTo(new Vector2(4, 5)));

            preset.SetPatternLayer(2, new PatternLayer
            {
                m_Enabled = true,
                m_Opacity = 0.7f,
            });
            effect.LoadPreset(preset, true);
            Assert.That(effect.GetPatternLayer(2).m_Opacity, Is.EqualTo(0.7f));
            Assert.That(effect.GetPatternLayer(3).m_Opacity, Is.EqualTo(0.45f));

            UIEffectPreset saved = ScriptableObject.CreateInstance<UIEffectPreset>();
            try
            {
                effect.SavePreset(saved, false);
                Assert.That(saved.GetPatternLayer(3).m_Color, Is.EqualTo(Color.yellow));

                UIEffectReplica replica = target.AddComponent<UIEffectReplica>();
                replica.preset = saved;
                Assert.That(replica.context.m_PatternLayers[3].m_Opacity, Is.EqualTo(0.45f));
            }
            finally
            {
                Object.DestroyImmediate(saved);
            }
        }

        [Test]
        public void MaterialBinding_AllFourLayerSlotsReceiveIndependentValues()
        {
            Shader shader = Shader.Find("Hidden/UI/Default (UIEffect)");
            Assert.That(shader, Is.Not.Null);
            Material material = new Material(shader);
            UIEffectContext context = new UIEffectContext
            {
                m_TransitionFilter = TransitionFilter.Pattern,
                m_PatternLayers = new PatternLayer[PatternLayer.MaxCount],
            };

            try
            {
                for (int i = 0; i < PatternLayer.MaxCount; i++)
                {
                    context.m_PatternLayers[i] = new PatternLayer
                    {
                        m_Enabled = true,
                        m_Opacity = (i + 1) * 0.2f,
                        m_Color = new Color(i * 0.2f, 0, 0, 1),
                    };
                }

                context.ApplyToMaterial(material);

                for (int i = 0; i < PatternLayer.MaxCount; i++)
                {
                    Assert.That(material.GetInt($"_PatternEnabled{i}"), Is.EqualTo(1));
                    Assert.That(material.GetVector($"_PatternParams1_{i}").x,
                        Is.EqualTo((i + 1) * 0.2f).Within(0.001f));
                    Assert.That(material.GetColor($"_PatternColor{i}").r,
                        Is.EqualTo(i * 0.2f).Within(0.001f));
                }
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void PatternLayers_ExposeExactlyFourSafeSlots()
        {
            preset = ScriptableObject.CreateInstance<UIEffectPreset>();

            Assert.That(preset.patternLayerCount, Is.EqualTo(4));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => preset.GetPatternLayer(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => preset.GetPatternLayer(4));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => preset.SetPatternLayer(4, new PatternLayer()));
        }

        [Test]
        public void FreshPreset_ResetDisablesOverlayLayers()
        {
            target = new GameObject("UIEffectResetTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            UIEffect effect = target.AddComponent<UIEffect>();
            effect.SetPatternLayer(3, new PatternLayer { m_Enabled = true, m_Opacity = 0.5f });
            preset = ScriptableObject.CreateInstance<UIEffectPreset>();

            effect.LoadPreset(preset, false);

            Assert.That(effect.GetPatternLayer(0).m_Enabled, Is.True);
            for (int i = 1; i < PatternLayer.MaxCount; i++)
            {
                Assert.That(effect.GetPatternLayer(i).m_Enabled, Is.False);
            }
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void ResetMigrationState(UIEffectPreset preset)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            SetMigrationVersion(preset, 0);
            typeof(UIEffectPreset).GetField("m_PatternLayers", flags)?.SetValue(preset, null);
        }

        private static void SetMigrationVersion(UIEffectPreset preset, int version)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(UIEffectPreset).GetField("m_PatternLayerVersion", flags)?.SetValue(preset, version);
        }

        private static void ResetMigrationState(UIEffect effect)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(UIEffect).GetField("m_PatternLayerVersion", flags)?.SetValue(effect, 0);
            typeof(UIEffect).GetField("m_PatternLayers", flags)?.SetValue(effect, null);
        }
    }
}
