using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Controllers.Network;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
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

        private bool canStartGame = false;
        private int playersLoaded;

        private int playersInExit;
        public int currentLevel;
        public bool CanChangeLevel => playersInExit >= players.Count;

        private static bool returningToMenu;
        public static bool fromCheckpoint = true;

        [Scene] public string menu;

        public event Action ServerReady;

        public static event Action MapStarted;

        public override void Start()
        {
            base.Start();
            model = Simulation.GetModel<GameModel>();
            ((PlayerAuthenticator)authenticator).OnAuthenticationResult += OnServerAddPlayer;

            if (Application.isEditor)
            {
                canStartGame = true;
            }

            SetToCheckpoint(true);
        }

        public void SelectNextLevel()
        {
            currentLevel++;
            int lastLevel = model.saveGame.current.lastReachedLevel;
            if (Application.isEditor)
            {
                lastLevel = model.levels.Count();
            }

            if (currentLevel > lastLevel)
            {
                currentLevel = lastLevel;
            }

            fromCheckpoint = false;
        }

        public void SelectPrevLevel()
        {
            currentLevel--;
            if (currentLevel < 0)
            {
                currentLevel = 0;
            }

            fromCheckpoint = false;
        }

        public void SetToCheckpoint(bool value)
        {
            if (value)
                currentLevel = model.saveGame.current.currentLevel;
            fromCheckpoint = value;
        }

        public bool HasPartner()
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                return NetworkClient.isConnected;
            }

            if (mode == NetworkManagerMode.Host)
            {
                return numPlayers == 2;
            }

            return false;
        }

        public string GetPartnerName()
        {
            if (mode == NetworkManagerMode.ClientOnly && NetworkClient.isConnected)
            {
                PlayerController[] players1 = FindObjectsOfType<PlayerController>();
                PlayerController player = players1.First(controller => !controller.isLocalPlayer);
                if (player != null)
                {
                    return player.PlayerName;
                }
            }

            if (mode == NetworkManagerMode.Host)
            {
                return players[1].PlayerName;
            }

            return "Error!";
        }

        public bool CanStartGame()
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                return NetworkClient.isConnected;
            }

            if (mode == NetworkManagerMode.Host)
            {
                return canStartGame;
            }

            return false;
        }


        public void StartGame()
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                NetworkClient.Send(new ClientChangeSceneRequest(SceneChangeType.START_GAME));
                return;
            }

            if (IsInLobby())
            {
                if (!Application.isEditor && !canStartGame) return;

                LevelData scene = model.levels.GetLevel(currentLevel);
                model.saveGame.current.currentLevel = currentLevel;

                if (!fromCheckpoint)
                {
                    model.saveGame.current.icePlayerInventory = new List<string>();
                    model.saveGame.current.firePlayerInventory = new List<string>();
                    model.saveGame.current.globalInventory = scene.defaultItems.ToList();
                }

                model.saveGame.Save();
                playersInExit = 0;
                ServerChangeScene(scene.scene);
            }
        }

        [Server]
        public void IncrementPlayersInExit()
        {
            playersInExit++;
        }

        [Server]
        public void DecrementPlayersInExit()
        {
            playersInExit--;
        }

        public void NextLevel()
        {
            NextLevel(false);
        }

        public void NextLevel(bool ignoreCondition)
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                NetworkClient.Send(new ClientChangeSceneRequest(SceneChangeType.NEXT_LEVEL));
                return;
            }


            if (!IsInLobby() && (playersInExit >= players.Count || ignoreCondition))
            {
                try
                {
                    playersLoaded = 0;
                    PrefabPoolController.ReturnAll();

                    LevelData scene = model.levels.GetLevel(currentLevel + 1);
                    currentLevel++;
                    model.saveGame.current.currentLevel = currentLevel;
                    int max = Mathf.Max(currentLevel, model.saveGame.current.lastReachedLevel);
                    model.saveGame.current.lastReachedLevel = max;
                    model.saveGame.Save();

                    playersInExit = 0;
                    ServerChangeScene(scene.scene);
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogWarning("No more levels!");
                }
            }
        }

        public void RestartLevel()
        {
            if (mode == NetworkManagerMode.ClientOnly)
            {
                NetworkClient.Send(new ClientChangeSceneRequest(SceneChangeType.RESTART_LEVEL));
                return;
            }

            if (!IsInLobby())
            {
                try
                {
                    PrefabPoolController.ReturnAll();
                    playersLoaded = 0;
                    LevelData scene = model.levels.GetLevel(currentLevel);
                    playersInExit = 0;
                    ServerChangeScene(scene.scene);
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogWarning($"Failed to get level {currentLevel}!");
                }
            }
        }

        public void ReturnToMenu()
        {
            returningToMenu = true;
            if (mode == NetworkManagerMode.Host)
                StopHost();
            if (mode == NetworkManagerMode.ClientOnly)
                StopClient();

            DontDestroy.DestroyAll();
            SceneManager.LoadScene(menu);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            if (!returningToMenu)
                ReturnToMenu();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            if (!returningToMenu)
                ReturnToMenu();
        }


        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            model.loadingUI.Show();
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
            NetworkServer.SendToAll(new HideFade());
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<SceneChangeFinished>(OnClientSceneChanged, false);
            NetworkServer.RegisterHandler<ClientChangeSceneRequest>(OnClientPressedPlay, false);

            GameObject contr = Instantiate(controllerPrefab, transform);
            NetworkServer.Spawn(contr);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.RegisterHandler<HideFade>(OnHideFadePacket, false);
        }

        private void OnHideFadePacket(HideFade obj)
        {
            model.loadingUI.Hide();
        }

        private void OnClientPressedPlay(NetworkConnectionToClient conn, ClientChangeSceneRequest packet)
        {
            switch (packet.type)
            {
                case SceneChangeType.START_GAME:
                    StartGame();
                    break;
                case SceneChangeType.RESTART_LEVEL:
                    RestartLevel();
                    break;
                case SceneChangeType.NEXT_LEVEL:
                    NextLevel(false);
                    break;
            }
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
            model.lobbyUI.HideLobby();
        }
    }
}