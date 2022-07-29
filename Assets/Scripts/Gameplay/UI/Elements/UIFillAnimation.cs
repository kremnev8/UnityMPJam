using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// UI element for a circular fill bar.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIFillAnimation : MonoBehaviour
    {
        private Image fillImage;

        private float startFill;
        private float endFill;
        private float timeElapsed;
        
        public float fillTime;

        private void Awake()
        {
            fillImage = GetComponent<Image>();
        }

        public void Display(float current, float next)
        {
            startFill = current;
            endFill = next;
            timeElapsed = 0;
            fillImage.fillAmount = startFill;
        }

        private void Update()
        {
            timeElapsed += Time.unscaledDeltaTime;
            if (timeElapsed < fillTime)
            {
                float t = timeElapsed / fillTime;
                fillImage.fillAmount = Mathf.Lerp(startFill, endFill, t);
            }
            else
            {
                fillImage.fillAmount = endFill;
            }
            
            
        }
    }
}