using System;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class FeedbackUI : MonoBehaviour
    {
        public TMP_Text feedbackText;

        public float feedbackHideTime;
        public AnimationCurve feedbackAlphaCurve;
        private float timeElapsed;

        private void Start()
        {
            timeElapsed = feedbackHideTime;
        }

        public void SetFeedback(string feedback)
        {
            feedbackText.text = feedback;
            feedbackText.color = Color.white;
            timeElapsed = 0;
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed < feedbackHideTime)
            {
                float t = timeElapsed / feedbackHideTime;
                t = feedbackAlphaCurve.Evaluate(t);
                feedbackText.color = new Color(1, 1, 1, t);
            }
            else
            {
                feedbackText.color = Color.clear;
            }
        }
    }
}