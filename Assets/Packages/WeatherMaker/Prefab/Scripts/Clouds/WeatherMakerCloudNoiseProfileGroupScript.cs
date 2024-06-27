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
    /// Cloud noise profile group script
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerCloudNoiseProfile_", menuName = "WeatherMaker/Cloud Noise Profile Group", order = 181)]
    public class WeatherMakerCloudNoiseProfileGroupScript : ScriptableObject
    {
        /// <summary>Profiles</summary>
        [Tooltip("Profiles")]
        public WeatherMakerCloudNoiseProfileScript[] NoiseProfiles;

        /// <summary>Texture size (width, height, depth)</summary>
        [Range(1, 1024)]
        [Tooltip("Texture size (width, height, depth)")]
        public int Size = 128;

        /// <summary>Count of slices</summary>
        [Range(1, 1024)]
        [Tooltip("Count of slices")]
        public int Count = 128;

        /// <summary>Step size (z direction, 0 for auto based on size)</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("Step size (z direction, 0 for auto based on size)")]
        public float Step = 0.0f;

        /// <summary>Filter mode to use when generating noise texture</summary>
        [Tooltip("Filter mode to use when generating noise texture")]
        public FilterMode FilterMode = FilterMode.Bilinear;

        /// <summary>Mip mode to use when generating noise texture</summary>
        [Tooltip("Mip mode to use when generating noise texture")]
        public bool GenerateMips = true;

        /// <summary>File name</summary>
        [Tooltip("File name")]
        public string FileName;

        private static readonly float[] types = new float[4];
        private static readonly Vector4[] perlinParam1 = new Vector4[4];
        private static readonly Vector4[] perlinParam2 = new Vector4[4];
        private static readonly Vector4[] worleyParam1 = new Vector4[4];
        private static readonly Vector4[] worleyParam2 = new Vector4[4];

        /// <summary>
        /// Set global shader values for noise values
        /// </summary>
        public void SetGlobalShader()
        {
            for (int i = 0; i < NoiseProfiles.Length; i++)
            {
                types[i] = (float)NoiseProfiles[i].NoiseType;
                perlinParam1[i] = NoiseProfiles[i].PerlinParameters.GetParams1();
                perlinParam2[i] = NoiseProfiles[i].PerlinParameters.GetParams2();
                worleyParam1[i] = NoiseProfiles[i].WorleyParameters.GetParams1();
                worleyParam2[i] = NoiseProfiles[i].WorleyParameters.GetParams2();
            }
            Shader.SetGlobalFloatArray(WMS._RealTimeCloudNoiseShapeTypes, types);
            Shader.SetGlobalVectorArray(WMS._RealTimeCloudNoiseShapePerlinParam1, perlinParam1);
            Shader.SetGlobalVectorArray(WMS._RealTimeCloudNoiseShapePerlinParam2, perlinParam2);
            Shader.SetGlobalVectorArray(WMS._RealTimeCloudNoiseShapeWorleyParam1, perlinParam1);
            Shader.SetGlobalVectorArray(WMS._RealTimeCloudNoiseShapeWorleyParam2, perlinParam2);
        }
    }
}
