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
    /// Sky manager interface
    /// </summary>
    public interface ISkyManager : IWeatherMakerManager
    {
    }

    /// <summary>
    /// Cloud manager default implementation
    /// </summary>
    public class WeatherMakerSkyManagerScript : MonoBehaviour, ISkyManager
    {
        /// <summary>Sky sphere script</summary>
        [Header("Dependencies")]
        [Tooltip("Sky sphere script")]
        public WeatherMakerSkySphereScript SkySphereScript;

        /// <summary>Sky plane script</summary>
        [Tooltip("Sky plane script")]
        public WeatherMakerSkyPlaneScript SkyPlaneScript;

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
        /// Sky manager weather profile changed handler
        /// </summary>
        /// <param name="oldProfile">Old profile</param>
        /// <param name="newProfile">New profile</param>
        /// <param name="transitionDelay">Transition delay</param>
        /// <param name="transitionDuration">Transition duration</param>
        public void WeatherProfileChanged(WeatherMakerProfileScript oldProfile, WeatherMakerProfileScript newProfile, float transitionDelay, float transitionDuration)
        {
            if (newProfile.SkyProfile != null)
            {
                if (SkySphereScript != null)
                {
                    SkySphereScript.SkySphereProfile = newProfile.SkyProfile;
                }
                else if (SkyPlaneScript != null)
                {
                    SkyPlaneScript.SkyPlaneProfile = newProfile.SkyProfile;
                }
            }
        }

        private static WeatherMakerSkyManagerScript instance;
        /// <summary>
        /// Shared instance of sky manager script
        /// </summary>
        public static WeatherMakerSkyManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
