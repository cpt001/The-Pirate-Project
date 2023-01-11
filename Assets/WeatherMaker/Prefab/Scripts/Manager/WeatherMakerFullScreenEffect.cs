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

// #define COPY_FULL_DEPTH_TEXTURE

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Down sample scales - do not change the values ever!
    /// </summary>
    public enum WeatherMakerDownsampleScale
    {
        /// <summary>
        /// No downsampling
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Use full resolution
        /// </summary>
        FullResolution = 1,

        /// <summary>
        /// Use half resolution
        /// </summary>
        HalfResolution = 2,

        /// <summary>
        /// Use quarter resolution
        /// </summary>
        QuarterResolution = 4,

        /// <summary>
        /// Use eighth resolution
        /// </summary>
        EighthResolution = 8
    }

    /// <summary>
    /// Temporal reprojection size
    /// </summary>
    public enum WeatherMakerTemporalReprojectionSize
    {
        /// <summary>
        /// No temporal reprojection
        /// </summary>
        None = 0,

        /// <summary>
        /// Full size, only really useful for debugging to make sure image does not change
        /// </summary>
        One = 1,

        /// <summary>
        /// 2x2 grid
        /// </summary>
        TwoByTwo = 2,

        /// <summary>
        /// 3x3 grid
        /// </summary>
        ThreeByThree = 3,

        /// <summary>
        /// 4x4 grid
        /// </summary>
        FourByFour = 4,

        /// <summary>
        /// 5x5 grid
        /// </summary>
        FiveByFive = 5,

        /// <summary>
        /// 6x6 grid
        /// </summary>
        SixBySix = 6,

        /// <summary>
        /// 7x7 grid
        /// </summary>
        SevenBySeven = 7,

        /// <summary>
        /// 8x8 grid
        /// </summary>
        EightByEight = 8
    }

    /// <summary>
    /// Full screen effect, wraps up rendering multiple commands and command buffers in a nice wrapper class
    /// </summary>
    [System.Serializable]
    public class WeatherMakerFullScreenEffect : System.IDisposable
    {
        private static readonly Dictionary<int, string> downsampleKeyword = new Dictionary<int, string>
        {
            { 1, "" },
            { 2, "WEATHER_MAKER_DOWNSAMPLE_2" },
            { 3, "WEATHER_MAKER_DOWNSAMPLE_4" },
            { 4, "WEATHER_MAKER_DOWNSAMPLE_4" },
            { 5, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 6, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 7, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 8, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 9, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 10, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 11, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 12, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 13, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 14, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 15, "WEATHER_MAKER_DOWNSAMPLE_8" },
            { 16, "WEATHER_MAKER_DOWNSAMPLE_8" }
        };

        private readonly List<WeatherMakerTemporalReprojectionState> temporalStates = new List<WeatherMakerTemporalReprojectionState>();

        /// <summary>Render queue for this full screen effect. Do not change this at runtime, set it in the inspector once before play.</summary>
        [Tooltip("Render queue for this full screen effect. Do not change this at runtime, set it in the inspector once before play.")]
        public CameraEvent RenderQueue = CameraEvent.BeforeForwardAlpha;

        /// <summary>Material for rendering/creating the effect</summary>
        [Tooltip("Material for rendering/creating the effect")]
        public Material Material;
        private Material clonedMaterial;

        /// <summary>Material for blurring</summary>
        [Tooltip("Material for blurring")]
        public Material BlurMaterial;

        /// <summary>Material for bilateral blurring</summary>
        [Tooltip("Material for bilateral blurring")]
        public Material BilateralBlurMaterial;

        /// <summary>Material to render the final pass if needed, not all setups will need this but it should be set anyway</summary>
        [Tooltip("Material to render the final pass if needed, not all setups will need this but it should be set anyway")]
        public Material BlitMaterial;

        /// <summary>Material to down-sample depth buffer if needed, can be null</summary>
        [Tooltip("Material to down-sample depth buffer if needed, can be null")]
        public Material DepthDownsampleMaterial;

        /// <summary>Material for temporal reprojection</summary>
        [Tooltip("Material for temporal reprojection")]
        public Material TemporalReprojectionMaterial;

        /// <summary>Temporal reprojection</summary>
        [Tooltip("Temporal reprojection")]
        public WeatherMakerTemporalReprojectionSize TemporalReprojection = WeatherMakerTemporalReprojectionSize.None;

        /// <summary>Downsample scale for Material</summary>
        [Tooltip("Downsample scale for Material")]
        public WeatherMakerDownsampleScale DownsampleScale = WeatherMakerDownsampleScale.FullResolution;

        /// <summary>Downsample scale for render buffer sampling, or 0 to not sample the render buffer.</summary>
        [Tooltip("Downsample scale for render buffer sampling, or 0 to not sample the render buffer.")]
        public WeatherMakerDownsampleScale DownsampleRenderBufferScale = WeatherMakerDownsampleScale.Disabled;

        /// <summary>Downsample scale for post process</summary>
        [Tooltip("Downsample scale for post process")]
        public WeatherMakerDownsampleScale DownsampleScalePostProcess = WeatherMakerDownsampleScale.QuarterResolution;

        /// <summary>Blur shader type</summary>
        [Tooltip("Blur shader type")]
        public BlurShaderType BlurShaderType = BlurShaderType.None;

        /// <summary>Source blend mode</summary>
        [Tooltip("Source blend mode")]
        public BlendMode SourceBlendMode = BlendMode.One;

        /// <summary>Dest blend mode</summary>
        [Tooltip("Dest blend mode")]
        public BlendMode DestBlendMode = BlendMode.OneMinusSrcAlpha;

        /// <summary>ZTest</summary>
        [Tooltip("ZTest")]
        public UnityEngine.Rendering.CompareFunction ZTest = CompareFunction.Always;

        /// <summary>
        /// The name for the command buffer that will be created for this effect. This should be unique for your project.
        /// </summary>
        public string CommandBufferName { get; set; }

        /// <summary>
        /// Whether the effect is enabled. The effect can be disabled to prevent command buffers from being created.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Action to fire when Material properties should be updated
        /// </summary>
        public System.Action<WeatherMakerCommandBuffer> UpdateMaterialProperties { get; set; }

        private System.Func<WeatherMakerCommandBuffer, bool> preSetupCommandBuffer;
        private System.Action<WeatherMakerCommandBuffer, RenderTargetIdentifier, RenderTextureDescriptor> postProcessCommandBuffer;
        private System.Action<WeatherMakerCommandBuffer> postSetupCommandBuffer;

        private static Mesh quad;
        private static Material commandBufferBlitMaterial;

        /// <summary>
        /// Quad for command buffer DrawQuad
        /// </summary>
        public static Mesh Quad
        {
            get
            {
                if (quad != null)
                {
                    return quad;
                }
                Vector2[] uvs = new Vector2[4]
                {
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(0.0f, 1.0f)
                };
                Vector3[] vertices = new Vector3[4]
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f, 1.0f, 0.0f)
                };
                quad = new Mesh
                {
                    vertices = vertices,
                    uv = uvs,
                    triangles = new int[] { 0, 1, 2, 1, 0, 3 }
                };
                quad.RecalculateBounds();
                return quad;
            }
        }

        /// <summary>
        /// Material to use for draw mesh commands for full screen quads for command buffers
        /// </summary>
        public static Material CommandBufferBlitMaterial
        {
            get
            {
                if (commandBufferBlitMaterial != null)
                {
                    return commandBufferBlitMaterial;
                }
                commandBufferBlitMaterial = Resources.Load("WeatherMakerBlitMaterial") as Material;
                return commandBufferBlitMaterial;
            }
        }

        private readonly List<KeyValuePair<Camera, WeatherMakerCommandBuffer>> weatherMakerCommandBuffers = new List<KeyValuePair<Camera, WeatherMakerCommandBuffer>>();

        /// <summary>
        /// Get render texture descriptor
        /// </summary>
        /// <param name="scale">Scale</param>
        /// <param name="mod">Mod width and height rounding</param>
        /// <param name="scale2">Second scale after mod</param>
        /// <param name="format">Format</param>
        /// <param name="depth">Depth</param>
        /// <param name="camera">Camera</param>
        /// <returns>RenderTextureDescriptor</returns>
        public static RenderTextureDescriptor GetRenderTextureDescriptor(int scale, int mod, int scale2, RenderTextureFormat format, int depth = 0, Camera camera = null)
        {
            scale = Mathf.Clamp(scale, 1, 16);
            RenderTextureDescriptor desc;
            if (WeatherMakerScript.HasXRDevice() && (camera == null || camera.stereoEnabled))
            {
                desc = UnityEngine.XR.XRSettings.eyeTextureDesc;
            }
            else if (camera == null)
            {
                desc = new RenderTextureDescriptor(Screen.width, Screen.height, format, depth);
            }
            else
            {
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, format, depth);
            }
            desc.depthBufferBits = depth;
            desc.width = desc.width / scale;
            desc.height = desc.height / scale;
            desc.autoGenerateMips = false;
            desc.useMipMap = false;
            if (mod > 0)
            {
                while (desc.width % mod != 0) { desc.width++; }
                while (desc.height % mod != 0) { desc.height++; }
            }
            desc.width /= scale2;
            desc.height /= scale2;
            return desc;
        }

        internal static RenderTexture CreateRenderTexture(RenderTextureDescriptor desc, bool mipMap, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool temporary = false)
        {
            desc.autoGenerateMips = mipMap;
            desc.useMipMap = mipMap;
            RenderTexture tex = (temporary ? RenderTexture.GetTemporary(desc) : new RenderTexture(desc));
            tex.filterMode = filterMode;
            tex.wrapMode = wrapMode;
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.Create();
            return tex;
        }

        internal static void DestroyRenderTexture(ref RenderTexture tex)
        {
            if (tex != null)
            {
                try
                {
                    if (tex == RenderTexture.active)
                    {
                        RenderTexture.active = null;
                    }
                    tex.Release();
                }
                catch
                {
                }
                try
                {
                    GameObject.DestroyImmediate(tex);
                }
                catch
                {
                }
                tex = null;
            }
        }

        internal static RenderTexture DestroyRenderTexture(RenderTexture tex)
        {
            DestroyRenderTexture(ref tex);
            return tex;
        }

        internal static void ReleaseCommandBuffer(ref CommandBuffer commandBuffer)
        {
            if (commandBuffer != null)
            {
                try
                {
                    commandBuffer.Clear();
                    commandBuffer.Release();
                    commandBuffer = null;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Get the default render texture format
        /// </summary>
        /// <returns>Default render texture format</returns>
        public static RenderTextureFormat DefaultRenderTextureFormat()
        {
            // bug: RenderTextureFormat.Default has no alpha channel on mobile platforms...
            if (Application.isMobilePlatform)
            {
                return RenderTextureFormat.ARGB32;
            }
            return RenderTextureFormat.ARGBHalf;
        }

        /// <summary>
        /// CameraTarget identifier
        /// </summary>
        /// <returns>RenderTargetIdentifier</returns>
        public static RenderTargetIdentifier CameraTargetIdentifier()
        {

#if UNITY_URP

            return WMS._CameraColorAttachmentA;

#else

            return BuiltinRenderTextureType.CameraTarget;

#endif

        }

        private void AttachPostProcessing(CommandBuffer commandBuffer, Material material, int w, int h, RenderTextureFormat defaultFormat,
            RenderTargetIdentifier renderedImageId, WeatherMakerTemporalReprojectionState reprojState, Camera camera,
            ref int postSourceId, ref RenderTargetIdentifier postSource, float currentDownsampleScale)
        {
            if (material.passCount > 2)
            {
                if (renderedImageId == CameraTargetIdentifier())
                {
                    Debug.LogError("Weather Maker command buffer post processing cannot blit directly to camera target");
                }
                else
                {
                    int downsampleMain = WMS._MainTex2;
                    float postScale = (float)Mathf.Max((int)DownsampleScale, (int)DownsampleScalePostProcess);
                    commandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, postScale);
                    SetDownsampleKeyword(commandBuffer, (int)postScale);
                    postSourceId = WMS._MainTex4;
                    postSource = new RenderTargetIdentifier(WMS._MainTex4);
                    commandBuffer.GetTemporaryRT(postSourceId, GetRenderTextureDescriptor((int)postScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                    if (reprojState == null)
                    {
                        if ((int)DownsampleScale < (int)DownsampleScalePostProcess)
                        {
                            // downsample main texture
                            commandBuffer.GetTemporaryRT(downsampleMain, GetRenderTextureDescriptor((int)postScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                            commandBuffer.GetTemporaryRT(postSourceId, GetRenderTextureDescriptor((int)postScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                            commandBuffer.SetGlobalTexture(WMS._MainTex2, downsampleMain);
                            WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, downsampleMain);
                            WeatherMakerFullScreenEffect.Blit(commandBuffer, downsampleMain, postSourceId, material, 2);
                            commandBuffer.ReleaseTemporaryRT(downsampleMain);
                        }
                        else
                        {
                            commandBuffer.SetGlobalTexture(WMS._MainTex2, renderedImageId);
                            WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, postSourceId, material, 2);
                        }
                    }
                    else if ((int)DownsampleScale * (int)reprojState.ReprojectionSize < (int)DownsampleScalePostProcess)
                    {
                        commandBuffer.GetTemporaryRT(downsampleMain, GetRenderTextureDescriptor((int)postScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                        commandBuffer.GetTemporaryRT(postSourceId, GetRenderTextureDescriptor((int)postScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                        commandBuffer.SetGlobalTexture(WMS._MainTex2, downsampleMain);
                        WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.PrevFrameTexture, downsampleMain);
                        WeatherMakerFullScreenEffect.Blit(commandBuffer, downsampleMain, postSourceId, material, 2);
                        commandBuffer.ReleaseTemporaryRT(downsampleMain);
                    }
                    else
                    {
                        commandBuffer.SetGlobalTexture(WMS._MainTex2, reprojState.PrevFrameTexture);
                        WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.PrevFrameTexture, postSourceId, material, 2);
                    }

                    // restore downsample scale
                    commandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, currentDownsampleScale);
                    SetDownsampleKeyword(commandBuffer, (int)currentDownsampleScale);
                    commandBuffer.SetGlobalTexture(WMS._MainTex4, postSourceId);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, postSourceId, renderedImageId, material, 3); // pass index 3 is post process blit pass
                    commandBuffer.ReleaseTemporaryRT(postSourceId);
                }
            }
        }

        private void AttachTemporalReprojection(CommandBuffer commandBuffer, WeatherMakerTemporalReprojectionState reprojState,
            RenderTargetIdentifier renderedImageId, Material material, int scale, RenderTextureFormat defaultFormat)
        {
            if (reprojState.NeedsFirstFrameHandling)
            {
                // do a more expensive render once to get the image looking nice, only happens once on first frame
                WeatherMakerFullScreenEffect.Blit(commandBuffer, CameraTargetIdentifier(), reprojState.PrevFrameTexture, material, 0);

                // copy to sub frame so it's ready for next pass
                WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.PrevFrameTexture, reprojState.SubFrameTexture);

                // copy to final image
                WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.PrevFrameTexture, renderedImageId);
            }
            else if (reprojState.IntegratedTemporalReprojection)
            {
                // render fast pass with offset temporal reprojection so it is available in the full pass
                commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionEnabled, 1.0f);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_VertexShaderProjection, new Vector4(reprojState.TemporalOffsetX * 2.0f, reprojState.TemporalOffsetY * 2.0f, 0.0f, 0.0f));
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_FragmentShader, new Vector4(reprojState.TemporalOffsetX, reprojState.TemporalOffsetY, 0.0f, 0.0f));
                WeatherMakerFullScreenEffect.Blit(commandBuffer, CameraTargetIdentifier(), reprojState.SubFrameTexture, material, 0);

                // perform full pass with full integrated temporal reprojection
                commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionEnabled, 2.0f);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_VertexShaderProjection, Vector4.zero);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_FragmentShader, Vector4.zero);
                WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.SubFrameTexture, renderedImageId, material, 0);
                WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, reprojState.PrevFrameTexture);
            }
            else
            {
                // render to sub frame (fast, this is a small texture)
                commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionEnabled, 1.0f);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_VertexShaderProjection, new Vector4(reprojState.TemporalOffsetX * 2.0f, reprojState.TemporalOffsetY * 2.0f, 0.0f, 0.0f));
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_FragmentShader, new Vector4(reprojState.TemporalOffsetX, reprojState.TemporalOffsetY, 0.0f, 0.0f));
                WeatherMakerFullScreenEffect.Blit(commandBuffer, CameraTargetIdentifier(), reprojState.SubFrameTexture, material, 0);

                // combine sub frame and prev frame to final image
                WeatherMakerFullScreenEffect.Blit(commandBuffer, reprojState.PrevFrameTexture, renderedImageId, reprojState.TemporalReprojectionMaterial, 0);

                // copy combined to previous frame for re-use next frame
                WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, reprojState.PrevFrameTexture);
            }

            commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionEnabled, 0.0f);
            commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_VertexShaderProjection, Vector4.zero);
            commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_FragmentShader, Vector4.zero);
        }

