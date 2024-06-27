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

using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;

#if UNITY_POST_PROCESSING_STACK_V2 && UNITY_2018_3_OR_NEWER

using UnityEngine.Rendering.PostProcessing;

#endif

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerGenerateNormalTexture2DWindow : EditorWindow
    {
        private string inputFile;
        private string outputFile;

        private Texture2D inputFileTexture;
        private Texture2D outputFileTexture;
        private Vector2 scrollPosition;
        private Material material;
        private bool texturesDirty = true;

        private void LoadTextures()
        {
            if (!texturesDirty || string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile))
            {
                return;
            }
            texturesDirty = false;

            try
            {
                if (inputFileTexture == null)
                {
                    inputFileTexture = new Texture2D(1, 1);
                }
                inputFileTexture.LoadImage(File.ReadAllBytes(inputFile));
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Unable to load input texture: {0}", ex.Message);
            }
            try
            {
                if (outputFileTexture == null)
                {
                    outputFileTexture = new Texture2D(1, 1);
                }
                outputFileTexture.LoadImage(File.ReadAllBytes(outputFile));
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Unable to load output texture: {0}", ex.Message);
            }
        }

        private void OnEnable()
        {
            OnFocus();
        }

        private void OnDisable()
        {
            OnLostFocus();
        }

        private void OnFocus()
        {
            inputFile = EditorPrefs.GetString("WeatherMaker_GenerateNormalTexture2DWindow_inputFile");
            outputFile = EditorPrefs.GetString("WeatherMaker_GenerateNormalTexture2DWindow_outputFile");
            LoadTextures();
        }

        private void OnLostFocus()
        {
            EditorPrefs.SetString("WeatherMaker_GenerateNormalTexture2DWindow_inputFile", inputFile);
            EditorPrefs.SetString("WeatherMaker_GenerateNormalTexture2DWindow_outputFile", outputFile);
            texturesDirty = true;
        }

        private void OnDestroy()
        {
            OnLostFocus();
        }

        private void OnGUI()
        {
            const float textHeight = 20.0f;
            const float textButtonWidth = 100.0f;
            const float submitButtonWidth = 150.0f;
            EditorGUIUtility.labelWidth = 80.0f;
            EditorGUI.BeginChangeCheck();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Input Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            inputFile = EditorGUILayout.TextField(new GUIContent(string.Empty, inputFile), inputFile, GUILayout.ExpandWidth(true), GUILayout.Height(textHeight));
            if (GUILayout.Button("Input...", GUILayout.MaxWidth(textButtonWidth)))
            {
                string newFile = EditorUtility.OpenFilePanelWithFilters("Select input image", string.Empty, WeatherMakerEditorCommands.ImageFilesFilter);
                if (!string.IsNullOrEmpty(newFile))
                {
                    inputFile = newFile;
                    LoadTextures();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Output Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            outputFile = EditorGUILayout.TextField(new GUIContent(string.Empty, outputFile), outputFile, GUILayout.ExpandWidth(true), GUILayout.Height(textHeight));
            if (GUILayout.Button("Output...", GUILayout.MaxWidth(textButtonWidth)))
            {
                string newFile = EditorUtility.SaveFilePanel("Select output image", string.Empty, "Normals.png", "png");
                if (!string.IsNullOrEmpty(newFile))
                {
                    outputFile = newFile;
                    LoadTextures();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(inputFileTexture, GUILayout.MaxWidth(256.0f), GUILayout.MaxHeight(256.0f));
            GUILayout.Label(outputFileTexture, GUILayout.MaxWidth(256.0f), GUILayout.MaxHeight(256.0f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Normals", GUILayout.MaxWidth(submitButtonWidth)))
            {
                // do normals
                if (material == null)
                {
                    Shader shader = Shader.Find("WeatherMaker/WeatherMakerNormalGenerator2DShader");
                    material = new Material(shader);
                }
                Texture2D input = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                input.wrapMode = TextureWrapMode.Repeat;
                input.filterMode = FilterMode.Bilinear;
                input.LoadImage(File.ReadAllBytes(inputFile));
                material.SetTexture(WMS._MainTex, input);
                Texture2D output = new Texture2D(input.width, input.height, TextureFormat.ARGB32, false);
                output.wrapMode = TextureWrapMode.Repeat;
                RenderTexture outputRT = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                RenderTexture active = RenderTexture.active;
                RenderTexture.active = outputRT;
                Graphics.Blit(input, material, 0);
                output.ReadPixels(new Rect(0, 0, input.width, input.height), 0, 0);
                output.Apply();
                RenderTexture.active = active;
                GameObject.DestroyImmediate(material);
                if (System.IO.Path.GetExtension(outputFile).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllBytes(outputFile, output.EncodeToPNG());
                }
                else
                {
                    File.WriteAllBytes(outputFile, output.EncodeToJPG(92));
                }
                UnityEngine.Object.DestroyImmediate(input);
                UnityEngine.Object.DestroyImmediate(output);
            }

            if (EditorGUI.EndChangeCheck())
            {
                texturesDirty = true;
                OnFocus();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    public static class WeatherMakerEditorCommands
    {
        public static readonly string[] ImageFilesFilter = new string[] { "Image Files", "png,jpg,jpeg", "All Files", "*" };

        private static void Create3DNoiseTextureByPickingAsset()
        {
            string inputFile = EditorUtility.OpenFilePanel("Select asset to convert to 3D texture", string.Empty, string.Empty);
            string rootPath = Application.dataPath;
            if (rootPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
            {
                rootPath = rootPath.Substring(0, rootPath.Length - 6);
            }
            inputFile = inputFile.Substring(rootPath.Length);
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(inputFile);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Unable to create 3D texture", "Unable to load asset", "OK");
                return;
            }

            int size = 32;
            TextureFormat format = TextureFormat.RGB24;
            byte[] bytes = asset.bytes;
            int count = size * size * size;
            Color32[] colors = new Color32[count];
            int j = 0;

            for (int i = 0; i < count; i++)
            {
                colors[i].r = bytes[j++];
                colors[i].g = bytes[j++];
                colors[i].b = bytes[j++];
                colors[i].a = format == TextureFormat.RGBA32 ? bytes[j++] : (byte)255;
            }

            Texture3D t = new Texture3D(size, size, size, format, true);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            t.SetPixels32(colors, 0);
            t.Apply(true);

            /*
            byte[] bytes = data.bytes;
            uint height = BitConverter.ToUInt32(data.bytes, 12);
            uint width = BitConverter.ToUInt32(data.bytes, 16);
            uint pitch = BitConverter.ToUInt32(data.bytes, 20);
            uint depth = BitConverter.ToUInt32(data.bytes, 24);
            uint formatFlags = BitConverter.ToUInt32(data.bytes, 20 * 4);
            BitConverter.ToUInt32(data.bytes, 21 * 4); // uint fourCC = 
            uint bitdepth = BitConverter.ToUInt32(data.bytes, 22 * 4);
            if (bitdepth == 0)
            {
                bitdepth = pitch / width * 8;
            }

            Color32[] c = new Color32[width * height * depth];

            uint index = 128;
            if (data.bytes[21 * 4] == 'D' && data.bytes[21 * 4 + 1] == 'X' && data.bytes[21 * 4 + 2] == '1' &&
                data.bytes[21 * 4 + 3] == '0' && (formatFlags & 0x4) != 0)
            {
                uint format = BitConverter.ToUInt32(data.bytes, (int)index);
                if (format >= 60 && format <= 65)
                {
                    bitdepth = 8;
                }
                else if (format >= 48 && format <= 52)
                {
                    bitdepth = 16;
                }
                else if (format >= 27 && format <= 32)
                {
                    bitdepth = 32;
                }
                index += 20;
            }

            Texture3D t = new Texture3D((int)width, (int)height, (int)depth, (bitdepth == 8 ? TextureFormat.Alpha8 : TextureFormat.ARGB32), false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Repeat;
            t.name = "Noise 3D (Weather Maker)";

            uint byteDepth = bitdepth / 8;
            pitch = (width * bitdepth + 7) / 8;

            for (int d = 0; d < depth; ++d)
            {
                for (int h = 0; h < height; ++h)
                {
                    for (int w = 0; w < width; ++w)
                    {
                        byte v = bytes[index + w * byteDepth];
                        c[w + h * width + d * width * height] = new Color32(v, v, v, v);
                    }

                    index += pitch;
                }
            }

            t.SetPixels32(c);
            t.Apply();
            */

            UnityEditor.AssetDatabase.CreateAsset(t, "Assets/My3DTexture.asset");
            EditorUtility.DisplayDialog("3D texture saved", "New texture asset created as 'Assets/My3DTexture.asset", "OK");
        }

        /// <summary>
        /// Instantiate a prefab from an asset into the active scene
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="undoName">Undo operation name</param>
        /// <param name="identityPosition">Whether to set position and rotation to 0</param>
        /// <param name="setup">Callback for any additional setup that will be performed on the instantiated prefab</param>
        public static void InstantiatePrefab(string assetName, string undoName, bool identityPosition, System.Action<GameObject> setup = null)
        {
            string[] results = AssetDatabase.FindAssets(assetName);
            if (results.Length == 0)
            {
                Debug.LogError("Unable to find " + assetName + " in project");
                return;
            }
            GameObject prefab = null;
            foreach (string result in results)
            {
                string path = AssetDatabase.GUIDToAssetPath(result);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    break;
                }
            }

            if (prefab == null)
            {
                Debug.LogError("Unable to deserialize prefab");
                return;
            }

#if UNITY_2018_3_OR_NEWER

            PrefabAssetType type = PrefabUtility.GetPrefabAssetType(prefab);
            if (type == PrefabAssetType.MissingAsset || type == PrefabAssetType.NotAPrefab)

#else

            PrefabType type = PrefabUtility.GetPrefabType(prefab);
            if (type == PrefabType.None)

#endif

            {
                Debug.LogError("Unable to load prefab, please reimport all assets");
                return;
            }
            var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            obj.name = obj.name.Replace("(Clone)", string.Empty).Trim();
            obj.transform.parent = null;
            if (identityPosition)
            {
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
            }
            if (setup != null)
            {
                setup.Invoke(obj);
            }
            Undo.RegisterCreatedObjectUndo(obj, undoName);
        }

        [MenuItem("Window/Weather Maker/Add Weather Maker to Scene", false, priority = 30)]
        public static void AddWeatherMakerPrefab()
        {
            string name = (Camera.main == null || !Camera.main.orthographic ? "WeatherMakerPrefab" : "WeatherMakerPrefab2D");
            InstantiatePrefab(name, "Add Weather Maker", true, (obj) =>
            {
                if (Camera.main != null)
                {
                    Undo.RecordObject(Camera.main, "Camera Clear Solid Color");
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.clear;
                }
            });
        }

        [MenuItem("Window/Weather Maker/Add Weather Maker Player to Scene", false, priority = 31)]
        public static void AddWeatherMakerPlayerPrefab()
        {
            InstantiatePrefab("WeatherMakerPlayer", "Add Weather Maker Player", true);
        }

        [MenuItem("Window/Weather Maker/Setup Post Processing", false, priority = 32)]
        public static void SetupPostProcessing()
        {

#if UNITY_POST_PROCESSING_STACK_V2 && UNITY_2018_3_OR_NEWER

            const string defaultName = "WeatherMakerPostProcessingProfile_Default";
            string[] assets = AssetDatabase.FindAssets("WeatherMakerPrefab");
            foreach (string asset in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                if (path.EndsWith("WeatherMakerPrefab.prefab", StringComparison.OrdinalIgnoreCase))
                {
                    string dir = Path.GetDirectoryName(path);
                    string profilesDir = Path.Combine(dir, "Profiles");
                    profilesDir = Path.Combine(profilesDir, "PostProcessing");
                    string assetPath = Path.Combine(profilesDir, defaultName + ".asset");
                    Directory.CreateDirectory(profilesDir);
                    PostProcessProfile profile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(assetPath);

                    if (profile == null)
                    {
                        EditorUtility.DisplayDialog("Error", "Unable to find Weather Maker default post processing profile at '" + assetPath + "'", "OK");
                        return;
                        /*
                        profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                        profile.name = defaultName;
                        AmbientOcclusion ambient = profile.AddSettings<AmbientOcclusion>();
                        ambient.intensity = new FloatParameter { value = 0.5f, overrideState = true };
                        Bloom bloom = profile.AddSettings<Bloom>();
                        bloom.intensity = new FloatParameter { value = 3.0f, overrideState = true };
                        ColorGrading color = profile.AddSettings<ColorGrading>();
                        color.tonemapper = new TonemapperParameter { value = Tonemapper.ACES, overrideState = true };
                        color.postExposure = new FloatParameter { value = 0.7f, overrideState = true };
                        DepthOfField df = profile.AddSettings<DepthOfField>();
                        df.focusDistance = new FloatParameter { value = 3.5f, overrideState = true };
                        Vignette vig = profile.AddSettings<Vignette>();
                        vig.intensity = new FloatParameter { value = 0.3f, overrideState = true };

                        if (createProfile)
                        {
                            AssetDatabase.CreateAsset(profile, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            EditorUtility.FocusProjectWindow();
                        }
                        */
                    }

                    Camera cam = Camera.main;
                    if (cam == null)
                    {
                        EditorUtility.DisplayDialog("Error", "Could not find main camera in scene, make sure your camera is tagged as main camera", "OK");
                        return;
                    }

                    PostProcessLayer layer = cam.GetComponent<PostProcessLayer>();
                    if (layer == null)
                    {
                        layer = Undo.AddComponent<PostProcessLayer>(cam.gameObject);
                    }
                    layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    layer.fog = new Fog { enabled = false, excludeSkybox = true };
                    layer.volumeLayer = -1;

                    PostProcessVolume volume = cam.GetComponent<PostProcessVolume>();
                    if (volume == null)
                    {
                        volume = Undo.AddComponent<PostProcessVolume>(cam.gameObject);
                    }
                    volume.sharedProfile = profile;
                    volume.isGlobal = true;
                    Undo.RegisterCompleteObjectUndo(layer, "Weather Maker Add Post Processing Volume");

                    Selection.activeObject = cam.gameObject;

                    EditorUtility.DisplayDialog("Success", "Post processing profile created at '" + defaultName + "' and applied to main camera. " +
                        "You should configure your layer on the camera and post processing for best performance.", "OK");

                    return;
                }
            }

            EditorUtility.DisplayDialog("Error", "There was an error setting up the post processing stack, please do it manually", "OK");

#else

            EditorUtility.DisplayDialog("Error", "Please use Unity 2018.3 or newer and use the package manager to add the post processing stack v2. " +
                "Also be sure 'UNITY_POST_PROCESSING_STACK_V2' is in player settings -> scripting defines. Do all this and try again.", "OK");

#endif

        }

#if UNITY_2021_1_OR_NEWER

        [MenuItem("Window/Weather Maker/Enable Standard Render Pipeline", false, priority = 43)]
        public static void EnableStandardPipeline()
        {
            UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = null;
            UnityEngine.QualitySettings.renderPipeline = null;
            WeatherMakerEditorUtility.ReplaceGrabPassInAllShaders(true);
            WeatherMakerEditorUtility.UpdatePreProcessor(false, "UNITY_URP");
            WeatherMakerEditorUtility.UpdatePreProcessor(false, "UNITY_HDRP");
            AssetDatabase.Refresh();
            UnityEditor.EditorUtility.DisplayDialog("Success", "Standard pipeline enabled, please restart Unity.", "OK");
        }

        [MenuItem("Window/Weather Maker/Enable Universal Render Pipeline", false, priority = 44)]
        public static void EnableURP()
        {
            EnableURPInternal("UnityEngine.Rendering.Universal.UniversalRenderPipeline",
                "WeatherMakerURPProfile",
                "Please import URP package first. If you just did, please restart Unity and try again.",
                "Unable to find WeatherMakerURPProfile, please re-download from asset store.",
                "URP enabled. Please restart Unity. Please ensure render features are assigned properly. You need screen space shadows as the first render feature and Weather Maker as the second. " +
                "If you are seeing strange errors, please re-create the URP forward renderer, add screen space shadows and then Weather Maker render features and " +
                "then re-assign to the URP render asset.");
        }

        private static void EnableURPInternal(string type, string profile, string importErrorText, string profileErrorText, string renderFeatureText)
        {
            if (!WeatherMakerEditorUtility.TypeExists(type))
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", importErrorText, "OK");
                return;
            }

            string[] guids = AssetDatabase.FindAssets(profile);
            if (guids.Length == 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", profileErrorText, "OK");
                return;
            }

            WeatherMakerEditorUtility.ReplaceGrabPassInAllShaders(false);
            WeatherMakerEditorUtility.UpdatePreProcessor(true, "UNITY_URP");
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(assetPath);
            UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = pipeline;
            UnityEngine.QualitySettings.renderPipeline = pipeline;
            UnityEditor.EditorUtility.DisplayDialog("Notice", renderFeatureText, "OK");
        }

#endif

        [MenuItem("Window/Weather Maker/Generate Normal Texture (2D)", false, priority = 71)]
        public static void GenerateNormalTexture2D()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(WeatherMakerGenerateNormalTexture2DWindow));
            window.titleContent = new GUIContent("Normals");
        }

        [MenuItem("Window/Weather Maker/Generate 3D Texture", false, priority = 72)]
        public static void Generate3DTexture()
        {
            string inputFolder = EditorUtility.OpenFolderPanel("Select input assets folder of images, i.e. myproject/assets/subfolder/images/", string.Empty, string.Empty);
            if (string.IsNullOrEmpty(inputFolder))
            {
                return;
            }
            WeatherMakerCloudNoiseGeneratorScript.Generate3DTexture(inputFolder, "Assets/My3DTexture.asset", FilterMode.Bilinear, true);
        }

        [MenuItem("Window/Weather Maker/Extract 3D Texture", false, priority = 73)]
        public static void Extract3DTexture()
        {
            string inputFile = EditorUtility.OpenFilePanel("Select 3d texture to extract, i.e. myproject/assets/subfolder/My3DTexture.asset/", string.Empty, "asset");
            if (string.IsNullOrEmpty(inputFile))
            {
                return;
            }

            string rootPath = Application.dataPath;
            if (rootPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
            {
                rootPath = rootPath.Substring(0, rootPath.Length - 6);
            }
            string assetPath = inputFile.Substring(rootPath.Length);
            Texture3D tex3D = AssetDatabase.LoadAssetAtPath<Texture3D>(assetPath);
            if (tex3D == null)
            {
                EditorUtility.DisplayDialog("Unable to load 3D texture", "Error loading asset", "OK");
                return;
            }
            string docsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            docsPath = System.IO.Path.Combine(docsPath, "WeatherMakerNoiseTexture/Extracted3DTexture");
            SerializationHelper.Extract3DTexture(tex3D, docsPath);
            EditorUtility.DisplayDialog("3D texture extracted", "Saved png to " + docsPath, "OK");
        }

        [MenuItem("Window/Weather Maker/Create 3D Texture from TextAsset", false, priority = 74)]
        public static void Create3DTextureFromTextAsset()
        {
            Create3DNoiseTextureByPickingAsset();
        }

        [MenuItem("Window/Weather Maker/Combine Normal and Alpha Textures", false, priority = 75)]
        public static void CombineNormalAndAlphaTextures()
        {
            string normalMapFile = EditorUtility.OpenFilePanelWithFilters("Select normal map texture", string.Empty, ImageFilesFilter);
            if (string.IsNullOrEmpty(normalMapFile))
            {
                return;
            }
            string alphaFile = EditorUtility.OpenFilePanelWithFilters("Select alpha texture", string.Empty, ImageFilesFilter);
            if (string.IsNullOrEmpty(alphaFile))
            {
                return;
            }
            try
            {
                Texture2D normalMapTexture = new Texture2D(1, 1);
                normalMapTexture.LoadImage(File.ReadAllBytes(normalMapFile));
                Texture2D alphaTexture = new Texture2D(1, 1);
                alphaTexture.LoadImage(File.ReadAllBytes(alphaFile));
                if (normalMapTexture.width != alphaTexture.width || normalMapTexture.height != alphaTexture.height)
                {
                    throw new InvalidOperationException("Normal map and alpha image must be same size");
                }
                Texture2D final = new Texture2D(normalMapTexture.width, normalMapTexture.height, TextureFormat.ARGB32, false, false);
                string combinedFile = EditorUtility.SaveFilePanel("Save final texture", string.Empty, "Combined.png", "png");
                if (string.IsNullOrEmpty(combinedFile))
                {
                    return;
                }

                Color32[] normalMapPixels = normalMapTexture.GetPixels32();
                Color32[] alphaPixels = alphaTexture.GetPixels32();
                Color32[] finalPixels = new Color32[normalMapPixels.Length];
                for (int i = 0; i < normalMapPixels.Length; i++)
                {
                    Vector3 normal = new Vector3(normalMapPixels[i].r / 255.0f, normalMapPixels[i].g / 255.0f, normalMapPixels[i].b / 255.0f).normalized;
                    finalPixels[i] = new Color32((byte)(normal.x * 255.0f), (byte)(normal.y * 255.0f), (byte)(normal.z * 255.0f), alphaPixels[i].a);
                }
                final.SetPixels32(finalPixels);
                final.Apply();
                if (System.IO.Path.GetExtension(combinedFile).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllBytes(combinedFile, final.EncodeToPNG());
                }
                else
                {
                    File.WriteAllBytes(combinedFile, final.EncodeToJPG(92));
                }
                EditorUtility.DisplayDialog("Success", "Combined texture saved at " + combinedFile, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
        }
    }
}
