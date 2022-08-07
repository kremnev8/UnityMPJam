using System;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(WorldElement))]
    public class Door : NetworkBehaviour, ILinked
    {
        public Transform doorTransform;
        public new Collider2D collider;
        public float moveTime;

        public RandomAudioSource opening;
        public RandomAudioSource end;
        
        public Vector3 openPos;
        public Vector3 closedPos;
        public float minPassDistance;

        public bool startState;

        public ContactFilter2D playerFilter;
        public int closeDamage;
        
        private bool state;
        private float timeElapsed;

        private bool reachedEnd;
       

        private static Collider2D[] colliders = new Collider2D[10];
        
        private void Start()
        {
            state = startState;
            reachedEnd = false;
            timeElapsed = moveTime;
        }

        public void ReciveStateChange(bool state)
        {
            SetState(state);
        }

        private void SetState(bool state)
        {
            if (this.state != state)
            {
                this.state = state;
                reachedEnd = false;

                if (timeElapsed > moveTime)
                {
                    timeElapsed = 0;
                }
                else
                {
                    timeElapsed = moveTime - timeElapsed;
                }
                if (opening != null)
                    opening.Play();
                if (end != null)
                    end.Stop();
            }
        }

        private void CheckForPlayer()
        {
            int hits = Physics2D.OverlapBox(transform.position, new Vector2(1.4f,1.4f), 0, playerFilter, colliders);

            Debug.DrawLine(transform.position, transform.position + new Vector3(0.7f,0.7f,0), hits > 0 ? Color.red : Color.green);
            
            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    Collider2D hitCollider = colliders[i];
                    if (hitCollider == null) continue;

                    Health health = hitCollider.GetComponent<Health>();
                    if (health == null) continue;

                    health.Decrement(closeDamage);
                }
            }
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed < moveTime)
            {
                float t = timeElapsed / moveTime;
                if (state)
                {
                    t = 1 - t;
                }
                
                doorTransform.localPosition = Vector3.Lerp(openPos, closedPos, t);
                float dist = (doorTransform.localPosition - openPos).magnitude;
                collider.enabled = dist > minPassDistance;

                if (isServer && dist > minPassDistance && !state)
                {
                    CheckForPlayer();
                }
            }
            else if (!reachedEnd)
            {
                doorTransform.localPosition = state ? openPos : closedPos;
                collider.enabled = !state;
                reachedEnd = true;
                if (opening != null)
                    opening.Stop();
                
                if (end != null)
                    end.Play();
            }
        }

        public WorldElement element { get; set; }
    }
}