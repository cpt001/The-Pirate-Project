//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sound zone, manages sounds for a specific region
    /// </summary>
    public class WeatherMakerSoundZoneScript : MonoBehaviour
    {
        /// <summary>Sounds to play if in the sound zone. If changing by script, ensure to destroy any sounds being replaced, and call AddSound for each new sound.</summary>
        [Tooltip("Sounds to play if in the sound zone. If changing by script, ensure to destroy " +
            "any sounds being replaced, and call AddSound for each new sound.")]
        public List<WeatherMakerSoundGroupScript> Sounds = new List<WeatherMakerSoundGroupScript>();

        private static readonly List<WeatherMakerSoundZoneScript> soundZoneStack = new List<WeatherMakerSoundZoneScript>();

        private bool addSounds = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!WeatherMakerScript.IsLocalPlayer(other.transform))
            {
                return;
            }

            // entered the trigger, can play
            StartSounds();

            // stop previous sound zones even if they were not exited
            for (int i = 0; i < soundZoneStack.Count; i++)
            {
                if (soundZoneStack[i] == null)
                {
                    soundZoneStack.RemoveAt(i--);
                }
                else if (soundZoneStack[i] == this && i == soundZoneStack.Count - 1)
                {
                    // same sound zone, already in
                    return;
                }
                else
                {
                    soundZoneStack[i].StopSounds();
                }
            }
            soundZoneStack.Add(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!WeatherMakerScript.IsLocalPlayer(other.transform))
            {
                return;
            }

            StopSounds();
            soundZoneStack.Remove(this);

            // the next item on the stack can play
            if (soundZoneStack.Count != 0)
            {
                int index = soundZoneStack.Count - 1;
                WeatherMakerSoundZoneScript script = soundZoneStack[index];
                if (script == null)
                {
                    soundZoneStack.RemoveAt(index);
                }
                else
                {
                    script.StartSounds();
                }
            }
        }

        private void LateUpdate()
        {
            // update all sounds
            for (int i = Sounds.Count - 1; i >= 0; i--)
            {
                Sounds[i].Update();
                if (Sounds[i].Sounds.Count == 0)
                {
                    Sounds[i].OnDestroy();
                    GameObject.Destroy(Sounds[i]);
                    Sounds.RemoveAt(i);
                }
            }
        }

        private void OnDisable()
        {
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.OnDisable();
            }
            soundZoneStack.Remove(this);
        }

        private void OnEnable()
        {
            if (addSounds)
            {
                addSounds = false;

                // ensure sounds
                for (int i = 0; i < Sounds.Count; i++)
                {
                    // false = cannot play until trigger is entered
                    Sounds[i] = AddSound(Sounds[i], false, false);
                }
            }

            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.OnEnable();
            }
        }

        private void OnDestroy()
        {
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                if (script != null)
                {
                    script.OnDestroy();
                    GameObject.Destroy(script);
                }
            }
        }

        /// <summary>
        /// Start all sounds
        /// </summary>
        public void StartSounds()
        {
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.CanPlay = true;
            }
        }

        /// <summary>
        /// Stop all sounds in the zone
        /// </summary>
        /// <param name="transitionDuration">Transition duration in seconds</param>
        /// <param name="destroySounds">True to destroy sounds as well</param>
        public void StopSounds(float transitionDuration = 5.0f, bool destroySounds = false)
        {
            foreach (WeatherMakerSoundGroupScript script in Sounds)
            {
                script.Stop(transitionDuration, destroySounds);
            }
        }
        
        /// <summary>
        /// Add a new sound group
        /// </summary>
        /// <param name="sound">Sound group</param>
        /// <param name="canPlay">Whether the sound group can play</param>
        /// <param name="add">True to add to the sound list</param>
        /// <returns>Sound group</returns>
        public WeatherMakerSoundGroupScript AddSound(WeatherMakerSoundGroupScript sound, bool canPlay, bool add = true)
        {
            // make a copy, if multiple zones are using the same sounds, we want each to have different state
            sound = ScriptableObject.Instantiate(sound);
            sound.Parent = gameObject;
            sound.Initialize();
            sound.CanPlay = canPlay;
            if (add)
            {
                Sounds.Add(sound);
            }
            return sound;
        }
    }
}