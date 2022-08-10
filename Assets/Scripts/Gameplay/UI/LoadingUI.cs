using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Gameplay.UI.Lobby
{
    public class LoadingUI : MonoBehaviour
    {
        public Camera tmp_camera;
        public AudioListener listener;
        public GameObject fadeUI;
        public GameObject elements;
        public Image background;

        public float fadeTime;
        private float fadeTimer;
        
        public void Show()
        {
            fadeUI.SetActive(true);
            elements.SetActive(true);
            background.color = Color.black;
        }

        public void Hide()
        {
            elements.SetActive(false);
            fadeTimer = fadeTime;
            if (listener != null)
                listener.enabled = false;
        }

        private void Update()
        {
            if (fadeTimer > 0)
            {
                fadeTimer -= Time.deltaTime;
                float t = fadeTimer / fadeTime;
                background.color = new Color(0, 0, 0, t);
                if (fadeTimer < 0)
                {
                    fadeUI.SetActive(false);
                    if (tmp_camera != null)
                        Destroy(tmp_camera.gameObject);
                }
            }
        }
    }
}