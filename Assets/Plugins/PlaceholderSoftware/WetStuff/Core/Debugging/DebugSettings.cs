using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Debugging
{
    public class DebugSettings
#if !NCRUNCH
        : ScriptableObject
#endif
    {
        private const string SettingsFileResourceName = "DebugSettings";
        public static readonly string SettingsFilePath = Path.Combine(WetDecalsRootPath.BaseResourcePath, SettingsFileResourceName + ".asset");

#if NCRUNCH
        private const LogLevel DefaultLevel = LogLevel.Trace;
#else
        private const LogLevel DefaultLevel = LogLevel.Info;
#endif

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local (Justification: Breaks unity serialization)
        private List<LogLevel> _levels;

        private static DebugSettings _instance;

        [NotNull]
        public static DebugSettings Instance
        {
            get { return _instance ?? (_instance = Load()); }
        }

        public DebugSettings()
        {
            var categories = ((LogCategory[]) Enum.GetValues(typeof(LogCategory)))
               .Select(c => (int) c)
               .Max();

            _levels = new List<LogLevel>(categories + 1);
        }

        public LogLevel GetLevel(int category)
        {
            if (_levels.Count > category)
                return _levels[category];

            return DefaultLevel;
        }

        public void SetLevel(int category, LogLevel level)
        {
            if (_levels.Count <= category)
            {
                for (var i = _levels.Count; i <= category; i++)
                    _levels.Add(DefaultLevel);
            }

            _levels[category] = level;
        }

        private static DebugSettings Load()
        {
#if NCRUNCH
            return new DebugSettings();
#else
            return Resources.Load<DebugSettings>(SettingsFileResourceName) ?? CreateInstance<DebugSettings>();
#endif
        }

        public static void Preload()
        {
            if (_instance == null)
                _instance = Load();
        }
    }
}