using Gameplay.Conrollers;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(SpaceTimeObject))]
    public class DungeonEntrance : MonoBehaviour, IInteractable, ITimeLinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;

        public PlayerRole pastTargetPlayer;
        public PlayerRole futureTargetPlayer;
        public NetworkStartPosition startPosition;
        public SpriteRenderer rubbleRenderer;

        public SpaceTimeObject timeObject { get; set; }

        public void Activate(PlayerController player)
        {
            if (timeObject.GetState(timeObject.timeline) != ObjectState.BROKEN)
            {
                player.Feedback("You don't want to go this way!");
            }
        }

        public PlayerRole GetTargetPlayer(Timeline timeline)
        {
            return timeline == Timeline.PAST ? pastTargetPlayer : futureTargetPlayer;
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