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

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Day night cycle manager
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerDayNightCycleManagerScript : MonoBehaviour
    {
        /// <summary>Day night cycle profile and color scheme</summary>
        [Tooltip("Day night cycle profile and color scheme")]
        public WeatherMakerDayNightCycleProfileScript DayNightProfile;

#if UNITY_EDITOR

#pragma warning disable 0414

        [ReadOnlyLabel]
        [SerializeField]
        private string TimeOfDayLabel = string.Empty;

#pragma warning restore 0414

#endif

        /// <summary>
        /// Day speed
        /// </summary>
        public float Speed { get { return DayNightProfile.Speed; } set { DayNightProfile.Speed = value; } }

        /// <summary>
        /// Night speed
        /// </summary>
        public float NightSpeed { get { return DayNightProfile.NightSpeed; } set { DayNightProfile.NightSpeed = value; } }

        /// <summary>
        /// Get / set the time of day in seconds, 0 to 86400
        /// </summary>
        public float TimeOfDay { get { return DayNightProfile.TimeOfDay; } set { DayNightProfile.TimeOfDay = value; } }

        /// <summary>
        /// Time of day category
        /// </summary>
        public WeatherMakerTimeOfDayCategory TimeOfDayCategory { get { return DayNightProfile.TimeOfDayCategory; } }

        /// <summary>
        /// Time of day as a TimeSpan object
        /// </summary>
        public TimeSpan TimeOfDayTimespan { get { return DayNightProfile.TimeOfDayTimespan; } set { DayNightProfile.TimeOfDayTimeSpan = value; } }

        /// <summary>
        /// Time zone offset in seconds
        /// </summary>
        public int TimeZoneOffsetSeconds { get { return DayNightProfile.TimeZoneOffsetSeconds; } set { DayNightProfile.TimeZoneOffsetSeconds = value; } }

        /// <summary>
        /// Year
        /// </summary>
        public int Year {  get { return DayNightProfile.Year; } set { DayNightProfile.Year = value; } }

        /// <summary>
        /// Month
        /// </summary>
        public int Month { get { return DayNightProfile.Month; } set { DayNightProfile.Month = value; } }

        /// <summary>
        /// Day
        /// </summary>
        public int Day { get { return DayNightProfile.Day; } set { DayNightProfile.Day = value; } }

        /// <summary>
        /// Get a date time object representing the current year, month, day and time of day in local time
        /// </summary>
        public DateTime DateTime
        {
            get { return DayNightProfile.DateTime; }
            set { DayNightProfile.DateTime = value; }
        }

        /// <summary>
        /// Latitude in degrees
        /// </summary>
        public double Latitude { get { return DayNightProfile.Latitude; } set { DayNightProfile.Latitude = value; } }

        /// <summary>
        /// Longitude in degrees
        /// </summary>
        public double Longitude { get { return DayNightProfile.Longitude; } set { DayNightProfile.Longitude = value; } }

        /// <summary>
        /// 1 if it is fully day
        /// </summary>
        public float DayMultiplier { get { return DayNightProfile.DayMultiplier; } }

        /// <summary>
        /// 1 if it is fully dawn or dusk
        /// </summary>
        public float DawnDuskMultiplier { get { return DayNightProfile.DawnDuskMultiplier; } }

        /// <summary>
        /// 1 if it is fully night
        /// </summary>
        public float NightMultiplier { get { return DayNightProfile.NightMultiplier; } }

        /// <summary>
        /// Sun data
        /// </summary>
        public WeatherMakerDayNightCycleProfileScript.SunInfo SunData { get { return DayNightProfile.SunData; } }

        private void EnsureProfile()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            if (DayNightProfile == null)
            {
                DayNightProfile = WeatherMakerScript.Instance.LoadResource<WeatherMakerDayNightCycleProfileScript>("WeatherMakerDayNightCycleProfile_Default");
            }

            if (Application.isPlaying)
            {
                DayNightProfile = ScriptableObject.Instantiate(DayNightProfile);
            }
        }

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
        }

        private void Awake()
        {
            EnsureProfile();
        }

        private void Start()
        {
            EnsureProfile();
            DayNightProfile.UpdateFromProfile(WeatherMakerScript.Instance != null && WeatherMakerScript.Instance.NetworkConnection.IsServer);
        }

        private void Update()
        {
            DayNightProfile.UpdateFromProfile(WeatherMakerScript.Instance != null && WeatherMakerScript.Instance.NetworkConnection.IsServer);

#if UNITY_EDITOR

            TimeOfDayLabel = DayNightProfile.TimeOfDayLabel;

#endif

        }

        private void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        private static WeatherMakerDayNightCycleManagerScript instance;
        /// <summary>
        /// Shared instance of day/night cycle manager script
        /// </summary>
        public static WeatherMakerDayNightCycleManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance, true); }
        }

        /// <summary>
        /// Check if there is a day night manager instance
        /// </summary>
        /// <returns>True if instance, false otherwise</returns>
        public static bool HasInstance()
        {
            return instance != null;
        }
    }
}

// resources:
// https://en.wikipedia.org/wiki/Position_of_the_Sun
// http://stackoverflow.com/questions/8708048/position-of-the-sun-given-time-of-day-latitude-and-longitude
// http://www.grasshopper3d.com/forum/topics/solar-calculation-plugin
// http://guideving.blogspot.nl/2010/08/sun-position-in-c.html
// https://github.com/mourner/suncalc
// http://stackoverflow.com/questions/1058342/rough-estimate-of-the-time-offset-from-gmt-from-latitude-longitude
// http://www.stjarnhimlen.se/comp/tutorial.html
// http://www.suncalc.net/#/40.7608,-111.891,12/2000.09.21/12:46
// http://www.suncalc.net/scripts/suncalc.js

// total eclipse:
// 43.7678
// -111.8323
// Maximum eclipse : 	2017/08/21	17:34:18.6	49.5°	133.1°