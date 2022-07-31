using System;
using Gameplay;
using Gameplay.Controllers;
using Gameplay.Core;
using Gameplay.World;
using UnityEngine;
using Mirror;
using ScriptableObjects;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.Conrollers
{

    public enum PlayerRole : byte
    {
        PAST,
        FUTURE
    }
    
    public class PlayerController : NetworkBehaviour, IPlayerData
    {
        [SyncVar]
        private string playerName;
        [SyncVar(hook=nameof(SetRoleHook))]
        private PlayerRole m_role;
        [SyncVar]
        public bool controlEnabled;
        
        public PlayerConfig config;
        public Vector2Int position;

        public new SpriteRenderer renderer;
        public Sprite pastSprite;
        public Sprite futureSprite;
        
        private InputAction action;
        private Rigidbody2D body;
        
        private Vector2Int prevPosition;
        private Vector2Int lastMoveDir;
        
        private PlayerState state;
        private float stateTimeLeft;

        private GameModel model;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];
        
        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        public PlayerRole role
        {
            get => m_role;
            set => m_role = value;
        }

        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            action = model.input.actions["move"];

            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            position = ((transform.position - Vector3.one) / 2).ToVector2Int();
            prevPosition = position;
            
            EnterState(PlayerState.IDLE);
            UpdateVisual();
            DontDestroyOnLoad(gameObject);
        }

        private void UpdateVisual()
        {
            renderer.sprite = role == PlayerRole.PAST ? pastSprite : futureSprite;
        }

        void SetRoleHook(PlayerRole before, PlayerRole now)
        {
            if (renderer == null)
            {
                renderer = GetComponent<SpriteRenderer>();
            }
            
            UpdateVisual();
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
            if (!controlEnabled) return;
            
            if (stateTimeLeft > 0)
            {
                stateTimeLeft -= Time.fixedDeltaTime;
            }
            
            UpdatePosition();
            UpdateInState(state);
            
        }

        private void UpdatePosition()
        {
            if (!isLocalPlayer) return;
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
                            CmdStuckAnim(lastMoveDir);
                            return;
                        }
                    }

                    prevPosition = position;
                    position += dir;
                    EnterState(PlayerState.MOVING);
                    CmdMove(prevPosition, position);
                }
            }
        }

        [Command]
        public void CmdStuckAnim(Vector2Int direction)
        {
            lastMoveDir = direction;
            EnterState(PlayerState.STUCK_ANIM);
            RpcStuckAnim(direction);
        }
        
        [ClientRpc(includeOwner = false)]
        public void RpcStuckAnim(Vector2Int direction)
        {
            lastMoveDir = direction;
            EnterState(PlayerState.STUCK_ANIM);
        }
        
        [Command]
        public void CmdMove(Vector2Int from, Vector2Int to)
        {
            Vector2Int dir = (to - from);
            dir.Clamp(Vector2Int.zero, Vector2Int.one);
            prevPosition = from;
            position = to;
            lastMoveDir = dir;
            EnterState(PlayerState.MOVING);
            RpcMove(from, to);
        }
        
        [ClientRpc(includeOwner = false)]
        public void RpcMove(Vector2Int from, Vector2Int to)
        {
            Vector2Int dir = (to - from);
            dir.Clamp(Vector2Int.zero, Vector2Int.one);
            prevPosition = from;
            position = to;
            lastMoveDir = dir;
            EnterState(PlayerState.MOVING);
        }

        [ClientRpc]
        public void RpcTeleport(Vector2Int target)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }
            
            prevPosition = target;
            position = target;
            body.position = ToWorldPos(target);
            EnterState(PlayerState.IDLE);
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

        private Vector2Int ToGridPos(Vector3 worldPos)
        {
            return ((worldPos - Vector3.one) / 2).ToVector2Int();
        }

        public void StartMap()
        {
            Debug.Log($"Start Map is called on {(isServer ? "Server" : "Client")}");

            controlEnabled = true;
            GameObject go = GameObject.Find("Spawn");
            SpawnPoints points = go.GetComponent<SpawnPoints>();
            Vector2Int pos = ToGridPos(points.spawns[(int)role].transform.position);
            
            RpcTeleport(pos);
        }
    }
}