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
using UnityEngine.Rendering;

// #define WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Atmosphere profile
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerAtmosphereProfile", menuName = "WeatherMaker/Atmosphere Profile", order = 32)]
    [System.Serializable]
    public class WeatherMakerAtmosphereProfileScript : ScriptableObject
    {
        private static WeatherMakerAtmosphereQuality lastAtmosphereQuality = WeatherMakerAtmosphereQuality.Disabled;

        /// <summary>Incoming light color</summary>
        [Header("Scattering")]
        [Tooltip("Incoming light color")]
        public Color IncomingLight = new Color(4, 4, 4, 4);
        private static Color lastIncomingLight = Color.black;

        /// <summary>Rayleight scattering coefficient</summary>
        [Tooltip("Rayleight scattering coefficient")]
        [Range(0, 10.0f)]
        public float RayleighScatterCoefficient = 1;
        private static float lastRayleightScatteringCoefficient = -1.0f;

        /// <summary>Rayleigh extinction coefficient</summary>
        [Tooltip("Rayleigh extinction coefficient")]
        [Range(0, 10.0f)]
        public float RayleighExtinctionCoefficient = 1.0f;

        /// <summary>Mie scattering coefficient</summary>
        [Tooltip("Mie scattering coefficient")]
        [Range(0, 10.0f)]
        public float MieScatterCoefficient = 1;
        private static float lastMieScatteringCoefficient = -1.0f;

        /// <summary>Mie extinction coefficient</summary>
        [Tooltip("Mie extinction coefficient")]
        [Range(0, 10.0f)]
        public float MieExtinctionCoefficient = 1.0f;

        /// <summary>MieG</summary>
        [Tooltip("MieG")]
        [Range(0.0f, 0.999f)]
        public float MieG = 0.62f;

        /// <summary>Atmosphere planet radius</summary>
        [Header("Atmosphere")]
        [Tooltip("Atmosphere planet radius")]
        [Range(0.0f, 100000000.0f)]
        public float AtmospherePlanetRadius = 6371000.0f;
        private static float lastAtmospherePlanetRadius = -1.0f;

        /// <summary>Atmosphere height</summary>
        [Tooltip("Atmosphere height")]
        [Range(0.0f, 1000000.0f)]
        public float AtmosphereHeight = 80000.0f;
        private static float lastAtmosphereHeight = -1.0f;

        /// <summary>Atmosphere thickness</summary>
        [Range(0.0f, 5.0f)]
        [Tooltip("Atmosphere thickness")]
        public float AtmosphereThickness = 1.0f;
        private static float lastAtmosphereThickness = -1.0f;

        /// <summary>Atmosphere turbidity</summary>
        [Range(0.0f, 10.0f)]
        [Tooltip("Atmosphere turbidity")]
        public float AtmosphereTurbidity = 1.0f;

        /// <summary>Atmosphere fog thickness</summary>
        [Tooltip("Atmosphere fog thickness")]
        [Range(0.01f, 1.0f)]
        public float AtmosphereFogThickness = 0.1f;

        /// <summary>Max length of atmosphere. Set to 0 to use camera far plane.</summary>
        [Tooltip("Max length of atmosphere. Set to 0 to use camera far plane.")]
        [Range(0.0f, 1000000.0f)]
        public float AtmosphereRayLength = 0.0f;

        /// <summary>Whether to enable light shafts</summary>
        [Header("Light Shafts")]
        [Tooltip("Whether to enable light shafts")]
        public bool LightShaftEnabled = true;

        /// <summary>Light shaft sample count</summary>
        [Range(2, 64)]
        [Tooltip("Light shaft sample count")]
        public int LightShaftSampleCount = 16;

        /// <summary>Light shaft max ray length</summary>
        [Range(100.0f, 100000.0f)]
        [Tooltip("Light shaft max ray length")]
        public float LightShaftMaxRayLength = 5000.0f;

        /// <summary>Whether to update the sun color</summary>
        [Header("Other")]
        [Tooltip("Whether to update the sun color")]
        public bool UpdateSunColor;

        /// <summary>Whether to update the ambient color</summary>
        [Tooltip("Whether to update the ambient color")]
        public bool UpdateAmbientColor;

        /// <summary>If updating ambient color, the ambient intensity</summary>
        [Tooltip("If updating ambient color, the ambient intensity")]
        [Range(0.5f, 3.0f)]
        public float AmbientColorIntensity = 1.0f;


        private void UpdateShaderVariables(Camera camera)
        {
            Shader.SetGlobalInt(WMS._WeatherMakerAtmosphereLevel, (int)lastAtmosphereQuality);
            Shader.SetGlobalVector(WMS._WeatherMakerDensityScaleHeight, DensityScale);
            Shader.SetGlobalVector(WMS._WeatherMakerScatteringR, RayleighSct * RayleighScatterCoefficient);
            Shader.SetGlobalVector(WMS._WeatherMakerScatteringM, MieSct * MieScatterCoefficient);
            Shader.SetGlobalVector(WMS._WeatherMakerExtinctionR, RayleighSct * RayleighExtinctionCoefficient);
            Shader.SetGlobalVector(WMS._WeatherMakerExtinctionM, MieSct * MieExtinctionCoefficient);
            Shader.SetGlobalColor(WMS._WeatherMakerIncomingLight, IncomingLight);
            Shader.SetGlobalFloat(WMS._WeatherMakerMieG, MieG);
            Shader.SetGlobalFloat(WMS._WeatherMakerAtmosphereDistanceScale, AtmosphereFogThickness);
            Shader.SetGlobalFloat(WMS._WeatherMakerAtmosphereRayLength, AtmosphereRayLength <= 0.0f && camera != null ? camera.farClipPlane : AtmosphereRayLength);
            Shader.SetGlobalFloat(WMS._WeatherMakerAtmosphereHeight, AtmosphereHeight);
            Shader.SetGlobalFloat(WMS._WeatherMakerAtmospherePlanetRadius, AtmospherePlanetRadius);

            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                Shader.SetGlobalInt(WMS._WeatherMakerAtmosphereLightShaftSampleCount, LightShaftSampleCount);
                Shader.SetGlobalFloat(WMS._WeatherMakerAtmosphereLightShaftMaxRayLength, LightShaftMaxRayLength);
            }
            else
            {
                Shader.SetGlobalInt(WMS._WeatherMakerAtmosphereLightShaftSampleCount, WeatherMakerScript.Instance.PerformanceProfile.AtmosphericLightShaftSampleCount);
                Shader.SetGlobalFloat(WMS._WeatherMakerAtmosphereLightShaftMaxRayLength, WeatherMakerScript.Instance.PerformanceProfile.AtmosphericLightShaftMaxRayLength);
            }
        }

        private void UpdateShaderTextures()
        {
            Shader.SetGlobalTexture(WMS._WeatherMakerRandomVectors, randomVectorsLUT);
            Shader.SetGlobalTexture(WMS._WeatherMakerInscatteringLUT, inscatteringLUT);
            Shader.SetGlobalTexture(WMS._WeatherMakerExtinctionLUT, extinctionLUT);
            Shader.SetGlobalTexture(WMS._WeatherMakerInscatteringLUT2, inscatteringLUT2);
            Shader.SetGlobalTexture(WMS._WeatherMakerExtinctionLUT2, extinctionLUT2);
            Shader.SetGlobalTexture(WMS._WeatherMakerParticleDensityLUT, particleDensityLUT);
            Shader.SetGlobalTexture(WMS._WeatherMakerSkyboxLUT, skyboxLUT);

#if WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

            Shader.SetGlobalTexture(WMS._WeatherMakerSkyboxLUT2, skyboxLUT2);

#endif
        }

        private void UpdatePrecomputedTextures(Material lookupMaterial, ComputeShader lookupComputeShader)
        {
            // precompute expensive values
            if (lookupMaterial != null && lookupComputeShader != null && !WeatherMakerScript.IsHeadlessMode &&
                WeatherMakerScript.Instance != null && WeatherMakerScript.Instance.PerformanceProfile != null &&
            (
                !IncomingLight.Equals(lastIncomingLight) ||
                lastAtmosphereQuality != WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality ||
                RayleighScatterCoefficient != lastRayleightScatteringCoefficient ||
                MieScatterCoefficient != lastMieScatteringCoefficient ||
                AtmosphereThickness != lastAtmosphereThickness ||
                AtmosphereHeight != lastAtmosphereHeight ||
                AtmospherePlanetRadius != lastAtmospherePlanetRadius))
            {
                lastAtmosphereQuality = WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality;
                lastIncomingLight = IncomingLight;
                lastRayleightScatteringCoefficient = RayleighScatterCoefficient;
                lastMieScatteringCoefficient = MieScatterCoefficient;
                lastAtmosphereThickness = AtmosphereThickness;
                lastAtmosphereHeight = AtmosphereHeight;
                lastAtmospherePlanetRadius = AtmospherePlanetRadius;
                DestroyTextures();
                InitializeRandomVectorsLUT();
                CreateParticleDensityLUT(lookupMaterial);
                CreateLightLUT(lookupMaterial, lookupComputeShader);
                CreateSkyboxLUT(lookupComputeShader);
                GL.Flush();
            }
        }

        /// <summary>
        /// Update shader variables
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="lookupMaterial">Lookup material</param>
        /// <param name="lookupComputeShader">Lookup compute shader</param>
        public void UpdateShaderVariables(Camera camera, Material lookupMaterial, ComputeShader lookupComputeShader)
        {
            if (WeatherMakerScript.IsHeadlessMode)
            {
                return;
            }

            UpdateShaderVariables(camera);

            WeatherMakerCelestialObjectScript sun = WeatherMakerLightManagerScript.Instance.PrimaryDirectionalLight;
            if (sun == null)
            {
                return;
            }

            bool disableAtmosphericScattering = (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality == WeatherMakerAtmosphereQuality.Disabled ||
                WeatherMakerLightManagerScript.Instance == null || WeatherMakerLightManagerScript.Instance.SunPerspective == null ||
                WeatherMakerCommandBufferManagerScript.Instance == null || camera == null || lookupMaterial == null || lookupComputeShader == null ||
                !SystemInfo.supportsComputeShaders);

            UpdateShaderTextures();
            UpdatePrecomputedTextures(lookupMaterial, lookupComputeShader);

            if (UpdateSunColor)
            {
                UpdateDirectionalLightColor(sun, ComputeLightColor(sun.Light));
            }
            if (UpdateAmbientColor)
            {
                UpdateAmbientLightColor(ComputeAmbientColor(sun.Light));
            }
            if (!disableAtmosphericScattering)
            {
                UpdateInscatteringLUT(lookupComputeShader, sun.Light, camera);
            }
        }

        /// <summary>
        /// Shutdown and dispose all global resources
        /// </summary>
        public static void Shutdown()
        {
            lastIncomingLight = Color.black;
            lastMieScatteringCoefficient = 1.0f;
            lastRayleightScatteringCoefficient = -1.0f;
            lastAtmosphereHeight = -1.0f;
            lastAtmospherePlanetRadius = -1.0f;
            lastAtmosphereThickness = -1.0f;
            DestroyTextures();
        }

        private RenderTexture Create3DTexture(Vector3 size, string name, RenderTextureFormat format = RenderTextureFormat.ARGBHalf)
        {
            if (size.x <= 0.0f || size.y <= 0.0f || size.z <= 0.0f)
            {
                return null;
            }

            RenderTexture tex = new RenderTexture((int)size.x, (int)size.y, 0, format, RenderTextureReadWrite.Linear);
            tex.volumeDepth = (int)size.z;
            tex.dimension = TextureDimension.Tex3D;
            tex.enableRandomWrite = true;
            tex.autoGenerateMips = false;
            tex.useMipMap = false;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = name;
            tex.Create();
            return tex;
        }

        private static void DestroyInscatteringTextures()
        {
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref extinctionLUT);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref inscatteringLUT);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref extinctionLUT2);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref inscatteringLUT2);
        }

        private static void DestroyTextures()
        {
            DestroyInscatteringTextures();
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref lightColorTexture);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref skyboxLUT);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref skyboxLUT);

