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
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sky sphere renderer and general sky handling script
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WeatherMakerSkySphereScript : WeatherMakerSphereCreatorScript
    {
        /// <summary>
        /// Sky sphere profile
        /// </summary>
        [Header("Sky sphere profile")]
        [Tooltip("Sky sphere profile")]
        public WeatherMakerSkyProfileScript SkySphereProfile;

        /// <summary>Atmosphere lookup texture creation material</summary>
        [Tooltip("Atmosphere lookup texture creation material")]
        public Material AtmosphereLookupMaterial;

        /// <summary>Atmosphere compute shader</summary>
        [Tooltip("Atmosphere compute shader")]
        public ComputeShader AtmosphereComputeShader;

        private void OnEnable()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }
            else if (Application.isPlaying && MeshRenderer.sharedMaterial != null &&
                MeshRenderer.sharedMaterial.name.IndexOf("(Clone)", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                MeshRenderer.sharedMaterial = GameObject.Instantiate(MeshRenderer.sharedMaterial) as Material;
            }
            WeatherMakerScript.EnsureInstance(this, ref instance);
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreRender(CameraPreRender, this);
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPostRender(CameraPostRender, this);
            }
            if (SkySphereProfile == null)
            {
                SkySphereProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerSkyProfileScript>("WeatherMakerSkyProfile_Procedural");
            }
            if (SkySphereProfile.AtmosphereProfile == null)
            {
                SkySphereProfile.AtmosphereProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerAtmosphereProfileScript>("WeatherMakerAtmosphereProfile_Default");
            }
            if (SkySphereProfile.AtmosphereProfile != null)
            {
                SkySphereProfile.AtmosphereProfile.UpdateShaderVariables(null, AtmosphereLookupMaterial, AtmosphereComputeShader);
            }
        }

        private void OnDisable()
        {
            
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Application.isPlaying && MeshRenderer.sharedMaterial != null &&
                MeshRenderer.sharedMaterial.name.IndexOf("(Clone)", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                GameObject.DestroyImmediate(MeshRenderer.sharedMaterial, true);
            }
            WeatherMakerAtmosphereProfileScript.Shutdown();
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreRender(this);
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPostRender(this);
            }
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (SkySphereProfile != null && WeatherMakerDayNightCycleManagerScript.HasInstance() &&
                (SkySphereProfile.SkyMode == WeatherMakeSkyMode.ProceduralUnityStyle || SkySphereProfile.SkyMode == WeatherMakeSkyMode.ProceduralPhysicallyBased) &&
                WeatherMakerDayNightCycleManagerScript.Instance.DayNightProfile != null && SkySphereProfile.RotateAxis != Vector3.zero)
            {
                float seconds = WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay;
                Transform t = transform;
                t.rotation = Quaternion.AngleAxis(Mathf.Lerp(0.0f, 360.0f, seconds / WeatherMakerDayNightCycleProfileScript.SecondsPerDay), SkySphereProfile.RotateAxis);
                Shader.SetGlobalVector(WMS._WeatherMakerSkyRotation, new Vector4(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w));
            }
        }

        private void CameraPreCull(Camera camera)
        {
            if (WeatherMakerScript.ShouldIgnoreCamera(this, camera) || camera.orthographic || WeatherMakerCommandBufferManagerScript.CameraStackCount > 1)
            {
                return;
            }

            if (SkySphereProfile != null && camera != null && isActiveAndEnabled && WeatherMakerLightManagerScript.Instance != null)
            {
                SkySphereProfile.UpdateSkySphere(camera, MeshRenderer.sharedMaterial, gameObject, WeatherMakerLightManagerScript.Instance.SunPerspective);
            }
            if (SkySphereProfile.AtmosphereProfile != null)
            {
                SkySphereProfile.AtmosphereProfile.UpdateShaderVariables(camera, AtmosphereLookupMaterial, AtmosphereComputeShader);
            }
        }

        private void CameraPreRender(Camera camera)
        {
            if (WeatherMakerScript.ShouldIgnoreCamera(this, camera) || camera.orthographic || WeatherMakerCommandBufferManagerScript.CameraStackCount > 1)
            {
                return;
            }

            if (SkySphereProfile.AtmosphereProfile != null)
            {
                SkySphereProfile.AtmosphereProfile.UpdateShaderVariables(camera, AtmosphereLookupMaterial, AtmosphereComputeShader);
            }
        }

        private void CameraPostRender(Camera camera)
        {
            if (WeatherMakerScript.ShouldIgnoreCamera(this, camera) || camera.orthographic || WeatherMakerCommandBufferManagerScript.CameraStackCount > 1)
            {
                return;
            }
        }

        private static WeatherMakerSkySphereScript instance;
        /// <summary>
        /// Shared instance of sky sphere script
        /// </summary>
        public static WeatherMakerSkySphereScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}