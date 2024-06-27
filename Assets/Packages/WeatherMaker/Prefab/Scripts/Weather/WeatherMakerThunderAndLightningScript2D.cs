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

using UnityEngine;
using System.Collections;
using System;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// 2D lightning script
    /// </summary>
    public class WeatherMakerThunderAndLightningScript2D : WeatherMakerThunderAndLightningScript
    {
        private void CalculateVisibleBounds(out Vector3 visibleMin, out Vector3 visibleMax, out float width, out float height)
        {
            WeatherMakerScript script = WeatherMakerScript.Instance;
            if (script == null || script.AllowCameras == null || script.AllowCameras.Count == 0)
            {
                Debug.LogError("Unable to calculate visible bounds, AllowCamera property must be set on WeatherMakerScript");
            }
            visibleMin = WeatherMakerScript.Instance.AllowCameras[0].ViewportToWorldPoint(Vector3.zero);
            visibleMax = WeatherMakerScript.Instance.AllowCameras[0].ViewportToWorldPoint(Vector3.one);
            width = (visibleMax.x - visibleMin.x);
            height = (visibleMax.y - visibleMin.y);
        }

        /// <summary>
        /// Compute start position
        /// </summary>
        /// <param name="anchorPosition">Anchor position</param>
        /// <param name="visibleInCamera">Visible in camera?</param>
        /// <param name="intense">Intense (close) or normal (far)</param>
        /// <param name="cloudOnly">Cloud only with no bolt?</param>
        /// <returns>Start position</returns>
        protected override Vector3 CalculateStartPosition(ref Vector3 anchorPosition, Camera visibleInCamera, bool intense, bool cloudOnly)
        {
            // TODO: Implement cloudOnly
            Vector3 visibleMin, visibleMax;
            float width, height;
            CalculateVisibleBounds(out visibleMin, out visibleMax, out width, out height);
            Vector3 start = new Vector3(visibleMin.x + (width * WeatherMakerRandomizer.Unity.Random(0.2f, 0.8f)), visibleMin.y + (height * StartYBase.Random()), 0.0f);
            start.x += (width * StartXVariance.Random());
            start.y += (height * StartYVariance.Random());
            return start;
        }

        /// <summary>
        /// Compute end position
        /// </summary>
        /// <param name="anchorPosition">Anchor position</param>
        /// <param name="start">Start position</param>
        /// <param name="visibleInCamera">Camera to show in, can be null</param>
        /// <param name="intense">Intense (close) or normal (far)</param>
        /// <returns>End position</returns>
        protected override Vector3 CalculateEndPosition(ref Vector3 anchorPosition, ref Vector3 start, Camera visibleInCamera, bool intense)
        {
            Vector3 dir;
            dir.x = EndXVariance.Random();
            dir.y = -1.0f;
            dir.z = EndZVariance.Random();
            dir = dir.normalized;

            RaycastHit2D h;
            if (WeatherMakerRandomizer.Unity.Random() <= GroundLightningChance ||
                ((h = Physics2D.Raycast(start, dir)).collider == null))
            {
                Vector3 visibleMin, visibleMax;
                float width, height;
                CalculateVisibleBounds(out visibleMin, out visibleMax, out width, out height);
                float maxDimen = Mathf.Max(width, height);
                float variance = maxDimen * EndYVariance.Random();
                Vector3 end = start + (dir * variance);
                return end;
            }
            else
            {
                // lightning hit the ground
                return h.point;
            }
        }
    }
}