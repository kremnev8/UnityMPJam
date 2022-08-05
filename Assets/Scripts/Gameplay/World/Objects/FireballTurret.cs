using System;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public enum FireMode
    {
        SINGLE,
        CONTINOUS
    }
    public class FireballTurret : NetworkBehaviour, ITimeLinked
    {
        public SpriteRenderer topRenderer;
        public SpriteRenderer leftRenderer;
        public SpriteRenderer rightRenderer;
        public SpriteRenderer bottomRenderer;

        public Sprite topEnabled;
        public Sprite topDisabled;
        
        public Sprite bottomEnabled;
        public Sprite bottomDisabled;
        
        public Sprite leftEnabled;
        public Sprite leftDisabled;

        public ProjectileID fireball;

        public Direction fireDirection;
        public float fireRate;
        public FireMode fireMode;
        public bool initialState;
        
        private bool state;
        private float timeElapsed;
        private GameModel model;

        public SpaceTimeObject timeObject { get; set; }

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            SetState(initialState, true);
        }

        private void SetState(bool state, bool isPermanent)
        {
            if (this.state != state)
            {
                this.state = state;
                timeElapsed = 0;

                SetVisual(state);
            }
        }

        private void SetVisual(bool state)
        {
            topRenderer.sprite = state ? topEnabled : topDisabled;
            leftRenderer.sprite = state ? leftEnabled : leftDisabled;
            rightRenderer.sprite = state ? leftEnabled : leftDisabled;
            bottomRenderer.sprite = state ? bottomEnabled : bottomDisabled;
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                if (!state) return;
                
                timeElapsed += Time.fixedDeltaTime;
                if (timeElapsed > fireRate)
                {
                    World world = model.spacetime.GetWorld(timeObject.timeline);
                    Vector2Int pos = world.GetGridPos(transform.position);
                    pos += fireDirection.GetVector();
                    Projectile projectile = model.projectiles.Get(fireball);

                    SpawnController.Spawn(timeObject.timeline, pos, fireDirection, projectile.prefab);
                    timeElapsed = 0;
                    if (fireMode == FireMode.SINGLE)
                    {
                        SetState(false, true);
                        RpcSetState(false);
                    }
                }
            }
        }

        [ClientRpc]
        public void RpcSetState(bool value)
        {
            SetState(value, true);
        }

        public void Configure(ObjectState state)
        {
            topRenderer.enabled = false;
            leftRenderer.enabled = false;
            rightRenderer.enabled = false;
            bottomRenderer.enabled = false;

            if (state == ObjectState.EXISTS)
            {
                switch (fireDirection)
                {
                    case Direction.FORWARD:
                        topRenderer.enabled = true;
                        break;
                    case Direction.RIGHT:
                        rightRenderer.enabled = true;
                        break;
                    case Direction.BACKWARD:
                        bottomRenderer.enabled = true;
                        break;
                    case Direction.LEFT:
                        leftRenderer.enabled = true;
                        break;
                }
            }
        }

        public void ReciveTimeEvent(int[] args)
        {
            SetState(args[0] == 1, true);
        }

        public void ReciveStateChange(bool value, bool isPermanent)
        {
            SetState(value, isPermanent);
        }
    }
}