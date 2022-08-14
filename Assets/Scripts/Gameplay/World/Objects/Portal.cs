using System;
using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using Util;

namespace Gameplay.World
{

    [SelectionBase]
    public class Portal : Spawnable
    {
        public new SpriteRenderer renderer;

        public SimpleAnim iceAnim;
        public SimpleAnim fireAnim;

        public int frameTime;

        private int animTime;
        private int currentFrame;

        public bool multiUse = true;
        
        private Direction faceDirection;
        [SyncVar]
        private PlayerRole ownerRole;
        private Portal otherPortal
        {
            get
            {
                PlayerRole other = ownerRole == PlayerRole.ICE_MAGE ? PlayerRole.FIRE_MAGE : PlayerRole.ICE_MAGE;
                if (portals.ContainsKey(other))
                {
                    if (portals[other].gameObject.activeSelf)
                    {
                        return portals[other];
                    }
                }

                return null;
            }
        }

        private bool ignoreEvent;

        private static Dictionary<PlayerRole, Portal> portals = new Dictionary<PlayerRole, Portal>();
        public static List<ICanTeleport> ignoreList = new List<ICanTeleport>();

        public override void Spawn(PlayerController player, Vector2Int position, Direction direction)
        {
            base.Spawn(player, position, direction);
            ownerRole = player.role;
            faceDirection = direction;
            if (portals.ContainsKey(player.role))
            {
                portals[player.role] = this;
            }
            else
            {
                portals.Add(player.role, this);
            }
        }

        [ClientRpc]
        public override void RpcSpawn(Vector2Int position, Direction direction)
        {
            base.RpcSpawn(position, direction);
            faceDirection = direction;
        }

        public override void Destroy()
        {
            base.Destroy();
            
            if (owner != null)
            {
                owner.RemoveObject(ProjectileID.PORTAL, gameObject);
            }
        }

        private void Update()
        {
            SimpleAnim anim = ownerRole == PlayerRole.ICE_MAGE ? iceAnim : fireAnim;

            animTime++;

            if (animTime >= frameTime)
            {
                animTime = 0;
                currentFrame++;
                if (currentFrame >= anim.frames.Length)
                {
                    currentFrame = 0;
                }
            }

            renderer.sprite = anim.frames[currentFrame];
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
                    if (ignoreList.Contains(canTeleport))
                    {
                        ignoreList.Remove(canTeleport);
                        return;
                    }
                    
                    ignoreList.Add(canTeleport);
                    bool result = canTeleport.Teleport(otherPortal.position, faceDirection);
                    if (result && !multiUse)
                    {
                        Destroy();
                        otherPortal.Destroy();
                    }
                }
            }
        }
    }
}