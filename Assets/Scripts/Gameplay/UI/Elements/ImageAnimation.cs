using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using Util;

namespace Gameplay.UI
{
    [ExecuteInEditMode]
    public class ImageAnimation : MonoBehaviour
    {
        public Sprite[] sprites;
        public int framesPerSprite = 6;

        public bool playOnAwake;
        public bool loop = true;
        public bool destroyOnEnd = false;

        private bool isPlaying;
        private int index = 0;
        private Image image;
        private int frame = 0;

        public UnityEvent onFinished;

        void Awake()
        {
            image = GetComponent<Image>();
            if (Application.isPlaying)
            {
                if (playOnAwake)
                {
                    isPlaying = true;
                }
            }
        }

        [InspectorButton]
        public void Play()
        {
            if (!Application.isPlaying && image == null)
            {
                image = GetComponent<Image>();
            }
            index = 0;
            frame = 0;
            isPlaying = true;
            image.sprite = sprites[0];
        }

        void Update()
        {
            if (!isPlaying) return;
            if (!loop && index == sprites.Length) return;
            
            frame++;
            if (frame < framesPerSprite) return;
            image.sprite = sprites[index];
            frame = 0;
            index++;
            if (index >= sprites.Length)
            {
                isPlaying = false;
                if (loop)
                {
                    Play();
                }
                if (destroyOnEnd) Destroy(gameObject);
                onFinished?.Invoke();
            }
        }
    }
}