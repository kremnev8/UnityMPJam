using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.World
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioSource : MonoBehaviour
    {
        private AudioSource source;
        public AudioClip[] clips;

        public bool selectOnAwake;
        public bool loop;
        public int selectedClip;
        
        
        public float playbackDelay = 1f;
        private float timeElapsed;
        
        private void Awake()
        {
            source = GetComponent<AudioSource>();

            if (selectOnAwake)
            {
                selectedClip = Random.Range(0, clips.Length);
            }
        }

        public void Play()
        {
            if (timeElapsed > 0.01) return;

            if (!selectOnAwake)
            {
                selectedClip = Random.Range(0, clips.Length);
            }

            if (loop)
            {
                source.clip = clips[selectedClip];
            }
            else
            {
                source.PlayOneShot(clips[selectedClip], 0.9F);
            }
            source.loop = loop;
            
            source.Play();
            timeElapsed = playbackDelay;
        }

        public void Stop()
        {
            source.Stop();
        }

        private void Update()
        {
            if (timeElapsed >= 0)
            {
                timeElapsed -= Time.deltaTime;
            }
        }
    }
}
