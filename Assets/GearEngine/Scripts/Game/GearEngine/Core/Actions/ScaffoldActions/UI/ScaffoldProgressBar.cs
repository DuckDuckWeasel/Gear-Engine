using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold.UI
{
    public class ScaffoldProgressBar : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The Foreground bar")]
        public Image foregroundBar;
        [Tooltip("The Delayed bar")]
        public Image delayedBar;

        [Header("Settings")]
        [Tooltip("The Fill speed")]
        public float fillSpeed = 5f;
        [Tooltip("The Delay before drain")]
        public float delayBeforeDrain = 1f;
        [Tooltip("The Drain speed")]
        public float drainSpeed = 2f;

        [Tooltip("The _target fill amount")]
        private float _targetFillAmount = 1f;
        [Tooltip("The _current delay")]
        private float _currentDelay = 0f;

        public void UpdateBar(float currentValue, float minValue, float maxValue)
        {
            float fillAmount = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            _targetFillAmount = fillAmount;

            // If taking damage, reset the delay
            if (foregroundBar != null && fillAmount < foregroundBar.fillAmount)
            {
                _currentDelay = delayBeforeDrain;
            }
        }

        private void Update()
        {
            if (foregroundBar == null)
            {
                return;
            }

            // Fast fill for foreground
            foregroundBar.fillAmount = Mathf.Lerp(foregroundBar.fillAmount, _targetFillAmount, Time.deltaTime * fillSpeed);

            // Delayed fill for the background bar (damage effect)
            if (delayedBar != null)
            {
                if (_currentDelay > 0)
                {
                    _currentDelay -= Time.deltaTime;
                }
                else
                {
                    delayedBar.fillAmount = Mathf.Lerp(delayedBar.fillAmount, foregroundBar.fillAmount, Time.deltaTime * drainSpeed);
                }
            }
        }
    }
}
