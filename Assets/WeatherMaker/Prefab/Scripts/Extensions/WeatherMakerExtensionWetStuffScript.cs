using UnityEngine;

#if WET_STUFF_PRESENT

using PlaceholderSoftware.WetStuff.Weather;

#endif

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Wet stuff integration script
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/WetStuff", 10)]
    public class WeatherMakerExtensionWetStuffScript

#if WET_STUFF_PRESENT

        : BaseExternalWetnessSource


#else

        : MonoBehaviour

#endif

    {

#if WET_STUFF_PRESENT

        [SerializeField, Range(-1, 1), Tooltip("Current change-per-second in environmental wetness")]
        private float _editorRainIntensity;

        public override float RainIntensity
        {
            get
            {

#if UNITY_EDITOR

                // If we're in edit mode then return the value from the inspector sliders
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    Debug.Log("Wetness: " + _editorRainIntensity);
                    return _editorRainIntensity;
                }

#endif

                var script = WeatherMakerPrecipitationManagerScript.Instance;
                if (script == null)
                {
                    return 0.0f;
                }
                var wetness = Mathf.Min(1.0f, script.RainIntensity + (script.SleetIntensity * 0.5f) +
                    (script.HailIntensity * 0.25f) + (script.SnowIntensity * 0.1f));
                Debug.Log("Wetness: " + wetness);
                return wetness <= 0.0f ? -0.5f : wetness;
            }
        }

#endif

    }
}
