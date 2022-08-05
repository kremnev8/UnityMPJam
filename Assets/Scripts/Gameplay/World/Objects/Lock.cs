using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class Lock : Interactible
    {
        public string itemId;
        public Animator animator;
        public SpriteRenderer topRenderer;
        public SpriteRenderer bottomRenderer;

        private static readonly int open = Animator.StringToHash("Open");

        public override void Activate(PlayerController player)
        {
            CmdActivate(player);
        }

        [Command(requiresAuthority = false)]
        private void CmdActivate(PlayerController player)
        {
            if (state) return;
            
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                if (inventory.TryConsume(itemId))
                {
                    SetState(true);
                    RpcSetState(true);
                    RpcUseKey();
                }
            }
        }

        [ClientRpc]
        public void RpcUseKey()
        {
            animator.SetTrigger(open);
        }
    }
}