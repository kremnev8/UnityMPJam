using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gameplay.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Util
{
    /// <summary>
    /// Extension methods for GameObjects and MonoBehaviors
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Invoke method after delay with arguments
        /// </summary>
        public static void Invoke(this MonoBehaviour behaviour, string method, object options, float delay)
        {
            behaviour.StartCoroutine((IEnumerator) _invoke(behaviour, method, delay, options));
        }

        private static IEnumerator _invoke(this MonoBehaviour behaviour, string method, float delay, object options)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            Type instance = behaviour.GetType();
            MethodInfo mthd = instance.GetMethod(method);
            mthd?.Invoke(behaviour, new[] {options});

            yield return null;
        }

        /// <summary>
        /// Destroy all children of this game object
        /// </summary>
        public static void ClearChildren(this GameObject thatObject)
        {
            ClearChildren(thatObject.transform);
        }
        
        /// <summary>
        /// Destroy all children of this game object
        /// </summary>
        public static void ClearChildren(this Transform trans)
        {
            //Array to hold all child obj
            List<GameObject> allChildren = new List<GameObject>(trans.childCount);

            //Find all child obj and store to that array
            foreach (Transform child in trans)
            {
                allChildren.Add(child.gameObject);
            }

            if (allChildren.Count == 0) return;


            //Now destroy them
            foreach (GameObject child in allChildren)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(child);
#else
                Object.Destroy(child);
#endif
            }
        }
        
        public static void DrawWireGizmo(this MonoBehaviour mono, List<ValueConnection> connections)
        {
            foreach (ValueConnection connection in connections)
            {
                if (connection != null && connection.target != null)
                {
                    Gizmos.color = connection.invert ? Color.blue : Color.magenta;
                    
                    Gizmos.DrawLine(mono.transform.position, connection.target.transform.position);
                }
            }
        }

        public static void DrawWireGizmo(this MonoBehaviour mono, List<LogicConnection> connections)
        {
            foreach (LogicConnection connection in connections)
            {
                if (connection != null && connection.target != null)
                {
                    Gizmos.color = connection.value ? Color.green : Color.red;
                    Gizmos.DrawLine(mono.transform.position, connection.target.transform.position);
                }
            }
        }
    }
}