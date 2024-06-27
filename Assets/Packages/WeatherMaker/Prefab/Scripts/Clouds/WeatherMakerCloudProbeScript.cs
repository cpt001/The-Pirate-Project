using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Cloud probe script, allows probing into the volumetric clouds and getting value back on CPU
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerCloudProbeScript : MonoBehaviour
    {
        /// <summary>The target to probe. Defaults to this script transform.</summary>
        [Tooltip("The target to probe. Defaults to this script transform.")]
        public Transform ProbeSource;

        /// <summary>Probe destination. If null or equal to ProbeTarget, then a single point sample is made. If ProbeDestination is elsewhere, then an accumulated ray march sample of clouds is taken.</summary>
        [Tooltip("Probe destination. If null or equal to ProbeTarget, then a single point sample is made. If ProbeDestination is elsewhere, then " +
            "an accumulated ray march sample of clouds is taken.")]
        public Transform ProbeDestination;

        /// <summary>Whether to override ProbeSource to the camera being rendered</summary>
        [Tooltip("Whether to override ProbeSource to the camera being rendered")]
        public bool ProbeSourceIsCamera;

#if UNITY_EDITOR

        /// <summary>Whether to show gizmoes</summary>
        [Tooltip("Whether to show gizmoes")]
        public bool ShowGizmos;

#endif

        private void OnEnable()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
        }

        private void CameraPreCull(Camera camera)
        {
            Transform source = (ProbeSourceIsCamera ? camera.transform : (ProbeSource == null ? transform : ProbeSource));
            if (WeatherMakerFullScreenCloudsScript.Instance != null && source != null)
            {
                WeatherMakerFullScreenCloudsScript.Instance.RequestCloudProbe(camera, source, ProbeDestination);
            }
        }

#if UNITY_EDITOR

        private static Texture2D blackTexture;
        private void OnDrawGizmos()
        {
            if (!ShowGizmos)
            {
                return;
            }

            Transform source = (ProbeSourceIsCamera ? Camera.current.transform : (ProbeSource == null ? transform : ProbeSource));
            if (WeatherMakerFullScreenCloudsScript.Instance != null && source != null)
            {
                // https://gamedev.stackexchange.com/questions/120960/how-can-i-debug-draw-different-shapes-in-unity
                WeatherMakerFullScreenCloudsScript.CloudProbeResult result = WeatherMakerFullScreenCloudsScript.Instance.GetCloudProbe(Camera.current ?? Camera.main, source, ProbeDestination);
                float d = Mathf.Min(1.0f, result.DensitySource * 5.0f);
                UnityEditor.Handles.color = new Color(d, d, d, 1.0f);
                UnityEditor.Handles.SphereHandleCap(0, source.position, Quaternion.identity, 16.0f, EventType.Repaint);
                GUIStyle textStyle = new GUIStyle();
                textStyle.normal.textColor = textStyle.active.textColor = Color.white;
                if (blackTexture == null)
                {
                    blackTexture = new Texture2D(1, 1);
                    blackTexture.SetPixel(0, 0, Color.black);
                    blackTexture.Apply();
                }
                textStyle.normal.background = textStyle.active.background = blackTexture;
                UnityEditor.Handles.Label(source.position, "Cloud: " + result.DensitySource.ToString("0.000"), textStyle);
                if (ProbeDestination != null && ProbeDestination != source)
                {
                    UnityEditor.Handles.SphereHandleCap(0, ProbeDestination.position, Quaternion.identity, 16.0f, EventType.Repaint);
                    UnityEditor.Handles.color = Color.green;
                    UnityEditor.Handles.DrawLine(source.position, ProbeDestination.position);
                    Vector3 dir = (ProbeDestination.position - source.position).normalized;
                    Quaternion rot = Quaternion.LookRotation(dir);
                    UnityEditor.Handles.ArrowHandleCap(0, ProbeDestination.position - (dir * 96.0f), rot, 64.0f, EventType.Repaint);
                    UnityEditor.Handles.Label(0.5f * (source.position + ProbeDestination.position), "Cloud: " + result.DensityRaySum.ToString("0.000"), textStyle);
                    UnityEditor.Handles.Label(ProbeDestination.position, "Cloud: " + result.DensityTarget.ToString("0.000"), textStyle);
                }
            }
        }

#endif

    }
}
