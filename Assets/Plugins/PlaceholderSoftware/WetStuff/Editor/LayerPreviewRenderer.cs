using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    internal class LayerPreviewRenderer
    {
        private static readonly int[] RangeEdgeIndices = {
            0, 3,
            1, 2,
            4, 7,
            5, 6
        };

        private static readonly int[] RangeFillIndices = {
            // front
            0, 1, 2,
            2, 3, 0,

            // back
            4, 7, 6,
            6, 5, 4,

            // top
            1, 5, 6,
            6, 2, 1,

            // bottom
            0, 4, 7,
            7, 3, 0
        };

        private readonly Matrix4x4 _inputTransform;
        private readonly Matrix4x4 _outputTransform;
        private Mesh _box;

        private Material _boxOutlineMaterial;
        private bool _firstTimeSetupDone;

        private Mesh _heightmap;
        private Material _highlightFillMaterial;
        private Material _highlightOutlineMaterial;
        private Material _inputHeightmapMaterial;
        private Matrix4x4 _inputHighlightTransform;
        private Material _outputHeightmapMaterial;
        private Matrix4x4 _outputHighlightTransform;

        private Mesh _range;

        public LayerPreviewRenderer()
        {
            _inputTransform = Matrix4x4.TRS(new Vector3(-0.75f, 0, 0), Quaternion.identity, new Vector3(1, 0.5f, 1));
            _outputTransform = Matrix4x4.TRS(new Vector3(0.75f, 0, 0), Quaternion.identity, new Vector3(1, 0.5f, 1));
        }

        private void FirstTimeSetup()
        {
            if (_firstTimeSetupDone)
                return;

            _boxOutlineMaterial = new Material(Shader.Find("Hidden/PremultipliedColor")) {
                hideFlags = HideFlags.DontSave
            };
            _highlightFillMaterial = new Material(Shader.Find("Hidden/PremultipliedColor")) {
                hideFlags = HideFlags.DontSave
            };
            _highlightOutlineMaterial = new Material(Shader.Find("Hidden/PremultipliedColor")) {
                hideFlags = HideFlags.DontSave
            };
            _inputHeightmapMaterial = new Material(Shader.Find("Hidden/LayerInputHeightmap")) {
                hideFlags = HideFlags.DontSave
            };
            _outputHeightmapMaterial = new Material(Shader.Find("Hidden/LayerOutputHeightmap")) {
                hideFlags = HideFlags.DontSave
            };

            _heightmap = CreateHeightmap();
            _box = Primitives.CreateBox(1, 1, 1);
            _box.hideFlags = HideFlags.DontSave;

            _range = new Mesh {
                subMeshCount = 2,
                vertices = new Vector3[8],
                hideFlags = HideFlags.DontSave
            };
            _range.SetIndices(RangeEdgeIndices, MeshTopology.Lines, 0);
            _range.SetIndices(RangeFillIndices, MeshTopology.Triangles, 1);

            _firstTimeSetupDone = true;
        }

        public void Configure(Texture2D map, Vector4 channels, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            FirstTimeSetup();

            _inputHighlightTransform = Matrix4x4.Translate(new Vector3(0, inputMin - (1 - (inputMax - inputMin)) * 0.5f, 0)) * Matrix4x4.Scale(new Vector3(1, inputMax - inputMin, 1));
            _outputHighlightTransform = Matrix4x4.Translate(new Vector3(0, outputMin - (1 - (outputMax - outputMin)) * 0.5f, 0)) * Matrix4x4.Scale(new Vector3(1, outputMax - outputMin, 1));

            CreateScaleConnector(_range, _inputTransform, _inputHighlightTransform, _outputTransform, _outputHighlightTransform);

            var color = (Color) channels;
            color.a = 1;
            _boxOutlineMaterial.color = new Color(0.3f, 0.3f, 0.4f);

            _highlightFillMaterial.color = color * 0.1f;
            _highlightOutlineMaterial.color = color * 0.7f;

            _inputHeightmapMaterial.SetTexture("_MainTex", map);
            _inputHeightmapMaterial.SetVector("_Channels", channels);
            _inputHeightmapMaterial.SetFloat("_Min", inputMin);
            _inputHeightmapMaterial.SetFloat("_Max", inputMax);

            _outputHeightmapMaterial.SetTexture("_MainTex", map);
            _outputHeightmapMaterial.SetVector("_Channels", channels);
            _outputHeightmapMaterial.SetFloat("_LayerInputStart", inputMin);
            _outputHeightmapMaterial.SetFloat("_LayerInputExtent", inputMax - inputMin);
            _outputHeightmapMaterial.SetFloat("_LayerOutputStart", outputMin);
            _outputHeightmapMaterial.SetFloat("_LayerOutputEnd", outputMax);
        }

        [NotNull]
        private static Mesh CreateHeightmap()
        {
            const int resolution = 128;

            var vertices = new Vector3[resolution * resolution];

            for (var x = 0; x < resolution; x++)
            for (var y = 0; y < resolution; y++)
            {
                var index = y * resolution + x;
                var uv = new Vector2((float) x / (resolution - 1), (float) y / (resolution - 1));

                vertices[index] = new Vector3(uv.x - 0.5f, 0, uv.y - 0.5f);
            }

            var mesh = new Mesh {
                vertices = vertices,
                hideFlags = HideFlags.DontSave
            };

            mesh.SetIndices(Enumerable.Range(0, vertices.Length).ToArray(), MeshTopology.Points, 0);

            return mesh;
        }

        private static void CreateScaleConnector([NotNull] Mesh mesh, Matrix4x4 inputTransform, Matrix4x4 inputHighlightTransform, Matrix4x4 outputTransform, Matrix4x4 outputHighlightTransform)
        {
            var inputTrans = inputTransform * inputHighlightTransform;
            var outputTrans = outputTransform * outputHighlightTransform;

            var inStart = inputTrans.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, -0.5f));
            var inEnd = inputTrans.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, -0.5f));
            var inStartFront = inputTrans.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, 0.5f));
            var inEndFront = inputTrans.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, 0.5f));
            var outStart = outputTrans.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, -0.5f));
            var outEnd = outputTrans.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, -0.5f));
            var outStartFront = outputTrans.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, 0.5f));
            var outEndFront = outputTrans.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, 0.5f));

            mesh.vertices = new[] {
                inStartFront, // 0
                inEndFront, // 1
                outEndFront, // 2
                outStartFront, // 3
                inStart, // 4
                inEnd, // 5
                outEnd, // 6
                outStart // 7
            };
        }

        public void Render([NotNull] PreviewRenderUtility preview)
        {
            // Input
            preview.DrawMesh(_heightmap, _inputTransform, _inputHeightmapMaterial, 0);
            preview.DrawMesh(_box, _inputTransform, _boxOutlineMaterial, 1);
            preview.DrawMesh(_box, _inputTransform * _inputHighlightTransform, _highlightFillMaterial, 0);
            preview.DrawMesh(_box, _inputTransform * _inputHighlightTransform, _highlightOutlineMaterial, 1);

            // Output
            preview.DrawMesh(_heightmap, _outputTransform, _outputHeightmapMaterial, 0);
            preview.DrawMesh(_box, _outputTransform, _boxOutlineMaterial, 1);
            preview.DrawMesh(_box, _outputTransform * _outputHighlightTransform, _highlightFillMaterial, 0);
            preview.DrawMesh(_box, _outputTransform * _outputHighlightTransform, _highlightOutlineMaterial, 1);

            // Connector
            preview.DrawMesh(_range, Matrix4x4.identity, _highlightFillMaterial, 1);
            preview.DrawMesh(_range, Matrix4x4.identity, _highlightOutlineMaterial, 0);
        }
    }
}