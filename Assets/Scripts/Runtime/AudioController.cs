using UnityEngine;

namespace BirdGame.Runtime
{
    public sealed class AudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip flapChirp;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] private float chirpVolume = 0.52f;

        private AudioSource musicSource;
        private AudioSource chirpSource;

        public void PlayFlapChirp()
        {
            if (chirpSource != null && flapChirp != null)
            {
                chirpSource.PlayOneShot(flapChirp, chirpVolume);
            }
        }

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            chirpSource = gameObject.AddComponent<AudioSource>();
            chirpSource.playOnAwake = false;

            if (backgroundMusic != null)
            {
                musicSource.Play();
            }
        }
    }
}
