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
    /// Unity Gradient class extensions
    /// </summary>
    public static class GradientExtensions
    {
        private static readonly List<float> keysTimes = new List<float>();

        /// <summary>
        /// Lerp between two gradients
        /// </summary>
        /// <param name="a">Base gradient</param>
        /// <param name="b">Final gradient</param>
        /// <param name="t">Lerp amount (0 - 1)</param>
        /// <returns></returns>
        public static UnityEngine.Gradient Lerp(this UnityEngine.Gradient a, UnityEngine.Gradient b, float t)
        {
            return Lerp(a, b, t, false, false);
        }

        private static UnityEngine.Gradient Lerp(this UnityEngine.Gradient a, UnityEngine.Gradient b, float t, bool noAlpha, bool noColor)
        {
            keysTimes.Clear();

            if (a == b || a.Equals(b))
            {
                return b;
            }

            GradientAlphaKey[] alphaKeys1 = a.alphaKeys;
            GradientColorKey[] colorKeys1 = a.colorKeys;
            GradientAlphaKey[] alphaKeys2 = b.alphaKeys;
            GradientColorKey[] colorKeys2 = b.colorKeys;

            if (alphaKeys1.Length == alphaKeys2.Length && colorKeys1.Length == colorKeys2.Length)
            {
                // full compare of all keys, save allocating memory if both gradients are equal
                bool equal = true;
                for (int i = 0; i < alphaKeys1.Length; i++)
                {
                    if (alphaKeys1[i].alpha != alphaKeys2[i].alpha || alphaKeys1[i].time != alphaKeys2[i].time ||
                        colorKeys1[i].color != colorKeys2[i].color || colorKeys1[i].time != colorKeys2[i].time)
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                {
                    return b;
                }
                Gradient gradient = new Gradient();
                GradientColorKey[] clrs = new GradientColorKey[colorKeys1.Length];
                GradientAlphaKey[] alphas = new GradientAlphaKey[colorKeys1.Length];
                for (int i = 0; i < colorKeys1.Length; i++)
                {
                    clrs[i] = new GradientColorKey(Color.Lerp(colorKeys1[i].color, colorKeys2[i].color, t), Mathf.Lerp(colorKeys1[i].time, colorKeys2[i].time, t));
                    alphas[i] = new GradientAlphaKey(Mathf.Lerp(alphaKeys1[i].alpha, alphaKeys2[i].alpha, t), Mathf.Lerp(alphaKeys1[i].time, alphaKeys2[i].time, t));
                }
                gradient.colorKeys = clrs;
                gradient.alphaKeys = alphas;
                return gradient;
            }
            else
            {
                for (int i = 0; i < colorKeys1.Length; i++)
                {
                    float k = colorKeys1[i].time;
                    if (!keysTimes.Contains(k))
                    {
                        keysTimes.Add(k);
                    }
                }

                for (int i = 0; i < colorKeys2.Length; i++)
                {
                    float k = colorKeys2[i].time;
                    if (!keysTimes.Contains(k))
                    {
                        keysTimes.Add(k);
                    }
                }
                for (int i = 0; i < alphaKeys1.Length; i++)
                {
                    float k = alphaKeys1[i].time;
                    if (!keysTimes.Contains(k))
                    {
                        keysTimes.Add(k);
                    }
                }
                for (int i = 0; i < alphaKeys2.Length; i++)
                {
                    float k = alphaKeys2[i].time;
                    if (!keysTimes.Contains(k))
                    {
                        keysTimes.Add(k);
                    }
                }

                GradientColorKey[] clrs = new GradientColorKey[keysTimes.Count];
                GradientAlphaKey[] alphas = new GradientAlphaKey[keysTimes.Count];

                for (int i = 0; i < keysTimes.Count; i++)
                {
                    float key = keysTimes[i];
                    var clr = Color.Lerp(a.Evaluate(key), b.Evaluate(key), t);
                    clrs[i] = new GradientColorKey(clr, key);
                    alphas[i] = new GradientAlphaKey(clr.a, key);
                }

                Gradient gradient = new Gradient();
                gradient.SetKeys(clrs, alphas);
                return gradient;
            }
        }
    }
}
