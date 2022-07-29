using System;
using Gameplay;
using Gameplay.Core;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Util;

namespace NobleConnect.Examples.Mirror
{
    // A super simple example player. Use arrow keys to move
    public class MirrorExamplePlayer : NetworkBehaviour
    {
        private InputAction action;
        
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