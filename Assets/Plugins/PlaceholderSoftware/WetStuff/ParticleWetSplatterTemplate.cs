using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [RequireComponent(typeof(ParticleWetSplatter)), HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/ParticleWetSplatterTemplate/")]
    public class ParticleWetSplatterTemplate
        : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField, UsedImplicitly, Range(0, 1), Tooltip("Chance of this template being used")]
        private float _probability;

        [SerializeField, UsedImplicitly]
        private DecalSettings _settings;
#pragma warning restore 0649

        public DecalSettings Settings
        {
            get { return _settings; }
        }

        public float Probability
        {
            get { return _probability; }
        }

        protected virtual void OnValidate()
        {
            if (Settings != null)
                Settings.OnChanged(true);
        }
    }
}