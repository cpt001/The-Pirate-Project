using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Demo script for updating reflection probe 30 times a second
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerDemoReflectionProbeScript : MonoBehaviour
    {
        /// <summary>
        /// Reflection probe to update
        /// </summary>
        public ReflectionProbe ReflectionProbe;

        private float seconds;

        private void Update()
        {
            seconds += Time.unscaledDeltaTime;
            if (seconds >= (1.0f / 30.0f))
            {
                seconds = 0.0f;
                ReflectionProbe.RenderProbe();
            }
        }
    }
}
