using System;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Timeline.Saturation
{
    [Serializable]
    internal class Data
        : TemplatedTimeline<Data, Clip, Mixer, Track, float, WetDecal>.BaseData
    {
        [SerializeField, UsedImplicitly] public float Saturation;
    }
}