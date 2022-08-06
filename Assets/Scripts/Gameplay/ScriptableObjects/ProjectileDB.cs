using System;
using Gameplay.Controllers.Player;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class Projectile : GenericItem<ProjectileID>
    {
        public ProjectileID itemId;

        public string projectileName;
        
        public int maxActive;
        public bool canReplace;
        public float startMoveSpeed;
        public float pushMoveSpeed;
        public float pushWithDashSpeed;
        
        public LayerMask wallMask;
        public LayerMask sinkMask;
        public float sinkOffset;

        public int damage;
        public bool destroySelfOnHit;
        public int projectileMass;
        public bool activateInteractibles;
        
        
        public GameObject prefab;
        public GameObject spawnOnHit;
        
        
        public ProjectileID ItemId => itemId;
    }
    
    [CreateAssetMenu(fileName = "Projectile DB", menuName = "SO/New Projectile DB", order = 0)]
    public class ProjectileDB : GenericDB<ProjectileID, Projectile>
    {
        
    }
}