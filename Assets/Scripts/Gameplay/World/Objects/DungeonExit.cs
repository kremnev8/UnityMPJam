using System;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(WorldElement))]
    public class DungeonExit : NetworkBehaviour, IInteractable, ILinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;

        private GameModel model;

        public WorldElement element { get; set; }

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
        }

        public void Activate(PlayerController player)
        {
            if (isClient)
            {
                CmdNextLevel(player);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdNextLevel(PlayerController player)
        {
            if (model.networkManager.CanChangeLevel)
            {
                model.networkManager.NextLevel();
            }
            else
            {
                player.RpcFeedback("Waiting for other player!");
            }
        }


        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isServer && col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    model.networkManager.IncrementPlayersInExit();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (isServer && col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    model.networkManager.DecrementPlayersInExit();
                }
            }
        }

        public void ReciveStateChange(bool value) { }
    }
}