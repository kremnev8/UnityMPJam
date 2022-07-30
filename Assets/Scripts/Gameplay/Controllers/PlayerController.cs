using System;
using Gameplay;
using Gameplay.Core;
using Gameplay.World;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.Conrollers
{

    public enum PlayerRole : byte
    {
        PAST,
        FUTURE
    }
    
    public class PlayerController : NetworkBehaviour, IPlayerData
    {
        private InputAction action;
        
        [SyncVar]
        private string playerName;
        [SyncVar]
        private PlayerRole m_role;
        
        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        public PlayerRole role
        {
            get => m_role;
            set => m_role = value;
        }

        public void Start()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            action = model.input.actions["move"];

        }

        void Update()
        {
            if (!isLocalPlayer) return;

            Vector2 dir = action.ReadValue<Vector2>();

            transform.position += dir.ToVector3() * Time.deltaTime * 5;
        }

        
        public void StartMap()
        {
            
        }
    }
}