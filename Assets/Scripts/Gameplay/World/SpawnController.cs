using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Util;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public static class SpawnController
    {
        [Server]
        public static GameObject Spawn(PlayerController owner, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Object.Instantiate(prefab);
            NetworkServer.Spawn(projectileObj);
            ISpawnable behavior = projectileObj.GetComponent<ISpawnable>();
            behavior.Spawn(owner, pos, direction);

            return projectileObj;
        }
        
        [Server]
        public static GameObject Spawn(Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Object.Instantiate(prefab);
            NetworkServer.Spawn(projectileObj);
            ISpawnable behavior = projectileObj.GetComponent<ISpawnable>();
            behavior.Spawn(null,pos, direction);

            return projectileObj;
        }

        [Server]
        public static void SpawnWithLink(PlayerController owner, ProjectileID projectile, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Spawn(owner, pos, direction, prefab);
            owner.AddProjectile(projectile, projectileObj);
        }

        [Server]
        public static void SpawnWithReplace(PlayerController owner, ProjectileID projectileID, Vector2Int pos, Direction direction, GameObject oldGO, GameObject prefab)
        {
            GameObject projectileObj = Spawn(owner, pos, direction, prefab);
            owner.ReplaceObject(projectileID, oldGO, projectileObj);
        }
    }
    
}