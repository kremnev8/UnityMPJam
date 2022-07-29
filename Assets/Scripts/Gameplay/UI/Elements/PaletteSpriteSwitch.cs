using System;
using System.Linq;
using System.Reflection;
using Gameplay.Core;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.UI
{
    /// <summary>
    /// UI helper that ensures all UI elements are using correct sprites
    /// </summary>
    [ExecuteInEditMode]
    public class PaletteSpriteSwitch : MonoBehaviour
    {
        public Sprite[] paletteSprites;
        
        private Image image;
        
        [NonSerialized]
        internal ColorPalette palette;

        private void Awake()
        {
            image = GetComponent<Image>();
           // palette = Simulation.GetModel<GameModel>()?.palette;
            
            if (palette != null)
                Array.Resize(ref paletteSprites, palette.themes.Count);
            
            ApplySprite();
        }

        public void ApplySprite()
        {
            if (palette == null)
            {
               // palette = Simulation.GetModel<GameModel>()?.palette;
                
                if (palette != null)
                    Array.Resize(ref paletteSprites, palette.themes.Count);
            }

            if (palette != null)
            {
                if (palette.themeIndex < paletteSprites.Length)
                {
                    image.sprite = paletteSprites[palette.themeIndex];
                }
                else
                {
                    Debug.Log($"Error setting palette sprite for {gameObject.name}: palette sprites array does not have enough items!");
                }
            }
        }
        
        private void OnDisable()
        {
            ColorPalette.paletteChanged -= ApplySprite;
        }

        private void OnEnable()
        {
            ColorPalette.paletteChanged += ApplySprite;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PaletteSpriteSwitch))]
    public class PaletteSpriteSwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PaletteSpriteSwitch graphic = (PaletteSpriteSwitch) target;

            if (GUILayout.Button("Apply"))
            {
                graphic.ApplySprite();
            }
        }
    }
#endif
}