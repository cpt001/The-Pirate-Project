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
    /// Shadow map generator script, add to a dir light
    /// </summary>
    [RequireComponent(typeof(Light))]
    [ExecuteInEditMode]
    public class WeatherMakerShadowMapScript : MonoBehaviour
    {
        /// <summary>The texture name for shaders to access the cascaded shadow map, null/empty for none.</summary>
        [Tooltip("The texture name for shaders to access the cascaded shadow map, null/empty for none.")]
        public string ShaderTextureName = "_WeatherMakerShadowMapTexture";

        /// <summary>Optional material to add cloud shadows to the shadow map, null for no cloud shadows.</summary>
        [Tooltip("Optional material to add cloud shadows to the shadow map, null for no cloud shadows.")]
        public Material CloudShadowMaterial;

        /// <summary>Gaussian blur material.</summary>
        [Tooltip("Gaussian blur material.")]
        public Material BlurMaterial;

        private Light _light;

#if UNITY_URP

        private CameraEvent urpEvent;

#endif

        private CommandBuffer commandBufferDepthShadows;
        private Dictionary<Camera, CommandBuffer> commandBufferScreenSpaceShadowsDictionary1;
        private List<CommandBuffer> commandBufferScreenSpaceShadowsList1;
        private Dictionary<Camera, CommandBuffer> commandBufferScreenSpaceShadowsDictionary2;
        private List<CommandBuffer> commandBufferScreenSpaceShadowsList2;
        private List<RenderTexture> tempShadowBuffers;

        private struct CommandBufferNameCacheEntry
        {
            public string Prefix { get; set; }
            public Camera Camera { get; set; }
            public string Name { get; set; }
        }
        private List<CommandBufferNameCacheEntry> commandBufferNameCache = new List<CommandBufferNameCacheEntry>();

        private string GetCommandBufferName(string prefix, Camera camera)
        {
            foreach (var existingEntry in commandBufferNameCache)
            {
                if (existingEntry.Prefix == prefix &&
                    existingEntry.Camera == camera)
                {
                    return existingEntry.Name;
                }
            }
            CommandBufferNameCacheEntry entry = new CommandBufferNameCacheEntry
            {
                Camera = camera,
                Prefix = prefix,
                Name = prefix + gameObject.CachedName() + "_" + camera.CachedName()
            };
            commandBufferNameCache.Add(entry);
            if (commandBufferNameCache.Count == 500)
            {
                // make sure we don't over-run memory
                commandBufferNameCache.Clear();
            }
            return entry.Name;
        }

        private void RemoveCommandBuffer(LightEvent evt, CommandBuffer commandBuffer, bool nullOut = true)
        {
            RemoveCommandBuffer(evt, ref commandBuffer, nullOut);
        }

        private void RemoveCommandBuffer(LightEvent evt, ref CommandBuffer commandBuffer, bool nullOut = true)
        {

#if !UNITY_URP

            if (_light != null && commandBuffer != null)
            {
                // putting these in try/catch as Unity 2018.3 throws weird errors
                try
                {
                    _light.RemoveCommandBuffer(evt, commandBuffer);
                    if (nullOut)
                    {
                        commandBuffer.Clear();
                        commandBuffer.Release();
                    }
                }
                catch
                {
                    // eat exceptions
                }
                if (nullOut)
                {
                    commandBuffer = null;
                }
            }

#endif

        }

        /// <summary>
        /// Initiate cleanup of command buffers
        /// </summary>
        public void CleanupCommandBuffers()
        {
            RemoveCommandBuffer(LightEvent.AfterShadowMap, ref commandBufferDepthShadows);
            if (commandBufferScreenSpaceShadowsDictionary1 != null)
            {
                foreach (CommandBuffer _commandBufferScreenSpaceShadows1 in commandBufferScreenSpaceShadowsDictionary1.Values)
                {
                    RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, _commandBufferScreenSpaceShadows1);
                }
                commandBufferScreenSpaceShadowsDictionary1.Clear();
                commandBufferScreenSpaceShadowsList1.Clear();
            }
            if (commandBufferScreenSpaceShadowsDictionary2 != null)
            {
                foreach (CommandBuffer _commandBufferScreenSpaceShadows2 in commandBufferScreenSpaceShadowsDictionary2.Values)
                {
                    RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, _commandBufferScreenSpaceShadows2);
                }
                commandBufferScreenSpaceShadowsDictionary2.Clear();
                commandBufferScreenSpaceShadowsList2.Clear();
            }
            if (tempShadowBuffers != null)
            {
                foreach (RenderTexture tempShadowBuffer in tempShadowBuffers)
                {
                    RenderTexture.ReleaseTemporary(tempShadowBuffer);
                }
                tempShadowBuffers.Clear();
            }
        }

        private void AddShadowMapCommandBuffer()
        {

#if !UNITY_URP

            if (_light != null && !string.IsNullOrEmpty(ShaderTextureName))
            {
                if (commandBufferDepthShadows == null)
                {
                    commandBufferDepthShadows = new CommandBuffer { name = "WeatherMakerShadowMapDepthShadowScript_" + gameObject.name };
                }
                commandBufferDepthShadows.Clear();
                _light.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBufferDepthShadows);
                commandBufferDepthShadows.SetGlobalTexture(ShaderTextureName, BuiltinRenderTextureType.CurrentActive);
                _light.AddCommandBuffer(LightEvent.AfterShadowMap, commandBufferDepthShadows);
            }

