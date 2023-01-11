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
    /// Player sound manager interface
    /// </summary>
    public interface IPlayerSoundManager : IWeatherMakerManager
    {
        // TODO: Expose things like sound list, etc.
    }

    /// <summary>
    /// Player sound manager default implementation
    /// </summary>
    public class WeatherMakerPlayerSoundManagerScript : MonoBehaviour, IPlayerSoundManager
    {
        /// <summary>How fast to fade out player sounds and apply new sounds when the weather changes. Set to 0 to not do this at all and leave the player sounds as they are. Use of this property requires a sound zone be added underneath the player object.</summary>
        [Tooltip("How fast to fade out player sounds and apply new sounds when the weather changes. " + 
            "Set to 0 to not do this at all and leave the player sounds as they are. Use of this " + 
            "property requires a sound zone be added underneath the player object.")]
        [Range(0.0f, 1.0f)]
        public float WeatherProfileSoundFadeOutMultiplier = 0.25f;

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
        }

        private void OnDisable()
        {
            
        }

        private void LateUpdate()
        {
        }

        private void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// Sound manager weather profile change handler
        /// </summary>
        /// <param name="oldProfile">Old profile</param>
        /// <param name="newProfile">New profile</param>
        /// <param name="transitionDelay">Transition delay</param>
        /// <param name="transitionDuration">Transition duration</param>
        public void WeatherProfileChanged(WeatherMakerProfileScript oldProfile, WeatherMakerProfileScript newProfile, float transitionDelay, float transitionDuration)
        {
            // override sound zones
            if (WeatherProfileSoundFadeOutMultiplier > 0.0f)
            {
                foreach (Camera camera in Camera.allCameras)
                {
                    if (WeatherMakerScript.IsLocalPlayer(camera.transform))
                    {
                        WeatherMakerSoundZoneScript soundZone = camera.GetComponentInChildren<WeatherMakerSoundZoneScript>();
                        if (soundZone != null && soundZone.enabled)
                        {
                            float stopSeconds = transitionDuration * WeatherProfileSoundFadeOutMultiplier;
                            soundZone.StopSounds(stopSeconds, true);

                            // add new sounds
                            if (newProfile.SoundProfile != null)
                            {
                                foreach (WeatherMakerSoundGroupScript soundScript in newProfile.SoundProfile.Sounds)
                                {
                                    soundZone.AddSound(soundScript, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static WeatherMakerPlayerSoundManagerScript instance;
        /// <summary>
        /// Shared instance of player sound manager script
        /// </summary>
        public static WeatherMakerPlayerSoundManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
