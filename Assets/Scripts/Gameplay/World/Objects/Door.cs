using System;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using UnityEngine;

namespace Gameplay.World
{
    public class Door : MonoBehaviour, ITimeLinked
    {
        public Transform doorTransform;
        public new Collider2D collider;
        public ParticleLight timeGlitchEffect;
        public float moveTime;
        
        public Vector3 openPos;
        public Vector3 closedPos;
        public float minPassDistance;


        private bool state;
        private float timeElapsed;

        private bool reachedEnd;

        private void Start()
        {
            reachedEnd = false;
            timeElapsed = moveTime;
        }

        public void ReciveStateChange(bool state, bool isPermanent)
        {
          
            SetState(state, isPermanent);
        }

        private void SetState(bool state, bool isPermanent)
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

                if (isPermanent)
                {
                    timeObject.SendTimeEvent(new[] { state ? 1 : 0 });
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

        public SpaceTimeObject timeObject { get; set; }
        public void Configure(ObjectState state)
        {
        }

        public void ReciveTimeEvent(int[] args)
        {
            SetStateImmidiate(args[0] == 1);
        }
    }
}