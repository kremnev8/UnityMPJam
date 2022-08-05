using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Gameplay.World;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Shoot Ability", menuName = "SO/New Shoot Ability", order = 0)]
    public class ShootAbility : BaseAbility
    {
        public ProjectileID projectileID;
        private static RaycastHit2D[] hits = new RaycastHit2D[6];
        
        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            try
            {
                GameModel model = Simulation.GetModel<GameModel>();
                Projectile projectile = model.projectiles.Get(projectileID);
                
                List<GameObject> active = player.GetActiveProjectiles(projectileID);
                World.World world = model.levelElement.GetWorld();
                
                Vector2Int pos = player.position + direction.GetVector();
                Vector2 worldPos = world.GetWorldSpacePos(player.position);

                int hitCount = Physics2D.CircleCastNonAlloc(worldPos, 0.9f, direction.GetVector(), hits, 2f, projectile.wallMask);

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
                    player.RpcFeedback("Can't use this ability facing a wall");
                    return false;
                }

                if (active.Count < projectile.maxActive)
                {
                    SpawnController.SpawnWithLink(player, projectile.itemId, pos, direction, projectile.prefab);
                    return true;
                }

                if (projectile.canReplace)
                {
                    GameObject oldObj = active.Last();
                    SpawnController.SpawnWithReplace(player, projectile.itemId, pos, direction, oldObj, projectile.prefab);
                    return true;
                }

                player.RpcFeedback("Can't spawn new projectile! You have too many!");
                return false;
            }
            catch (IndexOutOfRangeException e)
            {
                player.RpcFeedback($"Internal Server Error: No projectile with ID {projectileID}");
            }
            return false;
            
            
        }
    }
}