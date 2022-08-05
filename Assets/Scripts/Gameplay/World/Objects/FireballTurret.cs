using System;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.World
{
    public enum FireMode
    {
        SINGLE,
        CONTINOUS
    }

    public class FireballTurret : NetworkBehaviour, ILinked
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
        [FormerlySerializedAs("fireRate")] 
        public float fireDelay;
        public float initialOffset;
        
        public FireMode fireMode;
        public bool initialState;

        private bool state;
        private float timeElapsed;
        private GameModel model;

        public WorldElement element { get; set; }

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            timeElapsed = initialOffset;
            SetState(initialState);
        }

        private void OnDrawGizmos()
        {
            topRenderer.enabled = false;
            leftRenderer.enabled = false;
            rightRenderer.enabled = false;
            bottomRenderer.enabled = false;

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

        private void SetState(bool state)
        {
            if (this.state != state)
            {
                this.state = state;
                timeElapsed = initialOffset;

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
                if (timeElapsed > fireDelay)
                {
                    World world = model.levelElement.GetWorld();
                    Vector2Int pos = world.GetGridPos(transform.position);
                    pos += fireDirection.GetVector();
                    Projectile projectile = model.projectiles.Get(fireball);

                    SpawnController.Spawn(pos, fireDirection, projectile.prefab);
                    timeElapsed = 0;
                    if (fireMode == FireMode.SINGLE)
                    {
                        SetState(false);
                        RpcSetState(false);
                    }
                }
            }
        }

        [ClientRpc]
        public void RpcSetState(bool value)
        {
            SetState(value);
        }

        public void ReciveStateChange(bool value)
        {
            SetState(value);
        }
    }
}