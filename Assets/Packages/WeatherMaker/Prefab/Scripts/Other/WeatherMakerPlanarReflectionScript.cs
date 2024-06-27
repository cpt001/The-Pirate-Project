using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Planar reflection script
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerPlanarReflectionScript : MonoBehaviour
    {
        /// <summary>Renderer to draw reflection in, if null you must set ReflectMaterial.</summary>
        [Header("Reflection Rendering")]
        [Tooltip("Renderer to draw reflection in, if null you must set ReflectMaterial.")]
        public Renderer ReflectRenderer;

        /// <summary>Material put reflection textures on</summary>
        [Tooltip("Material put reflection textures on")]
        public Material ReflectMaterial;

        /// <summary>What layers to reflect, set to none or 0 to disable reflections.</summary>
        [Tooltip("What layers to reflect, set to none or 0 to disable reflections.")]
        public LayerMask ReflectionMask = ~(1 << 4);

        /// <summary>Whether to reflect the skybox</summary>
        [Header("Reflection Performance")]
        [Tooltip("Whether to reflect the skybox")]
        public bool ReflectSkybox;

        /// <summary>Reflection texture name for shaders to use</summary>
        [Tooltip("Reflection texture name for shaders to use")]
        public string ReflectionSamplerName = "_ReflectionTex";
        private string ReflectionSamplerName2;

        /// <summary>Maximum per pixel lights in reflection</summary>
        [Tooltip("Maximum per pixel lights in reflection")]
        [Range(0, 128)]
        public int MaximumPerPixelLightsToReflect = 8;

        /// <summary>Near clip plane offset for reflection</summary>
        [Tooltip("Near clip plane offset for reflection")]
        public float ClipPlaneOffset = 0.07f;

        /// <summary>Render texture size. Based on aspect ratio this will use this size as the width or height.</summary>
        [Tooltip("Render texture size. Based on aspect ratio this will use this size as the width or height.")]
        [Range(64, 4096)]
        public int RenderTextureSize = 1024;

        /// <summary>The reflection camera render path. Set to 'UsePlayerSettings' to take on the observing camera rendering path. DO NOT CHANGE AT RUNTIME.</summary>
        [Tooltip("The reflection camera render path. Set to 'UsePlayerSettings' to take on the observing camera rendering path. DO NOT CHANGE AT RUNTIME.")]
        public RenderingPath ReflectionCameraRenderingPath = RenderingPath.Forward;

        /// <summary>Whether normal is forward. True for (0,0,-1) false for (0,1,0). Quads are usually false, planes are usually true.</summary>
        [Header("Reflection Camera")]
        [Tooltip("Whether normal is forward. True for (0,0,-1) false for (0,1,0). Quads are usually false, planes are usually true.")]
        public bool NormalIsForward;

        /// <summary>Aspect ratio (width/height) for reflection camera, 0 for default.</summary>
        [Tooltip("Aspect ratio (width/height) for reflection camera, 0 for default.")]
        [Range(0.0f, 10.0f)]
        public float AspectRatio = 0.0f;

        /// <summary>Field of view for reflection camera, 0 for default.</summary>
        [Tooltip("Field of view for reflection camera, 0 for default.")]
        [Range(0.0f, 360.0f)]
        public float FieldOfView = 0.0f;

        /// <summary>Near plane for reflection camera, 0 for default.</summary>
        [Tooltip("Near plane for reflection camera, 0 for default.")]
        public float NearPlane = 0.0f;

        /// <summary>Far plane for reflection camera, 0 for default.</summary>
        [Tooltip("Far plane for reflection camera, 0 for default.")]
        public float FarPlane = 0.0f;

        /// <summary>Render texture format for reflection</summary>
        [Tooltip("Render texture format for reflection")]
        public RenderTextureFormat RenderTextureFormat = RenderTextureFormat.ARGB32;

        /// <summary>Left eye texture, set automatically</summary>
        [Header("Reflection Debug")]
        [Tooltip("Left eye texture, set automatically")]
        public RenderTexture LeftEyeTexture;

        /// <summary>Right eye texture, set automatically</summary>
        [Tooltip("Right eye texture, set automatically")]
        public RenderTexture RightEyeTexture;

        /// <summary>
        /// Set to true to negate the transform normal (i.e. underwater reflection vs above water reflection)
        /// </summary>
        public bool TransformNormalNegate { get; set; }

        /// <summary>
        /// Reflection offset func to offset transform position of source camera
        /// </summary>
        public System.Func<Vector3, Vector3> ReflectionOffsetFunc { get; set; }

        private const int waterLayerInverse = -17;

        private readonly List<KeyValuePair<Camera, Camera>> sourceToReflectionCameras = new List<KeyValuePair<Camera, Camera>>();

        private MaterialPropertyBlock reflectPropertyBlock;

        /// <summary>
        /// Determines whether a camera is a reflection camera
        /// </summary>
        /// <param name="cam">Camera</param>
        /// <returns>True if cam is a reflection camera, false otherwise</returns>
        public static bool CameraIsReflection(Camera cam)
        {
            string tmp;
            return CameraIsReflection(cam, out tmp);
        }

        /// <summary>
        /// Determines whether a camera is a reflection camera
        /// </summary>
        /// <param name="cam">Camera</param>
        /// <param name="camName">Receives camera name for re-use later</param>
        /// <returns>True if cam is a reflection camera, false otherwise</returns>
        public static bool CameraIsReflection(Camera cam, out string camName)
        {
            camName = cam.CachedName();
            return camName.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0 ||
                camName.IndexOf("refl", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Update()
        {
            CleanupCameras();
        }

        private void OnEnable()
        {
            if (ReflectRenderer != null)
            {
                ReflectMaterial = (ReflectMaterial == null ? ReflectRenderer.sharedMaterial : ReflectMaterial);
            }
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
            ReflectionSamplerName2 = ReflectionSamplerName + "2";
            reflectPropertyBlock = new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            CleanupCameras();
            CleanupTextures();
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
            if (ReflectMaterial == null || camera == null || WeatherMakerLightManagerScript.Instance == null || (ReflectRenderer != null && !ReflectRenderer.enabled) ||
                !gameObject.activeInHierarchy || WeatherMakerCommandBufferManagerScript.CameraStackCount > 1 || WeatherMakerScript.ShouldIgnoreCamera(this, camera, true))
            {
                return;
            }

            // HACK: Reflections need to ensure the frustum planes and corners are up to date. This is normally done in a pre-render
            //  event, but we cannot do that as rendering a reflection in pre-render mucks up state and such
            WeatherMakerLightManagerScript.Instance.CalculateFrustumPlanes(camera);

            // this frustum cull test is expensive, but well worth it if it prevents a reflection from needing to be rendered
            // get up to date frustum info
            if (ReflectRenderer != null && !WeatherMakerGeometryUtility.BoxIntersectsFrustum(camera, WeatherMakerLightManagerScript.Instance.CurrentCameraFrustumPlanes,
                WeatherMakerLightManagerScript.Instance.CurrentCameraFrustumCorners, ReflectRenderer.bounds))
            {
                return;
            }

            // render the reflection in pre-cull, pre-render is when shader properties and other such things are set
            Camera reflectionCamera = GetOrCreateReflectionCamera(camera);
            RenderReflectionCamera(camera, reflectionCamera);
        }

        private Camera GetOrCreateReflectionCamera(Camera sourceCamera)
        {
            Camera reflectionCamera = null;
            for (int i = 0; i < sourceToReflectionCameras.Count; i++)
            {
                if (sourceToReflectionCameras[i].Key == sourceCamera)
                {
                    reflectionCamera = sourceToReflectionCameras[i].Value;
                }
            }
            if (reflectionCamera == null)
            {
                GameObject obj = new GameObject("WeatherMakerPlanarReflectionScriptCamera");
                obj.hideFlags = HideFlags.HideAndDontSave;
                obj.SetActive(false);
                obj.transform.parent = transform;
                reflectionCamera = obj.AddComponent<Camera>();
                reflectionCamera.enabled = false;
                sourceToReflectionCameras.Add(new KeyValuePair<Camera, Camera>(sourceCamera, reflectionCamera));
            }

            return reflectionCamera;
        }

        private void CleanupCameras()
        {
            // cleanup reflection cameras
            for (int i = sourceToReflectionCameras.Count - 1; i >= 0; i--)
            {
                if (sourceToReflectionCameras[i].Key == null || sourceToReflectionCameras[i].Value == null)
                {
                    GameObject.DestroyImmediate(sourceToReflectionCameras[i].Value.gameObject);
                    sourceToReflectionCameras.RemoveAt(i);
                }
            }
        }

        private void CleanupTextures()
        {
            if (LeftEyeTexture != null)
            {
                LeftEyeTexture.Release();
                DestroyImmediate(LeftEyeTexture);
                LeftEyeTexture = null;
            }
            if (RightEyeTexture != null)
            {
                RightEyeTexture.Release();
                DestroyImmediate(RightEyeTexture);
                RightEyeTexture = null;
            }
        }

        private void CreateRenderTextures(Camera sourceCamera)
        {
            RenderTextureFormat textureFormat = WeatherMakerFullScreenEffect.DefaultRenderTextureFormat();
            int size = Math.Max(32, RenderTextureSize);
            if (WeatherMakerScript.Instance != null)
            {
                size = WeatherMakerScript.Instance.PerformanceProfile.ReflectionTextureSize;
            }

            // cleanup
            if (LeftEyeTexture != null && LeftEyeTexture.width != size)
            {
                LeftEyeTexture.Release();
                DestroyImmediate(LeftEyeTexture);
            }
            if (RightEyeTexture != null && RightEyeTexture.width != size)
            {
                RightEyeTexture.Release();
                DestroyImmediate(RightEyeTexture);
            }

            if (LeftEyeTexture == null || !LeftEyeTexture.IsCreated())
            {
                LeftEyeTexture = new RenderTexture(size, size, 16, textureFormat) { name = "WeatherMakerPlanaReflectionLeftEyeTexture" };
                LeftEyeTexture.wrapMode = TextureWrapMode.Clamp;
                LeftEyeTexture.filterMode = FilterMode.Bilinear;
            }

            MaterialPropertyBlock reflectBlock = null;
            if (ReflectRenderer == null)
            {
                ReflectMaterial.SetTexture(ReflectionSamplerName, LeftEyeTexture);
            }
            else
            {
                reflectBlock = reflectPropertyBlock;
                ReflectRenderer.GetPropertyBlock(reflectPropertyBlock);
                reflectBlock.SetTexture(ReflectionSamplerName, LeftEyeTexture);
            }

            if (sourceCamera.stereoEnabled)
            {
                if (RightEyeTexture == null || !RightEyeTexture.IsCreated())
                {
                    RightEyeTexture = new RenderTexture(size, size, 16, textureFormat) { name = "WeatherMakerPlanaReflectionRightEyeTexture" };
                    RightEyeTexture.wrapMode = TextureWrapMode.Clamp;
                    RightEyeTexture.filterMode = FilterMode.Bilinear;
                }

                if (reflectBlock == null)
                {
                    ReflectMaterial.SetTexture(ReflectionSamplerName2, RightEyeTexture);
                }
                else
                {
                    reflectBlock.SetTexture(ReflectionSamplerName2, RightEyeTexture);
                }
            }

            if (reflectBlock != null)
            {
                ReflectRenderer.SetPropertyBlock(reflectBlock);
            }
        }

        /// <summary>
        /// Render a reflection to the reflection render textures and apply to material
        /// </summary>
        /// <param name="sourceCamera">Source camera</param>
        /// <param name="reflectionCamera">Reflection camera</param>
        private void RenderReflectionCamera(Camera sourceCamera, Camera reflectionCamera)
        {
            CreateRenderTextures(sourceCamera);
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            int oldCullingMask = reflectionCamera.cullingMask;
            bool oldSoftParticles = QualitySettings.softParticles;
            int oldAntiAliasing = QualitySettings.antiAliasing;
            ShadowQuality oldShadows = QualitySettings.shadows;
            SyncCameraSettings(reflectionCamera, sourceCamera);
            if (WeatherMakerScript.Instance != null && QualitySettings.shadows != ShadowQuality.Disable &&
                WeatherMakerLightManagerScript.ScreenSpaceShadowMode != UnityEngine.Rendering.BuiltinShaderMode.Disabled)
            {
                QualitySettings.shadows = WeatherMakerScript.Instance.PerformanceProfile.ReflectionShadows;
            }

            // get reflection normal vector
            Transform reflectionTransform = transform;
            Vector3 reflectionNormal = (NormalIsForward ? -reflectionTransform.forward : reflectionTransform.up);
            if (TransformNormalNegate)
            {
                reflectionNormal = -reflectionNormal;
            }

            // render the actual reflection
            if (sourceCamera.stereoEnabled)
            {
                if (sourceCamera.stereoTargetEye == StereoTargetEyeMask.Both || sourceCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderReflectionInternal(sourceCamera, reflectionCamera, reflectionTransform, reflectionNormal, StereoTargetEyeMask.Left, LeftEyeTexture);
                }
                if (sourceCamera.stereoTargetEye == StereoTargetEyeMask.Both || sourceCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderReflectionInternal(sourceCamera, reflectionCamera, reflectionTransform, reflectionNormal, StereoTargetEyeMask.Right, RightEyeTexture);
                }
            }
            else
            {
                RenderReflectionInternal(sourceCamera, reflectionCamera, reflectionTransform, reflectionNormal, StereoTargetEyeMask.Both, LeftEyeTexture);
            }

            // restore render state
            reflectionCamera.cullingMask = oldCullingMask;
            QualitySettings.pixelLightCount = oldPixelLightCount;
            QualitySettings.softParticles = oldSoftParticles;
            QualitySettings.antiAliasing = oldAntiAliasing;
            QualitySettings.shadows = oldShadows;
        }

        /// <summary>
        /// Render a reflection camera. Reflection camera should already be setup with a render texture.
        /// </summary>
        /// <param name="sourceCamera">Camera source</param>
        /// <param name="reflectionCamera">Reflection camera</param>
        /// <param name="reflectionTransform">Reflection transform</param>
        /// <param name="reflectionNormal">Reflection normal vector</param>
        /// <param name="eye">Stereo eye mask</param>
        /// <param name="targetTexture">Target texture</param>
        private void RenderReflectionInternal
        (
            Camera sourceCamera,
            Camera reflectionCamera,
            Transform reflectionTransform,
            Vector3 reflectionNormal,
            StereoTargetEyeMask eye,
            RenderTexture targetTexture
        )
        {
            bool sourceCameraIsReflection = CameraIsReflection(sourceCamera);
            bool oldInvertCulling = GL.invertCulling;

            // find out the reflection plane: position and normal in world space
            Vector3 pos = reflectionTransform.position;

            // Render reflection
            // Reflect camera around reflection plane
            if (sourceCameraIsReflection && GL.invertCulling)
            {
                reflectionNormal = -reflectionNormal;
            }

            float d = -Vector3.Dot(reflectionNormal, pos) - ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(reflectionNormal.x, reflectionNormal.y, reflectionNormal.z, d);

            Matrix4x4 reflection;
            CalculateReflectionMatrix(out reflection, reflectionPlane);
            Vector3 origReflectionStartPos = sourceCamera.transform.position;
            Vector3 reflectionStartPos = origReflectionStartPos;
            if (ReflectionOffsetFunc != null)
            {
                reflectionStartPos = ReflectionOffsetFunc(reflectionStartPos);
            }
            Vector3 reflectionPos = reflection.MultiplyPoint(reflectionStartPos);
            Matrix4x4 worldToCameraMatrix = sourceCamera.worldToCameraMatrix;
            if (sourceCamera.stereoEnabled && eye == StereoTargetEyeMask.Left)
            {
                worldToCameraMatrix[12] += (sourceCamera.stereoSeparation * 0.5f);
                reflectionCamera.projectionMatrix = sourceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            }
            else if (sourceCamera.stereoEnabled && eye == StereoTargetEyeMask.Right)
            {
                worldToCameraMatrix[12] -= (sourceCamera.stereoSeparation * 0.5f);
                reflectionCamera.projectionMatrix = sourceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            }
            else
            {
                reflectionCamera.projectionMatrix = sourceCamera.projectionMatrix;
            }
            reflectionCamera.worldToCameraMatrix = worldToCameraMatrix * reflection;
            if (ClipPlaneOffset > 0.0f)
            {
                // Optimization: Setup oblique projection matrix so that near plane is our reflection plane.
                // This way we clip everything below/above it for free.
                Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, reflectionNormal, ClipPlaneOffset, GL.invertCulling ? -1.0f : 1.0f);
                reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlane);
            }
            GL.invertCulling = !GL.invertCulling;
            reflectionCamera.transform.position = reflectionPos;

            // set the eye mask so that any command buffers know how to render this camera
            reflectionCamera.stereoTargetEye = eye;
            reflectionCamera.targetTexture = targetTexture;
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RenderCamera(reflectionCamera);
            }
            reflectionCamera.targetTexture = null;

            reflectionCamera.transform.position = origReflectionStartPos;
            GL.invertCulling = oldInvertCulling;
        }

        private void SyncCameraSettings(Camera reflectCamera, Camera sourceCamera)
        {
            reflectCamera.nearClipPlane = (NearPlane <= 0.0f ? sourceCamera.nearClipPlane : NearPlane);
            reflectCamera.farClipPlane = (FarPlane <= 0.0f ? sourceCamera.farClipPlane : FarPlane);
            reflectCamera.aspect = (AspectRatio <= 0.0f ? sourceCamera.aspect : AspectRatio);
            if (!reflectCamera.stereoEnabled)
            {
                reflectCamera.fieldOfView = (FieldOfView <= 0.0f ? sourceCamera.fieldOfView : FieldOfView);
            }
            reflectCamera.orthographic = sourceCamera.orthographic;
            reflectCamera.orthographicSize = sourceCamera.orthographicSize;
            reflectCamera.renderingPath = (ReflectionCameraRenderingPath == RenderingPath.UsePlayerSettings ? sourceCamera.renderingPath : ReflectionCameraRenderingPath);
            reflectCamera.backgroundColor = Color.red;
            reflectCamera.clearFlags = ReflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            reflectCamera.cullingMask = ReflectionMask;
            reflectCamera.stereoSeparation = sourceCamera.stereoSeparation;
            reflectCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            //reflectCamera.transform.rotation = sourceCamera.transform.rotation;
            //reflectCamera.transform.position = sourceCamera.transform.position;

            if (ReflectSkybox)
            {
                if (sourceCamera.gameObject.GetComponent(typeof(Skybox)))
                {
                    Skybox sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
                    if (!sb)
                    {
                        sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
                        sb.hideFlags = HideFlags.HideAndDontSave;
                    }
                    sb.material = ((Skybox)sourceCamera.GetComponent(typeof(Skybox))).material;
                }
            }
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float clipPlaneOffset, float sideSign)
        {
            Vector3 offsetPos = pos + (normal * clipPlaneOffset);
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        /// <summary>
        /// Compute reflection matrix
        /// </summary>
        /// <param name="reflectionMat">Receives reflection matrix</param>
        /// <param name="plane">Plane</param>
        public static void CalculateReflectionMatrix(out Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
    }
}
