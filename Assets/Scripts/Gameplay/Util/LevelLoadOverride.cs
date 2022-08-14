using System;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class LevelLoadOverride : MonoBehaviour
    {
        public SaveGameController saveGame;

        private static bool needsUpdate;
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (needsUpdate)
            {
                LevelsDB items = Resources.Load<LevelsDB>("Settings/ScriptableObject/Levels DB");
                Scene scene = SceneManager.GetActiveScene();

                int index = Array.FindIndex(items.GetAll(), data => data.scene.Equals(scene.path));

                if (saveGame == null)
                {
                    saveGame = gameObject.AddComponent<SaveGameController>();
                    saveGame.Load();
                }

                saveGame.current.currentLevel = index;
                saveGame.Save();
                
                needsUpdate = false;
            }
        }

        private void OnValidate()
        {
            needsUpdate = true;
        }
    }
}