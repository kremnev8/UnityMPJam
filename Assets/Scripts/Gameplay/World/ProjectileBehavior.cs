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
    public class ProjectileBehavior : NetworkBehaviour, ISpawnable, ICanTeleport, IMoveAble, IHeavyObject
    {
        public ParticleSystem trailSystem;
        public new SpriteRenderer renderer;
        public Collider2D collider;
        
        private Rigidbody2D body;

        public ProjectileID projectileID;

        public Vector2Int position
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
        private PlayerController owner;
        private GameModel model;

        public float destoryTime;
        private float destroyTimer;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];

        [SyncVar] private Timeline m_timeline;

        public Timeline timeline
        {
            get => m_timeline;
            set => m_timeline = value;
        }
        
        public int Mass => projectile.projectileMass;

        [Server]
        public void Spawn(PlayerController player, Timeline timeline, Vector2Int position, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();
            body = GetComponent<Rigidbody2D>();
            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);
            collider = GetComponent<Collider2D>();

            m_timeline = timeline;
            this.position = position;
            owner = player;
            trailSystem.Play();

            RpcSpawn(position, timeline, direction);
            if (projectile.startMoveSpeed > 0)
            {
                StartMove(direction, projectile.startMoveSpeed);
            }
        }

        [ClientRpc]
        public void RpcSpawn(Vector2Int position, Timeline timeline, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();
            body = GetComponent<Rigidbody2D>();
            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);
            collider = GetComponent<Collider2D>();

            m_timeline = timeline;
            this.position = position;
            moveDir = direction;
            trailSystem.Play();
        }

        [Server]
        public void Destroy()
        {
            destroyTimer = destoryTime;
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

            UpdatePosition();
            UpdateInState(state);
        }

        private void UpdatePosition()
        {
            if (!isServer) return;
            if (state != ProjectileState.MOVING) return;

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
                            velocity = 0;
                            EnterState(ProjectileState.STUCK);
                            RpcStuck();
                            return;
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
            if (projectile.spawnOnHit != null)
            {
                
                owner.SpawnWithReplace(projectileID, m_timeline, transform.position.ToGridPos(timeline), moveDir.GetOpposite(), gameObject, projectile.spawnOnHit);
            }
            else if (projectile.destroySelfOnHit)
            {
                Destroy();
                owner.RemoveObject(projectileID, gameObject);
            }
            else
            {
                EnterState(ProjectileState.IDLE);
            }
        }


        public void UpdateInState(ProjectileState state)
        {
            switch (state)
            {
                case ProjectileState.IDLE:
                    break;
                case ProjectileState.STUCK:
                    if (isServer)
                    {
                        OnHit();
                    }
                    break;
                case ProjectileState.MOVING:

                    break;
            }
        }

        public bool Teleport(Timeline timeline, Vector2Int position)
        {
            if (destroyTimer > 0) return false;
            
            m_timeline = timeline;
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