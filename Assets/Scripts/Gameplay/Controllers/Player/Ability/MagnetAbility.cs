using Gameplay.Conrollers;
using Gameplay.Util;
using Gameplay.World;
using UnityEngine;
using Util;

namespace Gameplay.Controllers.Player.Ability
{
    public enum Polarity
    {
        PULL,
        PUSH
    }
    
    [CreateAssetMenu(fileName = "Magnet Ability", menuName = "SO/New Magnet Ability", order = 0)]
    public class MagnetAbility : BaseAbility
    {
        public int maxDistance;
        public Polarity polarity;

        public LayerMask targetMask;
        private static RaycastHit2D[] hits = new RaycastHit2D[20];
        
        public override bool ActivateAbility(PlayerController player, Direction direction)
        {
            Vector3 pos = player.position.ToWorldPos();
            
            int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.9f, direction.GetVector(), hits, 2f * maxDistance, targetMask);

            IMoveAble target = null;

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.collider != null && 
                        hit.collider != player.collider && 
                        !hit.collider.isTrigger)
                    {
                        IMoveAble moveAble = hit.collider.GetComponent<IMoveAble>();
                        if (moveAble != null && moveAble.isMagnetic)
                        {
                            target = moveAble;
                            break;
                        }
                    }
                }
            }

            if (target == null)
            {
                player.RpcFeedback("Nothing magnetic this way!");
                return false;
            }

            Direction pushDir = polarity == Polarity.PUSH ? direction : direction.GetOpposite();
            target.Move(pushDir);

            return true;

        }
    }
}