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
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Profile to contain all weather maker resource references. Anything you want in the build needs to go in here, or needs to be in a Resources folder.
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerResourceContainerScript", menuName = "WeatherMaker/Resource Container", order = 998)]
    public class WeatherMakerResourceContainerScript : ScriptableObject
    {
        private readonly Dictionary<string, UnityEngine.Object> lookup = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);

        private void OnEnable()
        {
            Build();
        }

        private void OnDisable()
        {
            lookup.Clear();
        }

        private void Build<T>(List<T> list) where T : UnityEngine.Object
        {
            if (list != null)
            {
                foreach (T obj in list)
                {
                    if (obj != null && obj.name != null)
                    {
                        lookup[obj.name] = obj;
                    }
                }
            }
        }

        /// <summary>
        /// Build lookup table
        /// </summary>
        private void Build()
        {
            lookup.Clear();
            Build(ProfilesClouds);
            Build(ProfilesFog);
            Build(ProfilesOther);
            Build(ProfilesPerformance);
            Build(ProfilesPrecipitation);
            Build(ProfilesSky);
            Build(ProfilesSound);
            Build(ProfilesWater);
            Build(ProfilesWind);
            Build(ProfilesWeather);
            Build(Textures2D);
            Build(Textures3D);
            Build(AudioClips);
            Build(Shaders);
            Build(ComputeShaders);
        }

        /// <summary>
        /// Attempt to get a resource out of the container
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="name">Name of the resource</param>
        /// <param name="value">Received value, or null if not found</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGetValue<T>(string name, out T value) where T : UnityEngine.Object
        {
            if (lookup.Count == 0)
            {
                Build();
            }

            UnityEngine.Object result;
            if (lookup.TryGetValue(name, out result))
            {
                value = result as T;
                return (value != null);
            }

            // this is the only place Resources.Load is allowed in all weather maker code
            value = Resources.Load<T>(name);
            if (value == null)
            {
                Debug.LogError("Unable to load resource with name " + name + ", have you added it to the 'ResourceContainer' property of WeatherMakerScript?");
                return false;
            }
            return true;
        }

        /// <summary>
        /// All Weather Maker weather scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Weather")]
        [Tooltip("All Weather Maker weather scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesWeather = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker cloud scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Clouds")]
        [Tooltip("All Weather Maker cloud scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesClouds = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker sky scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Sky")]
        [Tooltip("All Weather Maker sky scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesSky = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker performance scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Performance")]
        [Tooltip("All Weather Maker performance scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesPerformance = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker precipitation scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Precipitation")]
        [Tooltip("All Weather Maker precipitation scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesPrecipitation = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker fog scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Fog")]
        [Tooltip("All Weather Maker other scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesFog = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker sound scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Sound")]
        [Tooltip("All Weather Maker sound scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesSound = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker water scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Water")]
        [Tooltip("All Weather Maker other scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesWater = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker wind scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Wind")]
        [Tooltip("All Weather Maker wind scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesWind = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker misc scriptable object profiles for your game. These will be part of the final build.
        /// </summary>
        [Header("Profiles - Other")]
        [Tooltip("All Weather Maker other scriptable object profiles for your game. These will be part of the final build.")]
        public List<ScriptableObject> ProfilesOther = new List<ScriptableObject>();

        /// <summary>
        /// All Weather Maker 2D textures for your game. These will be part of the final build.
        /// </summary>
        [Header("2D Textures")]
        [Tooltip("All Weather Maker 3D textures for your game. These will be part of the final build.")]
        public List<Texture2D> Textures2D = new List<Texture2D>();

        /// <summary>
        /// All Weather Maker 3D textures for your game. These will be part of the final build.
        /// </summary>
        [Header("3D Textures")]
        [Tooltip("All Weather Maker 3D textures for your game. These will be part of the final build.")]
        public List<Texture3D> Textures3D = new List<Texture3D>();

        /// <summary>
        /// All Weather Maker audio clips for your game. These will be part of the final build.
        /// </summary>
        [Header("Audio Clips")]
        [Tooltip("All Weather Maker audio clips for your game. These will be part of the final build.")]
        public List<AudioClip> AudioClips = new List<AudioClip>();

        /// <summary>
        /// All Weather Maker shaders for your game. These will be part of the final build.
        /// </summary>
        [Header("Shaders")]
        [Tooltip("All Weather Maker shaders for your game. These will be part of the final build.")]
        public List<Shader> Shaders = new List<Shader>();

        /// <summary>
        /// All Weather Maker compute shaders for your game. These will be part of the final build.
        /// </summary>
        [Header("Shaders")]
        [Tooltip("All Weather Maker compute shaders for your game. These will be part of the final build.")]
        public List<ComputeShader> ComputeShaders = new List<ComputeShader>();
    }
}
