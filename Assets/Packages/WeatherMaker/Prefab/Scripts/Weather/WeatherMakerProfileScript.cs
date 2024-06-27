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

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Weather profile script, contains all profiles for all Weather Maker effects
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerProfile", menuName = "WeatherMaker/Weather Profile", order = 10)]
    [System.Serializable]
    public class WeatherMakerProfileScript : WeatherMakerBaseScriptableObjectScript
    {
        /// <summary>Cloud profile</summary>
        [Tooltip("Cloud profile")]
        public WeatherMakerCloudProfileScript CloudProfile;

        /// <summary>Sky profile</summary>
        [Tooltip("Sky profile")]
        public WeatherMakerSkyProfileScript SkyProfile;

        /// <summary>Aurora profile</summary>
        [Tooltip("Aurora profile")]
        public WeatherMakerAuroraProfileScript AuroraProfile;

        /// <summary>Precipitation profile</summary>
        [Tooltip("Precipitation profile")]
        public WeatherMakerPrecipitationProfileScript PrecipitationProfile;

        /// <summary>Fog profile</summary>
        [Tooltip("Fog profile")]
        public WeatherMakerFullScreenFogProfileScript FogProfile;

        /// <summary>Wind profile</summary>
        [Tooltip("Wind profile")]
        public WeatherMakerWindProfileScript WindProfile;

        /// <summary>Lightning profile</summary>
        [Tooltip("Lightning profile")]
        public WeatherMakerLightningProfileScript LightningProfile;

        /// <summary>Sound profile</summary>
        [Tooltip("Sound profile")]
        public WeatherMakerSoundProfileScript SoundProfile;

        /// <summary>Override random duration for profile to transition in, set to 0 to use the profile group transition duration.</summary>
        [MinMaxSlider(0.0f, 600.0f, "Override random duration for profile to transition in, set to 0 to use the profile group transition duration.")]
        public RangeOfFloats TransitionDuration = new RangeOfFloats { Minimum = 0, Maximum = 0 };

        /// <summary>Override Random duration for profile to hold before transition to another profile, set to 0 to use the profile group transition duration.</summary>
        [MinMaxSlider(0.0f, 600.0f, "Override Random duration for profile to hold before transition to another profile, set to 0 to use the profile group transition duration.")]
        public RangeOfFloats HoldDuration = new RangeOfFloats { Minimum = 0, Maximum = 0 };

        /// <summary>
        /// Get a random transition duration
        /// </summary>
        /// <returns>Random transition duration</returns>
        public float RandomTransitionDuration()
        {
            return (TransitionDuration.Maximum <= 0.0f ? 10.0f : TransitionDuration.Random());
        }

        /// <summary>
        /// Get a random hold duration
        /// </summary>
        /// <returns>Random hold duration</returns>
        public float RandomHoldDuration()
        {
            return (HoldDuration.Maximum <= 0.0f ? 10.0f : HoldDuration.Random());
        }

        /// <summary>
        /// Transition from one weather profile to another
        /// </summary>
        /// <param name="managers">Contains all the managers</param>
        /// <param name="script">The profile script to transition to</param>
        /// <param name="transitionDuration">Transition duration</param>
        public void TransitionFrom(IWeatherMakerProvider managers, WeatherMakerProfileScript script, float transitionDuration)
        {
            if (managers == null)
            {
                return;
            }

            float precipitationIntensity = (PrecipitationProfile == null ? 0.0f : PrecipitationProfile.IntensityRange.Random());
            float cloudChangeDuration;
            float precipitationChangeDelay;
            float precipitationChangeDuration;
            transitionDuration = (transitionDuration <= 0.0f ? float.MaxValue : transitionDuration);

            if (precipitationIntensity == 0.0f)
            {
                // changing to no precipitation, no delay in reduction of precipitation
                precipitationChangeDelay = 0.0f;

                // get precipitation removed quickly
                precipitationChangeDuration = transitionDuration * 0.5f;

                // clouds take full duration to transition out
                cloudChangeDuration = transitionDuration;
            }
            else
            {
                // delay change in precipitation so clouds, etc. have time to animate in
                precipitationChangeDelay = transitionDuration * 0.4f;

                // precipitation animates in with remainder of time after delay
                precipitationChangeDuration = transitionDuration - precipitationChangeDelay;

                // clouds animate in faster, we want them mostly in before precipitation gets going
                cloudChangeDuration = transitionDuration * 0.7f;
            }

            // notify precipitation manager
            if (managers.PrecipitationManager != null)
            {
                managers.PrecipitationManager.WeatherProfileChanged(script, this, precipitationChangeDelay, precipitationChangeDuration);
            }

            // notify clouds
            if (managers.CloudManager != null)
            {
                managers.CloudManager.WeatherProfileChanged(script, this, 0.0f, cloudChangeDuration);
            }

            // notify sky
            if (managers.SkyManager != null)
            {
                managers.SkyManager.WeatherProfileChanged(script, this, 0.0f, transitionDuration);
            }

            // notify aurora
            if (managers.AuroraManager != null)
            {
                managers.AuroraManager.WeatherProfileChanged(script, this, 0.0f, transitionDuration);
            }

            // notify fog
            if (managers.FogManager != null)
            {
                managers.FogManager.WeatherProfileChanged(script, this, 0.0f, transitionDuration);
            }

            // notify wind
            if (managers.WindManager != null)
            {
                managers.WindManager.WeatherProfileChanged(script, this, 0.0f, transitionDuration);
            }

            // notify thunder and lightning
            if (managers.ThunderAndLightningManager != null)
            {
                managers.ThunderAndLightningManager.WeatherProfileChanged(script, this, precipitationChangeDelay, transitionDuration);
            }

            // notify player sound manager
            if (managers.PlayerSoundManager != null)
            {
                managers.PlayerSoundManager.WeatherProfileChanged(script, this, precipitationChangeDelay, transitionDuration);
            }
        }
    }

    /// <summary>
    /// Provider of weather maker interfaces
    /// </summary>
    public interface IWeatherMakerProvider
    {
        /// <summary>
        /// Precipitation manager
        /// </summary>
        IPrecipitationManager PrecipitationManager { get; }

        /// <summary>
        /// Cloud manager
        /// </summary>
        ICloudManager CloudManager { get; }

        /// <summary>
        /// Sky manager
        /// </summary>
        ISkyManager SkyManager { get; }

        /// <summary>
        /// Aurora manager
        /// </summary>
        IAuroraManager AuroraManager { get; }

        /// <summary>
        /// Fog manager
        /// </summary>
        IFogManager FogManager { get; }

        /// <summary>
        /// Wind manager
        /// </summary>
        IWindManager WindManager { get; }

        /// <summary>
        /// Thunder and lightning manager
        /// </summary>
        IThunderAndLightningManager ThunderAndLightningManager { get; }

        /// <summary>
        /// Player sound manager
        /// </summary>
        IPlayerSoundManager PlayerSoundManager { get; }
    }
}
