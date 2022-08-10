using System;
using Gameplay.Conrollers;
using Gameplay.Core;
using UnityEngine;
using Util;

namespace Gameplay.World.Spacetime
{
    public class WorldElement : MonoBehaviour
    {
        public ILinked target;
        
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
        
        
        public void Set(ILinked link)
        {
            target = link;
            target.element = this;
        }


        private void Start()
        {
            target = GetComponent<ILinked>();
            model = Simulation.GetModel<GameModel>();
        }

        public void SendLogicState(string targetId, bool value)
        {
            if (Application.isPlaying && LevelElementController.eventListenerUp)
            {
                model ??= Simulation.GetModel<GameModel>();

                model.levelElement.ChangeLogicState(targetId, value);
            }
        }

    }
}