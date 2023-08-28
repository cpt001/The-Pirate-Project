using UnityEngine;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets
{
    /// <summary>
    /// Enable or disable a camera based on XR being enabled
    /// </summary>
    public class VrCameraSwitcher
        : MonoBehaviour
    {
        public bool EnableInVr;
        public Camera Camera;

        private void Start()
        {
            if (Camera == null)
                Camera = GetComponent<Camera>();

            if (EnableInVr)
                Camera.enabled = XRSettings.enabled;
            else
                Camera.enabled = !XRSettings.enabled;
        }
    }
}
