using System.IO;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    public class RenderSettings
#if !NCRUNCH
        : ScriptableObject
#endif
    {
        private const string SettingsFileResourceName = "RenderSettings";
        public static readonly string SettingsFilePath = Path.Combine(WetDecalsRootPath.BaseResourcePath, SettingsFileResourceName + ".asset");
#pragma warning disable 649 //Justifiction: assigned by Unity serialization
        [SerializeField] private bool _disableInstancing;
        [SerializeField] private bool _disableStencil;
        [SerializeField] private bool _disableNormalSmoothing;
#pragma warning restore 649

        public bool EnableInstancing
        {
            get
            {
                var mac = Application.platform == RuntimePlatform.OSXEditor ||
                          Application.platform == RuntimePlatform.OSXPlayer;
                
                return !mac && !_disableInstancing;
            }
            set { _disableInstancing = !value; }
        }

        public bool EnableStencil
        {
            get { return !_disableStencil; }
            set { _disableStencil = !value; }
        }

        public bool EnableNormalSmoothing
        {
            get { return !_disableNormalSmoothing; }
            set { _disableNormalSmoothing = !value; }
        }

        private static RenderSettings _instance;

        [NotNull]
        public static RenderSettings Instance
        {
            get { return _instance ?? (_instance = Load()); }
        }

        private static RenderSettings Load()
        {
#if NCRUNCH
            return new DebugSettings();
#else
            return Resources.Load<RenderSettings>(SettingsFileResourceName) ?? CreateInstance<RenderSettings>();
#endif
        }

        public static void Preload()
        {
            if (_instance == null)
                _instance = Load();
        }
    }
}