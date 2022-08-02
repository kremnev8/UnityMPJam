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
    public class ProjectileBehavior : NetworkBehaviour, ISpawnable, ICanTeleport
    {
        public new SpriteRenderer renderer;

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

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            ProjectileDB projectileDB = model.projectiles;
            projectile = projectileDB.Get(projectileID);

            body = GetComponent<Rigidbody2D>();

            EnterState(ProjectileState.MOVING);
        }

        [Server]
        public void Spawn(PlayerController player, Timeline timeline, Vector2Int position, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();

            m_timeline = timeline;
            this.position = position;
            owner = player;
            
            RpcSpawn(position, timeline, direction);
            StartMove(direction, projectile.startMoveSpeed);
        }

        [ClientRpc]
        public void RpcSpawn(Vector2Int position, Timeline timeline, Direction direction)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            m_timeline = timeline;
            this.position = position;
            moveDir = direction;
            EnterState(ProjectileState.IDLE);
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
                        if (!hit.collider.isTrigger)
                        {
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
                owner.SpawnWithReplace(projectileID, m_timeline, transform.position.ToGridPos(), moveDir.GetOpposite(), gameObject, projectile.spawnOnHit);
            }
            else
            {
                Destroy();
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

        public void Teleport(Timeline timeline, Vector2Int position)
        {
            m_timeline = timeline;
            this.position = position;
            moveDir = moveDir.GetOpposite();
            if (isServer)
            {
                RpcTeleport(timeline, position);
            }
        }

        [ClientRpc(includeOwner = false)]
        public void RpcTeleport(Timeline timeline, Vector2Int position)
        {
            if (isClientOnly)
            {
                Teleport(timeline, position);
            }
        }
    }
}