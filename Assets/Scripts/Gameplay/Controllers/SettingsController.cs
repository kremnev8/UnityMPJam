using System;
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
    public class SettingsEntry : ISaveData
    {
        public int Version
        {
            get => version;
            set => version = value;
        }

        public int difficulty;
        public int theme;
        public int version;
    }
    
    /// <summary>
    /// Controller that handles saving and loading player settings
    /// </summary>
    public class SettingsController : SaveDataBaseController<SettingsEntry>
    {
        public override int Version => 1;
        public override string Filename => "settings";

        public override void OnVersionChanged(int oldVersion)
        {
        }

        public override void InitializeSaveData(SettingsEntry data)
        {
        }

        public override void OnSaveDataLoaded()
        {
        }

        private void Start()
        {
        }

        public void MarkDirty()
        {
            Save();
        }
    }
}