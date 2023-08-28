using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     Contains methods for generating primitive shape meshes.
    /// </summary>
    internal static class Primitives
    {
        [NotNull]
        public static Mesh CreateFullscreenQuad()
        {
            var mesh = new Mesh {
                vertices = new[] {
                    new Vector3(-1, -1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0)
                },
                uv = new[] {
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1)
                }
            };

            mesh.SetIndices(new[] { 0, 1, 2, 2, 3, 0 }, MeshTopology.Triangles, 0);

            return mesh;
        }

        [NotNull]
        public static Mesh CreateWireframeBox(float width, float height, float depth)
        {
            var mesh = new Mesh();
            mesh.Clear();

            var p0 = new Vector3(-width * .5f, -height * .5f, depth * .5f);
            var p1 = new Vector3(width * .5f, -height * .5f, depth * .5f);
            var p2 = new Vector3(width * .5f, -height * .5f, -depth * .5f);
            var p3 = new Vector3(-width * .5f, -height * .5f, -depth * .5f);

            var p4 = new Vector3(-width * .5f, height * .5f, depth * .5f);
            var p5 = new Vector3(width * .5f, height * .5f, depth * .5f);
            var p6 = new Vector3(width * .5f, height * .5f, -depth * .5f);
            var p7 = new Vector3(-width * .5f, height * .5f, -depth * .5f);

            Vector3[] vertices = {
                // bottom
                p0, p1, p2, p3,

                // top
                p4, p5, p6, p7
            };

            int[] indices = {
                // bottom
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // top
                4, 5,
                5, 6,
                6, 7,
                7, 4,

                // sides
                0, 4,
                1, 5,
                2, 6,
                3, 7
            };

            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            mesh.RecalculateBounds();

            return mesh;
        }

        [NotNull]
        public static Mesh CreateBox(float width, float height, float depth)
        {
            // https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Box

            var mesh = new Mesh();
            mesh.Clear();

            #region Vertices

            var p0 = new Vector3(-width * .5f, -height * .5f, depth * .5f);
            var p1 = new Vector3(width * .5f, -height * .5f, depth * .5f);
            var p2 = new Vector3(width * .5f, -height * .5f, -depth * .5f);
            var p3 = new Vector3(-width * .5f, -height * .5f, -depth * .5f);

            var p4 = new Vector3(-width * .5f, height * .5f, depth * .5f);
            var p5 = new Vector3(width * .5f, height * .5f, depth * .5f);
            var p6 = new Vector3(width * .5f, height * .5f, -depth * .5f);
            var p7 = new Vector3(-width * .5f, height * .5f, -depth * .5f);

            Vector3[] vertices = {
                // bottom
                p0, p1, p2, p3,

                // left
                p7, p4, p0, p3,

                // front
                p4, p5, p1, p0,

                // back
                p6, p7, p3, p2,

                // right
                p5, p6, p2, p1,

                // top
                p7, p6, p5, p4
            };

            #endregion

            #region Normales

            var up = Vector3.up;
            var down = Vector3.down;
            var front = Vector3.forward;
            var back = Vector3.back;
            var left = Vector3.left;
            var right = Vector3.right;

            Vector3[] normales = {
                // bottom
                down, down, down, down,

                // left
                left, left, left, left,

                // front
                front, front, front, front,

                // back
                back, back, back, back,

                // right
                right, right, right, right,

                // top
                up, up, up, up
            };

            #endregion

            #region UVs

            var uv00 = new Vector2(0f, 0f);
            var uv10 = new Vector2(1f, 0f);
            var uv01 = new Vector2(0f, 1f);
            var uv11 = new Vector2(1f, 1f);

            Vector2[] uvs = {
                // bottom
                uv11, uv01, uv00, uv10,

                // left
                uv11, uv01, uv00, uv10,

                // front
                uv11, uv01, uv00, uv10,

                // back
                uv11, uv01, uv00, uv10,

                // right
                uv11, uv01, uv00, uv10,

                // top
                uv11, uv01, uv00, uv10
            };

            #endregion

            #region Triangles

            int[] triangles = {
                // bottom
                3, 1, 0,
                3, 2, 1,

                // left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

                // front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

                // back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

                // right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

                // top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5
            };

            #endregion

            #region Outline

            int[] outline = {
                // bottom
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // top
                0 + 20, 1 + 20,
                1 + 20, 2 + 20,
                2 + 20, 3 + 20,
                3 + 20, 0 + 20,

                // sides
                0, 3 + 20,
                1, 2 + 20,
                2, 1 + 20,
                3, 0 + 20
            };

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;

            mesh.subMeshCount = 2;
            mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
            mesh.SetIndices(outline, MeshTopology.Lines, 1);

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}