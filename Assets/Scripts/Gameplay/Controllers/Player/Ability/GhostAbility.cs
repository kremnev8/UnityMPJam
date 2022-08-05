using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Ghost Ability", menuName = "SO/New Ghost Ability", order = 0)]
    public class GhostAbility : BaseAbility
    {
        public float timeInGhost;
        
        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            player.RpcActivateGhostMode(timeInGhost);
            return true;
        }
    }
}