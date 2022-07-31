using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay.UI.Lobby
{
    public class LocalLobby : MonoBehaviour, ILobby
    {
        private GameNetworkManager networkManager;
        private GameModel model;

        public Button startGameButton;
        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
        }
        
        public void SetPast()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(PlayerRole.PAST);
        }

        public void SetFuture()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(PlayerRole.FUTURE);
        }
        
        public void StartGame()
        {
            if (networkManager != null)
            {
                networkManager.StartGame();
            }
        }
        
        public string GetPlayerName()
        {
            return $"Player {Random.Range(0, 25565)}";
        }
    }
}