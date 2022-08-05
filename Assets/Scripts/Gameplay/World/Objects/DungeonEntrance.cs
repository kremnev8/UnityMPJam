using Gameplay.Conrollers;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.World
{
    [RequireComponent(typeof(WorldElement))]
    public class DungeonEntrance : MonoBehaviour, IInteractable, ILinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;

        public PlayerRole targetPlayer;
        public NetworkStartPosition startPosition;

        public WorldElement element { get; set; }

        public void Activate(PlayerController player)
        {
            if (NetworkClient.active)
            {
                player.Feedback("You don't want to go this way!");
            }
        }

        public void ReciveStateChange(bool value) { }
    }
}