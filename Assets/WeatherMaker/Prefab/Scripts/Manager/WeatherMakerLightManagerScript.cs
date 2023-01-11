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

#define WEATHER_MAKER_TRACK_LIGHT_CHANGES

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Auto find light mode
    /// </summary>
    public enum AutoFindLightsMode
    {
        /// <summary>
        /// No auto light find
        /// </summary>
        None,

        /// <summary>
        /// Find lights at game start
        /// </summary>
        Once,

        /// <summary>
        /// Find lights every frame
        /// </summary>
        EveryFrame
    }

    /// <summary>
    /// Orbit type
    /// </summary>
    public enum WeatherMakerOrbitType
    {
        /// <summary>
        /// Orbit as viewed from Earth (3D)
        /// </summary>
        FromEarth = 0,

        /// <summary>
        /// Orthographic xy plane
        /// </summary>
        OrthographicXY = 1,

        /// <summary>
        /// Orthographic xz plane
        /// </summary>
        OrthographicXZ = 2,

        /// <summary>
        /// Orbit is controlled by external script (orthographic)
        /// </summary>
        CustomOrthographic = 42,

        /// <summary>
        /// Orbit is controlled by external script (perspective)
        /// </summary>
        CustomPerspective = 43
    }

    /// <summary>
    /// Blur shader type
    /// </summary>
    public enum BlurShaderType
    {
        /// <summary>
        /// No blur
        /// </summary>
        None,

        /// <summary>
        /// Gaussian blur 7 tap
        /// </summary>
        GaussianBlur7,

        /// <summary>
        /// Gaussian blur 17 tap
        /// </summary>
        GaussianBlur17,

        /// <summary>
        /// Bilateral blur
        /// </summary>
        Bilateral
    }

    /// <summary>
    /// Manages lights in world space for use in shaders - you do not need to add the directional light to the Lights list, it is done automatically
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerLightManagerScript : MonoBehaviour
    {
        /// <summary>Whether to find all lights in the scene automatically if no Lights were added programatically. If none, you must manually add / remove lights using the AutoAddLights property. To ensure correct behavior, do not change in script, set it once in the inspector and leave it. If this is set to EveryFrame, AddLight and RemoveLight do nothing.</summary>
        [Header("Lights")]
        [Tooltip("Whether to find all lights in the scene automatically if no Lights were added programatically. If none, you must manually add / remove lights using the AutoAddLights property. " +
            "To ensure correct behavior, do not change in script, set it once in the inspector and leave it. If this is set to EveryFrame, AddLight and RemoveLight do nothing.")]
        public AutoFindLightsMode AutoFindLights;

        /// <summary>A list of lights to automatically add to the light manager. Only used if AutoFindLights is false.</summary>
        [Tooltip("A list of lights to automatically add to the light manager. Only used if AutoFindLights is false.")]
        public List<Light> AutoAddLights;

        /// <summary>A list of lights to always ignore if AutoFindLights is being used.</summary>
        [Tooltip("A list of lights to always ignore if AutoFindLights is being used.")]
        public List<Light> IgnoreLights;

        /// <summary>How often to update shader state in seconds for each object (camera, collider, etc.), 0 for every frame.</summary>
        [Tooltip("How often to update shader state in seconds for each object (camera, collider, etc.), 0 for every frame.")]
        [Range(0.0f, 1.0f)]
        public float ShaderUpdateInterval = (30.0f / 1000.0f); // 30 fps

        /// <summary>Set this to a custom screen space shadow shader, by default this is the weather maker integrated screen space shadow shader.</summary>
        [Tooltip("Set this to a custom screen space shadow shader, by default this is the weather maker integrated screen space shadow shader.")]
        public Shader ScreenSpaceShadowShader;

        private readonly Dictionary<Component, float> shaderUpdateCounter = new Dictionary<Component, float>();
        private Component lastShaderUpdateComponent;

        /// <summary>Spot light quadratic attenuation.</summary>
        [Tooltip("Spot light quadratic attenuation.")]
        [Range(0.0f, 1000.0f)]
        public float SpotLightQuadraticAttenuation = 25.0f;

        /// <summary>Point light quadratic attenuation.</summary>
        [Tooltip("Point light quadratic attenuation.")]
        [Range(0.0f, 1000.0f)]
        public float PointLightQuadraticAttenuation = 25.0f;

        /// <summary>Area light quadratic attenuation. Set to 0 to turn off all area lights.</summary>
        [Tooltip("Area light quadratic attenuation. Set to 0 to turn off all area lights.")]
        [Range(0.0f, 1000.0f)]
        public float AreaLightQuadraticAttenuation = 50.0f;

        /// <summary>Multiplier for area light. Spreads and fades light out over x and y size.</summary>
        [Tooltip("Multiplier for area light. Spreads and fades light out over x and y size.")]
        [Range(1.0f, 20.0f)]
        public float AreaLightAreaMultiplier = 10.0f;

        /// <summary>Falloff for area light, as light moves away from center it falls off more as this increases.</summary>
        [Tooltip("Falloff for area light, as light moves away from center it falls off more as this increases.")]
        [Range(0.0f, 3.0f)]
        public float AreaLightFalloff = 0.5f;

        /// <summary>How intense is the scatter of directional light in the fog.</summary>
        [Tooltip("How intense is the scatter of directional light in the fog.")]
        [Range(0.0f, 100.0f)]
        public float FogDirectionalLightScatterIntensity = 2.0f;

        /// <summary>How quickly fog point lights falloff from the center radius. High values fall-off more.</summary>
        [Tooltip("How quickly fog point lights falloff from the center radius. High values fall-off more.")]
        [Range(0.0f, 4.0f)]
        public float FogSpotLightRadiusFalloff = 1.2f;

        /// <summary>How much the sun reduces fog lights. As sun intensity approaches 1, fog light intensity is reduced by this value.</summary>
        [Tooltip("How much the sun reduces fog lights. As sun intensity approaches 1, fog light intensity is reduced by this value.")]
        [Range(0.0f, 1.0f)]
        public float FogLightSunIntensityReducer = 0.8f;

        /// <summary>Noise texture for fog and other 3D effects.</summary>
        [Header("Noise textures")]
        [Tooltip("Noise texture for fog and other 3D effects.")]
        public Texture3D NoiseTexture3D;

        /// <summary>Blue noise texture, useful for dithering and eliminating banding</summary>
        [Tooltip("Blue noise texture, useful for dithering and eliminating banding")]
        public Texture2D BlueNoiseTexture;

        /// <summary>
        /// Dithering texture (8x8)
        /// </summary>
        public static Texture2D DitherTextureInstance { get; private set; }

        private WeatherMakerCelestialObjectScript sunPerspective;
        /// <summary>
        /// First perspective sun in light list
        /// </summary>
        public WeatherMakerCelestialObjectScript SunPerspective
        {
            get
            {
                if (sunPerspective == null)
                {
                    foreach (WeatherMakerCelestialObjectScript sun in Suns)
                    {
                        if (sun != null && sun.OrbitTypeIsPerspective)
                        {
                            sunPerspective = sun;
                            break;
                        }
                    }
                }
                return sunPerspective;
            }
        }

        private WeatherMakerCelestialObjectScript sunOrthographic;
        /// <summary>
        /// First orthographic sun in light list
        /// </summary>
        public WeatherMakerCelestialObjectScript SunOrthographic
        {
            get
            {
                if (sunOrthographic == null)
                {
                    foreach (WeatherMakerCelestialObjectScript sun in Suns)
                    {
                        if (sun != null && !sun.OrbitTypeIsPerspective)
                        {
                            sunOrthographic = sun;
                            break;
                        }
                    }
                }
                return sunOrthographic;
            }
        }

        private readonly List<WeatherMakerCelestialObjectScript> suns = new List<WeatherMakerCelestialObjectScript>();
        private readonly List<WeatherMakerCelestialObjectScript> moons = new List<WeatherMakerCelestialObjectScript>();

        /// <summary>
        /// Suns
        /// </summary>
        public IList<WeatherMakerCelestialObjectScript> Suns { get { return suns; } }

        /// <summary>
        /// Moons
        /// </summary>
        public IList<WeatherMakerCelestialObjectScript> Moons { get { return moons; } }

        /// <summary>The texture name for shaders to access the screen space shadow map, null/empty to not use screen space shadows</summary>
        [Header("Shadows")]
        [Tooltip("The texture name for shaders to access the screen space shadow map, null/empty to not use screen space shadows")]
        public string ScreenSpaceShadowsRenderTextureName = "_WeatherMakerShadowMapSSTexture";

        /// <summary>
        /// Directional light intensity multipliers - all are applied to the final directional light intensities
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> DirectionalLightIntensityMultipliers = new Dictionary<string, float>();

        /// <summary>
        /// Directional light shadow intensity multipliers - all are applied to the final directional light shadow intensities
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> DirectionalLightShadowIntensityMultipliers = new Dictionary<string, float>();

        /// <summary>
        /// The planes of the current camera view frustum
        /// </summary>
        [HideInInspector]
        public readonly Plane[] CurrentCameraFrustumPlanes = new Plane[6];

        /// <summary>
        /// Current camera
        /// </summary>
        [HideInInspector]
        public Camera CurrentCamera { get; private set; }

        /// <summary>
        /// The corners of the current camera view frustum
        /// </summary>
        public readonly Vector3[] CurrentCameraFrustumCorners = new Vector3[8];
        private readonly Vector3[] currentCameraFrustumCornersNear = new Vector3[4];

        /// <summary>
        /// The current bounds if checking a collider and not a camera
        /// </summary>
        [HideInInspector]
        public Bounds CurrentBounds;

        /// <summary>
        /// Null zones - this is handled automatically as null zone scripts are added
        /// </summary>
        public readonly List<WeatherMakerNullZoneScript> NullZones = new List<WeatherMakerNullZoneScript>();

        private readonly Vector4[] nullZoneArrayMin = new Vector4[MaximumNullZones];
        private readonly Vector4[] nullZoneArrayMax = new Vector4[MaximumNullZones];
        private readonly Vector4[] nullZoneArrayCenter = new Vector4[MaximumNullZones];
        private readonly Vector4[] nullZoneArrayQuaternion = new Vector4[MaximumNullZones];
        private readonly Vector4[] nullZoneArrayParams = new Vector4[MaximumNullZones];

        /// <summary>
        /// Global shared copy of NoiseTexture3D
        /// </summary>
        public static Texture3D NoiseTexture3DInstance { get; private set; }

        /// <summary>
        /// Max number of null zones - the n closest will be sent to shaders.
        /// </summary>
        public const int MaximumNullZones = 16;

        /// <summary>
        /// Maximum number of lights to send to the Weather Maker shaders - reduce if you are having performance problems
        /// This should match the constant 'MAX_LIGHT_COUNT' in WeatherMakerLightShaderInclude.cginc
        /// </summary>
        public const int MaximumLightCount = 16;

        /// <summary>
        /// Max number of moons supported. This should match the constant in WeatherMakerLightShaderInclude.cginc.
        /// </summary>
        public const int MaxMoonCount = 8;

        // dir lights
        private readonly Vector4[] lightPositionsDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightDirectionsDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightColorsDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightViewportPositionsDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPowerDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightQuaternionDir = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightVar1Dir = new Vector4[MaximumLightCount];

        // point lights
        private readonly Vector4[] lightPositionsPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightDirectionsPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightColorsPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightAttenPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightViewportPositionsPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPowerPoint = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightVar1Point = new Vector4[MaximumLightCount];

        // spot lights
        private readonly Vector4[] lightPositionsSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightDirectionsSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightEndsSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightColorsSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightAttenSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightViewportPositionsSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPowerSpot = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightVar1Spot = new Vector4[MaximumLightCount];

        // area lights
        private readonly Vector4[] lightPositionsArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPositionsEndArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPositionsMinArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPositionsMaxArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightRotationArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightDirectionArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightColorsArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightAttenArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightViewportPositionsArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightPowerArea = new Vector4[MaximumLightCount];
        private readonly Vector4[] lightVar1Area = new Vector4[MaximumLightCount];

        // unused, but needed for GetLightProperties when result is unused
        private Vector4 tempVec = Vector3.zero;

        /// <summary>
        /// A list of all the lights, sorted by importance of light
        /// </summary>
        private readonly List<LightState> lights = new List<LightState>();

        private bool autoFoundLights;
        private Vector3 currentLightSortPosition;

        // state for intersection test
        private delegate bool IntersectsFuncDelegate(ref Bounds bounds);
        private IntersectsFuncDelegate intersectsFuncCurrentCamera;
        private IntersectsFuncDelegate intersectsFuncCurrentBounds;
        private IntersectsFuncDelegate intersectsFunc;

        //private Vector2 lastResolution;

        private void NormalizePlane(ref Plane plane)
        {
            float length = plane.normal.magnitude;
            plane.normal /= length;
            plane.distance /= length;
        }

        private bool IntersectsFunctionCurrentCamera(ref Bounds bounds)
        {
            return GeometryUtility.TestPlanesAABB(CurrentCameraFrustumPlanes, bounds);
        }

        private bool IntersectsFunctionCurrentBounds(ref Bounds bounds)
        {
            return CurrentBounds.Intersects(bounds);
        }

        private bool PointLightIntersect(Light light)
        {
            float range = light.range + 1.0f;
            Bounds lightBounds = new Bounds { center = light.transform.position, extents = new Vector3(range, range, range) };
            return intersectsFunc(ref lightBounds);
        }

        private bool SpotLightIntersect(Light light)
        {
            float radius = light.range * Mathf.Tan(0.5f * light.spotAngle * Mathf.Deg2Rad);
            float h = Mathf.Sqrt((light.range * light.range) + (radius * radius)) + 1.0f;
            Bounds lightBounds = new Bounds { center = light.transform.position + (light.transform.forward * h * 0.5f), extents = new Vector3(h, h, h) };
            return intersectsFunc(ref lightBounds);
        }

        private bool AreaLightIntersect(ref Bounds lightBounds)
        {
            return intersectsFunc(ref lightBounds);
        }

        private bool IntersectLight(Light light)
        {
            switch (light.type)
            {
                case LightType.Area:
                {
                    Vector3 pos = light.transform.position;
                    Vector3 dir = light.transform.forward;
                    Vector2 areaSize = light.transform.lossyScale * AreaLightAreaMultiplier;
                    float maxValue = Mathf.Max(areaSize.x, areaSize.y);
                    maxValue = Mathf.Max(maxValue, light.range);
                    Bounds bounds = new Bounds(pos + (dir * light.range * 0.5f), Vector3.one * maxValue);
                    return AreaLightIntersect(ref bounds);
                }

                case LightType.Point:
                    return PointLightIntersect(light);

                case LightType.Spot:
                    return SpotLightIntersect(light);

                default:
                    return true;
            }
        }

        private bool ProcessLightProperties
        (
            LightState lightState,
            Camera camera,
            ref Vector4 pos,
            ref Vector4 pos2,
            ref Vector4 pos3,
            ref Vector4 atten,
            ref Vector4 color,
            ref Vector4 dir,
            ref Vector4 dir2,
            ref Vector4 end,
            ref Vector4 viewportPos,
            ref Vector4 lightPower,
            ref Vector4 lightVar1
        )
        {
            if (lightState == null)
            {
                return false;
            }
            Light light = lightState.Light;
            if (light == null || !light.enabled || light.color.a <= 0.001f || light.intensity <= 0.001f || light.range <= 0.001f ||
                !IntersectLight(light))
            {
                return false;
            }
            lightVar1.x = lightState.Update(camera);
            SetShaderViewportPosition(light, camera, ref viewportPos, ref lightVar1);
            color = new Vector4(light.color.r, light.color.g, light.color.b, light.intensity);
            lightPower = Vector4.zero;
            pos2 = lightPower;
            dir2 = lightPower;

            switch (light.type)
            {
                case LightType.Directional:
                {
                    pos = -light.transform.forward;
                    pos.w = -1.0f;
                    WeatherMakerCelestialObjectScript obj = light.GetComponent<WeatherMakerCelestialObjectScript>();
                    float isSun;
                    if (obj == null)
                    {
                        isSun = 1.0f;
                    }
                    else
                    {
                        if (obj.OrbitTypeIsPerspective != (camera == null || !camera.orthographic))
                        {
                            return false;
                        }
                        isSun = (obj.IsSun ? 1.0f : 0.0f);
                        lightVar1.z = obj.ShaftMultiplier;
                        float intensity = light.intensity;
                        lightVar1.w = Mathf.Clamp(intensity * Mathf.Pow(1.0f - Mathf.Abs(pos.y), obj.HorizonMultiplier.z) * obj.HorizonMultiplier.x, intensity, intensity + obj.HorizonMultiplier.y);
                    }
                    dir = light.transform.forward;
                    dir.w = isSun;
                    end = Vector4.zero;
                    atten = new Vector4(-1.0f, 1.0f, 0.0f, 0.0f);
                    if (light.shadows == LightShadows.None)
                    {
                        if (obj == null)
                        {
                            lightPower = new Vector4(1.0f, 1.0f, light.shadowStrength, 1.0f);
                        }
                        else
                        {
                            lightPower = new Vector4(obj.LightPower, obj.LightMultiplier, light.shadowStrength, 1.0f);
                        }
                    }
                    else if (obj == null)
                    {
                        lightPower = new Vector4(1.0f, 1.0f, light.shadowStrength, 1.0f - light.shadowStrength);
                    }
                    else
                    {
                        lightPower = new Vector4(obj.LightPower, obj.LightMultiplier, light.shadowStrength, 1.0f - light.shadowStrength);
                    }
                    return true;
                }

                case LightType.Spot:
                {
                    float radius = light.range * Mathf.Tan(0.5f * light.spotAngle * Mathf.Deg2Rad);
                    end = light.transform.position + (light.transform.forward * light.range); // center of cone base
                    float rangeSquared = Mathf.Sqrt((radius * radius) + (light.range * light.range));
                    end.w = rangeSquared * rangeSquared; // slant length squared
                    rangeSquared = light.range * light.range;
                    float outerCutOff = Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad);
                    float cutOff = 1.0f / (Mathf.Cos(light.spotAngle * 0.25f * Mathf.Deg2Rad) - outerCutOff);
                    atten = new Vector4(outerCutOff, cutOff, SpotLightQuadraticAttenuation / rangeSquared, 1.0f / rangeSquared);
                    pos = light.transform.position; // apex
                    pos.w = Mathf.Pow(light.spotAngle * Mathf.Deg2Rad / Mathf.PI, 0.5f); // falloff resistor, thinner angles do not fall off at edges
                    dir = light.transform.forward; // direction cone is facing from apex
                    dir.w = radius * radius; // radius at base squared
                    return true;
                }

                case LightType.Point:
                {
                    if (!PointLightIntersect(light))
                    {
                        return false;
                    }

                    float rangeSquared = light.range * light.range;
                    pos = light.transform.position;
                    pos.w = rangeSquared;
                    dir = light.transform.position.normalized;
                    dir.w = light.range;
                    end = Vector4.zero;
                    atten = new Vector4(-1.0f, 1.0f, PointLightQuadraticAttenuation / rangeSquared, 1.0f / rangeSquared);
                    return true;
                }

                case LightType.Area:
                {
                    if (AreaLightQuadraticAttenuation > 0.0f)
                    {
                        float range = light.range;
                        float rangeSquared = range * range;
                        dir2 = light.transform.forward;
                        dir2.w = 0.0f;
                        pos = light.transform.position;
                        pos2 = (Vector3)pos + ((Vector3)dir2 * range);
                        Quaternion rot = light.transform.rotation;
                        Vector2 areaSize = light.transform.lossyScale * AreaLightAreaMultiplier;
                        Vector3 minOffset = (Vector3)pos + (new Vector3(-0.5f * areaSize.x, -0.5f * areaSize.y, 0.0f));
                        Vector3 maxOffset = (Vector3)pos + (new Vector3(0.5f * areaSize.x, 0.5f * areaSize.y, range));
                        pos.w = rangeSquared;
                        dir = new Vector4(rot.x, rot.y, rot.z, rot.w);
                        pos3 = minOffset - (Vector3)pos;
                        pos3.w = 0.0f;
                        end = maxOffset - (Vector3)pos;
                        end.w = 0.0f;
                        float attenAvg = (areaSize.x + areaSize.y) * 0.5f;
                        float radiusSquared = (attenAvg * AreaLightFalloff);
                        radiusSquared *= radiusSquared;
                        atten = new Vector4(1.0f / radiusSquared, attenAvg, AreaLightQuadraticAttenuation / rangeSquared, 1.0f / rangeSquared);
                        return true;
                    } break;
                }
            }
            return false;
        }

        private System.Comparison<LightState> lightSorterReference;
        private int LightSorter(LightState lightState1, LightState lightState2)
        {
            Light light1 = lightState1.Light;
            Light light2 = lightState2.Light;
            int compare = 0;

            if (light1 == null)
            {
                return 1;
            }
            else if (light2 == null)
            {
                return -1;
            }
            else if (light1 == light2)
            {
                return compare;
            }
            compare = light1.type.CompareTo(light2.type);
            if (compare == 0)
            {
                if (light1.type == LightType.Directional)
                {
                    WeatherMakerCelestialObjectScript obj1 = light1.GetComponent<WeatherMakerCelestialObjectScript>();
                    WeatherMakerCelestialObjectScript obj2 = light2.GetComponent<WeatherMakerCelestialObjectScript>();
                    if (obj1 != null && obj2 != null)
                    {
                        // sort perspective dir lights first
                        int orbVal1 = (obj1.OrbitTypeIsPerspective ? 0 : 1);
                        int orbVal2 = (obj2.OrbitTypeIsPerspective ? 0 : 1);
                        compare = orbVal1.CompareTo(orbVal2);
                    }
                    if (compare == 0)
                    {
                        compare = light2.intensity.CompareTo(light1.intensity);
                        if (compare == 0)
                        {
                            compare = light2.shadows.CompareTo(light1.shadows);
                        }
                    }
                }
                else
                {
                    // compare by distance, then by intensity
                    float mag1 = Mathf.Max(0.0f, Vector3.Distance(light1.transform.position, currentLightSortPosition) - light1.range);
                    float mag2 = Mathf.Max(0.0f, Vector3.Distance(light2.transform.position, currentLightSortPosition) - light2.range);
                    compare = mag1.CompareTo(mag2);
                    if (compare == 0)
                    {
                        compare = light2.intensity.CompareTo(light1.intensity);
                    }
                }
            }
            return compare;
        }

        private System.Comparison<WeatherMakerNullZoneScript> nullZoneSorterReference;
        private int NullZoneSorter(WeatherMakerNullZoneScript b1, WeatherMakerNullZoneScript b2)
        {
            // sort by distance from camera
            float d1 = Vector3.SqrMagnitude(b1.bounds.center - currentLightSortPosition);
            float d2 = Vector3.SqrMagnitude(b2.bounds.center - currentLightSortPosition);
            return d1.CompareTo(d2);
        }

        private void SetLightsByTypeToShader(Camera camera, WeatherMakerShaderPropertiesScript m)
        {
            int dirLightCount = 0;
            int pointLightCount = 0;
            int spotLightCount = 0;
            int areaLightCount = 0;

            // ensure first light is cleared of any properties that might linger outside of light count checks
            lightColorsDir[0] = lightColorsPoint[0] = lightColorsSpot[0] = lightColorsArea[0] = Vector4.zero;
            lightVar1Dir[0] = lightVar1Point[0] = lightVar1Spot[0] = lightVar1Area[0] = Vector4.zero;

            for (int i = 0; i < lights.Count; i++)
            {
                LightState light = lights[i];
                if (light == null || light.Light == null)
                {
                    lights.RemoveAt(i--);
                    continue;
                }

                switch (light.Light.type)
                {
                    case LightType.Directional:
                        if (dirLightCount < MaximumLightCount && ProcessLightProperties(light, camera, ref lightPositionsDir[dirLightCount], ref tempVec, ref tempVec, ref tempVec,
                            ref lightColorsDir[dirLightCount], ref lightDirectionsDir[dirLightCount], ref tempVec, ref tempVec, ref lightViewportPositionsDir[dirLightCount],
                            ref lightPowerDir[dirLightCount], ref lightVar1Dir[dirLightCount]))
                        {
                            Quaternion rot = light.Light.transform.rotation;
                            lightQuaternionDir[dirLightCount] = new Vector4(rot.x, rot.y, rot.z, rot.w);
                            dirLightCount++;
                        }
                        break;

                    case LightType.Point:
                        if (pointLightCount < MaximumLightCount && ProcessLightProperties(light, camera, ref lightPositionsPoint[pointLightCount], ref tempVec, ref tempVec,
                            ref lightAttenPoint[pointLightCount], ref lightColorsPoint[pointLightCount], ref lightDirectionsPoint[pointLightCount], ref tempVec, ref tempVec,
                            ref lightViewportPositionsPoint[pointLightCount], ref lightPowerPoint[pointLightCount], ref lightVar1Point[pointLightCount]))
                        {
                            pointLightCount++;
                        }
                        break;

                    case LightType.Spot:
                        if (spotLightCount < MaximumLightCount && ProcessLightProperties(light, camera, ref lightPositionsSpot[spotLightCount], ref tempVec, ref tempVec,
                            ref lightAttenSpot[spotLightCount], ref lightColorsSpot[spotLightCount], ref lightDirectionsSpot[spotLightCount], ref tempVec, ref lightEndsSpot[spotLightCount],
                            ref lightViewportPositionsSpot[spotLightCount], ref lightPowerSpot[spotLightCount], ref lightVar1Spot[spotLightCount]))
                        {
                            spotLightCount++;
                        }
                        break;

                    case LightType.Area:
                        if (areaLightCount < MaximumLightCount && ProcessLightProperties(light, camera, ref lightPositionsArea[areaLightCount], ref lightPositionsEndArea[areaLightCount],
                            ref lightPositionsMinArea[areaLightCount], ref lightAttenArea[areaLightCount], ref lightColorsArea[areaLightCount], ref lightRotationArea[areaLightCount],
                            ref lightDirectionArea[areaLightCount], ref lightPositionsMaxArea[areaLightCount], ref lightViewportPositionsArea[areaLightCount],
                            ref lightPowerArea[areaLightCount], ref lightVar1Area[areaLightCount]))
                        {
                            areaLightCount++;
                        }
                        break;

                    default:
                        break;
                }
            }

            // dir lights
            m.SetInt(WMS._WeatherMakerDirLightCount, dirLightCount);
            m.SetVectorArray(WMS._WeatherMakerDirLightPosition, lightPositionsDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightDirection, lightDirectionsDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightColor, lightColorsDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightViewportPosition, lightViewportPositionsDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightPower, lightPowerDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightQuaternion, lightQuaternionDir);
            m.SetVectorArray(WMS._WeatherMakerDirLightVar1, lightVar1Dir);

            // point lights
            m.SetInt(WMS._WeatherMakerPointLightCount, pointLightCount);
            m.SetVectorArray(WMS._WeatherMakerPointLightPosition, lightPositionsPoint);
            m.SetVectorArray(WMS._WeatherMakerPointLightDirection, lightDirectionsPoint);
            m.SetVectorArray(WMS._WeatherMakerPointLightColor, lightColorsPoint);
            m.SetVectorArray(WMS._WeatherMakerPointLightAtten, lightAttenPoint);
            m.SetVectorArray(WMS._WeatherMakerPointLightViewportPosition, lightViewportPositionsPoint);
            m.SetVectorArray(WMS._WeatherMakerPointLightVar1, lightVar1Point);

            // spot lights
            m.SetInt(WMS._WeatherMakerSpotLightCount, spotLightCount);
            m.SetVectorArray(WMS._WeatherMakerSpotLightPosition, lightPositionsSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightColor, lightColorsSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightAtten, lightAttenSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightDirection, lightDirectionsSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightSpotEnd, lightEndsSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightViewportPosition, lightViewportPositionsSpot);
            m.SetVectorArray(WMS._WeatherMakerSpotLightVar1, lightVar1Spot);

            // area lights
            m.SetInt(WMS._WeatherMakerAreaLightCount, areaLightCount);
            m.SetVectorArray(WMS._WeatherMakerAreaLightPosition, lightPositionsArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightPositionEnd, lightPositionsEndArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightMinPosition, lightPositionsMinArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightMaxPosition, lightPositionsMaxArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightColor, lightColorsArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightAtten, lightAttenArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightRotation, lightRotationArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightDirection, lightDirectionArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightViewportPosition, lightViewportPositionsArea);
            m.SetVectorArray(WMS._WeatherMakerAreaLightVar1, lightVar1Area);
        }

        private void CleanupCelestialObjects()
        {
            for (int i = suns.Count - 1; i >= 0; i--)
            {
                if (suns[i] == null || !suns[i].enabled)
                {
                    suns.RemoveAt(i);
                }
            }
            for (int i = 0; i < moons.Count; i++)
            {
                if (moons[i] == null || !moons[i].enabled)
                {
                    moons.RemoveAt(i);
                }
            }
        }

        private void CleanupLights()
        {
            for (int i = lights.Count - 1; i >= 0; i--)
            {
                if (lights[i] == null)
                {
                    lights.RemoveAt(i);
                }
            }
            if (Application.isPlaying)
            {
                if (AutoAddLights != null)
                {
                    for (int i = AutoAddLights.Count - 1; i >= 0; i--)
                    {
                        if (AutoAddLights[i] == null)
                        {
                            AutoAddLights.RemoveAt(i);
                        }
                    }
                }
                if (IgnoreLights != null)
                {
                    for (int i = IgnoreLights.Count - 1; i >= 0; i--)
                    {
                        if (IgnoreLights[i] == null)
                        {
                            IgnoreLights.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void UpdateAllLights()
        {
            // if no user lights specified, find all the lights in the scene and sort them
            if ((AutoFindLights == AutoFindLightsMode.Once && !autoFoundLights) || AutoFindLights == AutoFindLightsMode.EveryFrame)
            {
                autoFoundLights = true;
                Light[] allLights = GameObject.FindObjectsOfType<Light>();
                lights.Clear();
                foreach (Light light in allLights)
                {
                    if (light != null && light.enabled && light.intensity > 0.0f && light.color.a > 0.0f && (IgnoreLights == null || !IgnoreLights.Contains(light)))
                    {
                        lights.Add(GetOrCreateLightState(light));
                    }
                }
            }
            else
            {
                if (Suns != null)
                {
                    foreach (WeatherMakerCelestialObjectScript sun in Suns)
                    {
                        if (sun != null)
                        {
                            // add the sun if it is on, else remove it
                            if (sun.LightIsOn)
                            {
                                AddLight(sun.Light);
                            }
                            else
                            {
                                RemoveLight(sun.Light);
                            }
                        }
                    }
                }
                if (Moons != null)
                {
                    // add each moon if it is on, else remove it
                    foreach (WeatherMakerCelestialObjectScript moon in Moons)
                    {
                        if (moon != null)
                        {
                            if (moon.LightIsOn)
                            {
                                AddLight(moon.Light);
                            }
                            else
                            {
                                RemoveLight(moon.Light);
                            }
                        }
                    }
                }

                if (AutoAddLights != null)
                {
                    // add each auto-add light if it is on, else remove it
                    for (int i = AutoAddLights.Count - 1; i >= 0; i--)
                    {
                        Light light = AutoAddLights[i];
                        if (light == null)
                        {
                            if (Application.isPlaying)
                            {
                                AutoAddLights.RemoveAt(i);
                            }
                        }
                        else if (light.intensity == 0.0f || !light.enabled || !light.gameObject.activeInHierarchy)
                        {
                            RemoveLight(light);
                        }
                        else
                        {
                            AddLight(light);
                        }
                    }
                }
            }
        }

        private void SetPerspectiveOrthographicSun()
        {
            bool setPerspective = false;
            bool setOrthographic = false;

            foreach (LightState light in lights)
            {
                if (light != null && light.Light != null && light.Light.type == LightType.Directional)
                {
                    WeatherMakerCelestialObjectScript sun = light.Light.GetComponent<WeatherMakerCelestialObjectScript>();
                    if (sun != null && sun.IsSun)
                    {
                        if (sun.OrbitTypeIsPerspective)
                        {
                            if (!setPerspective)
                            {
                                sunPerspective = sun;
                                setPerspective = true;
                            }
                        }
                        else if (!setOrthographic)
                        {
                            sunOrthographic = sun;
                            setOrthographic = true;
                        }
                    }
                }
            }
        }

        private void UpdateNullZones(WeatherMakerShaderPropertiesScript m)
        {
            int nullZoneCount = 0;

            // take out null/disabled fog zones
            for (int i = NullZones.Count - 1; i >= 0; i--)
            {
                if (NullZones[i] == null || !NullZones[i].enabled)
                {
                    NullZones.RemoveAt(i);
                }
            }

            NullZones.Sort(nullZoneSorterReference);
            for (int i = 0; i < NullZones.Count && nullZoneCount < MaximumNullZones; i++)
            {
                Bounds nullZoneBoundsInflated = NullZones[i].inflatedBounds;
                if (NullZones[i].NullZoneProfile == null || NullZones[i].NullZoneStrength < 0.001f ||
                    !WeatherMakerGeometryUtility.BoxIntersectsFrustum(CurrentCamera, CurrentCameraFrustumPlanes, CurrentCameraFrustumCorners, nullZoneBoundsInflated))
                {
                    continue;
                }

                Bounds nullZoneBounds = NullZones[i].bounds;
                int mask = NullZones[i].CurrentMask;

                if (intersectsFunc(ref nullZoneBoundsInflated))
                {
                    float fade = NullZones[i].CurrentFade;
                    float strength = NullZones[i].NullZoneStrength;
                    Quaternion r = NullZones[i].transform.rotation;
                    Vector4 center = nullZoneBounds.center;
                    float zoneType;
                    if (NullZones[i].SphereCollider != null)
                    {
                        // sphere
                        zoneType = 2.0f;

                        // radius squared
                        center.w = Mathf.Pow(NullZones[i].SphereCollider.bounds.extents.x, 2.0f);
}
                    else if (r == Quaternion.identity)
                    {
                        // box, no rotation
                        zoneType = 0.0f;
                    }
                    else
                    {
                        // it will be up to the shaders to detect that zone type is 1.0 and
                        // perform quaternion rotation on any ray cast, point intersect, etc.
                        zoneType = 1.0f;

                        // recompute bounds with no rotation - Unity bounds is the largest box that will contain,
                        // not what we want here, we want an AABB with no rotation centered on 0,0,0.
                        NullZones[i].transform.rotation = Quaternion.identity;
                        nullZoneBounds = NullZones[i].BoxCollider.bounds;
                        NullZones[i].transform.rotation = r;
                        nullZoneBounds.center = Vector3.zero;
                    }

                    nullZoneArrayMin[nullZoneCount] = nullZoneBounds.min;
                    nullZoneArrayMax[nullZoneCount] = nullZoneBounds.max;

                    if (NullZones[i].SphereCollider != null)
                    {
                        // radius
                        nullZoneArrayMax[nullZoneCount].w = NullZones[i].SphereCollider.bounds.extents.x;
                    }

                    nullZoneArrayCenter[nullZoneCount] = center;
                    nullZoneArrayQuaternion[nullZoneCount] = new Vector4(r.x, r.y, r.z, r.w);
                    nullZoneArrayParams[nullZoneCount] = new Vector4(strength, mask, fade, zoneType);
                    nullZoneCount++;
                }
            }
            m.SetInt(WMS._NullZoneCount, nullZoneCount);
            m.SetVectorArray(WMS._NullZonesMin, nullZoneArrayMin);
            m.SetVectorArray(WMS._NullZonesMax, nullZoneArrayMax);
            m.SetVectorArray(WMS._NullZonesCenter, nullZoneArrayCenter);
            m.SetVectorArray(WMS._NullZonesParams, nullZoneArrayParams);
            m.SetVectorArray(WMS._NullZonesQuaternion, nullZoneArrayQuaternion);
        }

        private void CreateDitherTexture()
        {
            if (DitherTextureInstance != null)
            {
                return;
            }

#if DITHER_4_4

            int size = 4;

#else

            int size = 8;

#endif

            DitherTextureInstance = new Texture2D(size, size, TextureFormat.Alpha8, false, true)
            {
                filterMode = FilterMode.Point,
                name = "WeatherMakerDitherTexture"
            };
            Color32[] c = new Color32[size * size];

            byte b;

#if DITHER_4_4

            b = (byte)(0.0f / 16.0f * 255); c[0] = new Color32(b, b, b, b);
            b = (byte)(8.0f / 16.0f * 255); c[1] = new Color32(b, b, b, b);
            b = (byte)(2.0f / 16.0f * 255); c[2] = new Color32(b, b, b, b);
            b = (byte)(10.0f / 16.0f * 255); c[3] = new Color32(b, b, b, b);

            b = (byte)(12.0f / 16.0f * 255); c[4] = new Color32(b, b, b, b);
            b = (byte)(4.0f / 16.0f * 255); c[5] = new Color32(b, b, b, b);
            b = (byte)(14.0f / 16.0f * 255); c[6] = new Color32(b, b, b, b);
            b = (byte)(6.0f / 16.0f * 255); c[7] = new Color32(b, b, b, b);

            b = (byte)(3.0f / 16.0f * 255); c[8] = new Color32(b, b, b, b);
            b = (byte)(11.0f / 16.0f * 255); c[9] = new Color32(b, b, b, b);
            b = (byte)(1.0f / 16.0f * 255); c[10] = new Color32(b, b, b, b);
            b = (byte)(9.0f / 16.0f * 255); c[11] = new Color32(b, b, b, b);

            b = (byte)(15.0f / 16.0f * 255); c[12] = new Color32(b, b, b, b);
            b = (byte)(7.0f / 16.0f * 255); c[13] = new Color32(b, b, b, b);
            b = (byte)(13.0f / 16.0f * 255); c[14] = new Color32(b, b, b, b);
            b = (byte)(5.0f / 16.0f * 255); c[15] = new Color32(b, b, b, b);

#else

            int i = 0;
            b = (byte)(1.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(49.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(13.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(61.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(4.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(52.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(16.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(64.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(33.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(17.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(45.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(29.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(36.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(20.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(48.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(32.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(9.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(57.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(5.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(53.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(12.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(60.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(8.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(56.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(41.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(25.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(37.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(21.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(44.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(28.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(40.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(24.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(3.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(51.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(15.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(63.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(2.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(50.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(14.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(62.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(35.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(19.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(47.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(31.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(34.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(18.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(46.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(30.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(11.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(59.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(7.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(55.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(10.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(58.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(6.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(54.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(43.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(27.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(39.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(23.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(42.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(26.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(38.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(22.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
#endif

            DitherTextureInstance.SetPixels32(c);
            DitherTextureInstance.Apply();
            Shader.SetGlobalTexture(WMS._WeatherMakerDitherTexture, DitherTextureInstance);
        }

        private void SetShaderViewportPosition(Light light, Camera camera, ref Vector4 viewportPosition, ref Vector4 lightVar1)
        {
            if (camera == null || WeatherMakerScript.GetCameraType(camera) != WeatherMakerCameraType.Normal)
            {
                return;
            }

            viewportPosition = camera.WorldToViewportPoint(light.transform.position);

            // as dir light leaves viewport, fade out
            Vector2 viewportCenter = new Vector2((camera.rect.min.x + camera.rect.max.x) * 0.5f, (camera.rect.min.y + camera.rect.max.y) * 0.5f);
            float distanceFromCenterViewport = ((Vector2)viewportPosition - viewportCenter).magnitude * 0.5f;
            viewportPosition.w = light.intensity * Mathf.SmoothStep(1.0f, 0.0f, distanceFromCenterViewport);
            foreach (WeatherMakerCelestialObjectScript sun in Suns)
            {
                if (sun != null && sun.Light == light)
                {
                    sun.ViewportPosition = viewportPosition;
                    lightVar1.y = (sun.OrbitType == WeatherMakerOrbitType.FromEarth || sun.OrbitType == WeatherMakerOrbitType.CustomPerspective ? 0.0f : 1.0f);
                    return;
                }
            }
            foreach (WeatherMakerCelestialObjectScript moon in Moons)
            {
                if (moon != null && moon.Light == light)
                {
                    moon.ViewportPosition = viewportPosition;
                    lightVar1.y = (moon.OrbitType == WeatherMakerOrbitType.FromEarth || moon.OrbitType == WeatherMakerOrbitType.CustomPerspective ? 0.0f : 1.0f);
                    break;
                }
            }
        }

        private float WrapAngle(float angle)
        {
            if (angle > Mathf.PI)
            {
                angle = -Mathf.PI + (angle - Mathf.PI);
            }
            else if (angle < -Mathf.PI)
            {
                angle = Mathf.PI - (-Mathf.PI - angle);
            }
            return angle;
        }

        private void SetGlobalShaders()
        {
            float t = Time.timeSinceLevelLoad;
            Vector4 time = new Vector4(t * 0.05f, t, (float)System.Math.Truncate(t * 0.05f), (float)System.Math.Truncate(t));
            Shader.SetGlobalVector(WMS._WeatherMakerTime, time);
            Shader.SetGlobalVector(WMS._WeatherMakerTimeSin, new Vector4(Mathf.Sin(time.x), Mathf.Sin(t), Mathf.Sin(t * 2.0f), Mathf.Sin(t * 3.0f)));
            Shader.SetGlobalVector(WMS._WeatherMakerTimeAngle, new Vector4(WrapAngle(time.x), WrapAngle(time.y), WrapAngle(time.z), WrapAngle(time.w)));
        }

        private void Initialize()
        {
            NoiseTexture3DInstance = NoiseTexture3D;
            nullZoneSorterReference = NullZoneSorter;
            lightSorterReference = LightSorter;
            intersectsFuncCurrentCamera = IntersectsFunctionCurrentCamera;
            intersectsFuncCurrentBounds = IntersectsFunctionCurrentBounds;
            intersectsFunc = intersectsFuncCurrentCamera;
            SetPerspectiveOrthographicSun();
            CreateDitherTexture();
            foreach (WeatherMakerCelestialObjectScript script in GameObject.FindObjectsOfType<WeatherMakerCelestialObjectScript>())
            {
                if (script != null)
                {
                    script.UpdateObject();
                }
            }
        }

        private void Update()
        {
            NullZones.Clear();
            Shader.SetGlobalTexture(WMS._WeatherMakerNoiseTexture3D, NoiseTexture3D);
            Shader.SetGlobalTexture(WMS._WeatherMakerBlueNoiseTexture, BlueNoiseTexture);
            SetPerspectiveOrthographicSun();
            CleanupCelestialObjects();
            CleanupLights();
        }

        private void LateUpdate()
        {
            Shader.SetGlobalFloat(WMS._WeatherMakerFogDirectionalLightScatterIntensity, FogDirectionalLightScatterIntensity);
            Shader.SetGlobalVector(WMS._WeatherMakerFogLightFalloff, new Vector4(FogSpotLightRadiusFalloff, 0.0f, 0.0f, 0.0f));
            Shader.SetGlobalFloat(WMS._WeatherMakerFogLightSunIntensityReducer, FogLightSunIntensityReducer);
            if (QualitySettings.shadows == ShadowQuality.Disable)
            {
                Shader.DisableKeyword("WEATHER_MAKER_SHADOWS_SPLIT_SPHERES");
                Shader.DisableKeyword("WEATHER_MAKER_SHADOWS_ONE_CASCADE");
                Shader.SetGlobalInt("_WeatherMakerShadowsEnabled", 0);
            }
            else
            {
                Shader.SetGlobalInt("_WeatherMakerShadowsEnabled", 1);
            }
            if (QualitySettings.shadowCascades < 2)
            {
                Shader.EnableKeyword("WEATHER_MAKER_SHADOWS_ONE_CASCADE");
                Shader.DisableKeyword("WEATHER_MAKER_SHADOWS_SPLIT_SPHERES");
            }
            {
                Shader.DisableKeyword("WEATHER_MAKER_SHADOWS_ONE_CASCADE");
                Shader.EnableKeyword("WEATHER_MAKER_SHADOWS_SPLIT_SPHERES");
            }
            SetGlobalShaders();
        }

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
            Initialize();

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                // light manager pre-render is high priority as it sets a lot of global state
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreRender(CameraPreRender, this, true);
            }
        }

        private void OnDisable()
        {
            //WeatherMakerFullScreenEffect.ReleaseRenderTexture(ref screenSpaceShadowsRenderTexture);
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreRender(this);
            }
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// This method calculates frustum planes and corners and sets current camera. Normally this is called automatically,
        /// but for something like a reflection camera render in a pre-cull event, call this manually
        /// </summary>
        /// <param name="camera"></param>
        public void CalculateFrustumPlanes(Camera camera)
        {
            Matrix4x4 mat = camera.projectionMatrix * camera.worldToCameraMatrix;
            CurrentCamera = camera;

            // left
            CurrentCameraFrustumPlanes[0].normal = new Vector3(mat.m30 + mat.m00, mat.m31 + mat.m01, mat.m32 + mat.m02);
            CurrentCameraFrustumPlanes[0].distance = mat.m33 + mat.m03;

            // right
            CurrentCameraFrustumPlanes[1].normal = new Vector3(mat.m30 - mat.m00, mat.m31 - mat.m01, mat.m32 - mat.m02);
            CurrentCameraFrustumPlanes[1].distance = mat.m33 - mat.m03;

            // bottom
            CurrentCameraFrustumPlanes[2].normal = new Vector3(mat.m30 + mat.m10, mat.m31 + mat.m11, mat.m32 + mat.m12);
            CurrentCameraFrustumPlanes[2].distance = mat.m33 + mat.m13;

            // top
            CurrentCameraFrustumPlanes[3].normal = new Vector3(mat.m30 - mat.m10, mat.m31 - mat.m11, mat.m32 - mat.m12);
            CurrentCameraFrustumPlanes[3].distance = mat.m33 - mat.m13;

            // near
            CurrentCameraFrustumPlanes[4].normal = new Vector3(mat.m30 + mat.m20, mat.m31 + mat.m21, mat.m32 + mat.m22);
            CurrentCameraFrustumPlanes[4].distance = mat.m33 + mat.m23;

            // far
            CurrentCameraFrustumPlanes[5].normal = new Vector3(mat.m30 - mat.m20, mat.m31 - mat.m21, mat.m32 - mat.m22);
            CurrentCameraFrustumPlanes[5].distance = mat.m33 - mat.m23;

            // normalize
            NormalizePlane(ref CurrentCameraFrustumPlanes[0]);
            NormalizePlane(ref CurrentCameraFrustumPlanes[1]);
            NormalizePlane(ref CurrentCameraFrustumPlanes[2]);
            NormalizePlane(ref CurrentCameraFrustumPlanes[3]);
            NormalizePlane(ref CurrentCameraFrustumPlanes[4]);
            NormalizePlane(ref CurrentCameraFrustumPlanes[5]);

            Transform ct = camera.transform;
            Vector3 cPos = ct.position;
            camera.CalculateFrustumCorners(new Rect(0.0f, 0.0f, 1.0f, 1.0f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, CurrentCameraFrustumCorners);
            camera.CalculateFrustumCorners(new Rect(0.0f, 0.0f, 1.0f, 1.0f), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, currentCameraFrustumCornersNear);
            CurrentCameraFrustumCorners[0] = cPos + ct.TransformDirection(CurrentCameraFrustumCorners[0]);
            CurrentCameraFrustumCorners[1] = cPos + ct.TransformDirection(CurrentCameraFrustumCorners[1]);
            CurrentCameraFrustumCorners[2] = cPos + ct.TransformDirection(CurrentCameraFrustumCorners[2]);
            CurrentCameraFrustumCorners[3] = cPos + ct.TransformDirection(CurrentCameraFrustumCorners[3]);
            CurrentCameraFrustumCorners[4] = cPos + ct.TransformDirection(currentCameraFrustumCornersNear[0]);
            CurrentCameraFrustumCorners[5] = cPos + ct.TransformDirection(currentCameraFrustumCornersNear[1]);
            CurrentCameraFrustumCorners[6] = cPos + ct.TransformDirection(currentCameraFrustumCornersNear[2]);
            CurrentCameraFrustumCorners[7] = cPos + ct.TransformDirection(currentCameraFrustumCornersNear[3]);
        }

        /// <summary>
        /// Add a light, unless AutoFindLights is true
        /// </summary>
        /// <param name="l">Light to add</param>
        /// <returns>True if light added, false if not</returns>
        public bool AddLight(Light l)
        {
            if (l != null && AutoFindLights == AutoFindLightsMode.None && !HasLight(l) && IgnoreLights != null && !IgnoreLights.Contains(l))
            {
                lights.Add(GetOrCreateLightState(l));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a light, unless AutoFindLights is true
        /// </summary>
        /// <param name="light"></param>
        /// <returns>True if light removed, false if not</returns>
        public bool RemoveLight(Light light)
        {
            bool result = false;
            if (light != null && AutoFindLights == AutoFindLightsMode.None)
            {
                for (int i = lights.Count - 1; i >= 0; i--)
                {
                    if (lights[i].Light == light)
                    {
                        ReturnLightStateToCache(lights[i]);
                        lights.RemoveAt(i);
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Set shader sun properties
        /// </summary>
        /// <param name="sun">The sun to set. If null or light is null, nothing happens.</param>
        /// <param name="m">Material to set</param>
        private void SetShaderSunProperties(WeatherMakerCelestialObjectScript sun, WeatherMakerShaderPropertiesScript m)
        {
            if (sun == null)
            {
                return;
            }

            Light sunLight = sun.Light;
            Vector3 sunPosition = sun.transform.position;
            Vector3 sunForward = sun.transform.forward;
            Vector3 sunForward2D = Quaternion.AngleAxis(-90.0f, Vector3.right) * sunForward;
            Vector4 sunColor = (new Vector4(sunLight.color.r, sunLight.color.g, sunLight.color.b, sunLight.intensity));
            Vector4 sunTintColor = new Vector4(sun.TintColor.r, sun.TintColor.g, sun.TintColor.b, sun.TintColor.a * sun.TintIntensity);
            float sunHorizonScaleMultiplier = Mathf.Clamp(Mathf.Abs(sunForward.y) * 3.0f, 0.5f, 1.0f);
            sunHorizonScaleMultiplier = Mathf.Min(1.0f, sun.Scale / sunHorizonScaleMultiplier);

            m.SetVector(WMS._WeatherMakerSunDirectionUp, -sunForward);
            m.SetVector(WMS._WeatherMakerSunDirectionUp2D, -sunForward2D);
            m.SetVector(WMS._WeatherMakerSunDirectionDown, sunForward);
            m.SetVector(WMS._WeatherMakerSunDirectionDown2D, sunForward2D);
            m.SetVector(WMS._WeatherMakerSunPositionNormalized, sunPosition.normalized);
            m.SetVector(WMS._WeatherMakerSunPositionWorldSpace, sunPosition);
            m.SetVector(WMS._WeatherMakerSunColor, sunColor);
            m.SetVector(WMS._WeatherMakerSunTintColor, sunTintColor);
            m.SetVector(WMS._WeatherMakerSunLightPower, new Vector4(sun.LightPower, sun.LightMultiplier, sunLight.shadowStrength, 1.0f - sunLight.shadowStrength));
            m.SetVector(WMS._WeatherMakerSunVar1, new Vector4(sunHorizonScaleMultiplier, Mathf.Pow(sunLight.intensity, 0.5f), Mathf.Pow(sunLight.intensity, 0.75f), sunLight.intensity * sunLight.intensity));
        }

        /// <summary>
        /// Called when a camera is about to render - sets up shader and light properties, etc.
        /// </summary>
        /// <param name="camera">The current camera</param>
        private void CameraPreRender(Camera camera)
        {
            // don't re-update lights for reflection or other types of cameras, just use the main camera lights
            if (WeatherMakerScript.ShouldIgnoreCamera(this, camera) || WeatherMakerCommandBufferManagerScript.CameraStackCount > 1 ||
                camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right || camera.stereoTargetEye == StereoTargetEyeMask.Right)
            {
                return;
            }
            UpdateShaderVariables(camera, WeatherMakerShaderPropertiesScript.Global, null);

            // clear screen space shadows texture for next camera
            //if (screenSpaceShadowsRenderTexture != null)
            //{
                //RenderTexture temp = RenderTexture.active;
                //RenderTexture.active = screenSpaceShadowsRenderTexture;
                //GL.Clear(true, true, Color.white);
                //RenderTexture.active = temp;
            //}
        }

        /// <summary>
        /// Update shader variables for an object.
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="material">Shader properties to update</param>
        /// <param name="collider">Collider</param>
        public void UpdateShaderVariables(Camera camera, WeatherMakerShaderPropertiesScript material, Collider collider)
        {
            if (camera == null && collider == null)
            {
                Debug.LogError("Must pass camera or collider to UpdateShaderVariables method");
                return;
            }
            Component obj;
            if (collider != null)
            {
                obj = collider;
                intersectsFunc = intersectsFuncCurrentBounds;
                CurrentBounds = collider.bounds;
                currentLightSortPosition = collider.transform.position;
            }
            else
            {
                obj = camera;
                intersectsFunc = intersectsFuncCurrentCamera;
                CalculateFrustumPlanes(camera);
                currentLightSortPosition = camera.transform.position;
            }

            // *** NOTE: if getting warnings about array sizes changing, simply restart the Unity editor ***
            float elapsed;
            if (!Application.isPlaying || !shaderUpdateCounter.TryGetValue(obj, out elapsed) || elapsed >= ShaderUpdateInterval || lastShaderUpdateComponent != obj)
            {
                shaderUpdateCounter[obj] = 0.0f;
                UpdateAllLights();
                lights.Sort(lightSorterReference);
                UpdateNullZones(material);
            }
            else
            {
                shaderUpdateCounter[obj] += Time.deltaTime;
            }
            lastShaderUpdateComponent = obj;

            // add lights for each type
            SetLightsByTypeToShader(camera, material);

            if (camera == null || !camera.orthographic)
            {
                // reduce volumetric point/spot lights based on sun strength
                float volumetricLightMultiplier = (SunPerspective == null ? 1.0f : Mathf.Max(0.0f, (1.0f - (SunPerspective.Light.intensity * FogLightSunIntensityReducer))));
                Shader.SetGlobalFloat(WMS._WeatherMakerVolumetricPointSpotMultiplier, volumetricLightMultiplier);
                SetShaderSunProperties(SunPerspective, material);
            }
            else
            {
                SetShaderSunProperties(SunOrthographic, material);
            }
        }

        /// <summary>
        /// Get a color for a gradient given a lookup value on the gradient
        /// </summary>
        /// <param name="gradient">Gradient</param>
        /// <param name="lookup">Lookup value (0 - 1)</param>
        /// <returns>Color</returns>
        public static Color EvaluateGradient(Gradient gradient, float lookup)
        {
            if (gradient == null)
            {
                return Color.white;
            }
            Color color = gradient.Evaluate(lookup);
            float a = color.a;
            color *= color.a;
            color.a = a;
            return color;
        }

        /// <summary>
        /// Get the primary sun given a camera
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <returns>Primary sun or null if no sun available</returns>
        public static WeatherMakerCelestialObjectScript SunForCamera(Camera camera)
        {
            if (Instance == null)
            {
                return null;
            }

            return (camera == null || !camera.orthographic ? Instance.SunPerspective : Instance.SunOrthographic);
        }

        /// <summary>
        /// Current set of lights sorted by importance
        /// </summary>
        public IEnumerable<LightState> Lights
        {
            get { return lights; }
        }
        
        /// <summary>
        /// Get primary dir light or null if none
        /// </summary>
        public WeatherMakerCelestialObjectScript PrimaryDirectionalLight
        {
            get
            {
                foreach (var sun in suns)
                {
                    if (sun.Light.intensity > 0.0f)
                    {
                        return sun;
                    }
                }
                foreach (var moon in moons)
                {
                    if (moon.Light.intensity > 0.0f)
                    {
                        return moon;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Return whether screen space shadows are enabled
        /// </summary>
        public static BuiltinShaderMode ScreenSpaceShadowMode
        {
            get { return UnityEngine.Rendering.GraphicsSettings.GetShaderMode(UnityEngine.Rendering.BuiltinShaderType.ScreenSpaceShadows); }
        }

        /// <summary>
        /// The amount of global shadow from Weather Maker fog, clouds, etc.
        /// </summary>
        public float GlobalShadow
        {
            get { return (WeatherMakerFullScreenCloudsScript.Instance != null && WeatherMakerFullScreenFogScript.Instance != null ?
                Mathf.Min(WeatherMakerFullScreenCloudsScript.Instance.CloudGlobalShadow, WeatherMakerFullScreenFogScript.Instance.FogGlobalShadow) : 1.0f); }
        }

        private static WeatherMakerLightManagerScript instance;
        /// <summary>
        /// Shared instance of light manager script
        /// </summary>
        public static WeatherMakerLightManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance, true); }
        }

        /// <summary>
        /// Check if there is a weather maker light manager instance
        /// </summary>
        /// <returns>True if there is a light manager instance, false otherwise</returns>
        public static bool HasInstance()
        {
            return instance != null;
        }

        private readonly List<LightState> lightStateCache = new List<LightState>();

        /// <summary>
        /// Light state
        /// </summary>
        public class LightState
        {

#if WEATHER_MAKER_TRACK_LIGHT_CHANGES

            private struct PreviousState
            {
                public Vector3 HSV;
                public Quaternion Rotation;
                public Vector3 Position;
                public float Range;
            }

#endif

            /// <summary>
            /// Light
            /// </summary>
            public Light Light { get; internal set; }

            /// <summary>
            /// Reset state
            /// </summary>
            public void Reset()
            {
                Light = null;

#if WEATHER_MAKER_TRACK_LIGHT_CHANGES

                states.Clear();

#endif

            }

#if WEATHER_MAKER_TRACK_LIGHT_CHANGES

            private readonly Dictionary<Camera, PreviousState> states = new Dictionary<Camera, PreviousState>();

#endif

            /// <summary>
            /// Update light state
            /// </summary>
            /// <param name="camera">Camera</param>
            /// <returns>Light difference from last frame in camera (0 - 1)</returns>
            internal float Update(Camera camera)
            {

#if WEATHER_MAKER_TRACK_LIGHT_CHANGES

                if (camera == null)
                {
                    return 0.0f;
                }

                PreviousState state;
                if (!states.TryGetValue(camera, out state))
                {
                    states[camera] = state;
                }
                Vector3 curHSV;
                Color.RGBToHSV(Light.color, out curHSV.x, out curHSV.y, out curHSV.z);
                Quaternion curRot = Light.transform.rotation;
                Vector3 curPos = Light.transform.position;
                float curRange = Light.range;
                float diff = Mathf.Abs(state.HSV.x - curHSV.x) + Mathf.Abs(state.HSV.y - curHSV.y) + Mathf.Abs(state.HSV.z - curHSV.z);
                diff += Mathf.Abs(state.Rotation.x - curRot.x) + Mathf.Abs(state.Rotation.y - curRot.y) + Mathf.Abs(state.Rotation.z - curRot.z) + Mathf.Abs(state.Rotation.w - curRot.w);
                float distModifier = Mathf.Min(1.0f, (camera.farClipPlane * 0.01f) / Mathf.Max(0.001f, Vector3.Distance(camera.transform.position, curPos)));
                diff += (distModifier * Vector3.Distance(curPos, state.Position));
                diff += (distModifier * Mathf.Abs(curRange - state.Range));
                diff = Mathf.Min(1.0f, diff);
                state.HSV = curHSV;
                state.Rotation = curRot;
                state.Position = curPos;
                state.Range = curRange;

                return diff;

#else

                return 0.0f;

#endif

            }
        }

        private bool HasLight(Light light)
        {
            if (light == null)
            {
                return false;
            }

            foreach (LightState lightState in lights)
            {
                if (lightState.Light == light)
                {
                    return true;
                }
            }
            return false;
        }

        private LightState GetOrCreateLightState(Light light)
        {
            if (lightStateCache.Count == 0)
            {
                return new LightState { Light = light };
            }
            int idx = lightStateCache.Count - 1;
            LightState lightState = lightStateCache[idx];
            lightState.Light = light;
            lightStateCache.RemoveAt(idx);
            return lightState;
        }

        private void ReturnLightStateToCache(LightState lightState)
        {
            if (lightState != null && !lightStateCache.Contains(lightState))
            {
                lightState.Reset();
                lightStateCache.Add(lightState);
            }
        }
    }
}
