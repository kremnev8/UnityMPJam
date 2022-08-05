using Gameplay.Conrollers;
using Gameplay.Util;
using Gameplay.World;
using UnityEngine;
using Util;

namespace Gameplay.Controllers.Player.Ability
{
    [CreateAssetMenu(fileName = "Dash Ability", menuName = "SO/New Dash Ability", order = 0)]
    public class DashAbility : BaseAbility
    {
        public int dashDistance = 2;
        public LayerMask dashMask;
        public LayerMask waterTestMask;
        
        private static RaycastHit2D[] hits = new RaycastHit2D[10];
        private static Collider2D[] colliders = new Collider2D[10];
        
        
        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            Vector2 pos = player.position.ToWorldPos();
            

            int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.9f, direction.GetVector(), hits, 2f * dashDistance, dashMask);
            bool hitWall = false;

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.collider != null && hit.collider != player.collider)
                    {
                        if (!hit.collider.isTrigger)
                        {
                            hitWall = true;
                        }
                    }
                }
            }
            
            if (hitWall)
            {
                player.RpcFeedback("Can't dash into walls!");
                return false;
            }

            ContactFilter2D filter2D = new ContactFilter2D()
            {
                layerMask = waterTestMask,
                useTriggers = false,
                useLayerMask = true,
            };
            Vector2 testPos = pos + (Vector2)direction.GetVector() * 2f * dashDistance;

            hitCount = Physics2D.OverlapPoint(testPos, filter2D, colliders);
            bool isFloor = true;
            
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D collider = colliders[i];

                    if (collider != null && collider != player.collider)
                    {
                        isFloor = false;
                        break;
                    }
                }
            }
            
            if (!isFloor)
            {
                player.RpcFeedback("Can't dash into water!");
                return false;
            }
            
            player.RpcDash( player.position, direction, dashDistance);
            return true;
        }
    }
}