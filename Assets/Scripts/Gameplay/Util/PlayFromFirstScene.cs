#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.EditorPrefs;
using Object = UnityEngine.Object;

namespace Util
{
    /// <summary>
    /// Utility to always load the game from main menu
    /// </summary>
    public static class PlayFromTheFirstScene
    {
        private const string startScene = "1";
        private const string playFromFirstMenuStr = "Edit/Always Start From Scene " + startScene + " &p";
 
        static bool playFromFirstScene
        {
            get => HasKey(playFromFirstMenuStr) && GetBool(playFromFirstMenuStr);
            set => SetBool(playFromFirstMenuStr, value);
        }
 
        [MenuItem(playFromFirstMenuStr, false, 150)]
        static void PlayFromFirstSceneCheckMenu() 
        {
            playFromFirstScene = !playFromFirstScene;
            Menu.SetChecked(playFromFirstMenuStr, playFromFirstScene);
 
            ShowNotifyOrLog(playFromFirstScene ? "Play from scene " + startScene : "Play from current scene");
        }
 
        // The menu won't be gray out, we use this validate method for update check state
        [MenuItem(playFromFirstMenuStr, true)]
        static bool PlayFromFirstSceneCheckMenuValidate()
        {
            Menu.SetChecked(playFromFirstMenuStr, playFromFirstScene);
            return true;
        }
 
        // This method is called before any Awake. It's the perfect callback for this feature
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 
        static void LoadFirstSceneAtGameBegins()
        {
            if(!playFromFirstScene)
                return;
 
            int index = int.Parse(startScene);
            
            if(index < EditorBuildSettings.scenes.Length)
            {
                Debug.LogWarning("The scene build list is empty. Can't play from first scene.");
                return;
            }
 
            foreach(GameObject go in Object.FindObjectsOfType<GameObject>())
                go.SetActive(false);
         
            SceneManager.LoadScene(index);
        }
 
        static void ShowNotifyOrLog(string msg)
        {
            if(Resources.FindObjectsOfTypeAll<SceneView>().Length > 0)
                EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent(msg));
            else
                Debug.Log(msg); // When there's no scene view opened, we just print a log
        }
    }
}
#endif