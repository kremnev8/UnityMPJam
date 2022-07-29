using Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class HoverUI : MonoBehaviour
    {
        public TMP_Text tipName;
        public TMP_Text tipText;

        private RectTransform rectTransform;
        private RectTransform rootTransform;

        private PlayerInput input;
        private InputAction mousePosition;
        private GameModel model;

        public bool active;


        private void Start()
        {
            rectTransform = (RectTransform)transform;
            rootTransform = (RectTransform)rectTransform.parent;

            model = Simulation.GetModel<GameModel>();
            input = model.input;
            mousePosition = input.actions["mouse"];

            Hide();
        }

        public void SetTip(string title, string text)
        {
            tipName.text = title;
            tipText.text = text;
            active = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            active = false;
            gameObject.SetActive(false);
        }


        private void Update()
        {
            if (active)
            {
                Vector2 mousePos = mousePosition.ReadValue<Vector2>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rootTransform, mousePos, null, out Vector2 localpoint);

                SetPosClamped(localpoint);
            }
        }

        private void SetPosClamped(Vector2 pos)
        {
            Vector2 screenSize = rectTransform.sizeDelta;

            Rect worldBounds = new Rect(-Screen.width / 2f, -Screen.height / 2f, Screen.width, Screen.height);

            Rect screenBounds = new Rect(
                worldBounds.position + new Vector2(0, screenSize.y),
                worldBounds.size - screenSize);

            pos.x = Mathf.Clamp(pos.x, screenBounds.xMin, screenBounds.xMax);
            pos.y = Mathf.Clamp(pos.y, screenBounds.yMin, screenBounds.yMax);

            rectTransform.anchoredPosition = pos;
        }
    }
}