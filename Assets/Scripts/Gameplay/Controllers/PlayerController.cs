using System;
using Gameplay;
using Gameplay.Core;
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
    
    public class PlayerController : NetworkBehaviour
    {
        private InputAction action;
        
        [SyncVar]
        public string playerName;
        
        [SyncVar]
        public PlayerRole role; 
        
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

    }
}