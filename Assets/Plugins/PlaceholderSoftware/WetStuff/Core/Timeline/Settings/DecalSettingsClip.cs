using System;
using System.ComponentModel;
using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline.Settings
{
    [Serializable, DisplayName("Decal Settings")]
    public class DecalSettingsClip
        : TemplatedTimeline<DecalSettingsDataContainer, DecalSettingsClip, DecalSettingsMixer, DecalSettingsTrack, DecalSettingsDataContainer.DecalSettingsData, WetDecal>.BaseClip
    {
        public override ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }
    }
}