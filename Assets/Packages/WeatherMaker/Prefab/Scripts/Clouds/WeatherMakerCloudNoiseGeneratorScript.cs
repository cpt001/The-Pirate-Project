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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalRuby.WeatherMaker
{

#if UNITY_EDITOR

    /// <summary>
    /// Cloud noise generation script
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherMakerCloudNoiseGeneratorScript : MonoBehaviour
    {
        /// <summary>Noise profile</summary>
        [Header("Noise")]
        [Tooltip("Noise profile")]
        public WeatherMakerCloudNoiseProfileGroupScript NoiseProfile;

        /// <summary>Noise materials</summary>
        [Tooltip("Noise materials")]
        public Material[] NoiseMaterials;

        /// <summary>Noise render textures</summary>
        [Tooltip("Noise render textures")]
        public RenderTexture[] NoiseRenderTextures;

        /// <summary>How many frames to advance one frame</summary>
        [Header("Frames")]
        [Tooltip("How many frames to advance one frame")]
        [Range(1, 60)]
        public int FPSFrame = 1;
        private int fpsFrameCounter;
        private int frameIndex;
        private float frame;

        /// <summary>Whether to auto set the frame, set to false to allow ManualFrame override</summary>
        [Tooltip("Whether to auto set the frame, set to false to allow ManualFrame override")]
        public bool AutoStepFrame = true;

        /// <summary>Manually set the frame</summary>
        [Tooltip("Manually set the frame")]
        [Range(0, 1024)]
        public float ManualFrame;

        private Text frameLabel;
        private InputField fileTextBox;

        private void Awake()
        {
            GameObject obj = GameObject.Find("FrameLabel");
            if (obj != null)
            {
                frameLabel = obj.GetComponent<Text>();
            }
            obj = GameObject.Find("FileName");
            if (obj != null)
            {
                fileTextBox = obj.GetComponent<InputField>();
            }

            string file = UnityEditor.EditorPrefs.GetString("WeatherMakerCloudNoiseGeneratorFileName", string.Empty);
            if (string.IsNullOrEmpty(file))
            {
                fileTextBox.text = "Assets/WeatherMaker/Prefab/Textures/Clouds/CloudNoiseTexture.asset";
            }
            else
            {
                fileTextBox.text = file;
            }
        }

        private void Update()
        {
            if (NoiseProfile == null || NoiseProfile.NoiseProfiles == null || NoiseProfile.NoiseProfiles.Length > NoiseMaterials.Length)
            {
                return;
            }

            if (++fpsFrameCounter >= FPSFrame || !AutoStepFrame)
            {
                for (int i = 0; i < NoiseProfile.NoiseProfiles.Length; i++)
                {
                    NoiseProfile.NoiseProfiles[i].ApplyToMaterial(NoiseMaterials[i]);
                }
                float step = (NoiseProfile.Step <= 0.0f ? 1.0f / (float)NoiseProfile.Count : NoiseProfile.Step);
                fpsFrameCounter = 0;
                frame = (AutoStepFrame ? (float)frameIndex * step : (float)ManualFrame * step);
                if (frameLabel != null)
                {
                    frameLabel.text = (AutoStepFrame ? frameIndex : ManualFrame).ToString();
                }
                foreach (Material m in NoiseMaterials)
                {
                    m.SetFloat(WMS._CloudNoiseFrame, frame);
                }
                if (AutoStepFrame)
                {
                    if (++frameIndex >= NoiseProfile.Count)
                    {
                        frameIndex = 0;
                    }
                }
                else
                {
                    frameIndex = 0;
                }
                for (int i = 0; i < NoiseProfile.NoiseProfiles.Length; i++)
                {
                    Graphics.Blit(null, NoiseRenderTextures[i], NoiseMaterials[i], 0);
                }
            }
        }

        /// <summary>
        /// Browse clicked handler
        /// </summary>
        public void BrowseClicked()
        {
            string file = PickAssetFile(fileTextBox.text);
            if (!string.IsNullOrEmpty(file))
            {
                fileTextBox.text = file;
            }
        }

        /// <summary>
        /// Export clicked handler
        /// </summary>
        public void ExportClicked()
        {
            GenerateFramesAnd3DTexture(NoiseProfile, NoiseMaterials, fileTextBox.text);
        }

        /// <summary>
        /// Pick asset file click handler
        /// </summary>
        /// <param name="file">File name</param>
        /// <returns>Asset</returns>
        public static string PickAssetFile(string file)
        {
            file = UnityEditor.EditorUtility.SaveFilePanel("Choose 3D texture to save to", Path.GetDirectoryName(file), Path.GetFileName(file), "asset");
            if (!string.IsNullOrEmpty(file))
            {
                string dataPath = Application.dataPath;
                int pos = file.IndexOf(dataPath);
                if (pos >= 0)
                {
                    file = "Assets" + file.Substring(dataPath.Length);
                }
            }
            return file;
        }

        /// <summary>
        /// Generates all the individual frame textures
        /// </summary>
        /// <param name="profile">Noise profile</param>
        /// <param name="materials">Materials, should be one for each layer</param>
        /// <param name="progress">Current progress</param>
        /// <param name="progressMultiplier">Progress multiplier</param>
        /// <param name="clearProgress">Whether to restart progress</param>
        /// <returns>Texture path</returns>
        public static string GenerateFrameTextures(WeatherMakerCloudNoiseProfileGroupScript profile, Material[] materials, float progress = 0.0f, float progressMultiplier = 1.0f, bool clearProgress = true)
        {
            if (profile == null || profile.NoiseProfiles == null || materials == null || profile.NoiseProfiles.Length > materials.Length)
            {
                return null;
            }

            string tempPath = Path.GetTempPath();
            string texturePath = System.IO.Path.Combine(tempPath, "WeatherMakerNoiseTexture");
            UnityEditor.EditorUtility.DisplayProgressBar("Progress...", "", progress);
            bool isCurl = profile.NoiseProfiles.Length == 1 && profile.NoiseProfiles[0].NoiseType == WeatherMakerCloudNoiseType.Curl;
            int channels = (isCurl ? 3 : profile.NoiseProfiles.Length);
            try
            {
                RenderTexture[] renderTextures = new RenderTexture[profile.NoiseProfiles.Length];
                Texture2D[] textures = new Texture2D[profile.NoiseProfiles.Length];
                float currentProgress;
                for (int channelIndex = 0; channelIndex < profile.NoiseProfiles.Length; channelIndex++)
                {
                    RenderTexture renderTexture = new RenderTexture(profile.Size, profile.Size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    renderTexture.autoGenerateMips = false;
                    renderTexture.name = "WeatherMakerCloudRenderTextureTemp" + channelIndex;
                    renderTextures[channelIndex] = renderTexture;

                    Texture2D tex2D = new Texture2D(profile.Size, profile.Size, TextureFormat.ARGB32, false, false);
                    tex2D.filterMode = FilterMode.Bilinear;
                    tex2D.wrapMode = TextureWrapMode.Clamp;
                    textures[channelIndex] = tex2D;
                }

                float frame = 0.0f;
                float step = (profile.Step <= 0.0f ? 1.0f / (float)profile.Count : profile.Step);
                if (Directory.Exists(texturePath))
                {
                    Directory.Delete(texturePath, true);
                    System.Threading.Thread.Sleep(100);
                }
                Directory.CreateDirectory(texturePath);
                string[] dirs = new string[profile.NoiseProfiles.Length];
                for (int channelIndex = 0; channelIndex < profile.NoiseProfiles.Length; channelIndex++)
                {
                    if (isCurl)
                    {
                        profile.NoiseProfiles[0].ApplyToMaterial(materials[0]);
                    }
                    else
                    {
                        profile.NoiseProfiles[channelIndex].ApplyToMaterial(materials[channelIndex]);
                    }
                    dirs[channelIndex] = Path.Combine(texturePath, channelIndex.ToString());
                    Directory.CreateDirectory(dirs[channelIndex]);
                }
                for (int frameIndex = 0; frameIndex < profile.Count; frameIndex++)
                {
                    frame = (float)frameIndex * step;
                    for (int channelIndex = 0; channelIndex < profile.NoiseProfiles.Length; channelIndex++)
                    {
                        materials[channelIndex].SetFloat(WMS._CloudNoiseFrame, frame);
                        Graphics.Blit(null, renderTextures[channelIndex], materials[channelIndex], 0);
                        RenderTexture.active = renderTextures[channelIndex];
                        textures[channelIndex].ReadPixels(new Rect(0, 0, profile.Size, profile.Size), 0, 0, false);
                        textures[channelIndex].Apply();
                        RenderTexture.active = null;
                        GL.Flush();
                        byte[] imageData = textures[channelIndex].EncodeToPNG();
                        System.IO.File.WriteAllBytes(Path.Combine(dirs[channelIndex], "WeatherMakerNoiseTexture_" + frameIndex.ToString("D4") + ".png"), imageData);
                        if (isCurl)
                        {
                            break;
                            }
                    }
                    currentProgress = progress + ((((float)(frameIndex + 1) / (float)profile.Count) * progressMultiplier));
                    UnityEditor.EditorUtility.DisplayProgressBar("Progress...", "", currentProgress);
                }
                for (int i = 0; i < renderTextures.Length; i++)
                {
                    renderTextures[i].Release();
                    DestroyImmediate(renderTextures[i]);
                    DestroyImmediate(textures[i]);
                }
            }
            finally
            {
                if (clearProgress)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
            }
            return texturePath;
        }

        /// <summary>
        /// Generate a 3D texture from images
        /// </summary>
        /// <param name="inputFolder">Input folder</param>
        /// <param name="outputAssetPath">Output path</param>
        /// <param name="filterMode">Filter mode</param>
        /// <param name="mips">Enable mip maps?</param>
        /// <param name="showConfirm">Show confirm dialog?</param>
        /// <param name="progress">Current progress</param>
        /// <param name="progressMultiplier">Progress multiplier</param>
        /// <param name="clearProgress">Whether to restart progress</param>
        /// <param name="isCurl">Is curl</param>
        public static void Generate3DTexture(string inputFolder, string outputAssetPath, FilterMode filterMode, bool mips,
            bool showConfirm = false, float progress = 0.0f, float progressMultiplier = 1.0f, bool clearProgress = true, bool isCurl = false)
        {
            float currentProgress;
            UnityEditor.EditorUtility.DisplayProgressBar("Progress...", "", progress);
            try
            {
                UnityEditor.EditorPrefs.SetString("WeatherMakerCloudNoiseGeneratorFileName", outputAssetPath);
                Directory.CreateDirectory(inputFolder);
                Texture2D asset2D = null;
                Texture3D asset3D = null;
                string[] allFiles = Directory.GetFiles(inputFolder);
                List<string> files1 = new List<string>();
                List<string> files2 = new List<string>();
                List<string> files3 = new List<string>();
                List<string> files4 = new List<string>();
                List<string>[] filesArray = new List<string>[0];
                foreach (string file in allFiles.OrderBy(f => f))
                {
                    if (System.IO.Path.GetExtension(file).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || System.IO.Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        files1.Add(file);
                    }
                }

                TextureFormat format;

                // if only sub folders, read those...
                if (files1.Count == 0)
                {
                    // try rgba channel approach, 4 folders, one for each channel
                    string[] subFolders = Directory.GetDirectories(inputFolder);
                    for (int i = 0; i < subFolders.Length; i++)
                    {
                        allFiles = Directory.GetFiles(subFolders[i]);
                        Array.Resize(ref filesArray, filesArray.Length + 1);
                        filesArray[filesArray.Length - 1] = (i == 0 ? files1 : (i == 1 ? files2 : (i == 2 ? files3 : files4)));
                        foreach (string file in allFiles.OrderBy(f => f))
                        {
                            if (System.IO.Path.GetExtension(file).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || System.IO.Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
                            {
                                filesArray[i].Add(file);
                            }
                        }
                    }
                    switch (subFolders.Length)
                    {
                        case 1:
                            format = (isCurl ? TextureFormat.RGB24 : TextureFormat.Alpha8);
                            break;

                        case 2:
                            format = TextureFormat.RG16;
                            break;

                        case 3:
                            format = TextureFormat.RGB24;
                            break;

                        default:
                            format = TextureFormat.ARGB32;
                            break;
                    }
                }
                else
                {
                    // files are in the specified folder itself
                    filesArray = new List<string>[] { files1 };
                    format = TextureFormat.ARGB32;
                }

                if (files1.Count == 0 || (files2.Count != 0 && (files1.Count != files2.Count || files2.Count != files3.Count || files3.Count != files4.Count)))
                {
                    UnityEditor.EditorUtility.DisplayDialog("Unable to create 3D texture", "No files found or mismatching file counts in sub folders", "OK");
                }
                else
                {
                    Color32[] allPixels;
                    Texture2D tex2D = new Texture2D(1, 1, format, false, false);
                    int idx = 0;

                    try
                    {
                        tex2D.LoadImage(File.ReadAllBytes(files1[0]));
                        if (files1.Count == 1)
                        {
                            asset2D = UnityEditor.AssetDatabase.LoadAssetAtPath(outputAssetPath, typeof(Texture2D)) as Texture2D;
                        }
                        else
                        {
                            asset3D = UnityEditor.AssetDatabase.LoadAssetAtPath(outputAssetPath, typeof(Texture3D)) as Texture3D;
                        }
                        bool saveAsset = false;
                        if (files1.Count == 1)
                        {
                            if (asset2D == null || asset2D.width != tex2D.width || asset2D.height != tex2D.height ||
                                asset2D.format != format || asset2D.filterMode != filterMode)
                            {
                                saveAsset = true;
                                asset2D = new Texture2D(tex2D.width, tex2D.height, format, mips)
                                {
                                    filterMode = filterMode,
                                    wrapMode = TextureWrapMode.Repeat,
                                    name = "Texture2D (Weather Maker)"
                                };
                            }
                        }
                        else if (asset3D == null || asset3D.width != tex2D.width || asset3D.height != tex2D.height || asset3D.depth != files1.Count ||
                            asset3D.format != format || asset3D.filterMode != filterMode)
                        {
                            saveAsset = true;
                            asset3D = new Texture3D(tex2D.width, tex2D.height, files1.Count, format, mips)
                            {
                                filterMode = filterMode,
                                wrapMode = TextureWrapMode.Repeat,
                                name = "Texture3D (Weather Maker)"
                            };
                        }
                        allPixels = new Color32[tex2D.width * tex2D.height * files1.Count];
                        if (files2.Count == 0)
                        {
                            for (int fileIndex = 0; fileIndex < files1.Count; fileIndex++)
                            {
                                string file = files1[fileIndex];
                                tex2D.LoadImage(File.ReadAllBytes(file));
                                Color32[] pixels = tex2D.GetPixels32();
                                for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
                                {
                                    Color32 p = pixels[pixelIndex];
                                    if (format == TextureFormat.Alpha8)
                                    {
                                        byte b = Math.Min(p.r, p.a);
                                        allPixels[idx].r = b;
                                        allPixels[idx].g = b;
                                        allPixels[idx].b = b;
                                        allPixels[idx].a = b;
                                    }
                                    else if (asset2D != null)
                                    {
                                        p.r = p.g = p.b = (byte)Math.Min(255, ((int)p.r + (int)p.g + (int)p.b + (int)p.a));
                                        p.a = 255;
                                        allPixels[idx] = p;
                                    }
                                    else
                                    {
                                        allPixels[idx] = p;
                                    }
                                    idx++;
                                }
                                currentProgress = (progress + (((float)(fileIndex + 1) / (float)filesArray[0].Count) * progressMultiplier));
                                UnityEditor.EditorUtility.DisplayProgressBar("Progress...", "", currentProgress);
                            }
                        }
                        else
                        {
                            int lastLength = -1;
                            Color32[] subPixels = new Color32[tex2D.width * tex2D.height];
                            for (int fileIndex = 0; fileIndex < filesArray[0].Count; fileIndex++)
                            {
                                for (int subFileIndex = 0; subFileIndex < filesArray.Length; subFileIndex++)
                                {
                                    string file = filesArray[subFileIndex][fileIndex];
                                    tex2D.LoadImage(File.ReadAllBytes(file));
                                    Color32[] imgPixels = tex2D.GetPixels32();
                                    if (lastLength != -1 && subPixels.Length != lastLength)
                                    {
                                        UnityEditor.EditorUtility.DisplayDialog("Unable to create 3D texture", "Mismatching image size in " + file, "OK");
                                        throw new InvalidOperationException("Mismatching image size in " + file);
                                    }
                                    lastLength = subPixels.Length;
                                    for (int pixelIndex = 0; pixelIndex < imgPixels.Length; pixelIndex++)
                                    {
                                        switch (subFileIndex)
                                        {
                                            case 0:
                                                subPixels[pixelIndex].r = Math.Min(imgPixels[pixelIndex].r, imgPixels[pixelIndex].a);
                                                break;

                                            case 1:
                                                subPixels[pixelIndex].g = Math.Min(imgPixels[pixelIndex].r, imgPixels[pixelIndex].a);
                                                break;

                                            case 2:
                                                subPixels[pixelIndex].b = Math.Min(imgPixels[pixelIndex].r, imgPixels[pixelIndex].a);
                                                break;

                                            case 3:
                                                subPixels[pixelIndex].a = Math.Min(imgPixels[pixelIndex].r, imgPixels[pixelIndex].a);
                                                break;
                                        }
                                    }
                                }
                                foreach (Color32 pixel in subPixels)
                                {
                                    if (asset2D != null)
                                    {
                                        Color32 p = pixel;
                                        p.r = p.g = p.b = (byte)Math.Min(255, ((int)p.r + (int)p.g + (int)p.b + (int)p.a));
                                        p.a = 255;
                                        allPixels[idx++] = p;
                                    }
                                    else
                                    {
                                        allPixels[idx++] = pixel;
                                    }
                                }
                                currentProgress = (progress + (((float)(fileIndex + 1) / (float)filesArray[0].Count) * progressMultiplier));
                                UnityEditor.EditorUtility.DisplayProgressBar("Progress...", "", currentProgress);
                            }
                        }
                        if (asset2D != null)
                        {
                            asset2D.SetPixels32(allPixels);
                            asset2D.Apply();
                        }
                        else
                        {
                            asset3D.SetPixels32(allPixels);
                            asset3D.Apply(true);
                        }
                        if (saveAsset)
                        {
                            if (asset2D != null)
                            {
                                UnityEditor.AssetDatabase.CreateAsset(asset2D, outputAssetPath);
                            }
                            else
                            {
                                UnityEditor.AssetDatabase.CreateAsset(asset3D, outputAssetPath);
                            }
                        }
                        else
                        {
                            UnityEditor.AssetDatabase.ImportAsset(outputAssetPath);
                        }
                        if (asset2D != null)
                        {
                            byte[] png = asset2D.EncodeToPNG();
                            File.WriteAllBytes(outputAssetPath + ".png", png);
                        }
                        UnityEditor.AssetDatabase.Refresh();
                        if (showConfirm)
                        {
                            if (asset2D != null)
                            {
                                UnityEditor.EditorUtility.DisplayDialog("2D texture saved", "New texture assets created as '" + outputAssetPath + "'", "OK");
                            }
                            else
                            {
                                UnityEditor.EditorUtility.DisplayDialog("3D texture saved", "New texture asset created as '" + outputAssetPath + "'", "OK");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("Unable to create 3D texture", ex.ToString(), "OK");
                        if (asset2D != null)
                        {
                            GameObject.DestroyImmediate(asset2D);
                        }
                        if (asset3D != null)
                        {
                            GameObject.DestroyImmediate(asset3D);
                        }
                    }
                    finally
                    {
                        GameObject.DestroyImmediate(tex2D);
                    }
                }
            }
            finally
            {
                if (clearProgress)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// Generates frames and 3D texture
        /// </summary>
        /// <param name="noiseProfile">Noise profile</param>
        /// <param name="materials">Materials, one for each layer</param>
        /// <param name="textureAssetFileName">Texture file name</param>
        /// <param name="showConfirm">Show confirm dialog?</param>
        public static void GenerateFramesAnd3DTexture(WeatherMakerCloudNoiseProfileGroupScript noiseProfile, Material[] materials, string textureAssetFileName, bool showConfirm = false)
        {
            string tempPath = Path.GetTempPath();
            tempPath = System.IO.Path.Combine(tempPath, "WeatherMakerNoiseTexture");
            string texturesPath = WeatherMakerCloudNoiseGeneratorScript.GenerateFrameTextures(noiseProfile, materials, 0.0f, 0.5f, false);
            bool isCurl = (noiseProfile.NoiseProfiles.Length == 1 && noiseProfile.NoiseProfiles[0].NoiseType == WeatherMakerCloudNoiseType.Curl);
            WeatherMakerCloudNoiseGeneratorScript.Generate3DTexture(texturesPath, textureAssetFileName, noiseProfile.FilterMode, noiseProfile.GenerateMips,
                progress: 0.5f, progressMultiplier: 0.5f, isCurl: isCurl);
        }
    }

#endif

}
