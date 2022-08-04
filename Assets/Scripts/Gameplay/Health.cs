using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : NetworkBehaviour
    {
        [Serializable]
        public class IntEvent : UnityEvent<int>
        {
            
        }
        
        [SyncVar]
        private int currentHp;
        
        public int maxHp = 1;
        public bool IsAlive => currentHp > 0;

        public IntEvent hurt;
        public IntEvent healed;
        public UnityEvent isZero;

        /// <summary>
        /// Increment the HP of the entity.
        /// </summary>
        [Server]
        public void Increment(int heal)
        {
            currentHp = Mathf.Clamp(currentHp + heal, 0, maxHp);
            healed?.Invoke(currentHp);
        }

        /// <summary>
        /// Decrement the HP of the entity. Will trigger a HealthIsZero event when
        /// current HP reaches 0.
        /// </summary>
        [Server]
        public void Decrement(int damage)
        {
            currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);

            switch (currentHp)
            {
                case 0:
                    isZero?.Invoke();
                    break;
                case > 0:
                    hurt?.Invoke(currentHp);
                    RpcHurt(currentHp);
                    break;
            }
        }

        [ClientRpc]
        private void RpcHurt(int current)
        {
            hurt?.Invoke(current);
        }

        [Server]
        public void Die()
        {
            Decrement(maxHp);
        }

        [Server]
        public void Respawn()
        {
            Increment(maxHp);
        }


        void Awake()
        {
            currentHp = maxHp;
        }
    }
}
