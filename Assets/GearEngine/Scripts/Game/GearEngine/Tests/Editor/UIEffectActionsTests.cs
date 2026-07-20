using System;
using System.Collections.Generic;
using System.Reflection;
using Coffee.UIEffects;
using GearEngine.Presentation.UI.Effects;
using NUnit.Framework;
using Scaffold;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class UIEffectActionsTests
    {
        private const string UiEffectsFolder = "Assets/3rdParty/UIEffect/UIEffectPresets/UIEffects";

        private GameObject _target;
        private UIEffectPreset _preset;
        private MaterialUIEffectPreset _configuration;
        private Material _material;

        [TearDown]
        public void TearDown()
        {
            if (_preset != null)
            {
                UnityEngine.Object.DestroyImmediate(_preset);
            }

            if (_configuration != null)
            {
                UnityEngine.Object.DestroyImmediate(_configuration);
            }

            if (_target != null)
            {
                UnityEngine.Object.DestroyImmediate(_target);
            }

            if (_material != null)
            {
                UnityEngine.Object.DestroyImmediate(_material);
            }
        }

        [Test]
        public void ApplyPreset_AddsMissingEffectToDynamicTarget()
        {
            _target = CreateUiTarget();
            _preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            _preset.m_TransitionFilter = TransitionFilter.Fade;
            _preset.m_TransitionRate = 0.25f;

            ApplyUIEffectPreset action = new ApplyUIEffectPreset();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "preset", _preset);
            SetField(action, "addIfMissing", new BooleanData(true));

            action.OnEnter();

            UIEffect effect = _target.GetComponent<UIEffect>();
            Assert.That(effect, Is.Not.Null);
            Assert.That(effect.transitionFilter, Is.EqualTo(TransitionFilter.Fade));
            Assert.That(effect.transitionRate, Is.EqualTo(0.25f));
        }

        [TestCase(SetUIEffectIntensity.IntensityChannel.Tone)]
        [TestCase(SetUIEffectIntensity.IntensityChannel.Color)]
        [TestCase(SetUIEffectIntensity.IntensityChannel.Sampling)]
        [TestCase(SetUIEffectIntensity.IntensityChannel.Transition)]
        public void SetIntensity_UpdatesTheSelectedChannel(SetUIEffectIntensity.IntensityChannel channel)
        {
            _target = CreateUiTarget();
            UIEffect effect = _target.AddComponent<UIEffect>();
            SetUIEffectIntensity action = new SetUIEffectIntensity();
            SetField(action, "targetEffect", effect);
            SetField(action, "channel", channel);
            SetField(action, "intensity", new FloatData(0.4f));

            action.OnEnter();

            float actual = channel switch
            {
                SetUIEffectIntensity.IntensityChannel.Tone => effect.toneIntensity,
                SetUIEffectIntensity.IntensityChannel.Color => effect.colorIntensity,
                SetUIEffectIntensity.IntensityChannel.Sampling => effect.samplingIntensity,
                SetUIEffectIntensity.IntensityChannel.Transition => effect.transitionRate,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
            };
            Assert.That(actual, Is.EqualTo(0.4f));
        }

        [Test]
        public void SetEnabled_UsesTheDynamicTarget()
        {
            _target = CreateUiTarget();
            UIEffect effect = _target.AddComponent<UIEffect>();
            SetUIEffectEnabled action = new SetUIEffectEnabled();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "isEnabled", new BooleanData(false));

            action.OnEnter();

            Assert.That(effect.enabled, Is.False);
        }

        [Test]
        public void ClearEffect_ResetsTheEffectToTheDefaultPreset()
        {
            _target = CreateUiTarget();
            UIEffect effect = _target.AddComponent<UIEffect>();
            effect.transitionFilter = TransitionFilter.Fade;
            ClearUIEffect action = new ClearUIEffect();
            SetField(action, "targetEffect", effect);

            action.OnEnter();

            Assert.That(effect.transitionFilter, Is.EqualTo(TransitionFilter.None));
        }

        [Test]
        public void ControlTweener_SetsManualTimeOnDynamicTarget()
        {
            _target = CreateUiTarget();
            _target.AddComponent<UIEffect>();
            UIEffectTweener tweener = _target.AddComponent<UIEffectTweener>();
            tweener.duration = 2f;
            ControlUIEffectTweener action = new ControlUIEffectTweener();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "operation", ControlUIEffectTweener.TweenerOperation.SetTime);
            SetField(action, "timeSeconds", new FloatData(0.75f));

            action.OnEnter();

            Assert.That(tweener.time, Is.EqualTo(0.75f));
        }

        [Test]
        public void CyclePreset_ReplacesTheEffectAndUpdatesTheLabel()
        {
            _target = CreateUiTarget();
            GameObject labelObject = new GameObject("EffectLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(_target.transform, false);
            Text label = labelObject.GetComponent<Text>();
            _preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            _preset.m_ColorFilter = ColorFilter.Additive;

            CycleUIEffectPreset action = new CycleUIEffectPreset();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "targetLabel", label);
            SetField(action, "presets", new List<UIEffectPreset> { _preset });
            SetField(action, "descriptions", new List<string> { "Adds a color glow." });

            action.OnEnter();

            UIEffect effect = _target.GetComponent<UIEffect>();
            Assert.That(effect, Is.Not.Null);
            Assert.That(effect.colorFilter, Is.EqualTo(ColorFilter.Additive));
            Assert.That(label.text, Does.Contain("Adds a color glow."));
        }

        [Test]
        public void ApplyLoopMaterial_AssignsMaterialAndDisablesNativeEffect()
        {
            _target = CreateUiTarget();
            UIEffect nativeEffect = _target.AddComponent<UIEffect>();
            _material = new Material(Shader.Find("UI/Default"));
            ApplyUILoopMaterial action = new ApplyUILoopMaterial();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "materialPreset", _material);
            SetField(action, "disableNativeUiEffect", new BooleanData(true));

            action.OnEnter();

            Image graphic = _target.GetComponent<Image>();
            Assert.That(_target.GetComponent<UILoopMaterialEffect>(), Is.Not.Null);
            Assert.That(graphic.material, Is.SameAs(_material));
            Assert.That(nativeEffect.enabled, Is.False);
        }

        [Test]
        public void ApplyEffect_WithMaterialConfiguration_AssignsMaterialAndDisablesNativeEffect()
        {
            _target = CreateUiTarget();
            UIEffect nativeEffect = _target.AddComponent<UIEffect>();
            _material = new Material(Shader.Find("UI/Default"));
            _configuration = ScriptableObject.CreateInstance<MaterialUIEffectPreset>();
            SetField(_configuration, "materialPreset", _material);
            ApplyUIEffectPreset action = new ApplyUIEffectPreset();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "configuration", new ObjectData(_configuration));

            action.OnEnter();

            Image graphic = _target.GetComponent<Image>();
            Assert.That(_target.GetComponent<UILoopMaterialEffect>(), Is.Not.Null);
            Assert.That(graphic.material, Is.SameAs(_material));
            Assert.That(nativeEffect.enabled, Is.False);
        }

        [Test]
        public void ApplyEffect_WithBlackboardConfigurationVariable_AssignsMaterial()
        {
            _target = CreateUiTarget();
            _material = new Material(Shader.Find("UI/Default"));
            _configuration = ScriptableObject.CreateInstance<MaterialUIEffectPreset>();
            SetField(_configuration, "materialPreset", _material);
            ObjectVariable configurationVariable = _target.AddComponent<ObjectVariable>();
            configurationVariable.Value = _configuration;
            ApplyUIEffectPreset action = new ApplyUIEffectPreset();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(
                action,
                "configuration",
                new ObjectData
                {
                    objectRef = configurationVariable,
                    source = VariableDataSource.BlackboardVariable,
                });

            action.OnEnter();

            Assert.That(_target.GetComponent<Image>().material, Is.SameAs(_material));
        }

        [Test]
        public void ApplyEffect_WithNativeConfiguration_LoadsNativePreset()
        {
            _target = CreateUiTarget();
            _configuration = ScriptableObject.CreateInstance<MaterialUIEffectPreset>();
            _configuration.m_TransitionFilter = TransitionFilter.Fade;
            ApplyUIEffectPreset action = new ApplyUIEffectPreset();
            SetField(action, "targetGameObject", new GameObjectData(_target));
            SetField(action, "configuration", new ObjectData(_configuration));

            action.OnEnter();

            Assert.That(_target.GetComponent<UIEffect>().transitionFilter, Is.EqualTo(TransitionFilter.Fade));
        }

        [Test]
        public void ExecutePreset_WithNativePreset_RestoresTheNativeMaterialPath()
        {
            _target = CreateUiTarget();
            UIEffect nativeEffect = _target.AddComponent<UIEffect>();
            _material = new Material(Shader.Find("UI/Default"));
            _configuration = ScriptableObject.CreateInstance<MaterialUIEffectPreset>();
            SetField(_configuration, "materialPreset", _material);
            nativeEffect.ExecutePreset(_configuration);

            _preset = ScriptableObject.CreateInstance<UIEffectPreset>();
            _preset.m_TransitionFilter = TransitionFilter.Fade;
            nativeEffect.ExecutePreset(_preset);

            Assert.That(_target.GetComponent<Image>().material, Is.Null);
            Assert.That(nativeEffect.enabled, Is.True);
            Assert.That(nativeEffect.transitionFilter, Is.EqualTo(TransitionFilter.Fade));
        }

        [Test]
        public void AssetCatalog_AllPresetsResolveAndApply()
        {
            string[] configurationGuids = AssetDatabase.FindAssets("t:MaterialUIEffectPreset", new[] { UiEffectsFolder });
            Assert.That(configurationGuids, Has.Length.EqualTo(32));

            int materialConfigurationCount = 0;
            int nativeConfigurationCount = 0;
            bool[] supportedModes = new bool[19];

            foreach (string configurationGuid in configurationGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(configurationGuid);
                MaterialUIEffectPreset catalogConfiguration = AssetDatabase.LoadAssetAtPath<MaterialUIEffectPreset>(path);
                Assert.That(catalogConfiguration, Is.Not.Null, path);

                bool hasMaterial = catalogConfiguration.MaterialPreset != null;
                Assert.That(hasMaterial || catalogConfiguration.m_ToneFilter != ToneFilter.None ||
                    catalogConfiguration.m_ColorFilter != ColorFilter.None ||
                    catalogConfiguration.m_SamplingFilter != SamplingFilter.None ||
                    catalogConfiguration.m_TransitionFilter != TransitionFilter.None ||
                    catalogConfiguration.m_ShadowMode != ShadowMode.None ||
                    catalogConfiguration.m_EdgeMode != EdgeMode.None ||
                    catalogConfiguration.m_GradationMode != GradationMode.None ||
                    catalogConfiguration.m_DetailFilter != DetailFilter.None, Is.True, path);

                GameObject target = CreateUiTarget();
                try
                {
                    ApplyUIEffectPreset action = new ApplyUIEffectPreset();
                    SetField(action, "targetGameObject", new GameObjectData(target));
                    SetField(action, "configuration", new ObjectData(catalogConfiguration));
                    action.OnEnter();

                    if (hasMaterial)
                    {
                        materialConfigurationCount++;
                        Material material = catalogConfiguration.MaterialPreset;
                        Assert.That(material.shader.name, Is.EqualTo("Gear/UI/LoopEffects"), path);
                        int effectMode = Mathf.RoundToInt(material.GetFloat("_EffectMode"));
                        Assert.That(effectMode, Is.InRange(1, 18), path);
                        supportedModes[effectMode] = true;
                        Assert.That(target.GetComponent<Image>().material, Is.SameAs(material), path);
                    }
                    else
                    {
                        nativeConfigurationCount++;
                        Assert.That(target.GetComponent<UIEffect>(), Is.Not.Null, path);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(target);
                }
            }

            Assert.That(materialConfigurationCount, Is.EqualTo(21));
            Assert.That(nativeConfigurationCount, Is.EqualTo(11));
            for (int effectMode = 1; effectMode <= 18; effectMode++)
            {
                Assert.That(supportedModes[effectMode], Is.True, $"Missing material for effect mode {effectMode}.");
            }
        }

        [Test]
        public void ClearLoopMaterial_RestoresOriginalMaterialAndNativeEffect()
        {
            _target = CreateUiTarget();
            Image graphic = _target.GetComponent<Image>();
            Material originalMaterial = graphic.material;
            UIEffect nativeEffect = _target.AddComponent<UIEffect>();
            _material = new Material(Shader.Find("UI/Default"));
            ApplyUILoopMaterial applyAction = new ApplyUILoopMaterial();
            SetField(applyAction, "targetGameObject", new GameObjectData(_target));
            SetField(applyAction, "materialPreset", _material);
            applyAction.OnEnter();

            ClearUILoopMaterial clearAction = new ClearUILoopMaterial();
            SetField(clearAction, "targetGameObject", new GameObjectData(_target));
            clearAction.OnEnter();

            Assert.That(graphic.material, Is.SameAs(originalMaterial));
            Assert.That(nativeEffect.enabled, Is.True);
        }

        private GameObject CreateUiTarget()
        {
            GameObject target = new GameObject("UIEffectTarget", typeof(CanvasRenderer), typeof(Image));
            return target;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            Type type = instance.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }

                type = type.BaseType;
            }

            throw new MissingFieldException(instance.GetType().FullName, fieldName);
        }
    }
}
