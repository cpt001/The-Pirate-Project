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

#if MIRROR

using Mirror;

#endif

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Network player script
    /// </summary>
    public class WeatherMakerNetworkPlayerScript

#if MIRROR

        : NetworkBehaviour

#else

        : MonoBehaviour

#endif

    {


#if MIRROR

#pragma warning disable

        /// <summary>Objects that should only activate for local player</summary>
        [SerializeField]
        [Tooltip("Objects that should only activate for local player")]
        private Behaviour[] LocalOnlyObjects;

        private Camera camera;

#pragma warning restore

        private void AddAllowedCamera()
        {
            if (WeatherMakerScript.Instance != null && camera != null && camera.enabled && !WeatherMakerScript.Instance.AllowCameras.Contains(camera))
            {
                WeatherMakerScript.Instance.AllowCameras.Add(camera);
            }
        }

        private void OnEnable()
        {
            camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            // cleanup networked players of cameras, audio listener, etc.
            if (isLocalPlayer)
            {
                if (LocalOnlyObjects != null)
                {
                    foreach (Behaviour obj in LocalOnlyObjects)
                    {
                        obj.enabled = true;
                    }
                }
                AddAllowedCamera();
            }
            else if (LocalOnlyObjects != null)
            {
                foreach (Behaviour obj in LocalOnlyObjects)
                {
                    obj.enabled = false;
                }
            }
        }

#endif

    }
}
