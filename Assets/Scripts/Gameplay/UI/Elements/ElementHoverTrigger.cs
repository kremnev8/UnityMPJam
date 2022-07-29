using System;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.UI
{
    public class ElementHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isPointerIn;
        private float timeIn;

        public float tipTime;

        public string tipName;
        public string tipText;

        public bool isEnabled;
        
        private GameModel model;
        private StringsDB strings;
        private HoverUI hoverUI;
        
        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            strings = model.strings;
            hoverUI = model.hoverUI;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerIn = true;
            timeIn = 0;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerIn = false;
            hoverUI.Hide();
        }

        private void Update()
        {
            if (isPointerIn && isEnabled)
            {
                timeIn += Time.deltaTime;
                if (timeIn > tipTime && !hoverUI.active)
                {
                    string title = strings.Get(tipName).text;
                    string text = strings.Get(tipText).text;
                    hoverUI.SetTip(title, text);
                }
            }
        }
    }
}