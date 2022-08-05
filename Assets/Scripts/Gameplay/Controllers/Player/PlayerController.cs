using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Controllers;
using Gameplay.Controllers.Player;
using Gameplay.Controllers.Player.Ability;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Util;

namespace Gameplay.Conrollers
{
    public enum PlayerRole
    {
        ICE_MAGE,
        FIRE_MAGE
    }

    public enum PlayerState
    {
        IDLE,
        MOVING,
        STUCK_ANIM,
        FALLING,
    }

    public class PlayerController : NetworkBehaviour, IPlayerData, ICanTeleport, IHeavyObject
    {
        [SyncVar] private string playerName;
        [SyncVar(hook = nameof(SetRoleHook))] private PlayerRole m_role;

        [SyncVar(hook = nameof(SetControlHook))]
        public bool controlEnabled;

        public new Camera camera;

        public PlayerConfig config;
        public Vector2Int position;

        public new SpriteRenderer renderer;
        public Transform lightTransform;
        public Light2D staffLight;
        public Inventory inventory;

        private InputAction movement;
        private InputAction firstAbility;
        private InputAction secondAbility;
        private InputAction mousePosition;

        private Rigidbody2D body;
        [HideInInspector] public new Collider2D collider;

        private Vector2Int prevPosition;
        private Vector2Int lastMoveDir;

        private Direction lastlookDir;

        private PlayerState state;
        private float stateTimeLeft;
        
        public bool isGhost;
        public float ghostTimeLeft;

        private Dictionary<BaseAbility, float> abilityCooldowns = new Dictionary<BaseAbility, float>();
        private Dictionary<ProjectileID, List<GameObject>> playerObjects = new Dictionary<ProjectileID, List<GameObject>>();

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

        public int Mass => 50;

        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            movement = model.input.actions["move"];
            firstAbility = model.input.actions["first"];
            mousePosition = model.input.actions["mouse"];

            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            inventory = GetComponent<Inventory>();

            EnterState(PlayerState.IDLE);
            UpdateVisual(Direction.BACKWARD);
            DontDestroyOnLoad(gameObject);
            camera.gameObject.SetActive(false);

            firstAbility.performed += OnCastAbility;
        }

        private void OnCastAbility(InputAction.CallbackContext obj)
        {
            if (!controlEnabled) return;
            if (!isLocalPlayer) return;

            ItemDesc itemDesc = inventory.SelectedItem();
            if (itemDesc.itemSpell != null)
            {
                Vector2Int dir = GetLookVector().ToVector2Int();

                if (itemDesc.itemSpell != null)
                {
                    CmdActivateAbility(itemDesc.itemSpell.ItemId, dir.GetDirection());
                }
            }
        }

        void SetRoleHook(PlayerRole before, PlayerRole now)
        {
            if (renderer == null)
            {
                renderer = GetComponent<SpriteRenderer>();
            }

            UpdateVisual(GetLookDirection());
        }

        private void SetControlHook(bool before, bool after)
        {
            camera.gameObject.SetActive(isLocalPlayer && after);
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

            foreach (BaseAbility ability in abilityCooldowns.Keys.ToArray())
            {
                if (abilityCooldowns[ability] > 0)
                {
                    abilityCooldowns[ability] -= Time.fixedDeltaTime;
                }
            }

            UpdatePosition();
            UpdateInState(state);
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                Direction dir = GetLookDirection();
                if (dir != lastlookDir)
                {
                    lastlookDir = dir;
                    CmdUpdateLookDirection(dir);
                }

                UpdateVisual(dir);
            }
            else
            {
                UpdateVisual(lastlookDir);
            }

