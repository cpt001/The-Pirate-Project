// #define ENABLE_TEMPORAL_REPROJECTION_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Temporal reprojection state
    /// </summary>
    public class WeatherMakerTemporalReprojectionState : System.IDisposable
    {

#if ENABLE_TEMPORAL_REPROJECTION_DEBUG

        private int debugCount;

#endif

        private int frameIndex;
        private int frameIndex2;
        private int[] frameNumbers;
        private int subFrameCurrent;
        private int subFrameNumber;
        private int subFrameNumber2;
        private bool lastCameraWasRightEye;

        /// <summary>
        /// 
        /// </summary>
        public int ReprojectionSize { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ReprojectionSizeSquared { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int FrameWidth { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int FrameHeight { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int SubFrameWidth { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int SubFrameHeight { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int XOffset { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int YOffset { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetXConstant { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetYConstant { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetXConstant2 { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetYConstant2 { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float TemporalOffsetX { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float TemporalOffsetY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool NeedsFirstFrameHandling { get; set; }

        /// <summary>
        /// Integrated temporal reprojection combines a full shader with temporal reprojection in an attempt to reduce artifacts, especially at higher
        /// temporal reprojection sizes.
        /// When IntegratedTemporalReprojection is false, temporal reprojection renders in a separate upscaling pass.
        /// </summary>
        public bool IntegratedTemporalReprojection { get; set; }

        /// <summary>
        /// Command buffer
        /// </summary>
        public WeatherMakerCommandBuffer CommandBuffer { get; set; }

        /// <summary>
        /// Sub frame texture
        /// </summary>
        public RenderTexture SubFrameTexture { get; private set; }

        /// <summary>
        /// Previous frame texture
        /// </summary>
        public RenderTexture PrevFrameTexture { get; private set; }

        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; private set; }

        /// <summary>
        /// Temporal reprojection material
        /// </summary>
        public Material TemporalReprojectionMaterial { get; private set; }

        private RenderTexture subFrameTexture;
        private RenderTexture prevFrameTexture;
        private RenderTexture subFrameTexture2;
        private RenderTexture prevFrameTexture2;

        private readonly Matrix4x4[] projection = new Matrix4x4[2];
        private readonly Matrix4x4[] view = new Matrix4x4[2];
        private readonly Matrix4x4[] inverseProjection = new Matrix4x4[2];
        private readonly Matrix4x4[] inverseProjectionView = new Matrix4x4[2];
        private readonly Matrix4x4[] inverseView = new Matrix4x4[2];
        private readonly Matrix4x4[] prevView = new Matrix4x4[2];
        private readonly Matrix4x4[] prevViewProjection = new Matrix4x4[2];
        private readonly Matrix4x4[] ipivpvp = new Matrix4x4[2];

        private void DisposeRenderTextures()
        {
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref subFrameTexture);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref subFrameTexture2);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref prevFrameTexture);
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref prevFrameTexture2);
        }

        private void CreateFrameNumbers()
        {
            int i = 0;
            frameNumbers = new int[ReprojectionSizeSquared];

            for (i = 0; i < ReprojectionSizeSquared; i++)
            {
                frameNumbers[i] = i;
            }

            // generate array of unique random numbers in range in random order, no duplicates
            // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
            while (i-- > 0)
            {
                int k = frameNumbers[i];
                int j = (int)(WeatherMakerRandomizer.Unity.Random() * 1000.0f) % ReprojectionSizeSquared;
                frameNumbers[i] = frameNumbers[j];
                frameNumbers[j] = k;
            }

            subFrameCurrent = frameNumbers[0];
            subFrameNumber = frameNumbers[0];
            subFrameNumber2 = frameNumbers[0];
            frameIndex = 0;
            frameIndex2 = 0;
        }

        private RenderTexture CreateRenderTexture(string name, int scale, int mod, int scale2, RenderTextureFormat format, Camera camera)
        {
            RenderTextureDescriptor desc = WeatherMakerFullScreenEffect.GetRenderTextureDescriptor(scale, mod, scale2, format, 0, camera);
            RenderTexture tex = WeatherMakerFullScreenEffect.CreateRenderTexture(desc, false, FilterMode.Bilinear, TextureWrapMode.Clamp);
            tex.name = name;
            return tex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="reprojMaterial">Temporal reprojection material</param>
        /// <param name="integratedTemporalReprojection">Whether to use integrated temporal reprojection (requires custom integrated shader)</param>
        public WeatherMakerTemporalReprojectionState(Camera camera, Material reprojMaterial, bool integratedTemporalReprojection)
        {
            Camera = camera;
            TemporalReprojectionMaterial = (reprojMaterial == null ? null : (Application.isPlaying ? new Material(reprojMaterial) : reprojMaterial));
            IntegratedTemporalReprojection = integratedTemporalReprojection;
            if (TemporalReprojectionMaterial != null && Application.isPlaying)
            {
                TemporalReprojectionMaterial.name += " (Clone)";
            }
        }

        private void ComputeTemporalOffsets(Camera camera)
        {
            if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
            {
                lastCameraWasRightEye = true;
                subFrameCurrent = subFrameNumber2;
                SubFrameTexture = subFrameTexture2;
                PrevFrameTexture = prevFrameTexture2;
            }
            else
            {
                subFrameCurrent = subFrameNumber;
                SubFrameTexture = subFrameTexture;
                PrevFrameTexture = prevFrameTexture;
            }

            // offset value for projection matrix
            XOffset = (subFrameCurrent % ReprojectionSize);
            YOffset = (subFrameCurrent / ReprojectionSize);
            TemporalOffsetX = ((float)XOffset * OffsetXConstant) + OffsetXConstant2;
            TemporalOffsetY = ((float)YOffset * OffsetYConstant) + OffsetYConstant2;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            DisposeRenderTextures();
            if (TemporalReprojectionMaterial != null && TemporalReprojectionMaterial.name.IndexOf("(Clone)") >= 0)
            {
                GameObject.DestroyImmediate(TemporalReprojectionMaterial);
            }
        }

        /// <summary>
        /// PreCull camera event handler
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="scale">Scale</param>
        /// <param name="projSize">Reprojection size</param>
        public void PreCullFrame(Camera camera, WeatherMakerDownsampleScale scale, WeatherMakerTemporalReprojectionSize projSize)
        {
            if (TemporalReprojectionMaterial == null)
            {
                return;
            }
            WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
            if (cameraType == WeatherMakerCameraType.Other)
            {
                return;
            }
            int projSizeInt = (int)projSize;
            int downsampling = (int)scale;
            int frameWidth = camera.pixelWidth / downsampling;
            int frameHeight = camera.pixelHeight / downsampling;

            // ensure reprojection fits cleanly into frame
            while (frameWidth % projSizeInt != 0) { frameWidth++; }
            while (frameHeight % projSizeInt != 0) { frameHeight++; }

            if (frameWidth != FrameWidth || frameHeight != FrameHeight || ReprojectionSize != projSizeInt)
            {
                DisposeRenderTextures();
                ReprojectionSize = projSizeInt;
                ReprojectionSizeSquared = ReprojectionSize * ReprojectionSize;
                CreateFrameNumbers();
                FrameWidth = frameWidth;
                FrameHeight = frameHeight;
                SubFrameWidth = frameWidth / projSizeInt;
                SubFrameHeight = frameHeight / projSizeInt;

                RenderTextureFormat format = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
                subFrameTexture = CreateRenderTexture("WeatherMakerTemporalReprojectionSubFrame_" + camera.name, downsampling, projSizeInt, projSizeInt, format, camera);
                prevFrameTexture = CreateRenderTexture("WeatherMakerTemporalReprojectionPrevFrame_" + camera.name, downsampling, projSizeInt, 1, format, camera);
                if (WeatherMakerScript.HasXRDeviceMultipass())
                {
                    subFrameTexture2 = CreateRenderTexture("WeatherMakerTemporalReprojectionSubFrame2_" + camera.name, downsampling, projSizeInt, projSizeInt, format, camera);
                    prevFrameTexture2 = CreateRenderTexture("WeatherMakerTemporalReprojectionPrevFrame2_" + camera.name, downsampling, projSizeInt, 1, format, camera);
                }

                OffsetXConstant = (1.0f / (float)frameWidth);
                OffsetYConstant = (1.0f / (float)frameHeight);
                OffsetXConstant2 = (-0.5f * (float)(ReprojectionSize - 1) * OffsetXConstant);
                OffsetYConstant2 = (-0.5f * (float)(ReprojectionSize - 1) * OffsetYConstant);
                NeedsFirstFrameHandling = true;
            }
        }

        /// <summary>
        /// Pre render camera event handler
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="lastNormalCamera">The last normal camera</param>
        /// <param name="commandBuffer">Command buffer</param>
        public void PreRenderFrame(Camera camera, Camera lastNormalCamera, CommandBuffer commandBuffer)
        {
            if (TemporalReprojectionMaterial == null)
            {
                return;
            }
            WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
            if (cameraType == WeatherMakerCameraType.Other)
            {
                return;
            }

            ComputeTemporalOffsets(camera);
            lastNormalCamera = (cameraType == WeatherMakerCameraType.Normal ? camera : (lastNormalCamera ?? camera));

            Vector3 pos = camera.transform.position;
            bool isStatic = Shader.GetGlobalInt(WMS._WeatherMakerWeatherMapTextureStatic) == 1;
            if (isStatic)
            {
                camera.transform.position = Vector3.zero;
            }

            // assign current matrixes, only normal stereo cameras use the stereo methods
            // reflection cameras are rendered one eye at a time without stereo
            if (camera.stereoEnabled)
            {
                view[0] = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                view[1] = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                projection[0] = lastNormalCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                projection[1] = lastNormalCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                inverseView[0] = view[0].inverse;
                inverseView[1] = view[1].inverse;
                inverseProjection[0] = projection[0].inverse;
                inverseProjection[1] = projection[1].inverse;
                inverseProjectionView[0] = inverseProjection[0] * inverseView[0];
                inverseProjectionView[1] = inverseProjection[1] * inverseView[1];
                prevViewProjection[0] = (projection[0] * prevView[0]);
                prevViewProjection[1] = (projection[1] * prevView[1]);
                ipivpvp[0] = projection[0] * prevView[0] * inverseView[0] * inverseProjection[0];
                ipivpvp[1] = projection[1] * prevView[1] * inverseView[1] * inverseProjection[1];
            }
            else
            {
                view[0] = view[1] = camera.worldToCameraMatrix;
                inverseView[0] = inverseView[1] = camera.cameraToWorldMatrix;
                projection[0] = projection[1] = lastNormalCamera.projectionMatrix;
                inverseProjection[0] = inverseProjection[1] = lastNormalCamera.projectionMatrix.inverse;
                inverseProjectionView[0] = inverseProjectionView[1] = inverseView[0] * inverseProjection[0];
                prevViewProjection[0] = prevViewProjection[1] = (projection[0] * prevView[0]);
                ipivpvp[0] = ipivpvp[1] = projection[0] * prevView[0] * inverseView[0] * inverseProjection[0];
            }

            if (isStatic)
            {
                camera.transform.position = pos;
            }

            commandBuffer.SetGlobalTexture(WMS._TemporalReprojection_SubFrame, SubFrameTexture);
            commandBuffer.SetGlobalTexture(WMS._TemporalReprojection_PrevFrame, PrevFrameTexture);
            commandBuffer.SetGlobalFloat(WMS._TemporalReprojection_SubFrameNumber, subFrameCurrent);
            commandBuffer.SetGlobalFloat(WMS._TemporalReprojection_SubPixelSize, ReprojectionSize);
            commandBuffer.SetGlobalFloat(WMS._WeatherMakerTemporalReprojectionAlphaThreshold, TemporalReprojectionMaterial.GetFloat(WMS._WeatherMakerTemporalReprojectionAlphaThreshold));
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_PreviousView, prevView);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_View, view);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_InverseView, inverseView);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_Projection, projection);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_InverseProjection, inverseProjection);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_InverseProjectionView, inverseProjectionView);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_PreviousViewProjection, prevViewProjection);
            commandBuffer.SetGlobalMatrixArray(WMS._TemporalReprojection_ipivpvp, ipivpvp);
        }

        /// <summary>
        /// Post render camera event handler
        /// </summary>
        /// <param name="camera">Camera</param>
        public void PostRenderFrame(Camera camera)
        {
            if (TemporalReprojectionMaterial == null)
            {
                return;
            }
            WeatherMakerCameraType cameraType = WeatherMakerScript.GetCameraType(camera);
            if (cameraType == WeatherMakerCameraType.Other)
            {
                return;
            }

            // move to next temporal reprojection frame
            unchecked
            {

#if ENABLE_TEMPORAL_REPROJECTION_DEBUG

                if (++debugCount % 60 == 0)

#endif

                {
                    if (lastCameraWasRightEye)
                    {
                        lastCameraWasRightEye = false;
                        if (++frameIndex2 == frameNumbers.Length)
                        {
                            frameIndex2 = 0;
                        }
                        subFrameNumber2 = frameNumbers[frameIndex2];
                        prevView[1] = view[1];

                        // turn off first frame handling
                        NeedsFirstFrameHandling = false;
                    }
                    else
                    {
                        if (++frameIndex == frameNumbers.Length)
                        {
                            frameIndex = 0;
                        }
                        subFrameNumber = frameNumbers[frameIndex];
                        prevView[0] = view[0];
                        if (!WeatherMakerScript.HasXRDeviceMultipass())
                        {
                            prevView[1] = view[1];

                            // turn off first frame handling
                            NeedsFirstFrameHandling = false;
                        }
                    }
                }
            }
        }
    }
}
