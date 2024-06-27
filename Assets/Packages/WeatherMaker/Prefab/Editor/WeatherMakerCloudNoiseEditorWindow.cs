using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerCloudNoiseEditorWindow : EditorWindow
    {
        private readonly Material[] materials = new Material[4];
        private readonly RenderTexture[] renderTextures = new RenderTexture[4];

        private WeatherMakerCloudNoiseProfileGroupScript noiseProfile;
        private float frame;
        private bool animated = true;

        private void OnEnable()
        {
            for (int i = 0; i < 4; i++)
            {
                string[] assets = AssetDatabase.FindAssets("WeatherMakerCloudNoiseGeneratorMaterial" + (i + 1));
                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                materials[i] = new Material(AssetDatabase.LoadAssetAtPath<Material>(path));
                assets = AssetDatabase.FindAssets("WeatherMakerCloudNoiseRenderTexture" + (i + 1));
                path = AssetDatabase.GUIDToAssetPath(assets[0]);
                renderTextures[i] = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            }
            string lastNoiseAssetPath = EditorPrefs.GetString("WeatherMakerCloudNoiseGeneratorLastProfile");
            if (!string.IsNullOrEmpty(lastNoiseAssetPath))
            {
                noiseProfile = AssetDatabase.LoadAssetAtPath<WeatherMakerCloudNoiseProfileGroupScript>(lastNoiseAssetPath);
            }
        }

        private void Update()
        {
            if (noiseProfile != null && noiseProfile.NoiseProfiles != null && noiseProfile.NoiseProfiles.Length <= materials.Length)
            {
                for (int i = 0; i < noiseProfile.NoiseProfiles.Length; i++)
                {
                    if (noiseProfile.NoiseProfiles[i] != null && materials[i] != null)
                    {
                        noiseProfile.NoiseProfiles[i].ApplyToMaterial(materials[i]);
                        materials[i].SetFloat(WMS._CloudNoiseFrame, frame);
                        Graphics.Blit(null, renderTextures[i], materials[i], 0);
                    }
                }
                if (animated)
                {
                    frame += Time.deltaTime * 0.05f;
                }
                Repaint();
            }
        }

        private void OnGUI()
        {
            float textureHeight = Mathf.Max(position.height - 80.0f, 180.0f);
            float fieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 800.0f;
            Rect rect = new Rect(4.0f, textureHeight + 8.0f, EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
            noiseProfile = EditorGUI.ObjectField(rect, "Cloud Noise Profile", noiseProfile, typeof(WeatherMakerCloudNoiseProfileGroupScript), false) as WeatherMakerCloudNoiseProfileGroupScript;
            if (noiseProfile != null && noiseProfile.NoiseProfiles != null && noiseProfile.NoiseProfiles.Length <= materials.Length)
            {
                for (int i = 0; i < noiseProfile.NoiseProfiles.Length; i++)
                {
                    float x = 4.0f + (i * textureHeight) + (i * 4.0f);
                    float y = 4.0f;
                    Rect rect2 = new Rect(x, y, textureHeight, textureHeight);
                    //GUI.DrawTexture(rect2, renderTextures[i], ScaleMode.ScaleToFit);
                    EditorGUI.DrawTextureTransparent(rect2, renderTextures[i], ScaleMode.ScaleToFit);
                }
                EditorPrefs.SetString("WeatherMakerCloudNoiseGeneratorLastProfile", AssetDatabase.GetAssetPath(noiseProfile));
            }

            rect.x += rect.width + 20.0f;
            rect.width = 200.0f;
            if (GUI.Button(rect, "Export"))
            {
                WeatherMakerCloudNoiseGeneratorScript.GenerateFramesAnd3DTexture(noiseProfile, materials, noiseProfile.FileName);
                Repaint();
            }
            EditorGUIUtility.fieldWidth = fieldWidth;
            rect.y += EditorGUIUtility.singleLineHeight + 4.0f;
            rect.xMin = 4.0f;
            rect.width = 200.0f;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 68.0f;
            animated = EditorGUI.Toggle(rect, "Animated", animated);
            EditorGUIUtility.labelWidth = labelWidth;
        }

        [MenuItem("Window/Weather Maker/Cloud Noise Editor", false, priority = 61)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(WeatherMakerCloudNoiseEditorWindow), false, "Cloud Noise");
        }
    }
}
