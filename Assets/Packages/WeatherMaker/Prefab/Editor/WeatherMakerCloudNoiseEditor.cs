using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    [System.Serializable]
    public enum NoiseMode
    {
        Mix = 0,
        PerlinOnly = 1,
        WorleyOnly = 2
    }

    [System.Serializable]
    public class NoiseSettings
    {
        [Range(1, 8)]
        public int Octaves = 5;

        [Range(1, 16)]
        public int StartPeriod = 4;

        [Range(0f, 2f)]
        public float Brightness = 1.0f;

        [Range(0f, 8f)]
        public float Contrast = 1.0f;

        public Vector4 GetParams()
        {
            return new Vector4(Octaves, StartPeriod, Brightness, Contrast);
        }
    }

    public class WeatherMakerCloudNoiseEditor : MonoBehaviour
    {

    }
}
