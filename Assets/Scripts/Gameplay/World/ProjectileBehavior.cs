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
        MOVING,
        SINK
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehavior : Spawnable, ICanTeleport, IMoveAble, IHeavyObject
    {
        public ParticleSystem trailSystem;
        public new SpriteRenderer renderer;
        public Animator animator;
        private new Collider2D collider;

        public RandomAudioSource audioSource;
        public RandomAudioSource deathSource;
        
        private Rigidbody2D body;

        public ProjectileID projectileID;

        public bool shouldRotate;

        public override Vector2Int position
        {
            get
            {
                World world = model.levelElement.GetWorld();
                return ((Vector3)body.position - world.transform.position).ToGridPos();
            }
            set
            {
                World world = model.levelElement.GetWorld();
                body.position = world.GetWorldSpacePos(value);
            }
        }

        public bool isMagnetic => projectile.magnetic;


        protected float velocity;
        protected Direction moveDir;
        protected Vector2Int startMovePos;

        private ProjectileState state;

        private Projectile projectile;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];
        private static Collider2D[] colliders = new Collider2D[6];
        private static readonly int die = Animator.StringToHash("Die");

        public int Mass
        {
            get
            {
                if (projectile == null)
                {
                    ProjectileDB projectileDB = Simulation.GetModel<GameModel>().projectiles;
                    projectile = projectileDB.Get(projectileID);
                }
                return projectile.projectileMass;
            }
        }

        [Server]
        public override void Spawn(PlayerController player, Vector2Int position, Direction direction)
        {
            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            if (animator != null)
            {
                animator.ResetTrigger(die);
                animator.Play("Base Layer.Idle");
            }

            base.Spawn(player, position, direction);

            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);
            trailSystem.Play();

            if (projectile.startMoveSpeed > 0)
            {
                StartMove(direction, projectile.startMoveSpeed);
            }
        }

        [ClientRpc]
        public override void RpcSpawn(Vector2Int position, Direction direction)
        {
            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            if (animator != null)
            {
                animator.ResetTrigger(die);
                animator.Play("Base Layer.Idle");
            }

            base.RpcSpawn(position, direction);

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
            if (state == ProjectileState.MOVING) return;
            
            moveDir = direction;
            this.velocity = velocity;
            startMovePos = position;

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
                startMovePos = position;
                EnterState(ProjectileState.MOVING);
            }
            if (audioSource != null)
                audioSource.Play();
        }

        [ClientRpc]
        public void RpcStuck()
        {
            velocity = 0;
            EnterState(ProjectileState.STUCK);
            if (audioSource != null)
                audioSource.Stop();
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
            if (shouldRotate)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, moveDir.GetVector().ToVector3());

            if (state != ProjectileState.MOVING) return;

            if (isServer)
            {
                Vector2 pos = body.position;

                int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.33f, moveDir.GetVector(), hits, 0.5f, projectile.wallMask);
                bool hitSomething = false;

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
                                Health health = hit.collider.GetComponent<Health>();
                                if (health != null && projectile.damage > 0)
                                {
                                    health.Decrement(projectile.damage);
                                }

                                IMoveAble moveAble = hit.collider.GetComponent<IMoveAble>();
                                IHeavyObject heavy = hit.collider.GetComponent<IHeavyObject>();
                                if (moveAble != null && heavy != null)
                                {
                                    if (heavy.Mass < Mass)
                                    {
                                        moveAble.MoveServer(moveDir);
                                    }
                                }

                                velocity = 0;
                                hitSomething = true;
                            }
                            else if (projectile.activateInteractibles)
                            {
                                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                                if (interactable != null)
                                {
                                    interactable.Activate(owner);
                                }
                            }
                        }
                    }
                }

                if (hitSomething)
                {
                    OnHit();
                    EnterState(ProjectileState.STUCK);
                    RpcStuck();
                    return;
                }

                ContactFilter2D filter2D = new ContactFilter2D()
                {
                    layerMask = projectile.sinkMask,
                    useLayerMask = true,
                };
                Vector2 testPos = body.position + (Vector2)moveDir.GetVector() * -1 * projectile.sinkOffset;

                hitCount = Physics2D.OverlapPoint(testPos, filter2D, colliders);
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider2D hitCollider = colliders[i];
                        if (hitCollider == null) continue;


                        EnterState(ProjectileState.SINK);
                        RpcSink();
                        Destroy();
                        if (owner != null)
                        {
                            owner.RemoveObject(projectileID, gameObject);
                        }

                        break;
                    }
                }
            }

            Vector3 newPos = body.position + (Vector2)moveDir.GetVector() * velocity * Time.fixedDeltaTime;
            body.MovePosition(newPos);

            if (projectile.maxTravelDistance > 0 && isServer)
            {
                float distance = (startMovePos.ToWorldPos() - (Vector2)newPos).magnitude;
                if (distance > projectile.maxTravelDistance)
                {
                    OnHit();
                    EnterState(ProjectileState.STUCK);
                    RpcStuck();
                }
            }
        }

        public override void Destroy()
        {
            if (!pendingDestroy)
            {
                base.Destroy();
                RpcDieAnimation();
            }
        }

        [ClientRpc]
        private void RpcDieAnimation()
        {
            if (animator != null)
                animator.SetTrigger(die);
            
            if (audioSource != null)
                audioSource.Stop();
            
            if (deathSource != null)
                deathSource.Play();
        }

        [ClientRpc]
        private void RpcSink()
        {
            velocity = 0;
            EnterState(ProjectileState.SINK);
            if (audioSource != null)
                audioSource.Stop();
        }

        [Server]
        public void OnHit()
        {
            if (projectile.spawnOnHit != null && owner != null)
            {
                PrefabPoolController.SpawnWithReplace(owner, projectileID, transform.position.ToGridPos(), moveDir, gameObject,
                    projectile.spawnOnHit);
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


        public bool Teleport(Vector2Int position, Direction direction)
        {
            if (destroyTimer > 0) return false;

            this.position = position;
            moveDir = direction;
            if (isServer)
            {
                RpcTeleport(position, direction);
            }

            return true;
        }

        [ClientRpc(includeOwner = false)]
        public void RpcTeleport(Vector2Int position, Direction direction)
        {
            if (isClientOnly)
            {
                Teleport(position, direction);
            }
        }

        [Client]
        public void MoveClient(Direction direction)
        {
            if (projectile.pushMoveSpeed > 0)
            {
                CmdStartMove(direction, projectile.pushMoveSpeed);
            }
        }

        [Server]
        public void MoveServer(Direction direction)
        {
            if (state == ProjectileState.MOVING) return;
            
            moveDir = direction;
            velocity = projectile.pushMoveSpeed;
            startMovePos = position;

            EnterState(ProjectileState.MOVING);
            RpcStartMove(direction, velocity);
        }
    }
}