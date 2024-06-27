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

using System;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Contains all code to render and handle full screen fog
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerFullScreenFogScript : WeatherMakerFogScript<WeatherMakerFullScreenFogProfileScript>
    {
        /// <summary>Whether to apply settings to Unity fog (true) or to use Weather Maker full screen volumetric fog (false).</summary>
        [Header("Unity fog")]
        [Tooltip("Whether to apply settings to Unity fog (true) or to use Weather Maker full screen volumetric fog (false).")]
        public bool UseUnityFog;

        /// <summary>Down sample scale.</summary>
        [Header("Full screen fog - rendering")]
        [Tooltip("Down sample scale.")]
        public WeatherMakerDownsampleScale DownSampleScale = WeatherMakerDownsampleScale.FullResolution;

        /// <summary>Down sample scale full screen shafts.</summary>
        [Tooltip("Down sample scale full screen shafts.")]
        public WeatherMakerDownsampleScale DownSampleScaleFullScreenShafts = WeatherMakerDownsampleScale.FullResolution;

        /// <summary>Material to render the fog full screen after it has been calculated</summary>
        [Tooltip("Material to render the fog full screen after it has been calculated")]
        public Material FogFullScreenMaterial;

        /// <summary>Fog Blur Material.</summary>
        [Tooltip("Fog Blur Material.")]
        public Material FogBlurMaterial;

        /// <summary>Material for temporal reprojection</summary>
        [Tooltip("Material for temporal reprojection")]
        public Material TemporalReprojectionMaterial;

        /// <summary>Fog Blur Shader Type.</summary>
        [Tooltip("Fog Blur Shader Type.")]
        public BlurShaderType BlurShader;

        /// <summary>Temporal reprojection size - allows rendering a portion of this effect over a number of frames to spread cost out over time. This can introduce rendering artifacts so be on the lookout for that.</summary>
        [Tooltip("Temporal reprojection size - allows rendering a portion of this effect over a number of frames to spread cost out over time. " +
            "This can introduce rendering artifacts so be on the lookout for that.")]
        public WeatherMakerTemporalReprojectionSize TemporalReprojection = WeatherMakerTemporalReprojectionSize.None;

        /// <summary>Render fog in this render queue for the command buffer.</summary>
        [Tooltip("Render fog in this render queue for the command buffer.")]
        public CameraEvent FogRenderQueue = CameraEvent.BeforeForwardAlpha;

        /// <summary>Whether to render fog in reflection cameras.</summary>
        [Tooltip("Whether to render fog in reflection cameras.")]
        public bool AllowReflections = true;

        private WeatherMakerFullScreenEffect effect;
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;

        private const string commandBufferName = "WeatherMakerFullScreenFogScript";

        private void UpdateUnityFog(Camera camera)
        {
            if (!UseUnityFog)
            {
                return;
            }

            WeatherMakerCelestialObjectScript sun = WeatherMakerLightManagerScript.SunForCamera(camera);
            if (sun == null)
            {
                return;
            }

            RenderSettings.fog = (FogProfile.fogDensity > 0.0001f && FogProfile.FogMode != WeatherMakerFogMode.None);
            Color fogColor = (FogProfile.FogColor * (sun == null || !sun.LightIsOn ? Color.black : sun.GetGradientColor(FogProfile.FogGradientColor)));
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                bool cameraIsPerspective = (camera == null || !camera.orthographic);
                foreach (WeatherMakerLightManagerScript.LightState light in WeatherMakerLightManagerScript.Instance.Lights)
                {
                    if (light.Light.type == LightType.Directional && light.Light != sun.Light)
                    {
                        WeatherMakerCelestialObjectScript obj = light.Light.GetComponent<WeatherMakerCelestialObjectScript>();
                        if (obj != null && obj.OrbitTypeIsPerspective == cameraIsPerspective)
                        {
                            fogColor += (FogProfile.FogColor * (light.Light.color * light.Light.intensity));
                        }
                        break;
                    }
                }
            }
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = FogProfile.fogDensity;
            switch (FogProfile.FogMode)
            {
                case WeatherMakerFogMode.Constant:
                case WeatherMakerFogMode.Linear:
                    RenderSettings.fogMode = FogMode.Linear;
                    break;

                case WeatherMakerFogMode.None:
                    RenderSettings.fogDensity = 0.0f;
                    break;

                case WeatherMakerFogMode.Exponential:
                    RenderSettings.fogMode = FogMode.Exponential;
                    break;

                case WeatherMakerFogMode.ExponentialSquared:
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    break;
            }
        }

        private void UpdateFogProperties(Camera camera)
        {
            if (FogProfile == null || WeatherMakerScript.Instance == null || WeatherMakerLightManagerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                return;
            }
            DownSampleScale = WeatherMakerScript.Instance.PerformanceProfile.FogDownsampleScale;
            TemporalReprojection = WeatherMakerScript.Instance.PerformanceProfile.FogTemporalReprojectionSize;
            DownSampleScaleFullScreenShafts = WeatherMakerScript.Instance.PerformanceProfile.FogDownsampleScaleFullScreenShafts;

            if (WeatherMakerLightManagerScript.ScreenSpaceShadowMode == BuiltinShaderMode.Disabled || QualitySettings.shadows == ShadowQuality.Disable)
            {
                float fogShadowMultiplier = Mathf.Clamp(FogProfile.FogIntensityMultiplier, 0.5f, 1.0f);
                WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers["WeatherMakerFullScreenFogScript"] = fogShadowMultiplier;
                WeatherMakerLightManagerScript.Instance.DirectionalLightShadowIntensityMultipliers["WeatherMakerFullScreenFogScript"] = FogProfile.FogGlobalShadow;
            }
            else
            {
                WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers.Remove("WeatherMakerFullScreenFogScript");
                WeatherMakerLightManagerScript.Instance.DirectionalLightShadowIntensityMultipliers.Remove("WeatherMakerFullScreenFogScript");
            }
            updateShaderPropertiesAction = (updateShaderPropertiesAction ?? UpdateShaderProperties);
            effect.SetupEffect
            (
                FogMaterial,
                FogFullScreenMaterial,
                FogBlurMaterial,
                FogBlurMaterial,
                BlurShader,
                DownSampleScale,
                FogProfile.SunShaftBackgroundColorMultiplier <= 0.0f ? WeatherMakerDownsampleScale.Disabled : DownSampleScaleFullScreenShafts,
                DownSampleScaleFullScreenShafts,
                TemporalReprojectionMaterial,
                TemporalReprojection,
                updateShaderPropertiesAction,
                (FogProfile.FogDensity > Mathf.Epsilon && FogProfile.FogMode != WeatherMakerFogMode.None && !UseUnityFog)
            );
            UpdateWind();
            UpdateUnityFog(camera);
        }

        private void UpdateShaderProperties(WeatherMakerCommandBuffer b)
        {
            if (FogProfile == null || WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null || WeatherMakerLightManagerScript.Instance == null)
            {
                return;
            }

            // temp turn off fog lights for reflection camera and save a lot of performance
            bool origFogLights = (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile.EnableFogLights);
            int origFogShafts = (WeatherMakerScript.Instance == null ? FogProfile.SunShaftSampleCount : WeatherMakerScript.Instance.PerformanceProfile.FogFullScreenSunShaftSampleCount);
            bool tempFogLights = origFogLights && (b.CameraType == WeatherMakerCameraType.Normal);
            int tempFogShafts = (b.CameraType == WeatherMakerCameraType.Normal ? origFogShafts : 0);
            float density = FogProfile.fogDensity;

            // TODO: See if this is really necessary anymore
            //if (b.CameraType == WeatherMakerCameraType.Reflection)
            //{
            // HACK: reflection camera hack, density is not correct but this compensates
            // TODO: figure out why fog formula is wrong in reflection camera
            //FogProfile.fogDensity = Mathf.Min(1.0f, density * 100.0f);
            //}
            WeatherMakerScript.Instance.PerformanceProfile.EnableFogLights = tempFogLights;
            WeatherMakerScript.Instance.PerformanceProfile.FogFullScreenSunShaftSampleCount = tempFogShafts;
            FogProfile.UpdateMaterialProperties(b.Material, b.Camera, true);
            FogProfile.fogDensity = density;
            if (WeatherMakerScript.Instance != null)
            {
                WeatherMakerScript.Instance.PerformanceProfile.EnableFogLights = origFogLights;
                WeatherMakerScript.Instance.PerformanceProfile.FogFullScreenSunShaftSampleCount = origFogShafts;
            }
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            // create fog profile now, base.Start will also create it but uses a non-full screen profile default
            if (FogProfile == null)
            {
                FogProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerFullScreenFogProfileScript>("WeatherMakerFullScreenFogProfile_Default");
            }

            base.Awake();

            effect = new WeatherMakerFullScreenEffect
            {
                CommandBufferName = commandBufferName,
                RenderQueue = FogRenderQueue
            };

            if (Application.isPlaying)
            {
                FogFullScreenMaterial = new Material(FogFullScreenMaterial);
                FogBlurMaterial = new Material(FogBlurMaterial);
            }
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            WeatherMakerScript.EnsureInstance(this, ref instance);
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreRender(CameraPreRender, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            if (effect != null)
            {
                effect.Dispose();
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (effect != null)
            {
                effect.Dispose();
            }
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreRender(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPostRender(this);
            }
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// Does nothing, no need to call base class as we set material properties elsewhere
        /// </summary>
        protected override void UpdateFogMaterialFromProfile()
        {
            // no need to call base class as we set material properties elsewhere
        }

        private void CameraPreCull(Camera camera)
        {
            if (effect != null && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {
                UpdateFogProperties(camera);
                effect.PreCullCamera(camera);
            }
        }

        private void CameraPreRender(Camera camera)
        {
            if (effect != null && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {
                effect.PreRenderCamera(camera);
            }
        }

        private void CameraPostRender(Camera camera)
        {
            if (effect != null && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {
                effect.PostRenderCamera(camera);
            }
        }

        /// <summary>
        /// Global shadow from fog effect
        /// </summary>
        public float FogGlobalShadow { get; private set; }

        private static WeatherMakerFullScreenFogScript instance;
        /// <summary>
        /// Shared instance of full screen fog script
        /// </summary>
        public static WeatherMakerFullScreenFogScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}