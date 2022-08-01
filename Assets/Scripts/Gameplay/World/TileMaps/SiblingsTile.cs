using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [CreateAssetMenu(fileName = "New Siblings Tile", menuName = "2D/Tiles/Big Siblings Tile", order = 0)]
    public class SiblingsTile : RuleTile<SiblingsTile.Neighbor>
    {
        public List<TileBase> sibings = new List<TileBase>();
        public class Neighbor : RuleTile.TilingRuleOutput.Neighbor {
            public const int Sibing = 3;
        }
        public override bool RuleMatch(int neighbor, TileBase tile) {
            switch (neighbor) {
                case Neighbor.Sibing: return sibings.Contains(tile);
            }
            return base.RuleMatch(neighbor, tile);
        }
    }
}