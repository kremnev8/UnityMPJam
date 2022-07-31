using System;
using Gameplay.Core;
using Gameplay.World;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.Controllers
{
    public enum PlayerState{
    
        IDLE,
        MOVING,
        STUCK_ANIM,
        FALLING
    }
    
    public class SpPlayerController : MonoBehaviour
    {
        public PlayerConfig config;
        public Vector2Int position;
        
        
        private InputAction action;
        private Rigidbody2D body;
        
        private Vector2Int prevPosition;
        private Vector2Int lastMoveDir;
        
        private PlayerState state;
        private float stateTimeLeft;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];

        public void Start()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            action = model.input.actions["move"];

            body = GetComponent<Rigidbody2D>();
            position = ((transform.position - Vector3.one) / 2).ToVector2Int();
            prevPosition = position;
            
            EnterState(PlayerState.IDLE);
        }

        public float GetTimeIn(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.IDLE:
                    return -1;
                case PlayerState.MOVING:
                    return config.moveTime;
                case PlayerState.STUCK_ANIM:
                    return config.failMoveTime;
                case PlayerState.FALLING:
                    return config.moveTime;
                default:
                    return -1;
            }
        }

        public void EnterState(PlayerState state)
        {
            float time = GetTimeIn(state);
            if (time > 0)
            {
                stateTimeLeft = time;
            }

            this.state = state;
        }

        void FixedUpdate()
        {
            if (stateTimeLeft > 0)
            {
                stateTimeLeft -= Time.fixedDeltaTime;
            }
            
            UpdatePosition();
            UpdateInState(state);
            
        }

        private void UpdatePosition()
        {
            if (state != PlayerState.IDLE) return;
                
            Vector2Int dir = action.ReadValue<Vector2>().Apply(Mathf.RoundToInt);
            if (dir != Vector2Int.zero && stateTimeLeft < 0.1f)
            {
                int xAbs = Mathf.Abs(dir.x);
                int yAbs = Mathf.Abs(dir.y);
                if (xAbs + yAbs == 1)
                {
                    lastMoveDir = dir;
                    Vector2 pos = ToWorldPos(position);

                    int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.9f, dir, hits, 2f, config.wallMask);
                    if (hitCount > 0)
                    {
                        bool shouldStop = false;
                        for (int i = 0; i < hitCount; i++)
                        {
                            RaycastHit2D hit = hits[i];
                            if (hit.collider != null)
                            {
                                if (hit.collider.isTrigger)
                                {
                                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                                    if (interactable != null && interactable.FacingDirection == -dir)
                                    {
                                        interactable.Activate();
                                    }
                                }
                                else
                                {
                                    shouldStop = true;
                                }
                            }
                        }

                        if (shouldStop)
                        {
                            EnterState(PlayerState.STUCK_ANIM);
                            return;
                        }
                    }

                    prevPosition = position;
                    position += dir;
                    EnterState(PlayerState.MOVING);
                }
            }
        }

        public void UpdateInState(PlayerState state)
        {
            float stateTime = GetTimeIn(state);
            float t = (stateTime - stateTimeLeft) / stateTimeLeft;
            
            switch (state)
            {
                case PlayerState.IDLE:
                    break;
                case PlayerState.MOVING:
                    
                    if (stateTimeLeft > 0)
                    {
                        body.MovePosition(Vector2.Lerp(ToWorldPos(prevPosition), ToWorldPos(position), t));
                    }
                    else
                    {
                        body.MovePosition(ToWorldPos(position));
                        EnterState(PlayerState.IDLE);
                    }
                    break;
                case PlayerState.STUCK_ANIM:
                    
                    if (stateTimeLeft > 0)
                    {
                        float nt = config.stuckAnim.Evaluate(t);
                        Vector2 pos = ToWorldPos(position);
                        body.MovePosition(Vector2.Lerp(pos, pos + lastMoveDir, nt));
                    }
                    else
                    {
                        body.MovePosition(ToWorldPos(position));
                        EnterState(PlayerState.IDLE);
                    }
                    
                    break;
                case PlayerState.FALLING:
                    break;
            }
        }

        private Vector2 ToWorldPos(Vector2Int gridPos)
        {
            return gridPos * 2 + Vector2Int.one;
        }
    }
}