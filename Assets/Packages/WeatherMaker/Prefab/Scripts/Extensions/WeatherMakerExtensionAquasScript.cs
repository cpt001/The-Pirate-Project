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
    /// Integration for AQUAS water and river set
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/Aquas", 6)]
    public class WeatherMakerExtensionAquasScript : WeatherMakerExtensionWaterScript

#if AQUAS_PRESENT

        <AQUAS_Reflection>

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if AQUAS_PRESENT

        private AQUAS_LensEffects lensScript;
        private bool fogWasEnabled;
        private bool wasUnderwater;

        protected override void Awake()
        {
            base.Awake();

            lensScript = GameObject.FindObjectOfType<AQUAS_LensEffects>();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (TypeScript != null && WeatherMakerFullScreenFogScript.Instance != null && WeatherMakerFullScreenCloudsScript.Instance != null &&
                WeatherMakerFullScreenCloudsScript.Instance.CloudProfile != null && WeatherMakerFullScreenFogScript.Instance.FogProfile != null)
            {
                Renderer renderer = TypeScript.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float specular = Mathf.Pow(WeatherMakerFullScreenFogScript.Instance.FogProfile.FogScatterReduction, CloudCoverWaterSpecularPower);
                    specular = Mathf.Min(specular, Mathf.Pow(1.0f - WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.CloudDirectionalLightDirectBlock, CloudCoverWaterSpecularPower));
                    specular = Mathf.Clamp(specular, CloudCoverWaterMinSpecular, 1.0f);
                    float reflection = Mathf.Pow(WeatherMakerFullScreenFogScript.Instance.FogProfile.FogScatterReduction, CloudCoverWaterReflectionPower);
                    reflection = Mathf.Min(reflection, Mathf.Pow(1.0f - WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.CloudDirectionalLightDirectBlock, CloudCoverWaterReflectionPower));
                    reflection = Mathf.Clamp(reflection, CloudCoverWaterMinReflection, 0.5f);
                    renderer.sharedMaterial.SetFloat(WMS._Specular, specular);
                    renderer.sharedMaterial.SetFloat(WMS._ReflectionIntensity, reflection);
                }
                if (WeatherMakerFullScreenFogScript.Instance.FogDensity > 0.0f && lensScript != null)
                {
                    if (lensScript.underWater)
                    {
                        if (!wasUnderwater)
                        {
                            wasUnderwater = true;
                            fogWasEnabled = WeatherMakerFullScreenFogScript.Instance.enabled;
                        }
                        WeatherMakerFullScreenFogScript.Instance.enabled = false;
                    }
                    else if (wasUnderwater)
                    {
                        WeatherMakerFullScreenFogScript.Instance.enabled = fogWasEnabled;
                        fogWasEnabled = wasUnderwater = false;
                    }
                }
            }
        }

#endif

    }
}