using Gameplay.Core;
using Gameplay.World;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.Controllers.Player
{
    public class CameraPanController : MonoBehaviour
    {
        private Transform cameraTranform;
        private new Camera camera;
        private PixelPerfectCamera perfectCamera;

        private InputAction direction;
        private InputAction anyDirKeyHold;

        private GameModel model;
        public static PanBounds bounds;

        public float releaseTime;
        public float returnTime;
        public float smoothTime;
        public float maxSpeed;

        private bool panEnabled;
        private float timeSinceKeypress;
        private float cameraReturnTimer;

        private Vector3 cameraVelocity;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();

            cameraTranform = GetComponent<Transform>();
            camera = GetComponent<Camera>();
            perfectCamera = GetComponent<PixelPerfectCamera>();

            direction = model.input.actions["dirArrows"];
            anyDirKeyHold = model.input.actions["holdArrows"];

            anyDirKeyHold.performed += OnPanStarted;
        }

        private void OnPanStarted(InputAction.CallbackContext obj)
        {
            cameraReturnTimer = 0;
            timeSinceKeypress = 0;
            panEnabled = true;
        }

        private void Update()
        {
            if (panEnabled)
            {
                Vector2 dir = direction.ReadValue<Vector2>();
                if (dir != Vector2.zero)
                {
                    Vector3 pos = cameraTranform.position;
                    Vector3 target = pos + (Vector3)dir;

                    Vector3 newPos = Vector3.SmoothDamp(pos, target, ref cameraVelocity, smoothTime, maxSpeed);
                    SetPosClamped(newPos);
                    timeSinceKeypress = 0;
                }
                else
                {
                    timeSinceKeypress += Time.deltaTime;
                }

                if (timeSinceKeypress > releaseTime)
                {
                    panEnabled = false;
                }
            }
            else
            {
                cameraReturnTimer += Time.deltaTime;
                if (cameraReturnTimer < returnTime)
                {
                    float t = cameraReturnTimer / returnTime;
                    Vector3 targetPos = cameraTranform.localPosition;
                    targetPos.x = 0;
                    targetPos.y = 0;
                    cameraTranform.localPosition = Vector3.Lerp(cameraTranform.localPosition, targetPos, t);
                }
                else
                {
                    Vector3 targetPos = cameraTranform.localPosition;
                    targetPos.x = 0;
                    targetPos.y = 0;
                    cameraTranform.localPosition = targetPos;
                }
            }
        }
        
        
        private void SetPosClamped(Vector3 pos)
        {
            if (bounds == null) return;
            Vector2 screenSize = camera.OrthographicSize(perfectCamera);

            Rect screenBounds = new Rect(
                bounds.worldBounds.position + screenSize / 2, 
                bounds.worldBounds.size - screenSize);

            Vector3 clampedPos = pos;
            
            clampedPos.x = Mathf.Clamp(clampedPos.x, screenBounds.xMin, screenBounds.xMax);
            clampedPos.y = Mathf.Clamp(clampedPos.y, screenBounds.yMin, screenBounds.yMax);

            cameraTranform.position = clampedPos;
        }
    }
}