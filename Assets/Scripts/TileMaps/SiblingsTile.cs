using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [CreateAssetMenu]
    public class SiblingsTile : RuleTile<SiblingsTile.Neighbor>
    {
        public class Neighbor : RuleTile.TilingRuleOutput.Neighbor {
            // 0, 1, 2 is using in RuleTile.TilingRule.Neighbor
            public const int MyRule1 = 3;
            public const int MyRule2 = 4;
        }
        public override bool RuleMatch(int neighbor, TileBase tile) {
            switch (neighbor) {
                case Neighbor.MyRule1: return false;
                case Neighbor.MyRule2: return true;
            }
            return base.RuleMatch(neighbor, tile);
        }
    }
}