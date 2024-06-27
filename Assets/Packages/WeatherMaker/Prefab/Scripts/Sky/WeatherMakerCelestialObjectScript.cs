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
    /// Makes a directional light a celestial object
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class WeatherMakerCelestialObjectScript : MonoBehaviour
    {
        /// <summary>Whether this is a sun (false if moon)</summary>
        [Tooltip("Whether this is a sun (false if moon)")]
        public bool IsSun;

        /// <summary>Rotation about y axis - changes how the celestial body orbits over the scene</summary>
        [Range(0.0f, 360.0f)]
        [Tooltip("Rotation about y axis - changes how the celestial body orbits over the scene")]
        public float RotateYDegrees;

        /// <summary>The orbit type. Only from Earth orbit is currently supported.</summary>
        [Tooltip("The orbit type. Only from Earth orbit is currently supported.")]
        public WeatherMakerOrbitType OrbitType = WeatherMakerOrbitType.FromEarth;

        /// <summary>The scale of the object. For the sun, this is shader specific. For moons, this is a percentage of camera far plane.</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("The scale of the object. For the sun, this is shader specific. For moons, this is a percentage of camera far plane.")]
        public float Scale = 0.02f;

        /// <summary>Whether to scale the object at the horizon (if it is a moon).</summary>
        [Tooltip("Whether to scale the object at the horizon (if it is a moon)")]
        public bool ScaleMoonAtHorizon = true;

        /// <summary>Light mode. If set to None, you must set the light intensity yourself.</summary>
        [Tooltip("Light mode. If set to None, you must set the light intensity yourself.")]
        public WeatherMakerCelestialObjectLightMode LightMode = WeatherMakerCelestialObjectLightMode.Standard;

        /// <summary>Light color</summary>
        [Tooltip("Light color")]
        public Color LightColor = new Color(1.0f / 254.0f, 1.0f / 255.0f, 1.0f / 201.0f, 1.0f);

        /// <summary>Light power, controls how intense the light lights up the clouds, etc. near the object. Lower values reduce the radius and increase the intensity.</summary>
        [Range(0.0f, 128.0f)]
        [Tooltip("Light power, controls how intense the light lights up the clouds, etc. near the object. Lower values reduce the radius and increase the intensity.")]
        public float LightPower = 8.0f;

        /// <summary>The intensity of the light of the object at default (full) intensity</summary>
        [Tooltip("The intensity of the light of the object at default (full) intensity")]
        [Range(0.0f, 10.0f)]
        public float LightBaseIntensity = 1.0f;

        /// <summary>The shadow strength of the light of the object at default (full) shadow intensity</summary>
        [Tooltip("The shadow strength of the light of the object at default (full) shadow intensity")]
        [Range(0.0f, 1.0f)]
        public float LightBaseShadowStrength = 0.8f;

        /// <summary>Light multiplier</summary>
        [Range(0.0f, 10.0f)]
        [Tooltip("Light multiplier")]
        public float LightMultiplier = 1.0f;

        /// <summary>Tint color of the object.</summary>
        [Tooltip("Tint color of the object.")]
        public Color TintColor = Color.white;

        /// <summary>Tint intensity</summary>
        [Range(0.0f, 10.0f)]
        [Tooltip("Tint intensity")]
        public float TintIntensity = 1.0f;

        /// <summary>Shafts multiplier. Brightens up cloud or fog shafts. Useful for moons.</summary>
        [Range(0.0f, 100.0f)]
        [Tooltip("Shafts multiplier. Brightens up cloud or fog shafts. Useful for moons.")]
        public float ShaftMultiplier = 1.0f;

        /// <summary>Horizon multiplier (x = multiplier, y = max intensity, z = power reducer). Increases light intensity in certain effects like clouds as object reaches horizon. Great for brighter sunsets.</summary>
        [Tooltip("Horizon multiplier (x = multiplier, y = max intensity, z = power reducer). Increases light intensity in certain effects like clouds as object reaches horizon. Great for brighter sunsets.")]
        public Vector3 HorizonMultiplier;

        /// <summary>Used to block lens flare if clouds are over the sun. Just needs to be a sphere collider.</summary>
        [Header("Lens flare")]
        [Tooltip("Used to block lens flare if clouds are over the sun. Just needs to be a sphere collider.")]
        public GameObject LensFlareBlocker;

        /// <summary>Game object to use as lens flare cloud probe target. This object should have a cloud probe script on it.</summary>
        [Tooltip("Game object to use as lens flare cloud probe target. This object should have a cloud probe script on it.")]
        public Transform FlareCloudProbeTarget;

        /// <summary>Minimum cloud cover to block the lens flare.</summary>
        [Tooltip("Minimum cloud cover to block the lens flare.")]
        [Range(0.0f, 1.0f)]
        public float MinimumCloudCoverToBlockLensFlare = 0.5f;

        /// <summary>
        /// The light for this celestial object
        /// </summary>
        public Light Light { get; private set; }

        /// <summary>
        /// The renderer for this celestial object
        /// </summary>
        public Renderer Renderer { get; private set; }

        /// <summary>
        /// The mesh filter for this celestial object
        /// </summary>
        public MeshFilter MeshFilter { get; private set; }

        /// <summary>
        /// The collider for this celestial object
        /// </summary>
        public Collider Collider { get; private set; }

        /// <summary>
        /// Lens flare (if any) for this celestial object
        /// </summary>
        public LensFlare Flare { get; private set; }

        /// <summary>
        /// Difference (0-1) of light from last frame. A value of 1 indicates enough difference that shaders should completely re-render.
        /// </summary>
        public float Difference { get; private set; }

        /// <summary>
        /// Whether the orbit type is custom
        /// </summary>
        public bool OrbitTypeIsCustom { get { return OrbitType == WeatherMakerOrbitType.CustomOrthographic || OrbitType == WeatherMakerOrbitType.CustomPerspective; } }

        /// <summary>
        /// Whether the orbit type is perspective (true) or orthographic (false)
        /// </summary>
        public bool OrbitTypeIsPerspective { get { return OrbitType == WeatherMakerOrbitType.FromEarth || OrbitType == WeatherMakerOrbitType.CustomPerspective; } }

        /// <summary>
        /// Whether the object is active
        /// </summary>
        public bool IsActive
        {
            get { return gameObject.activeInHierarchy; }
        }

        /// <summary>
        /// Whether the light for this object is active. A light that is not active is not on.
        /// </summary>
        public bool LightIsActive
        {
            get { return Light.enabled && IsActive; }
        }

        /// <summary>
        /// Whether the light is on. An active light can have a light that is off.
        /// </summary>
        public bool LightIsOn
        {
            get { return LightIsActive && Light.intensity > 0.0f && LightMultiplier > 0.0f && (Light.color.r > 0.0f || Light.color.g > 0.0f || Light.color.b > 0.0f); }
        }

        /// <summary>
        /// Gets the viewport positions of the object.
        /// </summary>
        public Vector3 ViewportPosition
        {
            get; internal set;
        }

        private Vector3 prevHSV;
        private Quaternion prevRotation;

        private void CalculateDifference()
        {
            Vector3 curHSV;
            Color.RGBToHSV(Light.color, out curHSV.x, out curHSV.y, out curHSV.z);
            Quaternion curRot = Light.transform.rotation;
            float diff = Mathf.Abs(prevHSV.x - curHSV.x) + Mathf.Abs(prevHSV.y - curHSV.y) + Mathf.Abs(prevHSV.z - curHSV.z);
            diff += Mathf.Abs(prevRotation.x - curRot.x) + Mathf.Abs(prevRotation.y - curRot.y) + Mathf.Abs(prevRotation.z - curRot.z) + Mathf.Abs(prevRotation.w - curRot.w);
            diff = Mathf.Min(1.0f, diff);
            prevHSV = curHSV;
            prevRotation = curRot;
            Difference = diff * 10.0f;
        }

        private void UpdateInternal()
        {
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                // ensure it is in the list
                if (IsSun)
                {
                    WeatherMakerLightManagerScript.Instance.Moons.Remove(this);
                    if (!WeatherMakerLightManagerScript.Instance.Suns.Contains(this))
                    {
                        WeatherMakerLightManagerScript.Instance.Suns.Add(this);
                    }
                }
                else
                {
                    if (!WeatherMakerLightManagerScript.Instance.Moons.Contains(this))
                    {
                        WeatherMakerLightManagerScript.Instance.Moons.Add(this);
                    }
                    WeatherMakerLightManagerScript.Instance.Suns.Remove(this);
                }
            }
            Light = (Light == null ? GetComponent<Light>() : Light);
            Renderer = (Renderer == null ? GetComponent<Renderer>() : Renderer);
            MeshFilter = (MeshFilter == null ? GetComponent<MeshFilter>() : MeshFilter);
            Collider = (Collider == null ? GetComponent<Collider>() : Collider);

            if (Renderer != null)
            {
                Renderer.enabled = false;
            }
        }

        private void Awake()
        {
            UpdateInternal();
        }

        private void OnEnable()
        {
            UpdateInternal();
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
            Flare = GetComponent<LensFlare>();
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
        }

        private void CameraPreCull(Camera camera)
        {
            if (Flare == null || !Flare.enabled || LensFlareBlocker == null || WeatherMakerFullScreenCloudsScript.Instance == null || WeatherMakerFullScreenCloudsScript.Instance.CloudProfile == null ||
                WeatherMakerCommandBufferManagerScript.CameraStackCount > 1 || MinimumCloudCoverToBlockLensFlare <= 0.0f)
            {
                return;
            }
            LensFlareBlocker.transform.position = camera.transform.position + (transform.forward * Mathf.Min(1000.0f, camera.farClipPlane) * -0.9f);

            if (WeatherMakerFullScreenCloudsScript.CloudProbeEnabled && WeatherMakerFullScreenCloudsScript.Instance != null && FlareCloudProbeTarget != null && SystemInfo.supportsComputeShaders)
            {
                FlareCloudProbeTarget.transform.position = camera.transform.position + (transform.forward * -100000.0f);
                WeatherMakerFullScreenCloudsScript.CloudProbeResult result = WeatherMakerFullScreenCloudsScript.Instance.GetCloudProbe(camera, camera.transform, FlareCloudProbeTarget.transform);
                LensFlareBlocker.SetActive(result.DensityRaySum > MinimumCloudCoverToBlockLensFlare);
                //Debug.Log("Flare probe: " + result.DensityRayAverage);
            }
            else if (WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.CloudCoverTotal < MinimumCloudCoverToBlockLensFlare)
            {
                LensFlareBlocker.SetActive(false);
            }
            else
            {
                LensFlareBlocker.SetActive(true);
            }
        }

        /// <summary>
        /// Call from the day night cycle manager update
        /// </summary>
        public void UpdateObject()
        {
            UpdateInternal();
            if (Renderer != null && Renderer.sharedMaterial != null)
            {
                Color tintColor = TintColor;
                tintColor.a *= TintIntensity;
                Renderer.sharedMaterial.SetColor(WMS._TintColor, tintColor);
            }
            CalculateDifference();
        }

        /// <summary>
        /// Get a color for a gradient given directional light position
        /// </summary>
        /// <param name="gradient">Gradient</param>
        /// <returns>Color</returns>
        public Color GetGradientColor(Gradient gradient)
        {
            if (gradient == null)
            {
                return WeatherMakerLightManagerScript.EvaluateGradient(gradient, 1.0f);
            }
            float sunGradientLookup = GetGradientLookup();
            return WeatherMakerLightManagerScript.EvaluateGradient(gradient, sunGradientLookup);
        }

        /// <summary>
        /// Get a gradient lookup value given directional light position
        /// </summary>
        /// <returns>Value</returns>
        public float GetGradientLookup()
        {
            if (!LightIsActive)
            {
                return 1.0f;
            }

            float sunGradientLookup;
            CameraMode mode = WeatherMakerScript.ResolveCameraMode();
            if (mode == CameraMode.OrthographicXY && !OrbitTypeIsPerspective)
            {
                sunGradientLookup = transform.forward.z;
            }
            else if (mode == CameraMode.OrthographicXZ && !OrbitTypeIsPerspective)
            {
                sunGradientLookup = transform.forward.x;
            }
            else
            {
                // TODO: Support full spherical world
                sunGradientLookup = -transform.forward.y;
            }
            sunGradientLookup = ((sunGradientLookup + 1.0f) * 0.5f);
            return sunGradientLookup;
        }

        /// <summary>
        /// Get mie dot
        /// </summary>
        /// <returns>Mie dot</returns>
        public float GetSunMieDot()
        {
            return Mathf.Pow(1.0f - Vector3.Dot(Vector3.up, -transform.forward), 5.0f);
        }
    }

    /// <summary>
    /// Celestial object light modes
    /// </summary>
    public enum WeatherMakerCelestialObjectLightMode
    {
        /// <summary>
        /// No lighting, external script must set the light
        /// </summary>
        None = 0,

        /// <summary>
        /// Basic lighting, just set to base light intensity
        /// </summary>
        Basic = 1,

        /// <summary>
        /// Standard lighting, take into account object position, moon phases, etc.
        /// </summary>
        Standard = 2
    }
}
