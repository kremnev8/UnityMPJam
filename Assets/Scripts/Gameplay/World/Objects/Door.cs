using System;
using Gameplay.World.Spacetime;
using UnityEngine;

namespace Gameplay.World
{
    public class Door : MonoBehaviour, ITimeLinked
    {
        public Transform doorTransform;
        public new Collider2D collider;
        public float moveTime;
        
        public Vector3 openPos;
        public Vector3 closedPos;
        public float minPassDistance;


        private bool state;
        private float timeElapsed;

        private bool reachedEnd;

        public void ReciveStateChange(bool state)
        {
            SetState(state);
        }

        private void SetState(bool state, bool sendEvent = true)
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

                if (sendEvent)
                {
                    timeObject.SendTimeEvent(new[] { state ? 1 : 0 });
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
            SetState(args[0] == 1, false);
        }
    }
}