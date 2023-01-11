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

using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.Rendering;

// #define COPY_FULL_DEPTH_TEXTURE

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Weather Maker master script
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(WeatherMakerCommandBufferManagerScript))]
    public class WeatherMakerScript : MonoBehaviour, IWeatherMakerProvider
    {
        /// <summary>Whether the prefab should exist forever. Set to false to have the prefab destroyed with the scene it is in.</summary>
        [Header("Setup")]
        [Tooltip("Whether the prefab should exist forever. Set to false to have the prefab destroyed with the scene it is in.")]
        public bool IsPermanent = true;

        /// <summary>Camera mode. This affects lighting, visual effects and more. Ensure this is set to the correct value for your game or app.</summary>
        [Tooltip("Camera mode. This affects lighting, visual effects and more. Ensure this is set to the correct value for your game or app.")]
        public CameraMode CameraType;

        /// <summary>Allowe cameras. Only cameras in this list are rendered. Defaults to main camera. The first object in the list is the primary camera, and is used to determine orthographic vs perspective setup, etc.</summary>
        [Tooltip("Allowed cameras. Only cameras in this list are rendered. Defaults to main camera. The first object in the list is the primary camera, " +
            "and is used to determine orthographic vs perspective setup, etc.")]
        public List<Camera> AllowCameras = new List<Camera>();

        /// <summary>Additional set of names for allowed cameras, useful for plugins that clone or add custom cameras that you want to allow. Names must match exactly.</summary>
        [Tooltip("Additional set of names for allowed cameras, useful for plugins that clone or add custom cameras that you want to allow. Names must match exactly.")]
        public List<string> AllowCamerasNames = new List<string>();

        /// <summary>Additional set of names for allowed cameras, useful for plugins that clone or add custom cameras that you want to allow. Names can partially match.</summary>
        [Tooltip("Additional set of names for allowed cameras, useful for plugins that clone or add custom cameras that you want to allow. Names can partially match.")]
        public List<string> AllowCamerasNamesPartial = new List<string>();

        /// <summary>Whether to auto-find all cameras tagged as MainCamera and add them to the AllowCameras list. Set to false if you don't need this or see any performance issue.</summary>
        [Tooltip("Whether to auto-find all cameras tagged as MainCamera and add them to the AllowCameras list. Set to false if you don't need this or see any performance issue.")]
        public bool AutoFindMainCamera = true;

#if UNITY_EDITOR

        /// <summary>Allow the scene camera to render Weather Maker. This can result in UI corruption due to Unity bugs, so turn off if you see UI corruption.</summary>
        [Tooltip("Allow the scene camera to render Weather Maker. This can result in UI corruption due to Unity bugs, so turn off if you see UI corruption.")]
        public bool AllowSceneCamera = true;

        /// <summary>Whether to auto-clone profiles when they change. Set to false to directly edit original profiles. Be careful if this is false - you can accidently overwrite changes to your profile at runtime.</summary>
        [Tooltip("Whether to auto-clone profiles when they change. Set to false to directly edit original profiles. Be careful if this is false - you can accidently overwrite changes to your profile at runtime.")]
        [Header("Profile Cloning")]
        public bool AutoCloneProfiles = true;

#endif

        /// <summary>Put all Weather Maker references in here including scriptable object profiles, sound files, textures, etc. Only these will be bundled and included in the final build.</summary>
        [Header("References")]
        [Tooltip("Put all Weather Maker references in here including scriptable object profiles, sound files, textures, etc. Only these will be bundled and included in the final build.")]
        public WeatherMakerResourceContainerScript ResourceContainer;

        /// <summary>The performance profile to use. If null, this will be populated automatically according to Unity quality setting.</summary>
        [Header("Performance")]
        [Tooltip("The performance profile to use. If null, this will be populated automatically according to Unity quality setting.")]
        [SerializeField]
        private WeatherMakerPerformanceProfileScript _PerformanceProfile;
        private int lastQualityLevel = -1;

        /// <summary>Whether to auto-set the performance profile based on Unity quality level</summary>
        [Tooltip("Whether to auto-set the performance profile based on Unity quality level")]
        public bool AutoSetPerformanceProfile = true;

        /// <summary>Executes when the weather profile changes for the local player. Parameter is WeatherMakerProfileScript.</summary>
        [Header("Events")]
        [Tooltip("Executes when the weather profile changes for the local player. Parameter is WeatherMakerProfileScript.")]
        public WeatherMakerEvent WeatherProfileChanged;

        /// <summary>Executes when the weather zones changes for the local player. Parameter is WeatherMakerWeatherZoneScript.</summary>
        [Tooltip("Executes when the weather zones changes for the local player. Parameter is WeatherMakerWeatherZoneScript.")]
        public WeatherMakerEvent WeatherZoneChanged;

        /// <summary>Executes when the day/night cycle year changes. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle year changes. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent YearChanged;

        /// <summary>Executes when the day/night cycle month changes. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle month changes. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent MonthChanged;

        /// <summary>Executes when the day/night cycle day changes. Parameter is DateTWeatherMakerDayNightCycleProfileScriptime.</summary>
        [Tooltip("Executes when the day/night cycle day changes. Parameter is DateTWeatherMakerDayNightCycleProfileScriptime.")]
        public WeatherMakerEvent DayChanged;

        /// <summary>Executes when the day/night cycle hour changes. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle hour changes. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent HourChanged;

        /// <summary>Executes when the day/night cycle minute changes. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle minute changes. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent MinuteChanged;

        /// <summary>Executes when the day/night cycle second changes. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle second changes. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent SecondChanged;

        /// <summary>Executes when the day/night cycle becomes night. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle becomes night. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent NightBegin;

        /// <summary>Executes when the day/night cycle becomes day. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle becomes day. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent DayBegin;

        /// <summary>Executes when the day/night cycle becomes dawn. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle becomes dawn. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent DawnBegin;

        /// <summary>Executes when the day/night cycle becomes dusk. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle becomes dusk. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent DuskBegin;

        /// <summary>Executes when the day/night cycle sunrise begins. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle sunrise begins. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent SunriseBegin;

        /// <summary>Executes when the day/night cycle sunrise ends. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle sunrise ends. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent SunriseEnd;

        /// <summary>Executes when the day/night cycle sunset begins. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle sunset begins. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent SunsetBegin;

        /// <summary>Executes when the day/night cycle sunset ends. Parameter is WeatherMakerDayNightCycleProfileScript.</summary>
        [Tooltip("Executes when the day/night cycle sunset ends. Parameter is WeatherMakerDayNightCycleProfileScript.")]
        public WeatherMakerEvent SunsetEnd;

        /// <summary>
        /// Main camera
        /// </summary>
        public Camera MainCamera { get { return (AllowCameras == null || AllowCameras.Count == 0 ? Camera.main : AllowCameras[0]); } }

        /// <summary>
        /// Get the performance profile or default profile if null
        /// </summary>
        public WeatherMakerPerformanceProfileScript PerformanceProfile
        {
            get { return _PerformanceProfile ?? defaultProfile ?? (defaultProfile = LoadResource<WeatherMakerPerformanceProfileScript>("WeatherMakerPerformanceProfile_Default")); }
        }
        private WeatherMakerPerformanceProfileScript defaultProfile;

        private WeatherMakerProfileScript lastLocalProfile;
        /// <summary>
        /// Last local profile that was set for the local player - do not try to set this value, it will have no effect, use a weather zone instead.
        /// </summary>
        public WeatherMakerProfileScript LastLocalProfile
        {
            get { return lastLocalProfile; }
            set
            {
                PreviousLastLocalProfile = lastLocalProfile;
                if (value != lastLocalProfile)
                {
                    lastLocalProfile = value;
                    WeatherProfileChanged.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Previous last local profile that was set for the local player - do not try to set this value, it will have no effect, use a weather zone instead.
        /// </summary>
        public WeatherMakerProfileScript PreviousLastLocalProfile { get; set; }

        private WeatherMakerCommandBufferManagerScript commandBufferManager;
        /// <summary>
        /// Command buffer manager
        /// </summary>
        public WeatherMakerCommandBufferManagerScript CommandBufferManager { get { return commandBufferManager; } }

        /// <summary>
        /// Precipitation manager
        /// </summary>
        public IPrecipitationManager PrecipitationManager { get; set; }

        /// <summary>
        /// Cloud manager
        /// </summary>
        public ICloudManager CloudManager { get; set; }

        /// <summary>
        /// Sky manager
        /// </summary>
        public ISkyManager SkyManager { get; set; }

        /// <summary>
        /// Aurora manager
        /// </summary>
        public IAuroraManager AuroraManager { get; set; }

        /// <summary>
        /// Fog manager
        /// </summary>
        public IFogManager FogManager { get; set; }

        /// <summary>
        /// Wind manager
        /// </summary>
        public IWindManager WindManager { get; set; }

        /// <summary>
        /// Thunder and lightning manager
        /// </summary>
        public IThunderAndLightningManager ThunderAndLightningManager { get; set; }

        /// <summary>
        /// Player sound manager
        /// </summary>
        public IPlayerSoundManager PlayerSoundManager { get; set; }

        private IWeatherMakerNetworkConnection networkConection = new WeatherMakerNoNetworkConnection();
        /// <summary>
        /// Network connection info
        /// </summary>
        public IWeatherMakerNetworkConnection NetworkConnection
        {
            get { return networkConection; }
            set
            {
                value = (value == null ? new WeatherMakerNoNetworkConnection() : value);
                networkConection = value;
            }
        }

        /// <summary>
        /// Event that fires when the weather profile changes (old, new, transition duration, connection ids (null for all connections))
        /// </summary>
        public event System.Action<WeatherMakerProfileScript, WeatherMakerProfileScript, float, string[]> WeatherProfileChangedEvent;

        /// <summary>
        /// Whether we have had a weather transition, if not first transition is instant
        /// </summary>
        public bool HasHadWeatherTransition { get; set; }

        /// <summary>
        /// Allows adding additional weather intensity modifiers by key. Note that setting any values in here will override the external intensity
        /// on any built in weather maker particle system scripts.
        /// </summary>
        [System.NonSerialized]
        public readonly System.Collections.Generic.Dictionary<string, float> IntensityModifierDictionary = new System.Collections.Generic.Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);

        private readonly System.Collections.Generic.List<System.Action> mainThreadActions = new System.Collections.Generic.List<System.Action>();

        /// <summary>
        /// Internal array for non-alloc physics operation
        /// </summary>
        internal static readonly Collider[] tempColliders = new Collider[16];

        private static readonly string[] cameraStringsForReflection = new string[] { "mirror", "water", "refl" };
        private static readonly string[] cameraStringsForCubeMap = new string[] { "probe" };
        private static readonly string[] cameraStringsForOther = new string[] { "prerender", "depthbuffer", "effects", "preview" };
        private static readonly Dictionary<string, WeatherMakerCameraType> cameraTypes = new Dictionary<string, WeatherMakerCameraType>(System.StringComparer.OrdinalIgnoreCase);

        private float mainCameraCheck = 1.0f;

        private void UpdateMainThreadActions()
        {
            lock (mainThreadActions)
            {
                foreach (System.Action action in mainThreadActions)
                {
                    action();
                }
                mainThreadActions.Clear();
            }
        }

        private void SetGlobalShaderProperties()
        {
            if (PerformanceProfile.EnablePerPixelLighting && SystemInfo.graphicsShaderLevel >= 30)
            {
                Shader.SetGlobalInt(WMS._WeatherMakerPerPixelLighting, 1);
            }
            else
            {
                Shader.SetGlobalInt(WMS._WeatherMakerPerPixelLighting, 0);
            }

            // global fog params, individual material can override
            // useful for hacks like the tree billboard shader override that account for full screen fog
            // LateUpdate of other scripts can override
            Shader.SetGlobalInt(WMS._WeatherMakerFogMode, 0);
            Shader.SetGlobalFloat(WMS._WeatherMakerFogDensity, 0.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerFogFactorMax, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerEnableToneMapping, PerformanceProfile.EnableTonemap ? 1.0f : 0.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerFogGlobalShadow, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerCloudVolumetricShadow, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow2, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerDirectionalLightScatterMultiplier, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerCloudAtmosphereShadow, 1.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerVREnabled, HasXRDevice() ? 1.0f : 0.0f);
            Shader.SetGlobalFloat(WMS._WeatherMakerPrecipitationLightMultiplier, 2.5f);

#if UNITY_URP

            Shader.EnableKeyword("UNITY_URP");

#else

            Shader.DisableKeyword("UNITY_URP");

#endif

        }

        private void CheckURP()
        {

#if UNITY_URP && UNITY_EDITOR && UNITY_2021_1_OR_NEWER

            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                return;
            }

            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
            string[] guids = UnityEditor.AssetDatabase.FindAssets("WeatherMakerURPProfile");
            if (guids.Length != 0)
            {
                UnityEngine.Rendering.RenderPipelineAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                if (asset != null)
                {
                    UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = asset;
                    UnityEditor.EditorUtility.DisplayDialog("Success", "Weather Maker URP is enabled. Double check WeatherMaker/Prefab/ScriptableRenderPipeline/URP/DemoSceneURP.", "OK");
                }
            }

#endif

        }

        private void CheckRequiredComponents()
        {

#if UNITY_EDITOR

            if (WeatherMakerLightManagerScript.Instance == null || !WeatherMakerLightManagerScript.Instance.isActiveAndEnabled)
            {
                Debug.LogError("Missing or deactivated light manager script, this script is required for correct functionality.");
            }
            if (WeatherMakerCommandBufferManagerScript.Instance == null || !WeatherMakerCommandBufferManagerScript.Instance.isActiveAndEnabled)
            {
                Debug.LogError("Missing or deactivated command buffer manager script, this script is required for correct functionality.");
            }
            if (WeatherMakerDayNightCycleManagerScript.Instance == null || !WeatherMakerDayNightCycleManagerScript.Instance.isActiveAndEnabled)
            {
                Debug.LogError("Missing or deactivated day night cycle manager script, this script is required for correct functionality. To disable dynamic time of day, set the day night profile speeds to 0.");
            }
            if (WeatherMakerSkySphereScript.Instance == null || !WeatherMakerSkySphereScript.Instance.isActiveAndEnabled)
            {
                if (WeatherMakerSkyPlaneScript.Instance == null || !WeatherMakerSkyPlaneScript.Instance.isActiveAndEnabled)
                {
                    Debug.LogError("Missing or deactivated sky sphere or sky plane script, this script is required for correct functionality. You can disable just the mesh renderer on the sky sphere or sky plane to disable the rendering.");
                }
            }

#endif

        }

        private void CheckPerformanceProfile()
        {
            int qualityLevel = UnityEngine.QualitySettings.GetQualityLevel();
            if (lastQualityLevel == -1)
            {
                lastQualityLevel = qualityLevel;
            }

#if UNITY_2021_1_OR_NEWER

            if (UnityEngine.QualitySettings.renderPipeline is null)
            {
                UnityEngine.QualitySettings.renderPipeline = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
            }

#endif

            if (Application.isPlaying && (_PerformanceProfile == null || (AutoSetPerformanceProfile && lastQualityLevel != qualityLevel)))
            {

#if DEBUG

                if (QualitySettings.names != null &&
                    (QualitySettings.names.Length < 3 || QualitySettings.names.Length > 6))
                {
                    Debug.LogErrorFormat("Weather Maker auto-performance profile expects between 3 to 6 quality settings, but you have {0}. Please click stop and set a performance profile manually on WeatherMakerScript, and set 'Auto Set Performance Profile' to false.", QualitySettings.names.Length);
                }

#endif

                string name = "WeatherMakerPerformanceProfile_Default";
                if (HasXRDevice())
                {
                    if (QualitySettings.names.Length == 3)
                    {
                        switch (qualityLevel)
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_VR_Fastest";
                                break;

                            case 1:
                                name = "WeatherMakerPerformanceProfile_VR_Fast";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_VR";
                                break;
                        }
                    }
                    else if (QualitySettings.names.Length == 4)
                    {
                        switch (qualityLevel)
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_VR_Fastest";
                                break;

                            case 1:
                            case 2:
                                name = "WeatherMakerPerformanceProfile_VR_Fast";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_VR";
                                break;
                        }
                    }
                    else if (QualitySettings.names.Length == 5 || QualitySettings.names.Length == 6)
                    {
                        switch (qualityLevel)
                        {
                            case 0:
                            case 1:
                                name = "WeatherMakerPerformanceProfile_VR_Fastest";
                                break;

                            case 2:
                            case 3:
                                name = "WeatherMakerPerformanceProfile_VR_Fast";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_VR";
                                break;
                        }
                    }
                }
                else
                {
                    if (QualitySettings.names.Length == 3)
                    {
                        switch (UnityEngine.QualitySettings.GetQualityLevel())
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_Fastest";
                                break;

                            case 1:
                                name = "WeatherMakerPerformanceProfile_Good";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_Fantastic";
                                break;
                        }
                    }
                    else if (QualitySettings.names.Length == 4)
                    {
                        switch (UnityEngine.QualitySettings.GetQualityLevel())
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_Fastest";
                                break;

                            case 1:
                                name = "WeatherMakerPerformanceProfile_Fast";
                                break;

                            case 2:
                                name = "WeatherMakerPerformanceProfile_Good";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_Fantastic";
                                break;
                        }
                    }
                    else if (QualitySettings.names.Length == 5)
                    {
                        switch (UnityEngine.QualitySettings.GetQualityLevel())
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_Fastest";
                                break;

                            case 1:
                                name = "WeatherMakerPerformanceProfile_Fast";
                                break;

                            case 2:
                                name = "WeatherMakerPerformanceProfile_Good";
                                break;

                            case 3:
                                name = "WeatherMakerPerformanceProfile_Beautiful";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_Fantastic";
                                break;
                        }
                    }
                    else if (QualitySettings.names.Length == 6)
                    {
                        switch (UnityEngine.QualitySettings.GetQualityLevel())
                        {
                            case 0:
                                name = "WeatherMakerPerformanceProfile_Fastest";
                                break;

                            case 1:
                                name = "WeatherMakerPerformanceProfile_Fast";
                                break;

                            case 2:
                                name = "WeatherMakerPerformanceProfile_Simple";
                                break;

                            case 3:
                                name = "WeatherMakerPerformanceProfile_Good";
                                break;

                            case 4:
                                name = "WeatherMakerPerformanceProfile_Beautiful";
                                break;

                            default:
                                name = "WeatherMakerPerformanceProfile_Fantastic";
                                break;
                        }
                    }
                }
                _PerformanceProfile = LoadResource<WeatherMakerPerformanceProfileScript>(name);
                if (_PerformanceProfile == null)
                {
                    Debug.LogError("Unable to auto-load performance profile with name " + name);
                }
            }
            if (lastQualityLevel != qualityLevel)
            {
                lastQualityLevel = qualityLevel;
                if (WeatherMakerFullScreenCloudsScript.Instance != null)
                {
                    WeatherMakerFullScreenCloudsScript.Instance.ForceReload();
                }
            }
        }

        private void Awake()
        {
            WMS.Initialize();
            if (ResourceContainer == null)
            {
                Debug.LogError("Please ensure you have set a resource container on the WeatherMakerScript, this is required to load Weather Maker resources properly");
            }
            commandBufferManager = GetComponentInChildren<WeatherMakerCommandBufferManagerScript>();
            if (commandBufferManager == null)
            {
                Debug.LogError("CommandBufferManager needs to be set on WeatherMakerScript");
            }
            CheckPerformanceProfile();
        }

        private void CheckForMainCamera()
        {
            if ((mainCameraCheck += Time.deltaTime) < 0.5f)
            {
                return;
            }

            mainCameraCheck = 0.0f;
            if (AutoFindMainCamera && AllowCameras != null)
            {
                GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
                foreach (GameObject obj in mainCameras)
                {
                    Camera cam = obj.GetComponent<Camera>();
                    if (cam != null && !AllowCameras.Contains(cam))
                    {
                        AllowCameras.Add(cam);
                    }
                }
            }

            if (AllowCameras != null && AllowCameras.Count == 0)
            {
                Debug.LogError("Weather Maker allow cameras list is empty, please ensure your camera(s) are added to the AllowCameras list of WeatherMakerScript");
            }
        }

        private void RemoveDestroyedCameras()
        {
            AllowCameras = (AllowCameras == null ? new List<Camera>() : AllowCameras);

            for (int i = AllowCameras.Count - 2; i >= 0; i--)
            {
                if (AllowCameras[i] == null)
                {
                    AllowCameras.RemoveAt(i);
                }
            }
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            shouldIgnoreCameraCache.Clear();
        }

        private void Update()
        {
            CheckForMainCamera();
            RemoveDestroyedCameras();
            CheckPerformanceProfile();
            SetGlobalShaderProperties();
            CheckRequiredComponents();
            CheckURP();
        }

        private void LateUpdate()
        {

#if UNITY_EDITOR

            if (transform.position != Vector3.zero || transform.localScale != Vector3.one || transform.rotation != Quaternion.identity)
            {
                Debug.LogError("For correct rendering, weather maker manager script/prefab should have position and rotation of 0, and scale of 1.");
            }

#endif

            UpdateMainThreadActions();
            UpdateExternalIntensities();
        }

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);

