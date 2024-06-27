using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Off screen buffer script
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerOffScreenBufferScript : MonoBehaviour
    {
        private CommandBuffer commandBuffer;

        /// <summary>
        /// Renderer
        /// </summary>
        public Renderer Renderer;

        /// <summary>
        /// Blur material
        /// </summary>
        public Material BlurMaterial;

        /// <summary>
        /// Scale
        /// </summary>
        public WeatherMakerDownsampleScale Scale = WeatherMakerDownsampleScale.HalfResolution;

        /// <summary>
        /// Camera event
        /// </summary>
        public CameraEvent CameraEvent = CameraEvent.AfterForwardOpaque;

        private void Update()
        {
            
        }

        private void OnEnable()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
            commandBuffer = new CommandBuffer { name = "WeatherMakerOffScreenBufferScript_" + gameObject.name };
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
            if (!WeatherMakerScript.ShouldIgnoreCamera(this, camera))
            {
                commandBuffer.Clear();
                if (BlurMaterial != null)
                {
                    RenderTextureFormat format = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
                    RenderTextureDescriptor desc1 = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor((int)Scale, 1, 1, format, 0, camera);
                    RenderTextureDescriptor desc2 = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(1, 1, 1, format, 0, camera);
                    commandBuffer.GetTemporaryRT(WMS._MainTex2, desc1);
                    commandBuffer.GetTemporaryRT(WMS._MainTex3, desc2);
                    commandBuffer.SetRenderTarget(WMS._MainTex2, 0, CubemapFace.Unknown, -1);
                    commandBuffer.ClearRenderTarget(true, true, Color.clear);
                    commandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, (float)Scale);
                    commandBuffer.DrawRenderer(Renderer, Renderer.sharedMaterial, 0, 0); // draw pass
                    commandBuffer.SetRenderTarget(WMS._MainTex3, 0, CubemapFace.Unknown, -1);
                    commandBuffer.ClearRenderTarget(true, true, Color.clear);
                    commandBuffer.DrawRenderer(Renderer, Renderer.sharedMaterial, 0, 1); // depth pass
                    commandBuffer.SetGlobalFloat(WMS._DstBlendMode, (float)BlendMode.OneMinusSrcAlpha);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, WMS._MainTex2, WeatherMakerFullScreenEffect.CameraTargetIdentifier(), BlurMaterial, 0);
                    commandBuffer.ReleaseTemporaryRT(WMS._MainTex2);
                    commandBuffer.ReleaseTemporaryRT(WMS._MainTex3);
                    camera.AddCommandBuffer(CameraEvent, commandBuffer);
                }
            }
        }

        private void CameraPreRender(Camera camera)
        {

        }

        private void CameraPostRender(Camera camera)
        {
            if (!WeatherMakerScript.ShouldIgnoreCamera(this, camera) && commandBuffer != null)
            {
                camera.RemoveCommandBuffer(CameraEvent, commandBuffer);
            }
        }
    }
}
