using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Presentation.UI.Effects
{
    /// <summary>
    /// Owns a self-animated material on a Graphic and restores the prior UI state when disabled.
    /// </summary>
    [AddComponentMenu("Gear/UI Effects/Loop Material Effect")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class UILoopMaterialEffect : MonoBehaviour
    {
        [SerializeField] private Material materialPreset;
        [SerializeField] private bool disableNativeUiEffect = true;

        private Graphic _graphic;
        private Material _originalMaterial;
        private UIEffect _nativeUiEffect;
        private bool _nativeUiEffectWasEnabled;
        private bool _isApplied;

        public bool DisableNativeUiEffect
        {
            get => disableNativeUiEffect;
            set => disableNativeUiEffect = value;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnDisable()
        {
            Restore();
        }

        public void SetMaterial(Material preset)
        {
            if (materialPreset == preset)
            {
                Apply();
                return;
            }

            Restore();
            materialPreset = preset;
            Apply();
        }

        public void Apply()
        {
            CacheReferences();
            if (_graphic == null || materialPreset == null)
            {
                return;
            }

            if (!_isApplied)
            {
                _originalMaterial = _graphic.material;
                _isApplied = true;
            }

            DisableNativeUiEffectIfNeeded();
            _graphic.material = materialPreset;
        }

        public void Restore()
        {
            if (!_isApplied)
            {
                return;
            }

            if (_graphic != null)
            {
                _graphic.material = _originalMaterial;
            }

            if (_nativeUiEffect != null && _nativeUiEffectWasEnabled)
            {
                _nativeUiEffect.enabled = true;
            }

            _nativeUiEffectWasEnabled = false;
            _isApplied = false;
        }

        public void Clear()
        {
            Restore();
            materialPreset = null;
        }

        private void CacheReferences()
        {
            if (_graphic == null)
            {
                _graphic = GetComponent<Graphic>();
            }

            if (_nativeUiEffect == null)
            {
                _nativeUiEffect = GetComponent<UIEffect>();
            }
        }

        private void DisableNativeUiEffectIfNeeded()
        {
            if (!disableNativeUiEffect || _nativeUiEffect == null || !_nativeUiEffect.enabled)
            {
                return;
            }

            _nativeUiEffectWasEnabled = true;
            _nativeUiEffect.enabled = false;
        }
    }
}
