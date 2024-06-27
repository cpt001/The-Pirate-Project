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
    /// Lightning manager interface
    /// </summary>
    public interface IThunderAndLightningManager : IWeatherMakerManager
    {
        // TODO: Expose things like lightning frequency and intensity
    }

    /// <summary>
    /// Thunder and lightning manager default implementation
    /// </summary>
    public class WeatherMakerThunderAndLightningManagerScript : MonoBehaviour, IThunderAndLightningManager
    {
        /// <summary>Thunder and lightning script</summary>
        [Header("Dependencies")]
        [Tooltip("Thunder and lightning script")]
        public WeatherMakerThunderAndLightningScript ThunderAndLightningScript;

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
        /// Weather profile changed for thunder and lightning
        /// </summary>
        /// <param name="oldProfile">Old profile</param>
        /// <param name="newProfile">New profile</param>
        /// <param name="transitionDelay">Transition delay</param>
        /// <param name="transitionDuration">Transition duration</param>
        public void WeatherProfileChanged(WeatherMakerProfileScript oldProfile, WeatherMakerProfileScript newProfile, float transitionDelay, float transitionDuration)
        {
            FloatTween tween = TweenFactory.Tween("WeatherMakerScriptProfileChangeLightning", 0.0f, 1.0f, 0.01f, TweenScaleFunctions.Linear, null, (completion) =>
            {
                ThunderAndLightningScript.LightningIntenseProbability = newProfile.LightningProfile.LightningIntenseProbability;
                ThunderAndLightningScript.LightningIntervalTimeRange = newProfile.LightningProfile.LightningIntervalTimeRange;
                ThunderAndLightningScript.LightningForcedVisibilityProbability = newProfile.LightningProfile.LightningForcedVisibilityProbability;
                ThunderAndLightningScript.GroundLightningChance = newProfile.LightningProfile.LightningGroundChance;
                ThunderAndLightningScript.CloudLightningChance = newProfile.LightningProfile.LightningCloudChance;
                ThunderAndLightningScript.EnableLightning = (newProfile.LightningProfile.LightningIntervalTimeRange.Minimum > 0.0f);
            });
            tween.Delay = transitionDelay;
        }

        private static WeatherMakerThunderAndLightningManagerScript instance;
        /// <summary>
        /// Shared instance of thunder and lightning manager script
        /// </summary>
        public static WeatherMakerThunderAndLightningManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
