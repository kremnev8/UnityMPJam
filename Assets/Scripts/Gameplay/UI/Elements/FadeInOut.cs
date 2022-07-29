using UnityEngine;

namespace Gameplay.UI
{
    public enum FadeDirection
    {
        FADE_IN,
        FADE_OUT
    }
    
    /// <summary>
    /// UI helper that can fade a UI element in or out
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeInOut : MonoBehaviour
    {
        public float fadeTime;

        private float timeElapsed;
        private bool isFading;
        private FadeDirection fadeMode;
        
        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Start a fade in a certain direction
        /// </summary>
        public void Fade(FadeDirection direction)
        {
            if (isFading) return;

            fadeMode = direction;
            isFading = true;
            timeElapsed = 0;
            
            if (direction == FadeDirection.FADE_IN)
                gameObject.SetActive(true);
        }

        /// <summary>
        /// Start a fade in
        /// </summary>
        public void FadeIn()
        {
            if (isFading) return;

            fadeMode = FadeDirection.FADE_IN;
            isFading = true;
            timeElapsed = 0;
        }

        /// <summary>
        /// Start a fade out
        /// </summary>
        public void FadeOut()
        {
            if (isFading) return;
            
            fadeMode = FadeDirection.FADE_OUT;
            isFading = true;
            timeElapsed = 0;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (isFading)
            {
                timeElapsed += Time.deltaTime;
                if (timeElapsed < fadeTime)
                {
                    float alpha =  timeElapsed / fadeTime;
                    if (fadeMode == FadeDirection.FADE_OUT) alpha = 1 - alpha;

                    group.alpha = alpha;
                }
                else
                {
                    group.alpha = fadeMode == FadeDirection.FADE_OUT ? 0 : 1;
                    isFading = false;
                    gameObject.SetActive(fadeMode == FadeDirection.FADE_IN);
                }
            }
        }
    }
}