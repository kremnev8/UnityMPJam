using Gameplay.Core;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class ArrowScroll : Selectable
    {
        public TMP_Text levelName;
        
        private InputAction leftAction;
        private InputAction rightAction;

        public UnityEvent left;
        public UnityEvent right;

        protected override void Awake()
        {
            base.Awake();
            GameModel model = Simulation.GetModel<GameModel>();
            if (model != null)
            {
                leftAction = model.input.actions["left"];
                rightAction = model.input.actions["right"];

                leftAction.performed += OnLeft;
                rightAction.performed += OnRight;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            leftAction.performed -= OnLeft;
            rightAction.performed -= OnRight;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (leftAction != null)
            {
                leftAction.performed -= OnLeft;
                rightAction.performed -= OnRight;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (leftAction != null)
            {
                leftAction.performed -= OnLeft;
                rightAction.performed -= OnRight;
                leftAction.performed += OnLeft;
                rightAction.performed += OnRight;
            }
        }

        private void OnRight(InputAction.CallbackContext obj)
        {
            OnRight();
        }

        private void OnLeft(InputAction.CallbackContext obj)
        {
            OnLeft();
        }


        public void OnLeft()
        {
            if (currentSelectionState == SelectionState.Selected)
            {
                left?.Invoke();
            }
        }

        public void OnRight()
        {
            if (currentSelectionState == SelectionState.Selected)
            {
                right?.Invoke();
            }
        }

        public void SetText(string text)
        {
            levelName.text = text;
        }
    }
}