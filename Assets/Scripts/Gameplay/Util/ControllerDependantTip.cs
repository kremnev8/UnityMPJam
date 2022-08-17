using System;
using System.Linq;
using Gameplay.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Gameplay.Util
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ControllerDependantTip : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        public Sprite keyboardSprite;
        public Sprite xboxSprite;
        public Sprite psSprite;

        private PlayerInput input;
        
        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            GameModel model = Simulation.GetModel<GameModel>();
            input = model.input;
            OnSchemeChanged();
        }
        
        private void OnEnable() {
            InputUser.onChange += OnInputDeviceChange;
        }
 
        private void OnDisable() {
            InputUser.onChange -= OnInputDeviceChange;
        }

        private void OnSchemeChanged()
        {
            if (input.currentControlScheme.Equals("Keyboard"))
            {
                spriteRenderer.sprite = keyboardSprite;
            }
            else
            {
                bool isPs = input.devices.Any(device => device.description.manufacturer.ToLower().Contains("sony"));
                spriteRenderer.sprite = isPs ? psSprite : xboxSprite;
            }
        }

        void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device) {
            if (change == InputUserChange.ControlSchemeChanged) {
                OnSchemeChanged();
            }
        }
    }
}