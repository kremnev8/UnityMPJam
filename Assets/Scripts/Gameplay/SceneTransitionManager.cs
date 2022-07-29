using System;
using System.Collections;
using Gameplay.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    /// <summary>
    /// Handles scene transition.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager instance;

        public static int gameMode;

        public static Action sceneUnloaded;
        

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
        
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartGame(int newGameMode)
        {
            gameMode = newGameMode;
            sceneUnloaded?.Invoke();
            StartCoroutine(LoadLevel("Game"));
        }

        public void LoadMenu()
        {
            Time.timeScale = 1f;
            sceneUnloaded?.Invoke();
            StartCoroutine(LoadLevel("MainMenu"));
        }

        IEnumerator LoadLevel(string levelName)
        {
            var async = SceneManager.LoadSceneAsync(levelName);
            async.allowSceneActivation = false;

            while (Math.Abs(async.progress - 0.9f) > 0.01f)
            {
                yield return null;
            }
            
            async.allowSceneActivation = true;

        }
    }
}