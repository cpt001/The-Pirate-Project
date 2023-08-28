using System;
using System.ComponentModel;
using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline.Saturation
{
    [Serializable]
    [DisplayName("Saturation")]
    internal class Clip
        : TemplatedTimeline<Data, Clip, Mixer, Track, float, WetDecal>.BaseClip
    {
        public override ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }
    }
}