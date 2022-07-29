using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Util
{
    /// <summary>
    /// Toggle utility class to handle Toggle groups using Unity events
    /// </summary>
    [RequireComponent(typeof(ToggleGroup))]
    [ExecuteInEditMode]
    public class EventToggleGroup : MonoBehaviour
    {
        [System.Serializable]
        public class ToggleEvent : UnityEvent<int> { }
     
        [SerializeField]
        public ToggleEvent onActiveTogglesChanged;
     
        [SerializeField]
        private Toggle[] _toggles;
     
        private ToggleGroup _toggleGroup;
     
        private void Awake()
        {
            _toggleGroup = GetComponent<ToggleGroup>();
        }
     
        // Start is called before the first frame update
        void OnEnable()
        {
            _toggles = GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in _toggles)
            {
                if (toggle.group != null && toggle.group != _toggleGroup)
                {
                    Debug.LogError($"EventToggleGroup is trying to register a Toggle that is a member of another group.");
                }
                toggle.group = _toggleGroup;
                toggle.onValueChanged.AddListener(HandleToggleValueChanged);
            }
        }
     
        void HandleToggleValueChanged(bool isOn)
        {
            if (isOn && Application.isPlaying)
            {
                Toggle toggle = _toggleGroup.ActiveToggles().FirstOrDefault();
                onActiveTogglesChanged?.Invoke(Array.IndexOf(_toggles, toggle));
            }
        }

        public void SetToggle(int index)
        {
            if (index < _toggles.Length)
            {
                _toggles[index].isOn = true;
            }
        }
     
        void OnDisable()
        {
            List<Toggle> activeToggles = _toggleGroup.ActiveToggles().ToList();
            foreach (Toggle toggle in activeToggles)
            {
                toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
                toggle.group = null;
            }
        }
    }

}