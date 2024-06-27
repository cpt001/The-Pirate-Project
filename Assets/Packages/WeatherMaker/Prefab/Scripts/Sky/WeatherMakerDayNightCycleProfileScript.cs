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
    /// Day night cycle profile, contains all fields and properties for time of day and how they effect everything
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerDayNightCycleProfile", menuName = "WeatherMaker/Day-Night Cycle Profile", order = 41)]
    public class WeatherMakerDayNightCycleProfileScript : ScriptableObject
    {
        #region Classes

        private class WebClientWithTimeout : System.Net.WebClient
        {
            protected override System.Net.WebRequest GetWebRequest(Uri uri)
            {
                System.Net.WebRequest w = base.GetWebRequest(uri);
                w.Timeout = Timeout;
                return w;
            }

            /// <summary>
            /// Milliseconds
            /// </summary>
            public int Timeout { get; set; }
        }

        /// <summary>
        /// Sun orbit information
        /// </summary>
        public class SunInfo
        {
            /// <summary>
            /// Calculation parameter, the date/time on the observer planet
            /// </summary>
            public DateTime DateTime;

            /// <summary>
            /// Calculation parameter, latitude of observer planet in degrees
            /// </summary>
            public double Latitude;

            /// <summary>
            /// Calculation parameter, longitude of observer planet in degrees
            /// </summary>
            public double Longitude;

            /// <summary>
            /// Calculation parameter, axis tilt of observer planet in degrees
            /// </summary>
            public double AxisTilt;

            /// <summary>
            /// Position (unit vector) of the sun in the sky from origin
            /// </summary>
            public Vector3 UnitVectorUp;

            /// <summary>
            /// Normal (unit vector) of the sun in the sky pointing to origin (negation of Position)
            /// </summary>
            public Vector3 UnitVectorDown;

            /// <summary>
            /// Time of dawn
            /// </summary>
            public TimeSpan Dawn;

            /// <summary>
            /// Time of sunrise
            /// </summary>
            public TimeSpan SunRise;

            /// <summary>
            /// Time of sunset
            /// </summary>
            public TimeSpan SunSet;

            /// <summary>
            /// Time of dusk
            /// </summary>
            public TimeSpan Dusk;

            /// <summary>
            /// JulianDays
            /// </summary>
            public double JulianDays;

            /// <summary>
            /// Declination
            /// </summary>
            public double Declination;

            /// <summary>
            /// RightAscension
            /// </summary>
            public double RightAscension;

            /// <summary>
            /// Azimuth
            /// </summary>
            public double Azimuth;

            /// <summary>
            /// Altitude
            /// </summary>
            public double Altitude;

            /// <summary>
            /// SolarMeanAnomaly
            /// </summary>
            public double SolarMeanAnomaly;

            /// <summary>
            /// EclipticLongitude
            /// </summary>
            public double EclipticLongitude;

            /// <summary>
            /// SiderealTime
            /// </summary>
            public double SiderealTime;
        }

        /// <summary>
        /// Moon orbit information
        /// </summary>
        public class MoonInfo
        {
            /// <summary>
            /// The sun data used to calculate the moon info
            /// </summary>
            public SunInfo SunData;

            /// <summary>
            /// Position (unit vector) of the moon in the sky from origin
            /// </summary>
            public Vector3 UnitVectorUp;

            /// <summary>
            /// Normal (unit vector) of the moon in the sky pointing to origin (negation of Position)
            /// </summary>
            public Vector3 UnitVectorDown;

            /// <summary>
            /// Distance in kilometers
            /// </summary>
            public double Distance;

            /// <summary>
            /// Moon illumination phase (0.5 is full, 0.0 to 1.0)
            /// </summary>
            public double Phase;

            /// <summary>
            /// Percent (0 to 1) that moon is full
            /// </summary>
            public double PercentFull;

            /// <summary>
            /// Moon illumination angle
            /// </summary>
            public double Angle;

            /// <summary>
            /// Moon illumination fraction
            /// </summary>
            public double Fraction;

            /// <summary>
            /// Azimuth
            /// </summary>
            public double Azimuth;

            /// <summary>
            /// Altitude
            /// </summary>
            public double Altitude;

            /// <summary>
            /// RightAscension
            /// </summary>
            public double RightAscension;

            /// <summary>
            /// Declination
            /// </summary>
            public double Declination;

            /// <summary>
            /// LunarMeanAnomaly
            /// </summary>
            public double LunarMeanAnomaly;

            /// <summary>
            /// EclipticLongitude
            /// </summary>
            public double EclipticLongitude;

            /// <summary>
            /// SiderealTime
            /// </summary>
            public double SiderealTime;

            /// <summary>
            /// ParallacticAngle
            /// </summary>
            public double ParallacticAngle;
        }

        #endregion Classes

        /// <summary>The day speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.</summary>
        [Header("Day/night cycle")]
        [Range(-100000, 100000.0f)]
        [Tooltip("The day speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float Speed = 10.0f;

        /// <summary>The night speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.</summary>
        [Range(-100000, 100000.0f)]
        [Tooltip("The night speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float NightSpeed = 10.0f;

        /// <summary>How often the update cycle updates. Use higher values if you have issues with shadow flickering, etc. Turning on temporal anti-aliasing is also a good way to reduce flicker, though you may have to play with the settings.</summary>
        [Tooltip("How often the update cycle updates. Use higher values if you have issues with shadow flickering, etc. Turning on temporal anti-aliasing " +
            "is also a good way to reduce flicker, though you may have to play with the settings.")]
        [Range(0.0f, 10.0f)]
        public float UpdateInterval = 0.03f;

        /// <summary>
        /// How much time has accumulated for the next update
        /// </summary>
        private float accumulatedTime = 10.0f;

        /// <summary>The current time of day in seconds (local time).</summary>
        [Range(0.0f, SecondsPerDay)]
        [Tooltip("The current time of day in seconds (local time).")]
        public float TimeOfDay = SecondsPerDay * 0.5f; // high noon default time of day

#if UNITY_EDITOR

#pragma warning disable 0414

        [ReadOnlyLabel]
        [SerializeField]
        internal string TimeOfDayLabel = string.Empty;

#pragma warning restore 0414

#endif

        /// <summary>The year for simulating the sun and moon position - this can change during runtime. The calculation is only correct for dates in the range March 1 1900 to February 28 2100.</summary>
        [Header("Date")]
        [Tooltip("The year for simulating the sun and moon position - this can change during runtime. " +
            "The calculation is only correct for dates in the range March 1 1900 to February 28 2100.")]
        [Range(1900, 2100)]
        public int Year = 2000;

        /// <summary>The month for simulating the sun and moon position - this can change during runtime.</summary>
        [Tooltip("The month for simulating the sun and moon position - this can change during runtime.")]
        [Range(1, 12)]
        public int Month = 9;

        /// <summary>The day for simulating the sun and moon position - this can change during runtime.</summary>
        [Tooltip("The day for simulating the sun and moon position - this can change during runtime.")]
        [Range(1, 31)]
        public int Day = 21;

        /// <summary>Whether to adjust the date when the day ends. This is important to maintain accurate sun and moon positions as days begin and end, but if your time is static you can turn it off.</summary>
        [Tooltip("Whether to adjust the date when the day ends. This is important to maintain accurate sun and moon positions as days begin and end, but if your time is static you can turn it off.")]
        public bool AdjustDateWhenDayEnds = true;

        /// <summary>Offset for the time zone of the lat / lon in seconds. Set to -1111 to auto-calculate (just tab out of the text field after you type -1111). Note about -1111: during editor mode, a web service is used. During play mode, longitude is used for fast calculation.</summary>
        [Tooltip("Offset for the time zone of the lat / lon in seconds. Set to -1111 to auto-calculate (just tab out of the text field after you type -1111). Note about -1111: during editor mode, a web service is used. During play mode, longitude is used for fast calculation.")]
        public int TimeZoneOffsetSeconds = -21600;

        /// <summary>The latitude in degrees on the planet that the camera is at - 90 (north pole) to -90 (south pole)</summary>
        [Header("Location")]
        [Range(-90.0f, 90.0f)]
        [Tooltip("The latitude in degrees on the planet that the camera is at - 90 (north pole) to -90 (south pole)")]
        public double Latitude = 40.7608; // salt lake city latitude

        /// <summary>The longitude in degrees on the planet that the camera is at. -180 to 180.</summary>
        [Range(-180.0f, 180.0f)]
        [Tooltip("The longitude in degrees on the planet that the camera is at. -180 to 180.")]
        public double Longitude = -111.8910; // salt lake city longitude

        /// <summary>The amount of degrees your planet is tilted - Earth is about 23.439f</summary>
        [Range(0.0f, 360.0f)]
        [Tooltip("The amount of degrees your planet is tilted - Earth is about 23.439f")]
        public float AxisTilt = 23.439f;

        /// <summary>Determines when it is day, dawn/dusk or night, where center of gradient is sun at horizon. Green = day, Red = dawn/dusk, Blue = night. If it's dawn/dusk, the time of day before 12 p.m. will be dawn, after 12 p.m. will be dusk.</summary>
        [Header("Time of Day Mapping")]
        [Tooltip("Determines when it is day, dawn/dusk or night, where center of gradient is sun at horizon. Green = day, Red = dawn/dusk, Blue = night. If it's dawn/dusk, the time of day before 12 p.m. will be dawn, after 12 p.m. will be dusk.")]
        public Gradient DayDawnDuskNightGradient;

        /// <summary>Sky tint color, center of gradient is sun at horizon.</summary>
        [Tooltip("Sky tint color, center of gradient is sun at horizon.")]
        public Gradient SkyTintColor;

        /// <summary>Sky add color, center of gradient is sun at horizon.</summary>
        [Tooltip("Sky add color, center of gradient is sun at horizon.")]
        public Gradient SkyAddColor;

        /// <summary>Whether to set the Unity built in ambient colors to the day, dawn/dusk and night ambient colors.</summary>
        [Header("Ambient colors")]
        [Tooltip("Whether to set the Unity built in ambient colors to the day, dawn/dusk and night ambient colors.")]
        public WeatherMakerDayNightAmbientColorMode AmbientColorMode = WeatherMakerDayNightAmbientColorMode.AmbientColor;

        /// <summary>Day ambient color, where far right is fully day - alpha is used for intensity</summary>
        [Header("Ambient colors - day")]
        [Tooltip("Day ambient color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColor;

        /// <summary>Additional day ambient color intensity</summary>
        [Tooltip("Additional day ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorIntensity = 0.05f;

        /// <summary>Day ambient sky color, where far right is fully day - alpha is used for intensity</summary>
        [Tooltip("Day ambient sky color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorSky;

        /// <summary>Additional day ambient sky color intensity</summary>
        [Tooltip("Additional day ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorSkyIntensity = 0.05f;

        /// <summary>Day ambient ground color, where far right is fully day - alpha is used for intensity</summary>
        [Tooltip("Day ambient ground color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorGround;

        /// <summary>Additional day ambient ground color intensity</summary>
        [Tooltip("Additional day ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorGroundIntensity = 0.05f;

        /// <summary>Day ambient equator color, where far right is fully day - alpha is used for intensity</summary>
        [Tooltip("Day ambient equator color, where far right is fully day - alpha is used for intensity")]
        public Gradient DayAmbientColorEquator;

        /// <summary>Additional day ambient equator color intensity</summary>
        [Tooltip("Additional day ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float DayAmbientColorEquatorIntensity = 0.05f;

        /// <summary>Dawn/dusk ambient color, where far right is fully dawn or dusk - alpha is used for intensity</summary>
        [Header("Ambient colors - dawn/dusk")]
        [Tooltip("Dawn/dusk ambient color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColor;

        /// <summary>Additional daw/dusk ambient color intensity</summary>
        [Tooltip("Additional daw/dusk ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorIntensity = 0.05f;

        /// <summary>Dawn/dusk ambient sky color, where far right is fully dawn or dusk - alpha is used for intensity</summary>
        [Tooltip("Dawn/dusk ambient sky color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorSky;

        /// <summary>Additional daw/dusk ambient sky color intensity</summary>
        [Tooltip("Additional daw/dusk ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorSkyIntensity = 0.05f;

        /// <summary>Dawn/dusk ambient ground color, where far right is fully dawn or dusk - alpha is used for intensity</summary>
        [Tooltip("Dawn/dusk ambient ground color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorGround;

        /// <summary>Additional daw/dusk ambient ground color intensity</summary>
        [Tooltip("Additional daw/dusk ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorGroundIntensity = 0.05f;

        /// <summary>Dawn/dusk ambient equator color, where far right is fully dawn or dusk - alpha is used for intensity</summary>
        [Tooltip("Dawn/dusk ambient equator color, where far right is fully dawn or dusk - alpha is used for intensity")]
        public Gradient DawnDuskAmbientColorEquator;

        /// <summary>Additional daw/dusk ambient equator color intensity</summary>
        [Tooltip("Additional daw/dusk ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float DawnDuskAmbientColorEquatorIntensity = 0.05f;

        /// <summary>Night ambient color, where far right is fully night - alpha is used for intensity</summary>
        [Header("Ambient colors - night")]
        [Tooltip("Night ambient color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColor;

        /// <summary>Additional night ambient color intensity</summary>
        [Tooltip("Additional night ambient color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorIntensity = 0.05f;

        /// <summary>Night ambient sky color, where far right is fully night - alpha is used for intensity</summary>
        [Tooltip("Night ambient sky color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorSky;

        /// <summary>Additional night ambient sky color intensity</summary>
        [Tooltip("Additional night ambient sky color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorSkyIntensity = 0.05f;

        /// <summary>Night ambient ground color, where far right is fully night - alpha is used for intensity</summary>
        [Tooltip("Night ambient ground color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorGround;

        /// <summary>Additional night ambient ground color intensity</summary>
        [Tooltip("Additional night ambient ground color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorGroundIntensity = 0.05f;

        /// <summary>Night ambient equator color, where far right is fully night - alpha is used for intensity</summary>
        [Tooltip("Night ambient equator color, where far right is fully night - alpha is used for intensity")]
        public Gradient NightAmbientColorEquator;

        /// <summary>Additional night ambient equator color intensity</summary>
        [Tooltip("Additional night ambient equator color intensity")]
        [Range(0.0f, 10.0f)]
        public float NightAmbientColorEquatorIntensity = 0.05f;

        /// <summary>Tint color of sun, where center is sun at horizon</summary>
        [Header("Sun modifiers")]
        [Tooltip("Tint color of sun, where center is sun at horizon")]
        public Gradient SunTintColorGradient;

        /// <summary>Control sun intensity where center of gradient is sun at horizon</summary>
        [Tooltip("Control sun intensity where center of gradient is sun at horizon")]
        public Gradient SunIntensityGradient;

        /// <summary>
        /// Time of day as a TimeSpan object
        /// </summary>
        public TimeSpan TimeOfDayTimespan { get; private set; }

        /// <summary>
        /// Time of day category
        /// </summary>
        public WeatherMakerTimeOfDayCategory TimeOfDayCategory { get; private set; }

        /// <summary>
        /// 1 if it is fully day
        /// </summary>
        public float DayMultiplier { get; private set; }

        /// <summary>
        /// 1 if it is fully dawn or dusk
        /// </summary>
        public float DawnDuskMultiplier { get; private set; }

        /// <summary>
        /// 1 if it is fully night
        /// </summary>
        public float NightMultiplier { get; private set; }

        /// <summary>
        /// Current sun info
        /// </summary>
        [NonSerialized]
        public readonly SunInfo SunData = new SunInfo();

        /// <summary>
        /// Current moon info
        /// </summary>
        [NonSerialized]
        public readonly List<MoonInfo> MoonDatas = new List<MoonInfo>();

        /// <summary>
        /// Number of seconds per day
        /// </summary>
        public const float SecondsPerDay = 86400.0f;

        /// <summary>
        /// Time of day at high noon
        /// </summary>
        public const float HighNoonTimeOfDay = SecondsPerDay * 0.5f;

        /// <summary>
        /// Number of seconds in one degree
        /// </summary>
        public const float SecondsForOneDegree = SecondsPerDay / 360.0f;

        /// <summary>
        /// How many seconds must elapse in time of day time to update dynamic GI should that option be selected
        /// </summary>
        public const float DynamicGIUpdateThresholdSeconds = 300.0f;

        private DateTime prevDt;
        /// <summary>
        /// Get a date time object representing the current year, month, day and time of day in local time
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                TimeSpan ts = TimeOfDayTimeSpan;
                return new DateTime(Year, Month, Day, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds, DateTimeKind.Local);
            }
            set
            {
                DateTime dt = value.ToLocalTime();
                Year = dt.Year;
                Month = dt.Month;
                Day = dt.Day;
                TimeOfDay = (float)dt.TimeOfDay.TotalSeconds;
            }
        }

        /// <summary>
        /// Get a TimeSpan from the TimeOfDay property
        /// </summary>
        public TimeSpan TimeOfDayTimeSpan { get { return TimeSpan.FromSeconds(TimeOfDay); } set { TimeOfDay = (float)value.TotalSeconds; } }

        private float lastTimeOfDayForDynamicGIUpdate = -999999.0f;

        /// <summary>
        /// Julian date to System.DateTime
        /// </summary>
        /// <param name="julianDate">Julian date</param>
        /// <returns>DateTime with UTC kind</returns>
        public static DateTime JulianToDateTime(double julianDate)
        {
            double unixTime = (julianDate - 2440587.5) * 86400;
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime);
            return dtDateTime;
        }

        /// <summary>
        /// Azimuth and atltidue unit vector
        /// </summary>
        /// <param name="azimuth">Azimuth</param>
        /// <param name="altitude">Altitude</param>
        /// <param name="vector">Unit vector</param>
        public static void ConvertAzimuthAtltitudeToUnitVector(double azimuth, double altitude, ref Vector3 vector)
        {
            vector.y = (float)Math.Sin(altitude);
            float hyp = (float)Math.Cos(altitude);
            vector.z = hyp * (float)Math.Cos(azimuth);
            vector.x = hyp * (float)Math.Sin(azimuth);
        }

        /// <summary>
        /// Calculate the position of the sun
        /// </summary>
        /// <param name="sunInfo">Calculates and receives sun info, including position, etc. Parameters marked as calculation parameters need to be set first.</param>
        /// <param name="rotateYDegrees">Rotate around the Y axis</param>
        public static void CalculateSunPosition(SunInfo sunInfo, float rotateYDegrees)
        {
            // dateTime should already be UTC format
            double d = (sunInfo.DateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / dayMs) + jDiff;
            //double d = sunInfo.DateTime.ToOADate() + 2415018.5;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double m = SolarMeanAnomaly(d);
            double l = EclipticLongitude(m);
            double dec = Declination(e, l, 0);
            double ra = RightAscension(e, l, 0);
            double lw = -degreesToRadians * sunInfo.Longitude;
            double phi = degreesToRadians * sunInfo.Latitude;
            double h = SiderealTime(d, lw) - ra;
            double azimuth = Azimuth(h, phi, dec);
            double altitude = Altitude(h, phi, dec);
            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref sunInfo.UnitVectorUp);

            sunInfo.UnitVectorUp = Quaternion.AngleAxis(rotateYDegrees, Vector3.up) * sunInfo.UnitVectorUp;
            sunInfo.UnitVectorDown = -sunInfo.UnitVectorUp;
            sunInfo.JulianDays = d;
            sunInfo.Declination = dec;
            sunInfo.RightAscension = ra;
            sunInfo.Azimuth = azimuth;
            sunInfo.Altitude = altitude;
            sunInfo.SolarMeanAnomaly = m;
            sunInfo.EclipticLongitude = l;
            sunInfo.SiderealTime = h;

            double n = JulianCycle(d, lw);
            double ds = ApproxTransit(0, lw, n);
            double jnoon = SolarTransit(ds, m, l);
            double jSunSet = JulianDateForSunAltitude(-0.8 * (Math.PI / 180.0), lw, phi, dec, n, m, l);
            double jSunRise = jnoon - (jSunSet - jnoon);
            double jDusk = JulianDateForSunAltitude(-10.0 * (Math.PI / 180.0), lw, phi, dec, n, m, l);
            double jDawn = jnoon - (jDusk - jnoon);

            try
            {
                sunInfo.Dawn = JulianToDateTime(jDawn).TimeOfDay;
                sunInfo.Dusk = JulianToDateTime(jDusk).TimeOfDay;
                sunInfo.SunRise = JulianToDateTime(jSunRise).TimeOfDay;
                sunInfo.SunSet = JulianToDateTime(jSunSet).TimeOfDay;
            }
            catch
            {
                // don't crash if date time is out of bounds
            }
        }

        /// <summary>
        /// Calculate moon position
        /// </summary>
        /// <param name="sunInfo">Sun info, already calculated</param>
        /// <param name="moonInfo">Receives moon info</param>
        /// <param name="rotateYDegrees">Rotate the moon in the sky around the y axis by this degrees</param>
        public static void CalculateMoonPosition(SunInfo sunInfo, MoonInfo moonInfo, float rotateYDegrees)
        {
            double d = sunInfo.JulianDays;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double L = degreesToRadians * (218.316 + 13.176396 * d); // ecliptic longitude
            double M = degreesToRadians * (134.963 + 13.064993 * d); // mean anomaly
            double F = degreesToRadians * (93.272 + 13.229350 * d); // mean distance
            double l = L + degreesToRadians * 6.289 * Math.Sin(M); // longitude
            double b = degreesToRadians * 5.128 * Math.Sin(F); // latitude
            double dist = 385001.0 - (20905.0 * Math.Cos(M)); // distance to the moon in km
            double ra = RightAscension(e, l, b);
            double dec = Declination(e, l, b);
            const double sunDistance = 149598000.0; // avg sun distance to Earth
            double phi = Math.Acos(Math.Sin(sunInfo.Declination) * Math.Sin(dec) + Math.Cos(sunInfo.Declination) * Math.Cos(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double inc = Math.Atan2(sunDistance * Math.Sin(phi), dist - sunDistance * Math.Cos(phi));
            double angle = Math.Atan2(Math.Cos(sunInfo.Declination) * Math.Sin(sunInfo.RightAscension - ra), Math.Sin(sunInfo.Declination) * Math.Cos(dec) - Math.Cos(sunInfo.Declination) * Math.Sin(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double fraction = (1.0 + Math.Cos(inc)) * 0.5;
            double phase = 0.5 + (0.5 * inc * Math.Sign(angle) * (1.0 / Math.PI));
            double lw = -degreesToRadians * sunInfo.Longitude;
            phi = degreesToRadians * sunInfo.Latitude;
            double H = SiderealTime(d, lw) - ra;
            double h = Altitude(H, phi, dec);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            double pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(dec) - Math.Sin(dec) * Math.Cos(H));
            h = h + AstroRefraction(h); // altitude correction for refraction
            double azimuth = Azimuth(H, phi, dec);
            double altitude = h;
            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref moonInfo.UnitVectorUp);

            // set moon position and look at the origin
            moonInfo.UnitVectorUp = Quaternion.AngleAxis(rotateYDegrees, Vector3.up) * moonInfo.UnitVectorUp;
            moonInfo.UnitVectorDown = -moonInfo.UnitVectorUp;
            moonInfo.Distance = dist;
            moonInfo.Phase = phase;
            moonInfo.PercentFull = 1.0 - Math.Abs((0.5 - phase) * 2.0);
            moonInfo.Angle = angle;
            moonInfo.Fraction = fraction;
            moonInfo.Azimuth = azimuth;
            moonInfo.Altitude = altitude;
            moonInfo.RightAscension = ra;
            moonInfo.Declination = dec;
            moonInfo.LunarMeanAnomaly = M;
            moonInfo.EclipticLongitude = L;
            moonInfo.SiderealTime = H;
            moonInfo.ParallacticAngle = pa;
        }

        private const double degreesToRadians = Math.PI / 180.0;
        private const double dayMs = 1000.0 * 60.0 * 60.0 * 24.0;
        private const double j0 = 0.0009;
        private const double j1970 = 2440587.5;
        private const double j2000 = 2451545.0;
        private const double jDiff = (j1970 - j2000);

        private static double RightAscension(double e, double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }

        private static double Declination(double e, double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        private static double Azimuth(double h, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        private static double Altitude(double h, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
        }

        private static double SiderealTime(double d, double lw)
        {
            return degreesToRadians * (280.16 + 360.9856235 * d) - lw;
        }

        private static double SolarMeanAnomaly(double d)
        {
            return degreesToRadians * (357.5291 + 0.98560028 * d);
        }

        private static double EclipticLongitude(double m)
        {
            double c = degreesToRadians * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2.0 * m) + 0.0003 * Math.Sin(3.0 * m)); // equation of center
            double p = degreesToRadians * 102.9372; // perihelion of the Earth
            return m + c + p + Math.PI;
        }

        private static double AstroRefraction(double h)
        {
            // the following formula works for positive altitudes only.
            // if h = -0.08901179 a div/0 would occur.
            h = (h < 0.0 ? 0.0 : h);

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }

        private static double JulianCycle(double d, double lw)
        {
            return Math.Round(d - j0 - lw / (2 * Math.PI));
        }

        private static double ApproxTransit(double Ht, double lw, double n)
        {
            return j0 + (Ht + lw) / (2 * Math.PI) + n;
        }

        private static double SolarTransit(double ds, double M, double L)
        {
            return j2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2 * L);
        }

        private static double HourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }

        private static double JulianDateForSunAltitude(double h, double lw, double phi, double dec, double n, double M, double L)
        {
            double w = HourAngle(h, phi, dec);
            double a = ApproxTransit(w, lw, n);
            return SolarTransit(a, M, L);
        }

        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return (2 * Math.PI) + angleInRadians;
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians - (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }

#if UNITY_EDITOR

        private DateTime lastTimeZoneCheck = DateTime.MinValue;

#endif

        private void SetCelestialObjectPosition(WeatherMakerCelestialObjectScript obj, Vector3 transformForward)
        {
            if (obj.OrbitType == WeatherMakerOrbitType.CustomOrthographic || obj.OrbitType == WeatherMakerOrbitType.CustomPerspective)
            {
                return;
            }
            obj.transform.forward = (transformForward == Vector3.zero ? obj.transform.forward : transformForward);
        }

        private void UpdateSuns(CameraMode mode, IList<WeatherMakerCelestialObjectScript> suns)
        {
            foreach (WeatherMakerCelestialObjectScript sun in suns)
            {
                if (sun == null)
                {
                    return;
                }
                else if (mode == CameraMode.OrthographicXY && !sun.OrbitTypeIsPerspective)
                {
                    if (!sun.OrbitTypeIsCustom)
                    {
                        sun.transform.rotation = Quaternion.AngleAxis(180.0f + ((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.right);
                    }
                    if (sun.LightMode != WeatherMakerCelestialObjectLightMode.None)
                    {
                        float dot = Mathf.Clamp(Vector3.Dot(sun.transform.forward, Vector3.forward) + 0.5f, 0.0f, 1.0f);
                        sun.Light.intensity = sun.LightBaseIntensity * dot;
                    }
                }
                else if (mode == CameraMode.OrthographicXZ && !sun.OrbitTypeIsPerspective)
                {
                    if (!sun.OrbitTypeIsCustom)
                    {
                        sun.transform.rotation = Quaternion.AngleAxis(180.0f + ((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.forward);
                    }
                    if (sun.LightMode != WeatherMakerCelestialObjectLightMode.None)
                    {
                        float dot = Mathf.Clamp(Vector3.Dot(sun.transform.forward, Vector3.up) + 0.5f, 0.0f, 1.0f);
                        sun.Light.intensity = sun.LightBaseIntensity * dot;
                    }
                }
                else
                {
                    // convert local time of day to UTC time of day - quick and dirty calculation
                    double offsetSeconds = TimeZoneOffsetSeconds;
                    TimeSpan t = TimeSpan.FromSeconds(TimeOfDay - offsetSeconds);
                    SunData.DateTime = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc) + t;
                    SunData.Latitude = Latitude;
                    SunData.Longitude = Longitude;
                    SunData.AxisTilt = AxisTilt;

                    // calculate and set sun position in sky
                    if (sun.OrbitType == WeatherMakerOrbitType.FromEarth)
                    {
                        CalculateSunPosition(SunData, sun.RotateYDegrees);
                    }
                    SetCelestialObjectPosition(sun, SunData.UnitVectorDown);

                    // calculate sun intensity and shadow strengths
                    float l = sun.GetGradientLookup();
                    float sunIntensityLookup = SunIntensityGradient.Evaluate(l).grayscale;
                    sunIntensityLookup *= sunIntensityLookup;
                    sun.Light.color = sun.LightColor * SunTintColorGradient.Evaluate(l);
                    sun.Light.shadowStrength = sun.LightBaseShadowStrength;
                    if (sun.LightMode != WeatherMakerCelestialObjectLightMode.None)
                    {
                        sun.Light.intensity = sun.LightBaseIntensity * sunIntensityLookup;
                    }
                }
            }
        }

        private void UpdateMoons(CameraMode mode, WeatherMakerCelestialObjectScript sun, IList<WeatherMakerCelestialObjectScript> moons)
        {
            float dot, yPower;
            float sunIntensityReducer = (sun == null ? 1.0f : 1.0f - Mathf.Min(1.0f, Mathf.Pow(sun.Light.intensity, 0.2f)));
            while (MoonDatas.Count > moons.Count)
            {
                MoonDatas.RemoveAt(MoonDatas.Count - 1);
            }
            while (MoonDatas.Count < moons.Count)
            {
                MoonDatas.Add(new MoonInfo());
            }

            for (int i = 0; i < moons.Count; i++)
            {
                WeatherMakerCelestialObjectScript moon = moons[i];
                if (moon == null)
                {
                    continue;
                }
                else if (mode == CameraMode.OrthographicXY && !moon.OrbitTypeIsPerspective)
                {
                    moon.transform.rotation = Quaternion.AngleAxis(((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.right);
                    dot = Mathf.Clamp(Vector3.Dot(moon.transform.forward, Vector3.forward) + 0.5f, 0.0f, 1.0f);
                }
                else if (mode == CameraMode.OrthographicXZ && !moon.OrbitTypeIsPerspective)
                {
                    moon.transform.rotation = Quaternion.AngleAxis(((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.forward);
                    dot = Mathf.Clamp(Vector3.Dot(moon.transform.forward, Vector3.up) + 0.5f, 0.0f, 1.0f);
                }
                else if (moon.OrbitType == WeatherMakerOrbitType.FromEarth)
                {
                    CalculateMoonPosition(SunData, MoonDatas[i], moon.RotateYDegrees);
                }

                if (!moon.OrbitTypeIsCustom)
                {
                    SetCelestialObjectPosition(moon, MoonDatas[i].UnitVectorDown);
                }

                // intensity raises squared compare to moon fullness - this means less full is squared amount of less light
                // moon light intensity reduces as sun light intensity approaches 1
                // reduce moon light as it drops below horizon
                dot = Mathf.Clamp(Vector3.Dot(MoonDatas[i].UnitVectorDown, Vector3.down) + 0.2f, 0.0f, 1.0f);
                dot = Mathf.Pow(dot, 0.25f);
                yPower = Mathf.Clamp((MoonDatas[i].UnitVectorUp.y + 0.2f) * 4.0f, 0.0f, 1.0f);
                moon.Light.color = moon.LightColor;
                moon.Light.intensity = moon.LightBaseIntensity * yPower * sunIntensityReducer * dot * moon.LightMultiplier;
                if (moon.OrbitType == WeatherMakerOrbitType.FromEarth)
                {
                    // as moon goes away from full, intensity reduces squared
                    moon.Light.intensity *= ((float)MoonDatas[i].PercentFull * (float)MoonDatas[i].PercentFull);
                }
                moon.Light.shadowStrength = moon.LightBaseShadowStrength;
            }
        }

        private void ProcessCelestialObject(WeatherMakerCelestialObjectScript obj,
            Dictionary<string, float> directionalLightIntensityModifiers,
            Dictionary<string, float> directionalLightShadowModifier)
        {
            if (obj != null)
            {
                foreach (KeyValuePair<string, float> multiplier in directionalLightIntensityModifiers)
                {
                    obj.Light.intensity *= multiplier.Value;
                }
                foreach (KeyValuePair<string, float> multiplier in directionalLightShadowModifier)
                {
                    obj.Light.shadowStrength *= multiplier.Value;
                }
                obj.UpdateObject();
            }
        }

        private void UpdateLightIntensitiesAndShadows(WeatherMakerCelestialObjectScript sun, ICollection<WeatherMakerCelestialObjectScript> moons, 
            Dictionary<string, float> directionalLightIntensityModifiers,
            Dictionary<string, float> directionalLightShadowModifier)
        {
            ProcessCelestialObject(sun, directionalLightIntensityModifiers, directionalLightShadowModifier);
            foreach (WeatherMakerCelestialObjectScript obj in moons)
            {
                ProcessCelestialObject(obj, directionalLightIntensityModifiers, directionalLightShadowModifier);
            }
        }

        private void UpdateDayMultipliers(CameraMode mode, WeatherMakerCelestialObjectScript sun, float l)
        {
            Color lookup = DayDawnDuskNightGradient.Evaluate(l);
            DayMultiplier = lookup.g;
            DawnDuskMultiplier = lookup.r;
            NightMultiplier = lookup.b;
            WeatherMakerTimeOfDayCategory current = TimeOfDayCategory;
            TimeOfDayCategory = WeatherMakerTimeOfDayCategory.None;
            if (DayMultiplier > 0.0f)
            {
                TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Day;
            }
            if (DawnDuskMultiplier > 0.0f)
            {
                if (TimeOfDay > (SecondsPerDay * 0.5))
                {
                    TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Dusk;
                }
                else
                {
                    TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Dawn;
                }
            }
            if (NightMultiplier > 0.0f)
            {
                TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Night;
            }

            if (mode == CameraMode.OrthographicXY)
            {
                float z = sun.transform.forward.z;
                if (z >= -0.1f && z <= 0.1f)
                {
                    if (TimeOfDay > (SecondsPerDay * 0.5f))
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunset;
                    }
                    else
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunrise;
                    }
                }
            }
            else if (mode == CameraMode.OrthographicXZ)
            {
                float x = sun.transform.forward.x;
                if (x >= -0.1f && x <= 0.1f)
                {
                    if (TimeOfDay > (SecondsPerDay * 0.5f))
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunset;
                    }
                    else
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunrise;
                    }
                }
            }
            else
            {
                float y = sun.transform.forward.y;
                if (y >= -0.05f && y <= 0.2f)
                {
                    if (TimeOfDay > (SecondsPerDay * 0.5f))
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunset;
                    }
                    else
                    {
                        TimeOfDayCategory |= WeatherMakerTimeOfDayCategory.Sunrise;
                    }
                }
            }

            // send events
            if ((current & WeatherMakerTimeOfDayCategory.Day) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Day) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.DayBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Night) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Night) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.NightBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Dawn) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Dawn) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.DawnBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Dusk) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Dusk) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.DuskBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Sunrise) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Sunrise) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.SunriseBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Sunrise) != WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Sunrise) == WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.SunriseEnd.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Sunset) == WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Sunset) != WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.SunsetBegin.Invoke(this);
            }
            if ((current & WeatherMakerTimeOfDayCategory.Sunset) != WeatherMakerTimeOfDayCategory.None && (TimeOfDayCategory & WeatherMakerTimeOfDayCategory.Sunset) == WeatherMakerTimeOfDayCategory.None)
            {
                WeatherMakerScript.Instance.SunsetEnd.Invoke(this);
            }
        }

        private void UpdateTimeZone()
        {
            if (TimeZoneOffsetSeconds == -1111)
            {
                TimeZoneOffsetSeconds = (int)(Longitude * 24 / 360) * 3600;

#if UNITY_EDITOR

                if ((DateTime.UtcNow - lastTimeZoneCheck).TotalSeconds > 10.0)
                {
                    lastTimeZoneCheck = DateTime.UtcNow;
                    WebClientWithTimeout c = new WebClientWithTimeout();
                    c.Timeout = 3000;
                    TimeSpan unixTimeSpan = new DateTime(Year, Month, Day, 1, 1, 1, DateTimeKind.Utc) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    string url = "http://api.timezonedb.com/v2/get-time-zone?by=position&lat=" + Latitude + "&lng=" + Longitude + "&time=" + (long)unixTimeSpan.TotalSeconds + "&key=1H9B390ZKKPX";
                    try
                    {
                        c.DownloadStringCompleted += (o, e) =>
                        {
                            string xml = e.Result;
                            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(xml, @"\<gmtOffset\>(?<gmtOffset>.*?)\</gmtOffset\>");
                            if (m.Success)
                            {
                                TimeZoneOffsetSeconds = int.Parse(m.Groups["gmtOffset"].Value);
                                WeatherMakerScript.QueueOnMainThread(() =>
                                {
                                    SerializationHelper.SetDirty(this);
                                });
                            }
                        };
                        c.DownloadStringAsync(new System.Uri(url));
                    }
                    catch
                    {
                        // eat exceptions
                    }
                }

#endif

            }
        }

        private void UpdateTimeOfDay(bool updateTimeOfDay)
        {
            UpdateTimeZone();
            prevDt = DateTime;

#if UNITY_EDITOR

            TimeOfDayLabel = DateTime.Today.Add(TimeSpan.FromSeconds(TimeOfDay)).ToString("hh:mm tt");

#endif

            if (!updateTimeOfDay || !Application.isPlaying)
            {
                return;
            }

            if (NightMultiplier != 1.0f && Speed != 0.0f)
            {
                TimeOfDay += (Speed * accumulatedTime);
            }
            else if (NightMultiplier == 1.0f && NightSpeed != 0.0f)
            {
                TimeOfDay += (NightSpeed * accumulatedTime);
            }
            if (AdjustDateWhenDayEnds)
            {
                // handle wrapping of time of day
                if (TimeOfDay < 0.0f)
                {
                    TimeOfDay += SecondsPerDay;
                    DateTime dt = new DateTime(Year, Month, Day) - TimeSpan.FromDays(1.0) + TimeSpan.FromSeconds(TimeOfDay);
                    Year = dt.Year;
                    Month = dt.Month;
                    Day = dt.Day;
                }
                else if (TimeOfDay >= SecondsPerDay)
                {
                    TimeOfDay -= SecondsPerDay;
                    DateTime dt = new DateTime(Year, Month, Day) + TimeSpan.FromDays(1.0) + TimeSpan.FromSeconds(TimeOfDay);
                    Year = dt.Year;
                    Month = dt.Month;
                    Day = dt.Day;
                }
            }
            else if (TimeOfDay < 0.0f)
            {
                TimeOfDay += SecondsPerDay;
            }
            else if (TimeOfDay >= SecondsPerDay)
            {
                TimeOfDay -= SecondsPerDay;
            }
            TimeOfDayTimespan = TimeSpan.FromSeconds(TimeOfDay);

            // send events
            if (prevDt.Year != Year)
            {
                WeatherMakerScript.Instance.YearChanged.Invoke(this);
            }
            if (prevDt.Month != Month)
            {
                WeatherMakerScript.Instance.MonthChanged.Invoke(this);
            }
            if (prevDt.Day != Day)
            {
                WeatherMakerScript.Instance.DayChanged.Invoke(this);
            }
            if (prevDt.Hour != TimeOfDayTimespan.Hours)
            {
                WeatherMakerScript.Instance.HourChanged.Invoke(this);
            }
            if (prevDt.Minute != TimeOfDayTimespan.Minutes)
            {
                WeatherMakerScript.Instance.MinuteChanged.Invoke(this);
            }
            if (prevDt.Second != TimeOfDayTimespan.Seconds)
            {
                WeatherMakerScript.Instance.SecondChanged.Invoke(this);
            }
        }

        private void UpdateAmbientColors(float l)
        {

#if UNITY_EDITOR

            if (DayAmbientColor == null)
            {
                return;
            }

#endif

            Color dayAmbient = DayAmbientColor.Evaluate(l);
            Color dayAmbientSky = DayAmbientColorSky.Evaluate(l);
            Color dayAmbientGround = DayAmbientColorGround.Evaluate(l);
            Color dayAmbientEquator = DayAmbientColorEquator.Evaluate(l);
            Color dawnDuskAmbient = DawnDuskAmbientColor.Evaluate(l);
            Color dawnDuskAmbientSky = DawnDuskAmbientColorSky.Evaluate(l);
            Color dawnDuskAmbientGround = DawnDuskAmbientColorGround.Evaluate(l);
            Color dawnDuskAmbientEquator = DawnDuskAmbientColorEquator.Evaluate(l);
            Color nightAmbient = NightAmbientColor.Evaluate(l);
            Color nightAmbientSky = NightAmbientColorSky.Evaluate(l);
            Color nightAmbientGround = NightAmbientColorGround.Evaluate(l);
            Color nightAmbientEquator = NightAmbientColorEquator.Evaluate(l);
            Color externalAmbient = Color.clear;

            if (WeatherMakerFullScreenCloudsScript.Instance != null && WeatherMakerFullScreenCloudsScript.Instance.CloudProfile != null &&
                WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.AuroraProfile != null && WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.AuroraProfile.AuroraEnabled)
            {
                externalAmbient = WeatherMakerFullScreenCloudsScript.Instance.CloudProfile.AuroraProfile.AnimationAmbientColor * NightMultiplier * Mathf.Max(NightAmbientColorIntensity, NightAmbientColorSkyIntensity);
            }

            Color ambientLight = (dayAmbient * DayMultiplier * dayAmbient.a * DayAmbientColorIntensity) +
                (dawnDuskAmbient * DawnDuskMultiplier * dawnDuskAmbient.a * DawnDuskAmbientColorIntensity) +
                (nightAmbient * NightMultiplier * nightAmbient.a * NightAmbientColorIntensity) +
                externalAmbient;
            Color ambientLightSky = (dayAmbientSky * DayMultiplier * dayAmbientSky.a * DayAmbientColorSkyIntensity) +
                (dawnDuskAmbientSky * DawnDuskMultiplier * dawnDuskAmbientSky.a * DawnDuskAmbientColorSkyIntensity) +
                (nightAmbientSky * NightMultiplier * nightAmbientSky.a * NightAmbientColorSkyIntensity) +
                externalAmbient;
            Color ambientLightGround = (dayAmbientGround * DayMultiplier * dayAmbientGround.a * DayAmbientColorGroundIntensity) +
                (dawnDuskAmbientGround * DawnDuskMultiplier * dawnDuskAmbientGround.a * DawnDuskAmbientColorGroundIntensity) +
                (nightAmbientGround * NightMultiplier * nightAmbientGround.a * NightAmbientColorGroundIntensity) +
                externalAmbient;
            Color ambientLightEquator = (dayAmbientEquator * DayMultiplier * dayAmbientEquator.a * DayAmbientColorEquatorIntensity) +
                (dawnDuskAmbientEquator * DawnDuskMultiplier * dawnDuskAmbientEquator.a * DawnDuskAmbientColorEquatorIntensity) +
                (nightAmbientEquator * NightMultiplier * nightAmbientEquator.a * NightAmbientColorEquatorIntensity) +
                externalAmbient;

            Shader.SetGlobalFloat(WMS._WeatherMakerDayMultiplier, DayMultiplier);
            Shader.SetGlobalFloat(WMS._WeatherMakerDayMultiplierInv, 1.0f - DayMultiplier);
            Shader.SetGlobalFloat(WMS._WeatherMakerDawnDuskMultiplier, DawnDuskMultiplier);
            Shader.SetGlobalFloat(WMS._WeatherMakerDawnDuskMultiplierInv, 1.0f - DawnDuskMultiplier);
            Shader.SetGlobalFloat(WMS._WeatherMakerNightMultiplier, NightMultiplier);
            Shader.SetGlobalFloat(WMS._WeatherMakerNightMultiplierInv, 1.0f - NightMultiplier);
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                float lookup = (WeatherMakerLightManagerScript.Instance.SunPerspective != null ? WeatherMakerLightManagerScript.Instance.SunPerspective.GetGradientLookup() :
                    (WeatherMakerLightManagerScript.Instance.SunOrthographic != null ? WeatherMakerLightManagerScript.Instance.SunOrthographic.GetGradientLookup() : 0.0f));
                Color tintColor = WeatherMakerLightManagerScript.EvaluateGradient(SkyTintColor, lookup);
                Color addColor = WeatherMakerLightManagerScript.EvaluateGradient(SkyAddColor, lookup);
                Shader.SetGlobalColor(WMS._WeatherMakerSkyTintColor2, tintColor * tintColor.a);
                Shader.SetGlobalColor(WMS._WeatherMakerSkyAddColor, addColor * addColor.a);
            }

            switch (AmbientColorMode)
            {
                case WeatherMakerDayNightAmbientColorMode.AmbientColor:
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = ambientLight;
                    RenderSettings.ambientSkyColor = ambientLight;
                    RenderSettings.ambientEquatorColor = ambientLight;
                    RenderSettings.ambientGroundColor = ambientLight;
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, ambientLight);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, ambientLight);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, ambientLight);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, ambientLight);
                    break;

                case WeatherMakerDayNightAmbientColorMode.SkyOnly:
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = Color.black;
                    RenderSettings.ambientSkyColor = ambientLightSky;
                    RenderSettings.ambientEquatorColor = Color.black;
                    RenderSettings.ambientGroundColor = Color.black;
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, ambientLightSky);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, Color.black);
                    break;

                case WeatherMakerDayNightAmbientColorMode.SkyEquatorGround:
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                    RenderSettings.ambientLight = Color.black;
                    RenderSettings.ambientSkyColor = ambientLightSky;
                    RenderSettings.ambientEquatorColor = ambientLightEquator;
                    RenderSettings.ambientGroundColor = ambientLightGround;
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, ambientLightSky);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, ambientLightGround);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, ambientLightEquator);
                    break;

                case WeatherMakerDayNightAmbientColorMode.All:
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                    goto case WeatherMakerDayNightAmbientColorMode.AllWithUnityMode;

                case WeatherMakerDayNightAmbientColorMode.AllWithUnityMode:
                {
                    float maxColorComponent = ambientLight.maxColorComponent;
                    RenderSettings.ambientLight = ambientLight;
                    RenderSettings.ambientSkyColor = (ambientLightSky = (ambientLightSky.maxColorComponent >= maxColorComponent ? ambientLightSky : ambientLight));
                    RenderSettings.ambientGroundColor = (ambientLightGround = (ambientLightGround.maxColorComponent >= maxColorComponent ? ambientLightGround : ambientLight));
                    RenderSettings.ambientEquatorColor = (ambientLightEquator = (ambientLightEquator.maxColorComponent >= maxColorComponent ? ambientLightEquator : ambientLight));
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, ambientLight);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, ambientLightSky);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, ambientLightGround);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, ambientLightEquator);
                } break;

                case WeatherMakerDayNightAmbientColorMode.DynamicGIUpdateOnly:
                    if (Mathf.Abs(TimeOfDay - lastTimeOfDayForDynamicGIUpdate) > DynamicGIUpdateThresholdSeconds)
                    {
                        lastTimeOfDayForDynamicGIUpdate = TimeOfDay;
                        DynamicGI.UpdateEnvironment();
                    }
                    goto case WeatherMakerDayNightAmbientColorMode.UnityAmbientSettings;

                case WeatherMakerDayNightAmbientColorMode.None:
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = Color.black;
                    RenderSettings.ambientSkyColor = Color.black;
                    RenderSettings.ambientEquatorColor = Color.black;
                    RenderSettings.ambientGroundColor = Color.black;
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, Color.black);
                    Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, Color.black);
                    break;

                case WeatherMakerDayNightAmbientColorMode.UnityAmbientSettings:
                    if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Flat)
                    {
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, RenderSettings.ambientLight);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, RenderSettings.ambientLight);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, RenderSettings.ambientLight);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, RenderSettings.ambientLight);
                    }
                    else
                    {
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColor, RenderSettings.ambientLight);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorSky, RenderSettings.ambientSkyColor);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorGround, RenderSettings.ambientEquatorColor);
                        Shader.SetGlobalColor(WMS._WeatherMakerAmbientLightColorEquator, RenderSettings.ambientGroundColor);
                    }
                    break;
            }
        }

        /// <summary>
        /// Update day night cycle using profile settings and light manager state
        /// </summary>
        /// <param name="updateTimeOfDay"></param>
        public void UpdateFromProfile(bool updateTimeOfDay)
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            Camera mainCamera = WeatherMakerScript.Instance.MainCamera;
            WeatherMakerCelestialObjectScript sun = WeatherMakerLightManagerScript.SunForCamera(mainCamera);
            if (sun == null)
            {
                return;
            }

            accumulatedTime += Time.deltaTime;
            if (accumulatedTime > UpdateInterval)
            {
                Dictionary<string, float> directionalLightIntensityModifiers = WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers;
                Dictionary<string, float> directionalLightShadowModifier = WeatherMakerLightManagerScript.Instance.DirectionalLightShadowIntensityMultipliers;
                CameraMode mode = WeatherMakerScript.ResolveCameraMode();
                IList<WeatherMakerCelestialObjectScript> suns = WeatherMakerLightManagerScript.Instance.Suns;
                IList<WeatherMakerCelestialObjectScript> moons = WeatherMakerLightManagerScript.Instance.Moons;
                float l = sun.GetGradientLookup();
                UpdateAmbientColors(l);
                UpdateTimeOfDay(updateTimeOfDay);
                UpdateSuns(mode, suns);
                UpdateDayMultipliers(mode, sun, l);
                UpdateMoons(mode, sun, moons);
                accumulatedTime = 0.0f;
                UpdateLightIntensitiesAndShadows(sun, moons, directionalLightIntensityModifiers, directionalLightShadowModifier);
            }
        }
    }

    /// <summary>
    /// Day / night cycle ambient color mode
    /// </summary>
    public enum WeatherMakerDayNightAmbientColorMode
    {
        /// <summary>
        /// Do not use ambient colors, set them all to black. No Unity ambient light.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use the ambient color for ambient, sky, equator and ground. Use Unity flat ambient lighting.
        /// </summary>
        AmbientColor = 1,

        /// <summary>
        /// Use the ambient sky color only, all other ambient colors are black. Use Unity flat ambient lighting.
        /// </summary>
        SkyOnly = 2,

        /// <summary>
        /// Use the ambient sky, equator and ground. Ambient color is black. Use Unity trilight ambient lighting.
        /// </summary>
        SkyEquatorGround = 4,

        /// <summary>
        /// Use the ambient color plus ambient sky, equator and ground. Ambient color is added to sky, equator and ground. Use Unity trilight ambient lighting.
        /// </summary>
        All = 8,

        /// <summary>
        /// Only call DynamicGI.UpdateEnvironment periodically, using DynamicGIUpdateThresholdSeconds constant. Do not modify ambient colors or settings.
        /// </summary>
        DynamicGIUpdateOnly = 16,

        /// <summary>
        /// Leave Unity ambient mode as is. Use the ambient color plus ambient sky, equator and ground. Ambient color is added to sky, equator and ground.
        /// </summary>
        AllWithUnityMode = 32,

        /// <summary>
        /// Use existing ambient color settings, ignore day night cycle profile ambient settings.
        /// </summary>
        UnityAmbientSettings = 64
    }

    /// <summary>
    /// Time of day category
    /// </summary>
    [Flags]
    public enum WeatherMakerTimeOfDayCategory
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Dawn
        /// </summary>
        Dawn = 1,

        /// <summary>
        /// Day
        /// </summary>
        Day = 2,

        /// <summary>
        /// Dusk
        /// </summary>
        Dusk = 4,

        /// <summary>
        /// Night
        /// </summary>
        Night = 8,

        /// <summary>
        /// Sunrise
        /// </summary>
        Sunrise = 16,

        /// <summary>
        /// Sunset
        /// </summary>
        Sunset = 32
    }
}
