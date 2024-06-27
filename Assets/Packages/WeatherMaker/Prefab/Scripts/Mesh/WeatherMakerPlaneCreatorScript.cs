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
    /// Allows creating plane object
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class WeatherMakerPlaneCreatorScript : MonoBehaviour
    {
        /// <summary>
        /// MeshRenderer
        /// </summary>
        public MeshRenderer MeshRenderer { get; private set; }

        /// <summary>
        /// MeshFilter
        /// </summary>
        public MeshFilter MeshFilter { get; private set; }

#if UNITY_EDITOR

        // / <summary>Whether to auto-create the plane</summary>
        [Header("Plane generation")]
        [Tooltip("Whether to auto-create the plane")]
        public bool AutoCreateMesh;

        /// <summary>The number of rows in the plane. The higher the number, the higher number of vertices.</summary>
        [Tooltip("The number of rows in the plane. The higher the number, the higher number of vertices.")]
        [Range(4, 124)]
        public int PlaneRows = 96;

        [SerializeField]
        [HideInInspector]
        private int lastPlaneRows = -1;

        /// <summary>The number of columns in the plane. The higher the number, the higher number of vertices.</summary>
        [Tooltip("The number of columns in the plane. The higher the number, the higher number of vertices.")]
        [Range(4, 124)]
        public int PlaneColumns = 32;

        [SerializeField]
        [HideInInspector]
        private int lastPlaneColumns = -1;

        /// <summary>Plane scale, increases space between vertices.</summary>
        [Tooltip("Plane scale, increases space between vertices.")]
        [Range(0.01f, 10000.0f)]
        public float PlaneScale = 1.0f;

        [SerializeField]
        [HideInInspector]
        private float lastPlaneScale = -1;

        /// <summary>True if plane forward is z axis (xy plane) false otherwise (xz plane).</summary>
        [Tooltip("True if plane forward is z axis (xy plane) false otherwise (xz plane).")]
        public bool PlaneForwardIsZAxis = true;

        [SerializeField]
        [HideInInspector]
        private bool lastPlaneForwardIsZAxis = true;

        /// <summary>Set to greater than 0 to make a cube. The cube will be this many units of depth from the plane.</summary>
        [Tooltip("Set to greater than 0 to make a cube. The cube will be this many units of depth from the plane.")]
        [Range(0, 4096)]
        public int CubeDepth;

        [SerializeField]
        [HideInInspector]
        private int lastCubeDepth;

        /// <summary>Whether to auto create an asset in the WeatherMaker/Prefab/Mesh folder</summary>
        [Tooltip("Whether to auto create an asset in the WeatherMaker/Prefab/Mesh folder")]
        public bool AutoCreateAsset;

        private void DestroyMesh()
        {
            if (MeshFilter.sharedMesh != null)
            {
                GameObject.DestroyImmediate(MeshFilter.sharedMesh, false);
                MeshFilter.sharedMesh = null;
            }
        }

        private Mesh CreatePlaneMesh()
        {
            DestroyMesh();
            Mesh m = new Mesh { name = "WeatherMakerPlane" };
            int vertIndex = 0;
            float stepX = PlaneScale / (float)PlaneColumns;
            float stepY = PlaneScale / (float)PlaneRows;
            float stepXUV = 1.0f / (float)PlaneColumns;
            float stepYUV = 1.0f / (float)PlaneRows;
            float start = PlaneScale * -0.5f;
            float end = PlaneScale * 0.5f;
            float colPos = 0.0f;
            float rowPos = 0.0f;
            float uvPosX = 0.0f;
            float uvPosY = 0.0f;

            List<Vector3> vertices = new List<Vector3>(PlaneColumns * PlaneRows);
            List<Vector2> uvs = new List<Vector2>(PlaneColumns * PlaneRows);
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            Vector3 defaultNormal;
            Vector3 forwardEdgeNormal = new Vector3(0.1f, 0.1f, 0.9f).normalized;
            Vector3 upEdgeNormal = new Vector3(0.1f, 0.9f, 0.1f).normalized;

            for (colPos = start; colPos < end; colPos += stepX, uvPosX += stepXUV)
            {
                uvPosY = 0.0f;
                for (rowPos = start; rowPos < end; rowPos += stepY, uvPosY += stepYUV)
                {
                    if (PlaneForwardIsZAxis)
                    {
                        float z = (CubeDepth > 0 ? -0.00001f : 0.0f); // just below 0 prevents far plane artifact
                        defaultNormal = Vector3.forward;
                        vertices.Add(new Vector3(colPos + stepX, rowPos, z));
                        normals.Add((colPos + stepX >= end || rowPos == start ? forwardEdgeNormal : defaultNormal));
                        vertices.Add(new Vector3(colPos + stepX, rowPos + stepY, z));
                        normals.Add((colPos + stepX >= end || rowPos + stepY >= end ? forwardEdgeNormal : defaultNormal));
                        vertices.Add(new Vector3(colPos, rowPos + stepY, z));
                        normals.Add((rowPos + stepY >= end || colPos == start ? forwardEdgeNormal : defaultNormal));
                        vertices.Add(new Vector3(colPos, rowPos, z));
                        normals.Add(colPos == start || rowPos == start ? forwardEdgeNormal : defaultNormal);
                        uvs.Add(new Vector2(uvPosX + stepXUV, uvPosY));
                        uvs.Add(new Vector2(uvPosX + stepXUV, uvPosY + stepYUV));
                        uvs.Add(new Vector2(uvPosX, uvPosY + stepYUV));
                        uvs.Add(new Vector2(uvPosX, uvPosY));
                    }
                    else
                    {
                        float y = (CubeDepth > 0 ? -0.00001f : 0.0f); // just below 0 prevents far plane artifact
                        defaultNormal = Vector3.up;
                        vertices.Add(new Vector3(colPos, y, rowPos));
                        normals.Add(colPos == start || rowPos == start ? upEdgeNormal : defaultNormal);
                        vertices.Add(new Vector3(colPos, y, rowPos + stepY));
                        normals.Add((rowPos + stepY >= end || colPos == start ? upEdgeNormal : defaultNormal));
                        vertices.Add(new Vector3(colPos + stepX, y, rowPos + stepY));
                        normals.Add((colPos + stepX >= end || rowPos + stepY >= end ? upEdgeNormal : defaultNormal));
                        vertices.Add(new Vector3(colPos + stepX, y, rowPos));
                        normals.Add((colPos + stepX >= end || rowPos == start ? upEdgeNormal : defaultNormal));
                        uvs.Add(new Vector2(uvPosX, uvPosY));
                        uvs.Add(new Vector2(uvPosX, uvPosY + stepYUV));
                        uvs.Add(new Vector2(uvPosX + stepXUV, uvPosY + stepYUV));
                        uvs.Add(new Vector2(uvPosX + stepXUV, uvPosY));
                    }
                    triangles.Add(vertIndex++);
                    triangles.Add(vertIndex++);
                    triangles.Add(vertIndex);
                    triangles.Add(vertIndex -= 2);
                    triangles.Add(vertIndex += 2);
                    triangles.Add(++vertIndex);
                    vertIndex++; // get ready for next position
                }
            }

            Bounds meshBounds;
            if (CubeDepth <= 0)
            {
                if (PlaneForwardIsZAxis)
                {
                    meshBounds = new Bounds(Vector3.zero, new Vector3(PlaneScale, PlaneScale, 1.0f));
                }
                else
                {
                    meshBounds = new Bounds(Vector3.zero, new Vector3(PlaneScale, 1.0f, PlaneScale));
                }
            }
            else if (PlaneForwardIsZAxis)
            {
                float zStart = 0.0f;
                float zEnd = -CubeDepth;
                float xEnd = colPos;
                float yEnd = rowPos;

                // left
                AddCubeFace(new Vector3(start, start, zStart), new Vector3(start, yEnd, zStart), new Vector3(start, yEnd, zEnd), new Vector3(start, start, zEnd),
                    Vector3.left, vertices, uvs, normals, triangles, ref vertIndex);

                // right
                AddCubeFace(new Vector3(xEnd, start, zStart), new Vector3(xEnd, yEnd, zStart), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, start, zEnd),
                    Vector3.right, vertices, uvs, normals, triangles, ref vertIndex, true);

                // bottom
                AddCubeFace(new Vector3(start, start, zStart), new Vector3(start, start, zEnd), new Vector3(xEnd, start, zEnd), new Vector3(xEnd, start, zStart),
                    Vector3.down, vertices, uvs, normals, triangles, ref vertIndex);

                // top
                AddCubeFace(new Vector3(start, yEnd, zStart), new Vector3(start, yEnd, zEnd), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, yEnd, zStart),
                    Vector3.up, vertices, uvs, normals, triangles, ref vertIndex, true);

                // back
                AddCubeFace(new Vector3(start, start, zEnd), new Vector3(start, yEnd, zEnd), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, start, zEnd),
                    Vector3.back, vertices, uvs, normals, triangles, ref vertIndex);

                meshBounds = new Bounds(new Vector3(0.0f, 0.0f, -0.5f * PlaneScale), PlaneScale * Vector3.one);
            }
            else
            {
                float yStart = 0.0f;
                float yEnd = -CubeDepth;
                float xEnd = colPos;
                float zEnd = rowPos;

                // left
                AddCubeFace(new Vector3(start, yStart, start), new Vector3(start, yEnd, start), new Vector3(start, yEnd, zEnd), new Vector3(start, yStart, zEnd),
                    Vector3.left, vertices, uvs, normals, triangles, ref vertIndex);

                // right
                AddCubeFace(new Vector3(xEnd, yStart, start), new Vector3(xEnd, yEnd, start), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, yStart, zEnd),
                    Vector3.right, vertices, uvs, normals, triangles, ref vertIndex, true);

                // front
                AddCubeFace(new Vector3(start, yStart, start), new Vector3(start, yEnd, start), new Vector3(xEnd, yEnd, start), new Vector3(xEnd, yStart, start),
                    Vector3.forward, vertices, uvs, normals, triangles, ref vertIndex, true);

                // back
                AddCubeFace(new Vector3(start, yStart, zEnd), new Vector3(start, yEnd, zEnd), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, yStart, zEnd),
                    Vector3.back, vertices, uvs, normals, triangles, ref vertIndex);

                // bottom
                AddCubeFace(new Vector3(start, yEnd, start), new Vector3(start, yEnd, zEnd), new Vector3(xEnd, yEnd, zEnd), new Vector3(xEnd, yEnd, start),
                    Vector3.down, vertices, uvs, normals, triangles, ref vertIndex, true);

                meshBounds = new Bounds(new Vector3(0.0f, -0.5f * PlaneScale, 0.0f), PlaneScale * Vector3.one);
            }
            m.SetVertices(vertices);
            m.SetUVs(0, uvs);
            m.SetTriangles(triangles, 0);
            m.SetNormals(normals);
            m.bounds = meshBounds;
            return m;
        }

        private void AddCubeFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, ref int vertIndex, bool reverseCull = false)
        {
            if (reverseCull)
            {
                vertices.Add(v4);
                vertices.Add(v3);
                vertices.Add(v2);
                vertices.Add(v1);
            }
            else
            {
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);
                vertices.Add(v4);
            }

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(1.0f, 1.0f));
            uvs.Add(new Vector2(1.0f, 0.0f));

            triangles.Add(vertIndex++);
            triangles.Add(vertIndex++);
            triangles.Add(vertIndex);
            triangles.Add(vertIndex -= 2);
            triangles.Add(vertIndex += 2);
            triangles.Add(++vertIndex);
            vertIndex++;
        }

