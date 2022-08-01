using UnityEngine;

namespace UnityEditor.Tilemaps
{
    static internal partial class AssetCreation
    {
        [MenuItem("Assets/Create/2D/Brushes/Prefab Brush", priority = (int) ExBrushMenuItemOrder.PrefabBrush)]
        static void CreatePrefabBrush()
        {
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<PrefabBrush>(), "New Prefab Brush.asset");
        }

        [MenuItem("Assets/Create/2D/Brushes/Prefab Random Brush",
            priority = (int) ExBrushMenuItemOrder.PrefabRandomBrush)]
        static void CreatePrefabRandomBrush()
        {
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<PrefabRandomBrush>(),
                "New Prefab Random Brush.asset");
        }

    }
}