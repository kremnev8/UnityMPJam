using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.ScriptableObjects;
using Gameplay.UI;
using Gameplay.UI.Lobby;
using Mirror;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay
{
    /// <summary>
    /// Game model contains all important game classes and stores them for easy access from anywhere
    /// </summary>
    [System.Serializable]
    public class GameModel
    {
        public PlayerInput input;
        public Camera mainCamera;
        public GameNetworkManager networkManager;
        public RoleController roleController;

        public LobbyUI lobbyUI;
        
        public HoverUI hoverUI;
        
        public SettingsController settings;

        public LayerSettings layers;
        public StringsDB strings;
    }
}