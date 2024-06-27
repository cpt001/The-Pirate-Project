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

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Depth texture creator for a camera
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerDepthCameraScript : MonoBehaviour
    {
        /// <summary>Render texture size</summary>
        [Tooltip("Render texture size")]
        [Range(64, 2048)]
        public int RenderTextureSize = 256;

        /// <summary>Camera height above transform y</summary>
        [Tooltip("Camera height above transform y")]
        [Range(100.0f, 1000.0f)]
        public float CameraHeight = 200.0f;

        /// <summary>Camera depth beyond transform y</summary>
        [Tooltip("Camera depth beyond transform y")]
        [Range(1.0f, 1000.0f)]
        public float CameraDepth = 200.0f;

        /// <summary>Layer mask</summary>
        [Tooltip("Layer mask")]
        public LayerMask LayerMask = ~(1 << 4);

        /// <summary>Whether to auto-detect orthographic size. If false, you must manually specify the orthographic size</summary>
        [Tooltip("Whether to auto-detect orthographic size. If false, you must manually specify the orthographic size")]
        public bool AutoBounds = true;

        /// <summary>Orthographic size</summary>
        [Tooltip("Orthographic size")]
        [Range(1.0f, 1000.0f)]
        public float OrthographicSize = 10.0f;

        internal float autoOrthographicSize { get; set; }

        /// <summary>Aspect ratio</summary>
        [Tooltip("Aspect ratio")]
        [Range(0.01f, 10.0f)]
        public float AspectRatio = 1.0f;

        /// <summary>Set to true to take a snapshot of the water depth buffer</summary>
        [Tooltip("Set to true to take a snapshot of the water depth buffer")]
        public bool Dirty = true;

        /// <summary>Renderer to set material properties on</summary>
        [Tooltip("Renderer to set material properties on")]
        public Renderer Renderer;

        private Camera depthCamera;
        private GameObject depthCameraGameObject;

        /// <summary>
        /// Output depth texture
        /// </summary>
        public RenderTexture DepthTexture;

#if UNITY_EDITOR

        private int dirtyCounter;

#endif

        private void Update()
        {
            if (Renderer != null && Renderer.sharedMaterial != null)
            {

#if UNITY_EDITOR

                if (!Application.isPlaying || ++dirtyCounter == 15)
                {
                    dirtyCounter = 0;
                    Dirty = true;
                }

#endif

                if (Dirty)
                {
                    RenderCamera();
                }
                Renderer.sharedMaterial.SetTexture(WMS._WeatherMakerYDepthTexture, DepthTexture);
            }
        }

        private void OnEnable()
        {
            Dirty = true;

            if (depthCameraGameObject != null)
            {
                return;
            }

            string cameraObjectName = name + "_YDepthBufferCamera_" + GetInstanceID();
            depthCameraGameObject = GameObject.Find(cameraObjectName);
            if (depthCameraGameObject == null)
            {
                depthCameraGameObject = new GameObject { name = cameraObjectName };
            }
            depthCameraGameObject.hideFlags = HideFlags.HideAndDontSave;
            depthCamera = depthCameraGameObject.AddComponent<Camera>();
            depthCamera.enabled = false;
            depthCamera.allowMSAA = false;
            depthCamera.useOcclusionCulling = false;
            depthCamera.allowHDR = true;
            depthCamera.renderingPath = RenderingPath.VertexLit;
            depthCamera.nearClipPlane = 0.01f;
            depthCamera.clearFlags = CameraClearFlags.Depth;
            depthCamera.backgroundColor = Color.black;
            depthCamera.orthographic = true;
        }

        private void OnDisable()
        {
            if (DepthTexture != null)
            {
                DepthTexture.Release();
                GameObject.DestroyImmediate(DepthTexture);
                DepthTexture = null;
            }
            if (depthCameraGameObject != null)
            {
                GameObject.DestroyImmediate(depthCameraGameObject);
            }
            if (Renderer != null && Renderer.sharedMaterial != null)
            {
                Renderer.sharedMaterial.SetVector(WMS._WeatherMakerYDepthParams, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Dirty = true;
            }
        }

        private void UpdateResources()
        {
            if (DepthTexture == null || DepthTexture.width != RenderTextureSize)
            {
                if (DepthTexture != null)
                {
                    DepthTexture.Release();
                }
                int size = Mathf.Max(RenderTextureSize, 16);
                DepthTexture = new RenderTexture(size, size, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
                DepthTexture.wrapMode = TextureWrapMode.Clamp;
                DepthTexture.filterMode = FilterMode.Bilinear;
                DepthTexture.autoGenerateMips = false;
                DepthTexture.useMipMap = false;
                DepthTexture.name = "WeatherMakerYDepthTexture";
                DepthTexture.hideFlags = HideFlags.DontSave;
            }
            if (depthCamera != null)
            {
                depthCamera.targetTexture = DepthTexture;
                depthCamera.orthographicSize = OrthographicSize;
                depthCamera.aspect = AspectRatio;
                depthCamera.farClipPlane = CameraHeight + CameraDepth;
                depthCamera.cullingMask = LayerMask;
            }
        }

        private void RenderCamera()
        {
            if (depthCamera == null || Renderer == null || Renderer.sharedMaterial == null || !enabled)
            {
                return;
            }

            UpdateResources();
            Vector3 pos = transform.position + (Vector3.up * CameraHeight);
            depthCameraGameObject.transform.position = pos;
            depthCameraGameObject.transform.rotation = transform.rotation;
            depthCameraGameObject.transform.forward = Vector3.down;
            depthCamera.Render();
            float heightPercent = CameraHeight / (CameraHeight + CameraDepth);
            float depthPercent = 1.0f - heightPercent;
            Renderer.sharedMaterial.SetVector(WMS._WeatherMakerYDepthParams, new Vector4(CameraDepth, heightPercent, depthPercent, 1.0f / depthPercent));
            Dirty = false;
        }
    }
}
