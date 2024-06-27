
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Allows lerping two textures into a third texture using computer shader
    /// </summary>
    public class WeatherMakerTextureLerpScript : System.IDisposable
    {
        /// <summary>
        /// First input texture
        /// </summary>
        public Texture Texture1 { get; private set; }

        /// <summary>
        /// Second input texture
        /// </summary>
        public Texture Texture2 { get; private set; }

        /// <summary>
        /// Output texture
        /// </summary>
        public Texture TextureResult { get; private set; }

        /// <summary>
        /// The current lerp value
        /// </summary>
        public float Lerp { get; private set; }

        private static ComputeShader cs2D;
        private static int cs2DKernel;
        private static ComputeShader cs3D;
        private static int cs3DKernel;
        private static Vector3 cs2DThreadSize;
        private static Vector3 cs3DThreadSize;

        private readonly int kernel;
        private readonly int thread1;
        private readonly int thread2;
        private readonly int thread3;

        private RenderTexture rex;
        private ComputeShader cs;
        private bool canDispatch = true;
        private UnityEngine.Rendering.AsyncGPUReadbackRequest currentRequest;

        private readonly System.Action<UnityEngine.Rendering.AsyncGPUReadbackRequest> csCallback;

        private static void InitializeStatic()
        {
            if (SystemInfo.supportsComputeShaders && SystemInfo.supportsAsyncGPUReadback && cs2D == null)
            {
                cs2D = WeatherMakerScript.Instance.LoadResource<ComputeShader>("WeatherMakerTexture2DLerpShader");
                cs3D = WeatherMakerScript.Instance.LoadResource<ComputeShader>("WeatherMakerTexture3DLerpShader");
                cs2DKernel = cs2D.FindKernel("CSMain");
                cs3DKernel = cs3D.FindKernel("CSMain");
                uint x, y, z;
                cs2D.GetKernelThreadGroupSizes(cs2DKernel, out x, out y, out z);
                cs2DThreadSize = new Vector3(x, y, z);
                cs3D.GetKernelThreadGroupSizes(cs2DKernel, out x, out y, out z);
                cs3DThreadSize = new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="t1">First texture to lerp from</param>
        /// <param name="t2">Second texture to lerp to</param>
        public WeatherMakerTextureLerpScript(Texture t1, Texture t2)
        {
            Texture1 = t1;
            Texture2 = t2;

            if (t1 == t2 || cs2D == null || cs3D == null ||
                !SystemInfo.supportsComputeShaders || !SystemInfo.supportsAsyncGPUReadback)
            {
                // identical textures or unable to compute lerp
                TextureResult = t2;
                return;
            }

            // until we get at least one run in, we have to stick with original texture
            InitializeStatic();
            TextureResult = t1;
            rex = new RenderTexture(t2.width, t2.height, 0, GetFormat(t2), RenderTextureReadWrite.Linear);

            if (t1 is Texture2D)
            {
                cs = GameObject.Instantiate<ComputeShader>(cs2D);
                kernel = cs2DKernel;
                thread1 = Mathf.FloorToInt(rex.width / cs2DThreadSize.x);
                thread2 = Mathf.FloorToInt(rex.height / cs2DThreadSize.y);
                thread3 = 1;
                cs.SetVector("_Dimensions", new Vector4(1.0f / (float)rex.width, 1.0f / (float)rex.height, 0.0f, 0.0f));
            }
            else if (t1 is Texture3D)
            {
                cs = GameObject.Instantiate<ComputeShader>(cs3D);
                kernel = cs3DKernel;
                rex.volumeDepth = ((Texture3D)t2).depth;
                thread1 = Mathf.FloorToInt(rex.width / cs3DThreadSize.x);
                thread2 = Mathf.FloorToInt(rex.height / cs3DThreadSize.y);
                thread3 = Mathf.FloorToInt(rex.volumeDepth / cs3DThreadSize.z);
                cs.SetVector("_Dimensions", new Vector4(1.0f / (float)rex.width, 1.0f / (float)rex.height, 1.0f / (float)rex.volumeDepth, 0.0f));
            }
            else
            {
                throw new System.ArgumentException("Invalid textures, need Texture2D or Texture3D");
            }

            rex.filterMode = t2.filterMode;
            rex.wrapMode = t2.wrapMode;
            rex.dimension = t2.dimension;
            rex.autoGenerateMips = false;
            rex.useMipMap = false;
            rex.enableRandomWrite = true;
            rex.name = t1.name + "_lerp_" + t2.name;
            rex.Create();

            csCallback = (UnityEngine.Rendering.AsyncGPUReadbackRequest req) =>
            {
                TextureResult = rex;
                canDispatch = true;
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            WeatherMakerFullScreenEffect.DestroyRenderTexture(ref rex);
            if (cs != null)
            {
                GameObject.DestroyImmediate(cs, true);
            }
            canDispatch = false;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="progress">Progress (0 - 1)</param>
        /// <returns>Current texture</returns>
        public Texture UpdateProgress(float progress)
        {
            Lerp = progress;

            if (cs != null)
            {
                // identical textures, no need to lerp
                if (progress <= 0.0f || Texture1 == Texture2)
                {
                    TextureResult = Texture1;
                }
                // done lerping
                else if (progress >= 1.0f)
                {
                    TextureResult = Texture2;
                }
                else if (canDispatch)
                {
                    canDispatch = false;
                    cs.SetTexture(kernel, "_Tex1", Texture1);
                    cs.SetTexture(kernel, "_Tex2", Texture2);
                    cs.SetTexture(kernel, "_Tex3", rex);
                    cs.SetFloat("_Lerp", Lerp);
                    cs.Dispatch(kernel, thread1, thread2, thread3);
                    UnityEngine.Rendering.AsyncGPUReadback.Request(rex, 0, csCallback);
                }
            }

            return TextureResult;
        }

        private RenderTextureFormat GetFormat(Texture t)
        {
            TextureFormat f;
            if (t is Texture2D)
            {
                f = ((Texture2D)t).format;
            }
            else if (t is Texture3D)
            {
                f = ((Texture3D)t).format;
            }
            else
            {
                throw new System.ArgumentException("Input textures must be either 2D or 3D textures");
            }

            switch (f)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.RHalf:
                    return RenderTextureFormat.RHalf;

                case TextureFormat.RFloat:
                    return RenderTextureFormat.RFloat;

                case TextureFormat.ARGB32:
                case TextureFormat.RGBA32:
                case TextureFormat.BGRA32:
                    return RenderTextureFormat.ARGB32;

                case TextureFormat.RGBAHalf:
                    return RenderTextureFormat.ARGBHalf;

                case TextureFormat.RGBAFloat:
                    return RenderTextureFormat.ARGBFloat;                    
            }

            throw new System.ArgumentException("Invalid lerp input texture format " + f);
        }
    }
}