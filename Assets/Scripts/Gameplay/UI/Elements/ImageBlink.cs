using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class ImageBlink : MonoBehaviour
    {
        public Image image;

        public Color normalColor;
        public Color brightColor;

        public float blinkTime;

        private float timeElapsed;
        private bool blinkState;
        
        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > blinkTime)
            {
                blinkState = !blinkState;
                timeElapsed = 0;

                image.color = blinkState ? brightColor : normalColor;
            }
        }
    }
}