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
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Full screen overlay
    /// </summary>
    /// <typeparam name="TProfile">Type of overlay profile</typeparam>
    [ExecuteInEditMode]
    public class WeatherMakerFullScreenOverlayScript<TProfile> : MonoBehaviour where TProfile : WeatherMakerOverlayProfileScriptBase
    {
        /// <summary>Overlay profile</summary>
        [Header("Overlay - profile")]
        [Tooltip("Overlay profile")]
        public TProfile OverlayProfile;

        /// <summary>Down sample scale.</summary>
        [Header("Overlay - rendering")]
        [Tooltip("Down sample scale.")]
        public WeatherMakerDownsampleScale DownSampleScale = WeatherMakerDownsampleScale.FullResolution;

        /// <summary>Material that renders the overlay effect</summary>
        [Tooltip("Material that renders the overlay effect")]
        public Material OverlayMaterial;

        /// <summary>Material to render the overlay full screen alpha in a second pass if needed</summary>
        [Tooltip("Material to render the overlay full screen alpha in a second pass if needed")]
        public Material OverlayAlphaMaterial;

        /// <summary>Overlay blur Material.</summary>
        [Tooltip("Overlay blur Material.")]
        public Material OverlayBlurMaterial;

        /// <summary>Overlay blur Shader Type.</summary>
        [Tooltip("Overlay blur Shader Type.")]
        public BlurShaderType BlurShader;

        /// <summary>Render overlay in this render queue for the command buffer.</summary>
        [Tooltip("Render overlay in this render queue for the command buffer.")]
        public CameraEvent OverlayRenderQueue = CameraEvent.BeforeImageEffectsOpaque;

        /// <summary>External intensity function</summary>
        [Tooltip("External intensity function")]
        public WeatherMakerOutputParameterEventFloat ExternalIntensityFunction;

        /// <summary>Whether to render overlay in reflection cameras.</summary>
        [Tooltip("Whether to render overlay in reflection cameras.")]
        public bool AllowReflections = true;

        /// <summary>
        /// Current effect
        /// </summary>
        public WeatherMakerFullScreenEffect Effect { get; private set; }

        private readonly WeatherMakerOutputParameterFloat param = new WeatherMakerOutputParameterFloat();
        private System.Action<WeatherMakerCommandBuffer> updateShaderPropertiesAction;

        /// <summary>
        /// Command buffer name
        /// </summary>
        protected string CommandBufferName = "WeatherMakerFullScreenOverlayScript";

        private void CleanupEffect()
        {
            if (Effect != null)
            {
                Effect.Dispose();
                Effect = null;
            }
        }

        private void UpdateEffectProperties()
        {
            if (Effect == null) 
            {
                Effect = new WeatherMakerFullScreenEffect
                {
                    CommandBufferName = this.CommandBufferName,
                    RenderQueue = OverlayRenderQueue
                };
            }
            SetupEffect(Effect);
            bool showOverlay = (OverlayProfile != null && !OverlayProfile.Disabled && (OverlayProfile.OverlayIntensity > 0.0001f || OverlayProfile.AutoIntensityMultiplier != 0.0f || OverlayProfile.OverlayMinimumIntensity > 0.0001f) && OverlayProfile.OverlayColor.a > 0.0f);
            if (showOverlay)
            {
                OverlayProfile.Update();
            }
            updateShaderPropertiesAction = (updateShaderPropertiesAction ?? UpdateShaderProperties);
            Effect.SetupEffect(OverlayMaterial, OverlayAlphaMaterial, OverlayBlurMaterial, OverlayBlurMaterial, BlurShader, DownSampleScale, WeatherMakerDownsampleScale.Disabled, WeatherMakerDownsampleScale.Disabled,
                null, WeatherMakerTemporalReprojectionSize.None, updateShaderPropertiesAction, showOverlay);
        }

        private float ExternalIntensityFunctionImpl()
        {
            ExternalIntensityFunction.Invoke(param);
            return param.Value;
        }

        /// <summary>
        /// Setup an effect
        /// </summary>
        /// <param name="effect">Effect to setup</param>
        protected virtual void SetupEffect(WeatherMakerFullScreenEffect effect)
        {
        }

        /// <summary>
        /// Apply all properties to shader fields
        /// </summary>
        /// <param name="commandBuffer">Command buffer</param>
        protected virtual void UpdateShaderProperties(WeatherMakerCommandBuffer commandBuffer)
        {
            if (OverlayProfile != null && !OverlayProfile.Disabled)
            {
                if (ExternalIntensityFunction == null)
                {
                    OverlayProfile.ExternalIntensityFunction = null;
                }
                else
                {
                    OverlayProfile.ExternalIntensityFunction = ExternalIntensityFunctionImpl;
                }
                OverlayProfile.UpdateMaterial(commandBuffer.Material);
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        protected virtual void Update()
        {
        }

        private void LateUpdate()
        {
            UpdateEffectProperties();
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            // clone profile to prevent accidental modification
            if (Application.isPlaying && OverlayProfile != null)
            {
                OverlayProfile = ScriptableObject.Instantiate(OverlayProfile) as TProfile;
            }
            CleanupEffect();

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreRender(CameraPreRender, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
        }

        private void OnDisable()
        {
            CleanupEffect();
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected virtual void OnDestroy()
        {
            CleanupEffect();

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreRender(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPostRender(this);
            }
        }

        /// <summary>
        /// Camera pre cull event
        /// </summary>
        /// <param name="camera">Camera</param>
        protected virtual void CameraPreCull(Camera camera)
        {
            if (Effect != null && Effect.Enabled && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {
                Effect.PreCullCamera(camera, true);
            }
        }

        /// <summary>
        /// Camera pre render event
        /// </summary>
        /// <param name="camera">Camera</param>
        protected virtual void CameraPreRender(Camera camera)
        {
            if (Effect != null && Effect.Enabled && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {

#if UNITY_EDITOR

                if (WeatherMakerScript.GetCameraType(camera) == WeatherMakerCameraType.Normal &&
                    camera.cameraType == CameraType.Game &&
                    (camera.actualRenderingPath == RenderingPath.Forward || camera.actualRenderingPath == RenderingPath.VertexLit))
                {
                    Debug.LogError("Weather Maker full screen overlay works best with deferred shading");
                }

#endif

                Effect.PreRenderCamera(camera);
            }
        }

        /// <summary>
        /// Camera post render event
        /// </summary>
        /// <param name="camera">Camera</param>
        protected virtual void CameraPostRender(Camera camera)
        {
            if (Effect != null && Effect.Enabled && !WeatherMakerScript.ShouldIgnoreCamera(this, camera, !AllowReflections))
            {
                Effect.PostRenderCamera(camera);
            }
        }
    }
}
