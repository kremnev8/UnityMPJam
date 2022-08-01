using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay.UI.Lobby
{
    public class LocalLobby : MonoBehaviour, ILobby
    {
        private GameNetworkManager networkManager;
        private GameModel model;

        public Button pastButton;
        public Button futureButton;
        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
        }
        
        public void SetPast()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(Timeline.PAST);
        }

        public void SetFuture()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(Timeline.FUTURE);
        }
        
        public void UpdateWaitUI()
        {
            if (NetworkClient.localPlayer != null)
            {
                PlayerController controller = NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>();
                pastButton.interactable = controller.role != Timeline.PAST;
                futureButton.interactable = controller.role != Timeline.FUTURE;
            }
        }
        
        public void StartGame()
        {
            if (networkManager != null)
            {
                networkManager.StartGame();
            }
        }

        public void Host()
        {
            networkManager.StartHost();
        }

        public void Connect()
        {
            networkManager.networkAddress = "localhost";
            networkManager.StartClient();
        }

        public string GetPlayerName()
        {
            return $"Player {Random.Range(0, 25565)}";
        }

        private void Update()
        {
            UpdateWaitUI();
        }
    }
}