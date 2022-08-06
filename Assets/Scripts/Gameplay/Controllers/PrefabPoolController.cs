using System;
using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.Util;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.World
{
    public class PrefabPoolController : MonoBehaviour
    {
        public List<GameObject> poolablePrefabs;

        public PrefabPool poolPrefab;
        public Transform poolsTransform;
        public int initialCapacity;

        private Dictionary<string, PrefabPool> pools = new Dictionary<string, PrefabPool>();

        private void Start()
        {
            foreach (GameObject prefab in poolablePrefabs)
            {
                ISpawnable spawnable = prefab.GetComponent<ISpawnable>();
                if (!string.IsNullOrEmpty(spawnable.prefabId))
                {
                    if (pools.ContainsKey(spawnable.prefabId))
                    {
                        Debug.LogError($"Error! Trying to register poolable prefab '{prefab.name}' with id '{spawnable.prefabId}', which is already taken!");
                        continue;
                    }

                    PrefabPool pool = Instantiate(poolPrefab, poolsTransform);
                    pool.gameObject.name = $"{spawnable.prefabId}_pool";
                    pool.InitializePool(prefab, initialCapacity);
                    pools.Add(spawnable.prefabId, pool);
                }
            }
        }

        public static PrefabPool GetPrefabPool(GameObject prefab)
        {
            GameModel model = Simulation.GetModel<GameModel>();
            
            ISpawnable spawnable = prefab.GetComponent<ISpawnable>();
            if (!string.IsNullOrEmpty(spawnable.prefabId))
            {
                return model.poolController.pools[spawnable.prefabId];
            }

            throw new InvalidOperationException($"Failed to get Prefab Pool for '{prefab.name}', because it is not registered!");
        }

        public static void Return(GameObject gameObject)
        {
            PrefabPool pool = GetPrefabPool(gameObject);
            NetworkServer.UnSpawn(gameObject);
            pool.Return(gameObject);
        }


        [Server]
        public static GameObject Spawn(PlayerController owner, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = GetPrefabPool(prefab).Get();
            
            NetworkServer.Spawn(projectileObj);
            ISpawnable behavior = projectileObj.GetComponent<ISpawnable>();
            behavior.Spawn(owner, pos, direction);

            return projectileObj;
        }

        [Server]
        public static GameObject Spawn(Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = GetPrefabPool(prefab).Get();
            NetworkServer.Spawn(projectileObj);
            ISpawnable behavior = projectileObj.GetComponent<ISpawnable>();
            behavior.Spawn(null, pos, direction);

            return projectileObj;
        }

        [Server]
        public static void SpawnWithLink(PlayerController owner, ProjectileID projectile, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Spawn(owner, pos, direction, prefab);
            owner.AddProjectile(projectile, projectileObj);
        }

        [Server]
        public static void SpawnWithReplace(PlayerController owner, ProjectileID projectileID, Vector2Int pos, Direction direction, GameObject oldGO,
            GameObject prefab)
        {
            GameObject projectileObj = Spawn(owner, pos, direction, prefab);
            owner.ReplaceObject(projectileID, oldGO, projectileObj);
        }
    }
}