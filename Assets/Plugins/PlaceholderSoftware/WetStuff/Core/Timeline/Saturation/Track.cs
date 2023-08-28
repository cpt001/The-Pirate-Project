using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline.Saturation
{
    [TrackColor(0.047f, 1, 0.17f), TrackClipType(typeof(Clip)), TrackBindingType(typeof(WetDecal))]
    internal class Track
        : TemplatedTimeline<Data, Clip, Mixer, Track, float, WetDecal>.BaseTrack
    {
    }
}