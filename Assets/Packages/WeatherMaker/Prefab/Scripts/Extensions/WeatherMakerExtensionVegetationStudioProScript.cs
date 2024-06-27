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

#if VEGETATION_STUDIO_PRO

using AwesomeTechnologies.VegetationStudio;

#endif

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Integration for vegetation studio pro
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Weather Maker/Extensions/Vegetation Studio Pro", 2)]
    public class WeatherMakerExtensionVegetationStudioProScript : WeatherMakerExtensionRainSnowSeasonScript<UnityEngine.MonoBehaviour>
    {

#if VEGETATION_STUDIO_PRO

        protected override void OnUpdateRain(float rain)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance == null)
            {
                return;
            }

            for (int sysIndex = 0; sysIndex < VegetationStudioManager.Instance.VegetationSystemList.Count; sysIndex++)
            {
                if (VegetationStudioManager.Instance.VegetationSystemList[sysIndex].EnvironmentSettings.RainAmount != WeatherMakerPrecipitationManagerScript.Instance.RainIntensity)
                {
                    VegetationStudioManager.Instance.VegetationSystemList[sysIndex].EnvironmentSettings.RainAmount = WeatherMakerPrecipitationManagerScript.Instance.RainIntensity;
                    VegetationStudioManager.Instance.VegetationSystemList[sysIndex].RefreshMaterials();
                }
            }
        }

        protected override void OnUpdateSnow(float snow)
        {
            if (WeatherMakerPrecipitationManagerScript.Instance == null)
            {
                return;
            }

            for (int sysIndex = 0; sysIndex < VegetationStudioManager.Instance.VegetationSystemList.Count; sysIndex++)
            {
                if (VegetationStudioManager.Instance.VegetationSystemList[sysIndex].EnvironmentSettings.SnowAmount != WeatherMakerPrecipitationManagerScript.Instance.SnowIntensity)
                {
                    VegetationStudioManager.Instance.VegetationSystemList[sysIndex].EnvironmentSettings.SnowAmount = WeatherMakerPrecipitationManagerScript.Instance.SnowIntensity;
                    VegetationStudioManager.Instance.VegetationSystemList[sysIndex].RefreshMaterials();
                }
            }
        }

        protected override void OnUpdateWind()
        {
            if (WeatherMakerWindManagerScript.Instance.WindScript.WindZone == null)
            {
                return;
            }

            for (int sysIndex = 0; sysIndex < VegetationStudioManager.Instance.VegetationSystemList.Count; sysIndex++)
            {
                if (VegetationStudioManager.Instance.VegetationSystemList[sysIndex].SelectedWindZone != WeatherMakerWindManagerScript.Instance.WindScript.WindZone)
                {
                    VegetationStudioManager.Instance.VegetationSystemList[sysIndex].SelectedWindZone = WeatherMakerWindManagerScript.Instance.WindScript.WindZone;
                }
            }
        }

#endif

    }
}