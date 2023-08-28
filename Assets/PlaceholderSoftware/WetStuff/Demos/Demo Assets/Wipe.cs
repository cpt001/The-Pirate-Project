using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets
{
    [RequireComponent(typeof(PlaceholderSoftware.WetStuff.WetStuff))]
    public class Wipe
        : MonoBehaviour
    {
#if UNITY_STANDALONE_OSX
        private WetStuff _wetStuff;

        private void Awake()
        {
            _wetStuff = GetComponent<WetStuff>();
        }
        
        public void OnGUI()
        {
            var toggleButton = new Rect(20, 15, 130, 30);
            var toggled = GUI.Button(toggleButton, "Toggle Wet Stuff");

            if (toggled)
                _wetStuff.enabled = !_wetStuff.enabled;
        }
#else
        private Mesh _mesh;
        private Material _material;
        private PlaceholderSoftware.WetStuff.WetStuff _decals;
        private Camera _camera;

        [Range(0, 1)]
        public float Progress = 0f;

        private void Start ()
        {
            var shader = Shader.Find("Demo/ExcludeWetness");
            _material = new Material(shader);
            _mesh = CreateFullscreenQuad();
            _decals = GetComponent<PlaceholderSoftware.WetStuff.WetStuff>();
            _camera = GetComponent<Camera>();

            _decals.AfterDecalRender += RecordCommandBuffer;
        }

        private static Mesh CreateFullscreenQuad()
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

        private void OnDestroy()
        {
            if (_decals != null)
                _decals.AfterDecalRender -= RecordCommandBuffer;
        }

        private void RecordCommandBuffer(CommandBuffer cmd)
        {
            if (_camera.enabled && !_camera.stereoEnabled)
            {
                var position = Mathf.Lerp(-2, 2, Progress);

                cmd.SetRenderTarget(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget);
                cmd.DrawMesh(_mesh, Matrix4x4.Translate(new Vector3(position, 0, 0)), _material);
            }
        }

        public void OnGUI()
        {
            if (!_camera.enabled)
                return;

            if (XRSettings.enabled && _camera.stereoEnabled)
            {
                var buttonBox = new Rect(20, 20, 200, 30);
                if (GUI.Button(buttonBox, "Toggle Decals"))
                    _decals.enabled = !_decals.enabled;
            }
            else
            {
                //Find out the width of the screen
                var width = GUILayoutUtility.GetRect(float.MaxValue, 1).width;

                //Create a box for the slider
                var sliderBox = new Rect(170, 20, width - 190, 30);
                Progress = GUI.HorizontalSlider(sliderBox, Progress, 0, 0.5f);

                //Create a box for the label
                var labelBox = new Rect(20, 15, 160, 30);
                GUI.Label(labelBox, "Remove Wetness Effect:");
            }
        }
#endif
    }
}
