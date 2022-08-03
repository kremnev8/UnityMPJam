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
    public enum Polarity
    {
        ALL,
        TO_PAST,
        TO_FUTURE
    }
    
    public class Portal : NetworkBehaviour, ISpawnable
    {
        public Polarity polarity;

        public SpriteRenderer renderer;
        
        public Color all;
        public Color topast;
        public Color tofuture;
        
        private Vector2Int position;
        
        private PlayerController owner;
        private Portal otherPortal;

        public float destoryTime;
        private float destroyTimer;

        private bool ignoreEvent;
        
        [SyncVar]
        private Timeline m_timeline;
        
        public Timeline timeline
        {
            get => m_timeline;
            set => m_timeline = value;
        }

        public void Spawn(PlayerController player, Timeline timeline,  Vector2Int position, Direction direction)
        {
            GameModel model = Simulation.GetModel<GameModel>();
            World world = model.spacetime.GetWorld(timeline);
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            renderer.color = all;
            if (polarity == Polarity.TO_PAST)
            {
                renderer.color = topast;
            }
            
            if (polarity == Polarity.TO_FUTURE)
            {
                renderer.color = tofuture;
            }
            
            m_timeline = timeline;
            owner = player;
            RpcSpawn(timeline, position, direction);

            if (owner.timeline == timeline)
            {
                Timeline other = timeline == Timeline.PAST ? Timeline.FUTURE : Timeline.PAST;
                GameObject portalPrefab = model.projectiles.Get(ProjectileID.PORTAL).spawnOnHit;

                GameObject otherPortalObj = owner.Spawn(other, position, direction, portalPrefab);
                otherPortal = otherPortalObj.GetComponent<Portal>();
                otherPortal.otherPortal = this;
            }
        }

        [ClientRpc]
        public void RpcSpawn(Timeline timeline, Vector2Int position, Direction direction)
        {
            GameModel model = Simulation.GetModel<GameModel>();
            World world = model.spacetime.GetWorld(timeline);
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            
            renderer.color = all;
            if (polarity == Polarity.TO_PAST)
            {
                renderer.color = topast;
            }
            
            if (polarity == Polarity.TO_FUTURE)
            {
                renderer.color = tofuture;
            }
            
            m_timeline = timeline;
        }

        public void Destroy()
        {
            if (destroyTimer <= 0)
            {
                destroyTimer = destoryTime;
                owner.RemoveObject(ProjectileID.PORTAL, gameObject);
                if (otherPortal != null)
                {
                    otherPortal.Destroy();
                }
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
            if (destroyTimer > 0) return;
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
}