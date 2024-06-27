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
using System.Collections;
using System;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Legacy cloud 2D script
    /// </summary>
    public class WeatherMakerLegacyCloudScript2D : MonoBehaviour
    {
        /// <summary>Material for the clouds</summary>
        [Header("Rendering")]
        [Tooltip("Material for the clouds")]
        public Material Material;
        private Material sharedMaterial;
        private Material lastMaterial;

        /// <summary>Override the texture for the material</summary>
        [Tooltip("Override the texture for the material")]
        public Texture2D MaterialTexture;

        /// <summary>Override the mask texture for the material</summary>
        [Tooltip("Override the mask texture for the material")]
        public Texture2D MaterialMaskTexture;

        /// <summary>Tint color for the clouds</summary>
        [Tooltip("Tint color for the clouds")]
        public Color TintColor = Color.white;

        /// <summary>Number of rows in the cloud material</summary>
        [Range(1, 16)]
        [Tooltip("Number of rows in the cloud material")]
        public int MaterialRows = 1;

        /// <summary>Number of columns in the cloud material</summary>
        [Range(1, 16)]
        [Tooltip("Number of columns in the cloud material")]
        public int MaterialColumns = 1;

        /// <summary>Whether to anchor the clouds to the Anchor (i.e. main camera) or not</summary>
        [Header("Positioning")]
        [Tooltip("Whether to anchor the clouds to the Anchor (i.e. main camera) or not")]
        public bool AnchorClouds = true;

        /// <summary>Offset from the anchor that the clouds should position themselves at</summary>
        [Tooltip("Offset from the anchor that the clouds should position themselves at")]
        public Vector3 AnchorOffset = new Vector3(0.0f, -700.0f, 0.0f);

        /// <summary>The total number of clouds to create</summary>
        [Header("Count")]
        [Range(0, 16250)]
        [Tooltip("The total number of clouds to create")]
        public int NumberOfClouds = 1000;

        private ParticleSystem cloudParticleSystem;
        private Renderer cloudParticleSystemRenderer;
        private bool wasPlaying;
        private Vector2 startMinMaxSize;

        private void UpdateParticleSystem()
        {
            var m = cloudParticleSystem.main;
            m.maxParticles = NumberOfClouds;
            var anim = cloudParticleSystem.textureSheetAnimation;
            anim.numTilesX = MaterialColumns;
            anim.numTilesY = MaterialRows;
            cloudParticleSystemRenderer.sharedMaterial.mainTexture = MaterialTexture;
            cloudParticleSystemRenderer.sharedMaterial.SetColor(WMS._TintColor, TintColor);
            cloudParticleSystemRenderer.sharedMaterial.EnableKeyword("ORTHOGRAPHIC_MODE");
            if (WeatherMakerScript.Instance.AllowCameras != null && WeatherMakerScript.Instance.AllowCameras.Count != 0)
            {
                Camera camera = WeatherMakerScript.Instance.AllowCameras[0];
                var size = m.startSize;
                size.constantMin = startMinMaxSize.x * (camera.orthographicSize * 0.25f);
                size.constantMax = startMinMaxSize.y * (camera.orthographicSize * 0.25f);
                m.startSize = size;
            }
        }

        private void UpdateTransform()
        {
            if (WeatherMakerScript.Instance.AllowCameras != null && WeatherMakerScript.Instance.AllowCameras.Count != 0 && AnchorClouds)
            {
                Camera camera = WeatherMakerScript.Instance.AllowCameras[0];
                Vector3 pos = camera.transform.position;
                Vector3 curPos = gameObject.transform.position;
                curPos.x = pos.x + AnchorOffset.x;
                curPos.y = AnchorOffset.y * (camera.orthographicSize * 0.25f);
                curPos.z = (camera.orthographic ? curPos.z : pos.z + AnchorOffset.z);
                gameObject.transform.position = curPos;
            }
        }

        private void UpdateMaterial()
        {
            if (Material != lastMaterial)
            {
                Renderer renderer = GetComponent<Renderer>();
                sharedMaterial = (Material == null ? null : new Material(Material));
                if (renderer != null)
                {
                    renderer.sharedMaterial = sharedMaterial;
                }
                lastMaterial = Material;
            }
            if (sharedMaterial != null)
            {
                sharedMaterial.mainTexture = (MaterialTexture == null ? sharedMaterial.mainTexture : MaterialTexture);
                sharedMaterial.SetTexture(WMS._MaskTex, MaterialMaskTexture);
                sharedMaterial.SetColor(WMS._TintColor, TintColor);
            }
        }

        private void Awake()
        {
            cloudParticleSystem = GetComponentInChildren<ParticleSystem>();
            cloudParticleSystemRenderer = cloudParticleSystem.GetComponent<Renderer>();
            cloudParticleSystem.Stop();
        }

        private void Start()
        {
            startMinMaxSize = new Vector2(cloudParticleSystem.main.startSize.constantMin, cloudParticleSystem.main.startSize.constantMax);
            UpdateMaterial();
            UpdateTransform();
            UpdateParticleSystem();
        }

        private void Update()
        {
            UpdateMaterial();
            UpdateTransform();
            UpdateParticleSystem();
            wasPlaying = cloudParticleSystem.isPlaying;
        }

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);

            if (cloudParticleSystem != null && wasPlaying)
            {
                cloudParticleSystem.Play();
            }
        }

        private void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// CreateClouds
        /// </summary>
        public void CreateClouds()
        {
            cloudParticleSystem.Play();
        }

        /// <summary>
        /// RemoveClouds
        /// </summary>
        public void RemoveClouds()
        {
            cloudParticleSystem.Stop();
            wasPlaying = false;
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            cloudParticleSystem.Stop();
            cloudParticleSystem.Clear();
            wasPlaying = false;
        }

        private static WeatherMakerLegacyCloudScript2D instance;
        /// <summary>
        /// Shared instance of full screen clouds script
        /// </summary>
        public static WeatherMakerLegacyCloudScript2D Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}