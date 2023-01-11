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

using System;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping
    /// </summary>
    public class WeatherMakerLoopingAudioSource
    {
        /// <summary>
        /// The audio source that is looping
        /// </summary>
        public AudioSource AudioSource { get; private set; }

        /// <summary>
        /// The target volume
        /// </summary>
        public float TargetVolume { get; private set; }

        /// <summary>
        /// Is this sound stopping?
        /// </summary>
        public bool Stopping { get; private set; }

        /// <summary>
        /// Whether the AudioSource will be destroyed upon stop
        /// </summary>
        public bool DestroyOnStopped { get; private set; }

        /// <summary>
        /// Volume modifier, clamped to 0 - 1
        /// </summary>
        public float VolumeModifier
        {
            get { return volumeModifier; }
            set { volumeModifier = Mathf.Clamp(value, 0.0f, 1.0f); }
        }

        /// <summary>
        /// Secondary volume modifier, clamped to 0 - 1
        /// </summary>
        public float SecondaryVolumeModifier
        {
            get { return secondaryVolumeModifier; }
            set { secondaryVolumeModifier = Mathf.Clamp(value, 0.0f, 1.0f); }
        }

        private float startVolume;
        private float startFade;
        private float stopFade;
        private float currentFade;
        private float timestamp;
        private float volumeModifier = 1.0f;
        private float secondaryVolumeModifier = 1.0f;
        private bool looping;
        private bool wasPlaying;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audioSource">Audio source, will be looped automatically</param>
        public WeatherMakerLoopingAudioSource(AudioSource audioSource) : this(audioSource, 2.0f, 2.0f)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audioSource">Audio source, will be looped automatically</param>
        /// <param name="startFade">Start fade - seconds to reach peak sound</param>
        /// <param name="stopFade">Stop fade - seconds to fade sound back to 0 volume when stopped</param>
        /// <param name="looping">Whether to loop the audio source - default is true</param>
        public WeatherMakerLoopingAudioSource(AudioSource audioSource, float startFade, float stopFade, bool looping = true)
        {
            AudioSource = audioSource;
            this.looping = looping;
            if (audioSource != null)
            {
                AudioSource.loop = looping;
                AudioSource.volume = 0.0f;
                AudioSource.Stop();
                SetFade(startFade, stopFade);
            }
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        public void Play()
        {
            Play(1.0f);
        }

        /// <summary>
        /// Play this looping audio source
        /// </summary>
        /// <param name="targetVolume">Max volume</param>
        public void Play(float targetVolume)
        {
            if (AudioSource != null)
            {
                AudioSource.volume = startVolume = (AudioSource.isPlaying ? AudioSource.volume : 0.0f);
                AudioSource.loop = looping;
                currentFade = startFade;
                TargetVolume = targetVolume;
                Stopping = false;
                timestamp = 0.0f;
                if (!AudioSource.isPlaying && AudioSource.gameObject.activeInHierarchy)
                {
                    AudioSource.Play();
                }
            }
        }

        /// <summary>
        /// Set new fade in / fade out parameters
        /// </summary>
        /// <param name="startFade">Start fade - seconds to reach peak sound</param>
        /// <param name="stopFade">Stop fade - seconds to fade sound back to 0 volume when stopped</param>
        public void SetFade(float startFade, float stopFade)
        {
            this.startFade = currentFade = startFade;
            this.stopFade = stopFade;
        }

        /// <summary>
        /// Stop this looping audio source. The sound will fade out smoothly.
        /// </summary>
        /// <param name="fadeOutSeconds">Number of seconds to fade out, 0 for default</param>
        /// <param name="destroyOnStopped">Whether to destroy the audio source upon stop</param>
        public void Stop(float fadeOutSeconds = 0.0f, bool destroyOnStopped = false)
        {
            if (AudioSource != null && AudioSource.isPlaying && !Stopping)
            {
                startVolume = AudioSource.volume;
                TargetVolume = 0.0f;
                currentFade = (fadeOutSeconds <= 0.0f ? stopFade : fadeOutSeconds);
                Stopping = true;
                timestamp = 0.0f;
            }
            DestroyOnStopped |= destroyOnStopped;
        }

        /// <summary>
        /// Pause the audio
        /// </summary>
        public void Pause()
        {
            if (AudioSource != null)
            {
                if (wasPlaying)
                {
                    AudioSource.Pause();
                }
                else
                {
                    AudioSource.Stop();
                }
            }
        }

        /// <summary>
        /// Resume from a pause
        /// </summary>
        public void Resume()
        {
            if (wasPlaying && !Stopping)
            {
                AudioSource.UnPause();
                AudioSource.Play();
            }
        }

        /// <summary>
        /// Update this looping audio source
        /// </summary>
        /// <returns>True if finished playing, false otherwise</returns>
        public bool Update()
        {
            if (AudioSource == null)
            {
                return true;
            }
            else if (AudioSource != null && AudioSource.isPlaying)
            {
                wasPlaying = true;

                // check if we need to stop because the volume has reached 0
                timestamp += Time.deltaTime;
                AudioSource.volume = Mathf.Lerp(startVolume, TargetVolume * volumeModifier * secondaryVolumeModifier, timestamp / currentFade);
                if (AudioSource.volume == 0.0f && Stopping)
                {
                    AudioSource.Stop();
                    Stopping = false;
                    if (DestroyOnStopped && AudioSource != null)
                    {
                        GameObject.Destroy(AudioSource);
                    }
                    // done playing
                    return true;
                }
                else
                {
                    // not done playing yet
                    return false;
                }
            }
            else
            {
                wasPlaying = false;
                Stopping = false;
                if (DestroyOnStopped && AudioSource != null)
                {
                    GameObject.Destroy(AudioSource);
                }
            }

            // done playing
            return true;
        }
    }
}