#if WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref skyboxLUT2);

#endif

            if (randomVectorsLUT != null)
            {
                GameObject.DestroyImmediate(randomVectorsLUT);
                randomVectorsLUT = null;
            }
            if (particleDensityLUT != null)
            {
                GameObject.DestroyImmediate(particleDensityLUT);
                particleDensityLUT = null;
            }
        }

        private void InitializeInscatteringLUT()
        {
            Vector3 inscatteringLUTSize = inscatteringLUTSizeQuality[(int)WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality];
            if (inscatteringLUT != null &&
                (inscatteringLUT.width != (int)inscatteringLUTSize.x || inscatteringLUT.height != (int)inscatteringLUTSize.y || inscatteringLUT.volumeDepth != (int)inscatteringLUTSize.z))
            {
                DestroyInscatteringTextures();
            }
            inscatteringLUT = (inscatteringLUT ?? Create3DTexture(inscatteringLUTSize, "WeatherMakerInscatteringLUT"));
            inscatteringLUT2 = (inscatteringLUT2 ?? Create3DTexture(inscatteringLUTSize, "WeatherMakerInscatteringLUT2"));
            extinctionLUT = (extinctionLUT ?? Create3DTexture(inscatteringLUTSize, "WeatherMakerInscatteringLUT"));
            extinctionLUT2 = (extinctionLUT2 ?? Create3DTexture(inscatteringLUTSize, "WeatherMakerInscatteringLUT2"));
        }

        private void CreateSkyboxLUT(ComputeShader computeShader)
        {
            skyboxLUT = (skyboxLUT ?? Create3DTexture(skyboxLUTSize, "WeatherMakerSkyboxLUT"));

#if WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

            skyboxLUT2 = (skyboxLUT2 ?? Create3DTexture(skyboxLUTSize, "WeatherMakerSkyboxLUT2", RenderTextureFormat.RGHalf);

#endif

            int kernel = computeShader.FindKernel("WeatherMakerSkyboxLUT");
            UpdateComputeShaderParameters(computeShader, kernel, null, null);
            computeShader.Dispatch(kernel, (int)skyboxLUTSize.x / 4, (int)skyboxLUTSize.y / 4, (int)skyboxLUTSize.z / 4);
        }

        private void UpdateComputeShaderParameters(ComputeShader computeShader, int kernel, Camera camera, Light light)
        {
            if (particleDensityLUT != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerParticleDensityLUT, particleDensityLUT);
            }
            if (inscatteringLUT != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerInscatteringLUT, inscatteringLUT);
            }
            if (extinctionLUT != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerExtinctionLUT, extinctionLUT);
            }
            if (inscatteringLUT2 != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerInscatteringLUT2, inscatteringLUT2);
            }
            if (extinctionLUT2 != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerExtinctionLUT2, extinctionLUT2);
            }
            if (skyboxLUT != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerSkyboxLUT, skyboxLUT);
            }

