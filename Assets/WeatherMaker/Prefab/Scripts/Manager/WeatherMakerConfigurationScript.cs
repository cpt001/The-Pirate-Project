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
    /// Configuration script for the Weather Maker configuration side panel
    /// </summary>
    public class WeatherMakerConfigurationScript : MonoBehaviour
    {
        /// <summary>
        /// Show FPS?
        /// </summary>
        public bool ShowFPS = true;

        /// <summary>
        /// Show time of day?
        /// </summary>
        public bool ShowTimeOfDay = true;

        /// <summary>
        /// Auto add lights upon play?
        /// </summary>
        public bool AutoAddLightsOnStart = true;

        /// <summary>
        /// Configuration panel
        /// </summary>
        public GameObject ConfigurationPanel;

        /// <summary>
        /// Label display for FPS
        /// </summary>
        public UnityEngine.UI.Text LabelFPS;

        /// <summary>
        /// Transition duration slider
        /// </summary>
        public UnityEngine.UI.Slider TransitionDurationSlider;

        /// <summary>
        /// Intensity slider
        /// </summary>
        public UnityEngine.UI.Slider IntensitySlider;

        /// <summary>
        /// Mouse look checkbox
        /// </summary>
        public UnityEngine.UI.Toggle MouseLookEnabledCheckBox;

        /// <summary>
        /// Flashlight checkbox
        /// </summary>
        public UnityEngine.UI.Toggle FlashlightToggle;

        /// <summary>
        /// Time of day checkbox
        /// </summary>
        public UnityEngine.UI.Toggle TimeOfDayEnabledCheckBox;

        /// <summary>
        /// Collision checkbox
        /// </summary>
        public UnityEngine.UI.Toggle CollisionToggle;

        /// <summary>
        /// Time of day slider
        /// </summary>
        public UnityEngine.UI.Slider DawnDuskSlider;

        /// <summary>
        /// Time of day text
        /// </summary>
        public UnityEngine.UI.Text TimeOfDayText;

        /// <summary>
        /// Time of day category text
        /// </summary>
        public UnityEngine.UI.Text TimeOfDayCategoryText;

        /// <summary>
        /// Cloud picker
        /// </summary>
        public UnityEngine.UI.Dropdown CloudDropdown;

        /// <summary>
        /// Weather map image
        /// </summary>
        public UnityEngine.UI.RawImage WeatherMapImage;

        /// <summary>
        /// Event system
        /// </summary>
        public UnityEngine.EventSystems.EventSystem EventSystem;

        /// <summary>
        /// Side panel root object
        /// </summary>
        public GameObject SidePanel;

        private int frameCount = 0;
        private float nextFrameUpdate = 0.0f;
        private float fps = 0.0f;
        private float frameUpdateRate = 4.0f; // 4 per second
        private int frameCounter;
        private WeatherMakerCloudType clouds;
        private WeatherMakerCloudType lastClouds;

        private void UpdateTimeOfDay()
        {
            if (WeatherMakerDayNightCycleManagerScript.HasInstance())
            {
                DawnDuskSlider.value = WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay;
                if (TimeOfDayText.IsActive() && ShowTimeOfDay)
                {
                    System.TimeSpan t = System.TimeSpan.FromSeconds(WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay);
                    TimeOfDayText.text = string.Format("{0:00}:{1:00}:{2:00}", t.Hours, t.Minutes, t.Seconds);
                }
                TimeOfDayCategoryText.text = WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDayCategory.ToString();
            }
        }

        private void DisplayFPS()
        {
            if (LabelFPS != null && ShowFPS)
            {
                frameCount++;
                if (Time.time > nextFrameUpdate)
                {
                    nextFrameUpdate += (1.0f / frameUpdateRate);
                    fps = (int)Mathf.Floor((float)frameCount * frameUpdateRate);
                    LabelFPS.text = "FPS: " + fps;
                    frameCount = 0;
                }
            }
        }

        private void Start()
        {
            if (WeatherMakerPrecipitationManagerScript.HasInstance())
            {
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = IntensitySlider.value = 0.5f;
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationChangeDuration = TransitionDurationSlider.value;
                DawnDuskSlider.value = WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay;
            }
            else
            {
                DawnDuskSlider.value = 0.5f;
            }
            if (WeatherMakerScript.HasInstance())
            {
                CollisionToggle.isOn = WeatherMakerScript.Instance.PerformanceProfile.EnablePrecipitationCollision;
            }
            else
            {
                CollisionToggle.isOn = false;
            }
            nextFrameUpdate = Time.time;
            if (UnityEngine.EventSystems.EventSystem.current == null && ConfigurationPanel != null && ConfigurationPanel.activeInHierarchy)
            {
                EventSystem.gameObject.SetActive(true);
                UnityEngine.EventSystems.EventSystem.current = EventSystem;
            }
            if (AutoAddLightsOnStart && WeatherMakerLightManagerScript.HasInstance())
            {
                foreach (Light light in GameObject.FindObjectsOfType<Light>())
                {
                    if (light.type != LightType.Directional && light.type != LightType.Area &&
                        !WeatherMakerLightManagerScript.Instance.AutoAddLights.Contains(light))
                    {
                        WeatherMakerLightManagerScript.Instance.AutoAddLights.Add(light);
                    }
                }
            }
            if (Camera.main != null && Camera.main.orthographic && WeatherMapImage != null)
            {
                WeatherMapImage.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            DisplayFPS();
            if (WeatherMakerFullScreenCloudsScript.Instance != null)
            {
                WeatherMapImage.texture = WeatherMakerFullScreenCloudsScript.Instance.WeatherMapRenderTexture;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                LightningStrikeButtonClicked();
            }
            if (Input.GetKeyDown(KeyCode.BackQuote) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                // Unity bug, disabling or setting transform scale to 0 will break any projectors in the scene
                RectTransform r = SidePanel.GetComponent<RectTransform>();
                Vector2 ap = r.anchoredPosition;
                bool visible;
                if (r.anchoredPosition.x < 0.0f)
                {
                    visible = true;
                    ap.x = 110.0f;
                }
                else
                {
                    visible = false;
                    WeatherMapImage.enabled = false;
                    ap.x = -9999.0f;
                }
                r.anchoredPosition = ap;
                if (WeatherMapImage.transform.childCount > 0)
                {
                    WeatherMapImage.transform.GetChild(0).gameObject.SetActive(visible);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.LeftShift))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(0, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(1, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(2, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(3, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(4, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    UnityEngine.QualitySettings.SetQualityLevel(5, true);
                }
            }

            /*
            // test additive scene loading
            if (Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.LeftShift))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                return;
            }
            */

            if (WeatherMapImage != null && Camera.main != null)
            {
                if (Camera.main.orthographic)
                {
                    WeatherMapImage.enabled = false;
                }
                else if (Input.GetKeyDown(KeyCode.M) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    WeatherMapImage.enabled = !WeatherMapImage.enabled;
                }
            }
            UpdateTimeOfDay();
            frameCounter++;
        }

        // Weather configuration...

        private void UpdateClouds()
        {
            if (clouds == lastClouds)
            {
                return;
            }
            lastClouds = clouds;
            if (WeatherMakerLegacyCloudScript2D.Instance != null && WeatherMakerLegacyCloudScript2D.Instance.enabled &&
                WeatherMakerLegacyCloudScript2D.Instance.gameObject.activeInHierarchy)
            {
                if (clouds == WeatherMakerCloudType.None)
                {
                    WeatherMakerLegacyCloudScript2D.Instance.RemoveClouds();
                }
                else if (clouds != WeatherMakerCloudType.Custom)
                {
                    WeatherMakerLegacyCloudScript2D.Instance.CreateClouds();
                }
            }
            else if (WeatherMakerFullScreenCloudsScript.Instance != null && WeatherMakerFullScreenCloudsScript.Instance.enabled &&
                WeatherMakerFullScreenCloudsScript.Instance.gameObject.activeInHierarchy)
            {
                float duration = WeatherMakerPrecipitationManagerScript.Instance.PrecipitationChangeDuration;
                if (clouds == WeatherMakerCloudType.None)
                {
                    WeatherMakerFullScreenCloudsScript.Instance.HideCloudsAnimated(duration);
                }
                else if (clouds != WeatherMakerCloudType.Custom)
                {
                    WeatherMakerFullScreenCloudsScript.Instance.ShowCloudsAnimated(duration, "WeatherMakerCloudProfile_" + clouds.ToString());
                }
                else
                {
                    // custom clouds, do not modify current cloud script state
                }
            }
        }

        /// <summary>
        /// Rain change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void RainToggleChanged(bool isOn)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.Precipitation = (isOn ? WeatherMakerPrecipitationType.Rain : WeatherMakerPrecipitationType.None);
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = IntensitySlider.value;
            }
        }

        /// <summary>
        /// Snow change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void SnowToggleChanged(bool isOn)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.Precipitation = (isOn ? WeatherMakerPrecipitationType.Snow : WeatherMakerPrecipitationType.None);
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = IntensitySlider.value;
            }
        }

        /// <summary>
        /// Hail change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void HailToggleChanged(bool isOn)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.Precipitation = (isOn ? WeatherMakerPrecipitationType.Hail : WeatherMakerPrecipitationType.None);
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = IntensitySlider.value;
            }
        }

        /// <summary>
        /// Sleet change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void SleetToggleChanged(bool isOn)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.Precipitation = (isOn ? WeatherMakerPrecipitationType.Sleet : WeatherMakerPrecipitationType.None);
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = IntensitySlider.value;
            }
        }

        /// <summary>
        /// Cloud change handler
        /// </summary>
        public void CloudToggleChanged()
        {
            string text = CloudDropdown.captionText.text;
            text = text.Replace("-", string.Empty).Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
            clouds = (WeatherMakerCloudType)System.Enum.Parse(typeof(WeatherMakerCloudType), text);
            UpdateClouds();
        }

        /// <summary>
        /// Lightning change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void LightningToggleChanged(bool isOn)
        {
            if (WeatherMakerThunderAndLightningScript.Instance != null)
            {
                WeatherMakerThunderAndLightningScript.Instance.EnableLightning = isOn;
            }
        }

        /// <summary>
        /// Collision change handler for precipitation
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void CollisionToggleChanged(bool isOn)
        {
            if (WeatherMakerScript.Instance != null)
            {
                WeatherMakerScript.Instance.PerformanceProfile.EnablePrecipitationCollision = isOn;
            }
        }

        /// <summary>
        /// Wind change handler
        /// </summary>
        /// <param name="isOn">True if on</param>
        public void WindToggleChanged(bool isOn)
        {
            if (WeatherMakerScript.Instance == null || WeatherMakerPrecipitationManagerScript.Instance == null || WeatherMakerWindScript.Instance == null)
            {
                return;
            }

            float duration = WeatherMakerPrecipitationManagerScript.Instance.PrecipitationChangeDuration;
            if (isOn)
            {
                // make a copy to avoid changes during runtime, drag in a profile to inspector manually to edit at runtime
                WeatherMakerWindScript.Instance.SetWindProfileAnimated(WeatherMakerScript.Instance.LoadResource<WeatherMakerWindProfileScript>("WeatherMakerWindProfile_MediumWind"), 0.0f, duration);
            }
            else
            {
                WeatherMakerWindScript.Instance.SetWindProfileAnimated(WeatherMakerScript.Instance.LoadResource<WeatherMakerWindProfileScript>("WeatherMakerWindProfile_None"), 0.0f, duration);
            }
        }

        /// <summary>
        /// Transition duration slider change
        /// </summary>
        /// <param name="val">New value</param>
        public void TransitionDurationSliderChanged(float val)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationChangeDuration = val;
            }
        }

        /// <summary>
        /// Intensity duration slider change
        /// </summary>
        /// <param name="val">New value</param>
        public void IntensitySliderChanged(float val)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance != null)
            {
                WeatherMakerPrecipitationManagerScript.Instance.PrecipitationIntensity = val;
            }
        }

        /// <summary>
        /// Mouse look value change
        /// </summary>
        /// <param name="val">New value</param>
        public void MouseLookEnabledChanged(bool val)
        {
            MouseLookEnabledCheckBox.isOn = val;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (WeatherMakerScript.IsLocalPlayer(obj.transform))
                {
                    WeatherMakerPlayerControllerScript controller = obj.GetComponent<WeatherMakerPlayerControllerScript>();
                    if (controller != null && controller.enabled)
                    {
                        controller.EnableMouseLook = val;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Flashlight value change
        /// </summary>
        /// <param name="val">New value</param>
        public void FlashlightChanged(bool val)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (WeatherMakerScript.IsLocalPlayer(obj.transform))
                {
                    Light[] lights = obj.GetComponentsInChildren<Light>();
                    foreach (Light light in lights)
                    {
                        if (light.name == "Flashlight")
                        {
                            light.enabled = val;
                            break;
                        }
                    }
                    break;
                }
            }
            if (FlashlightToggle != null)
            {
                FlashlightToggle.isOn = val;
            }
        }

        /// <summary>
        /// Fog change
        /// </summary>
        /// <param name="val">New value</param>
        public void FogChanged(bool val)
        {
            if (WeatherMakerFullScreenFogScript.Instance == null || WeatherMakerFullScreenFogScript.Instance.FogProfile == null)
            {
                Debug.LogError("Fog is not setup. If using 2D, fog is not yet supported.");
            }
            else
            {
                // if fog is not active, set the start fog density to 0, otherwise start at whatever density it is at
                float startFogDensity = WeatherMakerFullScreenFogScript.Instance.FogProfile.FogDensity;
                float endFogDensity = (startFogDensity == 0.0f ? 0.007f : 0.0f);
                WeatherMakerFullScreenFogScript.Instance.TransitionFogDensity(startFogDensity, endFogDensity, TransitionDurationSlider.value);
            }
        }

        /// <summary>
        /// Automatic weather value change
        /// </summary>
        /// <param name="val">New value</param>
        public void ManagerChanged(bool val)
        {
            if (WeatherMakerScript.Instance != null)
            {
                Transform parent = WeatherMakerScript.Instance.transform;
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (t.GetComponent<WeatherMakerWeatherZoneScript>() != null)
                    {
                        t.gameObject.SetActive(val);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Auto time of day value change
        /// </summary>
        /// <param name="val">New value</param>
        public void TimeOfDayEnabledChanged(bool val)
        {
            if (WeatherMakerDayNightCycleManagerScript.HasInstance())
            {
                WeatherMakerDayNightCycleManagerScript.Instance.Speed = WeatherMakerDayNightCycleManagerScript.Instance.NightSpeed = (val ? 10.0f : 0.0f);
            }
        }

        /// <summary>
        /// Lightning bolt button click handler
        /// </summary>
        public void LightningStrikeButtonClicked()
        {
            if (WeatherMakerThunderAndLightningScript.Instance != null)
            {
                WeatherMakerThunderAndLightningScript.Instance.CallIntenseLightning();
            }
        }

        /// <summary>
        /// Time of day slider change handler
        /// </summary>
        /// <param name="val">New value in seconds, 0 to 86400</param>
        public void DawnDuskSliderChanged(float val)
        {
            if (WeatherMakerDayNightCycleManagerScript.HasInstance())
            {
                WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay = val;
            }
        }
    }
}
