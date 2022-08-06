using System;
using Gameplay.Controllers.Player;
using UnityEditor;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public class PanBounds : MonoBehaviour
    {
        public Rect worldBounds;

        private void Start()
        {
            CameraPanController.bounds = this;
        }
    }
    
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PanBounds))]
    public class TouchCameraControllerEditor : Editor
    {
        private void OnSceneGUI()
        {
            if (!Application.isPlaying)
            {
                var rectExample = (PanBounds)target;

                var rect = RectUtils.ResizeRect(
                    rectExample.worldBounds,
                    Handles.CubeHandleCap,
                    Color.green,
                    Color.yellow,
                    HandleUtility.GetHandleSize(Vector3.zero) * .1f,
                    1);

                rectExample.worldBounds = rect;
            }
        }
    }
#endif
}