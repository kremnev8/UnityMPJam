using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Gameplay.Conrollers
{
    /// <summary>
    /// Interface for data model classes for <see cref="Gameplay.Conrollers.SaveDataBaseController"/>
    /// </summary>
    public interface ISaveData
    {
        [JsonIgnore]
        public int Version { get; set; }
    }

    /// <summary>
    /// Base class for controllers that need to save data in a json file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SaveDataBaseController<T> : MonoBehaviour
    where T : class, ISaveData, new()
    {
        public T current;

        private void Awake()
        {
            Load();
            SceneTransitionManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }
        
        private void OnDestroy()
        {
            SceneTransitionManager.sceneUnloaded -= OnSceneUnloaded;
        }
        
        private void OnSceneUnloaded()
        {
            Save();
        }

        public bool Load()
        {
            string dataPath = $"{Application.persistentDataPath}/{Filename}.json";
            string dirPath = Path.GetDirectoryName(dataPath);
            Directory.CreateDirectory(dirPath);
            if (File.Exists(dataPath))
            {
                try
                {
                    string json = File.ReadAllText(dataPath, Encoding.UTF8);
                    current = JsonConvert.DeserializeObject<T>(json);
                    if (current.Version < Version)
                    {
                        OnVersionChanged(current.Version);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to deserialize statistics file. Creating new one!");
                    Debug.Log(e);
                    current = new T();
                    InitializeSaveData(current);
                    return false;
                }
            }
            else
            {
                current = new T();
                InitializeSaveData(current);
                return false;
            }

            current.Version = Version;
            return true;
        }

        public void Save()
        {
            if (current != null)
            {
                Debug.Log("Saving!!");
                string dataPath = $"{Application.persistentDataPath}/{Filename}.json";
                string json = JsonConvert.SerializeObject(current, Formatting.Indented);
                File.WriteAllText(dataPath, json, Encoding.UTF8);
            }
        }

        public abstract int Version { get; }
        public abstract string Filename { get; }
        public abstract void OnVersionChanged(int oldVersion);
        public abstract void InitializeSaveData(T data);
        public abstract void OnSaveDataLoaded();
    }
}