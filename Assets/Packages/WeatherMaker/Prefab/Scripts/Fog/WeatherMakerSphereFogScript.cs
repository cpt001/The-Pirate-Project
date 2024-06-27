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
    /// Fog sphere script
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Collider))]
    public class WeatherMakerSphereFogScript : WeatherMakerFogScript<WeatherMakerFogProfileScript>
    {
        private Renderer fogRenderer;
        private Collider fogCollider;
        private MaterialPropertyBlock materialBlock;
        private WeatherMakerShaderPropertiesScript materialProps;

        /// <summary>
        /// OnInitialize
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            fogRenderer = GetComponent<Renderer>();
            fogCollider = GetComponent<Collider>();
            materialBlock = new MaterialPropertyBlock();
            materialProps = new WeatherMakerShaderPropertiesScript(materialBlock);

            if (fogRenderer.sharedMaterial == null)
            {
                fogRenderer.sharedMaterial = FogMaterial;
            }
            else
            {
                FogMaterial = fogRenderer.sharedMaterial;
            }
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (fogRenderer == null || materialBlock == null)
            {
                return;
            }

            Bounds b = fogRenderer.bounds;
            float radius = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) * 0.3333334f;
            Vector4 pos = transform.position;
            pos.w = radius * radius;
            fogRenderer.GetPropertyBlock(materialBlock);
            materialBlock.SetVector(WMS._WeatherMakerFogSpherePosition, pos);
            materialBlock.SetVector(WMS._WeatherMakerFogBoxCenter, pos);
            materialBlock.SetVector(WMS._WeatherMakerFogVolumePower, new Vector4(1.0f / pos.w, FogProfile.FogEdgeSmoothFactor, FogProfile.FogHeightFalloffPower, FogProfile.FogEdgeFalloffPower));
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                WeatherMakerLightManagerScript.Instance.UpdateShaderVariables(null, materialProps, fogCollider);
            }
            fogRenderer.SetPropertyBlock(materialBlock);
        }
    }
}