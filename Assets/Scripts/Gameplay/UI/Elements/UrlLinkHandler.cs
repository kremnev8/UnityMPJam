using System;
using Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.UI
{
    /// <summary>
    /// Handler for TMP rich text links. Opens any link as a URL
    /// </summary>
    public class UrlLinkHandler : MonoBehaviour
    {
        private TMP_Text text;

        private InputAction primaryContact;
        private InputAction primaryPosition;


        private void Start()
        {
            text = GetComponent<TMP_Text>();
            GameModel model = Simulation.GetModel<GameModel>();
            primaryContact = model.input.actions["primaryContact"];
            primaryPosition = model.input.actions["primaryPosition"];
            primaryContact.started += OnClick;
        }

        private void OnEnable()
        {
            if (primaryContact != null)
                primaryContact.started += OnClick;
        }

        private void OnDisable()
        {
            if (primaryContact != null)
                primaryContact.started -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext obj)
        {
            Vector2 position = primaryPosition.ReadValue<Vector2>();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, position, null);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                string currentLink = linkInfo.GetLinkID();

                if (!currentLink.Equals(""))
                {
                    Application.OpenURL(currentLink);
                }
            }
        }
    }
}