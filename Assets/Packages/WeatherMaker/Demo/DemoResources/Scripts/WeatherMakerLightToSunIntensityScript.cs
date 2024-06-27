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
    /// Set light intensity based on sun
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class WeatherMakerLightToSunIntensityScript : MonoBehaviour
    {
        /// <summary>
        /// Light min intensity
        /// </summary>
        [Range(0.0f, 8.0f)]
        public float LightMinIntensity = 0.0f;

        /// <summary>
        /// Light intensity
        /// </summary>
        [Range(0.0f, 8.0f)]
        public float LightIntensity = 0.35f;

        private Light _light;

        private void Start()
        {
            _light = GetComponent<Light>();
            if (LightIntensity <= 0.0f)
            {
                LightIntensity = _light.intensity;
            }
        }

        private void Update()
        {
            if (WeatherMakerLightManagerScript.Instance != null && WeatherMakerLightManagerScript.Instance.SunPerspective != null)
            {
                _light.intensity = Mathf.Clamp(LightIntensity, LightMinIntensity, Mathf.Max(LightMinIntensity, Mathf.Min(LightIntensity, WeatherMakerLightManagerScript.Instance.SunPerspective.Light.intensity)));
            }
        }
    }
}
