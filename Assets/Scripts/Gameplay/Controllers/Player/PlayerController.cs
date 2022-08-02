using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Controllers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Gameplay.World;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

namespace Gameplay.Conrollers
{
    public enum PlayerRole
    {
        ICE_MAGE,
        FIRE_MAGE
    }

    public class PlayerController : NetworkBehaviour, IPlayerData, ICanTeleport, IHeavyObject
    {
        [SyncVar] private string playerName;
        [SyncVar(hook = nameof(SetRoleHook))] private PlayerRole m_role;

        [SyncVar] public Timeline timeline;

        [SyncVar(hook = nameof(SetControlHook))]
        public bool controlEnabled;

        public new Camera camera;

        public PlayerConfig config;
        public Vector2Int position;

        public new SpriteRenderer renderer;
        public Sprite pastSprite;
        public Sprite futureSprite;

        private InputAction movement;
        private InputAction firstAbility;
        private InputAction secondAbility;
        private Rigidbody2D body;

        private Vector2Int prevPosition;
        private Vector2Int lastMoveDir;

        private PlayerState state;
        private float stateTimeLeft;

        private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();
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
            secondAbility = model.input.actions["second"];

            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();

            EnterState(PlayerState.IDLE);
            UpdateVisual();
            DontDestroyOnLoad(gameObject);
            camera.gameObject.SetActive(false);

            firstAbility.performed += OnCastFirstAbility;
            secondAbility.performed += OnCastSecondAbility;
        }

        private void OnCastSecondAbility(InputAction.CallbackContext obj)
        {
            if (!controlEnabled) return;
            if (!isLocalPlayer) return;

            AbilityStack abilityStack = model.abilities.GetAbilities(role);
            if (abilityStack.abilities.Count > 0)
            {
                Ability ability = abilityStack.abilities[0];
                CmdActivateAbility(ability);
            }
        }

        private void OnCastFirstAbility(InputAction.CallbackContext obj)
        {
            if (!controlEnabled) return;
            if (!isLocalPlayer) return;

            AbilityStack abilityStack = model.abilities.GetAbilities(role);
            if (abilityStack.abilities.Count > 1)
            {
                Ability ability = abilityStack.abilities[1];
                CmdActivateAbility(ability);
            }
        }

        private void UpdateVisual()
        {
            renderer.sprite = role == PlayerRole.ICE_MAGE ? pastSprite : futureSprite;
        }

        void SetRoleHook(PlayerRole before, PlayerRole now)
        {
            if (renderer == null)
            {
                renderer = GetComponent<SpriteRenderer>();
            }

            UpdateVisual();
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

            foreach (Ability ability in abilityCooldowns.Keys.ToArray())
            {
                if (abilityCooldowns[ability] > 0)
                {
                    abilityCooldowns[ability] -= Time.fixedDeltaTime;
                }
            }

            UpdatePosition();
            UpdateInState(state);
        }

