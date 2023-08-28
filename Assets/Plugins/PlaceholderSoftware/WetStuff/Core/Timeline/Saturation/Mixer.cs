using UnityEngine.Playables;

namespace PlaceholderSoftware.WetStuff.Timeline.Saturation
{
    internal class Mixer
        : TemplatedTimeline<Data, Clip, Mixer, Track, float, WetDecal>.BaseMixer
    {
        protected override float GetState(WetDecal trackBinding)
        {
            return trackBinding.Settings.Saturation;
        }

        protected override void ApplyState(float intermediate, WetDecal trackBinding)
        {
            trackBinding.Settings.Saturation = intermediate;
        }

        protected override float Mix(Playable playable, FrameData info, WetDecal trackBinding)
        {
            var inputCount = playable.GetInputCount();

            var totalWeight = 0f;
            var blendedSaturation = 0f;

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<Data>) playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                blendedSaturation += input.Saturation * inputWeight;
                totalWeight += inputWeight;
            }

            return blendedSaturation + Default * (1f - totalWeight);
        }
    }
}