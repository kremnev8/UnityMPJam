using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [CreateAssetMenu(fileName = "New Siblings Tile", menuName = "2D/Tiles/Big Siblings Tile", order = 0)]
    public class SiblingsTile : RuleTile
    {
        public List<TileBase> sibings = new List<TileBase>();
        
        public override bool RuleMatch(int neighbor, TileBase other) {
            if (other is RuleOverrideTile ot)
                other = ot.m_InstanceTile;

            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other == this || sibings.Contains(other);
                case TilingRuleOutput.Neighbor.NotThis: return other != this && !sibings.Contains(other);
            }
            return true;
        }
    }
}