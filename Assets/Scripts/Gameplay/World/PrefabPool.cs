using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class PrefabPool : MonoBehaviour
    {
        private GameObject prefab;

        public int currentCount;
        private Pool<GameObject> pool;

        public void InitializePool(GameObject poolPrefab, int initialCapacity)
        {
            prefab = poolPrefab;
            NetworkClient.RegisterPrefab(prefab, SpawnHandler, UnspawnHandler);
            // create pool with generator function
            pool = new Pool<GameObject>(CreateNew, initialCapacity);
        }
        
        void OnDestroy()
        {
            if (prefab != null)
                NetworkClient.UnregisterPrefab(prefab);
        }
        
        // used by NetworkClient.RegisterPrefab
        GameObject SpawnHandler(SpawnMessage msg) => Get();

        // used by NetworkClient.RegisterPrefab
        void UnspawnHandler(GameObject spawned) => Return(spawned);

        GameObject CreateNew()
        {
            // use this object as parent so that objects dont crowd hierarchy
            GameObject next = Instantiate(prefab, transform);
            next.name = $"{prefab.name}_pooled_{currentCount}";
            next.SetActive(false);
            currentCount++;
            return next;
        }

        // Used to take Object from Pool.
        // Should be used on server to get the next Object
        // Used on client by NetworkClient to spawn objects
        public GameObject Get()
        {
            GameObject next = pool.Get();

            // set active
            next.SetActive(true);
            return next;
        }

        // Used to put object back into pool so they can b
        // Should be used on server after unspawning an object
        // Used on client by NetworkClient to unspawn objects
        public void Return(GameObject spawned)
        {
            // disable object
            spawned.SetActive(false);

            // add back to pool
            pool.Return(spawned);
        }

        public void ReturnAll()
        {
            foreach (Transform trans in transform)
            {
                GameObject poolObj = trans.gameObject;
                if (poolObj.activeSelf)
                {
                    NetworkServer.UnSpawn(poolObj);
                    Return(poolObj);
                }
            }
        }
    }
}