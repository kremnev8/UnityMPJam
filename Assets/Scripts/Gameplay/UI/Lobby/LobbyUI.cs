using System;
using System.Collections.Generic;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

namespace Gameplay.UI.Lobby
{
    public class LobbyUI : EOSLobby, ILobby
    {
        private List<LobbyDetails> foundLobbies = new List<LobbyDetails>();
        private List<Attribute> lobbyData = new List<Attribute>();

        private List<LobbyItem> lobbyItems = new List<LobbyItem>();
        public GameObject lobbyUI;
        
        private GameNetworkManager networkManager;
        private GameModel model;

        public GameObject buttons;
        public GameObject createLobbyUI;
        public GameObject joinLobbyUI;

        public RectTransform lobbyListTrans;
        public LobbyItem lobbyItemPrefab;
        public Button refreshButton;
        
        public TMP_InputField lobbyNameField;
        public Button createbutton;

        public GameObject waitUI;
        public TMP_Text waitText;

        public TMP_InputField userNameInputField;

        public Button startGameButton;

        public bool canCreateLobby = true;
        public bool needToRefresh = false;

        public float timeSinceFind = 0;

        public override void Start()
        {
            base.Start();

            model = Simulation.GetModel<GameModel>();
            networkManager = model.networkManager;
        }


        //register events
        private void OnEnable() {
            //subscribe to events
            CreateLobbySucceeded += OnCreateLobbySuccess;
            JoinLobbySucceeded += OnJoinLobbySuccess;
            FindLobbiesSucceeded += OnFindLobbiesSuccess;
            LeaveLobbySucceeded += OnLeaveLobbySuccess;
            
            CreateLobbyFailed += OnCreateLobbyFail;
        }

        //deregister events
        private void OnDisable() {
            //unsubscribe from events
            CreateLobbySucceeded -= OnCreateLobbySuccess;
            JoinLobbySucceeded -= OnJoinLobbySuccess;
            FindLobbiesSucceeded -= OnFindLobbiesSuccess;
            LeaveLobbySucceeded -= OnLeaveLobbySuccess;

            CreateLobbyFailed -= OnCreateLobbyFail;
        }
        
        //when the lobby is successfully created, start the host
        private void OnCreateLobbySuccess(List<Attribute> attributes) {
            lobbyData = attributes;

            networkManager.StartHost();
            OpenWaitUI();
            
        }

        //when the user joined the lobby successfully, set network address and connect
        private void OnJoinLobbySuccess(List<Attribute> attributes) {
            lobbyData = attributes;

            networkManager.networkAddress = attributes.Find((x) => x.Data.Key == hostAddressKey).Data.Value.AsUtf8;
            networkManager.StartClient();
            OpenWaitUI();
        }

        //callback for FindLobbiesSucceeded
        private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound) {
            foundLobbies = lobbiesFound;
            needToRefresh = true;
        }

        //when the lobby was left successfully, stop the host/client
        private void OnLeaveLobbySuccess() {
            networkManager.StopHost();
            networkManager.StopClient();
            OpenMainMenu();
            
        }
        
        public void HideLobby()
        {
            lobbyUI.SetActive(false);
        }
        
        private void OnCreateLobbyFail(string errormessage)
        {
            canCreateLobby = true;
        }

        public void OnOpenCreateLobby()
        {
            createLobbyUI.SetActive(true);
            buttons.SetActive(false);
        }

        public void OnCreateLobby()
        {
            CreateLobby(2, LobbyPermissionLevel.Publicadvertised, false, new[]
            {
                new AttributeData { Key = AttributeKeys[0], Value = lobbyNameField.text },
            });
            canCreateLobby = false;
        }

        public void OnOpenJoinLobby()
        {
            joinLobbyUI.SetActive(true);
            buttons.SetActive(false);
            
            if (timeSinceFind < 0.1f)
            {
                FindLobbies();
                timeSinceFind = 2;
            }
        }

        public void OpenMainMenu()
        {
            buttons.SetActive(true);
            createLobbyUI.SetActive(false);
            joinLobbyUI.SetActive(false);
            waitUI.SetActive(false);
        }

        public void OpenWaitUI()
        {
            buttons.SetActive(false);
            createLobbyUI.SetActive(false);
            joinLobbyUI.SetActive(false);
            waitUI.SetActive(true);
        }

        public void Exit()
        {
            buttons.SetActive(true);
            createLobbyUI.SetActive(false);
            joinLobbyUI.SetActive(false);
            
            LeaveLobby();
            canCreateLobby = true;
        }

        public void ToggleRole()
        {
            if (model.roleController != null)
            {
                PlayerController controller = NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>();
                model.roleController.SetPlayerRole(controller.role == PlayerRole.ICE_MAGE ? PlayerRole.FIRE_MAGE : PlayerRole.ICE_MAGE);
            }
        }


        public string GetPlayerName()
        {
            return userNameInputField.text;
        }
        
        public void RefreshData()
        {
            if (timeSinceFind < 0.1f)
            {
                FindLobbies();
                timeSinceFind = 1;
            }
        }

        public void StartGame()
        {
            if (networkManager != null)
            {
                networkManager.StartGame();
            }
        }
        
        private void Update()
        {
            if (timeSinceFind > 0)
            {
                timeSinceFind -= Time.deltaTime;
            }

            refreshButton.interactable = timeSinceFind < 0.1f;
            createbutton.interactable = lobbyNameField.text.Length > 0 && canCreateLobby;

            if (foundLobbies.Count > 0 && needToRefresh)
            {
                lobbyListTrans.gameObject.ClearChildren();
                lobbyItems.Clear();
                foreach (LobbyDetails lobby in foundLobbies)
                {
                    LobbyItem item = Instantiate(lobbyItemPrefab, lobbyListTrans);
                    item.SetLobby(this, lobby);
                    lobbyItems.Add(item);
                }

                needToRefresh = false;
            }

            if (networkManager != null)
            {
                if (!networkManager.HasPartner())
                {
                    waitText.text = "Waiting for partner";
                }
                else
                {
                    waitText.text = $"Your Partner: {networkManager.GetPartnerName()}";
                }
                
                startGameButton.interactable = networkManager.CanStartGame();
            }
        }
    }
}