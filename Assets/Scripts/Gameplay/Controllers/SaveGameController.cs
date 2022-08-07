using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Conrollers
{
    /// <summary>
    /// Settings data model
    /// </summary>
    [Serializable]
    public class GameSaveData : ISaveData
    {
        public int Version
        {
            get => version;
            set => version = value;
        }

        public int version;

        public bool fullscreen;
        
        public float volume;
        public int width;
        public int height;

        public int lastReachedLevel;
        public int currentLevel;
        
        public List<string> globalInventory = new List<string>();
        public List<string> icePlayerInventory = new List<string>();
        public List<string> firePlayerInventory = new List<string>();
    }
    
    /// <summary>
    /// Controller that handles saving and loading player settings
    /// </summary>
    public class SaveGameController : SaveDataBaseController<GameSaveData>
    {
        public override int Version => 1;
        public override string Filename => "SaveData";

        public override void OnVersionChanged(int oldVersion)
        {
            current.version = Version;
        }

        public override void InitializeSaveData(GameSaveData data)
        {
            data.version = Version;
        }

        public override void OnSaveDataLoaded()
        {
        }

        public void MarkDirty()
        {
            Save();
        }
    }
}