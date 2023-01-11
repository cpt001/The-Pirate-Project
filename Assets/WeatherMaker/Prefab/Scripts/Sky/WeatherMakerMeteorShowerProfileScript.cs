using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Meteor shower profile script
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerMeteorShowerProfile", menuName = "WeatherMaker/Meteor Shower Profile", order = 28)]
    public class WeatherMakerMeteorShowerProfileScript : ScriptableObject
    {
        /// <summary>How many emissions per second</summary>
        [Header("Emission")]
        [SingleLine("How many emissions per second")]
        public RangeOfFloats EmissionRange = new RangeOfFloats(0.1f, 0.3f);

        /// <summary>Radius range, as percent of camera far plane. Determines in how large an area in the sky meteors are emitted from.</summary>
        [SingleLine("Radius range, as percent of camera far plane. Determines in how large an area in the sky meteors are emitted from.")]
        public RangeOfFloats RadiusRange = new RangeOfFloats(0.25f, 0.5f);

        /// <summary>Arc range, determines in what angles the meteors may be emitted from. 360 for all directions.</summary>
        [SingleLine("Arc range, determines in what angles the meteors may be emitted from. 360 for all directions.")]
        public RangeOfFloats ArcRange = new RangeOfFloats(180.0f, 360.0f);

        /// <summary>Speed range in percentage of camera far plane for emitted meteors (min value)</summary>
        [SingleLine("Speed range in percentage of camera far plane for emitted meteors (min value)")]
        public RangeOfFloats SpeedRangeMin = new RangeOfFloats(0.05f, 0.1f);

        /// <summary>Speed range in percentage of camera far plane for emitted meteors (max value)</summary>
        [SingleLine("Speed range in percentage of camera far plane for emitted meteors (max value)")]
        public RangeOfFloats SpeedRangeMax = new RangeOfFloats(0.15f, 0.2f);

        /// <summary>How long, in seconds, meteor emissions should live (min value)</summary>
        [SingleLine("How long, in seconds, meteor emissions should live (min value)")]
        public RangeOfFloats LifetimeRangeMin = new RangeOfFloats(0.4f, 0.9f);

        /// <summary>How long, in seconds, meteor emissions should live (max value)</summary>
        [SingleLine("How long, in seconds, meteor emissions should live (max value)")]
        public RangeOfFloats LifetimeRangeMax = new RangeOfFloats(3.0f, 4.0f);

        /// <summary>Random offset range (-1 to 1, min) multiplied by camera far plane from each rendered camera where the particle system will be placed.</summary>
        [Header("Position")]
        [Tooltip("Random offset range (-1 to 1, min) multiplied by camera far plane from each rendered camera where the particle system will be placed.")]
        public Vector3 OffsetMin = new Vector3(-0.1f, 0.9f, -0.3f);

        /// <summary>Random offset range (-1 to 1, max) multiplied by camera far plane from each rendered camera where the particle system will be placed.</summary>
        [Tooltip("Random offset range (-1 to 1, max) multiplied by camera far plane from each rendered camera where the particle system will be placed.")]
        public Vector3 OffsetMax = new Vector3(0.1f, 0.9f, 0.3f);

        /// <summary>How often in seconds to pick new random emission values. This helps randomize things a bit and keeps from a very static meteor shower, but you can set to 0,0 for no change.</summary>
        [Header("Refresh Rate")]
        [SingleLine("How often in seconds to pick new random emission values. This helps randomize things a bit and keeps from a very static meteor shower, but you can set to 0,0 for no change.")]
        public RangeOfFloats RefreshInterval = new RangeOfFloats(10.0f, 45.0f);

        /// <summary>When is the meteor shower visible, center is sun at horizon. White areas are fully visible, black are fully invisible.</summary>
        [Header("Visibility")]
        [Tooltip("When is the meteor shower visible, center is sun at horizon. White areas are fully visible, black are fully invisible.")]
        public Gradient Visibility;
    }
}
