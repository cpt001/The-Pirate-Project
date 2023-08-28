using PlaceholderSoftware.WetStuff.Components;
using PlaceholderSoftware.WetStuff.Weather;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(DripLine))]
    [CanEditMultipleObjects]
    public class DripLineEditor
        : BaseOkSectionEditor
    {
        public DripLineEditor()
            : base(new ReferencesSection { Expanded = true }, new AutoconfigureParticlesSection { Expanded = true }, new AutoconfigureWetSplatterSection { Expanded = true }, new OtherSettings { Expanded = true })
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var to in serializedObject.targetObjects)
            {
                var t = (DripLine)to;
                if (t.GetComponent<ParticleWetSplatterTemplate>() == null)
                {
                    EditorGUILayout.HelpBox("Add one or more `Particle Wet Splatter Template` components to define the decals placed by this splatter component", MessageType.Warning);
                    break;
                }
            }
        }

        private class ReferencesSection
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            private bool _extentsOk
            {
                get
                {
                    if (_start == null || _end == null)
                        return false;

                    foreach (var o in Target.targetObjects)
                    {
                        var item = (DripLine)o;
                        if (item.StartPoint == null || item.EndPoint == null)
                            return false;
                    }

                    return true;
                }
            }

            private bool _sourceOk
            {
                get
                {
                    if (_source == null)
                        return false;

                    foreach (var o in Target.targetObjects)
                    {
                        var item = (DripLine)o;
                        if (item.WetnessSource == null)
                            return false;
                    }

                    return true;
                }
            }

            public bool Ok
            {
                get { return _extentsOk && _sourceOk; }
            }

            public override bool Expanded
            {
                get
                {
                    if (!Ok)
                        return true;
                    else
                        return base.Expanded;
                }
                set
                {
                    base.Expanded = value;
                }
            }

            private readonly GUIContent _nullTitle = new GUIContent("References (Requires Attention)");
            private readonly GUIContent _title = new GUIContent("References");
            protected override GUIContent Title { get { return !Ok ? _nullTitle : _title; } }

            private SerializedProperty _source;
            private SerializedProperty _start;
            private SerializedProperty _end;

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _source = target.FindProperty("WetnessSource");
                _start = target.FindProperty("StartPoint");
                _end = target.FindProperty("EndPoint");
            }

            protected override void DrawContent()
            {
                if (!_sourceOk)
                    EditorGUILayout.HelpBox("Choose an environmental wetness provider component", MessageType.Warning);
                EditorGUILayout.PropertyField(_source);

                if (!_extentsOk)
                    EditorGUILayout.HelpBox("Choose start and end point for the line from which particles will spawn", MessageType.Warning);
                EditorGUILayout.PropertyField(_start);
                EditorGUILayout.PropertyField(_end);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_endpointEditorSectionExpanded");
            }
        }

        private class AutoconfigureParticlesSection
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok
            {
                get { return true; }
            }

            private readonly GUIContent _title = new GUIContent("Configure Particle System");
            protected override GUIContent Title { get { return _title; } }

            protected override void DrawContent()
            {
                if (GUILayout.Button(new GUIContent("Autoconfigure Particle System", "Overwrite particle system settings with dripping effect")))
                {
                    for (var i = 0; i < Target.targetObjects.Length; i++)
                    {
                        var obj = (DripLine)Target.targetObjects[i];
                        AutoConfigureParticles(obj.GetComponent<ParticleSystem>(), obj.StartPoint, obj.EndPoint);
                    }
                }
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_autoconfigureEditorSectionExpanded");
            }

            private void AutoConfigureParticles([NotNull] ParticleSystem particles, [NotNull] Transform start, [NotNull] Transform end)
            {
                var main = particles.main;
                main.loop = true;
                main.prewarm = false;
                main.startDelay = 0;
                main.startLifetime = 2;
                main.startSpeed = 0;
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                main.startSize3D = false;
                main.startRotation3D=false;
                main.startRotation = 0;
                main.startColor = Color.white;
                main.gravityModifier = 0.9f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.simulationSpeed = 1;
                main.playOnAwake = true;
                main.maxParticles = 100;

                //Setup particle emission rates to zero. They'll be modulated according to the wetness value at runtime
                var emission = particles.emission;
                emission.enabled = true;
                emission.rateOverTimeMultiplier = 0;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(0, 1);
                emission.rateOverDistanceMultiplier = 0;
                emission.rateOverDistance = 0;

                //Calculate transform required to connect endpoints
                Vector3 pos;
                Vector3 rot;
                Vector3 scale;
                DripLine.CalculateLineTransform(start.position, end.position, out pos, out rot, out scale);

                //Setup particle emitter shape (line between the two points)
                var shape = particles.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
                shape.radius = 1;
                shape.position = pos;
                shape.rotation = rot;
                shape.scale = scale;
                shape.alignToDirection = false;
                shape.randomDirectionAmount = 0;
                shape.randomPositionAmount = 0;

                //Disable this
                var velocityOverLifetime = particles.velocityOverLifetime;
                velocityOverLifetime.enabled = false;

                //Simulate a "terminal velocity" for the rain particle (roughly 8-12m/s)
                var limitVelocityOverLifetime = particles.limitVelocityOverLifetime;
                limitVelocityOverLifetime.enabled = true;
                limitVelocityOverLifetime.drag = 0;
                limitVelocityOverLifetime.space = ParticleSystemSimulationSpace.World;
                limitVelocityOverLifetime.limit = 8;
                limitVelocityOverLifetime.dampen = 0.5f;

                //Inherit velocity if edge is moving
                var inheritVelocity = particles.inheritVelocity;
                inheritVelocity.enabled = true;
                inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
                inheritVelocity.curveMultiplier = 1;

                //Disable more things we don't need
                var forceOverLifetime = particles.forceOverLifetime;
                forceOverLifetime.enabled = false;

                //Tint raindrops slightly grey and make them semi transparent.
                //Start them 100% transparent for a very short duration so that it's not obvious when they spawn inside something
                var colorOverLifetime = particles.colorOverLifetime;
                colorOverLifetime.enabled = true;
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Gradient {
                    mode = GradientMode.Blend,
                    alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.2f, 0.1f) },
                    colorKeys = new GradientColorKey[] { new GradientColorKey(new Color(0.7f, 0.7f, 0.75f), 0) }
                });

                var colorBySpeed = particles.colorBySpeed;
                colorBySpeed.enabled = false;

                var sizeOverLifetime = particles.sizeOverLifetime;
                sizeOverLifetime.enabled = false;

                //Stretch raindrop vertically as it falls faster
                var sizeBySpeed = particles.sizeBySpeed;
                sizeBySpeed.enabled = true;
                sizeBySpeed.range = new Vector2(0, 8);
                sizeBySpeed.separateAxes = true;
                sizeBySpeed.x = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 0.25f)));
                sizeBySpeed.y = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 1)));
                sizeBySpeed.z = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 0.25f)));
                sizeBySpeed.xMultiplier = 1;
                sizeBySpeed.yMultiplier = 1;
                sizeBySpeed.zMultiplier = 1;
                
                var rotationOverLifeTime = particles.rotationOverLifetime;
                rotationOverLifeTime.enabled = false;

                var rotationBySpeed = particles.rotationBySpeed;
                rotationBySpeed.enabled = false;

                //Allow wind to affect particles
                var externalForces = particles.externalForces;
                externalForces.enabled = true;
                externalForces.multiplier = 1f;

                //Very slight position noise to break up regularity of landing positions
                var noise = particles.noise;
                noise.enabled = true;
                noise.separateAxes = true;
                noise.strengthX = 0.02f;
                noise.strengthY = 0;
                noise.strengthZ = 0.02f;
                noise.frequency = 0.5f;
                noise.scrollSpeed = 0.1f;
                noise.damping = true;
                noise.octaveCount = 1;
                noise.quality = ParticleSystemNoiseQuality.Low;
                noise.remapEnabled = false;
                noise.rotationAmount = 0;
                noise.sizeAmount = 0;
                noise.positionAmount = 1;

                //Enable collisions with world
                var collision = particles.collision;
                collision.enabled = true;
                collision.type = ParticleSystemCollisionType.World;
                collision.mode = ParticleSystemCollisionMode.Collision3D;
                collision.lifetimeLoss = 1;
                collision.quality = ParticleSystemCollisionQuality.High;
                collision.colliderForce = 0;
                collision.sendCollisionMessages = true;

                var trigger = particles.trigger;
                trigger.enabled = false;

                var subEmitters = particles.subEmitters;
                subEmitters.enabled = false;

                var textureSheetAnimation = particles.textureSheetAnimation;
                textureSheetAnimation.enabled = false;

                var lights = particles.lights;
                lights.enabled = false;

                var trails = particles.trails;
                trails.enabled = false;

                var customData = particles.customData;
                customData.enabled = false;

                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.enabled = true;
                renderer.material = Resources.Load<Material>("DripRaindrop");
                renderer.maxParticleSize = 0.1f;
                renderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
            }
        }

        private class AutoconfigureWetSplatterSection
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok
            {
                get { return true; }
            }

            private readonly GUIContent _title = new GUIContent("Configure Particle Wet Splatter");
            protected override GUIContent Title { get { return _title; } }

            protected override void DrawContent()
            {
                if (GUILayout.Button(new GUIContent("Autoconfigure Particle Wet Splatter", "Overwrite Particle Wet Splatter settings with dripping effect")))
                {
                    for (var i = 0; i < Target.targetObjects.Length; i++)
                    {
                        var obj = (DripLine)Target.targetObjects[i];
                        AutoConfigureSplatter(obj.GetComponent<ParticleWetSplatter>(), obj.StartPoint, obj.EndPoint);
                    }
                }
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_autoconfigureSplatterEditorSectionExpanded");
            }

            private static void AutoConfigureSplatter([NotNull] ParticleWetSplatter splatter, [NotNull] Transform start, [NotNull] Transform end)
            {
                splatter.Core.DecalSize = new Vector3(0.1f, 0.1f, 0.1f);
                splatter.Core.VerticalOffset = -0.025f;
                splatter.Core.DecalChance = 0.5f;
                splatter.Core.Saturation = 0.75f;

                splatter.Limit.Enabled = true;
                splatter.Limit.MaxDecals = (int)Mathf.Ceil(Vector3.Distance(end.position, start.position) * 35);
                splatter.Limit.DecalChance = new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 0.25f));

                splatter.Lifetime.Enabled = true;
                splatter.Lifetime.MaxLifetime = 100;
                splatter.Lifetime.MinLifetime = 50;
                splatter.Lifetime.Saturation = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));

                splatter.Recycling.Enabled = true;
                splatter.Recycling.MaxAcceleratedAgeing = 5;
                splatter.Recycling.StealThreshold = splatter.Core.DecalSize.x * splatter.Core.DecalSize.z * 0.5f;

                splatter.RandomizeSize.Enabled = true;
                splatter.RandomizeSize.MinInflation = 0.75f;
                splatter.RandomizeSize.MaxInflation = 1.25f;

                splatter.ImpactVelocity.Enabled = false;
            }
        }

        private class OtherSettings
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }

            private readonly GUIContent _title = new GUIContent("Other Settings");
            protected override GUIContent Title { get { return _title; } }

            private SerializedProperty _liveTransform;
            private SerializedProperty _rateMultiplier;
            private SerializedProperty _rainTimeScale;
            private SerializedProperty _dryTimeScale;

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _liveTransform = target.FindProperty("LiveUpdateTransform");
                _rateMultiplier = target.FindProperty("EmissionRateMultiplier");
                _rainTimeScale = target.FindProperty("RainTimeScale");
                _dryTimeScale = target.FindProperty("DryTimeScale");
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_liveTransform);
                EditorGUILayout.PropertyField(_rateMultiplier);
                EditorGUILayout.PropertyField(_rainTimeScale);
                EditorGUILayout.PropertyField(_dryTimeScale);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_otherSettingsEditorSectionExpanded");
            }
        }
    }
}
