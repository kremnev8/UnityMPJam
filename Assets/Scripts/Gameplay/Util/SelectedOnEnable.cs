using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Util
{
    public class SelectedOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}