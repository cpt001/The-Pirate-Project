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

using System;
using System.Linq;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Aurora profile script, contains all aurora configuration fields
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerAuroraProfile", menuName = "WeatherMaker/Aurora Profile", order = 35)]
    [System.Serializable]
    public class WeatherMakerAuroraProfileScript : ScriptableObject
    {
        /// <summary>Sample count for ray march</summary>
        [Header("Aurora noise")]
        [Tooltip("Sample count for ray march")]
        [Range(0, 64)]
        public int SampleCount = 42;

        /// <summary>Noise sample count</summary>
        [Tooltip("Noise sample count")]
        [Range(1, 4)]
        public int SubSampleCount = 4;

        /// <summary>Aurora animation speed (xy scroll, z wobble)</summary>
        [Tooltip("Aurora animation speed (xy scroll, z wobble)")]
        public Vector3 AnimationSpeed = new Vector3(0.001f, 0.001f, 0.001f);

        /// <summary>Aurora march scale (xyz)</summary>
        [Tooltip("Aurora march scale (xyz)")]
        public Vector3 MarchScale = Vector3.one;

        /// <summary>Aurora detail scale, controls finer details of the aurora, x = shape scale, y = detail scale, z = detail influence (0 - 1).</summary>
        [Tooltip("Aurora detail scale, controls finer details of the aurora, x = shape scale, y = detail scale, z = detail influence (0 - 1).")]
        public Vector3 Scale = new Vector3(1.42341f, 2.9123f, 0.69f);

        /// <summary>Aurora noise octave, x = decay, y = amplitude</summary>
        [Tooltip("Aurora noise octave, x = decay, y = amplitude")]
        public Vector2 Octave = new Vector2(0.47312f, 0.72654f);

        /// <summary>Gradient with 4 colors. Left of gradient is bottom of aurora, right of gradient is top of aurora.</summary>
        [Header("Aurora appearance")]
        [Tooltip("Gradient with 4 colors. Left of gradient is bottom of aurora, right of gradient is top of aurora.")]
        public Gradient Color;

        /// <summary>Aurora intensity</summary>
        [Tooltip("Aurora intensity")]
        [Range(0.0f, 100.0f)]
        public float Intensity = 2.0f;

        /// <summary>Aurora power, controls how dim areas fade out more</summary>
        [Tooltip("Aurora power, controls how dim areas fade out more")]
        [Range(0.0f, 16.0f)]
        public float Power = 3.0f;

        /// <summary>Controls how aurora fades as it reaches max height</summary>
        [Tooltip("Controls how aurora fades as it reaches max height")]
        [Range(1.0f, 64.0f)]
        public float HeightFadePower = 4.0f;

        /// <summary>The min and max height (in world space units) for the aurora</summary>
        [SingleLine("The min and max height (in world space units) for the aurora")]
        public RangeOfFloats Height = new RangeOfFloats(10000.0f, 30000.0f);

        /// <summary>Radius (in world space units) of planet to render aurora around</summary>
        [Tooltip("Radius (in world space units) of planet to render aurora around")]
        [Range(1000.0f, 10000000.0f)]
        public float PlanetRadius = 1000000.0f;

        /// <summary>Aurora distance fade, fade by distance more as this approaches 1</summary>
        [Tooltip("Aurora distance fade, fade by distance more as this approaches 1")]
        [Range(0.0f, 1.0f)]
        public float DistanceFade = 0.4f;

        /// <summary>Aurora dither</summary>
        [Tooltip("Aurora dither")]
        [Range(0.0f, 0.1f)]
        public float Dither = 0.005f;

        /// <summary>Aurora ambient light</summary>
        [Tooltip("Aurora ambient light")]
        public Color AmbientLight = UnityEngine.Color.black;

        private readonly Vector4[] colors = new Vector4[4];

        private bool needsUpdate = true;

        private void LateUpdate()
        {
            if (needsUpdate)
            {
                needsUpdate = false;
                UpdateAnimationProperties();
            }
        }

        private void OnDisable()
        {
            needsUpdate = true;
        }

        /// <summary>
        /// Update aurora shader variables
        /// </summary>
        public void UpdateShaderVariables()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Shader.SetGlobalInt(WMS._WeatherMakerAuroraSampleCount, Mathf.Clamp(AnimationSampleCount, 0, 64));
            Shader.SetGlobalInt(WMS._WeatherMakerAuroraSubSampleCount, Mathf.Clamp(AnimationSubSampleCount, 0, 10));
            Shader.SetGlobalVector(WMS._WeatherMakerAuroraAnimationSpeed, AnimationAnimationSpeed);
            Shader.SetGlobalVector(WMS._WeatherMakerAuroraMarchScale, AnimationMarchScale);
            Shader.SetGlobalVector(WMS._WeatherMakerAuroraScale, AnimationScale);
            GradientColorKey[] colorKeys = AnimationColor.colorKeys;
            if (colorKeys.Length != 4)
            {
                Debug.LogErrorFormat("Weather Maker aurora profile '{0}' color must have exactly 4 color keys", name);
            }
            else
            {
                colors[0] = colorKeys[0].color;
                colors[1] = colorKeys[1].color;
                colors[2] = colorKeys[2].color;
                colors[3] = colorKeys[3].color;
                Shader.SetGlobalVectorArray(WMS._WeatherMakerAuroraColor, colors);
                Vector4 colorKeysVector = new Vector4(colorKeys[0].time, colorKeys[1].time,
                    colorKeys[2].time, colorKeys[3].time);
                Shader.SetGlobalVector(WMS._WeatherMakerAuroraColorKeys, colorKeysVector);
            }
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraIntensity, AnimationIntensity);
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraPower, AnimationPower);
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraHeightFadePower, AnimationHeightFadePower);
            Shader.SetGlobalVector(WMS._WeatherMakerAuroraHeight, AnimationHeight.ToVector2());
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraPlanetRadius, PlanetRadius);
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraDistanceFade, AnimationDistanceFade);
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraDither, AnimationDither);
            Shader.SetGlobalVector(WMS._WeatherMakerAuroraOctave, AnimationOctave);
        }

        /// <summary>
        /// Animate from one aurora profile to another
        /// </summary>
        /// <param name="oldProfile">Old profile</param>
        /// <param name="duration">Duration</param>
        /// <param name="key">Animation key</param>
        /// <param name="completion">Completion callback</param>
        public void AnimateFrom(WeatherMakerAuroraProfileScript oldProfile, float duration, string key, System.Action completion)
        {
            if (oldProfile != null)
            {
                TweenFactory.Tween(key, 0.0f, 1.0f, duration, TweenScaleFunctions.QuadraticEaseInOut, (ITween<float> v) =>
                {
                    AnimationSampleCount = Mathf.RoundToInt(Mathf.Lerp((float)oldProfile.AnimationSampleCount, (float)SampleCount, v.CurrentProgress));
                    AnimationSubSampleCount = Mathf.RoundToInt(Mathf.Lerp((float)oldProfile.AnimationSubSampleCount, (float)SubSampleCount, v.CurrentProgress));
                    AnimationAnimationSpeed = Vector3.Lerp(oldProfile.AnimationAnimationSpeed, AnimationSpeed, v.CurrentProgress);
                    AnimationColor = oldProfile.AnimationColor.Lerp(Color, v.CurrentProgress);
                    AnimationDither = Mathf.Lerp(oldProfile.AnimationDither, Dither, v.CurrentProgress);
                    AnimationIntensity = Mathf.Lerp(oldProfile.AnimationIntensity, Intensity, v.CurrentProgress);
                    AnimationPower = Mathf.Lerp(oldProfile.AnimationPower, Power, v.CurrentProgress);
                    AnimationHeightFadePower = Mathf.Lerp(oldProfile.AnimationHeightFadePower, HeightFadePower, v.CurrentProgress);
                    AnimationMarchScale = Vector3.Lerp(oldProfile.AnimationMarchScale, MarchScale, v.CurrentProgress);
                    AnimationScale = Vector3.Lerp(oldProfile.AnimationScale, Scale, v.CurrentProgress);
                    AnimationHeight = new RangeOfFloats(Mathf.Lerp(oldProfile.AnimationHeight.Minimum, Height.Minimum, v.CurrentProgress), Mathf.Lerp(oldProfile.Height.Maximum, Height.Maximum, v.CurrentProgress));
                    AnimationPlanetRadius = Mathf.Lerp(oldProfile.AnimationPlanetRadius, PlanetRadius, v.CurrentProgress);
                    AnimationDistanceFade = Mathf.Lerp(oldProfile.AnimationDistanceFade, DistanceFade, v.CurrentProgress);
                    AnimationAmbientColor = UnityEngine.Color.Lerp(oldProfile.AnimationAmbientColor, AmbientLight * Intensity, v.CurrentProgress);
                    AnimationOctave = Vector2.Lerp(oldProfile.AnimationOctave, Octave, v.CurrentProgress);

                    if (WeatherMakerScript.Instance != null)
                    {
                        AnimationSampleCount = Mathf.Min(WeatherMakerScript.Instance.PerformanceProfile.AuroraSampleCount, AnimationSampleCount);
                        AnimationSubSampleCount = Mathf.Min(WeatherMakerScript.Instance.PerformanceProfile.AuroraSubSampleCount, AnimationSubSampleCount);
                    }
                }, (ITween<float> v) =>
                {
                    if (completion != null)
                    {
                        completion.Invoke();
                    }
                });
            }
            else
            {
                UpdateAnimationProperties();
                if (completion != null)
                {
                    completion.Invoke();
                }
            }
        }

        /// <summary>
        /// Updates internal animation properties
        /// </summary>
        public void UpdateAnimationProperties()
        {
            AnimationSampleCount = SampleCount;
            AnimationSubSampleCount = SubSampleCount;
            AnimationAnimationSpeed = AnimationSpeed;
            AnimationColor = Color;
            AnimationDither = Dither;
            AnimationIntensity = Intensity;
            AnimationPower = Power;
            AnimationHeightFadePower = HeightFadePower;
            AnimationMarchScale = MarchScale;
            AnimationScale = Scale;
            AnimationHeight = Height;
            AnimationPlanetRadius = PlanetRadius;
            AnimationDistanceFade = DistanceFade;
            AnimationAmbientColor = AmbientLight * Intensity;
            AnimationOctave = Octave;

            if (WeatherMakerScript.Instance != null)
            {
                AnimationSampleCount = Mathf.Min(WeatherMakerScript.Instance.PerformanceProfile.AuroraSampleCount, AnimationSampleCount);
                AnimationSubSampleCount = Mathf.Min(WeatherMakerScript.Instance.PerformanceProfile.AuroraSubSampleCount, AnimationSubSampleCount);
            }
        }

        /// <summary>
        /// Whether the aurora should render
        /// </summary>
        public bool AuroraEnabled
        {
            get { return AnimationSampleCount > 0 && AnimationIntensity > 0.0f; }
        }

        /// <summary>
        /// Animation sample count for ray march
        /// </summary>
        public int AnimationSampleCount { get; set; }

        /// <summary>
        /// Animation sub-sample count for ray march
        /// </summary>
        public int AnimationSubSampleCount { get; set; }

        /// <summary>
        /// Animation speed, how fast aurora animates
        /// </summary>
        public Vector3 AnimationAnimationSpeed { get; set; }

        /// <summary>
        /// Animation march scale, determines how fine detailed the aurora ray-march is
        /// </summary>
        public Vector3 AnimationMarchScale { get; set; }

        /// <summary>
        /// Animation scale, determines how fine detailed the aurora noise is
        /// </summary>
        public Vector3 AnimationScale { get; set; }

        /// <summary>
        /// Animation color, tints the aurora
        /// </summary>
        public Gradient AnimationColor { get; set; }

        /// <summary>
        /// Animation intensity, modified brightness of the aurora
        /// </summary>
        public float AnimationIntensity { get; set; }

        /// <summary>
        /// Animation power, determines fade of the aurora
        /// </summary>
        public float AnimationPower { get; set; }

        /// <summary>
        /// Animation height power, determines aurora fade as height increases
        /// </summary>
        public float AnimationHeightFadePower { get; set; }

        /// <summary>
        /// Animation height, determines min/max height of aurora
        /// </summary>
        public RangeOfFloats AnimationHeight { get; set; }

        /// <summary>
        /// Aurora planet radius
        /// </summary>
        public float AnimationPlanetRadius { get; set; }

        /// <summary>
        /// Aurora distance fade
        /// </summary>
        public float AnimationDistanceFade { get; set; }

        /// <summary>
        /// Aurora dithering
        /// </summary>
        public float AnimationDither { get; set; }

        /// <summary>
        /// Aurora ambient color influence
        /// </summary>
        public Color AnimationAmbientColor { get; set; }

        /// <summary>
        /// Aurora octave, modifies noise generation
        /// </summary>
        public Vector2 AnimationOctave { get; set; }
    }
}
