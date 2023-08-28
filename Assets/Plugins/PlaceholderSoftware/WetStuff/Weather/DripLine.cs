using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather
{
    /// <summary>
    ///     Controls a particle system to emit based on the rate of a WetnessSource (e.g. weather).
    /// </summary>
    [RequireComponent(typeof(ParticleSystem)), RequireComponent(typeof(ParticleWetSplatter))]
    [HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/DripLine/")]
    public class DripLine
        : MonoBehaviour
    {
        // ReSharper disable NotAccessedField.Local (Justification: Used by editor through serialized properties)
        [SerializeField] private bool _endpointEditorSectionExpanded;
        [SerializeField] private bool _autoconfigureEditorSectionExpanded;
        [SerializeField] private bool _autoconfigureSplatterEditorSectionExpanded;
        [SerializeField] private bool _otherSettingsEditorSectionExpanded;
        // ReSharper restore NotAccessedField.Local

        [SerializeField] public BaseExternalWetnessSource WetnessSource;
        [SerializeField] public Transform StartPoint;
        [SerializeField] public Transform EndPoint;
        [SerializeField, Range(0, 10)] public float EmissionRateMultiplier = 1;
        [SerializeField, Range(0, 3)] public float RainTimeScale = 0.1f;
        [SerializeField, Range(0, 3)] public float DryTimeScale = 0.01f;

        [SerializeField] public bool LiveUpdateTransform;
        private Vector3 _lastStartPosition;
        private Vector3 _lastEndPosition;

        private float _emissionRateFromLength;
        private float _intensity;
        private ParticleSystem _particles;
        
        public void Awake()
        {
            _particles = GetComponent<ParticleSystem>();

            UpdatePosition(true);
        }

        public void Update()
        {
            var emission = _particles.emission;
            var lerpMultiplier = _intensity < WetnessSource.RainIntensity ? RainTimeScale : DryTimeScale;
            _intensity = Mathf.Lerp(_intensity, Mathf.Clamp01(WetnessSource.RainIntensity), Time.deltaTime * lerpMultiplier);
            emission.rateOverTimeMultiplier = EmissionRateMultiplier * _emissionRateFromLength * _intensity;

            if (LiveUpdateTransform)
                UpdatePosition();
        }

        private void UpdatePosition(bool force = false)
        {
            const float epsilon = float.Epsilon;

            var a = StartPoint.position;
            var b = EndPoint.position;

            // Early exit if neither point has moved (using manhattan distance is cheap to calculate)
            // Don't do the early exit check if an update has been forced
            if (!force)
            {
                var aDelta = _lastStartPosition - a;
                var bDelta = _lastEndPosition - b;
                if (aDelta.x < epsilon && aDelta.y < epsilon && aDelta.z < epsilon && bDelta.x < epsilon && bDelta.y < epsilon && bDelta.z < epsilon)
                    return;
            }

            // Calculate the required transform to connect the two points
            var mat = transform.worldToLocalMatrix;
            Vector3 p, r, s;
            CalculateLineTransform(mat.MultiplyPoint(a), mat.MultiplyPoint(b), out p, out r, out s);

            // Update the particle system spawn position
            var shape = _particles.shape;
            shape.position = p;
            shape.rotation = r;
            shape.scale = s;

            // Recalculate emission rate
            _emissionRateFromLength = Vector3.Distance(a, b) / 2 * 10;

            // Cache positions so that we can detect if they've moved in the future
            _lastStartPosition = a;
            _lastEndPosition = b;
        }

        /// <summary>
        ///     Calculate the transform required to connect two points with a line
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void CalculateLineTransform(Vector3 a, Vector3 b, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            // Center at the middle point
            position = a * 0.5f + b * 0.5f;

            // Length if the distance between the points
            var difference = b - a;
            var length = difference.magnitude;
            scale = new Vector3(length * 0.5f, 0, 0);

            // Rotate to connect the points
            var normalized = difference / length;
            rotation = (Quaternion.LookRotation(normalized, Vector3.up) * Quaternion.AngleAxis(90, Vector3.up)).eulerAngles;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            var sn = StartPoint == null;
            var en = EndPoint == null;
            if (sn ^ en)
            {
                // Draw spheres at endpoints if only one is not null
                Gizmos.color = Color.red;
                if (!sn) DrawWireTarget(StartPoint.position, 0.15f);
                if (!en) DrawWireTarget(EndPoint.position, 0.15f);
            }
            else if (!sn)
            {
                // Don't show the gizmos when not selected if the configuration is valid
                if (UnityEditor.Selection.activeGameObject != gameObject)
                    return;

                // Draw spheres at both endpoints and line between then
                Gizmos.color = new Color(0, 1, 0, 0.75f);
                DrawWireTarget(StartPoint.position, 0.05f);
                DrawWireTarget(EndPoint.position, 0.05f);
                Gizmos.DrawLine(StartPoint.position, EndPoint.position);
            }
        }

        private static void DrawWireTarget(Vector3 position, float radius, float extra = 0.0f, bool pulseExtra = false)
        {
            var lineExtent = radius + radius * (pulseExtra ? Mathf.SmoothStep(0, extra, Mathf.PingPong(Time.time, 1)) : extra);

            Gizmos.DrawWireSphere(position, radius);
            Gizmos.DrawLine(position + new Vector3(0, 0, lineExtent), position + new Vector3(0, 0, -lineExtent));
            Gizmos.DrawLine(position + new Vector3(0, lineExtent, 0), position + new Vector3(0, -lineExtent, 0));
            Gizmos.DrawLine(position + new Vector3(lineExtent, 0, 0), position + new Vector3(-lineExtent, 0, 0));
        }
#endif
    }
}