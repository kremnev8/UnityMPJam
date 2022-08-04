using System;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(SpaceTimeObject))]
    public class DungeonExit : NetworkBehaviour, IInteractable, ITimeLinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;

        private GameModel model;
        public SpriteRenderer rubbleRenderer;

        public SpaceTimeObject timeObject { get; set; }

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
        }

        public void Activate(PlayerController player)
        {
            if (timeObject.GetState(timeObject.timeline) != ObjectState.BROKEN)
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


        public void Configure(ObjectState state)
        {
            rubbleRenderer.enabled = false;
            if (state == ObjectState.DOES_NOT_EXIST)
            {
                gameObject.SetActive(false);
            }
            else if (state == ObjectState.BROKEN)
            {
                rubbleRenderer.enabled = true;
            }
        }

        public void ReciveTimeEvent(int[] args) { }

        public void ReciveStateChange(bool value, bool isPermanent) { }
    }
}