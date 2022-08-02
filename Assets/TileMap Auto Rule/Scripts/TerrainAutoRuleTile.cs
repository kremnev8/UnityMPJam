
#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using System.Linq;

// ----------------------------------------------------------------------------
// Author: Alexandre Brull
// https://brullalex.itch.io/
// ----------------------------------------------------------------------------


[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New Auto Rule Tile", menuName = "2D/Tiles/Terrain Auto Rule Tile", order = 0)]
public class TerrainAutoRuleTile : ScriptableObject
{

    [SerializeField]
    Texture2D TileMap;
    [SerializeField]
    RuleTile RuleTileTemplate;
    RuleTile RuleTileTemplate_Default;

    private void Awake()
    {
        // Ff there is a default template, load it when the asset is created.
        RuleTileTemplate_Default = Resources.Load("AutoRuleTile_default") as RuleTile;
        if (RuleTileTemplate_Default != null)
        {
            RuleTileTemplate = RuleTileTemplate_Default;
        }
    }

    public void OverrideRuleTile()
    {
        // Make a copy of the Rule Tile Template from a new asset.
        RuleTile _new = CreateInstance<RuleTile>();
        EditorUtility.CopySerialized(RuleTileTemplate, _new);

        // Get all the sprites in the Texture2D file (TileMap)
        string spriteSheet = AssetDatabase.GetAssetPath(TileMap);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
            .OfType<Sprite>()
            .OrderBy(sprite =>
            {
                if (sprite.name.Contains('_'))
                {
                    string index = sprite.name.Split("_")[^1];
                    return int.Parse(index);
                }

                return 0;
        }).ToArray();

        if (sprites.Length != RuleTileTemplate.m_TilingRules.Count)
        {
            Debug.LogWarning("The Tilemap doesn't have the same number of sprites than the Rule Tile template has rules.");
        }

        // Set all the sprites of the TileMap.
        for (int i = 0; i < RuleTileTemplate.m_TilingRules.Count; i++)
        {
            _new.m_TilingRules[i].m_Sprites[0] = sprites[i];
            _new.m_DefaultSprite = sprites[^1];
        }

        // Replace this Asset with the new one.
        AssetDatabase.CreateAsset(_new, AssetDatabase.GetAssetPath(this));
    }
}
#endif