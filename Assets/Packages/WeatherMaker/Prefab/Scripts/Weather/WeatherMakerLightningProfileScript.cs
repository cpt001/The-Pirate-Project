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
    /// Lightning profile script, contains all properties that change lightning appearance
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerLightningProfile", menuName = "WeatherMaker/Lightning Profile", order = 27)]
    [System.Serializable]
    public class WeatherMakerLightningProfileScript : WeatherMakerBaseScriptableObjectScript
    {
        /// <summary>Probability lightning will be intense (close up and loud).</summary>
        [Tooltip("Probability lightning will be intense (close up and loud).")]
        [Range(0.0f, 1.0f)]
        public float LightningIntenseProbability;

        /// <summary>The random range of seconds in between lightning strikes.</summary>
        [SingleLine("The random range of seconds in between lightning strikes.")]
        public RangeOfFloats LightningIntervalTimeRange = new RangeOfFloats { Minimum = 10.0f, Maximum = 25.0f };

        /// <summary>Probability that lightning strikes will be forced to be visible in the camera view. Even if this fails, there is still a change that the lightning will be visible. Ignored for some modes such as 2D.</summary>
        [Tooltip("Probability that lightning strikes will be forced to be visible in the camera view. Even if this fails, there is still " +
            "a change that the lightning will be visible. Ignored for some modes such as 2D.")]
        [Range(0.0f, 1.0f)]
        public float LightningForcedVisibilityProbability = 0.5f;

        /// <summary>The chance that non-cloud lightning will hit the ground.</summary>
        [Tooltip("The chance that non-cloud lightning will hit the ground.")]
        [Range(0.0f, 1.0f)]
        public float LightningGroundChance = 0.3f;

        /// <summary>The chance lightning will simply be in the clouds with no visible bolt.</summary>
        [Tooltip("The chance lightning will simply be in the clouds with no visible bolt.")]
        [Range(0.0f, 1.0f)]
        public float LightningCloudChance = 0.5f;
    }
}