        private void UpdatePosition()
        {
            if (!isLocalPlayer) return;
            if (state != PlayerState.IDLE) return;

            Vector2Int dir = movement.ReadValue<Vector2>().Apply(Mathf.RoundToInt);
            if (dir != Vector2Int.zero && stateTimeLeft < 0.1f)
            {
                int xAbs = Mathf.Abs(dir.x);
                int yAbs = Mathf.Abs(dir.y);
                if (xAbs + yAbs == 1)
                {
                    lastMoveDir = dir;
                    Vector2 pos = position.ToWorldPos(timeline);

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
        public void CmdActivateAbility(Ability ability)
        {
            if (CanCast(ability, out string feedback))
            {
                bool success = false;
                if (ability.abilityType == AbilityType.SHOOT)
                {
                    success = ShootProjectile(ability.projectileID);
                }
                else
                {
                    Debug.Log("Noting to see here!");
                }

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

        public void StartCooldown(Ability ability)
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

        public bool CanCast(Ability ability, out string feedback)
        {
            AbilityStack abilityStack = model.abilities.GetAbilities(role);
            if (abilityStack.abilities.Contains(ability))
            {
                if (abilityCooldowns.ContainsKey(ability))
                {
                    feedback = "Ability is on cooldown!";
                    return abilityCooldowns[ability] <= 0;
                }

                feedback = "OK!";
                return true;
            }

            feedback = "You don't have such ability!";
            return false;
        }

        [Server]
        private List<GameObject> GetActiveProjectiles(ProjectileID projectileID)
        {
            if (playerObjects.ContainsKey(projectileID))
            {
                return playerObjects[projectileID];
            }

            playerObjects.Add(projectileID, new List<GameObject>());
            return playerObjects[projectileID];
        }

        [Server]
        private void AddProjectile(ProjectileID projectileID, GameObject o)
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
        private void ReplaceObject(ProjectileID projectileID, GameObject oldObj, GameObject newObj)
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

        [Server]
        private bool ShootProjectile(ProjectileID projectileID)
        {
            try
            {
                Projectile projectile = model.projectiles.Get(projectileID);
                List<GameObject> active = GetActiveProjectiles(projectileID);
                Vector2Int pos = position + lastMoveDir;

                int hitCount = Physics2D.CircleCastNonAlloc(position, 0.3f, lastMoveDir, hits, 0.5f, config.wallMask);

                bool foundAWall = false;
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        RaycastHit2D hit = hits[i];
                        if (hit.collider == null || hit.collider.isTrigger) continue;

                        foundAWall = true;
                        break;
                    }
                }


                if (foundAWall)
                {
                    RpcFeedback("Can't use this ability facing a wall");
                    return false;
                }

                if (active.Count < projectile.maxActive)
                {
                    SpawnWithLink(projectile.itemId, timeline, pos, lastMoveDir.GetDirection(), projectile.prefab);
                    return true;
                }

                if (projectile.canReplace)
                {
                    GameObject oldObj = active.Last();
                    SpawnWithReplace(projectile.itemId, timeline, pos, lastMoveDir.GetDirection(), oldObj, projectile.prefab);
                    return true;
                }

                RpcFeedback("Can't spawn new projectile! You have too many!");
                return false;
            }
            catch (IndexOutOfRangeException e)
            {
                RpcFeedback($"Internal Server Error: No projectile with ID {projectileID}");
            }
            return false;
        }


        [Server]
        public GameObject Spawn(Timeline s_timeline, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Instantiate(prefab);
            NetworkServer.Spawn(projectileObj);
            ISpawnable behavior = projectileObj.GetComponent<ISpawnable>();
            behavior.Spawn(this, s_timeline, pos, direction);

            return projectileObj;
        }

        [Server]
        public void SpawnWithLink(ProjectileID projectile, Timeline s_timeline, Vector2Int pos, Direction direction, GameObject prefab)
        {
            GameObject projectileObj = Spawn(s_timeline, pos, direction, prefab);
            AddProjectile(projectile, projectileObj);
        }

        [Server]
        public void SpawnWithReplace(ProjectileID projectileID, Timeline s_timeline, Vector2Int pos, Direction direction, GameObject oldGO, GameObject prefab)
        {
            GameObject projectileObj = Spawn(s_timeline, pos, direction, prefab);
            ReplaceObject(projectileID, oldGO, projectileObj);
        }

        [ClientRpc]
        public void RpcFeedback(string message)
        {
            if (isLocalPlayer)
            {
                //TODO nice popups
                Debug.Log(message);
            }
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
        public void RpcTeleport(Timeline timeline, Vector2Int target)
        {
            if (isClientOnly)
            {
                Teleport(timeline, target);
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
                        body.MovePosition(Vector2.Lerp(prevPosition.ToWorldPos(timeline), position.ToWorldPos(timeline), t));
                    }
                    else
                    {
                        body.MovePosition(position.ToWorldPos(timeline));
                        EnterState(PlayerState.IDLE);
                    }

                    break;
                case PlayerState.STUCK_ANIM:

                    if (stateTimeLeft > 0)
                    {
                        float nt = config.stuckAnim.Evaluate(t);
                        Vector2 pos = position.ToWorldPos(timeline);
                        body.MovePosition(Vector2.Lerp(pos, pos + lastMoveDir, nt));
                    }
                    else
                    {
                        body.MovePosition(position.ToWorldPos(timeline));
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
            timeline = (Timeline)(int)role;
            
            World.World world = model.spacetime.GetWorld(timeline);
            Vector2Int pos = world.GetGridPos(points.spawns[(int)role].transform.position);
            
            Debug.Log($"Timeline: {timeline}, grid pos: {pos}");

            Teleport(timeline, pos);
        }

        public bool Teleport(Timeline timeline, Vector2Int position)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            World.World world = model.spacetime.GetWorld(timeline);

            this.timeline = timeline;
            prevPosition = position;
            this.position = position;
            transform.position = world.GetWorldSpacePos(position);
            EnterState(PlayerState.IDLE);
            if (isServer)
            {
                RpcTeleport(timeline, position);
            }

            return true;
        }
    }
}