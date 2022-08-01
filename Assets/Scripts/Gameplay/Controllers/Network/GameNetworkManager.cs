using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Controllers.Network;
using Gameplay.Core;
using Gameplay.World;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Controllers
{
    public class GameNetworkManager : NetworkManager
    {
        public GameObject controllerPrefab;
        private GameModel model;
        
        public List<PlayerController> players = new List<PlayerController>();
        private Dictionary<int, string> tmpUsernames = new Dictionary<int, string>();
        
        public bool canStartGame = false;
        private int playersLoaded;
        
        [Scene]
        public string lobbyScene;
        [Scene]
        public string gameScene;

        public event Action ServerReady;

        public static event Action MapStarted;
        
        public override void Start()
        {
            base.Start();
            model = Simulation.GetModel<GameModel>();
            ((PlayerAuthenticator) authenticator).OnAuthenticationResult += OnServerAddPlayer;

            if (Application.isEditor)
            {
                canStartGame = true;
            }
        }
        
        public void StartGame()
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                NetworkClient.Send(new ClientPressedPlay());
                return;
            }
            
            if (IsInLobby())
            {
                if (!Application.isEditor && !canStartGame) return;

                ServerChangeScene(gameScene);
            }
        }

        private bool IsInLobby()
        {
            return SceneManager.GetActiveScene().path.ToLower().Contains("lobby");
        }

        private void OnServerAddPlayer(NetworkConnectionToClient conn, string username)
        {
            if (tmpUsernames.Keys.Contains(conn.connectionId))
            {
                tmpUsernames[conn.connectionId] = username;
            }
            else
            {
                tmpUsernames.Add(conn.connectionId, username);
            }
        }

        private void OnClientSceneChanged(NetworkConnectionToClient conn, SceneChangeFinished msg)
        {
            playersLoaded++;
            if (playersLoaded >= players.Count)
            {
                OnAllSceneChanged();
            }
        }
        
        public void OnAllSceneChanged()
        {
            foreach (PlayerController player in players)
            {
                player.StartMap();
            }
        
            MapStarted?.Invoke();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<SceneChangeFinished>(OnClientSceneChanged, false);
            NetworkServer.RegisterHandler<ClientPressedPlay>(OnClientPressedPlay, false);

            GameObject contr = Instantiate(controllerPrefab, transform);
            NetworkServer.Spawn(contr);
        }

        private void OnClientPressedPlay(NetworkConnectionToClient arg1, ClientPressedPlay arg2)
        {
            StartGame();
        }


        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            
            PlayerController controller = conn.identity.gameObject.GetComponent<PlayerController>();
            model.roleController.SetPlayerRole(controller, model.roleController.GetNextRole());

            controller.PlayerName = tmpUsernames[conn.connectionId];
            players.Add(controller);
        
            if (numPlayers >= 2)
            {
                canStartGame = true;
            }
        }

        private void OnServerInitialized()
        {
            ServerReady?.Invoke();
        }
        
        
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            NetworkClient.Send(new SceneChangeFinished());
        }
    }
}