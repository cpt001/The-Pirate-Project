using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Demo script for animating movement
    /// </summary>
    public class WeatherMakerDemoAnimateMoveScript : MonoBehaviour
    {
        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// End position
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// Duration in seconds
        /// </summary>
        public float Duration;

        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(Start, End, elapsed / Duration);
        }
    }
}