using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public class Paradox : Spawnable
    {
        private List<SpaceTimeObject> targets = new List<SpaceTimeObject>();

        private void SetupParadox(Timeline timeline, Vector2Int position)
        {
            targets.Clear();
            World world = model.spacetime.GetWorld(timeline);
            try
            {
                SpaceTimeObject timeObject = world.objects.Values.First(pair => pair.transform.localPosition.ToGridPos() == position);
                timeObject.underParadox = true;
                targets.Add(timeObject);

                World otherWorld = model.spacetime.GetWorld(timeline == Timeline.PAST ? Timeline.FUTURE : Timeline.PAST);
                SpaceTimeObject timeObject1 = otherWorld.GetObject(timeObject.UniqueId);
                if (timeObject1 != null)
                {
                    timeObject1.underParadox = true;
                    targets.Add(timeObject1);
                }
            }
            catch (InvalidOperationException)
            {
                Debug.Log($"Failed to paradox at {position}!");
            }
        }

        public override void RpcSpawn(Timeline timeline, Vector2Int position, Direction direction)
        {
            base.RpcSpawn(timeline, position, direction);
            
            Debug.Log("Start the paradox");
            SetupParadox(timeline, position);
        }

        public override void Destroy()
        {
            base.Destroy();
            foreach (SpaceTimeObject target in targets)
            {
                target.underParadox = false;
            }
        }
    }
}