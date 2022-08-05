/*using System;
using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public enum Polarity
    {
        ALL,
        TO_PAST,
        TO_FUTURE
    }
    
    public class Portal : Spawnable
    {
        public Polarity polarity;

        public new SpriteRenderer renderer;
        
        public Color all;
        public Color topast;
        public Color tofuture;
        
        private Portal otherPortal;

        private bool ignoreEvent;

        public override void Spawn(PlayerController player, Timeline timeline,  Vector2Int position, Direction direction)
        {
            base.Spawn(player, timeline, position, direction);
            
            if (polarity == Polarity.TO_PAST)
            {
                renderer.color = topast;
            }
            
            if (polarity == Polarity.TO_FUTURE)
            {
                renderer.color = tofuture;
            }

            if (owner.timeline == timeline)
            {
                Timeline other = timeline == Timeline.PAST ? Timeline.FUTURE : Timeline.PAST;
                GameObject portalPrefab = model.projectiles.Get(ProjectileID.PORTAL).spawnOnHit;

                GameObject otherPortalObj = SpawnController.Spawn(player, other, position, direction, portalPrefab);
                otherPortal = otherPortalObj.GetComponent<Portal>();
                otherPortal.otherPortal = this;
            }
        }

        [ClientRpc]
        public override void RpcSpawn(Timeline timeline, Vector2Int position, Direction direction)
        {
            base.RpcSpawn(timeline, position, direction);

            renderer.color = all;
            if (polarity == Polarity.TO_PAST)
            {
                renderer.color = topast;
            }
            
            if (polarity == Polarity.TO_FUTURE)
            {
                renderer.color = tofuture;
            }
        }

        public override void Destroy()
        {
            if (destroyTimer <= 0)
            {
                destroyTimer = destoryTime;
                pendingDestroy = true;
                owner.RemoveObject(ProjectileID.PORTAL, gameObject);
                if (otherPortal != null)
                {
                    otherPortal.Destroy();
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (pendingDestroy) return;
            if (ignoreEvent) return;
            if (isServer && otherPortal != null)
            {
                ICanTeleport canTeleport = other.GetComponent<ICanTeleport>();
                if (canTeleport != null)
                {
                    if (polarity == Polarity.TO_PAST && 
                        otherPortal.timeline != Timeline.PAST) return;
                    
                    if (polarity == Polarity.TO_FUTURE && 
                        otherPortal.timeline != Timeline.FUTURE) return;

                    otherPortal.ignoreEvent = true;
                    bool result = canTeleport.Teleport(otherPortal.timeline, otherPortal.position);
                    if (result)
                    {
                        Destroy();
                    }
                    else
                    {
                        otherPortal.ignoreEvent = false;
                    }
                }
            }
            
        }
    }
}*/