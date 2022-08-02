using System;
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
    public class Portal : NetworkBehaviour, ISpawnable
    {
        private Vector2Int position;
        
        private PlayerController owner;
        private Portal otherPortal;

        public float destoryTime;
        private float destroyTimer;
        
        [SyncVar]
        private Timeline m_timeline;
        
        public Timeline timeline
        {
            get => m_timeline;
            set => m_timeline = value;
        }

        private static List<ICanTeleport> ignoreList = new List<ICanTeleport>();

        public void Spawn(PlayerController player, Timeline timeline,  Vector2Int position, Direction direction)
        {
            GameModel model = Simulation.GetModel<GameModel>();
            World world = model.spacetime.GetWorld(timeline);
            world.AddObject(gameObject, position);
            this.position = position;
            
            m_timeline = timeline;
            owner = player;

            if (owner.timeline == timeline)
            {
                Timeline other = timeline == Timeline.PAST ? Timeline.FUTURE : Timeline.PAST;
                GameObject portalPrefab = model.projectiles.Get(ProjectileID.PORTAL).spawnOnHit;

                GameObject otherPortalObj = owner.Spawn(other, position, direction, portalPrefab);
                otherPortal = otherPortalObj.GetComponent<Portal>();
                otherPortal.otherPortal = this;
            }
        }

        public void Destroy()
        {
            if (destroyTimer <= 0)
            {
                destroyTimer = destoryTime;
                owner.RemoveObject(ProjectileID.PORTAL, gameObject);
                otherPortal.Destroy();
            }
        }
        
        void FixedUpdate()
        {
            if (destroyTimer > 0)
            {
                destroyTimer -= Time.fixedDeltaTime;
                if (destroyTimer < 0)
                {
                    Destroy(gameObject);
                } 
                
                return;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isServer)
            {
                ICanTeleport canTeleport = other.GetComponent<ICanTeleport>();
                if (canTeleport != null)
                {
                    if (ignoreList.Contains(canTeleport))
                    {
                        ignoreList.Remove(canTeleport);
                        return;
                    }
                    
                    ignoreList.Add(canTeleport);
                    canTeleport.Teleport(otherPortal.timeline, otherPortal.position);
                    Destroy();
                }
            }
            
        }
    }
}