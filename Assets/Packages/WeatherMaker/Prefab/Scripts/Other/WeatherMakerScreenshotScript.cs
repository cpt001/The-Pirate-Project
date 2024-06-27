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

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Simple screenshot script for game view. Press R to take screenshots, file is saved to desktop. There is a short delay from pressing R and the screenshot actually saving.
    /// </summary>
    public class WeatherMakerScreenshotScript : MonoBehaviour
    {
        private int needsScreenshot;

        private void Update()
        {
            if (needsScreenshot == 0)
            {
                needsScreenshot = Input.GetKeyDown(KeyCode.R) ? 1 : 0;
            }
            else if (++needsScreenshot == 64)
            {
                ScreenCapture.CaptureScreenshot(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/screenshot.png");
                needsScreenshot = 0;
            }
        }
    }
}
