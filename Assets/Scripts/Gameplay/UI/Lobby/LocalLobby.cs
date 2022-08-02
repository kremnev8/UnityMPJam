using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay.UI.Lobby
{
    public class LocalLobby : MonoBehaviour, ILobby
    {
        private GameNetworkManager networkManager;
        private GameModel model;

        [FormerlySerializedAs("pastButton")] public Button iceButton;
        [FormerlySerializedAs("futureButton")] public Button fireButton;
        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
        }
        
        public void SetIce()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(PlayerRole.ICE_MAGE);
        }

        public void SetFire()
        {
            if ( model.roleController != null)
                model.roleController.SetPlayerRole(PlayerRole.FIRE_MAGE);
        }
        
        public void UpdateWaitUI()
        {
            if (NetworkClient.localPlayer != null)
            {
                PlayerController controller = NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>();
                iceButton.interactable = controller.role != PlayerRole.ICE_MAGE;
                fireButton.interactable = controller.role != PlayerRole.FIRE_MAGE;
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