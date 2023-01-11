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
using UnityEngine.Rendering;
using UnityEngine.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Full screen cloud script and renderer
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerFullScreenCloudsScript : MonoBehaviour
    {
        /// <summary>Cloud profile</summary>
        [Header("Full Screen Clouds - profile")]
        [Tooltip("Cloud profile")]
        [SerializeField]
        private WeatherMakerCloudProfileScript _CloudProfile;
        private WeatherMakerCloudProfileScript currentRenderCloudProfile;

        /// <summary>Seed to use for clouds, 0 for auto</summary>
        [Tooltip("Seed to use for clouds, -1 for auto")]
        public int CloudSeed = -1;
        private int lastCloudSeed;
        private bool regenerateCloudSeedOnEnable;

        /// <summary>Aurora borealis profile</summary>
        [Tooltip("Aurora borealis profile")]
        [FormerlySerializedAs("Aurora")]
        public WeatherMakerAuroraProfileScript AuroraProfile;

        /// <summary>Aurora animation in seconds when changing aurora profile</summary>
        [Tooltip("Aurora animation in seconds when changing aurora profile")]
        [Range(0.0f, 120.0f)]
        public float AuroraAnimationDuration = 10.0f;

        /// <summary>Down sample scale.</summary>
        [Header("Full Screen Clouds - rendering")]
        [Tooltip("Down sample scale.")]
        public WeatherMakerDownsampleScale DownSampleScale = WeatherMakerDownsampleScale.HalfResolution;

        /// <summary>Downsample scale for cloud post process (dir light rays, etc.)</summary>
        [Tooltip("Downsample scale for cloud post process (dir light rays, etc.)")]
        public WeatherMakerDownsampleScale DownSampleScalePostProcess = WeatherMakerDownsampleScale.QuarterResolution;

        /// <summary>Cloud rendering material.</summary>
        [Tooltip("Cloud rendering material.")]
        public Material Material;

        /// <summary>Material to blit the full screen clouds.</summary>
        [Tooltip("Material to blit the full screen clouds.")]
        public Material FullScreenMaterial;

        /// <summary>Blur Material.</summary>
        [Tooltip("Blur Material.")]
        public Material BlurMaterial;

        /// <summary>Bilateral Blur Material</summary>
        [Tooltip("Bilateral Blur Material")]
        public Material BilateralBlurMaterial;

        /// <summary>Material for temporal reprojection</summary>
        [Tooltip("Material for temporal reprojection")]
        public Material TemporalReprojectionMaterial;

        /// <summary>Material for cloud shadows</summary>
        [Tooltip("Material for cloud shadows")]
        public Material ShadowMaterial;

        /// <summary>Blur Shader Type.</summary>
        [Tooltip("Blur Shader Type.")]
        public BlurShaderType BlurShader;

        /// <summary>Temporal reprojection size - allows rendering a portion of this effect over a number of frames to spread cost out over time. This can introduce rendering artifacts so be on the lookout for that.</summary>
        [Tooltip("Temporal reprojection size - allows rendering a portion of this effect over a number of frames to spread cost out over time. " +
            "This can introduce rendering artifacts so be on the lookout for that.")]
        public WeatherMakerTemporalReprojectionSize TemporalReprojection = WeatherMakerTemporalReprojectionSize.TwoByTwo;

        /// <summary>Render Queue</summary>
        [Tooltip("Render Queue")]
        public CameraEvent RenderQueue = CameraEvent.BeforeImageEffectsOpaque;

        /// <summary>Whether to render clouds in reflection cameras.</summary>
        [Tooltip("Whether to render clouds in reflection cameras.")]
        public bool AllowReflections = true;

        /// <summary>Materials to post process clouds image.</summary>
        [Tooltip("Materials to post process clouds image.")]
        public Material[] CloudPostProcessors;

        /// <summary>Weather map material (volumetric clouds)</summary>
        [Header("Full Screen Clouds - weather map")]
        [Tooltip("Weather map material (volumetric clouds)")]
        public Material WeatherMapMaterial;
        private readonly WeatherMakerMaterialCopy weatherMapMaterialCopy = new WeatherMakerMaterialCopy();

        /// <summary>Override the weather map no matter what the cloud profile is specifying. R = cloud coverage, G = reserved, B = cloud type (0 = stratus, 1 = cumulus), A = reserved.</summary>
        [Tooltip("Override the weather map no matter what the cloud profile is specifying. R = cloud coverage, G = reserved, B = cloud type (0 = stratus, 1 = cumulus), A = reserved.")]
        public Texture2D WeatherMapOverride;

        /// <summary>Override the weather map mask no matter what the cloud profile is specifying. Alpha channel of 0 blocks clouds completely, 1 shows clouds completely.</summary>
        [Tooltip("Override the weather map mask no matter what the cloud profile is specifying. Alpha channel of 0 blocks clouds completely, 1 shows clouds completely.")]
        public Texture2D WeatherMapMaskOverride;

        /// <summary>Material for weather map signed distance field calculation</summary>
        [Tooltip("Material for weather map signed distance field calculation")]
        public Material WeatherMapSdfMaterial;

        /// <summary>Materials to post process the weather map, each will be executed in order</summary>
        [Tooltip("Weather map post processing materials")]
        public Material[] WeatherMapPostProcessors;

        /// <summary>Additional offset to cloud ray to bring clouds up or down.</summary>
        [Header("Full Screen Clouds - Other")]
        [Tooltip("Additional offset to cloud ray to bring clouds up or down.")]
        [Range(-1.0f, 1.0f)]
        public float CloudRayOffset = 0.0f;

        /// <summary>Multiply all ambient cloud colors by this alpha value, where center of gradient is sun at horizon. Helps if the cloud become too bright when you change your ambient lighting.</summary>
        [Tooltip("Multiply all ambient cloud colors by this alpha value, where center of gradient is sun at horizon. Helps if the cloud become too bright when you change your ambient lighting.")]
        public Gradient GlobalAmbientMultiplierGradient;

        /// <summary>An optional collider that if specified, will remap the entire volumetric weather map to the area of the collider.</summary>
        [Tooltip("An optional collider that if specified, will remap the entire volumetric weather map to the area of the collider.")]
        public BoxCollider VolumetricCloudBoxRemap;

        /// <summary>
        /// Real-time cloud noise profile, extremely slow do not use normally
        /// </summary>
        [Header("Full Screen Clouds - Realtime Noise (Debug Only, Very Slow)")]
        public WeatherMakerCloudNoiseProfileGroupScript CloudRealtimeNoiseProfile;

        /// <summary>
        /// Allow rendering to command buffer before the clouds render
        /// </summary>
        public readonly List<System.Action<WeatherMakerCommandBuffer>> BeforeCloudsRenderHooks = new List<System.Action<WeatherMakerCommandBuffer>>();

        private bool animatingAurora;
        private WeatherMakerShaderPropertiesScript shaderProps;
        private Material alphaBlitMaterial;
        private DigitalRuby.WeatherMaker.IRandomizer randomizer = DigitalRuby.WeatherMaker.WeatherMakerRandomizer.Unity;

        /// <summary>
        /// Cloud weather map
        /// </summary>
        public RenderTexture WeatherMapRenderTexture { get; private set; }

        /// <summary>
        /// Cloud shadow texture
        /// </summary>
        public RenderTexture CloudShadowRenderTexture { get; private set; }

        private WeatherMakerCloudProfileScript lastProfile;
        private WeatherMakerCloudProfileScript lastProfileOriginal;

        /// <summary>
        /// Get the current cloud profile
        /// </summary>
        public WeatherMakerCloudProfileScript CloudProfile
        {
            get { return _CloudProfile; }
            set { ShowCloudsAnimated(value, 0.0f, 5.0f); }
        }

        private WeatherMakerFullScreenEffect effect;
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;
        private static WeatherMakerCloudProfileScript emptyProfile;
        private Collider cloudCollider;

        private readonly List<CameraValues> cameraValues = new List<CameraValues>();

        private class CameraValues
        {
            public Camera Camera;
            public float FieldOfView;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CloudProbe
        {
            public Vector4 Source;
            public Vector4 Target;
        }

        /// <summary>
        /// Result of a cloud probe operation
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CloudProbeResult
        {
            /// <summary>
            /// Cloud density at the source (0 - 1)
            /// </summary>
            public float DensitySource;

            /// <summary>
            /// Cloud density at the target (0 - 1)
            /// </summary>
            public float DensityTarget;

            /// <summary>
            /// Cloud density ray average (64 samples, 0 - 1)
            /// </summary>
            public float DensityRayAverage;

            /// <summary>
            /// Cloud density ray sum (64 samples, 0 - 64)
            /// </summary>
            public float DensityRaySum;
        }

        private static readonly List<CloudProbeComputeRequest> probeRequestPool = new List<CloudProbeComputeRequest>();
        private static readonly List<ComputeBuffer> computeBufferPool = new List<ComputeBuffer>();
        private readonly List<CloudProbeRequest> tempRequests = new List<CloudProbeRequest>();

        private class CloudProbeComputeRequest : IDisposable
        {
            public Camera Camera;
            public List<CloudProbeRequest> UserRequests = new List<CloudProbeRequest>();
            public List<CloudProbe> SamplesList = new List<CloudProbe>();
            public ComputeShader Shader;
            public ComputeBuffer Result;
            public AsyncGPUReadbackRequest Request;
            public void Reset()
            {
                UserRequests.Clear();
                SamplesList.Clear();
            }
            public void Dispose()
            {
                if (Result != null)
                {
                    if (!computeBufferPool.Contains(Result))
                    {
                        computeBufferPool.Add(Result);
                    }
                    Result = null;
                }
                if (!probeRequestPool.Contains(this))
                {
                    probeRequestPool.Add(this);
                }
            }
        }

        private struct CloudProbeRequest
        {
            public Camera Camera;
            public Transform Source;
            public Transform Target;
        }

        private const float oneOver64 = 0.015625f;
        private ComputeShader cloudProbeShader;
        private readonly List<CloudProbeComputeRequest> cloudProbeComputeRequests = new List<CloudProbeComputeRequest>();
        private int cloudProbeShaderKernel;
        private readonly List<CloudProbeRequest> cloudProbeUserRequests = new List<CloudProbeRequest>();
        private readonly List<KeyValuePair<CloudProbeRequest, CloudProbeResult>> cloudProbeResults = new List<KeyValuePair<CloudProbeRequest, CloudProbeResult>>();

        private CommandBuffer weatherMapCommandBuffer;
        private System.Func<WeatherMakerCommandBuffer, bool> preSetupCommandBufferAction;
        private System.Action<WeatherMakerCommandBuffer, RenderTargetIdentifier, RenderTextureDescriptor> postProcessCommandBufferAction;
        private System.Action<WeatherMakerCommandBuffer> postSetupCommandBufferAction;
        private Texture2D whitePixel;
        private Texture2D blackPixel;

        private static readonly int[] sdfPrevIds = new int[]
        {
            Shader.PropertyToID("_PrevSdfTex0"),
            Shader.PropertyToID("_PrevSdfTex1"),
            Shader.PropertyToID("_PrevSdfTex2"),
            Shader.PropertyToID("_PrevSdfTex3"),
            Shader.PropertyToID("_PrevSdfTex4"),
            Shader.PropertyToID("_PrevSdfTex5"),
            Shader.PropertyToID("_PrevSdfTex6"),
            Shader.PropertyToID("_PrevSdfTex7"),
            Shader.PropertyToID("_PrevSdfTex8"),
            Shader.PropertyToID("_PrevSdfTex9"),
            Shader.PropertyToID("_PrevSdfTex10"),
        };

        private bool hasKuwaharaPostProcessor;
        private Vector4[] kuwaharaCoords;

        private void KuwaharaHack(CommandBuffer commandBuffer, int width, int height)
        {
            // So there is no way in a shader to initialize an array once before all the vertex and fragment pieces run,
            // hence this disgusting hack is necessary
            Material m;
            if (CloudPostProcessors == null ||
                CloudPostProcessors.Length == 0 ||
                (m = CloudPostProcessors.FirstOrDefault(p => p.CachedName().IndexOf("kuwahara", System.StringComparison.OrdinalIgnoreCase) >= 0)) == null)
            {
                return;
            }

            int radius = Math.Min(8, m.GetInt(WMS._Radius));
            Vector2 step;
            var R = new Vector4[4]
            {
                new Vector4(-radius, -radius, 0, 0),
                new Vector4(0, -radius, radius, 0),
                new Vector4(0, 0, radius, radius),
                new Vector4(-radius, 0, 0, radius)
            };
            int k, j, i;
            int index = 0;
            Vector2 texelSize = new Vector2(1.0f / (float)width, 1.0f / (float)height);
            kuwaharaCoords = (kuwaharaCoords == null ? new Vector4[324] : kuwaharaCoords);

            for (k = 0; k < 4; ++k)
			{
				for (step.y = (R[k].z * texelSize.y), j = (int)R[k].y; j <= R[k].w; ++j, step.y += texelSize.y)
				{
					for (step.x = (R[k].x * texelSize.x), i = (int)R[k].x; i <= R[k].z; ++i, step.x += texelSize.x)
					{
                        kuwaharaCoords[index++] = new Vector4(step.x, step.y, k, 0.0f);
					}
				}
			}

            commandBuffer.SetGlobalVectorArray(WMS._KuwaharaCoords, kuwaharaCoords);
            commandBuffer.SetGlobalInt(WMS._KuwaharaCoordsLength, index);
        }

        private Vector3 RandomRange(Vector3 velocity)
        {
            return new Vector3(randomizer.Random(-velocity.x, velocity.x),
                randomizer.Random(-velocity.y, velocity.y),
                randomizer.Random(-velocity.z, velocity.z));
        }

        private void GenerateWeatherMap(Camera camera)
        {
            if (currentRenderCloudProfile == null || camera == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                return;
            }

            weatherMapMaterialCopy.Update(WeatherMapMaterial);

            if (weatherMapMaterialCopy.Copy == null)
            {
                Debug.LogError("Must set weather map material on full screen cloud script");
                return;
            }

            if (ShadowMaterial == null)
            {
                Debug.LogError("Must set shadow material on full screen cloud script");
                return;
            }

            CreateWeatherMapTextures(camera);

            currentRenderCloudProfile.UpdateWeatherMap(this, weatherMapMaterialCopy, camera, cloudProbeShader, WeatherMapRenderTexture,
                (float)randomizer.Seed % 128.0f);

            if (weatherMapCommandBuffer == null)
            {
                weatherMapCommandBuffer = new CommandBuffer { name = "WeatherMapAndCloudShadows" };
            }

            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, weatherMapCommandBuffer);
            camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, weatherMapCommandBuffer);
            weatherMapCommandBuffer.Clear();
            if (WeatherMapRenderTexture != null)
            {
                if (WeatherMapOverride != null)
                {
                    Shader.SetGlobalInt(WMS._WeatherMakerWeatherMapTextureStatic, 1);
                    weatherMapCommandBuffer.SetGlobalTexture(WMS._MainTex, WeatherMapOverride);
                    WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, WeatherMapOverride, WeatherMapRenderTexture, WeatherMapMaterial, 1);
                }
                else if (currentRenderCloudProfile.WeatherMapRenderTextureOverride == null)
                {
                    Shader.SetGlobalInt(WMS._WeatherMakerWeatherMapTextureStatic, 0);

                    //Texture mask = (WeatherMapMaskOverride == null ? currentRenderCloudProfile.WeatherMapRenderTextureMask : WeatherMapMaskOverride);
                    //weatherMapCommandBuffer.SetGlobalTexture(WMS._WeatherMakerWeatherMapMaskTexture, mask);
                    //WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, mask, WeatherMapRenderTexture, weatherMapMaterialCopy, 0);

                    // render at reduced pixel count, blur upsample
                    weatherMapCommandBuffer.GetTemporaryRT(WMS._MainTex6,
                        new RenderTextureDescriptor(WeatherMapRenderTexture.width / 2, WeatherMapRenderTexture.height / 2, RenderTextureFormat.ARGBHalf),
                        FilterMode.Bilinear);
                    Texture mask = (WeatherMapMaskOverride == null ? currentRenderCloudProfile.WeatherMapRenderTextureMask : WeatherMapMaskOverride);
                    mask = (mask == null ? whitePixel : mask);
                    weatherMapCommandBuffer.SetGlobalTexture(WMS._WeatherMakerWeatherMapMaskTexture, mask);
                    WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, mask, WMS._MainTex6, weatherMapMaterialCopy, 0);
                    WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, WMS._MainTex6, WeatherMapRenderTexture);
                    weatherMapCommandBuffer.ReleaseTemporaryRT(WMS._MainTex6);
                }
                else
                {
                    Shader.SetGlobalInt(WMS._WeatherMakerWeatherMapTextureStatic, 1);
                    weatherMapCommandBuffer.SetGlobalTexture(WMS._MainTex, currentRenderCloudProfile.WeatherMapRenderTextureOverride);
                    WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, currentRenderCloudProfile.WeatherMapRenderTextureOverride, WeatherMapRenderTexture, WeatherMapMaterial, 1);
                }

                weatherMapCommandBuffer.SetGlobalTexture(WMS._WeatherMakerWeatherMapTexture, WeatherMapRenderTexture);
                int weatherMapSize = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudWeatherMapSize;
                WeatherMakerFullScreenEffect.ApplyPostProcessing(weatherMapCommandBuffer, WeatherMapPostProcessors,
                    WeatherMapRenderTexture, new RenderTextureDescriptor(weatherMapSize, weatherMapSize, RenderTextureFormat.ARGBHalf));

                RenderTextureDescriptor desc = new RenderTextureDescriptor(CloudShadowRenderTexture.width, CloudShadowRenderTexture.height, RenderTextureFormat.RHalf, 0) { sRGB = false };
                weatherMapCommandBuffer.GetTemporaryRT(WMS._MainTex6, desc, FilterMode.Bilinear);
                WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, 0, WMS._MainTex6, ShadowMaterial, 1);
                WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, WMS._MainTex6, CloudShadowRenderTexture, ShadowMaterial, 2);
                weatherMapCommandBuffer.ReleaseTemporaryRT(WMS._MainTex6);

                if (WeatherMapSdfMaterial != null && CloudProfile != null && CloudProfile.CloudLayerVolumetric1 != null &&
                    CloudProfile.CloudLayerVolumetric1.CloudSdfThreshold > 0.0f && CloudProfile.CloudLayerVolumetric1.CloudSdfThreshold < 1.0f &&
                    VolumetricCloudBoxRemap == null)
                {
                    int iterations = 0;
                    for (int num = WeatherMapRenderTexture.width / 2; num > 2; num /= 2)
                    {
                        iterations++;
                    }

                    // downsample sdf
                    for (int i = 0; i <= iterations; i++)
                    {
                        int currentSdfTextureSize = WeatherMapRenderTexture.width / (1 << i);
                        weatherMapCommandBuffer.GetTemporaryRT(sdfPrevIds[i], currentSdfTextureSize, currentSdfTextureSize, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                        if (i == 0)
                        {
                            WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, WeatherMapRenderTexture, sdfPrevIds[i]);
                        }
                        else
                        {
                            weatherMapCommandBuffer.SetGlobalTexture(WMS._PrevSdfTex, sdfPrevIds[i - 1]);
                            WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, 0, sdfPrevIds[i], WeatherMapSdfMaterial, 0);
                        }
                    }

                    // sdf calculation
                    for (int i = iterations; i >= 0; i--)
                    {
                        float numPixelsRepresented = (i < 1 ? 1.0f : (1.0f / (1 << i)));
                        //float numPixelsRepresented = (i / (float)iterations);
                        weatherMapCommandBuffer.SetGlobalVector(WMS._SdfPixelSize, new Vector4(numPixelsRepresented, 0.0f, CloudProfile.CloudLayerVolumetric1.CloudSdfThreshold, 0.0f));
                        weatherMapCommandBuffer.SetGlobalTexture(WMS._PrevSdfTex, sdfPrevIds[i]);
                        if (i == 0)
                        {
                            WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, 0, WeatherMapRenderTexture, WeatherMapSdfMaterial, 1);
                        }
                        else
                        {
                            WeatherMakerFullScreenEffect.Blit(weatherMapCommandBuffer, 0, sdfPrevIds[i - 1], WeatherMapSdfMaterial, 1);
                        }
                    }

                    // release temp textures
                    for (int i = 1; i <= iterations; i++)
                    {
                        weatherMapCommandBuffer.ReleaseTemporaryRT(sdfPrevIds[i]);
                    }
                }

                if (camera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    camera.AddCommandBuffer(CameraEvent.BeforeReflections, weatherMapCommandBuffer);
                }
                else
                {
                    camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, weatherMapCommandBuffer);
                }
            }
        }

        private void UpdateShaderProperties(WeatherMakerCommandBuffer b)
        {
            if (currentRenderCloudProfile == null || WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                return;
            }

            if (AuroraProfile != null)
            {
                AuroraProfile.UpdateShaderVariables();
            }

            WeatherMakerCloudVolumetricProfileScript vol = currentRenderCloudProfile.CloudLayerVolumetric1;
            int sampleCount = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightRaySampleCount;
            SetShaderCloudParameters(b.Material, b.Camera);
            float decayMultiplier = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightRayFalloffMultiplier;

            // check if shafts are disabled
            if (sampleCount <= 0 ||
                decayMultiplier <= 0.001f ||
                vol.CloudCover.GetLastValue(randomizer) <= 0.001f ||
                vol.CloudDirLightRayBrightness <= 0.001f ||
                b.CameraType != WeatherMakerCameraType.Normal ||
                vol.CloudDirLightRayTintColor.a <= 0.0f)
            {
                b.Material.SetInt(WMS._WeatherMakerFogSunShaftMode, 0);
                return;
            }

            // check each dir light to see if it can do shafts
            bool atLeastOneLightHasShafts = false;
            foreach (WeatherMakerCelestialObjectScript obj in WeatherMakerLightManagerScript.Instance.Suns.Union(WeatherMakerLightManagerScript.Instance.Moons))
            {
                if (obj != null && obj.OrbitTypeIsPerspective && obj.LightIsOn && obj.ViewportPosition.z > 0.0f && obj.ShaftMultiplier > 0.0f)
                {
                    atLeastOneLightHasShafts = true;
                    break;
                }
            }

            if (atLeastOneLightHasShafts)
            {
                // sun shafts are visible
                Vector4 dm = WeatherMakerCloudVolumetricProfileScript.CloudDirLightRayDitherMagic;
                float dither = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightRayDither;
                b.Material.SetInt(WMS._WeatherMakerFogSunShaftMode, 1);
                bool gamma = (QualitySettings.activeColorSpace == ColorSpace.Gamma);
                float brightness = vol.CloudDirLightRayBrightness * (gamma ? 1.0f : 0.33f);
                b.Material.SetVector(WMS._WeatherMakerFogSunShaftsParam1, new Vector4(vol.CloudDirLightRaySpread / (float)sampleCount, (float)sampleCount, brightness, 1.0f / (float)sampleCount));
                b.Material.SetVector(WMS._WeatherMakerFogSunShaftsParam2, new Vector4(vol.CloudDirLightRayStepMultiplier, vol.CloudDirLightRayDecay * decayMultiplier, dither, 0.0f));
                b.Material.SetVector(WMS._WeatherMakerFogSunShaftsTintColor, new Vector4(vol.CloudDirLightRayTintColor.r * vol.CloudDirLightRayTintColor.a, vol.CloudDirLightRayTintColor.g * vol.CloudDirLightRayTintColor.a,
                    vol.CloudDirLightRayTintColor.b * vol.CloudDirLightRayTintColor.a, vol.CloudDirLightRayTintColor.a));
                b.Material.SetVector(WMS._WeatherMakerFogSunShaftsDitherMagic, new Vector4(dm.x * Screen.width, dm.y * Screen.height,
                    dm.z * Screen.width, dm.w * Screen.height));
                b.Material.SetFloat(WMS._WeatherMakerFogSunShaftsBackgroundIntensity, 0.0f);
                b.Material.SetColor(WMS._WeatherMakerFogSunShaftsBackgroundTintColor, Color.clear);
            }
            else
            {
                b.Material.SetInt(WMS._WeatherMakerFogSunShaftMode, 0);
            }
        }

        private void DeleteAndTransitionRenderProfile(WeatherMakerCloudProfileScript newProfile)
        {
            lastProfileOriginal = newProfile;
            if (newProfile != null)
            {
                if (newProfile.name.IndexOf("(Clone)") < 0 && WeatherMakerScript.CloneProfiles && Application.isPlaying)
                {
                    newProfile = newProfile.Clone();
                }
            }
            if (currentRenderCloudProfile != null)
            {
                if (newProfile != null)
                {
                    currentRenderCloudProfile.CopyStateTo(newProfile);
                }
                if (currentRenderCloudProfile != emptyProfile)
                {
                    if (currentRenderCloudProfile.CloudLayer1.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile.CloudLayer1);
                    }
                    if (currentRenderCloudProfile.CloudLayer2.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile.CloudLayer2);
                    }
                    if (currentRenderCloudProfile.CloudLayer3.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile.CloudLayer3);
                    }
                    if (currentRenderCloudProfile.CloudLayer4.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile.CloudLayer4);
                    }
                    if (currentRenderCloudProfile.CloudLayerVolumetric1.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile.CloudLayerVolumetric1);
                    }
                    if (currentRenderCloudProfile.name.IndexOf("(Clone)") >= 0)
                    {
                        DestroyImmediate(currentRenderCloudProfile);
                    }
                }
            }
            currentRenderCloudProfile = _CloudProfile = lastProfile = newProfile;
            if (currentRenderCloudProfile != null)
            {
                currentRenderCloudProfile.randomizer = randomizer;
            }
        }

        private void EnsureProfile()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            if (emptyProfile == null)
            {
                emptyProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerCloudProfileScript>("WeatherMakerCloudProfile_None");
                if (Application.isPlaying)
                {
                    emptyProfile = emptyProfile.Clone();
                }
            }
            if (_CloudProfile == null)
            {
                _CloudProfile = lastProfileOriginal = emptyProfile;
            }
            else if (WeatherMakerScript.CloneProfiles && _CloudProfile.name.IndexOf("(Clone)", StringComparison.OrdinalIgnoreCase) < 0 && Application.isPlaying)
            {
                _CloudProfile = _CloudProfile.Clone();
            }
            currentRenderCloudProfile = _CloudProfile;
            if (effect == null)
            {
                lastProfile = currentRenderCloudProfile = _CloudProfile;
                effect = new WeatherMakerFullScreenEffect
                {
                    CommandBufferName = "WeatherMakerFullScreenCloudsScript",
                    RenderQueue = RenderQueue
                };
            }
        }

        private void Update()
        {
            if (CloudSeed != lastCloudSeed)
            {
                CloudSeedChanged();
            }

            if (WeatherMakerScript.Instance == null || cameraValues == null)
            {
                return;
            }

            // cleanup camera values
            for (int i = cameraValues.Count - 1; i >= 0; i--)
            {
                if (cameraValues[i].Camera == null)
                {
                    cameraValues.RemoveAt(i);
                }
            }

            if (AuroraProfile == null)
            {
                AuroraProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerAuroraProfileScript>("WeatherMakerAuroraProfile_None");
            }

            if (CloudProfile != null)
            {
                CloudProfile.randomizer = randomizer;
                if (WeatherMakerSkySphereScript.Instance != null)
                {
                    CloudProfile.AtmosphereProfile = WeatherMakerSkySphereScript.Instance.SkySphereProfile.AtmosphereProfile;
                }
                if (CloudProfile.AuroraProfile != AuroraProfile)
                {
                    WeatherMakerAuroraProfileScript oldProfile = CloudProfile.AuroraProfile;
                    WeatherMakerAuroraProfileScript newProfile = AuroraProfile;
                    CloudProfile.AuroraProfile = newProfile;
                    animatingAurora = true;
                    newProfile.AnimateFrom(oldProfile, AuroraAnimationDuration, "WeatherMakerFullScreenCloudsScriptAurora", () => animatingAurora = false);
                }
                CloudGlobalShadow = CloudProfile.CloudGlobalShadow;
            }
            if (AuroraProfile != null && !animatingAurora)
            {
                AuroraProfile.UpdateAnimationProperties();
            }

            CleanupCloudProbes();

            // debug only
            if (CloudRealtimeNoiseProfile != null)
            {
                CloudRealtimeNoiseProfile.SetGlobalShader();
            }
        }

        private void LateUpdate()
        {
            // ensure cover is 0 in case no one else sets it
            Shader.SetGlobalFloat(WMS._CloudCoverVolumetric, 0.0f);
            Shader.SetGlobalFloatArray(WMS._CloudCover, emptyFloatArray);

            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                return;
            }

            DownSampleScale = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDownsampleScale;
            DownSampleScalePostProcess = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDownsampleScalePostProcess;
            TemporalReprojection = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudTemporalReprojectionSize;
            if (!WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds)
            {
                // don't use temporal reprojection for flat clouds
                TemporalReprojection = WeatherMakerTemporalReprojectionSize.None;
            }

            if (CloudProfile != lastProfile)
            {
                if (CloudProfile != null)
                {
                    CloudProfile.EnsureNonNullLayers();
                }
                DeleteAndTransitionRenderProfile(CloudProfile);
            }

            updateShaderPropertiesAction = (updateShaderPropertiesAction ?? UpdateShaderProperties);
            if (currentRenderCloudProfile != null)
            {
                currentRenderCloudProfile.Update();
                if (effect != null)
                {
                    effect.SetupEffect(Material, FullScreenMaterial, BlurMaterial, BilateralBlurMaterial, BlurShader, DownSampleScale, WeatherMakerDownsampleScale.Disabled,
                        (WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds ? DownSampleScalePostProcess : WeatherMakerDownsampleScale.Disabled),
                        TemporalReprojectionMaterial, TemporalReprojection, updateShaderPropertiesAction, CloudProfile.CloudsEnabled,
                        preSetupCommandBufferAction, postProcessCommandBufferAction, postSetupCommandBufferAction);
                }
            }
        }

        private void CreateWeatherMapTextures(Camera camera)
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            var wrapMode = TextureWrapMode.Mirror;
            if (WeatherMapRenderTexture == null || WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudWeatherMapSize != WeatherMapRenderTexture.width)
            {
                WeatherMapRenderTexture = WeatherMakerFullScreenEffect.DestroyRenderTexture(WeatherMapRenderTexture);
                int size = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudWeatherMapSize;
                WeatherMapRenderTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                WeatherMapRenderTexture.name = "WeatherMakerWeatherMapRenderTexture";
                WeatherMapRenderTexture.wrapMode = wrapMode;
                WeatherMapRenderTexture.filterMode = FilterMode.Trilinear;
                WeatherMapRenderTexture.autoGenerateMips = true;
                WeatherMapRenderTexture.antiAliasing = 1;
                WeatherMapRenderTexture.anisoLevel = 1;
                WeatherMapRenderTexture.useMipMap = true;
                WeatherMapRenderTexture.Create();
            }

            if (CloudShadowRenderTexture == null || WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowTextureSize != CloudShadowRenderTexture.width)
            {
                CloudShadowRenderTexture = WeatherMakerFullScreenEffect.DestroyRenderTexture(CloudShadowRenderTexture);
                int size = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowTextureSize;
                CloudShadowRenderTexture = new RenderTexture(size, size, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
                CloudShadowRenderTexture.name = "WeatherMakerCloudShadowTexture";
                CloudShadowRenderTexture.wrapMode = wrapMode;
                CloudShadowRenderTexture.filterMode = FilterMode.Trilinear;
                CloudShadowRenderTexture.autoGenerateMips = true;
                CloudShadowRenderTexture.antiAliasing = 1;
                CloudShadowRenderTexture.anisoLevel = 1;
                CloudShadowRenderTexture.useMipMap = true;
                CloudShadowRenderTexture.Create();
            }

            Shader.SetGlobalTexture(WMS._WeatherMakerCloudShadowTexture, CloudShadowRenderTexture);
        }

        private void OnEnable()
        {
            if (Application.isPlaying && (CloudSeed < 0 || regenerateCloudSeedOnEnable))
            {
                regenerateCloudSeedOnEnable = true;
                CloudSeed = UnityEngine.Random.Range(0, int.MaxValue);
            }
            CloudSeedChanged();

            if (TemporalReprojectionMaterial == null || WeatherMakerScript.Instance == null)
            {
                return;
            }

            alphaBlitMaterial = Resources.Load<Material>("WeatherMakerAlphaBlitMaterial");
            handleCloudProbeResultCallback = HandleAsyncGpuCloudProbe;
            if (cloudProbeShader == null && SystemInfo.supportsComputeShaders && SystemInfo.supportsAsyncGPUReadback)
            {
                cloudProbeShader = WeatherMakerScript.Instance.LoadResource<ComputeShader>("WeatherMakerCloudProbeShader");
                if (cloudProbeShader != null)
                {
                    cloudProbeShaderKernel = cloudProbeShader.FindKernel("CSMain");
                }
            }

            preSetupCommandBufferAction = PreSetupCommandBuffer;
            postProcessCommandBufferAction = PostProcessCommandBuffer;
            postSetupCommandBufferAction = PostSetupCommandBuffer;
            shaderProps = new WeatherMakerShaderPropertiesScript();
            WeatherMakerScript.EnsureInstance(this, ref instance);
            EnsureProfile();
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreRender(CameraPreRender, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
            cloudCollider = GetComponent<Collider>();
            Shader.SetGlobalFloat(WMS._WeatherMakerAuroraSeed, WeatherMakerRandomizer.Unity.Random(-65536.0f, 65536.0f));
            if (AuroraProfile == null)
            {
                AuroraProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerAuroraProfileScript>("WeatherMakerAuroraProfile_None");
            }
            if (whitePixel == null)
            {
                whitePixel = new Texture2D(1, 1, TextureFormat.ARGB32, false, true) { name = "WhitePixel" };
                whitePixel.SetPixel(0, 0, Color.white);
                whitePixel.Apply();
            }
            if (blackPixel == null)
            {
                blackPixel = new Texture2D(1, 1, TextureFormat.ARGB32, false, true) { name = "BlackPixel" };
                blackPixel.SetPixel(0, 0, Color.clear);
                blackPixel.Apply();
            }
        }

        private void OnDisable()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
            WeatherMapRenderTexture = WeatherMakerFullScreenEffect.DestroyRenderTexture(WeatherMapRenderTexture);
            CloudShadowRenderTexture = WeatherMakerFullScreenEffect.DestroyRenderTexture(CloudShadowRenderTexture);
            weatherMapMaterialCopy.Dispose();

            DisposeCloudProbes();

            if (weatherMapCommandBuffer != null)
            {
                weatherMapCommandBuffer.Clear();
                weatherMapCommandBuffer.Release();
                weatherMapCommandBuffer = null;
            }
            if (whitePixel != null)
            {
                GameObject.DestroyImmediate(whitePixel);
            }
        }

        private void OnDestroy()
        {
            if (effect != null)
            {
                effect.Dispose();
            }
            DeleteAndTransitionRenderProfile(null);
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreRender(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPostRender(this);
            }
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        private static readonly float[] emptyFloatArray = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
        internal void SetShaderCloudParameters(Material material, Camera camera)
        {
            if (WeatherMakerLightManagerScript.Instance != null && currentRenderCloudProfile != null)
            {
                currentRenderCloudProfile.SetShaderCloudParameters(material, cloudProbeShader, camera, WeatherMapRenderTexture);
                DispatchCloudProbes(camera);
                shaderProps.Update(material);
                WeatherMakerLightManagerScript.Instance.UpdateShaderVariables(null, shaderProps, cloudCollider);
            }
        }

        private void CloudSeedChanged()
        {
            lastCloudSeed = CloudSeed;
            randomizer = new DigitalRuby.WeatherMaker.WeatherMakerRandomizer();
            randomizer.Seed = CloudSeed < 0 ? UnityEngine.Random.Range(0, int.MaxValue) : CloudSeed;
            CloudProfile.randomizer = randomizer;
        }

        private bool DisallowCamera(Camera camera)
        {
            bool allowReflections;
            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                allowReflections = AllowReflections;
            }
            else
            {
                allowReflections = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudAllowReflections;
            }
            return (effect == null || camera.orthographic || WeatherMakerScript.ShouldIgnoreCamera(this, camera, !allowReflections));
        }

        private void RenderCelestialObject(WeatherMakerCelestialObjectScript obj, Camera camera, CommandBuffer cmd)
        {
            if (obj.MeshFilter == null || obj.Renderer == null || obj.Renderer.sharedMaterial == null)
            {
                return;
            }

            if (obj.IsSun)
            {
                float sunScale = 10.0f;
                WeatherMakerFullScreenEffect.DrawMesh(cmd, camera.transform.position, obj.transform.localRotation,
                    new Vector3(sunScale, sunScale, sunScale), obj.MeshFilter.sharedMesh, obj.Renderer.sharedMaterial);
            }
            else
            {
                // moon slightly bigger at horizon
                float far = camera.farClipPlane * 0.5f;
                float scale = far * obj.Scale;
                if (obj.ScaleMoonAtHorizon)
                {
                    scale /= Mathf.Clamp(Mathf.Abs(obj.transform.forward.y) * 3.0f, 0.8f, 1.0f);
                }
                WeatherMakerFullScreenEffect.DrawMesh(cmd, camera.transform.position - (obj.transform.forward * far),
                    obj.transform.localRotation, new Vector3(scale, scale, scale), obj.MeshFilter.sharedMesh, obj.Renderer.sharedMaterial);
            }
        }

        private bool PreSetupCommandBuffer(WeatherMakerCommandBuffer cmdBuffer)
        {
            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerLightManagerScript.Instance == null || WeatherMakerCommandBufferManagerScript.Instance == null)
            {
                return false;
            }

            var dirLight = WeatherMakerLightManagerScript.Instance.PrimaryDirectionalLight;
            bool atmosphereOn = WeatherMakerScript.Instance.PerformanceProfile.AtmosphereQuality != WeatherMakerAtmosphereQuality.Disabled && SystemInfo.supportsComputeShaders &&
                dirLight != null && dirLight.LightIsOn;

            // draw sky
            if (WeatherMakerSkySphereScript.Instance != null)
            {
                MeshFilter filter = WeatherMakerSkySphereScript.Instance.MeshFilter;
                Renderer renderer = WeatherMakerSkySphereScript.Instance.MeshRenderer;
                Transform obj = WeatherMakerSkySphereScript.Instance.transform;
                float skyScale = Mathf.Min(1000.0f, cmdBuffer.Camera.farClipPlane * 0.95f);

                // render skybox for clouds
                RenderTextureFormat format = (cmdBuffer.Camera.allowHDR ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
                cmdBuffer.CommandBuffer.GetTemporaryRT(WMS._WeatherMakerBackgroundSkyTexture, WeatherMakerFullScreenEffect.GetRenderTextureDescriptor((int)DownSampleScale, 0, 1, format, 0, cmdBuffer.Camera));
                cmdBuffer.CommandBuffer.SetRenderTarget(WMS._WeatherMakerBackgroundSkyTexture, 0, CubemapFace.Unknown, -1);
                cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, (float)DownSampleScale);

                // draw sky to downsampled texture
                WeatherMakerFullScreenEffect.DrawMesh(cmdBuffer.CommandBuffer,
                    cmdBuffer.Camera.transform.position,
                    obj.rotation,
                    new Vector3(skyScale, skyScale, skyScale),
                    filter.sharedMesh,
                    renderer.sharedMaterial,
                    1);

                cmdBuffer.CommandBuffer.SetRenderTarget(WeatherMakerFullScreenEffect.CameraTargetIdentifier(), 0, CubemapFace.Unknown, -1);
                cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, (float)1.0f);

                // draw sky to camera target if it was asked for
                if (cmdBuffer.Camera.clearFlags != CameraClearFlags.Skybox || RenderSettings.skybox == null)
                {
                    WeatherMakerFullScreenEffect.Blit(cmdBuffer.CommandBuffer, WMS._WeatherMakerBackgroundSkyTexture, WeatherMakerFullScreenEffect.CameraTargetIdentifier(), renderer.sharedMaterial, 2);
                }
            }

            foreach (WeatherMakerCelestialObjectScript obj in WeatherMakerLightManagerScript.Instance.Suns)
            {
                RenderCelestialObject(obj, cmdBuffer.Camera, cmdBuffer.CommandBuffer);
            }
            foreach (WeatherMakerCelestialObjectScript obj in WeatherMakerLightManagerScript.Instance.Moons)
            {
                RenderCelestialObject(obj, cmdBuffer.Camera, cmdBuffer.CommandBuffer);
            }

            if (atmosphereOn)
            {
                RenderTextureFormat defaultFormat = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
                bool lightShafts = (BilateralBlurMaterial != null && WeatherMakerScript.Instance.PerformanceProfile.AtmosphericLightShaftSampleCount > 0);
                if (lightShafts)
                {
                    cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._WeatherMakerAtmosphereLightShaftEnable, 1.0f);
                    cmdBuffer.CommandBuffer.GetTemporaryRT(WMS._MainTex3, WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(2, 0, 1, RenderTextureFormat.RHalf, 0, cmdBuffer.Camera), FilterMode.Bilinear);
                    cmdBuffer.CommandBuffer.GetTemporaryRT(WMS._MainTex2, WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(1, 0, 1, RenderTextureFormat.RHalf, 0, cmdBuffer.Camera), FilterMode.Bilinear);
                    WeatherMakerFullScreenEffect.Blit(cmdBuffer.CommandBuffer, WeatherMakerFullScreenEffect.CameraTargetIdentifier(), WMS._MainTex3, cmdBuffer.Material, 5);
                    cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._SrcBlendMode, (float)BlendMode.One);
                    cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._DstBlendMode, (float)BlendMode.Zero);
                    WeatherMakerFullScreenEffect.AttachBilateralBlur(cmdBuffer.CommandBuffer, BilateralBlurMaterial, WMS._MainTex3, WMS._MainTex2, RenderTextureFormat.RHalf, WeatherMakerDownsampleScale.HalfResolution, cmdBuffer.Camera);
                    cmdBuffer.CommandBuffer.SetGlobalTexture(WMS._WeatherMakerAtmosphereLightShaftTexture, WMS._MainTex2);
                }
                else
                {
                    cmdBuffer.CommandBuffer.SetGlobalFloat(WMS._WeatherMakerAtmosphereLightShaftEnable, 0.0f);
                }
                cmdBuffer.CommandBuffer.GetTemporaryRT(WMS._MainTex5, WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(1, 0, 1, defaultFormat, 0, cmdBuffer.Camera), FilterMode.Bilinear);

                // save background for blending atmospheric scattering
                cmdBuffer.CommandBuffer.SetGlobalTexture(WMS._MainTex2, WeatherMakerFullScreenEffect.CameraTargetIdentifier());

                // render atmospheric scattering into temporary texture
                WeatherMakerFullScreenEffect.Blit(cmdBuffer.CommandBuffer, WeatherMakerFullScreenEffect.CameraTargetIdentifier(), WMS._MainTex5, cmdBuffer.Material, 4);

                // re-draw temporary texture on to frame buffer
                WeatherMakerFullScreenEffect.Blit(cmdBuffer.CommandBuffer, WMS._MainTex5, WeatherMakerFullScreenEffect.CameraTargetIdentifier());
                cmdBuffer.CommandBuffer.ReleaseTemporaryRT(WMS._MainTex5);

                if (lightShafts)
                {
                    cmdBuffer.CommandBuffer.ReleaseTemporaryRT(WMS._MainTex3);
                    cmdBuffer.CommandBuffer.ReleaseTemporaryRT(WMS._MainTex2);
                }
            }

            foreach (System.Action<WeatherMakerCommandBuffer> action in BeforeCloudsRenderHooks)
            {
                action.Invoke(cmdBuffer);
            }

            return CloudProfile.CloudsEnabled;
        }

        private void PostProcessCommandBuffer(WeatherMakerCommandBuffer cmdBuffer, RenderTargetIdentifier target, RenderTextureDescriptor desc)
        {
            KuwaharaHack(cmdBuffer.CommandBuffer, desc.width, desc.height);
            WeatherMakerFullScreenEffect.ApplyPostProcessing(cmdBuffer.CommandBuffer,
                CloudPostProcessors,
                target,
                desc);
        }

        private void PostSetupCommandBuffer(WeatherMakerCommandBuffer cmdBuffer)
        {
            // release sky texture if it exists
            if (WeatherMakerSkySphereScript.Instance != null)
            {
                cmdBuffer.CommandBuffer.ReleaseTemporaryRT(WMS._WeatherMakerBackgroundSkyTexture);
            }
        }

        private void CameraPreCull(Camera camera)
        {
            if (DisallowCamera(camera))
            {
                return;
            }

            // setup weather map and positions for this camera
            if (WeatherMakerScript.GetCameraType(camera) == WeatherMakerCameraType.Normal && WeatherMakerCommandBufferManagerScript.CameraStackCount < 2)
            {
                GenerateWeatherMap(camera);
            }

            effect.PreCullCamera(camera);
        }

        private void DisposeCloudProbes()
        {
            // lock for pool access
            lock (cloudProbeComputeRequests)
            {
                foreach (CloudProbeComputeRequest request in cloudProbeComputeRequests)
                {
                    request.Dispose();
                }
                cloudProbeComputeRequests.Clear();
            }

            cloudProbeUserRequests.Clear();
            cloudProbeResults.Clear();
            foreach (ComputeBuffer buffer in computeBufferPool)
            {
                buffer.Dispose();
            }
            computeBufferPool.Clear();
        }

        private System.Action<AsyncGPUReadbackRequest> handleCloudProbeResultCallback;

        private void HandleAsyncGpuCloudProbe(AsyncGPUReadbackRequest request)
        {
            CloudProbeComputeRequest requestToHandle = null;

            // lock for pool access
            lock (cloudProbeComputeRequests)
            {
                foreach (CloudProbeComputeRequest request2 in cloudProbeComputeRequests)
                {
                    if (request2.Request.Equals(request))
                    {
                        requestToHandle = request2;
                        break;
                    }
                }
            }

            if (requestToHandle != null)
            {
                HandleCloudProbeResult(requestToHandle);
            }
        }

        private void HandleCloudProbeResult(CloudProbeComputeRequest request)
        {
            Unity.Collections.NativeArray<CloudProbe> samples = request.Request.GetData<CloudProbe>();
            for (int userRequestIndex = 0; userRequestIndex < request.UserRequests.Count; userRequestIndex++)
            {
                int existingResultIndex = -1;
                for (int resultIndex = 0; resultIndex < cloudProbeResults.Count; resultIndex++)
                {
                    if (cloudProbeResults[resultIndex].Key.Camera == request.Camera)
                    {
                        existingResultIndex = resultIndex;
                        break;
                    }
                }
                CloudProbeResult result = new CloudProbeResult
                {
                    DensitySource = samples[userRequestIndex].Source.x,
                    DensityTarget = samples[userRequestIndex].Source.y,
                    DensityRaySum = samples[userRequestIndex].Source.z,
                    DensityRayAverage = samples[userRequestIndex].Source.z * oneOver64
                };
                if (existingResultIndex == -1)
                {
                    cloudProbeResults.Add(new KeyValuePair<CloudProbeRequest, CloudProbeResult>(request.UserRequests[userRequestIndex], result));
                }
                else
                {
                    cloudProbeResults[existingResultIndex] = new KeyValuePair<CloudProbeRequest, CloudProbeResult>(request.UserRequests[userRequestIndex], result);
                }
            }

            // lock for pool access
            lock (cloudProbeComputeRequests)
            {
                request.Dispose();
                cloudProbeComputeRequests.Remove(request);
            }
        }

        private void CleanupCloudProbes()
        {
            cloudProbeUserRequests.Clear();
            for (int i = cloudProbeResults.Count - 1; i >= 0; i--)
            {
                if (cloudProbeResults[i].Key.Camera == null ||
                    cloudProbeResults[i].Key.Target == null ||
                    cloudProbeResults[i].Key.Source == null ||
                    !cloudProbeResults[i].Key.Source.gameObject.activeInHierarchy)
                {
                    cloudProbeResults.RemoveAt(i);
                }
            }
        }

        private void DispatchCloudProbes(Camera camera)
        {
            if (cloudProbeShader != null && cloudProbeUserRequests.Count != 0)
            {
                CloudProbeComputeRequest existingRequest;

                // lock for pool access
                lock (cloudProbeComputeRequests)
                {
                    // if we have a pending request, don't stack requests, just wait for it to be done
                    existingRequest = cloudProbeComputeRequests.FirstOrDefault(r => r.Camera == camera);
                    if (existingRequest != null)
                    {
                        return;
                    }
                }

                tempRequests.Clear();
                tempRequests.AddRange(cloudProbeUserRequests.Where(r => r.Camera == camera));
                if (tempRequests.Count == 0)
                {
                    return;
                }

                if (probeRequestPool.Count == 0)
                {
                    existingRequest = new CloudProbeComputeRequest();
                }
                else
                {
                    // lock for pool access
                    lock (cloudProbeComputeRequests)
                    {
                        existingRequest = probeRequestPool[probeRequestPool.Count - 1];
                        probeRequestPool.RemoveAt(probeRequestPool.Count - 1);
                    }
                    existingRequest.Reset();
                }

                ComputeBuffer computeBuffer = null;

                // lock for pool access
                lock (cloudProbeComputeRequests)
                {
                    for (int i = computeBufferPool.Count - 1; i >= 0; i--)
                    {
                        if (computeBufferPool[i].count == tempRequests.Count)
                        {
                            computeBuffer = computeBufferPool[i];
                            computeBufferPool.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (computeBuffer == null)
                {
                    computeBuffer = new ComputeBuffer(tempRequests.Count, 32);
                }
                existingRequest.Camera = camera;
                existingRequest.UserRequests.AddRange(tempRequests);
                existingRequest.Shader = cloudProbeShader;
                existingRequest.Result = computeBuffer;
                for (int i = 0; i < tempRequests.Count; i++)
                {
                    existingRequest.SamplesList.Add(new CloudProbe
                    {
                        Source = (tempRequests[i].Source == null ? Vector4.zero : (Vector4)tempRequests[i].Source.position),
                        Target = (tempRequests[i].Target == null ? Vector4.zero : (Vector4)tempRequests[i].Target.position)
                    });
                }
                existingRequest.Result.SetData(existingRequest.SamplesList);

                // lock for pool access
                lock (cloudProbeComputeRequests)
                {
                    existingRequest.Shader.SetBuffer(cloudProbeShaderKernel, "probe", existingRequest.Result);
                    existingRequest.Shader.Dispatch(cloudProbeShaderKernel, Mathf.CeilToInt(existingRequest.SamplesList.Count / 4.0f), 1, 1);
                    cloudProbeComputeRequests.Add(existingRequest);
                    existingRequest.Request = AsyncGPUReadback.Request(existingRequest.Result, handleCloudProbeResultCallback);
                }
            }
        }

        private void CameraPreRender(Camera camera)
        {
            if (DisallowCamera(camera))
            {
                return;
            }

            effect.PreRenderCamera(camera);
        }

        private void CameraPostRender(Camera camera)
        {
            if (DisallowCamera(camera))
            {
                return;
            }

            effect.PostRenderCamera(camera);
            UpdateCameraValues(camera);
        }

        private void UpdateCameraValues(Camera camera)
        {
            // add new cameras
            foreach (CameraValues value in cameraValues)
            {
                if (value.Camera == camera)
                {
                    return;
                }
            }
            cameraValues.Add(new CameraValues { Camera = camera, FieldOfView = camera.fieldOfView });
        }

        /// <summary>
        /// Get cloud probe results
        /// </summary>
        /// <param name="camera">The camera to get cloud density with</param>
        /// <param name="source">The source transform to get cloud density with</param>
        /// <param name="dest">The destination transform to get cloud density with, set to null or source if sampling just a single point</param>
        /// <returns>Cloud probe result</returns>
        public CloudProbeResult GetCloudProbe(Camera camera, Transform source, Transform dest)
        {
            if (cloudProbeShader != null)
            {
                dest = (dest == null ? source : dest);
                foreach (var kv in cloudProbeResults)
                {
                    if (kv.Key.Camera == camera && kv.Key.Source == source && kv.Key.Target == dest)
                    {
                        return kv.Value;
                    }
                }
            }

            return new CloudProbeResult();
        }

        /// <summary>
        /// Request a cloud probe for a transform, must be called every frame as these are cleared every frame. Call from camera pre cull event.
        /// </summary>
        /// <param name="camera">Current camera</param>
        /// <param name="source">Transform to request</param>
        /// <param name="target">Target or null for just the point of the transform. If target is set, a ray is cast out and a cloud density sample accumulated.</param>
        /// <returns>True if added, false if failure or there are already pending requests for this camera</returns>
        public bool RequestCloudProbe(Camera camera, Transform source, Transform target)
        {
            if (source == null || cloudProbeShader == null || WeatherMakerScript.GetCameraType(camera) != WeatherMakerCameraType.Normal || !enabled || !gameObject.activeInHierarchy)
            {
                return false;
            }

            // if we have a pending request for this camera, source and target, do not add new request
            target = (target == null ? source : target);
            foreach (var cloudProbeRequest in cloudProbeUserRequests)
            {
                if (cloudProbeRequest.Camera == camera ||
                    cloudProbeRequest.Source == source ||
                    cloudProbeRequest.Target == target)
                {
                    // a request already exists, don't make another one
                    return false;
                }
            }

            cloudProbeUserRequests.Add(new CloudProbeRequest { Camera = camera, Source = source, Target = target });
            return true;
        }

        /// <summary>
        /// Show clouds animated. Animates cover, density, light absorption and color. To ensure smooth animations, all noise textures on all layers in both profiles should match.
        /// </summary>
        /// <param name="duration">Transition duration in seconds</param>
        /// <param name="profileName">Cloud profile name</param>
        /// <param name="tweenKey">Tween key</param>
        public void ShowCloudsAnimated(float duration, string profileName, string tweenKey = null)
        {
            if (WeatherMakerScript.Instance != null)
            {
                ShowCloudsAnimated(WeatherMakerScript.Instance.LoadResource<WeatherMakerCloudProfileScript>(profileName), 0.0f, duration, string.Empty);
            }
        }

        /// <summary>
        /// Show clouds animated. Animates cover, density, sharpness, light absorption and color. To ensure smooth animations, all noise textures on all layers in both profiles should match.
        /// </summary>
        /// <param name="newProfile">Cloud profile, or pass null to hide clouds</param>
        /// <param name="transitionDelay">Delay before transition</param>
        /// <param name="transitionDuration">Transition duration in seconds</param>
        /// <param name="tweenKey">Tween key</param>
        public void ShowCloudsAnimated(WeatherMakerCloudProfileScript newProfile, float transitionDelay, float transitionDuration, string tweenKey = null)
        {
            if (!isActiveAndEnabled || currentRenderCloudProfile == null || WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerLightManagerScript.Instance == null || WeatherMakerLightManagerScript.Instance.SunPerspective == null)
            {
                Debug.LogError("Full screen cloud script must be enabled to show clouds");
                return;
            }

            WeatherMakerCelestialObjectScript sun = WeatherMakerLightManagerScript.Instance.SunPerspective;
            tweenKey = (tweenKey ?? string.Empty);
            WeatherMakerCloudProfileScript oldProfile = currentRenderCloudProfile;
            // set to empty profile if null passed in
            if (newProfile == null)
            {
                newProfile = emptyProfile;
            }
            newProfile.EnsureNonNullLayers();
            newProfile.randomizer = randomizer;

            // dynamic start and end properties
            WeatherMakerCloudVolumetricProfileScript oldVol = oldProfile.CloudLayerVolumetric1;
            oldVol.CreateCurves();

            WeatherMakerCloudVolumetricProfileScript newVol = newProfile.CloudLayerVolumetric1;
            newVol.CreateCurves();

            Color oldVolColor = oldVol.CloudColor;
            float oldVolDitherMultiplier = oldVol.CloudDitherMultiplier.GetLastValue(randomizer);
            float oldVolHeight = oldProfile.CloudHeight.GetLastValue(randomizer);
            float oldVolHeightTop = oldProfile.CloudHeightTop.GetLastValue(randomizer);
            float oldVolPlanetRadius = oldProfile.CloudPlanetRadius;
            float oldVolRayOffset = oldVol.CloudRayOffset;
            float[] oldVolDetailCurve = oldVol.cloudNoiseDetailPowerCurve.Clone() as float[];

            float oldVolCoverageAnvilStrength = oldProfile.CloudLayerVolumetric1.CloudAnvilStrength.GetLastValue(randomizer);
            float oldVolCoverageAnvilStart = oldProfile.CloudLayerVolumetric1.CloudAnvilStart.GetLastValue(randomizer);
            float[] oldVolCoverCurve = oldVol.cloudCoverCurve.Clone() as float[];
            Vector3 oldVolCoverVelocity = oldProfile.WeatherMapCloudCoverageVelocity;
            float oldVolCover = oldVol.CloudCover.GetLastValue(randomizer);
            float oldVolCoverSecondary = oldVol.CloudCoverSecondary.GetLastValue(randomizer);
            float oldVolCoverageNoiseType = oldProfile.WeatherMapCloudCoverageNoiseType.GetLastValue(randomizer);
            float oldVolCoverageScale = oldProfile.WeatherMapCloudCoverageScale.GetLastValue(randomizer);
            float oldVolCoverageAdder = oldProfile.WeatherMapCloudCoverageAdder.GetLastValue(randomizer);
            //Vector3 oldVolCoverageOffset = oldProfile.cloudCoverageOffset;
            float oldVolCoveragePower = oldProfile.WeatherMapCloudCoveragePower.GetLastValue(randomizer);
            Vector4 oldVolCoverageWarp = oldProfile.WeatherMapCloudCoverageWarp;
            float oldVolCoverageRotation = oldProfile.WeatherMapCloudCoverageRotation.GetLastValue(randomizer);

            Vector3 oldVolCoverNegationVelocity = oldProfile.WeatherMapCloudCoverageNegationVelocity;
            float oldVolCoverageNegationScale = oldProfile.WeatherMapCloudCoverageNegationScale.GetLastValue(randomizer);
            float oldVolCoverageNegationAdder = oldProfile.WeatherMapCloudCoverageNegationAdder.GetLastValue(randomizer);
            //Vector3 oldVolCoverageNegationOffset = oldProfile.weatherMapCloudCoverageNegationOffsetCalculated;
            float oldVolCoverageNegationPower = oldProfile.WeatherMapCloudCoverageNegationPower.GetLastValue(randomizer);
            Vector4 oldVolCoverageNegationWarp = oldProfile.WeatherMapCloudCoverageNegationWarp;
            float oldVolCoverageNegationRotation = oldProfile.WeatherMapCloudCoverageNegationRotation.GetLastValue(randomizer);

            Vector3 oldVolDensityVelocity = oldProfile.WeatherMapCloudDensityVelocity;
            float oldVolDensity = oldVol.CloudDensity.GetLastValue(randomizer);
            float[] oldVolDensityCurve = oldVol.cloudDensityCurve.Clone() as float[];
            float oldVolDensitySecondary = oldVol.CloudDensitySecondary.GetLastValue(randomizer);
            float oldVolDensityNoiseType = oldProfile.WeatherMapCloudDensityNoiseType.GetLastValue(randomizer);
            float oldVolDensityScale = oldProfile.WeatherMapCloudDensityScale.GetLastValue(randomizer);
            float oldVolDensityAdder = oldProfile.WeatherMapCloudDensityAdder.GetLastValue(randomizer);
            //Vector3 oldVolDensityOffset = oldProfile.cloudDensityOffset;
            float oldVolDensityPower = oldProfile.WeatherMapCloudDensityPower.GetLastValue(randomizer);
            Vector4 oldVolDensityWarp = oldProfile.WeatherMapCloudDensityWarp;
            float oldVolDensityRotation = oldProfile.WeatherMapCloudDensityRotation.GetLastValue(randomizer);

            Vector3 oldVolDensityNegationVelocity = oldProfile.WeatherMapCloudDensityNegationVelocity;
            float oldVolDensityNegationScale = oldProfile.WeatherMapCloudDensityNegationScale.GetLastValue(randomizer);
            float oldVolDensityNegationAdder = oldProfile.WeatherMapCloudDensityNegationAdder.GetLastValue(randomizer);
            //Vector3 oldVolDensityNegationOffset = oldProfile.weatherMapCloudDensityNegationOffsetCalculated;
            float oldVolDensityNegationPower = oldProfile.WeatherMapCloudDensityNegationPower.GetLastValue(randomizer);
            Vector4 oldVolDensityNegationWarp = oldProfile.WeatherMapCloudDensityNegationWarp;
            float oldVolDensityNegationRotation = oldProfile.WeatherMapCloudDensityNegationRotation.GetLastValue(randomizer);

            Vector3 oldVolTypeVelocity = oldProfile.WeatherMapCloudTypeVelocity;
            float oldVolType = oldVol.CloudType.GetLastValue(randomizer);
            float[] oldVolTypeCurve = oldVol.cloudTypeCurve.Clone() as float[];
            float oldVolTypeSecondary = oldVol.CloudTypeSecondary.GetLastValue(randomizer);
            float oldVolTypeNoiseType = oldProfile.WeatherMapCloudTypeNoiseType.GetLastValue(randomizer);
            float oldVolTypeScale = oldProfile.WeatherMapCloudTypeScale.GetLastValue(randomizer);
            float oldVolTypeAdder = oldProfile.WeatherMapCloudTypeAdder.GetLastValue(randomizer);
            //Vector3 oldVolTypeOffset = oldProfile.cloudTypeOffset;
            float oldVolTypePower = oldProfile.WeatherMapCloudTypePower.GetLastValue(randomizer);
            Vector4 oldVolTypeWarp = oldProfile.WeatherMapCloudTypeWarp;
            float oldVolTypeRotation = oldProfile.WeatherMapCloudTypeRotation.GetLastValue(randomizer);

            Vector3 oldVolTypeNegationVelocity = oldProfile.WeatherMapCloudTypeNegationVelocity;
            float oldVolTypeNegationScale = oldProfile.WeatherMapCloudTypeNegationScale.GetLastValue(randomizer);
            float oldVolTypeNegationAdder = oldProfile.WeatherMapCloudTypeNegationAdder.GetLastValue(randomizer);
            //Vector3 oldVolTypeNegationOffset = oldProfile.weatherMapCloudTypeNegationOffsetCalculated;
            float oldVolTypeNegationPower = oldProfile.WeatherMapCloudTypeNegationPower.GetLastValue(randomizer);
            Vector4 oldVolTypeNegationWarp = oldProfile.WeatherMapCloudTypeNegationWarp;
            float oldVolTypeNegationRotation = oldProfile.WeatherMapCloudTypeNegationRotation.GetLastValue(randomizer);

            Vector4 oldVolStratusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(oldProfile.CloudLayerVolumetric1.CloudGradientStratus);
            Vector4 oldVolStratoCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(oldProfile.CloudLayerVolumetric1.CloudGradientStratoCumulus);
            Vector4 oldVolCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(oldProfile.CloudLayerVolumetric1.CloudGradientCumulus);
            Vector4 oldVolWeights = oldProfile.CloudLayerVolumetric1.CloudNoiseShapeWeights;

            Vector4 oldVolShapeAnimationVelocity = oldVol.CloudShapeAnimationVelocity;
            Vector4 oldVolDetailAnimationVelocity = oldVol.CloudDetailAnimationVelocity;
            Vector4 oldVolHPhase = oldVol.CloudHenyeyGreensteinPhase;
            float oldVolDirLightMultiplier = oldVol.CloudDirLightMultiplier;
            float oldVolPointSpotLightMultiplier = oldVol.CloudPointSpotLightMultiplier;
            float oldVolAmbientGroundIntensity = oldVol.CloudAmbientGroundIntensity;
            float oldVolAmbientSkyIntensity = oldVol.CloudAmbientSkyIntensity;
            float oldVolAmbientGroundHeightMultiplier = oldVol.CloudAmbientGroundHeightMultiplier;
            float oldVolAmbientSkyHeightMultiplier = oldVol.CloudAmbientSkyHeightMultiplier;
            float oldVolAmbientShadow = oldVol.CloudAmbientShadow;
            float oldVolLightAbsorption = oldVol.CloudLightAbsorption;
            float oldVolDirLightIndirectMultiplier = oldVol.CloudDirLightIndirectMultiplier;
            float oldVolPowderMultiplier = oldVol.CloudPowderMultiplier.GetLastValue(randomizer);
            float oldVolOpticalDistanceMultiplier = oldVol.CloudOpticalDistanceMultiplier;
            Vector4 oldVolHorizonFadeMultiplier = oldVol.CloudHorizonFade;

            float oldVolNoiseShapeScale = oldVol.CloudNoiseShapeScale.GetLastValue(randomizer);
            float oldVolNoiseDetailScale = oldVol.CloudNoiseDetailScale.GetLastValue(randomizer);
            float oldVolNoiseShapeScalar = oldVol.CloudNoiseShapeScalar.GetLastValue(randomizer);
            float oldVolNoiseDetailPower = oldVol.CloudNoiseDetailPower.GetLastValue(randomizer);
            float oldVolNoiseDetailHeightMultiplier = oldVol.CloudNoiseDetailHeightMultiplier.GetLastValue(randomizer);
            float oldVolNoiseDetailCurlScale = oldVol.CloudNoiseDetailCurlScale.GetLastValue(randomizer);
            float oldVolNoiseDetailCurlIntensity = oldVol.CloudNoiseDetailCurlIntensity.GetLastValue(randomizer);

            Color oldVolDirLightGradientColor = oldVol.CloudDirLightGradientColorColor;
            Color oldVolEmissionColor = oldVol.CloudEmissionGradientColorColor;
            float oldVolDirLightRayBrightness = oldVol.CloudDirLightRayBrightness;
            float oldVolDirLightRayDecay = oldVol.CloudDirLightRayDecay;
            float oldVolDirLightRaySpread = oldVol.CloudDirLightRaySpread;
            float oldVolDirLightRayStepMultiplier = oldVol.CloudDirLightRayStepMultiplier;
            Color oldVolDirLightRayTintColor = oldVol.CloudDirLightRayTintColor;
            Vector3 oldVolWeatherMapScale = new Vector3(oldProfile.NoiseScaleX.GetLastValue(randomizer), oldProfile.NoiseScaleY.GetLastValue(randomizer), oldProfile.WorldScale);
            float oldVolCloudLightDither = oldProfile.CloudLayerVolumetric1.CloudLightDitherLevel;
            Vector4 oldRayMarchParam = oldProfile.CloudLayerVolumetric1.CloudRayMarchParameters;

            Color newVolColor = newVol.CloudColor;
            float newVolDitherMultiplier = newVol.CloudDitherMultiplier.Random(randomizer);
            float newVolHeight = newProfile.CloudHeight.Random(randomizer);
            float newVolHeightTop = newProfile.CloudHeightTop.Random(randomizer);
            float newVolPlanetRadius = newProfile.CloudPlanetRadius;
            float newVolRayOffset = newVol.CloudRayOffset;
            float[] newVolDetailCurve = newVol.cloudNoiseDetailPowerCurve.Clone() as float[];

            float newVolCoverageAnvilStrength = newProfile.CloudLayerVolumetric1.CloudAnvilStrength.Random(randomizer);
            float newVolCoverageAnvilStart = newProfile.CloudLayerVolumetric1.CloudAnvilStart.Random(randomizer);

            Vector3 newVolCoverVelocity = RandomRange(newProfile.WeatherMapCloudCoverageVelocity);
            float newVolCover = newVol.CloudCover.Random(randomizer);
            float[] newVolCoverCurve = newVol.cloudCoverCurve.Clone() as float[];
            if (!WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds)
            {
                // turn off volumetric clouds if not allowed
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudCover.LastValue = oldVolCover = newVolCover = 0.0f;
            }
            float newVolCoverSecondary = newVol.CloudCoverSecondary.Random(randomizer);
            float newVolCoverageNoiseType = newProfile.WeatherMapCloudCoverageNoiseType.Random(randomizer);
            float newVolCoverageScale = newProfile.WeatherMapCloudCoverageScale.Random(randomizer);
            float newVolCoverageAdder = newProfile.WeatherMapCloudCoverageAdder.Random(randomizer);
            //Vector3 newVolCoverageOffset = Vector3.zero;
            float newVolCoveragePower = newProfile.WeatherMapCloudCoveragePower.Random(randomizer);
            Vector4 newVolCoverageWarp = newProfile.WeatherMapCloudCoverageWarp;
            float newVolCoverageRotation = newProfile.WeatherMapCloudCoverageRotation.Random(randomizer);

            Vector3 newVolCoverNegationVelocity = RandomRange(newProfile.WeatherMapCloudCoverageNegationVelocity);
            float newVolCoverageNegationScale = newProfile.WeatherMapCloudCoverageNegationScale.Random(randomizer);
            float newVolCoverageNegationAdder = newProfile.WeatherMapCloudCoverageNegationAdder.Random(randomizer);
            //Vector3 newVolCoverageNegationOffset = newProfile.weatherMapCloudCoverageNegationOffsetCalculated;
            float newVolCoverageNegationPower = newProfile.WeatherMapCloudCoverageNegationPower.Random(randomizer);
            Vector4 newVolCoverageNegationWarp = newProfile.WeatherMapCloudCoverageNegationWarp;
            float newVolCoverageNegationRotation = newProfile.WeatherMapCloudCoverageNegationRotation.Random(randomizer);

            Vector3 newVolDensityVelocity = RandomRange(newProfile.WeatherMapCloudDensityVelocity);
            float newVolDensity = newVol.CloudDensity.Random(randomizer);
            float[] newVolDensityCurve = newVol.cloudDensityCurve.Clone() as float[];
            float newVolDensitySecondary = newVol.CloudDensitySecondary.Random(randomizer);
            float newVolDensityNoiseType = newProfile.WeatherMapCloudDensityNoiseType.Random(randomizer);
            float newVolDensityScale = newProfile.WeatherMapCloudDensityScale.Random(randomizer);
            float newVolDensityAdder = newProfile.WeatherMapCloudDensityAdder.Random(randomizer);
            //Vector3 newVolDensityOffset = Vector3.zero;
            float newVolDensityPower = newProfile.WeatherMapCloudDensityPower.Random(randomizer);
            Vector4 newVolDensityWarp = newProfile.WeatherMapCloudDensityWarp;
            float newVolDensityRotation = newProfile.WeatherMapCloudDensityRotation.Random(randomizer);

            Vector3 newVolDensityNegationVelocity = RandomRange(newProfile.WeatherMapCloudDensityNegationVelocity);
            float newVolDensityNegationScale = newProfile.WeatherMapCloudDensityNegationScale.Random(randomizer);
            float newVolDensityNegationAdder = newProfile.WeatherMapCloudDensityNegationAdder.Random(randomizer);
            //Vector3 newVolDensityNegationOffset = newProfile.weatherMapCloudDensityNegationOffsetCalculated;
            float newVolDensityNegationPower = newProfile.WeatherMapCloudDensityNegationPower.Random(randomizer);
            Vector4 newVolDensityNegationWarp = newProfile.WeatherMapCloudDensityNegationWarp;
            float newVolDensityNegationRotation = newProfile.WeatherMapCloudDensityNegationRotation.Random(randomizer);

            Vector3 newVolTypeVelocity = RandomRange(newProfile.WeatherMapCloudTypeVelocity);
            float newVolType = newVol.CloudType.Random(randomizer);
            float[] newVolTypeCurve = newVol.cloudTypeCurve.Clone() as float[];
            float newVolTypeSecondary = newVol.CloudTypeSecondary.Random(randomizer);
            float newVolTypeNoiseType = newProfile.WeatherMapCloudTypeNoiseType.Random(randomizer);
            float newVolTypeScale = newProfile.WeatherMapCloudTypeScale.Random(randomizer);
            float newVolTypeAdder = newProfile.WeatherMapCloudTypeAdder.Random(randomizer);
            //Vector3 newVolTypeOffset = Vector3.zero;
            float newVolTypePower = newProfile.WeatherMapCloudTypePower.Random(randomizer);
            Vector4 newVolTypeWarp = newProfile.WeatherMapCloudTypeWarp;
            float newVolTypeRotation = newProfile.WeatherMapCloudTypeRotation.Random(randomizer);

            Vector3 newVolTypeNegationVelocity = RandomRange(newProfile.WeatherMapCloudTypeNegationVelocity);
            float newVolTypeNegationScale = newProfile.WeatherMapCloudTypeNegationScale.Random(randomizer);
            float newVolTypeNegationAdder = newProfile.WeatherMapCloudTypeNegationAdder.Random(randomizer);
            //Vector3 newVolTypeNegationOffset = newProfile.weatherMapCloudTypeNegationOffsetCalculated;
            float newVolTypeNegationPower = newProfile.WeatherMapCloudTypeNegationPower.Random(randomizer);
            Vector4 newVolTypeNegationWarp = newProfile.WeatherMapCloudTypeNegationWarp;
            float newVolTypeNegationRotation = newProfile.WeatherMapCloudTypeNegationRotation.Random(randomizer);

            Vector4 newVolStratusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(newProfile.CloudLayerVolumetric1.CloudGradientStratus);
            Vector4 newVolStratoCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(newProfile.CloudLayerVolumetric1.CloudGradientStratoCumulus);
            Vector4 newVolCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(newProfile.CloudLayerVolumetric1.CloudGradientCumulus);
            Vector4 newVolWeights = newProfile.CloudLayerVolumetric1.CloudNoiseShapeWeights;

            Vector4 newVolShapeAnimationVelocity = newProfile.CloudLayerVolumetric1.CloudShapeAnimationVelocity;
            Vector4 newVolDetailAnimationVelocity = newProfile.CloudLayerVolumetric1.CloudDetailAnimationVelocity;
            Vector4 newVolHPhase = newVol.CloudHenyeyGreensteinPhase;
            float newVolDirLightMultiplier = newVol.CloudDirLightMultiplier;
            float newVolPointSpotLightMultiplier = newVol.CloudPointSpotLightMultiplier;
            float newVolAmbientGroundIntensity = newVol.CloudAmbientGroundIntensity;
            float newVolAmbientSkyIntensity = newVol.CloudAmbientSkyIntensity;
            float newVolAmbientGroundHeightMultiplier = newVol.CloudAmbientGroundHeightMultiplier;
            float newVolAmbientSkyHeightMultiplier = newVol.CloudAmbientSkyHeightMultiplier;
            float newVolAmbientShadow = newVol.CloudAmbientShadow;
            float newVolLightAbsorption = newVol.CloudLightAbsorption;
            float newVolDirLightIndirectMultiplier = newVol.CloudDirLightIndirectMultiplier;
            float newVoPowderMultiplier = newVol.CloudPowderMultiplier.Random(randomizer);
            float newVolOpticalDistanceMultiplier = newVol.CloudOpticalDistanceMultiplier;
            Vector4 newVolHorizonFadeMultiplier = newVol.CloudHorizonFade;

            float newVolNoiseShapeScale = newVol.CloudNoiseShapeScale.Random(randomizer);
            float newVolNoiseShapeScalar = newVol.CloudNoiseShapeScalar.Random(randomizer);
            float newVolNoiseDetailScale = newVol.CloudNoiseDetailScale.Random(randomizer);
            float newVolNoiseDetailPower = newVol.CloudNoiseDetailPower.Random(randomizer);
            float newVolNoiseDetailHeightMultiplier = newVol.CloudNoiseDetailHeightMultiplier.Random(randomizer);
            float newVolNoiseDetailCurlScale = newVol.CloudNoiseDetailCurlScale.Random(randomizer);
            float newVolNoiseDetailCurlIntensity = newVol.CloudNoiseDetailCurlIntensity.Random(randomizer);

            Color newVolDirLightGradientColor = sun.GetGradientColor(newVol.CloudDirLightGradientColor);
            Color newVolEmissionColor = sun.GetGradientColor(newVol.CloudEmissionGradientColor);
            float newVolDirLightRayBrightness = newVol.CloudDirLightRayBrightness;
            float newVolDirLightRayDecay = newVol.CloudDirLightRayDecay;
            float newVolDirLightRaySpread = newVol.CloudDirLightRaySpread;
            float newVolDirLightRayStepMultiplier = newVol.CloudDirLightRayStepMultiplier;
            Color newVolDirLightRayTintColor = newVol.CloudDirLightRayTintColor;
            Vector3 newVolWeatherMapScale = new Vector3(newProfile.NoiseScaleX.GetLastValue(randomizer), newProfile.NoiseScaleY.GetLastValue(randomizer), newProfile.WorldScale);
            float newVolCloudLightDither = newProfile.CloudLayerVolumetric1.CloudLightDitherLevel;
            Vector4 newRayMarchParam = newProfile.CloudLayerVolumetric1.CloudRayMarchParameters;
            float startDirectionalLightIntensityMultiplier = oldProfile.DirectionalLightIntensityMultiplier;
            float startDirectionalLightScatterMultiplier = oldProfile.DirectionalLightScatterMultiplier;
            float startDirectionalLightAtmosphereShadow = oldProfile.DirectionalLightAtmosphericShadow;
            float startCloudSdfThreshold = oldVol.CloudSdfThreshold;
            float startCloudCoverageMinimum = oldVol.CloudCoverMinimum;

            float endDirectionalLightIntensityMultiplier = newProfile.DirectionalLightIntensityMultiplier;
            float endDirectionalLightScatterMultiplier = newProfile.DirectionalLightScatterMultiplier;
            float endDirectionalLightAtmosphereShadow = newProfile.DirectionalLightAtmosphericShadow;
            float endCloudSdfTheshold = newVol.CloudSdfThreshold;
            float endCloudCoverageMinimum = newVol.CloudCoverMinimum;

            // flat clouds - start transition values
            float startCover1 = oldProfile.CloudLayer1.CloudCover.GetLastValue(randomizer);
            float startCover2 = oldProfile.CloudLayer2.CloudCover.GetLastValue(randomizer);
            float startCover3 = oldProfile.CloudLayer3.CloudCover.GetLastValue(randomizer);
            float startCover4 = oldProfile.CloudLayer4.CloudCover.GetLastValue(randomizer);
            Color startColor1 = oldProfile.CloudLayer1.CloudColor;
            Color startColor2 = oldProfile.CloudLayer2.CloudColor;
            Color startColor3 = oldProfile.CloudLayer3.CloudColor;
            Color startColor4 = oldProfile.CloudLayer4.CloudColor;
            Gradient startGradientColor1 = oldProfile.CloudLayer1.CloudGradientColor;
            Gradient startGradientColor2 = oldProfile.CloudLayer2.CloudGradientColor;
            Gradient startGradientColor3 = oldProfile.CloudLayer3.CloudGradientColor;
            Gradient startGradientColor4 = oldProfile.CloudLayer4.CloudGradientColor;
            Color startEmissionColor1 = oldProfile.CloudLayer1.CloudEmissionColor;
            Color startEmissionColor2 = oldProfile.CloudLayer2.CloudEmissionColor;
            Color startEmissionColor3 = oldProfile.CloudLayer3.CloudEmissionColor;
            Color startEmissionColor4 = oldProfile.CloudLayer4.CloudEmissionColor;
            float startAmbientGroundMultiplier1 = oldProfile.CloudLayer1.CloudAmbientGroundMultiplier.GetLastValue(randomizer);
            float startAmbientGroundMultiplier2 = oldProfile.CloudLayer2.CloudAmbientGroundMultiplier.GetLastValue(randomizer);
            float startAmbientGroundMultiplier3 = oldProfile.CloudLayer3.CloudAmbientGroundMultiplier.GetLastValue(randomizer);
            float startAmbientGroundMultiplier4 = oldProfile.CloudLayer4.CloudAmbientGroundMultiplier.GetLastValue(randomizer);
            float startAmbientSkyMultiplier1 = oldProfile.CloudLayer1.CloudAmbientSkyMultiplier.GetLastValue(randomizer);
            float startAmbientSkyMultiplier2 = oldProfile.CloudLayer2.CloudAmbientSkyMultiplier.GetLastValue(randomizer);
            float startAmbientSkyMultiplier3 = oldProfile.CloudLayer3.CloudAmbientSkyMultiplier.GetLastValue(randomizer);
            float startAmbientSkyMultiplier4 = oldProfile.CloudLayer4.CloudAmbientSkyMultiplier.GetLastValue(randomizer);
            Vector4 startScatterMultiplier1 = oldProfile.CloudLayer1.CloudScatterMultiplier;
            Vector4 startScatterMultiplier2 = oldProfile.CloudLayer2.CloudScatterMultiplier;
            Vector4 startScatterMultiplier3 = oldProfile.CloudLayer3.CloudScatterMultiplier;
            Vector4 startScatterMultiplier4 = oldProfile.CloudLayer4.CloudScatterMultiplier;
            Vector4 startVelocity1 = oldProfile.CloudLayer1.CloudNoiseVelocity;
            Vector4 startVelocity2 = oldProfile.CloudLayer2.CloudNoiseVelocity;
            Vector4 startVelocity3 = oldProfile.CloudLayer3.CloudNoiseVelocity;
            Vector4 startVelocity4 = oldProfile.CloudLayer4.CloudNoiseVelocity;
            //Vector4 startMaskVelocity1 = oldProfile.CloudLayer1.CloudNoiseMaskVelocity;
            //Vector4 startMaskVelocity2 = oldProfile.CloudLayer2.CloudNoiseMaskVelocity;
            //Vector4 startMaskVelocity3 = oldProfile.CloudLayer3.CloudNoiseMaskVelocity;
            //Vector4 startMaskVelocity4 = oldProfile.CloudLayer4.CloudNoiseMaskVelocity;
            Vector4 startRotation = new Vector4(oldProfile.CloudLayer1.CloudNoiseRotation.GetLastValue(randomizer), oldProfile.CloudLayer2.CloudNoiseRotation.GetLastValue(randomizer), oldProfile.CloudLayer3.CloudNoiseRotation.GetLastValue(randomizer), oldProfile.CloudLayer4.CloudNoiseRotation.GetLastValue(randomizer));
            //Vector4 startMaskRotation = new Vector4(oldProfile.CloudLayer1.CloudNoiseMaskRotation.GetLastValue(randomizer), oldProfile.CloudLayer2.CloudNoiseMaskRotation.GetLastValue(randomizer), oldProfile.CloudLayer3.CloudNoiseMaskRotation.GetLastValue(randomizer), oldProfile.CloudLayer4.CloudNoiseMaskRotation.GetLastValue(randomizer));
            float startScale1 = oldProfile.CloudLayer1.CloudNoiseScale.GetLastValue(randomizer);
            float startScale2 = oldProfile.CloudLayer2.CloudNoiseScale.GetLastValue(randomizer);
            float startScale3 = oldProfile.CloudLayer3.CloudNoiseScale.GetLastValue(randomizer);
            float startScale4 = oldProfile.CloudLayer4.CloudNoiseScale.GetLastValue(randomizer);
            float startAdder1 = oldProfile.CloudLayer1.CloudNoiseAdder.GetLastValue(randomizer);
            float startAdder2 = oldProfile.CloudLayer2.CloudNoiseAdder.GetLastValue(randomizer);
            float startAdder3 = oldProfile.CloudLayer3.CloudNoiseAdder.GetLastValue(randomizer);
            float startAdder4 = oldProfile.CloudLayer4.CloudNoiseAdder.GetLastValue(randomizer);
            float startMultiplier1 = oldProfile.CloudLayer1.CloudNoiseMultiplier.GetLastValue(randomizer);
            float startMultiplier2 = oldProfile.CloudLayer2.CloudNoiseMultiplier.GetLastValue(randomizer);
            float startMultiplier3 = oldProfile.CloudLayer3.CloudNoiseMultiplier.GetLastValue(randomizer);
            float startMultiplier4 = oldProfile.CloudLayer4.CloudNoiseMultiplier.GetLastValue(randomizer);
            float startDither1 = oldProfile.CloudLayer1.CloudNoiseDither.GetLastValue(randomizer);
            float startDither2 = oldProfile.CloudLayer2.CloudNoiseDither.GetLastValue(randomizer);
            float startDither3 = oldProfile.CloudLayer3.CloudNoiseDither.GetLastValue(randomizer);
            float startDither4 = oldProfile.CloudLayer4.CloudNoiseDither.GetLastValue(randomizer);
            //float startMaskScale1 = oldProfile.CloudLayer1.CloudNoiseMaskScale;
            //float startMaskScale2 = oldProfile.CloudLayer2.CloudNoiseMaskScale;
            //float startMaskScale3 = oldProfile.CloudLayer3.CloudNoiseMaskScale;
            //float startMaskScale4 = oldProfile.CloudLayer4.CloudNoiseMaskScale;
            float startLightAbsorption1 = oldProfile.CloudLayer1.CloudLightAbsorption.GetLastValue(randomizer);
            float startLightAbsorption2 = oldProfile.CloudLayer2.CloudLightAbsorption.GetLastValue(randomizer);
            float startLightAbsorption3 = oldProfile.CloudLayer3.CloudLightAbsorption.GetLastValue(randomizer);
            float startLightAbsorption4 = oldProfile.CloudLayer4.CloudLightAbsorption.GetLastValue(randomizer);
            float startHeight1 = oldProfile.CloudLayer1.CloudHeight.GetLastValue(randomizer);
            float startHeight2 = oldProfile.CloudLayer2.CloudHeight.GetLastValue(randomizer);
            float startHeight3 = oldProfile.CloudLayer3.CloudHeight.GetLastValue(randomizer);
            float startHeight4 = oldProfile.CloudLayer4.CloudHeight.GetLastValue(randomizer);
            float startRayOffset1 = oldProfile.CloudLayer1.CloudRayOffset;
            float startRayOffset2 = oldProfile.CloudLayer2.CloudRayOffset;
            float startRayOffset3 = oldProfile.CloudLayer3.CloudRayOffset;
            float startRayOffset4 = oldProfile.CloudLayer4.CloudRayOffset;

            // flat clouds - end transition values
            bool hasVolumetric = newVolCover > 0.0f; // if vol clouds are on, randomize flat coverage more if max coverage less than 1.0
            float endCover1Multiplier = (hasVolumetric && newProfile.CloudLayer1.CloudCover.Maximum < 1.0f ? Mathf.Pow(randomizer.Random(), 0.5f) : 1.0f);
            float endCover1 = newProfile.CloudLayer1.CloudCover.Random(randomizer) * endCover1Multiplier;
            float endCover2Multiplier = (hasVolumetric && newProfile.CloudLayer2.CloudCover.Maximum < 1.0f ? Mathf.Pow(randomizer.Random(), 0.5f) : 1.0f);
            float endCover2 = newProfile.CloudLayer2.CloudCover.Random(randomizer) * endCover2Multiplier;
            float endCover3Multiplier = (hasVolumetric && newProfile.CloudLayer3.CloudCover.Maximum < 1.0f ? Mathf.Pow(randomizer.Random(), 0.5f) : 1.0f);
            float endCover3 = newProfile.CloudLayer3.CloudCover.Random(randomizer) * endCover3Multiplier;
            float endCover4Multiplier = (hasVolumetric && newProfile.CloudLayer4.CloudCover.Maximum < 1.0f ? Mathf.Pow(randomizer.Random(), 0.5f) : 1.0f);
            float endCover4 = newProfile.CloudLayer4.CloudCover.Random(randomizer) * endCover4Multiplier;
            Color endColor1 = newProfile.CloudLayer1.CloudColor;
            Color endColor2 = newProfile.CloudLayer2.CloudColor;
            Color endColor3 = newProfile.CloudLayer3.CloudColor;
            Color endColor4 = newProfile.CloudLayer4.CloudColor;
            Gradient endGradientColor1 = newProfile.CloudLayer1.CloudGradientColor;
            Gradient endGradientColor2 = newProfile.CloudLayer2.CloudGradientColor;
            Gradient endGradientColor3 = newProfile.CloudLayer3.CloudGradientColor;
            Gradient endGradientColor4 = newProfile.CloudLayer4.CloudGradientColor;
            Color endEmissionColor1 = newProfile.CloudLayer1.CloudEmissionColor;
            Color endEmissionColor2 = newProfile.CloudLayer2.CloudEmissionColor;
            Color endEmissionColor3 = newProfile.CloudLayer3.CloudEmissionColor;
            Color endEmissionColor4 = newProfile.CloudLayer4.CloudEmissionColor;
            float endAmbientGroundMultiplier1 = newProfile.CloudLayer1.CloudAmbientGroundMultiplier.Random(randomizer);
            float endAmbientGroundMultiplier2 = newProfile.CloudLayer2.CloudAmbientGroundMultiplier.Random(randomizer);
            float endAmbientGroundMultiplier3 = newProfile.CloudLayer3.CloudAmbientGroundMultiplier.Random(randomizer);
            float endAmbientGroundMultiplier4 = newProfile.CloudLayer4.CloudAmbientGroundMultiplier.Random(randomizer);
            float endAmbientSkyMultiplier1 = newProfile.CloudLayer1.CloudAmbientSkyMultiplier.Random(randomizer);
            float endAmbientSkyMultiplier2 = newProfile.CloudLayer2.CloudAmbientSkyMultiplier.Random(randomizer);
            float endAmbientSkyMultiplier3 = newProfile.CloudLayer3.CloudAmbientSkyMultiplier.Random(randomizer);
            float endAmbientSkyMultiplier4 = newProfile.CloudLayer4.CloudAmbientSkyMultiplier.Random(randomizer);
            Vector4 endScatterMultiplier1 = newProfile.CloudLayer1.CloudScatterMultiplier;
            Vector4 endScatterMultiplier2 = newProfile.CloudLayer2.CloudScatterMultiplier;
            Vector4 endScatterMultiplier3 = newProfile.CloudLayer3.CloudScatterMultiplier;
            Vector4 endScatterMultiplier4 = newProfile.CloudLayer4.CloudScatterMultiplier;
            Vector3 endVelocity1 = RandomRange(newProfile.CloudLayer1.CloudNoiseVelocity);
            Vector3 endVelocity2 = RandomRange(newProfile.CloudLayer2.CloudNoiseVelocity);
            Vector3 endVelocity3 = RandomRange(newProfile.CloudLayer3.CloudNoiseVelocity);
            Vector3 endVelocity4 = RandomRange(newProfile.CloudLayer4.CloudNoiseVelocity);
            //Vector3 endMaskVelocity1 = newProfile.CloudLayer1.CloudNoiseMaskVelocity;
            //Vector3 endMaskVelocity2 = newProfile.CloudLayer2.CloudNoiseMaskVelocity;
            //Vector3 endMaskVelocity3 = newProfile.CloudLayer3.CloudNoiseMaskVelocity;
            //Vector3 endMaskVelocity4 = newProfile.CloudLayer4.CloudNoiseMaskVelocity;
            Vector4 endRotation = new Vector4(newProfile.CloudLayer1.CloudNoiseRotation.Random(randomizer), newProfile.CloudLayer2.CloudNoiseRotation.Random(randomizer), newProfile.CloudLayer3.CloudNoiseRotation.Random(randomizer), newProfile.CloudLayer4.CloudNoiseRotation.Random(randomizer));
            //Vector4 endMaskRotation = new Vector4(newProfile.CloudLayer1.CloudNoiseMaskRotation.Random(randomizer), newProfile.CloudLayer2.CloudNoiseMaskRotation.Random(randomizer), newProfile.CloudLayer3.CloudNoiseMaskRotation.Random(randomizer), newProfile.CloudLayer4.CloudNoiseMaskRotation.Random(randomizer));
            float endScale1 = newProfile.CloudLayer1.CloudNoiseScale.Random(randomizer);
            float endScale2 = newProfile.CloudLayer2.CloudNoiseScale.Random(randomizer);
            float endScale3 = newProfile.CloudLayer3.CloudNoiseScale.Random(randomizer);
            float endScale4 = newProfile.CloudLayer4.CloudNoiseScale.Random(randomizer);
            float endAdder1 = newProfile.CloudLayer1.CloudNoiseAdder.Random(randomizer);
            float endAdder2 = newProfile.CloudLayer2.CloudNoiseAdder.Random(randomizer);
            float endAdder3 = newProfile.CloudLayer3.CloudNoiseAdder.Random(randomizer);
            float endAdder4 = newProfile.CloudLayer4.CloudNoiseAdder.Random(randomizer);
            float endMultiplier1 = newProfile.CloudLayer1.CloudNoiseMultiplier.Random(randomizer);
            float endMultiplier2 = newProfile.CloudLayer2.CloudNoiseMultiplier.Random(randomizer);
            float endMultiplier3 = newProfile.CloudLayer3.CloudNoiseMultiplier.Random(randomizer);
            float endMultiplier4 = newProfile.CloudLayer4.CloudNoiseMultiplier.Random(randomizer);
            float endDither1 = newProfile.CloudLayer1.CloudNoiseDither.Random(randomizer);
            float endDither2 = newProfile.CloudLayer2.CloudNoiseDither.Random(randomizer);
            float endDither3 = newProfile.CloudLayer3.CloudNoiseDither.Random(randomizer);
            float endDither4 = newProfile.CloudLayer4.CloudNoiseDither.Random(randomizer);
            //float endMaskScale1 = newProfile.CloudLayer1.CloudNoiseMaskScale;
            //float endMaskScale2 = newProfile.CloudLayer2.CloudNoiseMaskScale;
            //float endMaskScale3 = newProfile.CloudLayer3.CloudNoiseMaskScale;
            //float endMaskScale4 = newProfile.CloudLayer4.CloudNoiseMaskScale;
            float endLightAbsorption1 = newProfile.CloudLayer1.CloudLightAbsorption.Random(randomizer);
            float endLightAbsorption2 = newProfile.CloudLayer2.CloudLightAbsorption.Random(randomizer);
            float endLightAbsorption3 = newProfile.CloudLayer3.CloudLightAbsorption.Random(randomizer);
            float endLightAbsorption4 = newProfile.CloudLayer4.CloudLightAbsorption.Random(randomizer);
            float endHeight1 = newProfile.CloudLayer1.CloudHeight.Random(randomizer);
            float endHeight2 = newProfile.CloudLayer2.CloudHeight.Random(randomizer);
            float endHeight3 = newProfile.CloudLayer3.CloudHeight.Random(randomizer);
            float endHeight4 = newProfile.CloudLayer4.CloudHeight.Random(randomizer);
            float endRayOffset1 = newProfile.CloudLayer1.CloudRayOffset;
            float endRayOffset2 = newProfile.CloudLayer2.CloudRayOffset;
            float endRayOffset3 = newProfile.CloudLayer3.CloudRayOffset;
            float endRayOffset4 = newProfile.CloudLayer4.CloudRayOffset;

            // use new profile for transition
            _CloudProfile = newProfile;
            DeleteAndTransitionRenderProfile(newProfile);

            // create temp object for animation, we don't want to modify variables in the actual assets during animation
            if (Application.isPlaying && _CloudProfile.name.IndexOf("(clone)", StringComparison.OrdinalIgnoreCase) < 0)
            {
                _CloudProfile = _CloudProfile.Clone();
            }
            lastProfile = currentRenderCloudProfile = _CloudProfile;
            lastProfile.CloudLayerVolumetric1.CreateCurves();

            if (newVolCover > 0.0f)
            {
                var layerMask = currentRenderCloudProfile.CloudLayerVolumetric1.FlatLayerMask & WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudFlatLayerMask;
                if ((layerMask & WeatherMakerVolumetricCloudsFlatLayerMask.One) != WeatherMakerVolumetricCloudsFlatLayerMask.One)
                {
                    endCover1 = currentRenderCloudProfile.CloudLayer1.CloudCover.LastValue = 0.0f;
                }
                if ((layerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Two) != WeatherMakerVolumetricCloudsFlatLayerMask.Two)
                {
                    endCover2 = currentRenderCloudProfile.CloudLayer2.CloudCover.LastValue = 0.0f;
                }
                if ((layerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Three) != WeatherMakerVolumetricCloudsFlatLayerMask.Three)
                {
                    endCover3 = currentRenderCloudProfile.CloudLayer3.CloudCover.LastValue = 0.0f;
                }
                if ((layerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Four) != WeatherMakerVolumetricCloudsFlatLayerMask.Four)
                {
                    endCover4 = currentRenderCloudProfile.CloudLayer4.CloudCover.LastValue = 0.0f;
                }
            }

            currentRenderCloudProfile.AtmosphereProfile = (currentRenderCloudProfile.AtmosphereProfile ?? oldProfile.AtmosphereProfile); // copy aurora profile straight over if new clouds have no aurora profile
            currentRenderCloudProfile.AuroraProfile = (currentRenderCloudProfile.AuroraProfile ?? oldProfile.AuroraProfile); // copy aurora profile straight over if new clouds have no aurora profile
            currentRenderCloudProfile.isAnimating = true;

            WeatherMakerTextureLerpScript shapeLerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayerVolumetric1.CloudNoiseShape,
                newProfile.CloudLayerVolumetric1.CloudNoiseShape);
            currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseShape = shapeLerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript detailLerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayerVolumetric1.CloudNoiseDetail,
                newProfile.CloudLayerVolumetric1.CloudNoiseDetail);
            currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetail = detailLerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript curlLerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayerVolumetric1.CloudNoiseCurl,
                newProfile.CloudLayerVolumetric1.CloudNoiseCurl);
            currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseCurl = curlLerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript noise1Lerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayer1.CloudNoise,
                newProfile.CloudLayer1.CloudNoise);
            currentRenderCloudProfile.CloudLayer1.CloudNoise = noise1Lerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript noise2Lerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayer2.CloudNoise,
                newProfile.CloudLayer2.CloudNoise);
            currentRenderCloudProfile.CloudLayer2.CloudNoise = noise2Lerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript noise3Lerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayer3.CloudNoise,
                newProfile.CloudLayer3.CloudNoise);
            currentRenderCloudProfile.CloudLayer3.CloudNoise = noise3Lerp.UpdateProgress(0.0f);
            WeatherMakerTextureLerpScript noise4Lerp = new WeatherMakerTextureLerpScript(oldProfile.CloudLayer4.CloudNoise,
                newProfile.CloudLayer4.CloudNoise);
            currentRenderCloudProfile.CloudLayer4.CloudNoise = noise4Lerp.UpdateProgress(0.0f);

            // animate animatable properties
            FloatTween tween = TweenFactory.Tween("WeatherMakerClouds_" + GetInstanceID() + tweenKey, 0.0f, 1.0f, transitionDuration, TweenScaleFunctions.QuadraticEaseInOut, (ITween<float> c) =>
            {
                float progress = c.CurrentValue;
                currentRenderCloudProfile.CloudLayer1.CloudNoiseScale.LastValue = Mathf.Lerp(startScale1, endScale1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseScale.LastValue = Mathf.Lerp(startScale2, endScale2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseScale.LastValue = Mathf.Lerp(startScale3, endScale3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseScale.LastValue = Mathf.Lerp(startScale4, endScale4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseAdder.LastValue = Mathf.Lerp(startAdder1, endAdder1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseAdder.LastValue = Mathf.Lerp(startAdder2, endAdder2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseAdder.LastValue = Mathf.Lerp(startAdder3, endAdder3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseAdder.LastValue = Mathf.Lerp(startAdder4, endAdder4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseMultiplier.LastValue = Mathf.Lerp(startMultiplier1, endMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseMultiplier.LastValue = Mathf.Lerp(startMultiplier2, endMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseMultiplier.LastValue = Mathf.Lerp(startMultiplier3, endMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseMultiplier.LastValue = Mathf.Lerp(startMultiplier4, endMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseDither.LastValue = Mathf.Lerp(startDither1, endDither1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseDither.LastValue = Mathf.Lerp(startDither2, endDither2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseDither.LastValue = Mathf.Lerp(startDither3, endDither3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseDither.LastValue = Mathf.Lerp(startDither4, endDither4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.x, endRotation.x, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.y, endRotation.y, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.z, endRotation.z, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseRotation.LastValue = Mathf.Lerp(startRotation.w, endRotation.w, progress);
                currentRenderCloudProfile.CloudLayer1.CloudNoiseVelocity = Vector3.Lerp(startVelocity1, endVelocity1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudNoiseVelocity = Vector3.Lerp(startVelocity2, endVelocity2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudNoiseVelocity = Vector3.Lerp(startVelocity3, endVelocity3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudNoiseVelocity = Vector3.Lerp(startVelocity4, endVelocity4, progress);
                //currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale1, endMaskScale1, progress);
                //currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale2, endMaskScale2, progress);
                //currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale3, endMaskScale3, progress);
                //currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskScale = Mathf.Lerp(startMaskScale4, endMaskScale4, progress);
                //currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.x, endMaskRotation.x, progress);
                //currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.y, endMaskRotation.y, progress);
                //currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.z, endMaskRotation.z, progress);
                //currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskRotation.LastValue = Mathf.Lerp(startMaskRotation.w, endMaskRotation.w, progress);
                //currentRenderCloudProfile.CloudLayer1.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity1, endMaskVelocity1, progress);
                //currentRenderCloudProfile.CloudLayer2.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity2, endMaskVelocity2, progress);
                //currentRenderCloudProfile.CloudLayer3.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity3, endMaskVelocity3, progress);
                //currentRenderCloudProfile.CloudLayer4.CloudNoiseMaskVelocity = Vector3.Lerp(startMaskVelocity4, endMaskVelocity4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudCover.LastValue = Mathf.Lerp(startCover1, endCover1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudCover.LastValue = Mathf.Lerp(startCover2, endCover2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudCover.LastValue = Mathf.Lerp(startCover3, endCover3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudCover.LastValue = Mathf.Lerp(startCover4, endCover4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudLightAbsorption.LastValue = Mathf.Lerp(startLightAbsorption1, endLightAbsorption1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudLightAbsorption.LastValue = Mathf.Lerp(startLightAbsorption2, endLightAbsorption2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudLightAbsorption.LastValue = Mathf.Lerp(startLightAbsorption3, endLightAbsorption3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudLightAbsorption.LastValue = Mathf.Lerp(startLightAbsorption4, endLightAbsorption4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudColor = Color.Lerp(startColor1, endColor1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudColor = Color.Lerp(startColor2, endColor2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudColor = Color.Lerp(startColor3, endColor3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudColor = Color.Lerp(startColor4, endColor4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudGradientColor = startGradientColor1.Lerp(endGradientColor1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudGradientColor = startGradientColor2.Lerp(endGradientColor2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudGradientColor = startGradientColor3.Lerp(endGradientColor3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudGradientColor = startGradientColor4.Lerp(endGradientColor4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudEmissionColor = Color.Lerp(startEmissionColor1, endEmissionColor1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudEmissionColor = Color.Lerp(startEmissionColor2, endEmissionColor2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudEmissionColor = Color.Lerp(startEmissionColor3, endEmissionColor3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudEmissionColor = Color.Lerp(startEmissionColor4, endEmissionColor4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudAmbientGroundMultiplier.LastValue = Mathf.Lerp(startAmbientGroundMultiplier1, endAmbientGroundMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudAmbientGroundMultiplier.LastValue = Mathf.Lerp(startAmbientGroundMultiplier2, endAmbientGroundMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudAmbientGroundMultiplier.LastValue = Mathf.Lerp(startAmbientGroundMultiplier3, endAmbientGroundMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudAmbientGroundMultiplier.LastValue = Mathf.Lerp(startAmbientGroundMultiplier4, endAmbientGroundMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudAmbientSkyMultiplier.LastValue = Mathf.Lerp(startAmbientSkyMultiplier1, endAmbientSkyMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudAmbientSkyMultiplier.LastValue = Mathf.Lerp(startAmbientSkyMultiplier2, endAmbientSkyMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudAmbientSkyMultiplier.LastValue = Mathf.Lerp(startAmbientSkyMultiplier3, endAmbientSkyMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudAmbientSkyMultiplier.LastValue = Mathf.Lerp(startAmbientSkyMultiplier4, endAmbientSkyMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudScatterMultiplier = Vector4.Lerp(startScatterMultiplier1, endScatterMultiplier1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudScatterMultiplier = Vector4.Lerp(startScatterMultiplier2, endScatterMultiplier2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudScatterMultiplier = Vector4.Lerp(startScatterMultiplier3, endScatterMultiplier3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudScatterMultiplier = Vector4.Lerp(startScatterMultiplier4, endScatterMultiplier4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudHeight.LastValue = Mathf.Lerp(startHeight1, endHeight1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudHeight.LastValue = Mathf.Lerp(startHeight2, endHeight2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudHeight.LastValue = Mathf.Lerp(startHeight3, endHeight3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudHeight.LastValue = Mathf.Lerp(startHeight4, endHeight4, progress);
                currentRenderCloudProfile.CloudLayer1.CloudRayOffset = Mathf.Lerp(startRayOffset1, endRayOffset1, progress);
                currentRenderCloudProfile.CloudLayer2.CloudRayOffset = Mathf.Lerp(startRayOffset2, endRayOffset2, progress);
                currentRenderCloudProfile.CloudLayer3.CloudRayOffset = Mathf.Lerp(startRayOffset3, endRayOffset3, progress);
                currentRenderCloudProfile.CloudLayer4.CloudRayOffset = Mathf.Lerp(startRayOffset4, endRayOffset4, progress);

                currentRenderCloudProfile.WeatherMapCloudCoverageVelocity = Vector4.Lerp(oldVolCoverVelocity, newVolCoverVelocity, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageNegationVelocity = Vector4.Lerp(oldVolCoverNegationVelocity, newVolCoverNegationVelocity, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityVelocity = Vector4.Lerp(oldVolDensityVelocity, newVolDensityVelocity, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityNegationVelocity = Vector4.Lerp(oldVolDensityNegationVelocity, newVolDensityNegationVelocity, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeVelocity = Vector4.Lerp(oldVolTypeVelocity, newVolTypeVelocity, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeNegationVelocity = Vector4.Lerp(oldVolTypeNegationVelocity, newVolTypeNegationVelocity, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudColor = Color.Lerp(oldVolColor, newVolColor, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudCover.LastValue = Mathf.Lerp(oldVolCover, newVolCover, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudCoverSecondary.LastValue = Mathf.Lerp(oldVolCoverSecondary, newVolCoverSecondary, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDitherMultiplier.LastValue = Mathf.Lerp(oldVolDitherMultiplier, newVolDitherMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudEmissionGradientColorColor = Color.Lerp(oldVolEmissionColor, newVolEmissionColor, progress);
                currentRenderCloudProfile.CloudHeight.LastValue = Mathf.Lerp(oldVolHeight, newVolHeight, progress);
                currentRenderCloudProfile.CloudHeightTop.LastValue = Mathf.Lerp(oldVolHeightTop, newVolHeightTop, progress);
                currentRenderCloudProfile.CloudPlanetRadius = Mathf.Lerp(oldVolPlanetRadius, newVolPlanetRadius, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudHenyeyGreensteinPhase = Vector4.Lerp(oldVolHPhase, newVolHPhase, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightMultiplier = Mathf.Lerp(oldVolDirLightMultiplier, newVolDirLightMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudLightDitherLevel = Mathf.Lerp(oldVolCloudLightDither, newVolCloudLightDither, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudPointSpotLightMultiplier = Mathf.Lerp(oldVolPointSpotLightMultiplier, newVolPointSpotLightMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAmbientGroundIntensity = Mathf.Lerp(oldVolAmbientGroundIntensity, newVolAmbientGroundIntensity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAmbientSkyIntensity = Mathf.Lerp(oldVolAmbientSkyIntensity, newVolAmbientSkyIntensity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAmbientGroundHeightMultiplier = Mathf.Lerp(oldVolAmbientGroundHeightMultiplier, newVolAmbientGroundHeightMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAmbientSkyHeightMultiplier = Mathf.Lerp(oldVolAmbientSkyHeightMultiplier, newVolAmbientSkyHeightMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAmbientShadow = Mathf.Lerp(oldVolAmbientShadow, newVolAmbientShadow, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudLightAbsorption = Mathf.Lerp(oldVolLightAbsorption, newVolLightAbsorption, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightIndirectMultiplier = Mathf.Lerp(oldVolDirLightIndirectMultiplier, newVolDirLightIndirectMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudPowderMultiplier.LastValue = Mathf.Lerp(oldVolPowderMultiplier, newVoPowderMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudOpticalDistanceMultiplier = Mathf.Lerp(oldVolOpticalDistanceMultiplier, newVolOpticalDistanceMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudHorizonFade = Vector4.Lerp(oldVolHorizonFadeMultiplier, newVolHorizonFadeMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudShapeAnimationVelocity = Vector4.Lerp(oldVolShapeAnimationVelocity, newVolShapeAnimationVelocity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDetailAnimationVelocity = Vector4.Lerp(oldVolDetailAnimationVelocity, newVolDetailAnimationVelocity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseShapeScale.LastValue = Mathf.Lerp(oldVolNoiseShapeScale, newVolNoiseShapeScale, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightGradientColorColor = Color.Lerp(oldVolDirLightGradientColor, newVolDirLightGradientColor, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseShapeScalar.LastValue = Mathf.Lerp(oldVolNoiseShapeScalar, newVolNoiseShapeScalar, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDensity.LastValue = Mathf.Lerp(oldVolDensity, newVolDensity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDensitySecondary.LastValue = Mathf.Lerp(oldVolDensitySecondary, newVolDensitySecondary, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetailScale.LastValue = Mathf.Lerp(oldVolNoiseDetailScale, newVolNoiseDetailScale, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetailCurlScale.LastValue = Mathf.Lerp(oldVolNoiseDetailCurlScale, newVolNoiseDetailCurlScale, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetailCurlIntensity.LastValue = Mathf.Lerp(oldVolNoiseDetailCurlIntensity, newVolNoiseDetailCurlIntensity, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetailPower.LastValue = Mathf.Lerp(oldVolNoiseDetailPower, newVolNoiseDetailPower, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetailHeightMultiplier.LastValue = Mathf.Lerp(oldVolNoiseDetailHeightMultiplier, newVolNoiseDetailHeightMultiplier, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudRayOffset = Mathf.Lerp(oldVolRayOffset, newVolRayOffset, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudType.LastValue = Mathf.Lerp(oldVolType, newVolType, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudTypeSecondary.LastValue = Mathf.Lerp(oldVolTypeSecondary, newVolTypeSecondary, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightRayBrightness = Mathf.Lerp(oldVolDirLightRayBrightness, newVolDirLightRayBrightness, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightRayDecay = Mathf.Lerp(oldVolDirLightRayDecay, newVolDirLightRayDecay, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightRaySpread = Mathf.Lerp(oldVolDirLightRaySpread, newVolDirLightRaySpread, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightRayStepMultiplier = Mathf.Lerp(oldVolDirLightRayStepMultiplier, newVolDirLightRayStepMultiplier, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudDirLightRayTintColor = Color.Lerp(oldVolDirLightRayTintColor, newVolDirLightRayTintColor, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAnvilStrength.LastValue = Mathf.Lerp(oldVolCoverageAnvilStrength, newVolCoverageAnvilStrength, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudAnvilStart.LastValue = Mathf.Lerp(oldVolCoverageAnvilStart, newVolCoverageAnvilStart, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudGradientStratusVector = Vector4.Lerp(oldVolStratusVector, newVolStratusVector, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudGradientStratoCumulusVector = Vector4.Lerp(oldVolStratoCumulusVector, newVolStratoCumulusVector, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudGradientCumulusVector = Vector4.Lerp(oldVolCumulusVector, newVolCumulusVector, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseShapeWeights = Vector4.Lerp(oldVolWeights, newVolWeights, progress);

                currentRenderCloudProfile.CloudLayerVolumetric1.CloudRayMarchParameters = Vector4.Lerp(oldRayMarchParam, newRayMarchParam, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseShape = shapeLerp.UpdateProgress(progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseDetail = detailLerp.UpdateProgress(progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudNoiseCurl = curlLerp.UpdateProgress(progress);

                currentRenderCloudProfile.WeatherMapCloudCoverageNoiseType.LastValue = Mathf.Lerp(oldVolCoverageNoiseType, newVolCoverageNoiseType, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageScale.LastValue = Mathf.Lerp(oldVolCoverageScale, newVolCoverageScale, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageAdder.LastValue = Mathf.Lerp(oldVolCoverageAdder, newVolCoverageAdder, progress);
                //currentRenderCloudProfile.cloudCoverageOffset = Vector3.Lerp(oldVolCoverageOffset, newVolCoverageOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudCoveragePower.LastValue = Mathf.Lerp(oldVolCoveragePower, newVolCoveragePower, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageRotation.LastValue = Mathf.Lerp(oldVolCoverageRotation, newVolCoverageRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageWarp = Vector4.Lerp(oldVolCoverageWarp, newVolCoverageWarp, progress);

                currentRenderCloudProfile.WeatherMapCloudCoverageNegationAdder.LastValue = Mathf.Lerp(oldVolCoverageNegationAdder, newVolCoverageNegationAdder, progress);
                //currentRenderCloudProfile.weatherMapCloudCoverageNegationOffsetCalculated = Vector2.Lerp(oldVolCoverageNegationOffset, newVolCoverageNegationOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageNegationPower.LastValue = Mathf.Lerp(oldVolCoverageNegationPower, newVolCoverageNegationPower, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageNegationRotation.LastValue = Mathf.Lerp(oldVolCoverageNegationRotation, newVolCoverageNegationRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageNegationScale.LastValue = Mathf.Lerp(oldVolCoverageNegationScale, newVolCoverageNegationScale, progress);
                currentRenderCloudProfile.WeatherMapCloudCoverageNegationWarp = Vector4.Lerp(oldVolCoverageNegationWarp, newVolCoverageNegationWarp, progress);

                currentRenderCloudProfile.WeatherMapCloudDensityNoiseType.LastValue = Mathf.Lerp(oldVolDensityNoiseType, newVolDensityNoiseType, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityScale.LastValue = Mathf.Lerp(oldVolDensityScale, newVolDensityScale, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityAdder.LastValue = Mathf.Lerp(oldVolDensityAdder, newVolDensityAdder, progress);
                //currentRenderCloudProfile.cloudDensityOffset = Vector3.Lerp(oldVolDensityOffset, newVolDensityOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityPower.LastValue = Mathf.Lerp(oldVolDensityPower, newVolDensityPower, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityRotation.LastValue = Mathf.Lerp(oldVolDensityRotation, newVolDensityRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityWarp = Vector4.Lerp(oldVolDensityWarp, newVolDensityWarp, progress);

                currentRenderCloudProfile.WeatherMapCloudDensityNegationAdder.LastValue = Mathf.Lerp(oldVolDensityNegationAdder, newVolDensityNegationAdder, progress);
                //currentRenderCloudProfile.weatherMapCloudDensityNegationOffsetCalculated = Vector2.Lerp(oldVolDensityNegationOffset, newVolDensityNegationOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityNegationPower.LastValue = Mathf.Lerp(oldVolDensityNegationPower, newVolDensityNegationPower, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityNegationRotation.LastValue = Mathf.Lerp(oldVolDensityNegationRotation, newVolDensityNegationRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityNegationScale.LastValue = Mathf.Lerp(oldVolDensityNegationScale, newVolDensityNegationScale, progress);
                currentRenderCloudProfile.WeatherMapCloudDensityNegationWarp = Vector4.Lerp(oldVolDensityNegationWarp, newVolDensityNegationWarp, progress);

                currentRenderCloudProfile.WeatherMapCloudTypeNoiseType.LastValue = Mathf.Lerp(oldVolTypeNoiseType, newVolTypeNoiseType, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeScale.LastValue = Mathf.Lerp(oldVolTypeScale, newVolTypeScale, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeAdder.LastValue = Mathf.Lerp(oldVolTypeAdder, newVolTypeAdder, progress);
                //currentRenderCloudProfile.cloudTypeOffset = Vector3.Lerp(oldVolTypeOffset, newVolTypeOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudTypePower.LastValue = Mathf.Lerp(oldVolTypePower, newVolTypePower, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeRotation.LastValue = Mathf.Lerp(oldVolTypeRotation, newVolTypeRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeWarp = Vector4.Lerp(oldVolTypeWarp, newVolTypeWarp, progress);

                currentRenderCloudProfile.WeatherMapCloudTypeNegationAdder.LastValue = Mathf.Lerp(oldVolTypeNegationAdder, newVolTypeNegationAdder, progress);
                //currentRenderCloudProfile.weatherMapCloudTypeNegationOffsetCalculated = Vector2.Lerp(oldVolTypeNegationOffset, newVolTypeNegationOffset, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeNegationPower.LastValue = Mathf.Lerp(oldVolTypeNegationPower, newVolTypeNegationPower, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeNegationRotation.LastValue = Mathf.Lerp(oldVolTypeNegationRotation, newVolTypeNegationRotation, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeNegationScale.LastValue = Mathf.Lerp(oldVolTypeNegationScale, newVolTypeNegationScale, progress);
                currentRenderCloudProfile.WeatherMapCloudTypeNegationWarp = Vector4.Lerp(oldVolTypeNegationWarp, newVolTypeNegationWarp, progress);

                currentRenderCloudProfile.WorldScale = Mathf.Lerp(oldVolWeatherMapScale.z, newVolWeatherMapScale.z, progress);
                currentRenderCloudProfile.NoiseScaleX.LastValue = Mathf.Lerp(oldVolWeatherMapScale.x, newVolWeatherMapScale.x, progress);
                currentRenderCloudProfile.NoiseScaleY.LastValue = Mathf.Lerp(oldVolWeatherMapScale.y, newVolWeatherMapScale.y, progress);

                currentRenderCloudProfile.DirectionalLightIntensityMultiplier = Mathf.Lerp(startDirectionalLightIntensityMultiplier, endDirectionalLightIntensityMultiplier, progress);
                currentRenderCloudProfile.DirectionalLightScatterMultiplier = Mathf.Lerp(startDirectionalLightScatterMultiplier, endDirectionalLightScatterMultiplier, progress);
                currentRenderCloudProfile.DirectionalLightAtmosphericShadow = Mathf.Lerp(startDirectionalLightAtmosphereShadow, endDirectionalLightAtmosphereShadow, progress);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudSdfThreshold = Mathf.Min(startCloudSdfThreshold, endCloudSdfTheshold);
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudCoverMinimum = Mathf.Min(startCloudCoverageMinimum, endCloudCoverageMinimum);

                for (int i = 0; i < WeatherMakerCloudVolumetricProfileScript.CurveLength; i++)
                {
                    currentRenderCloudProfile.CloudLayerVolumetric1.cloudCoverCurve[i] = Mathf.Lerp(oldVolCoverCurve[i], newVolCoverCurve[i], progress);
                    currentRenderCloudProfile.CloudLayerVolumetric1.cloudDensityCurve[i] = Mathf.Lerp(oldVolDensityCurve[i], newVolDensityCurve[i], progress);
                    currentRenderCloudProfile.CloudLayerVolumetric1.cloudTypeCurve[i] = Mathf.Lerp(oldVolTypeCurve[i], newVolTypeCurve[i], progress);
                    currentRenderCloudProfile.CloudLayerVolumetric1.cloudNoiseDetailPowerCurve[i] = Mathf.Lerp(oldVolDetailCurve[i], newVolDetailCurve[i], progress);
                }

                currentRenderCloudProfile.CloudLayerVolumetric1.lerpProgress = progress;
            }, (ITween<float> c) =>
            {
                currentRenderCloudProfile.isAnimating = false;
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudSdfThreshold = endCloudSdfTheshold;
                currentRenderCloudProfile.CloudLayerVolumetric1.CloudCoverMinimum = endCloudCoverageMinimum;
                shapeLerp.Dispose();
                detailLerp.Dispose();
                curlLerp.Dispose();
            });
            tween.Delay = transitionDelay;
        }

        internal void ForceReload()
        {
            ShowCloudsAnimated(lastProfileOriginal, 0.0f, 0.01f);
        }

        /// <summary>
        /// Hide clouds animated, all layers
        /// </summary>
        /// <param name="duration">Transition duration in seconds</param>
        public void HideCloudsAnimated(float duration)
        {
            ShowCloudsAnimated((WeatherMakerCloudProfileScript)null, 0.0f, duration);
        }

        public static bool CloudProbeEnabled
        {
            get { return SystemInfo.supportsComputeShaders && SystemInfo.supportsAsyncGPUReadback; }
        }

        /// <summary>
        /// Get the current global shadow from clouds
        /// </summary>
        public float CloudGlobalShadow { get; private set; }

        private static WeatherMakerFullScreenCloudsScript instance;
        /// <summary>
        /// Shared instance of full screen clouds script
        /// </summary>
        public static WeatherMakerFullScreenCloudsScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
