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

using UnityEngine;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Base class for thunder and lightning
    /// </summary>
    public abstract class WeatherMakerThunderAndLightningScript : MonoBehaviour
    {
        /// <summary>Lightning bolt script</summary>
        [Header("Script and Camera")]
        [Tooltip("Lightning bolt script")]
        public WeatherMakerLightningBoltPrefabScript LightningBoltScript;

        /// <summary>Whether lightning is enabled or not, set this after changing all other properties</summary>
        [Tooltip("Whether lightning is enabled or not, set this after changing all other properties")]
        [UnityEngine.Serialization.FormerlySerializedAs("EnableLightning")]
        [SerializeField]
        private bool _EnableLightning;

        /// <summary>
        /// Toggle lightning enabled - set this last after changing all other properties
        /// </summary>
        public bool EnableLightning
        {
            get { return _EnableLightning; }
            set
            {
                _EnableLightning = value;
                CalculateNextLightningTime();
            }
        }


        /// <summary>Random interval between strikes.</summary>
        [Header("Timing")]
        [SingleLine("Random interval between strikes.")]
        public RangeOfFloats LightningIntervalTimeRange = new RangeOfFloats { Minimum = 10.0f, Maximum = 25.0f };

        /// <summary>Probability (0-1) of an intense lightning bolt that hits really close. Intense lightning has increased brightness and louder thunder compared to normal lightning, and the thunder sounds plays a lot sooner.</summary>
        [Header("Intensity")]
        [Tooltip("Probability (0-1) of an intense lightning bolt that hits really close. Intense lightning has increased brightness and louder thunder compared to normal lightning, and the thunder sounds plays a lot sooner.")]
        [Range(0.0f, 1.0f)]
        public float LightningIntenseProbability = 0.2f;

        /// <summary>Sounds to play for normal thunder. One will be chosen at random for each lightning strike. Depending on intensity, some normal lightning may not play a thunder sound.</summary>
        [Header("Audio")]
        [Tooltip("Sounds to play for normal thunder. One will be chosen at random for each lightning strike. Depending on intensity, some normal lightning may not play a thunder sound.")]
        public AudioClip[] ThunderSoundsNormal;

        /// <summary>Sounds to play for intense thunder. One will be chosen at random for each lightning strike.</summary>
        [Tooltip("Sounds to play for intense thunder. One will be chosen at random for each lightning strike.")]
        public AudioClip[] ThunderSoundsIntense;

        /// <summary>Starting y value for the lightning strikes. Can be absolute or percentage of visible scene height depending on 2D or 3D mode.</summary>
        [Header("Positioning")]
        [SingleLine("Starting y value for the lightning strikes. Can be absolute or percentage of visible scene height depending on 2D or 3D mode.")]
        public RangeOfFloats StartYBase = new RangeOfFloats { Minimum = 500.0f, Maximum = 600.0f };

        /// <summary>Starting y value for the cloud only lightning strikes. Can be absolute or percentage of visible scene height depending on 2D or 3D mode.</summary>
        [SingleLine("Starting y value for the cloud only lightning strikes. Can be absolute or percentage of visible scene height depending on 2D or 3D mode.")]
        public RangeOfFloats StartYBaseCloudOnly = new RangeOfFloats { Minimum = 1500.0f, Maximum = 5000.0f };

        /// <summary>The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartXVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        /// <summary>The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartYVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 0.0f };

        /// <summary>The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats StartZVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        /// <summary>The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the x direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndXVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        /// <summary>The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the y direction. Does not get applied if the lightning hit the ground. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndYVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 0.0f };

        /// <summary>The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.</summary>
        [SingleLine("The variance of the end point in the z direction. Can be absolute or percentage depending on 2D or 3D mode.")]
        public RangeOfFloats EndZVariance = new RangeOfFloats { Minimum = -500.0f, Maximum = 500.0f };

        /// <summary>Probability that lightning strikes will be forced to be visible in the camera view. Even if this fails, there is still a change that the lightning will be visible. Ignored for some modes such as 2D.</summary>
        [Tooltip("Probability that lightning strikes will be forced to be visible in the camera view. Even if this fails, there is still " +
            "a change that the lightning will be visible. Ignored for some modes such as 2D.")]
        [Range(0.0f, 1.0f)]
        public float LightningForcedVisibilityProbability = 0.5f;

        /// <summary>The chance that non-cloud lightning will hit the ground</summary>
        [Tooltip("The chance that non-cloud lightning will hit the ground")]
        [Range(0.0f, 1.0f)]
        public float GroundLightningChance = 0.3f;

        /// <summary>The chance lightning will simply be in the clouds with no visible bolt</summary>
        [Tooltip("The chance lightning will simply be in the clouds with no visible bolt")]
        [Range(0.0f, 1.0f)]
        public float CloudLightningChance = 0.5f;

        /// <summary>Volume modifier for thunder</summary>
        [Tooltip("Volume modifier for thunder")]
        [System.NonSerialized]
        public float VolumeModifier = 1.0f;

        /// <summary>External intensity modifier to reduce amount of lightning. For example if a player moves indoors or in a cave, you could set this to 0 to turn off lightning.</summary>
        [Tooltip("External intensity modifier to reduce amount of lightning. For example if a player moves indoors or in a cave, you could set this to 0 to turn off lightning.")]
        [Range(0.0f, 1.0f)]
        public float ExternalIntensityMultiplier = 1.0f;

        /// <summary>
        /// Thunder audio source
        /// </summary>
        public AudioSource AudioSourceThunder { get; private set; }

        /// <summary>
        /// Fires when a thunder sounds plays. Parameters are the audio clip and intensity.
        /// </summary>
        public System.Action<AudioClip, float> ThunderSoundPlayed;

        /// <summary>
        /// Next strike time
        /// </summary>
        protected float NextLightningTime { get; private set; }

        /// <summary>
        /// Last sound played
        /// </summary>
        protected AudioClip LastThunderSound { get; private set; }

        private void CalculateNextLightningTime()
        {
            NextLightningTime = Time.time + LightningIntervalTimeRange.Random();
        }

        private void CheckForLightning()
        {
            // time for another strike?
            float v = (Time.time - NextLightningTime);
            if (v >= 0.0f)
            {
                // as long as we haven't gone over a second beyond the strike time, do the strike
                // if they pause or background the game, we don't want a strike immediately on un-pause
                if (v < 1.0f && ExternalIntensityMultiplier > 0.0f)
                {
                    StartCoroutine(ProcessLightning(null, null));
                }
                CalculateNextLightningTime();
            }
        }

        private IEnumerator ProcessLightning(Vector3? start, Vector3? end, bool? _intense = null, bool? _forceVisible = null)
        {
            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.AllowCameras == null ||
                WeatherMakerScript.Instance.AllowCameras.Count == 0)
            {
                yield break;
            }

            bool intense = _intense ?? (WeatherMakerRandomizer.Unity.Random() <= LightningIntenseProbability);
            bool forceVisible = _forceVisible ?? (WeatherMakerRandomizer.Unity.Random() <= LightningForcedVisibilityProbability);
            float sleepTime;
            AudioClip[] sounds;
            float intensity;

            if (intense)
            {
                float percent = WeatherMakerRandomizer.Unity.Random();
                intensity = Mathf.Lerp(2.0f, 8.0f, percent);
                sleepTime = 5.0f / intensity;
                sounds = ThunderSoundsIntense;
            }
            else
            {
                float percent = WeatherMakerRandomizer.Unity.Random();
                intensity = Mathf.Lerp(1.0f, 2.0f, percent);
                sleepTime = 30.0f / intensity;
                sounds = ThunderSoundsNormal;
            }

            // perform the strike
            Camera camera = WeatherMakerScript.Instance.AllowCameras[0];
            Strike(start, end, intense, intensity, camera, forceVisible);

            // thunder will play depending on intensity of lightning
            bool playThunder = (intensity >= 1.0f);

            //Debug.Log("Lightning intensity: " + intensity.ToString("0.00") + ", thunder delay: " +
            //          (playThunder ? sleepTime.ToString("0.00") : "No Thunder"));

            if (playThunder && sounds != null && sounds.Length != 0)
            {
                // wait for a bit then play a thunder sound
                yield return new WaitForSeconds(sleepTime);

                AudioClip clip = null;
                do
                {
                    // pick a random thunder sound that wasn't the same as the last sound, unless there is only one sound, then we have no choice
                    clip = sounds[WeatherMakerRandomizer.Unity.Random(0, sounds.Length - 1)];
                }
                while (sounds.Length > 1 && clip == LastThunderSound);

                // set the last sound and play it
                LastThunderSound = clip;

                float thunderSoundIntensity = intensity * 0.5f * VolumeModifier;
                AudioSourceThunder.PlayOneShot(clip, thunderSoundIntensity);

                if (ThunderSoundPlayed != null)
                {
                    ThunderSoundPlayed(clip, thunderSoundIntensity);
                }
            }
        }

        private void Strike(Vector3? _start, Vector3? _end, bool intense, float intensity, Camera camera, bool forceVisible)
        {
            WeatherMakerCelestialObjectScript sun = WeatherMakerLightManagerScript.SunForCamera(camera);
            if (sun == null)
            {
                return;
            }

            // save the generations and trunk width in case of cloud only lightning which will modify these properties
            int generations = LightningBoltScript.Generations;
            RangeOfFloats trunkWidth = LightningBoltScript.TrunkWidthRange;

            if (WeatherMakerRandomizer.Unity.Random() < CloudLightningChance)
            {
                // cloud only lightning
                LightningBoltScript.TrunkWidthRange = new RangeOfFloats();
                LightningBoltScript.Generations = 1;
            }

            Vector3 anchorPosition = camera.transform.position;
            Vector3 start = _start ?? CalculateStartPosition(ref anchorPosition, (forceVisible ? camera : null), intense, LightningBoltScript.Generations <= 1);
            Vector3 end = _end ?? CalculateEndPosition(ref anchorPosition, ref start, (forceVisible ? camera : null), intense);

            float intensityModifier = 1.0f;
            CameraMode mode = WeatherMakerScript.ResolveCameraMode(null, camera);
            if (sun != null)
            {
                if (mode != CameraMode.Perspective)
                {
                    float sunX = (sun.transform.eulerAngles.x + 180.0f);
                    sunX = (sunX >= 360.0f ? sunX - 360.0f : sunX);
                    sunX = Mathf.Abs((sunX * 0.5f) - 90.0f);
                    intensityModifier = Mathf.Lerp(0.1f, 0.75f, sunX * 0.016f);
                }
                else
                {
                    float sunX = (sun.transform.eulerAngles.x + 90.0f);
                    sunX = (sunX >= 360.0f ? sunX - 360.0f : sunX);
                    sunX = Mathf.Abs((sunX * 0.5f) - 90.0f);
                    intensityModifier = Mathf.Lerp(0.1f, 0.75f, sunX * 0.006f);
                }
            }
            LightningBoltScript.LightParameters.LightIntensity = Mathf.Max(1.0f, intensity * intensityModifier);
            if (mode == CameraMode.Perspective)
            {
                if (LightningBoltScript.Generations > 1)
                {
                    Vector3 lightStart = start;
                    if (camera != null)
                    {
                        // HACK: Pull light toward camera to light clouds better, for some reason the light appears behind the lightning bolt
                        //  need to figure out why...
                        Vector3 toLight = camera.transform.position - lightStart;
                        lightStart += (toLight * 0.5f);
                        lightStart.y = 1000.0f;
                    }
                    LightningBoltScript.LightParameters.LightPosition = lightStart;
                }
                else
                {
                    LightningBoltScript.LightParameters.LightPosition = start;
                }
                LightningBoltScript.LightParameters.LightRange = WeatherMakerRandomizer.Unity.Random(3000.0f, 6000.0f);
            }
            LightningBoltScript.Trigger(start, end);

            // restore properties in case they were modified
            LightningBoltScript.TrunkWidthRange = trunkWidth;
            LightningBoltScript.Generations = generations;
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
        }

        /// <summary>
        /// Start
        /// </summary>
        protected virtual void Start()
        {
            AudioSourceThunder = gameObject.GetComponent<AudioSource>();
            CalculateNextLightningTime();
        }

        /// <summary>
        /// Compute start position
        /// </summary>
        /// <param name="anchorPosition">Anchor position</param>
        /// <param name="visibleInCamera">Visible in camera?</param>
        /// <param name="intense">Intense (close) or normal (far)</param>
        /// <param name="cloudOnly">Cloud only with no bolt?</param>
        /// <returns>Start position</returns>
        protected abstract Vector3 CalculateStartPosition(ref Vector3 anchorPosition, Camera visibleInCamera, bool intense, bool cloudOnly);

        /// <summary>
        /// Compute end position
        /// </summary>
        /// <param name="anchorPosition">Anchor position</param>
        /// <param name="end">Start position</param>
        /// <param name="visibleInCamera">Camera to show in, can be null</param>
        /// <param name="intense">Intense (close) or normal (far)</param>
        /// <returns>End position</returns>
        protected abstract Vector3 CalculateEndPosition(ref Vector3 anchorPosition, ref Vector3 end, Camera visibleInCamera, bool intense);
        
        /// <summary>
        /// Update
        /// </summary>
        protected virtual void Update()
        {
            VolumeModifier = WeatherMakerAudioManagerScript.CachedWeatherVolumeModifier;
            if (EnableLightning)
            {
                CheckForLightning();
            }
        }

        private void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// Summon normal lightning
        /// </summary>
        public void CallNormalLightning()
        {
            CallNormalLightning(null, null);
        }

        /// <summary>
        /// Summon normal lightning
        /// </summary>
        /// <param name="start">Force start position or null for random</param>
        /// <param name="end">Force end position or null for random</param>
        public void CallNormalLightning(Vector3? start, Vector3? end)
        {
            StartCoroutine(ProcessLightning(start, end, false, true));
        }

        /// <summary>
        /// Summon intense lightning
        /// </summary>
        public void CallIntenseLightning()
        {
            CallIntenseLightning(null, null);
        }

        /// <summary>
        /// Summon intense lightning
        /// </summary>
        /// <param name="start">Force start position or null for random</param>
        /// <param name="end">Force end position or null for random</param>
        public void CallIntenseLightning(Vector3? start, Vector3? end)
        {
            StartCoroutine(ProcessLightning(start, end, true, true));
        }

        private static WeatherMakerThunderAndLightningScript instance;
        /// <summary>
        /// Shared instance of thunder and lightning script
        /// </summary>
        public static WeatherMakerThunderAndLightningScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}