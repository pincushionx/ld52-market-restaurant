using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class SoundController : MonoBehaviour
    {
        private Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();

        private void Awake()
        {
            sounds.Clear();
            AudioSource[] sources = GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in sources)
            {
                sounds.Add(source.gameObject.name, source);
            }
        }

        public void PlaySound(string soundName)
        {
            if (sounds.ContainsKey(soundName))
            {
                AudioSource sound = sounds[soundName];
                if (!sound.isPlaying)
                {
                    StopSound(soundName);
                    sound.Play();
                }
            }
        }

        public void StopSound(string soundName)
        {
            if (sounds.ContainsKey(soundName))
            {
                AudioSource sound = sounds[soundName];
                if (sound.isPlaying)
                {
                    sound.Stop();
                }
            }
        }

        public AudioSource GetAudioSource(string soundName)
        {
            if (sounds.ContainsKey(soundName))
            {
                return sounds[soundName];
            }
            return null;
        }
    }
}