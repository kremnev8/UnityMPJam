using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Controllers.Player;
using Gameplay.Controllers.Player.Ability;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.UI;
using Gameplay.Util;
using Gameplay.World;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Util;
using Random = UnityEngine.Random;

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
        DASHING,
        DEAD
    }

    public class PlayerController : NetworkBehaviour, IPlayerData, IHeavyObject
    {
        [SyncVar] private string playerName;
        [SyncVar(hook = nameof(SetRoleHook))] private PlayerRole m_role;

        [SyncVar(hook = nameof(SetControlHook))]
        public bool controlEnabled;

        public new Camera camera;

        public PlayerConfig config;
        public Vector2Int position;

        public new SpriteRenderer renderer;
        public SpriteRenderer eyesRenderer;
        public Transform eyesTransform;
        public Transform lightTransform;
        public Light2D staffLight;
        public Inventory inventory;
        public Health health;
        public FeedbackUI feedbackUI;

        public GameObject fadeObj;

        public RandomAudioSource footstepSource;
        public RandomAudioSource errorSource;

        public Animator animator;

        public bool isGhost;
        public float ghostTimeLeft;
        public bool allowSwap;

        private PlayerInput input;
        private InputAction movement;
        private InputAction firstAbility;
        private InputAction secondAbility;
        private InputAction mousePosition;
        private InputAction controllerRight;

        private Rigidbody2D body;
        [HideInInspector] public new Collider2D collider;

        private Vector2Int prevPosition;
        private Vector2Int lastMoveDir;

        private Direction lastlookDir;

        private PlayerState state;
        private float stateTimeLeft;

        private int animationTime;
        private int currentFrame;

        public int eyesAnimTime;
        public int currentEyesFrameTime;
        public int eyesFrame;

        public int footstepsTime;
        public int currentMaxFootstepsTime;

        private Dictionary<BaseAbility, float> abilityCooldowns = new Dictionary<BaseAbility, float>();
        private Dictionary<ProjectileID, List<GameObject>> playerObjects = new Dictionary<ProjectileID, List<GameObject>>();

        private GameModel model;

        private static RaycastHit2D[] hits = new RaycastHit2D[6];
        private static readonly int fadeIn = Animator.StringToHash("FadeIn");
        private static readonly int fadeOut = Animator.StringToHash("FadeOut");
        public static bool ignoreInventoryEvent;


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

        public int Mass => 25;

        #region Init

        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            input = model.input;
            movement = input.actions["move"];
            firstAbility = input.actions["first"];
            mousePosition = input.actions["mouse"];
            controllerRight = input.actions["controllerDir"];

            body = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            inventory = GetComponent<Inventory>();
            health = GetComponent<Health>();

            EnterState(PlayerState.IDLE);
            UpdateVisual(Direction.BACKWARD);
            DontDestroyOnLoad(gameObject);
            camera.gameObject.SetActive(false);

            firstAbility.performed += OnCastAbility;
            if (isServer)
            {
                model.globalInventory.serverInventoryChanged += OnServerItemAdded;
                inventory.serverInventoryChanged += OnServerPlayerItemAdded;
            }
        }

        private void OnServerItemAdded()
        {
            int extraInventory = model.globalInventory.GetItems().Select(desc => desc.extraPlayerInventory).Sum();
            inventory.InventoryCap = extraInventory + 1;

            if (ignoreInventoryEvent) return;

            model.saveGame.current.globalInventory = model.globalInventory.GetItems().Select(desc => desc.itemId).ToList();
            model.saveGame.Save();
        }

        private void OnServerPlayerItemAdded()
        {
            if (ignoreInventoryEvent) return;

            if (role == PlayerRole.ICE_MAGE)
            {
                model.saveGame.current.icePlayerInventory = inventory.GetItems().Select(desc => desc.itemId).ToList();
            }
            else
            {
                model.saveGame.current.firePlayerInventory = inventory.GetItems().Select(desc => desc.itemId).ToList();
            }

            model.saveGame.Save();
        }

        public void StartMap()
        {
            ClearPlayerObjects();
            SpawnPlayer();

            List<string> items;
            ignoreInventoryEvent = true;

            if (isLocalPlayer && isServer)
            {
                items = model.saveGame.current.globalInventory;
                if (items != null && items.Count > 0)
                {
                    model.globalInventory.Clear();
                    foreach (string item in items)
                    {
                        model.globalInventory.AddItem(item);
                    }
                }
            }

            int extraInventory = model.globalInventory.GetItems().Select(desc => desc.extraPlayerInventory).Sum();
            inventory.InventoryCap = extraInventory + 1;

            items = role == PlayerRole.ICE_MAGE ? model.saveGame.current.icePlayerInventory : model.saveGame.current.firePlayerInventory;
            if (items != null)
            {
                inventory.Clear();
                foreach (string item in items)
                {
                    inventory.AddItem(item);
                }
            }

            ignoreInventoryEvent = false;
        }

        private void SpawnPlayer()
        {
            controlEnabled = true;
            health.Increment(10);

            GameObject go = GameObject.Find("Spawn");
            SpawnPoints points = go.GetComponent<SpawnPoints>();

            try
            {
                DungeonEntrance entrance = points.spawns.First(entrance => entrance.targetPlayer == role);
                World.World world = model.levelElement.GetWorld();
                Vector2Int pos = world.GetGridPos(entrance.startPosition.transform.position);

                Teleport(pos);
            }
            catch (Exception)
            {
                Debug.Log($"Failed to spawn player {role}, because there is no spawn point matching!");
            }
        }

        #endregion

        #region Hooks

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

        #endregion

        #region State

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
                case PlayerState.DASHING:
                    return config.dashTime;
                case PlayerState.DEAD:
                    return config.respawnTime;
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

            currentFrame = 0;
            animationTime = 0;
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

        public void UpdateInState(PlayerState state)
        {
            float stateTime = GetTimeIn(state);
            float t = (stateTime - stateTimeLeft) / stateTimeLeft;

            switch (state)
            {
                case PlayerState.IDLE:
                    break;
                case PlayerState.MOVING:
                case PlayerState.DASHING:

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
                case PlayerState.DEAD:
                    if (isServer)
                    {
                        //TODO death can be permanent if the player has some lag
                        if (stateTimeLeft < 0.1f)
                        {
                            SpawnPlayer();
                            RpcRespawned();
                        }
                    }

                    break;
            }
        }

        [Server]
        public void OnDead()
        {
            if (state != PlayerState.DEAD)
            {
                EnterState(PlayerState.DEAD);
                RpcDead();
            }
        }

        [ClientRpc]
        private void RpcDead()
        {
            fadeObj.SetActive(true);
            animator.SetTrigger(fadeIn);
            animator.ResetTrigger(fadeOut);
            EnterState(PlayerState.DEAD);
        }

        [ClientRpc]
        private void RpcRespawned()
        {
            animator.SetTrigger(fadeOut);
            Invoke(nameof(ForceHideFade), 1);
        }

        private void ForceHideFade()
        {
            fadeObj.SetActive(false);
        }

        #endregion

        #region Animation

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
                    gameObject.layer = LayerMask.NameToLayer("Player");
                    isGhost = false;
                    renderer.color = Color.white;
                }
            }
        }

        private void UpdateVisual(Direction direction)
        {
            Color lightColor = role == PlayerRole.ICE_MAGE ? config.iceMageLightColor : config.fireMageLightColor;
            SimpleAnim[] eyes = role == PlayerRole.ICE_MAGE ? config.iceMageEyes : config.fireMageEyes;
            PlayerAnim[] sprites;
            animationTime++;
            eyesAnimTime++;
            footstepsTime++;
            Direction displayDir = direction;

            if (state == PlayerState.IDLE)
            {
                sprites = role == PlayerRole.ICE_MAGE ? config.iceMageIdle : config.fireMageIdle;
                if (animationTime >= config.idleFrameTime)
                {
                    animationTime = 0;
                    currentFrame++;
                }

                eyesRenderer.enabled = true;
            }
            else if (state == PlayerState.DEAD)
            {
                PlayerAnim sprite = role == PlayerRole.ICE_MAGE ? config.iceMageDeath : config.fireMageDeath;
                if (animationTime >= config.deathFrameTime)
                {
                    animationTime = 0;
                    currentFrame++;
                }

                if (currentFrame < sprite.frames.Length)
                {
                    renderer.sprite = sprite.frames[currentFrame];
                    lightTransform.localPosition = sprite.staffLight[currentFrame];
                }

                eyesRenderer.enabled = false;
                return;
            }
            else
            {
                sprites = role == PlayerRole.ICE_MAGE ? config.iceMageWalk : config.fireMageWalk;
                if (animationTime >= config.walkFrameTime)
                {
                    animationTime = 0;
                    currentFrame++;
                }

                displayDir = lastMoveDir.GetDirection();
                eyesRenderer.enabled = true;
            }

            if (state == PlayerState.MOVING)
            {
                if (footstepsTime > currentMaxFootstepsTime)
                {
                    footstepSource.Play();
                    footstepsTime = 0;
                    currentMaxFootstepsTime = Random.Range(config.footstepMinTime, config.footstepMaxTime);
                }
            }

            if (eyesAnimTime >= currentEyesFrameTime)
            {
                eyesAnimTime = 0;
                eyesFrame++;
                if (eyesFrame >= eyes[(int)displayDir].frames.Length)
                {
                    eyesFrame = 0;
                }

                int holdTime = Random.Range(config.eyeHoldMinTime, config.eyeHoldMaxTime);
                currentEyesFrameTime = eyesFrame == 0 ? holdTime : config.eyeBlinkTime;
            }

            if (currentFrame >= sprites[(int)displayDir].frames.Length)
            {
                currentFrame = 0;
            }

            renderer.sprite = sprites[(int)displayDir].frames[currentFrame];
            lightTransform.localPosition = sprites[(int)displayDir].staffLight[currentFrame];

            eyesRenderer.sprite = eyes[(int)displayDir].frames[eyesFrame];
            eyesTransform.localPosition = sprites[(int)displayDir].GetEyePos(currentFrame);
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

        #endregion

        #region Ability

        private void OnCastAbility(InputAction.CallbackContext obj)
        {
            if (!controlEnabled) return;
            if (!isLocalPlayer) return;

            try
            {
                ItemDesc itemDesc = inventory.SelectedItem();
                if (itemDesc != null && itemDesc.itemSpell != null)
                {
                    Vector2Int dir = GetLookVector().ToVector2Int();

                    if (itemDesc.itemSpell != null)
                    {
                        CmdActivateAbility(itemDesc.itemSpell.ItemId, dir.GetDirection());
                    }
                }
                else
                {
                    Feedback("You don't have any spell selected!", false);
                }
            }
            catch (Exception e)
            {
                Feedback("Internal Error.", false);
                Debug.Log(e);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdTransferItem(bool toGlobal, string itemId)
        {
            if (allowSwap)
            {
                if (toGlobal)
                {
                    model.globalInventory.Transfer(inventory, itemId);
                }
                else
                {
                    inventory.Transfer(model.globalInventory, itemId);
                }
            }
            else
            {
                RpcFeedback("Can't swap abilities here!", false);
            }
        }

        [Command]
        public void CmdActivateAbility(string abilityId, Direction direction)
        {
            if (CanCast(abilityId, out string feedback, out bool error))
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
                RpcFeedback(feedback, error);
            }
        }

        [Server]
        private void ClearPlayerObjects()
        {
            foreach (var pair in playerObjects)
            {
                pair.Value.Clear();
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

        public bool CanCast(string abilityId, out string feedback, out bool error)
        {
            try
            {
                BaseAbility ability = inventory.GetItems().First(item => item.itemSpell != null && item.itemSpell.ItemId == abilityId).itemSpell;

                if (abilityCooldowns.ContainsKey(ability))
                {
                    error = false;
                    feedback = "Ability is on cooldown!";
                    return abilityCooldowns[ability] <= 0;
                }

                error = true;
                feedback = "OK!";
                return true;
            }
            catch (Exception e)
            {
                error = false;
                feedback = "You don't have such ability!";
                Debug.Log(e);
                return false;
            }
        }

        [ClientRpc]
        public void RpcFeedback(string message, bool success)
        {
            if (isLocalPlayer)
            {
                Feedback(message, success);
            }
        }

        [Client]
        public void Feedback(string message, bool success)
        {
            feedbackUI.SetFeedback(message);
            if (!success)
            {
                errorSource.Play();
            }
        }

        [ClientRpc]
        public void RpcActivateGhostMode(float time)
        {
            gameObject.layer = LayerMask.NameToLayer("Ghost");
            ghostTimeLeft = time;
            isGhost = true;
            renderer.color = new Color(1, 1, 1, 0.5f);
        }

        #endregion

        #region Movement

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


                    int hitCount = Physics2D.CircleCastNonAlloc(pos, 0.8f, dir, hits, 1.5f, mask);
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
                                    if (interactable != null && (interactable.FacingDirection == -dir || !interactable.checkFacing))
                                    {
                                        interactable.Activate(this);
                                    }
                                }
                                else
                                {
                                    IMoveAble moveAble = hit.collider.GetComponent<IMoveAble>();
                                    if (moveAble != null)
                                    {
                                        moveAble.MoveClient(lastMoveDir.GetDirection());
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

                    Direction direction = dir.GetDirection();
                    prevPosition = position;
                    position += dir;
                    lastlookDir = direction;
                    EnterState(PlayerState.MOVING);
                    CmdMove(prevPosition, direction);
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
            lastlookDir = direction;
            prevPosition = pos;
            position = pos + lastMoveDir;
            EnterState(PlayerState.MOVING);
            RpcMove(pos, direction);
        }

        [ClientRpc(includeOwner = false)]
        public void RpcMove(Vector2Int pos, Direction direction)
        {
            lastMoveDir = direction.GetVector();
            lastlookDir = direction;
            prevPosition = pos;
            position = pos + lastMoveDir;
            EnterState(PlayerState.MOVING);
        }

        public void RpcDash(Vector2Int pos, Direction direction, int distance)
        {
            lastMoveDir = direction.GetVector();
            prevPosition = pos;
            position = pos + lastMoveDir * distance;
            EnterState(PlayerState.DASHING);
        }

        [ClientRpc]
        public void RpcTeleport(Vector2Int target)
        {
            if (isClientOnly)
            {
                Teleport(target);
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

        #endregion

        #region Utils

        private Vector3 GetLookVector()
        {
            if (mousePosition == null) return Vector3.down;

            if (input.currentControlScheme.Equals("Controller"))
            {
                Vector2 direction = controllerRight.ReadValue<Vector2>();
                if (direction.Equals(Vector2.zero))
                {
                    return lastlookDir.GetVector().ToVector3();
                }

                return direction.normalized.AxisRound();
            }
            
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

        #endregion
    }
}