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
    /// Rotate/move light script
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class WeatherMakerDemoScriptSpotlightRotate : MonoBehaviour
    {
        /// <summary>
        /// Max range
        /// </summary>
        [Tooltip("Maximum movement range")]
        public float Range;

        /// <summary>
        /// Light range
        /// </summary>
        [SingleLine("Light range. 0 for default.")]
        public RangeOfFloats LightRange;

        /// <summary>
        /// Intensity range
        /// </summary>
        [SingleLine("Intensity range. 0 for default.")]
        public RangeOfFloats IntensityRange;

        private Light _light;
        private MeshRenderer _meshRenderer;
        private MeshRenderer meshRenderer2;
        private Transform moveTransform;
        private float baseIntensity;
        private Vector3 initialPos;
        private Vector3 startPos;
        private Vector3 endPos;
        private float currentDuration;
        private float totalDuration;
        private Quaternion startRotation;
        private Quaternion endRotation;

        private void OnEnable()
        {
            initialPos = startPos = transform.position;
            _light = GetComponent<Light>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null && transform.parent != null)
            {
                _meshRenderer = transform.parent.GetComponent<MeshRenderer>();
                if (_meshRenderer != null)
                {
                    moveTransform = _meshRenderer.transform;
                }
            }
            if (moveTransform == null)
            {
                moveTransform = transform;
            }
            meshRenderer2 = transform.childCount == 0 ? null : transform.GetChild(0).GetComponent<MeshRenderer>();
            _light.color = new Color(WeatherMakerRandomizer.Unity.Random(), WeatherMakerRandomizer.Unity.Random(), WeatherMakerRandomizer.Unity.Random());
            _light.intensity = baseIntensity = (IntensityRange.Maximum <= 0.0f ? (_light.intensity * WeatherMakerRandomizer.Unity.Random(0.5f, 1.5f)) : IntensityRange.Random());
            if (_light.type == LightType.Spot)
            {
                _light.range = (LightRange.Maximum <= 0.0f ? WeatherMakerRandomizer.Unity.Random(100.0f, 200.0f) : LightRange.Random());
                _light.spotAngle = WeatherMakerRandomizer.Unity.Random(30.0f, 90.0f);
                if (_meshRenderer != null)
                {
                    moveTransform.localScale *= (_light.range / 200.0f);
                    Vector2 movement = WeatherMakerRandomizer.Unity.RandomInsideUnitCircle() * WeatherMakerRandomizer.Unity.Random(-Range, Range);
                    moveTransform.Translate(movement.x, 0.0f, movement.y);
                }
            }
            else if (_light.type == LightType.Point)
            {
                _light.range = (LightRange.Maximum <= 0.0f ? WeatherMakerRandomizer.Unity.Random(32.0f, 64.0f) : LightRange.Random());
                moveTransform.localScale *= (_light.range / 100.0f);
            }
            if (_meshRenderer != null)
            {
                _meshRenderer.sharedMaterial = new Material(_meshRenderer.sharedMaterial);
                _meshRenderer.sharedMaterial.SetColor(WMS._EmissionColor, _light.color);
            }
            if (meshRenderer2 != null)
            {
                meshRenderer2.sharedMaterial = new Material(_meshRenderer.sharedMaterial);
                meshRenderer2.sharedMaterial.SetColor(WMS._EmissionColor, Color.gray);
            }
            WeatherMakerCommandBufferManagerScript.Instance.OriginOffsetChanged.AddListener(OriginOffsetChanged);
        }

        private void OnDisable()
        {
            WeatherMakerCommandBufferManagerScript.Instance.OriginOffsetChanged.RemoveListener(OriginOffsetChanged);
        }

        private void OriginOffsetChanged(WeatherMakerCameraProperties props)
        {
            initialPos += props.OriginOffsetCurrent;
            startPos += props.OriginOffsetCurrent;
            endPos += props.OriginOffsetCurrent;
        }

        private void LateUpdate()
        {
            currentDuration -= Time.deltaTime;
            if (_light.type == LightType.Spot || _light.type == LightType.Area)
            {
                if (currentDuration <= 0.0f)
                {
                    totalDuration = currentDuration = WeatherMakerRandomizer.Unity.Random(3.0f, 6.0f);
                    Vector3 ray = WeatherMakerRandomizer.Unity.RandomInsideUnitSphere();
                    ray.y = Mathf.Min(ray.y, -0.25f);
                    startRotation = moveTransform.rotation;
                    endRotation = Quaternion.LookRotation(ray);
                }
                moveTransform.rotation = Quaternion.Lerp(startRotation, endRotation, 1.0f - (currentDuration / totalDuration));
            }
            else if (_light.type == LightType.Point)
            {
                if (currentDuration <= 0.0f)
                {
                    totalDuration = currentDuration = WeatherMakerRandomizer.Unity.Random(3.0f, 6.0f);
                    startPos = moveTransform.position;
                    endPos = initialPos + new Vector3(WeatherMakerRandomizer.Unity.Random(-Range, Range), 0.0f, WeatherMakerRandomizer.Unity.Random(-Range, Range));
                }
                moveTransform.position = Vector3.Lerp(startPos, endPos, 1.0f - (currentDuration / totalDuration));
            }
            if (_light.type != LightType.Area)
            {
                _light.intensity = baseIntensity * (0.5f + Mathf.PerlinNoise(Time.timeSinceLevelLoad * 0.01f, baseIntensity + _light.range));
            }
        }
    }
}