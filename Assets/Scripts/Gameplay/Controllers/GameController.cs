using Gameplay.Controllers.Player.Ability;
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
            if (Application.isPlaying && instance != null)
            {
                Destroy(gameObject);
            }
            
            instance = this;
            Simulation.SetModel(model);
            
            model.abilities = Resources.LoadAll<BaseAbility>("Settings/ScriptableObject/Ablities/");
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