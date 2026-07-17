using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold.UI
{
    public class ScaffoldProgressBar : MonoBehaviour
    {
        [Header("UI References")]
        public Image foregroundBar;
        public Image delayedBar;
        
        [Header("Settings")]
        public float fillSpeed = 5f;
        public float delayBeforeDrain = 1f;
        public float drainSpeed = 2f;
        
        private float _targetFillAmount = 1f;
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
            if (foregroundBar == null) return;
            
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