#endif

        }

        private void AddScreenSpaceShadowsCommandBuffer(Camera camera)
        {
            if (CloudShadowMaterial != null &&
                _light != null &&
                _light.type == LightType.Directional &&
                _light.shadows != LightShadows.None &&
                WeatherMakerCommandBufferManagerScript.Instance != null &&
                WeatherMakerLightManagerScript.Instance != null &&
                tempShadowBuffers.Count > 0 &&
                WeatherMakerLightManagerScript.ScreenSpaceShadowMode != BuiltinShaderMode.Disabled &&
                WeatherMakerScript.Instance != null &&
                WeatherMakerScript.Instance.PerformanceProfile != null &&
                WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowDownsampleScale != WeatherMakerDownsampleScale.Disabled &&
                WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowSampleCount > 0)
            {
                RenderTexture tempShadowBuffer = tempShadowBuffers[tempShadowBuffers.Count - 1];

                commandBufferScreenSpaceShadowsDictionary1.TryGetValue(camera, out CommandBuffer _commandBufferScreenSpaceShadows1);
                if (_commandBufferScreenSpaceShadows1 == null)
                {
                    // copy the screen space shadow texture for re-use later
                    _commandBufferScreenSpaceShadows1 = WeatherMakerCommandBufferManagerScript.Instance.GetOrCreateCommandBuffer();
                    _commandBufferScreenSpaceShadows1.name = GetCommandBufferName("WeatherMakerShadowMapScreensSpaceShadowScriptBlur_", camera);
                    commandBufferScreenSpaceShadowsDictionary1[camera] = _commandBufferScreenSpaceShadows1;
                }
                commandBufferScreenSpaceShadowsDictionary2.TryGetValue(camera, out CommandBuffer _commandBufferScreenSpaceShadows2);
                if (_commandBufferScreenSpaceShadows2 == null)
                {
                    // copy the screen space shadow texture for re-use later
                    _commandBufferScreenSpaceShadows2 = WeatherMakerCommandBufferManagerScript.Instance.GetOrCreateCommandBuffer();
                    _commandBufferScreenSpaceShadows2.name = GetCommandBufferName("WeatherMakerShadowMapScreensSpaceShadowScriptBlit_", camera);
                    commandBufferScreenSpaceShadowsDictionary2[camera] = _commandBufferScreenSpaceShadows2;
                }

                _commandBufferScreenSpaceShadows1.Clear();
                _commandBufferScreenSpaceShadows1.SetGlobalFloat(WMS._BlendOp, (float)BlendOp.Add);
                _commandBufferScreenSpaceShadows1.SetGlobalFloat(WMS._SrcBlendMode, (float)BlendMode.One);
                _commandBufferScreenSpaceShadows1.SetGlobalFloat(WMS._DstBlendMode, (float)BlendMode.Zero);

#if UNITY_URP

                if (commandBufferScreenSpaceShadowsList1.Count != 0)
                {
                    camera.RemoveCommandBuffer(urpEvent, commandBufferScreenSpaceShadowsList1[commandBufferScreenSpaceShadowsList1.Count - 1]);
                }
                _commandBufferScreenSpaceShadows1.SetGlobalTexture(WeatherMakerLightManagerScript.Instance.ScreenSpaceShadowsRenderTextureName, WMS._ScreenSpaceShadowmapTexture);
                _commandBufferScreenSpaceShadows1.Blit(WMS._ScreenSpaceShadowmapTexture, tempShadowBuffer, CloudShadowMaterial, 0);
                camera.AddCommandBuffer(urpEvent, _commandBufferScreenSpaceShadows1);

#else

                if (commandBufferScreenSpaceShadowsList1.Count != 0)
                {
                    _light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBufferScreenSpaceShadowsList1[commandBufferScreenSpaceShadowsList1.Count - 1]);
                }
                _commandBufferScreenSpaceShadows1.SetGlobalTexture(WeatherMakerLightManagerScript.Instance.ScreenSpaceShadowsRenderTextureName, BuiltinRenderTextureType.CurrentActive);
                _commandBufferScreenSpaceShadows1.Blit(BuiltinRenderTextureType.CurrentActive, tempShadowBuffer, CloudShadowMaterial, 0);
                _light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, _commandBufferScreenSpaceShadows1);

