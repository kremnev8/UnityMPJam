using System;
using Gameplay.Conrollers;
using Gameplay.Core;
using UnityEngine;
using Util;

namespace Gameplay.World.Spacetime
{
    public enum ObjectState : byte
    {
        DOES_NOT_EXIST,
        EXISTS,
        BROKEN
    }

    public class SpaceTimeObject : MonoBehaviour
    {
        public Timeline timeline;
        
        public ObjectState pastState;
        public ObjectState futureState;

        public ITimeLinked target;
        
        [SerializeField]
        protected string m_uniqueId;
        
        private GameModel model;
        
        public string UniqueId 
        {
            get
            {
                if (String.IsNullOrEmpty(m_uniqueId))
                {
                    Vector2Int pos = transform.localPosition.ToGridPos();
                    return $"x:{pos.x},y:{pos.y}";
                }

                return m_uniqueId;
            }
        }
        
        
        public void Set(Timeline currentTime, ITimeLinked link)
        {
            timeline = currentTime;
            target = link;
            target.timeObject = this;
            target.Configure(GetState(timeline));
        }

        public ObjectState GetState(Timeline time)
        {
            
            return time == Timeline.PAST ? pastState : futureState;
        }


        private void Start()
        {
            target = GetComponent<ITimeLinked>();
            model = Simulation.GetModel<GameModel>();
        }

        public void SendTimeEvent(int[] args)
        {
            if (Application.isPlaying && SpacetimeController.eventListenerUp)
            {
                model.spacetime.CallTimeEvent(timeline, UniqueId, args);
            }
        }

        public void SendLogicState(string targetId, bool value)
        {
            if (Application.isPlaying && SpacetimeController.eventListenerUp)
            {
                model.spacetime.ChangeLogicState(timeline, targetId, value);
            }
        }

    }
}