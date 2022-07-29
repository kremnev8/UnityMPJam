using System;
using System.Linq;
using Gameplay.Core;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using ScriptableObjects;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.UI
{
    /// <summary>
    /// UI helper that ensures all UI elements are using correct colors
    /// </summary>
    [ExecuteInEditMode]
    public class PaletteGraphic : MonoBehaviour
    {
        private Graphic graphic;
        private new Renderer renderer;
        private SpriteRenderer spriteRenderer;
        private SpriteShapeRenderer shapeRenderer;
        private new Camera camera;

        public bool useColorOnSprites;
        public bool keepAlpha;
        
        [NonSerialized]
        internal ColorPalette palette;

        [HideInInspector]
        [SerializeField]
        public string colorName;


        private void Awake()
        {
            graphic = GetComponent<Graphic>();
            renderer = GetComponent<Renderer>();
            camera = GetComponent<Camera>();

            if (renderer != null)
            {
                shapeRenderer = renderer as SpriteShapeRenderer;
                spriteRenderer = renderer as SpriteRenderer;
            }

           // palette = Simulation.GetModel<GameModel>()?.palette;
        }

        private void Start()
        {
            ApplyColor();
        }

        public void ApplyColor()
        {
            //if (palette == null) palette = Simulation.GetModel<GameModel>()?.palette;
            
            if (palette != null && !String.IsNullOrEmpty(colorName))
            {
                FieldInfo info = typeof(Theme).GetField(colorName);
                Color color = (Color) info.GetValue(palette.currentTheme);

                if (graphic != null)
                {
                    graphic.color = GetColor(graphic.color, color);
                }else if (shapeRenderer != null)
                {
                    shapeRenderer.color = GetColor(shapeRenderer.color, color);
                }else if (spriteRenderer != null && useColorOnSprites)
                {
                    spriteRenderer.color = GetColor(spriteRenderer.color, color);
                }
                else if (renderer != null && renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.color = GetColor(renderer.sharedMaterial.color, color);
                }else if (camera != null)
                {
                    camera.backgroundColor = color;
                }
            }
        }

        private void OnDisable()
        {
            ColorPalette.paletteChanged -= ApplyColor;
        }

        private void OnEnable()
        {
            ColorPalette.paletteChanged += ApplyColor;
            ApplyColor();
        }

        private Color GetColor(Color oldColor, Color paletteColor)
        {
            if (keepAlpha)
            {
                return new Color(paletteColor.r, paletteColor.g, paletteColor.b, oldColor.a);
            }

            return paletteColor;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PaletteGraphic))]
    public class PaletteGraphicEditor : Editor
    {
        SerializedProperty colorNameProperty;
 
        void OnEnable()
        {
            colorNameProperty = serializedObject.FindProperty("colorName");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PaletteGraphic graphic = (PaletteGraphic) target;

            string[] colorNames = typeof(Theme).GetFields()
                .Where(field => field.FieldType == typeof(Color))
                .Select(field => field.Name).ToArray();

            string currentName = colorNameProperty.stringValue;


            int indexof = Array.IndexOf(colorNames, currentName);

            EditorGUI.BeginChangeCheck();

            indexof = EditorGUILayout.Popup("Palette color", indexof, colorNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (indexof != -1 && !string.IsNullOrEmpty(colorNames[indexof]))
                {
                    colorNameProperty.stringValue = colorNames[indexof];
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (graphic.palette != null)
            {
                Color color = Color.black;
                if (!String.IsNullOrEmpty(graphic.colorName))
                {
                    FieldInfo info = typeof(Theme).GetField(graphic.colorName);
                    color = (Color) info.GetValue(graphic.palette.currentTheme);
                }


                GUI.enabled = false;
                EditorGUILayout.ColorField("Graphic Color", color);
                GUI.enabled = true;
            }

            if (GUILayout.Button("Apply"))
            {
                graphic.ApplyColor();
            }
        }
    }
#endif
}