using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using RenderSettings = PlaceholderSoftware.WetStuff.Rendering.RenderSettings;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     Modifies the GBuffer textures to simulate wet surfaces.
    /// </summary>
    internal class WetAttributeModifier : IDisposable
    {
        private static readonly RenderTargetIdentifier[] RenderTargets = {
            BuiltinRenderTextureType.GBuffer0,
            BuiltinRenderTextureType.GBuffer1,
            BuiltinRenderTextureType.CameraTarget
        };

        private readonly Camera _camera;
        private readonly Material _gbufferMaterial;
//        private readonly Material _normalsMaterial;
        private readonly Material _normalSmoothingMaterial;
        private readonly Material _blit;

        private Mesh _fullScreenQuad;

        private bool _destroyed;

        public float AmbientDarkenStrength
        {
            get { return _gbufferMaterial.GetFloat("_AmbientDarken"); }
            set { _gbufferMaterial.SetFloat("_AmbientDarken", value); }
        }

        public WetAttributeModifier([NotNull] Camera camera)
        {
            if (!camera) throw new ArgumentNullException("camera");

            _gbufferMaterial = new Material(Shader.Find("Hidden/WetSurfaceModifier")) {
                hideFlags = HideFlags.DontSave
            };

//            _normalsMaterial = new Material(Shader.Find("Hidden/ReconstructNormals")) {
//                hideFlags = HideFlags.DontSave
//            };

            var shader = Shader.Find("Hidden/WS_BlurNormals");
            _normalSmoothingMaterial = new Material(shader) {
                hideFlags = HideFlags.DontSave
            };

            _blit = new Material(Shader.Find("Hidden/StereoBlit")) {
                hideFlags = HideFlags.DontSave
            };

            _camera = camera;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_gbufferMaterial);
            Object.DestroyImmediate(_blit);
//            Object.DestroyImmediate(_normalsMaterial);
            Object.DestroyImmediate(_normalSmoothingMaterial);
            Object.DestroyImmediate(_fullScreenQuad);

            _destroyed = true;
        }

        public void RecordCommandBuffer([NotNull] CommandBuffer cmd)
        {
            if (_destroyed)
                return;
            if (!_fullScreenQuad)
                _fullScreenQuad = Primitives.CreateFullscreenQuad();

            // Reconstruct geometry normals
            // cmd.SetRenderTarget(BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget);
            // cmd.DrawMesh(fsq, Matrix4x4.identity, _normalsMaterial, 0, 0);
            
            // Normal blur
            if (RenderSettings.Instance.EnableNormalSmoothing)
            {
                var tmpId = Shader.PropertyToID("_NormalsItermediateBlur");
                var sourceId = Shader.PropertyToID("_Source");

                if (XRSettings.enabled && _camera.stereoEnabled)
                {
                    var desc = XRSettings.eyeTextureDesc;
                    desc.colorFormat = RenderTextureFormat.ARGB2101010;
                    cmd.GetTemporaryRT(tmpId, desc, FilterMode.Bilinear);
                }
                else
                {
                    cmd.GetTemporaryRT(tmpId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB2101010);
                }

                cmd.SetRenderTarget(tmpId, BuiltinRenderTextureType.CameraTarget);
                cmd.SetGlobalTexture(sourceId, BuiltinRenderTextureType.GBuffer2);
                cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _normalSmoothingMaterial, 0, RenderSettings.Instance.EnableStencil ? 2 : 0);
                cmd.SetRenderTarget(BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget);
                cmd.SetGlobalTexture(sourceId, tmpId);
                cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _normalSmoothingMaterial, 0, RenderSettings.Instance.EnableStencil ? 3 : 1);
                cmd.ReleaseTemporaryRT(tmpId);
            }

            // Copy the specular texture, as we need to both read and write to it
            var specularId = CopyGBufferTexture(cmd, "_GBufferSpecularCopy", BuiltinRenderTextureType.GBuffer1, _camera.allowHDR);

            // Render our effect into the diffuse and specular gbuffer targets as a full screen pass
            cmd.SetRenderTarget(
                RenderTargets,
                BuiltinRenderTextureType.CameraTarget
            );

            cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _gbufferMaterial, 0, RenderSettings.Instance.EnableStencil ? 1 : 0);

            // Release our copy of the specular texture
            cmd.ReleaseTemporaryRT(specularId);
        }

        private int CopyGBufferTexture([NotNull] CommandBuffer cmd, [NotNull] string shaderParameterName, BuiltinRenderTextureType texture, bool hdr)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            var id = Shader.PropertyToID(shaderParameterName);

            if (XRSettings.enabled && _camera.stereoEnabled)
            {
                var desc = XRSettings.eyeTextureDesc;
                desc.colorFormat = GBufferFormat(texture, hdr);
                desc.depthBufferBits = 0;

                cmd.GetTemporaryRT(id, desc, FilterMode.Point);
            }
            else
            {
                cmd.GetTemporaryRT(id, -1, -1, 0, FilterMode.Point, GBufferFormat(texture, hdr));
            }

            cmd.Blit(texture, id, _blit);

            return id;
        }

        private static RenderTextureFormat GBufferFormat(BuiltinRenderTextureType gbuffer, bool hdr)
        {
            // https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html

            switch (gbuffer)
            {
                // RT0, ARGB32 format: Diffuse color (RGB), occlusion(A).
                case BuiltinRenderTextureType.GBuffer0:
                    return RenderTextureFormat.ARGB32;

                // RT1, ARGB32 format: Specular color (RGB), roughness (A).
                case BuiltinRenderTextureType.GBuffer1:
                    return RenderTextureFormat.ARGB32;

                // RT2, ARGB2101010 format: World space normal (RGB), unused (A).
                case BuiltinRenderTextureType.GBuffer2:
                    return RenderTextureFormat.ARGB2101010;

                // RT3, Light Buffer
                case BuiltinRenderTextureType.GBuffer3:
                    return hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB2101010;

                default:
                    throw new ArgumentException("Provided render texture type is not a GBuffer", "gbuffer");
            }
        }
    }
}