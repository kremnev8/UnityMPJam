﻿using System;
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
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            
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
            if (isServer && otherPortal != null)
            {
                ICanTeleport canTeleport = other.GetComponent<ICanTeleport>();
                if (canTeleport != null)
                {
                    if (ignoreList.Contains(canTeleport))
                    {
                        ignoreList.Remove(canTeleport);
                        return;
                    }
                    
                    bool result = canTeleport.Teleport(otherPortal.timeline, otherPortal.position);
                    if (result)
                    {
                        ignoreList.Add(canTeleport);
                        Destroy();
                    }
                }
            }
            
        }
    }
}