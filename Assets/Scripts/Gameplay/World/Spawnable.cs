using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class Spawnable : NetworkBehaviour, ISpawnable
    {
        public string m_prefabId;
        public string prefabId => m_prefabId;
        
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
        public bool pendingDestroy;

        public virtual void Spawn(PlayerController player, Vector2Int position, Direction direction)
        {
            pendingDestroy = false;
            destroyTimer = 0;
            model = Simulation.GetModel<GameModel>();
            World world = model.levelElement.GetWorld();
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
            owner = player;
            RpcSpawn(position, direction);
        }

        [ClientRpc]
        public virtual void RpcSpawn(Vector2Int position, Direction direction)
        {
            model = Simulation.GetModel<GameModel>();
            World world = model.levelElement.GetWorld();
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
        }

        public virtual void Destroy()
        {
            destroyTimer = destoryTime;
            pendingDestroy = true;
        }
        
        protected virtual void FixedUpdate()
        {
            if (!pendingDestroy) return;
            
            if (destroyTimer > 0)
            {
                destroyTimer -= Time.fixedDeltaTime;
            }
            
            if (destroyTimer <= 0)
            {
                PrefabPoolController.Return(gameObject);
            }
        }
    }
}