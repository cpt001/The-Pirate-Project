using PlaceholderSoftware.WetStuff.Timeline.Mixers;
using UnityEngine.Playables;

namespace PlaceholderSoftware.WetStuff.Timeline.Settings
{
    public class DecalSettingsMixer
        : TemplatedTimeline<DecalSettingsDataContainer, DecalSettingsClip, DecalSettingsMixer, DecalSettingsTrack, DecalSettingsDataContainer.DecalSettingsData, WetDecal>.BaseMixer
    {
        #region Nested Types

        private class DecalLayerDataMixer
            : BaseMixer<DecalSettingsDataContainer.DecalLayerData>
        {
            private readonly BoolMixer _enabledMixer = new BoolMixer();

            private readonly DecalLayerChannelDataMixer _ch1 = new DecalLayerChannelDataMixer();
            private readonly DecalLayerChannelDataMixer _ch2 = new DecalLayerChannelDataMixer();
            private readonly DecalLayerChannelDataMixer _ch3 = new DecalLayerChannelDataMixer();
            private readonly DecalLayerChannelDataMixer _ch4 = new DecalLayerChannelDataMixer();

            public override void Start()
            {
                base.Start();

                _enabledMixer.Start();
                _ch1.Start();
                _ch2.Start();
                _ch3.Start();
                _ch4.Start();
            }

            protected override void MixImpl(float weight, DecalSettingsDataContainer.DecalLayerData data)
            {
                _ch1.Mix(weight, data.Channel1);
                _ch2.Mix(weight, data.Channel2);
                _ch3.Mix(weight, data.Channel3);
                _ch4.Mix(weight, data.Channel4);
            }

            protected override DecalSettingsDataContainer.DecalLayerData GetResult(DecalSettingsDataContainer.DecalLayerData defaultValue)
            {
                return new DecalSettingsDataContainer.DecalLayerData(
                    _ch1.Result(defaultValue.Channel1),
                    _ch2.Result(defaultValue.Channel2),
                    _ch3.Result(defaultValue.Channel3),
                    _ch4.Result(defaultValue.Channel4)
                );
            }
        }

        private class DecalLayerChannelDataMixer
            : BaseMixer<DecalSettingsDataContainer.DecalLayerChannelData>
        {
            private readonly SingleMixer _inputRangeSoftnessMixer = new SingleMixer();
            private readonly SingleMixer _inputRangeThresholdMixer = new SingleMixer();
            private readonly Vector2Mixer _outputRangeMixer = new Vector2Mixer();

            public override void Start()
            {
                base.Start();

                _inputRangeThresholdMixer.Start();
                _inputRangeSoftnessMixer.Start();
                _outputRangeMixer.Start();
            }

            protected override void MixImpl(float weight, DecalSettingsDataContainer.DecalLayerChannelData data)
            {
                _inputRangeThresholdMixer.Mix(weight, data.InputRangeThreshold);
                _inputRangeSoftnessMixer.Mix(weight, data.InputRangeSoftness);

                _outputRangeMixer.Mix(weight, data.OutputRange);
            }

            protected override DecalSettingsDataContainer.DecalLayerChannelData GetResult(DecalSettingsDataContainer.DecalLayerChannelData defaultValue)
            {
                return new DecalSettingsDataContainer.DecalLayerChannelData(
                    _inputRangeThresholdMixer.Result(defaultValue.InputRangeThreshold),
                    _inputRangeSoftnessMixer.Result(defaultValue.InputRangeSoftness),
                    _outputRangeMixer.Result(defaultValue.OutputRange)
                );
            }
        }

        #endregion

        private readonly SingleMixer _saturation = new SingleMixer();
        private readonly DecalLayerDataMixer _x = new DecalLayerDataMixer();
        private readonly DecalLayerDataMixer _y = new DecalLayerDataMixer();
        private readonly DecalLayerDataMixer _z = new DecalLayerDataMixer();

        protected override DecalSettingsDataContainer.DecalSettingsData GetState(WetDecal trackBinding)
        {
            return trackBinding.Settings.Get();
        }

        protected override void ApplyState(DecalSettingsDataContainer.DecalSettingsData intermediate, WetDecal trackBinding)
        {
            trackBinding.Settings.Apply(intermediate);
        }

        protected override DecalSettingsDataContainer.DecalSettingsData Mix(Playable playable, FrameData info, WetDecal trackBinding)
        {
            var inputCount = playable.GetInputCount();

            // Shortcut common case where nothing is being mixed and we're simply outputting a known value
            if (inputCount == 1)
                return ((ScriptPlayable<DecalSettingsDataContainer>)playable.GetInput(0)).GetBehaviour().Data;

            // Reset mixers ready for new data
            _saturation.Start();
            _x.Start();
            _y.Start();
            _z.Start();

            // Feed all the data to the mixers
            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<DecalSettingsDataContainer>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                _saturation.Mix(inputWeight, input.Data.Saturation);
                _x.Mix(inputWeight, input.Data.XLayer);
                _y.Mix(inputWeight, input.Data.YLayer);
                _z.Mix(inputWeight, input.Data.ZLayer);
            }

            // Extract result from mixers
            return new DecalSettingsDataContainer.DecalSettingsData(
                _saturation.Result(Default.Saturation),
                _x.Result(Default.XLayer),
                _y.Result(Default.YLayer),
                _z.Result(Default.ZLayer)
            );
        }
    }
}