#endif

        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshRenderer.sortingOrder = int.MinValue;
            MeshFilter = GetComponent<MeshFilter>();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected virtual void Update()
        {

#if UNITY_EDITOR

            if (MeshFilter.sharedMesh == null ||
                lastPlaneColumns != PlaneColumns ||
                lastPlaneRows != PlaneRows ||
                lastPlaneScale != PlaneScale ||
                lastPlaneForwardIsZAxis != PlaneForwardIsZAxis ||
                lastCubeDepth != CubeDepth)
            {
                lastPlaneColumns = PlaneColumns;
                lastPlaneRows = PlaneRows;
                lastPlaneScale = PlaneScale;
                lastPlaneForwardIsZAxis = PlaneForwardIsZAxis;
                lastCubeDepth = CubeDepth;
                MeshFilter.sharedMesh = CreatePlaneMesh();
            }

            if (AutoCreateAsset)
            {
                string path = "Assets/WeatherMaker/Prefab/Mesh/Plane_" + PlaneColumns.ToString() +
                    "_" + PlaneRows.ToString() + "_" + PlaneScale + "_" + (PlaneForwardIsZAxis ? "ForwardZ" : "ForwardY") + "_" + CubeDepth + ".asset";
                if (UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) == null)
                {
                    Mesh savedMesh = Object.Instantiate(MeshFilter.sharedMesh) as Mesh;
                    UnityEditor.AssetDatabase.CreateAsset(savedMesh, path);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }

#endif

        }
    }
}
