using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class Spawnable : NetworkBehaviour, ISpawnable
    {
        [SyncVar] private Timeline m_timeline;

        public Timeline timeline
        {
            get => m_timeline;
            set => m_timeline = value;
        }

        protected Vector2Int m_position;

        public virtual Vector2Int position
        {
            get => m_position;
            set => m_position = value;
        }

        protected PlayerController owner;
        public float destoryTime;
        protected float destroyTimer;
        protected GameModel model;

        public virtual void Spawn(PlayerController player, Timeline timeline, Vector2Int position, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();
            World world = model.spacetime.GetWorld(timeline);
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            m_timeline = timeline;
            owner = player;
            RpcSpawn(timeline, position, direction);
        }

        [ClientRpc]
        public virtual void RpcSpawn(Timeline timeline, Vector2Int position, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();
            World world = model.spacetime.GetWorld(timeline);
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            m_timeline = timeline;
        }

        public virtual void Destroy()
        {
            destroyTimer = destoryTime;
        }
        
        protected virtual void FixedUpdate()
        {
            if (destroyTimer > 0)
            {
                destroyTimer -= Time.fixedDeltaTime;
                if (destroyTimer < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}