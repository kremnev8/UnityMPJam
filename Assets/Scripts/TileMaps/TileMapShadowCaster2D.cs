using UnityEngine;
using Util;

#if UNITY_EDITOR
using EditorUtil;
using UnityEditor;
#endif

// Copyright 2021 Alejandro Villalba Avila
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

namespace TileMaps
{
    /// <summary>
    /// Generates ShadowCaster2Ds for every continuous block of a tilemap on Awake, applying some settings.
    /// </summary>
    public class TileMapShadowCaster2D : MonoBehaviour
    {
        [SerializeField] protected CompositeCollider2D m_TilemapCollider;

        [SerializeField] public bool m_SelfShadows = true;

        [SerializeField] [HideInInspector] public int[] applyToLayers;

        protected void Reset()
        {
            m_TilemapCollider = GetComponent<CompositeCollider2D>();
        }

        protected void Awake()
        {
            ShadowCaster2DGenerator.GenerateTilemapShadowCasters(m_TilemapCollider, m_SelfShadows);
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(TileMapShadowCaster2D))]
    public class TileMapShadowCaster2DEditor : Editor
    {
        public static GUIContent sortingLayerPrefixLabel = EditorGUIUtility.TrTextContent("Target Sorting Layers", "Apply shadows to the specified sorting layers.");

        
        CustomSortingLayerDropDown m_SortingLayerDropDown;
        
        
        public void OnEnable()
        {
            m_SortingLayerDropDown = new CustomSortingLayerDropDown();
            m_SortingLayerDropDown.OnEnable(serializedObject, "applyToLayers");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            m_SortingLayerDropDown.OnTargetSortingLayers(serializedObject, targets, sortingLayerPrefixLabel, null);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    #endif
}