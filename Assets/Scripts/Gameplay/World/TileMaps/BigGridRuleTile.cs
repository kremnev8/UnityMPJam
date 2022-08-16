using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [CreateAssetMenu(fileName = "New Big Grid Rule Tile", menuName = "2D/Tiles/Big Grid Rule Tile", order = 0)]
    public class BigGridRuleTile : RuleTile
    {
        public Vector3Int offset;

        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            var iden = Matrix4x4.identity;
            iden = Matrix4x4.identity;

            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            Matrix4x4 transform = iden;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (RuleMatches(rule, position, tilemap, ref transform))
                {
                    Vector3Int pos = position + offset;

                    int xoff = Mathf.Abs(pos.x % 2);
                    int yoff = Mathf.Abs(pos.y % 2) * 2;
                    int index = xoff + yoff;
                    if (rule.m_Sprites != null && index < rule.m_Sprites.Length)
                    {
                        tileData.sprite = rule.m_Sprites[index];
                    }
                    else
                    {
                        tileData.sprite = rule.m_Sprites[0];
                    }

                    tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;
                    break;
                }
            }
        }
    }
}