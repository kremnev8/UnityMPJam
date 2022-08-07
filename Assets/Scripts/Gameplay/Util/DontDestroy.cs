using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Util
{
    public class DontDestroy : MonoBehaviour
    {
        public static List<GameObject> dontDestorylist = new List<GameObject>();

        private void Awake()
        {
            dontDestorylist.Add(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        public static void DestroyAll()
        {
            foreach (GameObject o in dontDestorylist)
            {
                Destroy(o);
            }
        }
    }
}