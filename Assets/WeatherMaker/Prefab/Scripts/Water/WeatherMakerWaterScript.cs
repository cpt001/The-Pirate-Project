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

using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Water script, handles rendering Weather Maker water
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(BoxCollider))]
    public class WeatherMakerWaterScript : MonoBehaviour
    {
        /// <summary>Water Profile</summary>
        [Header("Profile")]
        [Tooltip("Water Profile")]
        public WeatherMakerWaterProfileScript WaterProfile;

        /// <summary>Underwater audio source (loops while underwater)</summary>
        [Header("Underwater")]
        [Tooltip("Underwater audio source (loops while underwater)")]
        public AudioSource UnderwaterAudioSource;

        /// <summary>Splash audio source (when entering / exiting the water)</summary>
        [Tooltip("Splash audio source (when entering / exiting the water)")]
        public AudioSource SplashAudioSource;

        /// <summary>
        /// Callback when a camera goes underwater
        /// </summary>
        public System.Action<WeatherMakerWaterScript, Camera, bool> UnderwaterCallback { get; set; }

        /// <summary>
        /// Water mesh renderer
        /// </summary>
        public MeshRenderer MeshRenderer { get; private set; }

        private readonly HashSet<Camera> underwaterCameras = new HashSet<Camera>();

        private WeatherMakerPlanarReflectionScript reflection;
        private BoxCollider waterCollider;
        private WeatherMakerDepthCameraScript depthScript;
        private bool isUnderwater;
        private Material lastWaterProfileMaterial;
        private MaterialPropertyBlock materialBlock;
        private WeatherMakerShaderPropertiesScript shaderProps;

        private void UpdateProfile()
        {
            if (WaterProfile != null && WaterProfile.Material != null && WaterProfile.Material != lastWaterProfileMaterial)
            {
                lastWaterProfileMaterial = WaterProfile.Material;
                MeshRenderer.sharedMaterial = WaterProfile.Material;
                if (depthScript != null)
                {
                    depthScript.Dirty = true;
                }
            }
        }

        private void OnEnable()
        {
            materialBlock = new MaterialPropertyBlock();
            shaderProps = new WeatherMakerShaderPropertiesScript(materialBlock);
            MeshRenderer = GetComponent<MeshRenderer>();
            reflection = GetComponent<WeatherMakerPlanarReflectionScript>();
            waterCollider = GetComponent<BoxCollider>();
            depthScript = GetComponent<WeatherMakerDepthCameraScript>();
            if (depthScript != null)
            {
                depthScript.Renderer = MeshRenderer;
            }
            UpdateProfile();
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
            if (WaterProfile != null)
            {
                foreach (AudioClip clip in WaterProfile.SplashAudioClips)
                {
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                    }
                }
            }
        }

        private void OnDisable()
        {
            underwaterCameras.Clear();
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
        }

        private void Update()
        {
            UpdateDepthScript();
            UpdateShader();
        }

        private void PrecomputeWaves(int waveName, int wavePrecomputeName, int waveParam1Name)
        {
            Vector4 waveVar = MeshRenderer.sharedMaterial.GetVector(waveName);
            Vector4 waveParam1 = MeshRenderer.sharedMaterial.GetVector(waveParam1Name);
            float speed = waveParam1.x;
            waveVar.x = 2.0f * (Mathf.PI / waveVar.w); //float k = 2 * UNITY_PI / wavelength;
            waveVar.y = Mathf.Sqrt(9.8f / waveVar.x) * speed * Time.timeSinceLevelLoad; //float c = sqrt(9.8 / k);
            waveVar.z = waveVar.z / waveVar.x; //float a = steepness / k;
            waveVar.w = 0.0f;
            materialBlock.SetVector(wavePrecomputeName, waveVar);
        }

        private void UpdateShader()
        {
            if (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerLightManagerScript.Instance == null || WaterProfile == null || MeshRenderer.sharedMaterial == null)
            {
                return;
            }

            UpdateProfile();

            if (isUnderwater)
            {
                // bug in Unity can't set via material property block the are ignored
                //materialBlock.SetFloat(WMS._Cull, (float)UnityEngine.Rendering.CullMode.Front);
                //materialBlock.SetFloat(WMS._ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                //materialBlock.SetFloat(WMS._ZWrite, 0);

                MeshRenderer.sharedMaterial.SetFloat(WMS._Cull, (float)UnityEngine.Rendering.CullMode.Front);
                MeshRenderer.sharedMaterial.SetFloat(WMS._ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                MeshRenderer.sharedMaterial.SetFloat(WMS._ZWrite, 0);
            }
            else
            {
                // bug in Unity can't set via material property block the are ignored
                //materialBlock.SetFloat(WMS._Cull, (float)UnityEngine.Rendering.CullMode.Back);
                //materialBlock.SetFloat(WMS._ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                //materialBlock.SetFloat(WMS._ZWrite, 1);
                MeshRenderer.sharedMaterial.SetFloat(WMS._Cull, (float)UnityEngine.Rendering.CullMode.Back);
                MeshRenderer.sharedMaterial.SetFloat(WMS._ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);

                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Metal)
                {
                    MeshRenderer.sharedMaterial.SetFloat(WMS._ZWrite, 1);
                }
                else
                {
                    // zwrite causes corruption on metal
                    MeshRenderer.sharedMaterial.SetFloat(WMS._ZWrite, 0);
                }
            }

            MeshRenderer.GetPropertyBlock(materialBlock);
            materialBlock.SetFloat(WMS._WaterDepthThreshold, (WaterProfile.WaterDepthThreshold <= 0.0f ? float.MinValue : WaterProfile.WaterDepthThreshold / transform.lossyScale.y));
            materialBlock.SetFloat(WMS._WaterUnderwater, (isUnderwater ? 1 : 0));
            if (reflection != null && reflection.enabled && !isUnderwater)
            {
                materialBlock.SetFloat(WMS._WaterReflective, 1);
            }
            else
            {
                materialBlock.SetFloat(WMS._WaterReflective, 0);
            }
            if (!WeatherMakerScript.Instance.PerformanceProfile.WaterEnableRefraction)
            {
                materialBlock.SetFloat(WMS._WaterRefractionStrength, 0.0f);
            }
            if (!WeatherMakerScript.Instance.PerformanceProfile.WaterEnableCaustics)
            {
                materialBlock.SetColor(WMS._WaterCausticsTintColor, Color.clear);
            }
            if (!WeatherMakerScript.Instance.PerformanceProfile.WaterEnableFoam)
            {
                materialBlock.SetVector(WMS._WaterFoamParam1, Vector4.zero);
            }

            materialBlock.SetFloat(WMS._SrcBlendMode, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materialBlock.SetFloat(WMS._DstBlendMode, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materialBlock.SetVector(WMS._WeatherMakerFogBoxMax, transform.position);
            if (depthScript != null)
            {
                float b = (0.5f * (depthScript.autoOrthographicSize / depthScript.OrthographicSize)) - 0.5f;
                materialBlock.SetVector(WMS._WeatherMakerWaterDepthMaxUV, new Vector4(-b, 1.0f + b, 0.0f, 0.0f));
            }

            PrecomputeWaves(WMS._WaterWave1, WMS._WaterWave1_Precompute, WMS._WaterWave1_Params1);
            PrecomputeWaves(WMS._WaterWave2, WMS._WaterWave2_Precompute, WMS._WaterWave2_Params1);
            PrecomputeWaves(WMS._WaterWave3, WMS._WaterWave3_Precompute, WMS._WaterWave3_Params1);
            PrecomputeWaves(WMS._WaterWave4, WMS._WaterWave4_Precompute, WMS._WaterWave4_Params1);
            PrecomputeWaves(WMS._WaterWave5, WMS._WaterWave5_Precompute, WMS._WaterWave5_Params1);
            PrecomputeWaves(WMS._WaterWave6, WMS._WaterWave6_Precompute, WMS._WaterWave6_Params1);
            PrecomputeWaves(WMS._WaterWave7, WMS._WaterWave7_Precompute, WMS._WaterWave7_Params1);
            PrecomputeWaves(WMS._WaterWave8, WMS._WaterWave8_Precompute, WMS._WaterWave8_Params1);

            if (WaterProfile == null || !WaterProfile.WindAffectsWaves || WeatherMakerWindScript.Instance == null || WeatherMakerWindScript.Instance.WindProfile == null ||
                WeatherMakerWindScript.Instance.WindZone == null)
            {
                materialBlock.SetFloat(WMS._WaterWaveMultiplier, 1.0f);
            }
            else
            {
                materialBlock.SetFloat(WMS._WaterWaveMultiplier, WeatherMakerWindScript.Instance.WindZone.windMain * 5.0f);
            }
            WeatherMakerLightManagerScript.Instance.UpdateShaderVariables(Camera.current, shaderProps, waterCollider);
            MeshRenderer.SetPropertyBlock(materialBlock);

            if (reflection != null)
            {
                reflection.enabled = !isUnderwater && reflection.ReflectionMask != 0 && (WeatherMakerScript.Instance == null || WeatherMakerScript.Instance.PerformanceProfile.ReflectionTextureSize >= 128);
            }
        }

        private void UpdateDepthScript()
        {
            if (depthScript != null)
            {
                Bounds bounds = MeshRenderer.bounds;
                depthScript.autoOrthographicSize = bounds.size.z * 0.5f;
                if (depthScript.AutoBounds)
                {
                    depthScript.OrthographicSize = depthScript.autoOrthographicSize;
                }
                depthScript.AspectRatio = bounds.size.x / bounds.size.z;
            }
        }

        private void PlaySplashSound()
        {
            if (SplashAudioSource != null && WaterProfile != null && WaterProfile.SplashAudioClips != null && WaterProfile.SplashAudioClips.Length != 0)
            {
                AudioClip clip = WaterProfile.SplashAudioClips[WeatherMakerRandomizer.Unity.Random(0, WaterProfile.SplashAudioClips.Length - 1)];
                if (clip != null)
                {
                    SplashAudioSource.clip = clip;
                    SplashAudioSource.PlayDelayed(0.01f);
                }
            }
        }

        private void PlayUnderwaterSound()
        {
            if (UnderwaterAudioSource != null && WaterProfile != null && WaterProfile.UnderwaterAudioClip != null)
            {
                UnderwaterAudioSource.clip = WaterProfile.UnderwaterAudioClip;
                UnderwaterAudioSource.PlayDelayed(1.0f);
            }
        }

        private bool ShouldProcessCamera(Camera camera)
        {
            return (camera != null && waterCollider != null && waterCollider.enabled &&
                !WeatherMakerScript.ShouldIgnoreCamera(this, camera) && WeatherMakerScript.IsLocalPlayer(camera.transform));
        }

        private void CameraPreCull(Camera camera)
        {
            if (!Application.isPlaying || !ShouldProcessCamera(camera))
            {
                return;
            }

            bool intersectsCamera = false;
            bool hasWaterNullZone = false;
            if (WeatherMakerScript.IsLocalPlayer(camera.transform))
            {
                int hits = Physics.OverlapSphereNonAlloc(camera.transform.position, 0.001f, WeatherMakerScript.tempColliders);
                for (int i = 0; i < hits; i++)
                {
                    if (WeatherMakerScript.tempColliders[i] == waterCollider)
                    {
                        intersectsCamera = true;
                    }
                    else
                    {
                        WeatherMakerNullZoneScript script = WeatherMakerScript.tempColliders[i].GetComponent<WeatherMakerNullZoneScript>();
                        hasWaterNullZone |= (script != null && script.CurrentState != null && (script.CurrentState.RenderMask & NullZoneRenderMask.Water) != NullZoneRenderMask.Water);
                    }
                }
            }

            if (intersectsCamera && !hasWaterNullZone)
            {
                if (underwaterCameras.Add(camera))
                {
                    // transition to being underwater
                    //Debug.Log("Went underwater: " + camera.name);
                    PlayUnderwaterSound();
                    PlaySplashSound();
                    if (UnderwaterCallback != null)
                    {
                        UnderwaterCallback.Invoke(this, camera, true);
                    }

                    // TODO: Activate any post processing volume
                }

                // render surface after precipitation and other alpha effects if under water
                MeshRenderer.sharedMaterial.renderQueue = 3000;
                isUnderwater = true;

                // reduce sun light as camera goes further down in the water
                if (WeatherMakerLightManagerScript.Instance != null)
                {
                    float density = MeshRenderer.sharedMaterial.GetFloat(WMS._WaterFogDensity);
                    float depth = Mathf.Max(0.0f, transform.position.y - camera.transform.position.y);
                    float atten = (1.0f / (density * density * depth * depth));
                    atten = Mathf.Clamp(atten, 0.0f, 1.0f);
                    WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers["WeatherMakerWaterScript" + GetInstanceID()] = atten;
                }
            }
            else
            {
                if (underwaterCameras.Contains(camera))
                {
                    // transition away from being underwater
                    //Debug.Log("No longer underwater: " + camera.name);
                    underwaterCameras.Remove(camera);
                    if (UnderwaterAudioSource != null)
                    {
                        UnderwaterAudioSource.Stop();
                    }
                    PlaySplashSound();
                    if (UnderwaterCallback != null)
                    {
                        UnderwaterCallback.Invoke(this, camera, false);
                    }
                    if (WeatherMakerLightManagerScript.Instance != null)
                    {
                        WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers.Remove("WeatherMakerWaterScript" + GetInstanceID());
                    }

                    // TODO: Deactivate any post processing volume
                }

#if UNITY_URP

                // can't use alpha test trick, so just render just before transparent
                MeshRenderer.sharedMaterial.renderQueue = 2998;

#else

                // default render queue if above water
                MeshRenderer.sharedMaterial.renderQueue = -1;

#endif

                isUnderwater = false;
            }
        }
    }
}