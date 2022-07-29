using Gameplay.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Conrollers
{
    /// <summary>
    /// Main game controller
    /// </summary>
    [ExecuteInEditMode]
    public class GameController : MonoBehaviour
    {
        public static GameController instance;

        public GameModel model;

        private void Awake()
        {
            instance = this;
            Simulation.SetModel(model);
        }


        private static void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            Application.Quit();
#endif
        }
    }
}