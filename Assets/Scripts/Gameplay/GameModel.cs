using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Controllers.Player;
using Gameplay.ScriptableObjects;
using Gameplay.UI;
using Gameplay.UI.Lobby;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


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
        [FormerlySerializedAs("spacetime")] public LevelElementController levelElement;

        public Inventory globalInventory;
        public InventoryUI globalInventoryUI;
        public InventoryUI playerInventoryUI;
        
        public LobbyUI eosLobby;
        public LocalLobby localLobby;
        public LoadingUI loadingUI;
        
        public ILobby lobbyUI
        {
            get
            {
                if (eosLobby != null)
                {
                    return eosLobby;
                }

                return localLobby;
            }
        }

        public HoverUI hoverUI;
        
        public SettingsController settings;

        public LayerSettings layers;
        public StringsDB strings;
        public AbilityDB abilities;
        public ProjectileDB projectiles;
        public LevelsDB levels;
        public ItemDB items;
    }
}