#if UNITY_URP

        internal static UnityEngine.Rendering.Universal.ScriptableRenderPass urpPass;

#endif

        internal static void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat = null, int pass = 0)
        {
            mat = (mat == null ? CommandBufferBlitMaterial : mat);

            if (pass >= mat.passCount)
            {
                return;
                //Debug.LogError("Pass is greater than pass count on material " + mat.name);
            }

#if UNITY_URP

            /*cmd.SetRenderTarget(dest);
            if (source != 0)
            {
                cmd.SetGlobalTexture(WMS._MainTex, source);
            }
            cmd.DrawMesh(UnityEngine.Rendering.Universal.RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat, 0, pass);*/
            if (urpPass == null)
            {
                cmd.Blit(source == 0 ? (Texture2D)null : source, dest, mat, pass);
            }
            else
            {
                urpPass.Blit(cmd, source == 0 ? (Texture2D)null : source, dest, mat, pass);
            }

#else

            cmd.Blit(source == 0 ? (Texture2D)null : source, dest, mat, pass);

#endif

        }

        internal static void ApplyPostProcessing(CommandBuffer commandBuffer,
            Material[] materials,
            RenderTargetIdentifier source,
            RenderTextureDescriptor sourceDesc)
        {
            if (materials == null || materials.Length == 0)
            {
                return;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                Material m = materials[i];
                if (m != null)
                {
                    commandBuffer.GetTemporaryRT(WMS._MainTex2, sourceDesc);
                    commandBuffer.SetGlobalTexture(WMS._MainTex, source);
                    Blit(commandBuffer, source, WMS._MainTex2, m); // copy to temp texture with post processor
                    Blit(commandBuffer, WMS._MainTex2, source); // copy back to source
                    commandBuffer.ReleaseTemporaryRT(WMS._MainTex2);
                }
            }
        }

        /// <summary>
        /// Bilateral blur
        /// </summary>
        /// <param name="commandBuffer">Command buffer</param>
        /// <param name="blurMaterial">Blur material</param>
        /// <param name="sourceIdentifier">Texture to bilateral blur out of</param>
        /// <param name="targetIdentifier">Texture to bilateral blur to (can be source)</param>
        /// <param name="format">Texture format</param>
        /// <param name="downsampleScale">Downsample scale</param>
        /// <param name="camera">Camera</param>
        internal static void AttachBilateralBlur(CommandBuffer commandBuffer, Material blurMaterial, RenderTargetIdentifier sourceIdentifier,
            RenderTargetIdentifier targetIdentifier, RenderTextureFormat format, WeatherMakerDownsampleScale downsampleScale, Camera camera)
        {
            int scale = (int)downsampleScale;
            RenderTextureDescriptor desc = GetRenderTextureDescriptor(scale, 0, 1, format, 0, camera);
            commandBuffer.GetTemporaryRT(WMS._MainTex6, desc, FilterMode.Bilinear);
            int blurPass;
            int blitPass;
            switch (scale)
            {
                default:
                    blurPass = 0;
                    blitPass = 8;
                    break;

                case 2:
                    blurPass = 2;
                    blitPass = 9;
                    break;

                case 4:
                    blurPass = 4;
                    blitPass = 10;
                    break;

                case 8:
                    blurPass = 6;
                    blitPass = 11;
                    break;
            }

            // horizontal blur
            WeatherMakerFullScreenEffect.Blit(commandBuffer, sourceIdentifier, WMS._MainTex6, blurMaterial, blurPass);
            //commandBuffer.DrawQuad(sourceIdentifier, temporaryBufferId, blurMaterial, blurPass);

            // vertical blur
            WeatherMakerFullScreenEffect.Blit(commandBuffer, WMS._MainTex6, sourceIdentifier, blurMaterial, blurPass + 1);
            //commandBuffer.DrawQuad(temporaryBufferId, sourceIdentifier, blurMaterial, blurPass + 1);

            if (sourceIdentifier != targetIdentifier)
            {
                // upsample
                WeatherMakerFullScreenEffect.Blit(commandBuffer, sourceIdentifier, targetIdentifier, blurMaterial, blitPass);
                //commandBuffer.DrawQuad(sourceIdentifier, targetIdentifier, blurMaterial, blitPass);
            }

            // cleanup
            commandBuffer.ReleaseTemporaryRT(WMS._MainTex6);
        }

        internal void AttachBlurBlit(CommandBuffer commandBuffer, RenderTargetIdentifier renderedImageId,
            RenderTargetIdentifier finalTargetId, Material material, BlurShaderType blur, RenderTextureFormat defaultFormat,
            WeatherMakerTemporalReprojectionState reprojState, Camera camera)
        {
            // set blend mode for blitting to camera target
            commandBuffer.SetGlobalFloat(WMS._SrcBlendMode, (float)SourceBlendMode);
            commandBuffer.SetGlobalFloat(WMS._DstBlendMode, (float)DestBlendMode);

            // blur if requested
            if (blur == BlurShaderType.None)
            {
                // blit texture directly on top of camera without blur, depth aware alpha blend
                WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, finalTargetId, BlitMaterial);
            }
            else
            {
                // blur texture directly on to camera target, depth and alpha aware blur, alpha blend
                if (blur == BlurShaderType.GaussianBlur7)
                {
                    commandBuffer.SetGlobalFloat(WMS._Blur7, 1.0f);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, finalTargetId, BlurMaterial);
                }
                else if (blur == BlurShaderType.GaussianBlur17)
                {
                    commandBuffer.SetGlobalFloat(WMS._Blur7, 0.0f);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, renderedImageId, finalTargetId, BlurMaterial);
                }
                else
                {
                    AttachBilateralBlur(commandBuffer, BilateralBlurMaterial, renderedImageId, finalTargetId, defaultFormat, DownsampleScale, camera);
                }
            }
        }

        internal static void AttachDepthDownsample(CommandBuffer commandBuffer, WeatherMakerTemporalReprojectionState temporalState, Material depthMaterial, Camera camera)
        {
            RenderTargetIdentifier cameraId = CameraTargetIdentifier();
            int mod = (temporalState == null || temporalState.ReprojectionSize < 2 ? 1 : temporalState.ReprojectionSize);
            RenderTextureDescriptor desc = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(1, 1, 1, RenderTextureFormat.RFloat, 0, camera);
            RenderTextureDescriptor desc2 = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(2, mod, 1, RenderTextureFormat.RFloat, 0, camera);
            RenderTextureDescriptor desc4 = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(4, mod, 1, RenderTextureFormat.RFloat, 0, camera);
            RenderTextureDescriptor desc8 = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(8, mod, 1, RenderTextureFormat.RFloat, 0, camera);

            RenderTargetIdentifier descId2 = new RenderTargetIdentifier(WMS._CameraDepthTextureHalf);
            RenderTargetIdentifier descId4 = new RenderTargetIdentifier(WMS._CameraDepthTextureQuarter);
            RenderTargetIdentifier descId8 = new RenderTargetIdentifier(WMS._CameraDepthTextureEighth);

            commandBuffer.GetTemporaryRT(WMS._CameraDepthTextureHalf, desc2, FilterMode.Point);
            commandBuffer.GetTemporaryRT(WMS._CameraDepthTextureQuarter, desc4, FilterMode.Point);
            commandBuffer.GetTemporaryRT(WMS._CameraDepthTextureEighth, desc8, FilterMode.Point);

            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeSource, new Vector4(1.0f / desc.width, 1.0f / desc.height, desc.width, desc.height));
            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeDest, new Vector4(1.0f / desc2.width, 1.0f / desc2.height, desc2.width, desc2.height));
            WeatherMakerFullScreenEffect.Blit(commandBuffer, cameraId, descId2, depthMaterial, 1);

            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeSource, new Vector4(1.0f / desc2.width, 1.0f / desc2.height, desc2.width, desc2.height));
            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeDest, new Vector4(1.0f / desc4.width, 1.0f / desc4.height, desc4.width, desc4.height));
            WeatherMakerFullScreenEffect.Blit(commandBuffer, cameraId, descId4, depthMaterial, 2);

            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeSource, new Vector4(1.0f / desc4.width, 1.0f / desc4.height, desc4.width, desc4.height));
            commandBuffer.SetGlobalVector(WMS._DepthTexelSizeDest, new Vector4(1.0f / desc8.width, 1.0f / desc8.height, desc8.width, desc8.height));
            WeatherMakerFullScreenEffect.Blit(commandBuffer, cameraId, descId8, depthMaterial, 3);
            commandBuffer.SetRenderTarget(CameraTargetIdentifier(), 0, CubemapFace.Unknown, -1);
        }

        internal static void AttachDepthDownsampleRelease(CommandBuffer commandBuffer)
        {
            commandBuffer.ReleaseTemporaryRT(WMS._CameraDepthTextureHalf);
            commandBuffer.ReleaseTemporaryRT(WMS._CameraDepthTextureQuarter);
            commandBuffer.ReleaseTemporaryRT(WMS._CameraDepthTextureEighth);
        }

        private static readonly Matrix4x4[] instancedMatrixes = new Matrix4x4[2];
        internal static void DrawMesh(CommandBuffer cmdBuffer, Vector3 objPos, Quaternion objRot, Vector3 objScale, Mesh mesh, Material mat, int pass = 0)
        {
            cmdBuffer.DrawMesh(mesh, Matrix4x4.TRS(objPos, objRot, objScale), mat, 0, pass, null);
        }

        internal static void DrawMesh(CommandBuffer cmdBuffer, Transform transform, Mesh mesh, Material mat, int pass = 0)
        {
            cmdBuffer.DrawMesh(mesh, transform.localToWorldMatrix, mat, 0, pass, null);
        }

        private WeatherMakerCommandBuffer GetOrCreateWeatherMakerCommandBuffer(Camera camera)
        {
            foreach (KeyValuePair<Camera, WeatherMakerCommandBuffer> kv in weatherMakerCommandBuffers)
            {
                if (kv.Key == camera)
                {
                    return kv.Value;
                }
            }
            WeatherMakerCommandBuffer cmd = new WeatherMakerCommandBuffer
            {
                Camera = camera,
                CameraType = WeatherMakerScript.GetCameraType(camera),
                CommandBuffer = new CommandBuffer { name = CommandBufferName }
            };
            weatherMakerCommandBuffers.Add(new KeyValuePair<Camera, WeatherMakerCommandBuffer>(camera, cmd));
            return cmd;
        }

        private void SetDownsampleKeyword(CommandBuffer commandBuffer, int scale)
        {
            string keyword = downsampleKeyword[scale];

            if (clonedMaterial != null)
            {
                clonedMaterial.DisableKeyword(downsampleKeyword[2]);
                clonedMaterial.DisableKeyword(downsampleKeyword[4]);
                clonedMaterial.DisableKeyword(downsampleKeyword[8]);
                if (keyword.Length != 0)
                {
                    clonedMaterial.EnableKeyword(downsampleKeyword[scale]);
                }
            }
        }

        private WeatherMakerCommandBuffer CreateCommandBuffer(Camera camera,
            WeatherMakerTemporalReprojectionState reprojState,
            WeatherMakerDownsampleScale downsampleScale,
            System.Func<WeatherMakerCommandBuffer, bool> preSetupCommandBuffer,
            System.Action<WeatherMakerCommandBuffer, RenderTargetIdentifier, RenderTextureDescriptor> postProcessCommandBuffer,
            System.Action<WeatherMakerCommandBuffer> postSetupCommandBuffer)
        {
            if (WeatherMakerScript.Instance == null)
            {
                Debug.LogError("Cannot create command buffer, WeatherMakerScript.Instance is null");
                return null;
            }

            // Debug.Log("Creating command buffer " + CommandBufferName);

            WeatherMakerCommandBuffer weatherMakerCommandBuffer = GetOrCreateWeatherMakerCommandBuffer(camera);
            if (weatherMakerCommandBuffer != null && weatherMakerCommandBuffer.CommandBuffer != null)
            {
                camera.RemoveCommandBuffer(RenderQueue, weatherMakerCommandBuffer.CommandBuffer);
                weatherMakerCommandBuffer.CommandBuffer.Clear();
            }

            RenderTextureFormat defaultFormat = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
            CommandBuffer commandBuffer = weatherMakerCommandBuffer.CommandBuffer;
            int postSourceId = -1;
            RenderTargetIdentifier postSource = postSourceId;
            WeatherMakerCameraType cameraType = weatherMakerCommandBuffer.CameraType;

            // setup reprojection state
            if (reprojState != null)
            {
                reprojState.CommandBuffer = weatherMakerCommandBuffer;
                reprojState.PreRenderFrame(camera, WeatherMakerCommandBufferManagerScript.BaseCamera, commandBuffer);
                if (reprojState.NeedsFirstFrameHandling)
                {
                    commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV, Vector4.zero);
                }
                else
                {
                    float xo = reprojState.TemporalOffsetX;
                    float yo = reprojState.TemporalOffsetY;
                    commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV, new Vector4(xo, yo, 1.0f, 1.0f));
                }
            }

            int scale = (int)downsampleScale;
            commandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, (float)scale);
            SetDownsampleKeyword(commandBuffer, scale);

            // downsample depth if we need it
            if (scale > 1)
            {
                AttachDepthDownsample(commandBuffer, reprojState, DepthDownsampleMaterial, camera);
            }

            bool result = true;
            if (preSetupCommandBuffer != null)
            {
                result = preSetupCommandBuffer.Invoke(weatherMakerCommandBuffer);
            }
            if (Enabled && result)
            {
                commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionEnabled, 0.0f);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_VertexShaderProjection, Vector4.zero);
                commandBuffer.SetGlobalVector(WMS._WeatherMakerTemporalUV_FragmentShader, Vector4.zero);

                if (DownsampleRenderBufferScale > WeatherMakerDownsampleScale.FullResolution)
                {
                    // render camera target to texture, performing separate down-sampling
                    commandBuffer.GetTemporaryRT(WMS._MainTex5, GetRenderTextureDescriptor((int)DownsampleRenderBufferScale, 0, 1, defaultFormat, 0, camera), FilterMode.Bilinear);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, CameraTargetIdentifier(), WMS._MainTex5);
                }

                commandBuffer.SetGlobalFloat(WMS._ZTest, (float)ZTest);

                // if no blur, no downsample, no temporal reprojection and no post processing (pass count > 2) then we can just draw directly to camera target
                if (BlurShaderType == BlurShaderType.None &&
                    DownsampleScale == WeatherMakerDownsampleScale.FullResolution &&
                    reprojState == null &&
                    clonedMaterial.passCount < 3 &&
                    postProcessCommandBuffer == null)
                {
                    commandBuffer.SetGlobalFloat(WMS._SrcBlendMode, (float)SourceBlendMode);
                    commandBuffer.SetGlobalFloat(WMS._DstBlendMode, (float)DestBlendMode);
                    WeatherMakerFullScreenEffect.Blit(commandBuffer, CameraTargetIdentifier(), CameraTargetIdentifier(), clonedMaterial, 0);
                }
                else
                {
                    // render to texture, using current image target as input _MainTex, no blend
                    // alpha or blue will use _MainTex to render the final result
                    int renderTargetRenderedImageId = WMS._MainTex3;
                    RenderTargetIdentifier renderTargetRenderedImage = new RenderTargetIdentifier(renderTargetRenderedImageId);
                    BlurShaderType blur = BlurShaderType;

                    if (cameraType != WeatherMakerCameraType.Normal)
                    {
                        if (BlurMaterial.passCount > 3)
                        {
                            blur = BlurShaderType.Bilateral;
                        }
                        else
                        {
                            blur = BlurShaderType.GaussianBlur17;
                        }
                    }

                    int mod = (reprojState == null ? 0 : reprojState.ReprojectionSize);
                    RenderTextureDescriptor renderedTextureDesc = GetRenderTextureDescriptor(scale, mod, 1, defaultFormat, 0, camera);
                    commandBuffer.GetTemporaryRT(renderTargetRenderedImageId, renderedTextureDesc, FilterMode.Bilinear);

                    // set blend mode for blitting to render texture
                    commandBuffer.SetGlobalFloat(WMS._SrcBlendMode, (float)BlendMode.One);
                    commandBuffer.SetGlobalFloat(WMS._DstBlendMode, (float)BlendMode.Zero);

                    if (reprojState == null || reprojState.TemporalReprojectionMaterial == null)
                    {
                        // render to final destination
                        WeatherMakerFullScreenEffect.Blit(commandBuffer, renderTargetRenderedImage, renderTargetRenderedImage, clonedMaterial, 0);
                    }
                    else
                    {
                        AttachTemporalReprojection(commandBuffer, reprojState, renderTargetRenderedImage, clonedMaterial, scale, defaultFormat);
                    }

                    if (postProcessCommandBuffer != null)
                    {
                        postProcessCommandBuffer.Invoke(weatherMakerCommandBuffer, renderTargetRenderedImage, renderedTextureDesc);
                    }

                    if (cameraType == WeatherMakerCameraType.Normal && DownsampleScalePostProcess != WeatherMakerDownsampleScale.Disabled)
                    {
                        AttachPostProcessing(commandBuffer, clonedMaterial, 0, 0, defaultFormat, renderTargetRenderedImage, reprojState, camera, ref postSourceId, ref postSource, scale);
                        //AttachPostProcessing(commandBuffer, material, w, h, defaultFormat, renderTargetRenderedImageId, reprojState, ref postSourceId, ref postSource);
                    }

                    AttachBlurBlit(commandBuffer, renderTargetRenderedImage, CameraTargetIdentifier(), clonedMaterial, blur, defaultFormat, reprojState, camera);

                    // cleanup
                    commandBuffer.ReleaseTemporaryRT(renderTargetRenderedImageId);

                    if (scale > 1)
                    {
                        AttachDepthDownsampleRelease(commandBuffer);
                    }

                    commandBuffer.SetGlobalTexture(WMS._MainTex2, new RenderTargetIdentifier(BuiltinRenderTextureType.None));
                }

                if (DownsampleRenderBufferScale > WeatherMakerDownsampleScale.FullResolution)
                {
                    // cleanup
                    commandBuffer.ReleaseTemporaryRT(WMS._MainTex5);
                }

                if (postSetupCommandBuffer != null)
                {
                    postSetupCommandBuffer.Invoke(weatherMakerCommandBuffer);
                }
            }

            // reset downsample scale
            commandBuffer.SetGlobalFloat(WMS._WeatherMakerDownsampleScale, 1.0f);
            weatherMakerCommandBuffer.Material = clonedMaterial;
            weatherMakerCommandBuffer.ReprojectionState = reprojState;
            weatherMakerCommandBuffer.RenderQueue = RenderQueue;
            camera.AddCommandBuffer(RenderQueue, commandBuffer);
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.AddCommandBuffer(weatherMakerCommandBuffer);
            }
            return weatherMakerCommandBuffer;
        }

        private void CloneMaterial(ref Material materialProperty, ref Material cloneMaterial, Material material)
        {
            // setup material, if playing we want to clone it to avoid modifying the material
            if (materialProperty != material || (Application.isPlaying && clonedMaterial == null))
            {
                materialProperty = material;
                if (Application.isPlaying)
                {
                    if (clonedMaterial != null)
                    {
                        GameObject.DestroyImmediate(clonedMaterial);
                    }
                    clonedMaterial = new Material(material);
                    clonedMaterial.name += " (Clone)";
                }
                else
                {
                    clonedMaterial = Material;
                }
            }
        }

        /// <summary>
        /// Call from LateUpdate in script
        /// </summary>
        public void SetupEffect
        (
            Material material,
            Material blitMaterial,
            Material blurMaterial,
            Material bilateralBlurMaterial,
            BlurShaderType blurShaderType,
            WeatherMakerDownsampleScale downSampleScale,
            WeatherMakerDownsampleScale downsampleRenderBufferScale,
            WeatherMakerDownsampleScale downsampleScalePostProcess,
            Material temporalReprojectionMaterial,
            WeatherMakerTemporalReprojectionSize temporalReprojection,
            System.Action<WeatherMakerCommandBuffer> updateMaterialProperties,
            bool enabled,
            System.Func<WeatherMakerCommandBuffer, bool> preSetupCommandBuffer = null,
            System.Action<WeatherMakerCommandBuffer, RenderTargetIdentifier, RenderTextureDescriptor> postProcessCommandBuffer = null,
            System.Action<WeatherMakerCommandBuffer> postSetupCommandBuffer = null
        )
        {
            Enabled = enabled;
            if (Enabled || preSetupCommandBuffer != null || postSetupCommandBuffer != null)
            {
                CloneMaterial(ref Material, ref clonedMaterial, material);
                BlitMaterial = blitMaterial;
                BlurMaterial = blurMaterial;
                BilateralBlurMaterial = bilateralBlurMaterial;
                DepthDownsampleMaterial = WeatherMakerCommandBufferManagerScript.Instance.DownsampleDepthMaterial;
                TemporalReprojectionMaterial = temporalReprojectionMaterial;
                DownsampleScalePostProcess = downsampleScalePostProcess;
                BlurShaderType = blurShaderType;
                DownsampleScale = (downSampleScale == WeatherMakerDownsampleScale.Disabled ? WeatherMakerDownsampleScale.FullResolution : downSampleScale);
                DownsampleRenderBufferScale = downsampleRenderBufferScale;
                TemporalReprojection = temporalReprojection;
                UpdateMaterialProperties = updateMaterialProperties;
                this.preSetupCommandBuffer = preSetupCommandBuffer;
                this.postProcessCommandBuffer = postProcessCommandBuffer;
                this.postSetupCommandBuffer = postSetupCommandBuffer;

                // cleanup dead cameras
                for (int i = weatherMakerCommandBuffers.Count - 1; i >= 0; i--)
                {
                    if (weatherMakerCommandBuffers[i].Key == null)
                    {
                        if (weatherMakerCommandBuffers[i].Value.CommandBuffer != null)
                        {
                            weatherMakerCommandBuffers[i].Value.CommandBuffer.Clear();
                            weatherMakerCommandBuffers[i].Value.CommandBuffer.Release();
                        }
                        weatherMakerCommandBuffers.RemoveAt(i);
                    }
                }
            }
            else
            {
                Dispose();
            }
        }

        /// <summary>
        /// Update the full screen effect, usually called from OnPreCull
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="integratedTemporalReprojection">Whether to use integrated temporal reprojection (if temporal reprojection is enabled)</param>
        public void PreCullCamera(Camera camera, bool integratedTemporalReprojection = true)
        {
            //integratedTemporalReprojection = false;
            if (WeatherMakerScript.Instance != null && WeatherMakerScript.Instance.CommandBufferManager != null && (Enabled || preSetupCommandBuffer != null) &&
                Material != null && clonedMaterial != null && camera != null)
            {
                WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
                if (cameraType == WeatherMakerCameraType.Other)
                {
                    return;
                }

                WeatherMakerTemporalReprojectionState reprojState = null;
                WeatherMakerDownsampleScale downsampleScale = DownsampleScale;
                if (cameraType != WeatherMakerCameraType.Normal)
                {
                    if (camera.pixelWidth > 2000)
                    {
                        downsampleScale = WeatherMakerDownsampleScale.QuarterResolution;
                    }
                    else if (camera.pixelWidth > 1000)
                    {
                        downsampleScale = WeatherMakerDownsampleScale.HalfResolution;
                    }
                }

                if (Enabled)
                {
                    // setup temporal reprojection state
                    reprojState = temporalStates.Find(b => b.Camera == camera);

                    if (reprojState == null)
                    {
                        // temporal reprojection is not currently possible with cubemap
                        if (TemporalReprojection != WeatherMakerTemporalReprojectionSize.None && TemporalReprojectionMaterial != null &&
                            (cameraType == WeatherMakerCameraType.Normal || cameraType == WeatherMakerCameraType.Reflection))
                        {
                            reprojState = new WeatherMakerTemporalReprojectionState(camera, TemporalReprojectionMaterial, integratedTemporalReprojection);
                            temporalStates.Add(reprojState);
                        }
                    }
                    else if (TemporalReprojection == WeatherMakerTemporalReprojectionSize.None || TemporalReprojectionMaterial == null)
                    {
                        reprojState.Dispose();
                        temporalStates.Remove(reprojState);
                        reprojState = null;
                    }

                    if (reprojState != null)
                    {
                        WeatherMakerTemporalReprojectionSize reprojSize = (cameraType == WeatherMakerCameraType.Normal ? TemporalReprojection : WeatherMakerTemporalReprojectionSize.FourByFour);
                        reprojState.PreCullFrame(camera, downsampleScale, reprojSize);
                    }
                }
            }
        }

        /// <summary>
        /// Pre render event
        /// </summary>
        /// <param name="camera">Camera</param>
        public void PreRenderCamera(Camera camera)
        {
            WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
            if (cameraType == WeatherMakerCameraType.Other)
            {
                return;
            }

            WeatherMakerDownsampleScale downsampleScale = DownsampleScale;
            if (cameraType != WeatherMakerCameraType.Normal)
            {
                if (camera.pixelWidth > 2000)
                {
                    downsampleScale = WeatherMakerDownsampleScale.QuarterResolution;
                }
                else if (camera.pixelWidth > 1000)
                {
                    downsampleScale = WeatherMakerDownsampleScale.HalfResolution;
                }
            }

            WeatherMakerTemporalReprojectionState reprojState = temporalStates.Find(b => b.Camera == camera);
            WeatherMakerCommandBuffer cmdBuffer = CreateCommandBuffer(camera,
                reprojState,
                downsampleScale,
                preSetupCommandBuffer,
                postProcessCommandBuffer,
                postSetupCommandBuffer);
            if (UpdateMaterialProperties != null)
            {
                UpdateMaterialProperties(cmdBuffer);
            }
        }

        /// <summary>
        /// Post render event
        /// </summary>
        /// <param name="camera">Camera</param>
        public void PostRenderCamera(Camera camera)
        {
            if (camera != null && Enabled)
            {
                WeatherMakerTemporalReprojectionState reprojState = temporalStates.Find(b => b.Camera == camera);
                if (reprojState != null)
                {
                    reprojState.PostRenderFrame(camera);
                }
            }
        }

        /// <summary>
        /// Cleanup all resources and set Enabled to false
        /// </summary>
        public void Dispose()
        {
            if (Application.isPlaying && clonedMaterial != null && clonedMaterial.name.IndexOf("(clone)", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                GameObject.DestroyImmediate(clonedMaterial);
                clonedMaterial = null;
            }

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                foreach (KeyValuePair<Camera, WeatherMakerCommandBuffer> kv in weatherMakerCommandBuffers)
                {
                    WeatherMakerCommandBufferManagerScript.Instance.RemoveCommandBuffer(kv.Value);
                }
            }

            foreach (KeyValuePair<Camera, WeatherMakerCommandBuffer> kv in weatherMakerCommandBuffers)
            {
                if (kv.Value != null && kv.Value.CommandBuffer != null)
                {
                    if (kv.Key != null)
                    {
                        kv.Key.RemoveCommandBuffer(RenderQueue, kv.Value.CommandBuffer);
                    }
                    kv.Value.CommandBuffer.Clear();
                    kv.Value.CommandBuffer.Release();
                }
            }
            weatherMakerCommandBuffers.Clear();
            foreach (WeatherMakerTemporalReprojectionState state in temporalStates)
            {
                state.Dispose();
            }
            temporalStates.Clear();
            Enabled = false;
        }
    }

    /// <summary>
    /// Command buffer extensions
    /// </summary>
    public static class CommandBufferExtensions
    {
        /// <summary>
        /// Draw a full screen quad
        /// </summary>
        /// <param name="commandBuffer">Command buffer</param>
        /// <param name="source">Source</param>
        /// <param name="dest">Dest</param>
        /// <param name="mat">Material</param>
        /// <param name="pass">Pass</param>
        public static void DrawQuad(this CommandBuffer commandBuffer, RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass)
        {
            //commandBuffer.SetGlobalTexture(WMS._MainTex, source);
            commandBuffer.SetRenderTarget(dest, 0, CubemapFace.Unknown, -1);
            commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            commandBuffer.DrawMesh(WeatherMakerFullScreenEffect.Quad, Matrix4x4.identity, mat, 0, pass);
        }
    }
}