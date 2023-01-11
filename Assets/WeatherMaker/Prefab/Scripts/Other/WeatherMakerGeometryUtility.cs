using System;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Geometry helper methods
    /// </summary>
    public static class WeatherMakerGeometryUtility
    {
        /// <summary>
        /// Exact frustum box intersect, unlike Unity frustum culling this does not sometimes fail on objects that are not in the frustum
        /// All credit goes to https://www.iquilezles.org/www/articles/frustumcorrect/frustumcorrect.htm, very smart person :)
        /// Also http://www.flipcode.com/archives/Frustum_Culling.shtml was helpful
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="frustumPlanes">Frustum planes</param>
        /// <param name="frustumCorners">Frustum corners</param>
        /// <param name="box">AABB box</param>
        /// <returns>True if intersect or contained in frustum, false otherwise. Returns true if camera is null.</returns>
        public static bool BoxIntersectsFrustum(Camera camera, Plane[] frustumPlanes, Vector3[] frustumCorners, Bounds box)
        {
            if (camera == null || frustumPlanes == null || frustumCorners == null)
            {
                return false;
            }

            /*
            float fViewLen = camera.farClipPlane - camera.nearClipPlane;

            // use some trig to find the height of the frustum at the far plane
            float fHeight = fViewLen * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);

            // with an aspect ratio of 1, the width will be the same
            float fWidth = fHeight * camera.aspect;

            // halfway point between near/far planes starting at the origin and extending along the z axis
            Vector3 P = new Vector3(0.0f, 0.0f, camera.nearClipPlane + fViewLen * 0.5f);

            // the calculate far corner of the frustum
            Vector3 Q = new Vector3(fWidth, fHeight, fViewLen);

            // the vector between P and Q
            Vector3 vDiff = P - Q;

            // the radius becomes the length of this vector
            float sphereRadiusSquared = vDiff.sqrMagnitude;

            // get the look vector of the camera from the view matrix
            Vector3 forward = camera.transform.forward;

            // calculate the center of the sphere
            Vector3 sphereCenter = camera.transform.position + (forward * ((fViewLen * 0.5f) + camera.nearClipPlane));
            */

            // near plane is tiny usually, no need to include it in calculations
            Vector3 sphereCenter = camera.transform.position + (camera.transform.forward * camera.farClipPlane * 0.5f);
            float sphereRadiusSquared = Vector3.SqrMagnitude(frustumCorners[0] - camera.transform.position);
            Vector3 closestPointInAabb = Vector3.Min(Vector3.Max(sphereCenter, box.min), box.max);
            double distanceSquared = (closestPointInAabb - sphereCenter).sqrMagnitude;
            bool inSphere = (distanceSquared < sphereRadiusSquared);
            if (!inSphere)
            {
                return false;
            }

            /*
            // check box outside/inside of frustum - seems bugged for some bounds..., maybe Unity planes are different than the planes from this code
            for( int i=0; i<6; i++ )
            {
                int out = 0;
                out += ((dot( fru.mPlane[i], vec4(box.mMinX, box.mMinY, box.mMinZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMaxX, box.mMinY, box.mMinZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMinX, box.mMaxY, box.mMinZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMaxX, box.mMaxY, box.mMinZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMinX, box.mMinY, box.mMaxZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMaxX, box.mMinY, box.mMaxZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMinX, box.mMaxY, box.mMaxZ, 1.0f) ) < 0.0 )?1:0);
                out += ((dot( fru.mPlane[i], vec4(box.mMaxX, box.mMaxY, box.mMaxZ, 1.0f) ) < 0.0 )?1:0);
                if( out==8 ) return false;
            }
            */

            bool insideFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, box);
            if (!insideFrustum)
            {
                return false;
            }

            // check frustum outside/inside box 
            int o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].x > box.max.x) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].x < box.min.x) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].y > box.max.y) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].y < box.min.y) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].z > box.max.z) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            o = 0;
            for (int i = 0; i < 8; i++)
            {
                o += ((frustumCorners[i].z < box.min.z) ? 1 : 0);
                if (o == 8)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Smoothstep just like a shader
        /// </summary>
        /// <param name="edge0">Value 1</param>
        /// <param name="edge1">Value 2</param>
        /// <param name="x">Amount (0 - 1)</param>
        /// <returns>Result</returns>
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }
    }
}