#if UNITY_EDITOR

            if (GameObject.FindObjectsOfType<WeatherMakerScript>().Length > 1)
            {
                Debug.LogError("Only one WeatherMakerScript should exist in your game. Use the WeatherMakerPrefab and call DontDestroyOnLoad.");
            }

#endif

            nullInstances.Clear();
            WMS.Initialize();
            PrecipitationManager = FindIfNull<IPrecipitationManager, WeatherMakerPrecipitationManagerScript>(PrecipitationManager);
            CloudManager = FindIfNull<ICloudManager, WeatherMakerCloudManagerScript>(CloudManager);
            if (CloudManager == null)
            {
                CloudManager = FindIfNull<ICloudManager, WeatherMakerCloudManager2DScript>(CloudManager);
            }
            SkyManager = FindIfNull<ISkyManager, WeatherMakerSkyManagerScript>(SkyManager);
            AuroraManager = FindIfNull<IAuroraManager, WeatherMakerAuroraManagerScript>(AuroraManager);
            FogManager = FindIfNull<IFogManager, WeatherMakerFogManagerScript>(FogManager);
            WindManager = FindIfNull<IWindManager, WeatherMakerWindManagerScript>(WindManager);
            ThunderAndLightningManager = FindIfNull<IThunderAndLightningManager, WeatherMakerThunderAndLightningManagerScript>(ThunderAndLightningManager);
            PlayerSoundManager = FindIfNull<IPlayerSoundManager, WeatherMakerPlayerSoundManagerScript>(PlayerSoundManager);

            // wire up lightning bolt lights to the light manager
            if (Application.isPlaying)
            {
                if (WeatherMakerLightManagerScript.Instance != null && WeatherMakerThunderAndLightningScript.Instance != null)
                {
                    WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightAddedCallback += LightningLightAdded;
                    WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightRemovedCallback += LightningLightRemoved;
                }
                if (transform.parent != null)
                {
                    Debug.LogError("Weather Maker prefab should not have a parent");
                }
                else if (IsPermanent)
                {
                    DontDestroyOnLoad(gameObject);
                }
                if (HasXRDevice() && Camera.main != null && Camera.main.actualRenderingPath != RenderingPath.Forward)
                {
                    Debug.LogError("Weather Maker requires the main camera to use Forward rendering path");
                }
            }
        }

        private void OnDisable()
        {
            cameraTypes.Clear();
            nullInstances.Clear();
        }

        private void OnDestroy()
        {
            TweenFactory.Clear();
            WeatherMakerObjectExtensions.Clear();

            // remove lightning bolt lights from the light manager
            if (Application.isPlaying && WeatherMakerLightManagerScript.Instance != null && WeatherMakerThunderAndLightningScript.Instance != null)
            {
                WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightAddedCallback -= LightningLightAdded;
                WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightRemovedCallback -= LightningLightRemoved;
            }

            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        private TInterface FindIfNull<TInterface, T>(TInterface value) where TInterface : class where T : UnityEngine.Component, TInterface
        {
            if (value == null)
            {
                value = gameObject.GetComponentInChildren<T>();
            }
            return value as TInterface;
        }

        private void LightningLightAdded(Light l)
        {
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                WeatherMakerLightManagerScript.Instance.AddLight(l);
            }
        }

        private void LightningLightRemoved(Light l)
        {
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                WeatherMakerLightManagerScript.Instance.RemoveLight(l);
            }
        }

        private void UpdateExternalIntensities()
        {
            if (WeatherMakerThunderAndLightningScript.Instance != null)
            {
                WeatherMakerThunderAndLightningScript.Instance.ExternalIntensityMultiplier = 1.0f;
                foreach (float multiplier in IntensityModifierDictionary.Values)
                {
                    WeatherMakerThunderAndLightningScript.Instance.ExternalIntensityMultiplier *= multiplier;
                }
            }
            if (WeatherMakerWindScript.Instance != null)
            {
                WeatherMakerWindScript.Instance.ExternalIntensityMultiplier = 1.0f;
                foreach (float multiplier in IntensityModifierDictionary.Values)
                {
                    WeatherMakerWindScript.Instance.ExternalIntensityMultiplier *= multiplier;
                }
            }
        }

        private static readonly Dictionary<Camera, bool> shouldIgnoreCameraCache = new Dictionary<Camera, bool>();

        /// <summary>
        /// Clear the ignore camera cache
        /// </summary>
        public static void ClearShouldIgnoreCameraCache()
        {
            shouldIgnoreCameraCache.Clear();
        }

        /// <summary>
        /// Determine if a camera should be rendered in weather maker
        /// </summary>
        /// <param name="script">Script</param>
        /// <param name="camera">Camera</param>
        /// <param name="ignoreReflections">Whether to ignore reflection cameras (i.e. water reflection or mirror)</param>
        /// <returns>True to ignore, false to render</returns>
        public static bool ShouldIgnoreCamera(MonoBehaviour script, Camera camera, bool ignoreReflections = true)
        {
            if (camera == null || Instance == null || script == null || !script.enabled || !script.gameObject.activeInHierarchy ||
                camera.cameraType == UnityEngine.CameraType.Preview)
            {
                return true;
            }
            else if (shouldIgnoreCameraCache.TryGetValue(camera, out bool shouldIgnoreCameraCachedValue))
            {
                return shouldIgnoreCameraCachedValue;
            }

            bool ignoreCamera;
            if ((Instance.AllowCameras.Count == 0 && Instance.AllowCamerasNames.Count == 0 && Instance.AllowCamerasNamesPartial.Count == 0) ||
                (camera.CachedName().IndexOf("depth", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                ignoreCamera = true;
            }

#if UNITY_EDITOR

            else if (camera.cameraType == UnityEngine.CameraType.SceneView)
            {
                ignoreCamera = (WeatherMakerScript.Instance == null || !WeatherMakerScript.Instance.AllowSceneCamera);
            }
            else

#endif

            {
                WeatherMakerCameraType type = WeatherMakerScript.GetCameraType(camera);

                // if camera is not in allow list and
                // camera is not an allowed reflection camera and
                // camera is not an allowed reflection probe camera
                // then ignore it
                bool notInAllowList = !Instance.AllowCameras.Contains(camera);
                bool notInAllowNameList = true;
                foreach (string s in Instance.AllowCamerasNames)
                {
                    if (camera.CachedName() == s)
                    {
                        notInAllowNameList = false;
                        break;
                    }
                }
                if (notInAllowNameList)
                {
                    foreach (string s in Instance.AllowCamerasNamesPartial)
                    {
                        if (camera.CachedName().IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            notInAllowNameList = false;
                            break;
                        }
                    }
                }
                bool allowReflection = (!ignoreReflections && type == WeatherMakerCameraType.Reflection);
                bool allowProbe = (!Instance.PerformanceProfile.IgnoreReflectionProbes && type == WeatherMakerCameraType.CubeMap);
                bool allowList = (!notInAllowList || !notInAllowNameList);
                ignoreCamera = !(allowReflection || allowProbe || allowList);
            }

            if (camera != null && camera.gameObject.activeInHierarchy && camera.enabled)
            {
                shouldIgnoreCameraCache[camera] = ignoreCamera;
                if (shouldIgnoreCameraCache.Count > 1000)
                {
                    // prevent getting too big
                    shouldIgnoreCameraCache.Clear();
                }
            }

            return ignoreCamera;
        }

        /// <summary>
        /// Get the type of a camera
        /// </summary>
        /// <param name="c">Camera</param>
        /// <returns>Camera type</returns>
        public static WeatherMakerCameraType GetCameraType(Camera c)
        {
            // cube map camera with RenderToCubemap always have aspect 1 and an activeTexture, are always game camera and have a texture named TempBuffer
            if (c.activeTexture != null && c.cameraType == UnityEngine.CameraType.Game &&
                c.activeTexture.dimension == TextureDimension.Cube)
            {
                return WeatherMakerCameraType.CubeMap;
            }

            string name = c.CachedName();

            // cache types, save string compares
            WeatherMakerCameraType type;
            if (cameraTypes.TryGetValue(name, out type))
            {
                return type;
            }

            if (c.cameraType == UnityEngine.CameraType.Reflection)
            {
                type = WeatherMakerCameraType.CubeMap;
                cameraTypes[name] = type;
                return type;
            }

            foreach (string s in cameraStringsForOther)
            {
                if (name.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    type = WeatherMakerCameraType.Other;
                    cameraTypes[name] = type;
                    return type;
                }
            }
            foreach (string s in cameraStringsForCubeMap)
            {
                if (name.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    type = WeatherMakerCameraType.CubeMap;
                    cameraTypes[name] = type;
                    return type;
                }
            }
            foreach (string s in cameraStringsForReflection)
            {
                if (name.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    type = WeatherMakerCameraType.Reflection;
                    cameraTypes[name] = type;
                    return type;
                }
            }

            type = WeatherMakerCameraType.Normal;
            cameraTypes[name] = type;
            return type;
        }

        /// <summary>
        /// Call whenever the weather profile needs to change, handles client/server, etc.
        /// If no networking or network server, this will perform the transition.
        /// WeatherProfileChanged will then be called.
        /// </summary>
        /// <param name="oldProfile">Old weather profile</param>
        /// <param name="newProfile">New weather profile</param>
        /// <param name="transitionDuration">Transition duration</param>
        /// <param name="holdDuration">Hold duration</param>
        /// <param name="forceTransition">True to force a transition, false otherwise. True means it was forced from a server or some other way.</param>
        /// <param name="connectionIds">Connection ids to send to (null for none or single player)</param>
        public void RaiseWeatherProfileChanged(WeatherMakerProfileScript oldProfile, WeatherMakerProfileScript newProfile, float transitionDuration,
            float holdDuration, bool forceTransition, string[] connectionIds)
        {
            // default behavior is to perform transition
            if (forceTransition || !NetworkConnection.IsConnected)
            {
                // if no network involved OR we are a client and we got a server update, perform the fast transition if needed
                if (!HasHadWeatherTransition)
                {
                    HasHadWeatherTransition = true;
                    transitionDuration = 0.001f;
                }
                Debug.LogFormat("Changing weather profile {0} to {1}, transition time: {2}, hold time: {3}",
                (oldProfile == null ? "None" : oldProfile.name), (newProfile == null ? "None" : newProfile.name), transitionDuration, (holdDuration <= 0.0f ? "Unknown" : holdDuration.ToString(CultureInfo.InvariantCulture)));
                newProfile.TransitionFrom(this, oldProfile, transitionDuration);
            }

            // notify listeners, if using network this should notify the network script to blast out a transition to all clients
            if (WeatherProfileChangedEvent != null)
            {
                WeatherProfileChangedEvent.Invoke(oldProfile, newProfile, transitionDuration, connectionIds);
            }
        }

        /// <summary>
        /// Queue an action to run on the main thread - this action should run as fast as possible to avoid locking the main thread.
        /// </summary>
        /// <param name="action">Action to run</param>
        public static void QueueOnMainThread(System.Action action)
        {
            if (Instance != null)
            {
                lock (Instance.mainThreadActions)
                {
                    Instance.mainThreadActions.Add(action);
                }
            }
        }

        /// <summary>
        /// Resolve camera mode if mode is Auto. Will resolve against camera or Camera.main if camera is null
        /// </summary>
        /// <param name="mode">Camera mode (null for current camera type or auto)</param>
        /// <param name="camera">Camera or null for main camera</param>
        /// <returns>Camera mode</returns>
        public static CameraMode ResolveCameraMode(CameraMode? mode = null, Camera camera = null)
        {
            camera = (camera == null ? Camera.main : camera);
            if (mode == null)
            {
                mode = (Instance == null ? CameraMode.Auto : Instance.CameraType);
            }
            if (camera == null)
            {
                if (mode.Value == CameraMode.Auto)
                {
                    return CameraMode.Perspective;
                }
                return mode.Value;
            }
            else if (mode.Value == CameraMode.OrthographicXY || (mode.Value == CameraMode.Auto && camera.orthographic))
            {
                return CameraMode.OrthographicXY;
            }
            else if (mode.Value == CameraMode.Perspective || (mode.Value == CameraMode.Auto && !camera.orthographic))
            {
                return CameraMode.Perspective;
            }
            return CameraMode.OrthographicXZ;
        }

        /// <summary>
        /// Determine if an object is a player (including network players)
        /// </summary>
        /// <param name="obj">Transform</param>
        /// <returns>True if player, false if not</returns>
        public static bool IsPlayer(Transform obj)
        {
            WeatherMakerIsPlayerScript playerScript = obj.GetComponentInParent<WeatherMakerIsPlayerScript>();
            if (playerScript != null)
            {
                return true;
            }
            return (obj.GetComponentInParent<AudioListener>() != null);
        }

        /// <summary>
        /// Get whether an object is the local player
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>True if local player, false otherwise</returns>
        public static bool IsLocalPlayer(Transform obj)
        {
            WeatherMakerIsPlayerScript playerScript = obj.GetComponentInParent<WeatherMakerIsPlayerScript>();
            if (playerScript != null)
            {
                return playerScript.IsLocalPlayer;
            }
            AudioListener listener = obj.GetComponentInParent<AudioListener>();
            return (listener != null && listener.enabled);
        }

        private static bool? hasXRDevice;
        private static bool? hasXRDeviceMultipass;

        /// <summary>
        /// Determine whether stereo rendering path is multipass
        /// </summary>
        /// <returns>True if multipass, false otherwise</returns>
        public static bool HasXRDeviceMultipass()
        {
            if (hasXRDeviceMultipass.HasValue)
            {
                return hasXRDeviceMultipass.Value;
            }
            hasXRDeviceMultipass = (HasXRDevice() && UnityEngine.XR.XRSettings.stereoRenderingMode == UnityEngine.XR.XRSettings.StereoRenderingMode.MultiPass);
            return hasXRDeviceMultipass.Value;
        }

        /// <summary>
        /// Detect if an xr device is connected
        /// </summary>
        /// <returns>True if xr device connected, false otherwise</returns>
        public static bool HasXRDevice()
        {
            if (hasXRDevice.HasValue)
            {
                return hasXRDevice.Value;
            }

            hasXRDevice = false;

#if UNITY_2020_1_OR_NEWER

            var xrDisplaySubsystems = new List<UnityEngine.XR.XRDisplaySubsystem>();
            SubsystemManager.GetInstances<UnityEngine.XR.XRDisplaySubsystem>(xrDisplaySubsystems);
            foreach (var xrDisplay in xrDisplaySubsystems)
            {
                if (xrDisplay.running)
                {
                    hasXRDevice = true;
                    break;
                }
            }

#else

            hasXRDevice = UnityEngine.XR.XRDevice.isPresent;

#endif

            return hasXRDevice.Value;
        }

        /// <summary>
        /// Retrieve a Weather Maker resources. The resource should be added to the References property ideally, or as a fallback can be in a Resources folder.
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="name">Name of resource to get</param>
        /// <returns>Resource or null if not found</returns>
        public T LoadResource<T>(string name) where T : Object
        {
            T result;
            if (ResourceContainer != null && ResourceContainer.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }

        private static readonly HashSet<System.Type> nullInstances = new HashSet<System.Type>();

        internal static bool StringContainsAny(string text, params string[] items)
        {
            foreach (string item in items)
            {
                if (text.IndexOf(item, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Find or create a singleton object. In OnDestroy, you must call ReleaseInstance to avoid memory leaks.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="instance">Singleton instance variable</param>
        /// <param name="required">Whether the instance is required</param>
        /// <returns>Result, can be null</returns>
        public static T FindOrCreateInstance<T>(ref T instance, bool required = false) where T : MonoBehaviour
        {
            if (instance == null &&
                !nullInstances.Contains(typeof(T)) &&
                !StringContainsAny(StackTraceUtility.ExtractStackTrace(), "OnDestroy", "OnDisable"))
            {
                T[] scripts = GameObject.FindObjectsOfType<T>();
                foreach (T script in scripts)
                {
                    if (script.enabled || script is WeatherMakerScript)
                    {
                        instance = script;
                        nullInstances.Remove(typeof(T));
                        break;
                    }
                }
                if (instance == null)
                {
                    if (required)
                    {
                        if (Application.isPlaying)
                        {
                            Debug.LogErrorFormat("Object of type {0} is required, please ensure it is active in the WeatherMakerPrefab", typeof(T));
                        }
                    }
                    else
                    {
                        nullInstances.Add(typeof(T));
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// Ensures that an object is the correct singleton for itself
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="instance">Singleton reference</param>
        public static void EnsureInstance<T>(MonoBehaviour obj, ref T instance) where T : MonoBehaviour
        {
            if (instance == null)
            {
                instance = obj as T;

                if (instance == null)
                {
                    Debug.LogError("Incorrect object type passed to EnsureInstance, must be of type T");
                }
            }
            else if (instance != obj)
            {
                Debug.LogErrorFormat("Multiple instances of {0} detected, this is not supported", typeof(T).FullName);
            }
        }

        /// <summary>
        /// Release an instance created with FindOrCreateInstance
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="instance">Instance to release</param>
        public static void ReleaseInstance<T>(ref T instance) where T : MonoBehaviour
        {
            instance = null;
            nullInstances.Remove(typeof(T));
        }

        /// <summary>
        /// Determine if running in headless (server only, no rendering) mode
        /// </summary>
        /// <returns>True if headless, false otherwise</returns>
        public static bool IsHeadlessMode
        {
            get { return UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null; }
        }

        /// <summary>
        /// Whether profiles should be auto-cloned to avoid accidental changes at runtime
        /// </summary>
        public static bool CloneProfiles
        {

#if UNITY_EDITOR

            get { return instance == null ? false : instance.AutoCloneProfiles; }

#else

            get { return true; }

#endif

        }

        private static WeatherMakerScript instance;
        /// <summary>
        /// Shared instance of weather maker manager script
        /// </summary>
        public static WeatherMakerScript Instance
        {
            get { return FindOrCreateInstance<WeatherMakerScript>(ref instance, true); }
        }

        /// <summary>
        /// Check if an instance exists
        /// </summary>
        /// <returns>True if instance exists, false otherwise</returns>
        public static bool HasInstance()
        {
            return instance != null;
        }
    }

    /// <summary>
    /// Interface for all weather maker managers
    /// </summary>
    public interface IWeatherMakerManager
    {
        /// <summary>
        /// Handle a weather profile change
        /// </summary>
        /// <param name="oldProfile">Old weather profile</param>
        /// <param name="newProfile">New weather profile</param>
        /// <param name="transitionDelay">Transition delay</param>
        /// <param name="transitionDuration">Transition duration</param>
        void WeatherProfileChanged(WeatherMakerProfileScript oldProfile, WeatherMakerProfileScript newProfile, float transitionDelay, float transitionDuration);
    }

    /// <summary>
    /// Generic network connection interface
    /// </summary>
    public interface IWeatherMakerNetworkConnection
    {
        /// <summary>
        /// Get the connection id of a game object
        /// </summary>
        /// <param name="obj">Transform</param>
        /// <returns>Connection id or null if not found</returns>
        string GetConnectionId(Transform obj);

        /// <summary>
        /// IsServer
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// IsClient
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        /// IsConnected
        /// </summary>
        bool IsConnected { get; }
    }

    /// <summary>
    /// Null network connection
    /// </summary>
    public class WeatherMakerNoNetworkConnection : IWeatherMakerNetworkConnection
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WeatherMakerNoNetworkConnection()
        {
            IsServer = true;
            IsClient = true;
            IsConnected = false;
        }

        /// <summary>
        /// Always 0
        /// </summary>
        /// <param name="obj">Transform</param>
        /// <returns>0</returns>
        public string GetConnectionId(Transform obj)
        {
            return "0";
        }

        /// <summary>
        /// False
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// False
        /// </summary>
        public bool IsClient { get; private set; }

        /// <summary>
        /// False
        /// </summary>
        public bool IsConnected { get; private set; }
    }

    /// <summary>
    /// WeatherMaker event
    /// </summary>
    [System.Serializable]
    public class WeatherMakerEvent : UnityEngine.Events.UnityEvent<object> { }
}
