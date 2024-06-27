using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Simple script to rotate around y axis
    /// </summary>
    public class WeatherMakerDemoScriptRotateYScript : MonoBehaviour
    {
        /// <summary>
        /// Rotation speed
        /// </summary>
        [Tooltip("Rotation speed")]
        [Range(0.0f, 1000.0f)]
        public float RotationSpeed = 10.0f;

        private void Update()
        {
            transform.Rotate(transform.up, Time.deltaTime * RotationSpeed, Space.Self);
        }
    }
}
