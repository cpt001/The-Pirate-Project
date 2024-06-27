#if UNITY_HDRP

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sets up HDRP render passes
    /// </summary>
    public class WeatherMakerHDRPCustomPass : CustomPass
    {
        // TODO: Create and add CustomPassVolume with global state as needed

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            // Setup code here
        }

        protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
        {
            // Executed every frame for all the camera inside the pass volume
        }

        protected override void Cleanup()
        {
            // Cleanup code
        }
    }
}

#endif
