using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.Util;
using Gameplay.World;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Spawn Portal Ability", menuName = "SO/New Spawn Portal Ability", order = 0)]
    public class SpawnPortalAbility : BaseAbility
    {
        public GameObject prefab;
        public LayerMask mask;
        private static RaycastHit2D[] hits = new RaycastHit2D[6];

        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            try
            {
                GameModel model = Simulation.GetModel<GameModel>();

                List<GameObject> active = player.GetActiveProjectiles(ProjectileID.PORTAL);
                World.World world = model.levelElement.GetWorld();

                Vector2Int pos = player.position + direction.GetVector();
                Vector2 worldPos = world.GetWorldSpacePos(player.position);

                int hitCount = Physics2D.CircleCastNonAlloc(worldPos, 0.9f, direction.GetVector(), hits, 2f, mask);

                bool foundAWall = false;
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        RaycastHit2D hit = hits[i];
                        if (hit.collider != null && hit.collider != player.collider && !hit.collider.isTrigger)
                        {
                            foundAWall = true;
                            break;
                        }
                    }
                }


                if (foundAWall)
                {
                    player.RpcFeedback("Can't use this ability facing a wall", false);
                    return false;
                }

                if (active.Count < 1)
                {
                    PrefabPoolController.SpawnWithLink(player, ProjectileID.PORTAL, pos, direction, prefab);
                    return true;
                }

                GameObject oldObj = active.Last();
                PrefabPoolController.SpawnWithReplace(player, ProjectileID.PORTAL, pos, direction, oldObj, prefab);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
            }

            return false;
        }
    }
}