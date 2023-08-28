using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather
{
    /// <summary>
    ///     Provides values about the current wetness of the general environment.
    /// </summary>
    [HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/BaseExternalWetnessSource/")]
    public abstract class BaseExternalWetnessSource
        : MonoBehaviour
    {
        /// <summary>
        ///     Get the current raining or drying intensity.
        /// </summary>
        public abstract float RainIntensity { get; }
    }
}