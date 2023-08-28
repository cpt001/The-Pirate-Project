using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline.Settings
{
    [TrackColor(0.047f, 1, 0.17f), TrackClipType(typeof(DecalSettingsClip)), TrackBindingType(typeof(WetDecal))]
    public class DecalSettingsTrack
        : TemplatedTimeline<DecalSettingsDataContainer, DecalSettingsClip, DecalSettingsMixer, DecalSettingsTrack, DecalSettingsDataContainer.DecalSettingsData, WetDecal>.BaseTrack
    {
    }
}