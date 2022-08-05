using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public enum ProjectileState
    {
        IDLE,
        STUCK,
        MOVING
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehavior : Spawnable, ICanTeleport, IMoveAble, IHeavyObject
    {
        public ParticleSystem trailSystem;
        public new SpriteRenderer renderer;
        private new Collider2D collider;
        
        private Rigidbody2D body;

        public ProjectileID projectileID;

        public bool shouldRotate;

        public override Vector2Int position
        {
            get
            {
                World world = model.spacetime.GetWorld(timeline);
                return ((Vector3)body.position - world.transform.position).ToGridPos();
            }
            set
            {
                World world = model.spacetime.GetWorld(timeline);
                body.position = world.GetWorldSpacePos(value);
            }
        }


        protected float velocity;
        protected Direction moveDir;

        private ProjectileState state;

        private Projectile projectile;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];

        public int Mass => projectile.projectileMass;

        [Server]
        public override void Spawn(PlayerController player, Timeline timeline, Vector2Int position, Direction direction)
        {
            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            
            base.Spawn(player, timeline, position, direction);
            
            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);
            trailSystem.Play();

            if (projectile.startMoveSpeed > 0)
            {
                StartMove(direction, projectile.startMoveSpeed);
            }
        }

        [ClientRpc]
        public override void RpcSpawn(Timeline timeline, Vector2Int position, Direction direction)
        {
            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            
            base.RpcSpawn(timeline, position, direction);
            
            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);
            
            moveDir = direction;
            trailSystem.Play();
        }

        public void EnterState(ProjectileState state)
        {
            this.state = state;
        }

        [Server]
        public void StartMove(Direction direction, float velocity)
        {
            moveDir = direction;
            this.velocity = velocity;
            
            EnterState(ProjectileState.MOVING);
            RpcStartMove(direction, velocity);
        }

        [Command(requiresAuthority = false)]
        public void CmdStartMove(Direction direction, float velocity)
        {
            moveDir = direction;
            this.velocity = velocity;
            
            EnterState(ProjectileState.MOVING);
            RpcStartMove(direction, velocity);
        }
        

        [ClientRpc]
        public void RpcStartMove(Direction direction, float velocity)
        {
            if (isClientOnly)
            {
                moveDir = direction;
                this.velocity = velocity;
                EnterState(ProjectileState.MOVING);
            }
        }

        [ClientRpc]
        public void RpcStuck()
        {
            velocity = 0;
            EnterState(ProjectileState.STUCK);
        }


        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!pendingDestroy)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, moveDir.GetVector().ToVector3());
            
            if (state != ProjectileState.MOVING) return;

            if (isServer)
            {
                Vector2 pos = body.position;

                int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.33f, moveDir.GetVector(), hits, 0.5f, projectile.wallMask);
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        RaycastHit2D hit = hits[i];
                        if (hit.collider != null)
                        {
                            if (hit.collider == collider) continue;

                            if (!hit.collider.isTrigger)
                            {
                                Debug.Log("hit!");
                                Health health = hit.collider.GetComponent<Health>();
                                if (health != null && projectile.damage > 0 && owner == null)
                                {
                                    health.Decrement(projectile.damage);
                                }
                                
                                velocity = 0;
                                OnHit();
                                EnterState(ProjectileState.STUCK);
                                RpcStuck();
                                return;
                            }
                        }
                    }
                }
            }

            Vector3 newPos = body.position + (Vector2)moveDir.GetVector() * velocity * Time.fixedDeltaTime;
            body.MovePosition(newPos);
        }

        [Server]
        public void OnHit()
        {
            if (projectile.spawnOnHit != null && owner != null)
            {
                SpawnController.SpawnWithReplace(owner, projectileID, timeline, transform.position.ToGridPos(timeline), moveDir.GetOpposite(), gameObject, projectile.spawnOnHit);
            }
            else if (projectile.destroySelfOnHit)
            {
                Destroy();
                if (owner != null)
                {
                    owner.RemoveObject(projectileID, gameObject);
                }
            }
            else
            {
                EnterState(ProjectileState.IDLE);
            }
        }
        

        public bool Teleport(Timeline timeline, Vector2Int position)
        {
            if (destroyTimer > 0) return false;
            
            this.timeline = timeline;
            this.position = position;
            moveDir = moveDir.GetOpposite();
            if (isServer)
            {
                RpcTeleport(timeline, position);
            }

            return true;
        }

        [ClientRpc(includeOwner = false)]
        public void RpcTeleport(Timeline timeline, Vector2Int position)
        {
            if (isClientOnly)
            {
                Teleport(timeline, position);
            }
        }

        [Client]
        public void Move(Direction direction)
        {
            if (projectile.pushMoveSpeed > 0)
            {
                CmdStartMove(direction, projectile.pushMoveSpeed);
            }
        }
    }
}