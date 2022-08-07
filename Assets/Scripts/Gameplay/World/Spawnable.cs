using System;
using Gameplay.Conrollers;
using Gameplay.Controllers;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

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
        [FormerlySerializedAs("destoryTime")] 
        public float destroyTime;
        protected float destroyTimer;
        protected GameModel model;
        public bool pendingDestroy;

        public bool spawnInTheWorld;
        
        private void Start()
        {
            GameNetworkManager.MapStarted += OnMapStarted;
        }

        private void OnEnable()
        {
            GameNetworkManager.MapStarted -= OnMapStarted;
            GameNetworkManager.MapStarted += OnMapStarted;
        }

        private void OnDisable()
        {
            GameNetworkManager.MapStarted -= OnMapStarted;
        }

        private void OnDestroy()
        {
            GameNetworkManager.MapStarted -= OnMapStarted;
        }

        private void OnMapStarted()
        {
            if (isServer && spawnInTheWorld)
            {
                Vector2Int pos = transform.localPosition.ToGridPos();
                Spawn(null, pos, Direction.FORWARD);
            }
        }

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
            Debug.Log($"Spawn {gameObject.name}");
            pendingDestroy = false;
            destroyTimer = 0;
            model = Simulation.GetModel<GameModel>();
            World world = model.levelElement.GetWorld();
            transform.position = world.GetWorldSpacePos(position);
            this.position = position;
        }

        public virtual void Destroy()
        {
            destroyTimer = destroyTime;
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
                if (!prefabId.Equals(""))
                {
                    PrefabPoolController.Return(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}