using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Epic.OnlineServices;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;
using ValueConnection = Gameplay.World.ValueConnection;

namespace Editor
{
    [CanEditMultipleObjects] // Don't ruin everyone's day
    [CustomEditor(typeof(MonoBehaviour), true)] // Target all MonoBehaviours and descendants
    public class MonoBehaviourCustomEditor : UnityEditor.Editor
    {
        private void Awake()
        {
            NetworkBehaviourInspector.onSceneGUI += OnSceneGUI_Internal;
        }

        private void OnDestroy()
        {
            NetworkBehaviourInspector.onSceneGUI -= OnSceneGUI_Internal;
        }

        private void OnSceneGUI()
        {
            OnSceneGUI_Internal(target);
        }

        private static MonoBehaviour lastTarget;
        private static Vector3 handlePos;

        private const int ConnectionControlID = 546143;

        private static void ConnectionHandle(MonoBehaviour target, Action<WorldElement> connectCallback)
        {
            void ResetPosition()
            {
                Vector3 startPos = target.transform.position;
                startPos += new Vector3(1, 1, 0);
                handlePos = startPos;
                lastTarget = target;
            }
            
            Vector3 snap = Vector3.one * 0.5f;
            if (target != lastTarget)
            {
                ResetPosition();
            }

            Event current = Event.current;
            if (current.type == EventType.MouseUp && current.button == 0)
            {
                Transform parent = target.transform.parent;
                foreach (Transform trans in parent)
                {
                    if (trans == target.transform) continue;

                    float dist = (trans.position - handlePos).magnitude;
                    if (dist < 0.5f)
                    {
                        WorldElement element = trans.GetComponent<WorldElement>();
                        if (element != null)
                        {
                            connectCallback?.Invoke(element);
                            break;
                        }
                    }
                }

                ResetPosition();
            }

            handlePos = Handles.FreeMoveHandle(ConnectionControlID, handlePos, Quaternion.identity, 0.25f, snap, Handles.CircleHandleCap);

            Vector3 startPos = target.transform.position;
            startPos += new Vector3(1, 1, 0);

            if (Vector3.SqrMagnitude(startPos - handlePos) > 0.1)
            {
                Handles.DrawLine(target.transform.position, handlePos);
            }
        }

        public static void OnSceneGUI_Internal(Object target)
        {
            MonoBehaviour mono = (MonoBehaviour)target;
            if (Selection.activeGameObject != mono.gameObject) return;

            if (target is IValueConnectable value)
            {
                ConnectionHandle(mono, element =>
                {
                    int index = value.Connections.FindIndex(connection => connection.target == element);
                    if (index != -1)
                    {
                        Debug.Log($"Disconnecting {target.name} from {element.name}!");
                        value.Connections.RemoveAt(index);
                    }
                    else
                    {
                        Debug.Log($"Connecting {target.name} with {element.name}!");
                        value.Connections.Add(new ValueConnection(element));
                    }
                });
            }
            else if (target is ILogicConnectable logic)
            {
                ConnectionHandle(mono, element =>
                {
                    int index = logic.Connections.FindIndex(connection => connection.target == element);
                    if (index != -1)
                    {
                        Debug.Log($"Disconnecting {target.name} from {element.name}!");
                        logic.Connections.RemoveAt(index);
                    }
                    else
                    {
                        Debug.Log($"Connecting {target.name} with {element.name}!");
                        logic.Connections.Add(new LogicConnection(element));
                    }
                });
            }
        }


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the normal inspector
            // Get the type descriptor for the MonoBehaviour we are drawing
            var type = target.GetType();

            // Iterate over each private or public instance method (no static methods atm)
            foreach (var method in
                     type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                // make sure it is decorated by our custom attribute
                var attributes = method.GetCustomAttributes(typeof(InspectorButtonAttribute), true);
                if (attributes.Length > 0)
                {
                    if (GUILayout.Button("Run: " + method.Name))
                    {
                        MethodInfo methodInfo = target.GetType().GetMethod(method.Name,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                        methodInfo?.Invoke(target, Array.Empty<object>());
                    }
                }
            }
        }
    }
}