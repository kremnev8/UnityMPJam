using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Dash Ability", menuName = "SO/New Dash Ability", order = 0)]
    public class DashAbility : BaseAbility
    {
        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            Debug.Log("Not implemented!");
            return true;
        }
    }
}