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
        public PlayableDirector director;
        public GameObject fadeUI;
        public GameObject elements;
        public Image background;

        public float fadeTime;
        private float fadeTimer;
        
        public void Show()
        {
            Debug.Log("Play loading anim");
            fadeUI.SetActive(true);
            elements.SetActive(true);
            background.color = Color.black;
            director.time = 0;
            director.Play();
        }

        public void Hide()
        {
            Debug.Log("Stop loading anim");
            elements.SetActive(false);
            director.Stop();
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