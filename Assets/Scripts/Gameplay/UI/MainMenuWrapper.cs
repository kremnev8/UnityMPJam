using System;
using Gameplay.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class MainMenuWrapper : MonoBehaviour
    {
        private PlayerInput input;
        private InputAction action;

        private bool state = false;
        
        public UnityEvent<bool> onEscape;

        public void Start()
        {
            input=  Simulation.GetModel<GameModel>().input;
            action = input.actions["ecsape"];

            action.performed += OnEscape;
        }

        private void OnEscape(InputAction.CallbackContext obj)
        {
            state = !state;
            onEscape?.Invoke(state);
        }

        public void StartGame()
        {
            SceneTransitionManager.instance.StartGame((int)DateTime.Now.Ticks);
        }

        public void StartSameSeed()
        {
            SceneTransitionManager.instance.StartGame(SceneTransitionManager.gameMode);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            
        }
    }
}