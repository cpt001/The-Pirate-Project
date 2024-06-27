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
    /// Random generator interface
    /// </summary>
    public interface IRandomizer
    {
        /// <summary>
        /// Random number
        /// </summary>
        /// <param name="min">Min inclusive</param>
        /// <param name="max">Max inclusive</param>
        /// <returns>Random number</returns>
        int Random(int min, int max);

        /// <summary>
        /// Random number
        /// </summary>
        /// <param name="min">Min inclusive</param>
        /// <param name="max">Max inclusive</param>
        /// <returns>Random number</returns>
        float Random(float min, float max);

        /// <summary>
        /// Random number of 0 to 1 inclusive
        /// </summary>
        /// <returns>Random number</returns>
        float Random();

        /// <summary>
        /// Random vector inside unit circle
        /// </summary>
        /// <returns>Random vector inside unit circle</returns>
        Vector2 RandomInsideUnitCircle();

        /// <summary>
        /// Random vector inside unit sphere
        /// </summary>
        /// <returns>Random vector inside unit sphere</returns>
        Vector3 RandomInsideUnitSphere();

        /// <summary>
        /// Random vector on unit sphere
        /// </summary>
        /// <returns>Random vector on unit sphere</returns>
        Vector3 RandomOnUnitSphere();

        /// <summary>
        /// Seed - must be positive int. If not set or unknown, -1 is returned.
        /// </summary>
        int Seed { get; set; }
    }

    /// <summary>
    /// Generate random numbers using weather maker random flow with option to log
    /// </summary>
    public class WeatherMakerRandomizer : IRandomizer
    {
        /// <summary>
        /// Generate random numbers using UnityEngine.Random
        /// </summary>
        private class WeatherMakerRandomizerUnity : IRandomizer
        {
            private int seed = -1;

            /// <summary>
            /// Constructor
            /// </summary>
            public WeatherMakerRandomizerUnity()
            {
            }

            /// <summary>
            /// Random number
            /// </summary>
            /// <param name="min">Min inclusive</param>
            /// <param name="max">Max inclusive</param>
            /// <returns>Random number</returns>
            public int Random(int min, int max)
            {
                return LogRandom(UnityEngine.Random.Range(min, max + 1));
            }

            /// <summary>
            /// Random number
            /// </summary>
            /// <param name="min">Min inclusive</param>
            /// <param name="max">Max inclusive</param>
            /// <returns>Random number</returns>
            public float Random(float min, float max)
            {
                return LogRandom(UnityEngine.Random.Range(min, max));
            }

            /// <summary>
            /// Random number of 0 to 1 inclusive
            /// </summary>
            /// <returns>Random number</returns>
            public float Random()
            {
                return LogRandom(UnityEngine.Random.value);
            }

            /// <summary>
            /// Random vector inside unit circle
            /// </summary>
            /// <returns>Random vector inside unit circle</returns>
            public Vector2 RandomInsideUnitCircle()
            {
                return LogRandom(UnityEngine.Random.insideUnitCircle);
            }

            /// <summary>
            /// Random vector inside unit sphere
            /// </summary>
            /// <returns>Random vector inside unit sphere</returns>
            public Vector3 RandomInsideUnitSphere()
            {
                return LogRandom(UnityEngine.Random.insideUnitSphere);
            }

            /// <summary>
            /// Random vector on unit sphere
            /// </summary>
            /// <returns>Random vector on unit sphere</returns>
            public Vector3 RandomOnUnitSphere()
            {
                return LogRandom(UnityEngine.Random.onUnitSphere);
            }

            /// <summary>
            /// Seed - must be positive int
            /// </summary>
            public int Seed
            {
                get { return seed; }
                set { UnityEngine.Random.InitState(seed = value); }
            }
        }

        /// <summary>
        /// Randomizer that uses UnityEngine.Random
        /// </summary>
        public static readonly IRandomizer Unity = new WeatherMakerRandomizerUnity();

        /// <summary>
        /// Randomizer that uses System.Random
        /// </summary>
        public static readonly IRandomizer System = new WeatherMakerRandomizer();

        private int seed = -1;
        private System.Random random = new System.Random();

        internal static T LogRandom<T>(T value)
        {

#if DEBUG

            //UnityEngine.Debug.LogFormat("Random value {0}", value);

#endif

            return value;
        }

        /// <inheritdoc />
        public int Random(int min, int max)
        {
            return LogRandom(random.Next(min, max + 1));
        }

        /// <inheritdoc />
        public float Random(float min, float max)
        {
            float v = (float)random.NextDouble();
            v = Mathf.Lerp(min, max, v);
            return LogRandom(v);
        }

        /// <inheritdoc />
        public float Random()
        {
            return LogRandom((float)random.NextDouble());
        }

        /// <inheritdoc />
        public Vector2 RandomInsideUnitCircle()
        {
            float angle = 2.0f * Mathf.PI * (float)random.NextDouble();
            float radius = Mathf.Sqrt((float)random.NextDouble());
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            return LogRandom(new Vector2(x, y));
        }

        /// <inheritdoc />
        public Vector3 RandomInsideUnitSphere()
        {
            var u = (float)random.NextDouble();
            var v = (float)random.NextDouble();
            var theta = u * 2.0f * Mathf.PI;
            var phi = Mathf.Acos(2.0f * v - 1.0f);
            //var r = Mathf.Cbrt(Math.random());
            var r2 = Mathf.Pow((float)random.NextDouble(), 1.0f / 3.0f);
            var sinTheta = Mathf.Sin(theta);
            var cosTheta = Mathf.Cos(theta);
            var sinPhi = Mathf.Sin(phi);
            var cosPhi = Mathf.Cos(phi);
            var x = r2 * sinPhi * cosTheta;
            var y = r2 * sinPhi * sinTheta;
            var z = r2 * cosPhi;
            return LogRandom(new Vector3(x, y, z));
        }

        /// <inheritdoc />
        public Vector3 RandomOnUnitSphere()
        {
            var u = (float)random.NextDouble();
            var v = (float)random.NextDouble();
            var theta = 2.0f * Mathf.PI * u;
            var phi = Mathf.Acos(2.0f * v - 1.0f);
            var x = (Mathf.Sin(phi) * Mathf.Cos(theta));
            var y = (Mathf.Sin(phi) * Mathf.Sin(theta));
            var z = (Mathf.Cos(phi));
            return LogRandom(new Vector3(x, y, z));
        }

        /// <inheritdoc />
        public int Seed
        {
            get { return seed; }
            set
            {
                if (seed != value)
                {
                    random = new System.Random(seed = value);
                }
            }
        }
    }
}
