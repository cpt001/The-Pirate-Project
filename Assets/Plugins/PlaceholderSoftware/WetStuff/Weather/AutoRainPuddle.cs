using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather
{
    /// <summary>
    ///     A rain puddle which is automatically controlled by a wetness source. Puddle expands as wetness is increasing and
    ///     shrinks when wetness is decreasing.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/AutoRainPuddle/")]
    public class AutoRainPuddle
        : RainPuddle
    {
        private const int MinFrameCounter = 10;

        [SerializeField] public BaseExternalWetnessSource WetnessSource;
        [SerializeField, Range(0, 1)] public float RainingSpeed = 0.05f;
        [SerializeField, Range(0, 1)] public float DryingSpeed = 0.02f;

        private bool _raining;
        private int _frameCounter;

        protected override void Update()
        {
            UpdateState();
            base.Update();
        }

        private void UpdateState()
        {
            if (Application.isPlaying)
            {
                // Get wetness values from weather source
                var delta = WetnessSource.RainIntensity;
                
                // Determine if wetness is increasing or not
                var raining = delta > 0;

                // Change rate of rain puddle change based on the wetness delta
                var multiplier = raining ? RainingSpeed : DryingSpeed;
                Rate = Time.deltaTime <= float.Epsilon ? 0 : Mathf.Abs(delta) * multiplier;

                // If raining state changed, reset frame counter
                if (_raining != raining)
                    _frameCounter = 0;

                // If the new state has been stable for at least a minimum number of frames
                if (_frameCounter > MinFrameCounter)
                {
                    // Detect if the desired mode is not what we are currently playing,
                    // start playing a new set of curves if so
                    if (raining && State != RainState.Raining)
                        BeginRaining();
                    else if (!raining && State != RainState.Drying)
                        BeginDrying();
                }

                _frameCounter++;
                _raining = raining;
            }
        }
    }
}