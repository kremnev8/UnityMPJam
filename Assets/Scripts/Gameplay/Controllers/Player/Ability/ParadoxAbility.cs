using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Paradox Ability", menuName = "SO/New Paradox Ability", order = 0)]
    public class ParadoxAbility : BaseAbility
    {
        public GameObject paradoxPrefab;

        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            List<GameObject> active = player.GetActiveProjectiles(ProjectileID.PARADOX);
            Vector2Int pos = player.position + direction.GetVector();


            if (active.Count < 1)
            {
                player.SpawnWithLink(ProjectileID.PARADOX, player.timeline, pos, direction, paradoxPrefab);
                return true;
            }

            GameObject oldObj = active.Last();
            player.SpawnWithReplace(ProjectileID.PARADOX, player.timeline, pos, direction, oldObj, paradoxPrefab);
            return true;
        }
    }
}