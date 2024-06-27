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
using System.Text;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Generates lightning
    /// </summary>
    public class LightningGenerator
    {
        private void GetPerpendicularVector(ref Vector3 directionNormalized, out Vector3 side)
        {
            if (directionNormalized == Vector3.zero)
            {
                side = Vector3.right;
            }
            else
            {
                // use cross product to find any perpendicular vector around directionNormalized:
                // 0 = x * px + y * py + z * pz
                // => pz = -(x * px + y * py) / z
                // for computational stability use the component farthest from 0 to divide by
                float x = directionNormalized.x;
                float y = directionNormalized.y;
                float z = directionNormalized.z;
                float px, py, pz;
                float ax = Mathf.Abs(x), ay = Mathf.Abs(y), az = Mathf.Abs(z);
                if (ax >= ay && ay >= az)
                {
                    // x is the max, so we can pick (py, pz) arbitrarily at (1, 1):
                    py = 1.0f;
                    pz = 1.0f;
                    px = -(y * py + z * pz) / x;
                }
                else if (ay >= az)
                {
                    // y is the max, so we can pick (px, pz) arbitrarily at (1, 1):
                    px = 1.0f;
                    pz = 1.0f;
                    py = -(x * px + z * pz) / y;
                }
                else
                {
                    // z is the max, so we can pick (px, py) arbitrarily at (1, 1):
                    px = 1.0f;
                    py = 1.0f;
                    pz = -(x * px + y * py) / z;
                }
                side = new Vector3(px, py, pz).normalized;
            }
        }

        /// <summary>
        /// Callback for lightning bolt generation
        /// </summary>
        /// <param name="bolt">Lightning bolt to fill in</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="parameters"></param>
        protected virtual void OnGenerateLightningBolt(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters parameters)
        {
            GenerateLightningBoltStandard(bolt, start, end, parameters.Generations, parameters.Generations, 0.0f, parameters);
        }

        /// <summary>
        /// Determine if a fork should be created
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="generation">Generation</param>
        /// <param name="totalGenerations">Max generation</param>
        /// <returns>True to create fork, false otherwise</returns>
        public bool ShouldCreateFork(LightningBoltParameters parameters, int generation, int totalGenerations)
        {
            return (generation > parameters.generationWhereForksStop && generation >= totalGenerations - parameters.forkednessCalculated && (float)parameters.Random.NextDouble() < parameters.Forkedness);
        }

        /// <summary>
        /// Create a fork
        /// </summary>
        /// <param name="bolt">Lightning bolt</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="generation">Generation</param>
        /// <param name="totalGenerations">Total generations</param>
        /// <param name="start">Start</param>
        /// <param name="midPoint">Mid point</param>
        public void CreateFork(LightningBolt bolt, LightningBoltParameters parameters, int generation, int totalGenerations, Vector3 start, Vector3 midPoint)
        {
            if (ShouldCreateFork(parameters, generation, totalGenerations))
            {
                Vector3 branchVector = (midPoint - start) * parameters.ForkMultiplier();
                Vector3 splitEnd = midPoint + branchVector;
                GenerateLightningBoltStandard(bolt, midPoint, splitEnd, generation, totalGenerations, 0.0f, parameters);
            }
        }

        /// <summary>
        /// Generate a normal lightning bolt
        /// </summary>
        /// <param name="bolt">Lightning bolt</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="generation">Generation</param>
        /// <param name="totalGenerations">Total generations</param>
        /// <param name="offsetAmount">Offset amount</param>
        /// <param name="parameters">Parameters</param>
        public void GenerateLightningBoltStandard(LightningBolt bolt, Vector3 start, Vector3 end, int generation, int totalGenerations, float offsetAmount, LightningBoltParameters parameters)
        {
            if (generation < 1)
            {
                return;
            }

            LightningBoltSegmentGroup group = bolt.AddGroup();
            group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

            // every generation, get the percentage we have gone down and square it, this makes lines thinner
            float widthMultiplier = (float)generation / (float)totalGenerations;
            widthMultiplier *= widthMultiplier;

            Vector3 randomVector;
            group.LineWidth = parameters.TrunkWidth * widthMultiplier;
            group.Generation = generation;
            group.Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(255.0f * widthMultiplier));
            group.EndWidthMultiplier = parameters.EndWidthMultiplier * parameters.ForkEndWidthMultiplier;
            if (offsetAmount <= 0.0f)
            {
                offsetAmount = (end - start).magnitude * parameters.ChaosFactor;
            }

            while (generation-- > 0)
            {
                int previousStartIndex = group.StartIndex;
                group.StartIndex = group.Segments.Count;
                for (int i = previousStartIndex; i < group.StartIndex; i++)
                {
                    start = group.Segments[i].Start;
                    end = group.Segments[i].End;

                    // determine a new direction for the split
                    Vector3 midPoint = (start + end) * 0.5f;

                    // adjust the mid point to be the new location
                    RandomVector(bolt, ref start, ref end, offsetAmount, parameters.Random, out randomVector);
                    midPoint += randomVector;

                    // add two new segments
                    group.Segments.Add(new LightningBoltSegment { Start = start, End = midPoint });
                    group.Segments.Add(new LightningBoltSegment { Start = midPoint, End = end });

                    CreateFork(bolt, parameters, generation, totalGenerations, start, midPoint);
                }

                // halve the distance the lightning can deviate for each generation down
                offsetAmount *= 0.5f;
            }
        }

        /// <summary>
        /// Get a random 3D direction
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random unit Vector3</returns>
        public Vector3 RandomDirection3D(System.Random r)
        {
            float z = (2.0f * (float)r.NextDouble()) - 1.0f; // z is in the range [-1,1]
            Vector3 planar = RandomDirection2D(r) * Mathf.Sqrt(1.0f - (z * z));
            planar.z = z;

            return planar;
        }

        /// <summary>
        /// Get a random 2D direction on XY plane
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random unit Vector3</returns>
        public Vector3 RandomDirection2D(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), Mathf.Sin(azimuth), 0.0f);
        }

        /// <summary>
        /// Get a random 2D direction on XZ plane
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random unit Vector3</returns>
        public Vector3 RandomDirection2DXZ(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), 0.0f, Mathf.Sin(azimuth));
        }

        /// <summary>
        /// Get a random vector for a lightning bolt
        /// </summary>
        /// <param name="bolt">Lightning bolt</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="offsetAmount">Offset amount</param>
        /// <param name="random">Random</param>
        /// <param name="result">Result Vector3</param>
        public void RandomVector(LightningBolt bolt, ref Vector3 start, ref Vector3 end, float offsetAmount, System.Random random, out Vector3 result)
        {
            if (bolt.CameraMode == CameraMode.Perspective)
            {
                Vector3 direction = (end - start).normalized;
                Vector3 side = Vector3.Cross(start, end);
                if (side == Vector3.zero)
                {
                    // slow path, rarely hit unless cross product is zero
                    GetPerpendicularVector(ref direction, out side);
                }
                else
                {
                    side.Normalize();
                }

                // generate random distance and angle
                float distance = (((float)random.NextDouble() + 0.1f) * offsetAmount);

#if DEBUG

                float rotationAngle = ((float)random.NextDouble() * 360.0f);
                result = Quaternion.AngleAxis(rotationAngle, direction) * side * distance;

#else

                // optimized path for RELEASE mode, skips two normalize and two multiplies in Quaternion.AngleAxis
                float rotationAngle = ((float)random.NextDouble() * Mathf.PI);
                direction *= (float)System.Math.Sin(rotationAngle);
                Quaternion rotation;
                rotation.x = direction.x;
                rotation.y = direction.y;
                rotation.z = direction.z;
                rotation.w =  (float)System.Math.Cos(rotationAngle);
                result = rotation * side * distance;

#endif

            }
            else if (bolt.CameraMode == CameraMode.OrthographicXY)
            {
                // XY plane
                end.z = start.z;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.y, directionNormalized.x, 0.0f);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
            else
            {
                // XZ plane
                end.y = start.y;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.z, 0.0f, directionNormalized.x);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
        }

        /// <summary>
        /// Generate a lightning bolt
        /// </summary>
        /// <param name="bolt">Lightning bolt to fill in</param>
        /// <param name="parameters">Parameters</param>
        public void GenerateLightningBolt(LightningBolt bolt, LightningBoltParameters parameters)
        {
            Vector3 start, end;
            GenerateLightningBolt(bolt, parameters, out start, out end);
        }

        /// <summary>
        /// Generate a lightning bolt
        /// </summary>
        /// <param name="bolt">Lightning bolt to fill in</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        public void GenerateLightningBolt(LightningBolt bolt, LightningBoltParameters parameters, out Vector3 start, out Vector3 end)
        {
            start = parameters.ApplyVariance(parameters.Start, parameters.StartVariance);
            end = parameters.ApplyVariance(parameters.End, parameters.EndVariance);
            OnGenerateLightningBolt(bolt, start, end, parameters);
        }

        /// <summary>
        /// Lightning generator singleton
        /// </summary>
        public static readonly LightningGenerator GeneratorInstance = new LightningGenerator();
    }

    /// <summary>
    /// Generates lightning that follows a path
    /// </summary>
    public class LightningGeneratorPath : LightningGenerator
    {
        /// <summary>
        /// Lightning path generator singelton
        /// </summary>
        public static readonly LightningGeneratorPath PathGeneratorInstance = new LightningGeneratorPath();

        /// <summary>
        /// Generate path lightning
        /// </summary>
        /// <param name="bolt">Lightning bolt to fill in</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="parameters">Parameters</param>
        public void GenerateLightningBoltPath(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters parameters)
        {
            if (parameters.Points.Count < 2)
            {
                Debug.LogError("Lightning path should have at least two points");
                return;
            }

            int generation = parameters.Generations;
            int totalGenerations = generation;
            float offsetAmount, d;
            int smoothingFactor = parameters.SmoothingFactor - 1;
            Vector3 distance, randomVector;
            LightningBoltSegmentGroup group = bolt.AddGroup();
            group.LineWidth = parameters.TrunkWidth;
            group.Generation = generation--;
            group.EndWidthMultiplier = parameters.EndWidthMultiplier;
            group.Color = Color.white;

            parameters.Start = parameters.Points[0] + start;
            parameters.End = parameters.Points[parameters.Points.Count - 1] + end;
            end = parameters.Start;

            for (int i = 1; i < parameters.Points.Count; i++)
            {
                start = end;
                end = parameters.Points[i];
                distance = (end - start);
                d = Mathf.Sqrt(distance.sqrMagnitude);
                if (parameters.ChaosFactor > 0.0f)
                {
                    if (bolt.CameraMode == CameraMode.Perspective)
                    {
                        end += (d * parameters.ChaosFactor * RandomDirection3D(parameters.Random));
                    }
                    else if (bolt.CameraMode == CameraMode.OrthographicXY)
                    {
                        end += (d * parameters.ChaosFactor * RandomDirection2D(parameters.Random));
                    }
                    else
                    {
                        end += (d * parameters.ChaosFactor * RandomDirection2DXZ(parameters.Random));
                    }
                    distance = (end - start);
                }
                group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

                offsetAmount = d * parameters.ChaosFactor;
                RandomVector(bolt, ref start, ref end, offsetAmount, parameters.Random, out randomVector);

                if (ShouldCreateFork(parameters, generation, totalGenerations))
                {
                    Vector3 branchVector = distance * parameters.ForkMultiplier() * smoothingFactor * 0.5f;
                    Vector3 forkEnd = end + branchVector + randomVector;
                    GenerateLightningBoltStandard(bolt, start, forkEnd, generation, totalGenerations, 0.0f, parameters);
                }

                if (--smoothingFactor == 0)
                {
                    smoothingFactor = parameters.SmoothingFactor - 1;
                }
            }
        }

        /// <summary>
        /// Callback for lightning bolt generation
        /// </summary>
        /// <param name="bolt">Lightning bolt to fill in</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="parameters">Parameters</param>
        protected override void OnGenerateLightningBolt(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters parameters)
        {
            GenerateLightningBoltPath(bolt, start, end, parameters);
        }
    }
}