            if (ghostTimeLeft > 0)
            {
                ghostTimeLeft -= Time.deltaTime;
                if (ghostTimeLeft <= 0)
                {
                    isGhost = false;
                    renderer.color = Color.white;
                }
            }
        }

        private void UpdateVisual(Direction direction)
        {
            Sprite[] sprites = role == PlayerRole.ICE_MAGE ? config.iceMageSprites : config.fireMageSprites;
            Vector3[] ligthPoses = role == PlayerRole.ICE_MAGE ? config.iceStaffLightPos : config.fireStaffLightPos;
            Color lightColor = role == PlayerRole.ICE_MAGE ? config.iceMageLightColor : config.fireMageLightColor;


            renderer.sprite = sprites[(int)direction];
            lightTransform.localPosition = ligthPoses[(int)direction];
            staffLight.color = lightColor;
        }

        [Command]
        private void CmdUpdateLookDirection(Direction direction)
        {
            RpcUpdateLookDirection(direction);
        }

        [ClientRpc(includeOwner = false)]
        private void RpcUpdateLookDirection(Direction direction)
        {
            lastlookDir = direction;
        }

        private Vector3 GetLookVector()
        {
            if (mousePosition == null) return Vector3.down;

            Vector3 mouse = mousePosition.ReadValue<Vector2>();
            mouse.z = 10;
            Vector3 worldMousePos = camera.ScreenToWorldPoint(mouse);
            worldMousePos.z = 0;
            Vector3 dir = (worldMousePos - transform.position).normalized.AxisRound();
            return dir;
        }

        private Direction GetLookDirection()
        {
            return GetLookVector().ToVector2Int().GetDirection();
        }

        private void UpdatePosition()
        {
            if (!isLocalPlayer) return;
            if (state != PlayerState.IDLE) return;

            Vector2Int dir = movement.ReadValue<Vector2>().Apply(Mathf.RoundToInt);
            LayerMask mask = isGhost ? config.ghostMask : config.wallMask;
            if (dir != Vector2Int.zero && stateTimeLeft < 0.1f)
            {
                int xAbs = Mathf.Abs(dir.x);
                int yAbs = Mathf.Abs(dir.y);
                if (xAbs + yAbs == 1)
                {
                    lastMoveDir = dir;
                    Vector2 pos = position.ToWorldPos();
                    

                    int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.9f, dir, hits, 2f, mask);
                    if (hitCount > 0)
                    {
                        bool shouldStop = false;
                        for (int i = 0; i < hitCount; i++)
                        {
                            RaycastHit2D hit = hits[i];
                            if (hit.collider != null && hit.collider != collider)
                            {
                                if (hit.collider.isTrigger)
                                {
                                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                                    if (interactable != null && interactable.FacingDirection == -dir)
                                    {
                                        interactable.Activate(this);
                                    }
                                }
                                else
                                {
                                    IMoveAble moveAble = hit.collider.GetComponent<IMoveAble>();
                                    if (moveAble != null)
                                    {
                                        moveAble.Move(lastMoveDir.GetDirection());
                                    }

                                    shouldStop = true;
                                }
                            }
                        }

                        if (shouldStop)
                        {
                            EnterState(PlayerState.STUCK_ANIM);
                            CmdStuckAnim(lastMoveDir.GetDirection());
                            return;
                        }
                    }

                    prevPosition = position;
                    position += dir;
                    EnterState(PlayerState.MOVING);
                    CmdMove(prevPosition, dir.GetDirection());
                }
            }
        }

        [Command]
        public void CmdStuckAnim(Direction direction)
        {
            lastMoveDir = direction.GetVector();
            EnterState(PlayerState.STUCK_ANIM);
            RpcStuckAnim(direction);
        }

        [ClientRpc(includeOwner = false)]
        public void RpcStuckAnim(Direction direction)
        {
            lastMoveDir = direction.GetVector();
            EnterState(PlayerState.STUCK_ANIM);
        }

        [Command]
        public void CmdMove(Vector2Int pos, Direction direction)
        {
            lastMoveDir = direction.GetVector();
            prevPosition = pos;
            position = pos + lastMoveDir;
            EnterState(PlayerState.MOVING);
            RpcMove(pos, direction);
        }

        #region Ability

        [Command]
        public void CmdActivateAbility(string abilityId, Direction direction)
        {
            if (CanCast(abilityId, out string feedback))
            {
                BaseAbility ability = model.abilities.Get(abilityId);
                bool success = ability.ActivateAbility(this, direction);

                if (success)
                {
                    StartCooldown(ability);
                }
            }
            else
            {
                RpcFeedback(feedback);
            }
        }

        [Server]
        public List<GameObject> GetActiveProjectiles(ProjectileID projectileID)
        {
            if (playerObjects.ContainsKey(projectileID))
            {
                return playerObjects[projectileID];
            }

            playerObjects.Add(projectileID, new List<GameObject>());
            return playerObjects[projectileID];
        }

        [Server]
        public void AddProjectile(ProjectileID projectileID, GameObject o)
        {
            if (playerObjects.ContainsKey(projectileID))
            {
                playerObjects[projectileID].Add(o);
            }
            else
            {
                playerObjects.Add(projectileID, new List<GameObject>());
                playerObjects[projectileID].Add(o);
            }
        }

        [Server]
        public void ReplaceObject(ProjectileID projectileID, GameObject oldObj, GameObject newObj)
        {
            RemoveObject(projectileID, oldObj);
            AddProjectile(projectileID, newObj);

            if (oldObj != null)
            {
                ISpawnable behavior = oldObj.GetComponent<ISpawnable>();
                behavior.Destroy();
            }
            else
            {
                Debug.Log("Old object is destroyed!");
            }
        }

        public void RemoveObject(ProjectileID projectileID, GameObject o)
        {
            if (playerObjects.ContainsKey(projectileID))
            {
                playerObjects[projectileID].Remove(o);
            }
        }

        public void StartCooldown(BaseAbility ability)
        {
            if (abilityCooldowns.ContainsKey(ability))
            {
                abilityCooldowns[ability] = ability.abilityCooldown;
            }
            else
            {
                abilityCooldowns.Add(ability, ability.abilityCooldown);
            }
        }

        public bool CanCast(string abilityId, out string feedback)
        {
            try
            {
                BaseAbility ability = inventory.GetItems().First(item => item.itemSpell != null && item.itemSpell.ItemId == abilityId).itemSpell;

                if (abilityCooldowns.ContainsKey(ability))
                {
                    feedback = "Ability is on cooldown!";
                    return abilityCooldowns[ability] <= 0;
                }

                feedback = "OK!";
                return true;
            }
            catch (Exception)
            {
                feedback = "You don't have such ability!";
                return false;
            }
            
        }

        [ClientRpc]
        public void RpcFeedback(string message)
        {
            if (isLocalPlayer)
            {
                Feedback(message);
            }
        }

        [Client]
        public void Feedback(string message)
        {
            //TODO nice popups
            Debug.Log(message);
        }

        #endregion

        [ClientRpc(includeOwner = false)]
        public void RpcMove(Vector2Int pos, Direction direction)
        {
            lastMoveDir = direction.GetVector();
            prevPosition = pos;
            position = pos + lastMoveDir;
            EnterState(PlayerState.MOVING);
        }

        [ClientRpc]
        public void RpcTeleport(Vector2Int target)
        {
            if (isClientOnly)
            {
                Teleport(target);
            }
        }

        [ClientRpc]
        public void RpcActivateGhostMode(float time)
        {
            ghostTimeLeft = time;
            isGhost = true;
            renderer.color = new Color(1, 1, 1, 0.5f);
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
                        body.MovePosition(Vector2.Lerp(prevPosition.ToWorldPos(), position.ToWorldPos(), t));
                    }
                    else
                    {
                        body.MovePosition(position.ToWorldPos());
                        EnterState(PlayerState.IDLE);
                    }

                    break;
                case PlayerState.STUCK_ANIM:

                    if (stateTimeLeft > 0)
                    {
                        float nt = config.stuckAnim.Evaluate(t);
                        Vector2 pos = position.ToWorldPos();
                        body.MovePosition(Vector2.Lerp(pos, pos + lastMoveDir, nt));
                    }
                    else
                    {
                        body.MovePosition(position.ToWorldPos());
                        EnterState(PlayerState.IDLE);
                    }

                    break;
                case PlayerState.FALLING:
                    break;
            }
        }

        public void StartMap()
        {
            Debug.Log($"Start Map is called on {(isServer ? "Server" : "Client")}");

            controlEnabled = true;
            GameObject go = GameObject.Find("Spawn");
            SpawnPoints points = go.GetComponent<SpawnPoints>();

            try
            {
                DungeonEntrance entrance =
                    points.spawns.First(entrance =>
                    {
                        return entrance.targetPlayer == role;
                    });

                World.World world = model.levelElement.GetWorld();

                Vector2Int pos = world.GetGridPos(entrance.startPosition.transform.position);

                Teleport(pos);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to spawn player {role}, because there is no spawn point matching!");
                return;
            }
        }

        public bool Teleport(Vector2Int position)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            World.World world = model.levelElement.GetWorld();
            prevPosition = position;
            this.position = position;
            transform.position = world.GetWorldSpacePos(position);
            EnterState(PlayerState.IDLE);
            if (isServer)
            {
                RpcTeleport(position);
            }

            return true;
        }
    }
}