#if WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

            if (skyboxLUT2 != null)
            {
                computeShader.SetTexture(kernel, WMS._WeatherMakerSkyboxLUT2, skyboxLUT2);
            }

#endif

            Shader.SetGlobalVector(WMS._WeatherMakerInscatteringLUT_Dimensions, inscatteringLUTSizeQuality[(int)WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality]);
            computeShader.SetVector(WMS._WeatherMakerInscatteringLUT_Dimensions, inscatteringLUTSizeQuality[(int)WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality]);
            computeShader.SetVector(WMS._WeatherMakerSkyboxLUT_Dimensions, skyboxLUTSize);
            if (light != null)
            {
                computeShader.SetVector(WMS._WeatherMakerSunDirectionDown, light.transform.forward);
                computeShader.SetVector(WMS._WeatherMakerSunDirectionUp, -light.transform.forward);
            }
            computeShader.SetVector(WMS._WeatherMakerDensityScaleHeight, DensityScale);
            computeShader.SetFloat(WMS._WeatherMakerAtmosphereDistanceScale, AtmosphereFogThickness);
            computeShader.SetVector(WMS._WeatherMakerScatteringR, RayleighSct * RayleighScatterCoefficient);
            computeShader.SetVector(WMS._WeatherMakerScatteringM, MieSct * MieScatterCoefficient);
            computeShader.SetVector(WMS._WeatherMakerExtinctionR, RayleighSct * RayleighExtinctionCoefficient);
            computeShader.SetVector(WMS._WeatherMakerExtinctionM, MieSct * MieExtinctionCoefficient);
            computeShader.SetVector(WMS._WeatherMakerIncomingLight, IncomingLight);
            computeShader.SetFloat(WMS._WeatherMakerMieG, MieG);
            computeShader.SetFloat(WMS._WeatherMakerAtmosphereHeight, AtmosphereHeight);
            computeShader.SetFloat(WMS._WeatherMakerAtmospherePlanetRadius, AtmospherePlanetRadius);
            computeShader.SetVector(WMS._WeatherMakerSkyAtmosphereParams, new Vector4(AtmosphereThickness, AtmosphereTurbidity, 1.0f, 0.0f));
            if (camera != null && WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UpdateShaderPropertiesForCamera(computeShader, camera);
            }
            computeShader.SetVectorArray(WMS._WeatherMakerCameraFrustumCorners, WeatherMakerCommandBufferManagerScript.Instance.cameraFrustumCorners);
            if (camera != null && camera.stereoEnabled)
            {
                computeShader.SetInt(WMS._WeatherMakerStereoEyeIndex, 1);
            }
            else
            {
                computeShader.SetInt(WMS._WeatherMakerStereoEyeIndex, 0);
            }
        }

        private void UpdateInscatteringLUT(ComputeShader computeShader, Light light, Camera camera)
        {
            InitializeInscatteringLUT();
            int kernel = computeShader.FindKernel("WeatherMakerInscatteringLUT");
            UpdateComputeShaderParameters(computeShader, kernel, camera, light);
            Vector3 inscatteringLUTSize = inscatteringLUTSizeQuality[(int)WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality];
            computeShader.Dispatch(kernel, (int)inscatteringLUTSize.x / 4, (int)inscatteringLUTSize.y / 4, 1);
        }

        private void CreateLightLUT(Material material, ComputeShader computeShader)
        {
            if (lightColorTexture == null)
            {
                lightColorTexture = new RenderTexture(lightLUTSize, 1, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                lightColorTexture.wrapMode = TextureWrapMode.Clamp;
                lightColorTexture.filterMode = FilterMode.Bilinear;
                lightColorTexture.name = "LightColorTexture";
                lightColorTexture.Create();
            }

            Texture2D lightColorTextureTemp = new Texture2D(lightLUTSize, 1, TextureFormat.RGBAHalf, false, true);
            lightColorTextureTemp.wrapMode = TextureWrapMode.Clamp;
            lightColorTextureTemp.filterMode = FilterMode.Bilinear;
            lightColorTextureTemp.name = "LightColorTextureTemp";
            lightColorTextureTemp.Apply();

            // ambient LUT
            Graphics.Blit((Texture)null, lightColorTexture, material, 1);

            lightColorTextureTemp.ReadPixels(new Rect(0, 0, lightLUTSize, 1), 0, 0);
            ambientLightLUT = lightColorTextureTemp.GetPixels(0, 0, lightLUTSize, 1);

            // directional LUT
            Graphics.Blit((Texture)null, lightColorTexture, material, 2);

            lightColorTextureTemp.ReadPixels(new Rect(0, 0, lightLUTSize, 1), 0, 0);
            directionalLightLUT = lightColorTextureTemp.GetPixels(0, 0, lightLUTSize, 1);
            Graphics.SetRenderTarget(null, 0, CubemapFace.Unknown, -1);

            GameObject.DestroyImmediate(lightColorTextureTemp);
        }

        private void InitializeRandomVectorsLUT()
        {
            if (randomVectorsLUT == null)
            {
                randomVectorsLUT = new Texture2D(256, 1, TextureFormat.RGBAHalf, false, true);
                randomVectorsLUT.name = "RandomVectorsLUT";
            }

            Color[] colors = new Color[256];
            for (int i = 0; i < colors.Length; ++i)
            {
                Vector3 vector = WeatherMakerRandomizer.Unity.RandomOnUnitSphere();
                colors[i] = new Color(vector.x, vector.y, vector.z, 1.0f);
            }
            randomVectorsLUT.SetPixels(colors);
            randomVectorsLUT.Apply();
        }

        private void CreateParticleDensityLUT(Material material)
        {
            if (particleDensityLUT == null)
            {
                particleDensityLUT = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
                particleDensityLUT.name = "ParticleDensityLUT";
                particleDensityLUT.wrapMode = TextureWrapMode.Clamp;
                particleDensityLUT.filterMode = FilterMode.Bilinear;
                particleDensityLUT.Create();
            }

            Graphics.Blit((Texture)null, particleDensityLUT, material, 0);
        }

        private Color ComputeLightColor(Light light)
        {
            float cosAngle = Vector3.Dot(Vector3.up, -light.transform.forward);
            float u = (cosAngle + 0.1f) / 1.1f;// * 0.5f + 0.5f;

            u = u * lightLUTSize;
            int index0 = Mathf.FloorToInt(u);
            float weight1 = u - index0;
            int index1 = index0 + 1;
            float weight0 = 1 - weight1;

            index0 = Mathf.Clamp(index0, 0, lightLUTSize - 1);
            index1 = Mathf.Clamp(index1, 0, lightLUTSize - 1);

            Color c = directionalLightLUT[index0] * weight0 + directionalLightLUT[index1] * weight1;
            return c.gamma;
        }

        private void UpdateDirectionalLightColor(WeatherMakerCelestialObjectScript light, Color c)
        {
            Vector3 color = new Vector3(c.r, c.g, c.b);
            float length = color.magnitude;
            color /= length;
            light.LightColor = new Color(Mathf.Max(color.x, 0.01f), Mathf.Max(color.y, 0.01f), Mathf.Max(color.z, 0.01f), 1.0f);
        }

        private Color ComputeAmbientColor(Light sun)
        {
            float cosAngle = Vector3.Dot(Vector3.up, -sun.transform.forward);
            float u = (cosAngle + 0.1f) / 1.1f;

            u = u * lightLUTSize;
            int index0 = Mathf.FloorToInt(u);
            float weight1 = u - index0;
            int index1 = index0 + 1;
            float weight0 = 1 - weight1;

            index0 = Mathf.Clamp(index0, 0, lightLUTSize - 1);
            index1 = Mathf.Clamp(index1, 0, lightLUTSize - 1);

            Color c = ambientLightLUT[index0] * weight0 + ambientLightLUT[index1] * weight1;
            return c.gamma;
        }

        private void UpdateAmbientLightColor(Color c)
        {
            RenderSettings.ambientLight = c * AmbientColorIntensity;
        }

        private const int lightLUTSize = 128;
        private static readonly Vector4 DensityScale = new Vector4(7994.0f, 1200.0f, 0, 0);
        private static readonly Vector4 RayleighSct = new Vector4(5.8f, 13.5f, 33.1f, 0.0f) * 0.000001f;
        private static readonly Vector4 MieSct = new Vector4(2.0f, 2.0f, 2.0f, 0.0f) * 0.00001f;
        private static readonly Vector3 skyboxLUTSize = new Vector3(32, 128, 32);
        private static readonly Vector3[] inscatteringLUTSizeQuality = new Vector3[]
        {
            Vector3.zero,
            new Vector3(8.0f, 8.0f, 64.0f),
            new Vector3(12.0f, 16.0f, 64.0f),
            new Vector3(16.0f, 32.0f, 64.0f)
        };

        private static RenderTexture particleDensityLUT;
        private static Texture2D randomVectorsLUT;
        private static RenderTexture lightColorTexture;
        private static RenderTexture skyboxLUT;

#if WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY

        private static RenderTexture skyboxLUT2;

#endif

        private static RenderTexture inscatteringLUT;
        private static RenderTexture extinctionLUT;
        private static RenderTexture inscatteringLUT2;
        private static RenderTexture extinctionLUT2;

        private static Color[] directionalLightLUT;
        private static Color[] ambientLightLUT;
    }

    /// <summary>
    /// Atmosphere rendering quality
    /// </summary>
    public enum WeatherMakerAtmosphereQuality
    {
        /// <summary>
        /// No atomspheric scattering
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Low quality
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium quality
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High quality
        /// </summary>
        High = 3
    }
}
