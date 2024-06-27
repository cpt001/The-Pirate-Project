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
    /// A null zone can block precipitation, fog, overlay and water fog from rendering
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerNullZoneScript : MonoBehaviour
    {
        /// <summary>The profile for this null zone. If null, a default profile will be used.</summary>
        [Tooltip("The profile for this null zone. If null, a default profile will be used.")]
        public WeatherMakerNullZoneProfile NullZoneProfile;

        /// <summary>Strength of the null zone, 0 means no null zone at all, 1 means full strength.</summary>
        [Tooltip("Strength of the null zone, 0 means no null zone at all, 1 means full strength.")]
        [Range(0.0f, 1.0f)]
        public float NullZoneStrength = 1.0f;

        /// <summary>
        /// One minus NullZoneStrength
        /// </summary>
        public float OneMinusNullZoneStrength { get; private set; }

        /// <summary>
        /// Current null zone mask
        /// </summary>
        public int CurrentMask { get; private set; }

        /// <summary>
        /// Current null zone fade
        /// </summary>
        public float CurrentFade { get; private set; }

        /// <summary>
        /// Current null zone state
        /// </summary>
        public WeatherMakerZullZoneState CurrentState { get; private set; }

        internal bool HasEntered { get; private set; }

        internal BoxCollider BoxCollider { get; private set; }
        internal SphereCollider SphereCollider { get; private set; }
        internal Bounds bounds;
        internal Bounds inflatedBounds;

        /// <summary>
        /// Whether the null zone is active
        /// </summary>
        public bool NullZoneActive { get; private set; }

        /// <summary>
        /// Set a new null zone strength, animated
        /// </summary>
        /// <param name="newStrength">New strength, clamped to 0 - 1</param>
        /// <param name="duration">Duration</param>
        public void SetStrengthAnimated(float newStrength, float duration)
        {
            TweenFactory.Tween("WeatherMakerZullZoneScript_" + GetInstanceID(), NullZoneStrength, Mathf.Clamp(newStrength, 0.0f, 1.0f),
                duration, TweenScaleFunctions.Linear, (t) => NullZoneStrength = t.CurrentProgress);
        }

        private void AddNullZone()
        {
            // these are cleared every frame, safe to add over and over again
            if (WeatherMakerLightManagerScript.Instance != null && NullZoneActive)
            {
                if (BoxCollider != null)
                {
                    bounds = BoxCollider.bounds;
                }
                else if (SphereCollider != null)
                {
                    bounds = SphereCollider.bounds;
                }
                inflatedBounds = bounds;

                // expand the inflated bounds if there is a fade - the fade goes beyond the null zone collider
                if (CurrentFade > 0.0f && CurrentFade < 100.0f)
                {
                    inflatedBounds.Expand(Mathf.Max(1.0f, 0.6f / CurrentFade));
                }

                WeatherMakerLightManagerScript.Instance.NullZones.Add(this);
            }
        }

        private void Update()
        {
            if (NullZoneProfile == null)
            {
                return;
            }

            CurrentState = (HasEntered && NullZoneProfile.EnteredState.Enabled ? NullZoneProfile.EnteredState : NullZoneProfile.DefaultState);
            CurrentMask = System.Convert.ToInt32(CurrentState.RenderMask);
            CurrentFade = (CurrentState.Enabled && CurrentState.Fade < 100.0f ? CurrentState.Fade : 9999999.0f);

            // ensure a max amount of fade
            if (CurrentFade >= 0.0f)
            {
                CurrentFade = Mathf.Max(0.005f, CurrentFade);
            }
            else
            {
                CurrentFade = Mathf.Min(-0.005f, CurrentFade);
            }

            if (CurrentMask < 0)
            {
                // WTF Unity, sometimes this is positive other times drops below 0...
                CurrentMask += 16;
            }

            // if the null zone would render everything, no point in adding it to the shader
            NullZoneActive = (CurrentState.Enabled && CurrentMask != 15);

            OneMinusNullZoneStrength = 1.0f - NullZoneStrength;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (WeatherMakerScript.IsLocalPlayer(other.transform))
            {
                HasEntered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (WeatherMakerScript.IsLocalPlayer(other.transform))
            {
                HasEntered = false;
            }
        }

        private void OnEnable()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            BoxCollider = GetComponent<BoxCollider>();
            SphereCollider = GetComponent<SphereCollider>();
            NullZoneProfile = (NullZoneProfile == null ? WeatherMakerScript.Instance.LoadResource<WeatherMakerNullZoneProfile>("WeatherMakerNullZoneProfile_Default") : NullZoneProfile);
            if (BoxCollider == null && SphereCollider == null)
            {
                Debug.LogError("Null zone script needs a box or sphere trigger collider");
            }
            if (NullZoneProfile == null)
            {
                Debug.LogError("Null zone script profile is null, WeatherMakerNullZoneProfile_Default default profile is missing");
            }
            AddNullZone();
        }

        private void OnDisable()
        {
            HasEntered = false;
        }

        private void LateUpdate()
        {
            AddNullZone();
        }
    }
}