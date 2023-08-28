using System;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     Renders all decals active in the scene into a Camera.
    ///     This component must be added to all cameras for which you want the effect to be visible.
    /// </summary>
    /// <inheritdoc />
    [ImageEffectAllowedInSceneView, DisallowMultipleComponent, ExecuteInEditMode, RequireComponent(typeof(Camera)), HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/WetStuff/")]
    public partial class WetStuff
        : MonoBehaviour
    {
        private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(WetStuff).Name);

        [SerializeField, Range(0, 1)] private float _ambientModificationFactor = 0.35f;
        private bool _appliedEditorRestartHack;

        private Camera _camera;
        private CommandBuffer _cmd;
        private WetDecalRenderer _decalRenderer;
        private WetAttributeModifier _gbufferModifier;

        private bool _initialized;

        public event Action<CommandBuffer> AfterDecalRender;

        private void Startup()
        {
            if (_initialized) return;

            _initialized = true;

            _camera = GetComponent<Camera>();
            if (_camera.actualRenderingPath != RenderingPath.DeferredShading)
            {
                if (_camera.name == "SceneCamera")
                    Log.Warn("Scene Camera is in 2D mode or '{0}' mode, Wet Decals will not be rendered in scene view.", _camera.actualRenderingPath);
                else
                    Log.Error("Camera '{0}' rendering path is '{1}', 'DeferredShading' is required for Wet Decals to render.", _camera.name, _camera.actualRenderingPath);
            }

            _cmd = new CommandBuffer {
                name = "Wet Surface Decals"
            };

            _camera.AddCommandBuffer(CameraEvent.BeforeReflections, _cmd);
            if (_camera.commandBufferCount < 1)
                Log.Error("Failed to attach CommandBuffer");

            _decalRenderer = new WetDecalRenderer(_camera);
            _gbufferModifier = new WetAttributeModifier(_camera) {
                AmbientDarkenStrength = _ambientModificationFactor
            };

#if UNITY_EDITOR
            if (!_appliedEditorRestartHack)
            {
                _appliedEditorRestartHack = true;

                // Check if one has already been added, if so destroy it and create a new one
                var hack = gameObject.GetComponent<EditorRestartHack>();
                if (hack != null)
                    DestroyImmediate(hack);

                // Add a new one and apply it
                gameObject.AddComponent<EditorRestartHack>().Apply(this);
            }
#endif
        }

        private void Teardown()
        {
            if (!_initialized) return;

            _initialized = false;

            _decalRenderer.Dispose();
            _gbufferModifier.Dispose();

            _camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, _cmd);
            _cmd.Dispose();

            _cmd = null;
            _decalRenderer = null;
            _gbufferModifier = null;
        }

        protected virtual void OnEnable()
        {
            Startup();
        }

        protected virtual void OnDisable()
        {
            Teardown();
        }

        protected virtual void OnDestroy()
        {
            Teardown();
        }

        protected virtual void Update()
        {
            Logs.WriteMultithreadedLogs();
        }

        protected virtual void LateUpdate()
        {
            _decalRenderer.Update();
        }

        protected virtual void OnPreRender()
        {
            if (_initialized)
            {
                _cmd.Clear();
                _decalRenderer.RecordCommandBuffer(_cmd);
                OnAfterDecalRender();
                _gbufferModifier.RecordCommandBuffer(_cmd);
            }
        }

        protected virtual void OnValidate()
        {
            if (_initialized)
                _gbufferModifier.AmbientDarkenStrength = _ambientModificationFactor;
        }

        private void OnAfterDecalRender()
        {
            var handler = AfterDecalRender;
            if (handler != null)
                handler(_cmd);
        }
    }
}