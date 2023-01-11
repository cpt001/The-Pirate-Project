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
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Full screen wetness overlay script
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerFullScreenWetnessOverlayScript : WeatherMakerFullScreenOverlayScript<WeatherMakerWetnessOverlayProfileScript>
    {
        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
            CommandBufferName = "WeatherMakerFullScreenWetnessOverlayScript";
            base.OnEnable();
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        private static WeatherMakerFullScreenWetnessOverlayScript instance;
        /// <summary>
        /// Shared instance of full screen overlay script
        /// </summary>
        public static WeatherMakerFullScreenWetnessOverlayScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
