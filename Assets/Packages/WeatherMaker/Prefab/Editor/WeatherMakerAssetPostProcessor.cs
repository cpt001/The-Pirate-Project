/*
This one source file is MIT license.

Copyright 2019 Digital Ruby, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Pre-process define utility
    /// </summary>
    public class WeatherMakerPreProcessorManager : AssetPostprocessor
    {
        private static void CleanupOldFiles()
        {
            string[] found = AssetDatabase.FindAssets("WeatherMakerAuroraShader");
            foreach (string item in found)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                if (path.EndsWith("Prefab/Shaders/WeatherMakerAuroraShader.cginc", System.StringComparison.OrdinalIgnoreCase))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            try
            {
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "/WeatherMaker/Prefab/Scripts/Manager/WeatherMakerLWRPRenderFeatureScript.cs");
            }
            catch
            {
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (deletedAssets != null && deletedAssets.Length != 0)
            {
                int scriptCount = deletedAssets.Count(a => a.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
                if (scriptCount > 0)
                {
                    Debug.LogWarningFormat("{0} script{1} been deleted from the project. If you see compile errors, please edit your player settings, scripting define symbols and remove any defines that no longer exist in the project.",
                        scriptCount, (scriptCount > 1 ? "s have" : " has"));
                }
            }
            DidReloadScripts();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        [UnityEditor.InitializeOnLoadMethod]
        private static void DidReloadScripts()
        {
            // executes on first load or whenever code finishes compiling

            CleanupOldFiles();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            bool hasWeatherMaker = false;
            bool hasPostProcessV2 = false;
            bool hasPlaymaker = false;
            bool hasCts = false;
            bool hasCrest = false;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                // check for any assemblies that obviously won't have the
                // types we care about and skip them. Some of these assemblies
                // will throw a ReflectionTypeLoadException if we try to read
                // their types. One example being:
                // Microsoft.CodeAnalysis.Scripting, Version=2.6.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e3
                string assemblyName = a.GetName().Name;
                if (assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                    assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                    assemblyName.StartsWith("Mono.", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    foreach (Type type in a.GetTypes())
                    {
                        switch (type.FullName.ToLowerInvariant())
                        {
                            case "digitalruby.weathermaker.weathermakerscript":
                                hasWeatherMaker = true;
                                break;

                            case "unityengine.rendering.postprocessing.postprocesslayer":
                                hasPostProcessV2 = true;
                                break;

                            case "hutonggames.playmaker.fsmprocessor":
                                hasPlaymaker = true;
                                break;

                            case "cts.completeterrainshader":
                                hasCts = true;
                                break;

                            case "crest.oceanrenderer":
                                hasCrest = true;
                                break;
                        }
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.LogErrorFormat("Could not read types on assembly {0}", a.FullName);
                    Debug.LogException(e);
                }
            }
            WeatherMakerEditorUtility.UpdatePreProcessor(hasWeatherMaker, "WEATHER_MAKER_PRESENT");
            WeatherMakerEditorUtility.UpdatePreProcessor(hasPostProcessV2, "UNITY_POST_PROCESSING_STACK_V2");
            WeatherMakerEditorUtility.UpdatePreProcessor(hasPlaymaker, "PLAYMAKER_PRESENT");
            WeatherMakerEditorUtility.UpdatePreProcessor(hasCts, "CTS_PRESENT");
            WeatherMakerEditorUtility.UpdatePreProcessor(hasCrest, "CREST_OCEAN_PRESENT");
        }
    }
}
