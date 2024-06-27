using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Meteor shower script
    /// </summary>
    public class WeatherMakerMeteorShowerScript : MonoBehaviour
    {
        /// <summary>The meteor shower profile</summary>
        [Tooltip("The meteor shower profile")]
        public WeatherMakerMeteorShowerProfileScript Profile;

        private ParticleSystem meteorParticles;
        private ParticleSystemRenderer meteorParticlesRenderer;

        private float nextChange;
        private System.Action<WeatherMakerCommandBuffer> meteorShowerRenderAction;
        private Vector3 offset;
        private float radius;
        private float arc;
        private Vector2 speed;

        private void CameraPreCull(Camera camera)
        {
            if (Profile != null && WeatherMakerScript.GetCameraType(camera) == WeatherMakerCameraType.Normal)
            {
                if (radius == 0.0f)
                {
                    var shape = meteorParticles.shape;
                    shape.radius = radius * camera.farClipPlane;
                    shape.arc = arc;
                }
                var mn = meteorParticles.main;
                var speed = mn.startSpeed;
                speed.constantMin = this.speed.x * camera.farClipPlane;
                speed.constantMax = this.speed.y * camera.farClipPlane;
                mn.startSpeed = speed;
                transform.position = camera.transform.position + (offset * camera.farClipPlane);
            }
        }

        private void RenderMeteorShower(WeatherMakerCommandBuffer cmdBuffer)
        {
            cmdBuffer.CommandBuffer.DrawRenderer(meteorParticlesRenderer, meteorParticlesRenderer.material, 0, 0);
            cmdBuffer.CommandBuffer.DrawRenderer(meteorParticlesRenderer, meteorParticlesRenderer.trailMaterial, 1, 0);
        }

        private void OnEnable()
        {
            meteorShowerRenderAction = RenderMeteorShower;
            meteorParticles = GetComponentInChildren<ParticleSystem>();
            if (meteorParticles == null)
            {
                Debug.LogError("Unable to find meteor shower particle system, a particle system must exist on or underneath the WeatherMakerMeteorShowerScript");
            }

            if (WeatherMakerFullScreenCloudsScript.Instance != null)
            {
                // have to render meteor showers in the full screen cloud command buffer so they render before the clouds
                meteorParticlesRenderer = meteorParticles.GetComponent<ParticleSystemRenderer>();
                meteorParticlesRenderer.enabled = false;
                WeatherMakerFullScreenCloudsScript.Instance.BeforeCloudsRenderHooks.Add(meteorShowerRenderAction);
            }

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
        }

        private void OnDisable()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
            if (WeatherMakerFullScreenCloudsScript.Instance != null)
            {
                WeatherMakerFullScreenCloudsScript.Instance.BeforeCloudsRenderHooks.Remove(meteorShowerRenderAction);
            }
        }

        private void Update()
        {
            if (Profile == null)
            {
                return;
            }

            
            nextChange -= Time.deltaTime;
            if (nextChange <= 0.0f)
            {
                if (Profile.RefreshInterval.Minimum > 0.0f && Profile.RefreshInterval.Maximum > 0.0f)
                {
                    nextChange = Profile.RefreshInterval.Random();
                }
                else
                {
                    nextChange = float.MaxValue;
                }
                var em = meteorParticles.emission;
                float value1 = Profile.EmissionRange.Random();
                float value2 = Profile.EmissionRange.Random();
                em.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Min(value1, value2), Mathf.Max(value1, value2));

                var mn = meteorParticles.main;
                float l1 = Profile.LifetimeRangeMin.Random();
                float l2 = Profile.LifetimeRangeMax.Random();
                var lifeTime = mn.startLifetime;
                lifeTime.constantMin = Mathf.Min(l1, l2);
                lifeTime.constantMax = Mathf.Max(l1, l2);
                mn.startLifetime = lifeTime;

                l1 = Profile.SpeedRangeMin.Random();
                l2 = Profile.SpeedRangeMax.Random();
                speed = new Vector2(Mathf.Min(l1, l2), Mathf.Max(l1, l2));

                // pick a random offset in the sky
                offset = new Vector3(WeatherMakerRandomizer.Unity.Random(Profile.OffsetMin.x, Profile.OffsetMax.x),
                    WeatherMakerRandomizer.Unity.Random(Profile.OffsetMin.y, Profile.OffsetMax.y),
                    WeatherMakerRandomizer.Unity.Random(Profile.OffsetMin.z, Profile.OffsetMax.z));

                radius = Profile.RadiusRange.Random();
                arc = Profile.ArcRange.Random();
            }

            // update color based on sun position
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                WeatherMakerCelestialObjectScript sun = (WeatherMakerLightManagerScript.Instance.SunPerspective ?? WeatherMakerLightManagerScript.Instance.SunOrthographic);
                if (sun != null)
                {
                    float a = sun.GetGradientColor(Profile.Visibility).r;
                    var m = meteorParticles.main;
                    var colorRange = m.startColor;
                    Color minColor = colorRange.colorMin;
                    Color maxcolor = colorRange.colorMax;
                    minColor.a = maxcolor.a = a;
                    colorRange.colorMin = minColor;
                    colorRange.colorMax = maxcolor;
                    m.startColor = colorRange;
                }
            }
        }
    }
}
