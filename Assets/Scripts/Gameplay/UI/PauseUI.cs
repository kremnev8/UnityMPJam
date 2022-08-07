using System;
using Gameplay.Controllers;
using Gameplay.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class PauseUI : MonoBehaviour
    {
        public GameObject pauseUIObject;

        private GameNetworkManager networkManager;
        private InputAction pause;

        private bool pauseOpen;
        
        private void Start()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
            pause = model.input.actions["escape"];
            pauseUIObject.SetActive(false);

            pause.performed += OnPause;
        }

        private void OnDestroy()
        {
            pause.performed -= OnPause;
        }

        private void OnPause(InputAction.CallbackContext obj)
        {
            SetPauseState(!pauseOpen);
        }

        private void SetPauseState(bool value)
        {
            pauseOpen = value;
            pauseUIObject.SetActive(value);
        }

        public void OnResume()
        {
            SetPauseState(false);
        }

        public void OnRestart()
        {
            SetPauseState(false);
            networkManager.RestartLevel();
        }

        public void OnExitToMenu()
        {
            SetPauseState(false);
            networkManager.ReturnToMenu();
        }

        public void OnExit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}