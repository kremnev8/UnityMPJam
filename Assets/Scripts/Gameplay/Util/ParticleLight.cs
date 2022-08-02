using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.Util
{
    public class ParticleLight : MonoBehaviour
    {
        private new ParticleSystem particleSystem;
        public new Light2D light;

        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        public void Play()
        {
            light.enabled = true;
            particleSystem.Play();
        }

        private void OnParticleSystemStopped()
        {
            light.enabled = false;
        }
    }
}