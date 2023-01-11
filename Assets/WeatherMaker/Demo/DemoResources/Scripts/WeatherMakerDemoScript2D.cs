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

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// 2D demo script
    /// </summary>
    public class WeatherMakerDemoScript2D : MonoBehaviour
    {
        private void Start()
        {

        }

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(Vector3.zero);
            Vector3 worldTopRight = Camera.main.ViewportToWorldPoint(Vector3.one);
            float visibleWorldWidth = worldTopRight.x - worldBottomLeft.x;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Camera.main.transform.Translate(Time.deltaTime * -(visibleWorldWidth * 0.1f), 0.0f, 0.0f);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                Camera.main.transform.Translate(Time.deltaTime * (visibleWorldWidth * 0.1f), 0.0f, 0.0f);
            }
        }
    }
}