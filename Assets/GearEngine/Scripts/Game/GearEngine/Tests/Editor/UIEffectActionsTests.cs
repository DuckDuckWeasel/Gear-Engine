using System;
using System.Collections.Generic;
using System.Reflection;
using Coffee.UIEffects;
using NUnit.Framework;
using Scaffold;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class UIEffectActionsTests
    {
        private GameObject _target;
        private UIEffectPreset _preset;

        [TearDown]
        public void TearDown()
        {
            if (_preset != null)
            {
                UnityEngine.Object.DestroyImmediate(_preset);
            }

            if (_target != null)
            {
                UnityEngine.Object.DestroyImmediate(_target);
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
