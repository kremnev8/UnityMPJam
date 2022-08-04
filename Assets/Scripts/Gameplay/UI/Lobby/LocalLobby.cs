using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using Mirror;
using TMPro;
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
        public GameObject lobbyUI;
        
        public TMP_Text roleText;
        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
        }
        
        public void UpdateWaitUI()
        {
            if (NetworkClient.localPlayer != null)
            {
                PlayerController controller = NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>();
                roleText.text = controller.role == PlayerRole.ICE_MAGE ? "Ice" : "Fire";
            }
        }

        public void ToggleRole()
        {
            if (model.roleController != null)
            {
                PlayerController controller = NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>();
                model.roleController.SetPlayerRole(controller.role == PlayerRole.ICE_MAGE ? PlayerRole.FIRE_MAGE : PlayerRole.ICE_MAGE);
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

        public void HideLobby()
        {
            lobbyUI.SetActive(false);
        }

        private void Update()
        {
            UpdateWaitUI();
        }
    }
}