#endif

                // screen space shadow mask does not use concept of stereo, so turn it off
                _commandBufferScreenSpaceShadows2.Clear();
                _commandBufferScreenSpaceShadows2.SetGlobalFloat(WMS._SrcBlendMode, (float)BlendMode.One);
                _commandBufferScreenSpaceShadows2.SetGlobalFloat(WMS._DstBlendMode, (float)BlendMode.One);
                _commandBufferScreenSpaceShadows2.SetGlobalFloat(WMS._WeatherMakerAdjustFullScreenUVStereoDisable, 1.0f);

#if UNITY_URP

                _commandBufferScreenSpaceShadows2.Blit(tempShadowBuffer, WMS._ScreenSpaceShadowmapTexture, BlurMaterial, 0);

#else

                _commandBufferScreenSpaceShadows2.Blit(tempShadowBuffer, BuiltinRenderTextureType.CurrentActive, BlurMaterial, 0);

#endif

                // must be set back to 0 after the blit
                _commandBufferScreenSpaceShadows2.SetGlobalFloat(WMS._WeatherMakerAdjustFullScreenUVStereoDisable, 0.0f);

#if UNITY_URP

                if (commandBufferScreenSpaceShadowsList2.Count != 0)
                {
                    camera.RemoveCommandBuffer(urpEvent, commandBufferScreenSpaceShadowsList2[commandBufferScreenSpaceShadowsList2.Count - 1]);
                }
                _commandBufferScreenSpaceShadows2.SetRenderTarget(WeatherMakerFullScreenEffect.CameraTargetIdentifier(), WMS._CameraDepthTexture, 0, CubemapFace.Unknown, -1);
                camera.AddCommandBuffer(urpEvent, _commandBufferScreenSpaceShadows2);

#else

                if (commandBufferScreenSpaceShadowsList2.Count != 0)
                {
                    _light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBufferScreenSpaceShadowsList2[commandBufferScreenSpaceShadowsList2.Count - 1]);
                }
                _light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, _commandBufferScreenSpaceShadows2);

