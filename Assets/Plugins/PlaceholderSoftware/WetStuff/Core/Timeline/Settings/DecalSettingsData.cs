using System;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Timeline.Settings
{
    [Serializable]
    public class DecalSettingsDataContainer
        : TemplatedTimeline<DecalSettingsDataContainer, DecalSettingsClip, DecalSettingsMixer, DecalSettingsTrack, DecalSettingsDataContainer.DecalSettingsData, WetDecal>.BaseData
    {
        #region Nested Types

        [Serializable]
        public struct DecalSettingsData
        {
            [SerializeField, UsedImplicitly, Range(0, 1)]
            public float Saturation;

            [SerializeField, UsedImplicitly] public DecalLayerData XLayer;
            [SerializeField, UsedImplicitly] public DecalLayerData YLayer;
            [SerializeField, UsedImplicitly] public DecalLayerData ZLayer;

            public DecalSettingsData(float saturation, DecalLayerData xLayer, DecalLayerData yLayer, DecalLayerData zLayer)
            {
                Saturation = saturation;
                XLayer = xLayer;
                YLayer = yLayer;
                ZLayer = zLayer;
            }
        }

        [Serializable]
        public struct DecalLayerData
        {
            [SerializeField, UsedImplicitly] public DecalLayerChannelData Channel1;
            [SerializeField, UsedImplicitly] public DecalLayerChannelData Channel2;
            [SerializeField, UsedImplicitly] public DecalLayerChannelData Channel3;
            [SerializeField, UsedImplicitly] public DecalLayerChannelData Channel4;

#pragma warning disable 0414    // Value assigned but never used
            [SerializeField, UsedImplicitly] private bool _editorSectionFoldout;
#pragma warning restore 0414

            public DecalLayerData(DecalLayerChannelData ch1, DecalLayerChannelData ch2, DecalLayerChannelData ch3, DecalLayerChannelData ch4)
            {
                Channel1 = ch1;
                Channel2 = ch2;
                Channel3 = ch3;
                Channel4 = ch4;

                _editorSectionFoldout = false;
            }
        }

        [Serializable]
        public struct DecalLayerChannelData
        {
            [SerializeField, UsedImplicitly, Range(0, 1)]
            public float InputRangeThreshold;

            [SerializeField, UsedImplicitly, Range(0, 1)]
            public float InputRangeSoftness;

            [SerializeField, UsedImplicitly, MinMax(0, 1)]
            public Vector2 OutputRange;

            public DecalLayerChannelData(float inputRangeThreshold, float inputRangeSoftness, Vector2 outputRange)
            {
                InputRangeThreshold = inputRangeThreshold;
                InputRangeSoftness = inputRangeSoftness;

                OutputRange = outputRange;
            }
        }

        #endregion

        [SerializeField, UsedImplicitly] public DecalSettingsData Data;
    }
}