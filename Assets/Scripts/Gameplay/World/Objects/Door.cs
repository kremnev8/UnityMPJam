using System;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using UnityEngine;

namespace Gameplay.World
{
    [RequireComponent(typeof(WorldElement))]
    public class Door : MonoBehaviour, ILinked
    {
        public Transform doorTransform;
        public new Collider2D collider;
        public ParticleLight timeGlitchEffect;
        public float moveTime;
        
        public Vector3 openPos;
        public Vector3 closedPos;
        public float minPassDistance;

        public bool startState;
        
        private bool state;
        private float timeElapsed;

        private bool reachedEnd;

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
            }
        }

        private void SetStateImmidiate(bool value)
        {
            state = value;
            reachedEnd = false;
            timeElapsed = moveTime;
            timeGlitchEffect.Play();
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
            }
            else if (!reachedEnd)
            {
                doorTransform.localPosition = state ? openPos : closedPos;
                collider.enabled = !state;
                reachedEnd = true;
            }
        }

        public WorldElement element { get; set; }
    }
}