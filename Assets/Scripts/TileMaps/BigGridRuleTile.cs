using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class BigGridRuleTile : TileBase
{
    public Vector3Int offset;
    public Sprite[] sprites;
    
    /// <summary>
    /// Retrieves any tile rendering data from the scripted tile.
    /// </summary>
    /// <param name="position">Position of the Tile on the Tilemap.</param>
    /// <param name="tilemap">The Tilemap the tile is present on.</param>
    /// <param name="tileData">Data to render the tile.</param>
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        var iden = Matrix4x4.identity;

        Vector3Int pos = position + offset;

        int xoff = Mathf.Abs(pos.x % 2);
        int yoff = Mathf.Abs(pos.y % 2) * 2;
        int index = xoff + yoff;
        if (sprites != null && index < sprites.Length)
        {
            tileData.sprite = sprites[index];
        }
        else
        {
            tileData.sprite = default;
        }

        tileData.gameObject = default;
        tileData.colliderType = Tile.ColliderType.None;
        tileData.flags = TileFlags.LockTransform;
        tileData.transform = iden;
    }
}