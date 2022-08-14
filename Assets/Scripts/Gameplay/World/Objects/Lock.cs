﻿using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    [SelectionBase]
    public class Lock : Interactible
    {
        public string itemId;
        public Animator animator;
        public SpriteRenderer topRenderer;
        public SpriteRenderer bottomRenderer;

        public RandomAudioSource sound;
        
        private static readonly int open = Animator.StringToHash("Open");
        private GameModel model;

        public float activateDelay;
        
        protected override void Start()
        {
            base.Start();
            model = Simulation.GetModel<GameModel>();
        }

        public override void Activate(PlayerController player)
        {
            if (isClient)
            {
                CmdActivate(player);
            }
            else
            {
                ServerActivate(player);
            }
            
        }

        [Command(requiresAuthority = false)]
        private void CmdActivate(PlayerController player)
        {
            ServerActivate(player);
        }

        [Server]
        private void ServerActivate(PlayerController player)
        {
            if (state) return;

            if (player != null && model.globalInventory != null)
            {
                if (model.globalInventory.TryConsume(itemId))
                {
                    RpcUseKey();
                    Invoke(nameof(ActualActivate), activateDelay);
                }
            }
        }

        [Server]
        private void ActualActivate()
        {
            SetState(true);
            RpcSetState(true);
        }

        [ClientRpc]
        public void RpcUseKey()
        {
            animator.SetTrigger(open);
            sound.Play();
        }
    }
}