#endif

                commandBufferScreenSpaceShadowsList1.Add(_commandBufferScreenSpaceShadows1);
                commandBufferScreenSpaceShadowsList2.Add(_commandBufferScreenSpaceShadows2);
            }
            else
            {
                CleanupCommandBuffers();
            }
        }

        private void CreateCommandBuffers()
        {
            CleanupCommandBuffers();
            AddShadowMapCommandBuffer();
        }

        private void Update()
        {
            // ensure that the any shader using cloud shadows knows the correct cloud shadow parameters
            if (CloudShadowMaterial != null)
            {
                Shader.SetGlobalFloat(WMS._CloudShadowMapAdder, CloudShadowMaterial.GetFloat(WMS._CloudShadowMapAdder));
                Shader.SetGlobalFloat(WMS._CloudShadowMapMultiplier, CloudShadowMaterial.GetFloat(WMS._CloudShadowMapMultiplier));
                Shader.SetGlobalFloat(WMS._CloudShadowMapPower, CloudShadowMaterial.GetFloat(WMS._CloudShadowMapPower));
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudVolumetricShadowDither, CloudShadowMaterial.GetFloat(WMS._WeatherMakerCloudVolumetricShadowDither));
                Shader.SetGlobalTexture(WMS._WeatherMakerCloudShadowDetailTexture, CloudShadowMaterial.GetTexture(WMS._WeatherMakerCloudShadowDetailTexture));
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudShadowDetailScale, CloudShadowMaterial.GetFloat(WMS._WeatherMakerCloudShadowDetailScale));
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudShadowDetailIntensity, CloudShadowMaterial.GetFloat(WMS._WeatherMakerCloudShadowDetailIntensity));
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudShadowDetailFalloff, CloudShadowMaterial.GetFloat(WMS._WeatherMakerCloudShadowDetailFalloff));
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudShadowDistanceFade, CloudShadowMaterial.GetFloat(WMS._WeatherMakerCloudShadowDistanceFade));
            }
        }

        private void OnEnable()
        {

#if UNITY_URP

            urpEvent = (Camera.main != null && Camera.main.actualRenderingPath == RenderingPath.DeferredShading ? CameraEvent.AfterGBuffer : CameraEvent.BeforeForwardOpaque);

#endif

            tempShadowBuffers = new List<RenderTexture>();
            commandBufferScreenSpaceShadowsDictionary1 = new Dictionary<Camera, CommandBuffer>();
            commandBufferScreenSpaceShadowsList1 = new List<CommandBuffer>();
            commandBufferScreenSpaceShadowsDictionary2 = new Dictionary<Camera, CommandBuffer>();
            commandBufferScreenSpaceShadowsList2 = new List<CommandBuffer>();
            _light = GetComponent<Light>();
            CreateCommandBuffers();
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
        }

        private void OnDisable()
        {
            CleanupCommandBuffers();
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPostRender(this);
            }
        }

        private void CameraPreCull(Camera camera)
        {
            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null)
            {
                return;
            }

            WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
            if ((WeatherMakerCommandBufferManagerScript.CameraStackCount == 1 && cameraType == WeatherMakerCameraType.Normal) ||
                ((cameraType == WeatherMakerCameraType.Reflection || cameraType == WeatherMakerCameraType.CubeMap)
                && WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudReflectionShadows))
            {
                // render cloud shadows at half scale for large screens
                int scale = (int)WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowDownsampleScale;
                RenderTextureFormat format = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
                RenderTexture tempShadowBuffer = RenderTexture.GetTemporary(WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(scale, 0, 1,
                    format, 0, camera));
                tempShadowBuffer.wrapMode = TextureWrapMode.Clamp;
                tempShadowBuffer.filterMode = FilterMode.Bilinear;
                tempShadowBuffers.Add(tempShadowBuffer);
                AddScreenSpaceShadowsCommandBuffer(camera);
            }
        }

        private void CameraPostRender(Camera camera)
        {
            if (tempShadowBuffers.Count == 0)
            {
                return;
            }

            RenderTexture tempShadowBuffer = tempShadowBuffers[tempShadowBuffers.Count - 1];
            tempShadowBuffers.RemoveAt(tempShadowBuffers.Count - 1);
            RenderTexture.ReleaseTemporary(tempShadowBuffer);

            if (WeatherMakerScript.Instance == null ||
                WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerCommandBufferManagerScript.Instance == null)
            {
                return;
            }

            commandBufferScreenSpaceShadowsDictionary1.TryGetValue(camera, out CommandBuffer _commandBufferScreenSpaceShadows1);
            commandBufferScreenSpaceShadowsDictionary2.TryGetValue(camera, out CommandBuffer _commandBufferScreenSpaceShadows2);

#if UNITY_URP

            if (_commandBufferScreenSpaceShadows1 != null)
            {
                camera.RemoveCommandBuffer(urpEvent, _commandBufferScreenSpaceShadows1);
            }
            if (_commandBufferScreenSpaceShadows2 != null)
            {
                camera.RemoveCommandBuffer(urpEvent, _commandBufferScreenSpaceShadows2);
            }

#else

            if (_commandBufferScreenSpaceShadows1 != null)
            {
                RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, ref _commandBufferScreenSpaceShadows1, false);
            }
            if (_commandBufferScreenSpaceShadows2 != null)
            {
                RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, ref _commandBufferScreenSpaceShadows2, false);
            }

#endif

            CommandBuffer existing;

            if (commandBufferScreenSpaceShadowsDictionary1.TryGetValue(camera, out existing))
            {
                WeatherMakerCommandBufferManagerScript.Instance.ReturnCommandBufferToPool(existing);
                commandBufferScreenSpaceShadowsDictionary1.Remove(camera);
            }
            if (_commandBufferScreenSpaceShadows1 != null)
            {
                commandBufferScreenSpaceShadowsList1.Remove(_commandBufferScreenSpaceShadows1);
            }

            if (commandBufferScreenSpaceShadowsDictionary2.TryGetValue(camera, out existing))
            {
                WeatherMakerCommandBufferManagerScript.Instance.ReturnCommandBufferToPool(existing);
                commandBufferScreenSpaceShadowsDictionary2.Remove(camera);
            }
            if (_commandBufferScreenSpaceShadows2 != null)
            {
                commandBufferScreenSpaceShadowsList2.Remove(_commandBufferScreenSpaceShadows2);
            }

#if UNITY_URP

            if (commandBufferScreenSpaceShadowsList1.Count != 0)
            {
                camera.AddCommandBuffer(urpEvent, commandBufferScreenSpaceShadowsList1[commandBufferScreenSpaceShadowsList1.Count - 1]);
            }
            if (commandBufferScreenSpaceShadowsList2.Count != 0)
            {
                camera.AddCommandBuffer(urpEvent, commandBufferScreenSpaceShadowsList2[commandBufferScreenSpaceShadowsList2.Count - 1]);
            }

#else

            if (commandBufferScreenSpaceShadowsList1.Count != 0)
            {
                _light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBufferScreenSpaceShadowsList1[commandBufferScreenSpaceShadowsList1.Count - 1]);
            }
            if (commandBufferScreenSpaceShadowsList2.Count != 0)
            {
                _light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBufferScreenSpaceShadowsList2[commandBufferScreenSpaceShadowsList2.Count - 1]);
            }

#endif

        }
    }
}