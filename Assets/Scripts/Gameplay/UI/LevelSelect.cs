using System;
using Gameplay.Controllers;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Mirror;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class LevelSelect : MonoBehaviour
    {
        public GameObject mainBox;
        public GameObject levelSelectBox;
        public ArrowScroll scroll;

        private GameModel model;
        private GameNetworkManager networkManager;
        private LevelsDB levels;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
            levels = model.levels;
        }

        public void StartFromCheckpoint(bool value)
        {
            if (value)
            {
                networkManager.SetToCheckpoint();
            }
            levelSelectBox.SetActive(!value);
            UpdateLevelName();
        }

        public void Left()
        {
            networkManager.SelectPrevLevel();
            UpdateLevelName();
        }

        private void UpdateLevelName()
        {
            try
            {
                LevelData level = levels.GetLevel(networkManager.currentLevel);
                scroll.SetText(level.levelName);
            }
            catch (InvalidOperationException) { }
        }

        public void Right()
        {
            networkManager.SelectNextLevel();
            UpdateLevelName();
        }

        private void Update()
        {
            if (networkManager != null)
            {
                mainBox.SetActive(networkManager.mode == NetworkManagerMode.Host);
            }
        }
    }
}