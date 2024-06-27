//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using UnityEngine;
using System.Collections.Generic;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Precipitation particles 3D script
    /// </summary>
    public class WeatherMakerFallingParticleScript3D : WeatherMakerFallingParticleScript
    {
        /// <summary>Whether the particles will anchor themselves to each rendered camera. If false, particles stay where you put them.</summary>
        [Header("3D Settings")]
        [Tooltip("Whether the particles will anchor themselves to each rendered camera. If false, particles stay where you put them.")]
        public bool IsFirstPerson = true;

        /// <summary>How fast particle systems should follow each camera if first person. Higher values follow faster but can result in visible snapping of particles. Set to 0 for instant follow.</summary>
        [Tooltip("How fast particle systems should follow each camera if first person. Higher values follow faster but can result in visible snapping of particles. " +
            "Set to 0 for instant follow.")]
        [Range(0.0f, 100.0f)]
        public float FirstPersonFollowSpeed = 3.0f;

        /// <summary>The height above the camera that the particles will start falling from</summary>
        [Tooltip("The height above the camera that the particles will start falling from")]
        public float Height = 25.0f;

        /// <summary>How far the particle system is ahead of the camera</summary>
        [Tooltip("How far the particle system is ahead of the camera")]
        public float ForwardOffset = -7.0f;

        /// <summary>The height above the camera that the secondary particles will start falling from</summary>
        [Tooltip("The height above the camera that the secondary particles will start falling from")]
        public float SecondaryHeight = 100.0f;

        /// <summary>How far the secondary particle system is ahead of the camera</summary>
        [Tooltip("How far the secondary particle system is ahead of the camera")]
        public float SecondaryForwardOffset = 25.0f;

        /// <summary>Forward offset for mist</summary>
        [Tooltip("Forward offset for mist")]
        public float MistForward = 0.0f;

        /// <summary>The top y value of the mist particles</summary>
        [Tooltip("The top y value of the mist particles")]
        public float MistHeight = 3.0f;

        /// <summary>Height to place particle systems if in a null zone</summary>
        [Tooltip("Height to place particle systems if in a null zone")]
        public float NullZoneHeight = 10.0f;

        /// <summary>Optional animated texture renderer, used to render things like ripples / splashes all inside a shader without need for particle collisions.</summary>
        [Tooltip("Optional animated texture renderer, used to render things like ripples / splashes all inside a shader without need for particle collisions.")]
        public Renderer AnimatedTextureRenderer;

        /// <summary>The intensity at which the animated texture (puddles) are fully visible.</summary>
        [Tooltip("The intensity at which the animated texture (puddles) are fully visible.")]
        [Range(0.01f, 1.0f)]
        public float AnimatedTextureRendererIntensityThreshold = 0.5f;

        /// <summary>How much to offset the animated texture renderer should offset from nearest hit below camera. All values should be &gt;= 0. If z is &gt; 0, the drops will rotate with the camera to keep the majority in front of the camera.</summary>
        [Tooltip("How much to offset the animated texture renderer should offset from nearest hit below camera. All values should be >= 0. If z is > 0, the drops will rotate with the camera to keep the majority in front of the camera.")]
        public Vector3 AnimatedTextureRendererPositionOffset = new Vector3(0.0f, 0.1f, 0.0f);

        /// <summary>Layer mask for collision check above player to turn off animated texture if something is hit, i.e. a roof or tree.</summary>
        [Tooltip("Layer mask for collision check above player to turn off animated texture if something is hit, i.e. a roof or tree.")]
        public LayerMask AnimatedTextureCollisionMaskAbove = 0;

        /// <summary>Layer mask for collision check below player to position animated texture, i.e. the ground.</summary>
        [Tooltip("Layer mask for collision check below player to position animated texture, i.e. the ground.")]
        public LayerMask AnimatedTextureCollisionMaskBelow = -1;

        /// <summary>ParticleSystem Near Width</summary>
        [Header("Particle System Emitters")]
        [Tooltip("ParticleSystem Near Width")]
        [Range(0.0f, 10.0f)]
        public float ParticleSystemNearWidth = 5.0f;

        /// <summary>ParticleSystem Far Width</summary>
        [Tooltip("ParticleSystem Far Width")]
        [Range(0.0f, 2000.0f)]
        public float ParticleSystemFarWidth = 70.0f;

        /// <summary>ParticleSystem Near Depth</summary>
        [Tooltip("ParticleSystem Near Depth")]
        [Range(0.0f, 100.0f)]
        public float ParticleSystemNearDepth = 0.25f;

        /// <summary>ParticleSystem Far Depth</summary>
        [Tooltip("ParticleSystem Far Depth")]
        [Range(0.0f, 500.0f)]
        public float ParticleSystemFarDepth = 50.0f;

        /// <summary>ParticleSystemSecondary Near Width</summary>
        [Tooltip("ParticleSystemSecondary Near Width")]
        [Range(0.0f, 10.0f)]
        public float ParticleSystemSecondaryNearWidth = 5.0f;

        /// <summary>ParticleSystemSecondary Far Width</summary>
        [Tooltip("ParticleSystemSecondary Far Width")]
        [Range(0.0f, 2000.0f)]
        public float ParticleSystemSecondaryFarWidth = 500.0f;

        /// <summary>ParticleSystemSecondary Near Depth</summary>
        [Tooltip("ParticleSystemSecondary Near Depth")]
        [Range(0.0f, 100.0f)]
        public float ParticleSystemSecondaryNearDepth = 0.25f;

        /// <summary>ParticleSystemSecondary Far Depth</summary>
        [Tooltip("ParticleSystemSecondary Far Depth")]
        [Range(0.0f, 500.0f)]
        public float ParticleSystemSecondaryFarDepth = 50.0f;

        /// <summary>ParticleSystemMist Near Width</summary>
        [Tooltip("ParticleSystemMist Near Width")]
        [Range(0.0f, 10.0f)]
        public float ParticleSystemMistNearWidth = 5.0f;

        /// <summary>ParticleSystemMist Far Width</summary>
        [Tooltip("ParticleSystemMist Far Width")]
        [Range(0.0f, 2000.0f)]
        public float ParticleSystemMistFarWidth = 70.0f;

        /// <summary>ParticleSystemMist Near Depth</summary>
        [Tooltip("ParticleSystemMist Near Depth")]
        [Range(0.0f, 100.0f)]
        public float ParticleSystemMistNearDepth = 0.25f;

        /// <summary>ParticleSystemMist Far Depth</summary>
        [Tooltip("ParticleSystemMist Far Depth")]
        [Range(0.0f, 500.0f)]
        public float ParticleSystemMistFarDepth = 50.0f;

        private Camera lastCamera;

        private class ParticleSystemState
        {
            public Vector3 PreviousPosition;
            public bool WasChangedFromLocalToWorldSpace;
        }

        private readonly Dictionary<Transform, Dictionary<ParticleSystem, ParticleSystemState>> states = new Dictionary<Transform, Dictionary<ParticleSystem, ParticleSystemState>>();

        private void UpdateCollisionForParticleSystem(ParticleSystem p)
        {
            if (p != null)
            {
                var c = p.collision;
                var s = p.subEmitters;
                c.enabled = CollisionEnabled;
                s.enabled = CollisionEnabled;
            }
        }

        private void CreateMeshEmitter(ParticleSystem p, float nearWidth, float farWidth, float nearDepth, float farDepth)
        {
            if (p == null || p.shape.shapeType != ParticleSystemShapeType.Mesh)
            {
                return;
            }

            Mesh emitter = new Mesh { name = "WeatherMakerFaillingParticleScript3D_Triangle" };
            emitter.vertices = new Vector3[]
            {
                new Vector3(-nearWidth, 0.0f, nearDepth),
                new Vector3(nearWidth, 0.0f, nearDepth),
                new Vector3(-farWidth, 0.0f, farDepth),
                new Vector3(farWidth, 0.0f, farDepth)
            };
            emitter.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            var s = p.shape;
            s.mesh = emitter;
            s.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        }

        private void TransformParticleSystem(ParticleSystem particles, Transform anchorTransform, float forward, float height, float rotationYModifier)
        {
            if (particles == null || !IsFirstPerson || Intensity == 0.0f)
            {
                return;
            }
            Vector3 pos = anchorTransform.position;
            Vector3 anchorForward;
            bool inNullZone = false;
            int count = Physics.OverlapSphereNonAlloc(anchorTransform.position, 0.001f, WeatherMakerScript.tempColliders);
            for (int i = 0; i < count; i++)
            {
                WeatherMakerNullZoneScript script = WeatherMakerScript.tempColliders[i].GetComponent<WeatherMakerNullZoneScript>();
                if (script != null && script.NullZoneActive && (script.CurrentMask & (int)NullZoneRenderMask.Precipitation) == 0)
                {
                    inNullZone = true;
                    break;
                }
            }

            // update particle system state for this transform
            ParticleSystemState state;
            Dictionary<ParticleSystem, ParticleSystemState> stateDict;
            if (!states.TryGetValue(anchorTransform, out stateDict))
            {
                states[anchorTransform] = stateDict = new Dictionary<ParticleSystem, ParticleSystemState>();
            }
            if (!stateDict.TryGetValue(particles, out state))
            {
                stateDict[particles] = state = new ParticleSystemState { PreviousPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue) };
            }

            // if we were forced to world space from local space, go back to local space
            if (state.WasChangedFromLocalToWorldSpace)
            {
                state.WasChangedFromLocalToWorldSpace = false;
                var main = particles.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }

            if (particles.main.simulationSpace == ParticleSystemSimulationSpace.Local && !inNullZone)
            {
                if (WeatherMakerWindScript.Instance != null && WeatherMakerWindScript.Instance.CurrentWindVelocity != Vector3.zero)
                {
                    // as wind intensity increases, move forward and height of the system more inline with the player so it blows in their face
                    anchorForward = -(WeatherMakerWindScript.Instance.CurrentWindVelocity.normalized);
                    pos += (anchorForward * Mathf.Lerp(forward, Mathf.Max(forward, 10.0f), WeatherMakerWindScript.Instance.WindZone.windMain * 4.0f));
                }
                else
                {
                    // can't move against the transform forward because it will look awful as player rotates
                    // just set on top of player directly
                    anchorForward = Vector3.zero;
                    pos.y += height;
                }
            }
            else
            {
                if (inNullZone)
                {
                    // in null zone, put the precipitation right above the player at a distance
                    forward = 0.0f;
                    float standardHeight = (particles == ParticleSystem ? Height : (particles == MistParticleSystem ? MistHeight : SecondaryHeight));
                    height = Mathf.Max(NullZoneHeight, standardHeight);

                    var main = particles.main;
                    if (main.simulationSpace == ParticleSystemSimulationSpace.Local)
                    {
                        // mark this so we can switch back to local space when the null zone is exited
                        state.WasChangedFromLocalToWorldSpace = true;

                        // must use world space, looks funny swimming under water or moving in a building and moving with the player
                        main.simulationSpace = ParticleSystemSimulationSpace.World;
                    }
                }

                // position up and forward based on parameters
                anchorForward = anchorTransform.forward;
                pos.x += anchorForward.x * forward;
                pos.y += height;
                pos.z += anchorForward.z * forward;
            }

            // if new state or a long distance, just snap the particle system into place
            if (Vector3.Distance(pos, state.PreviousPosition) > 100.0f)
            {
                particles.transform.position = state.PreviousPosition = pos;
            }
            else
            {
                // go to new position quickly but not instantly
                particles.transform.position = state.PreviousPosition = (FirstPersonFollowSpeed <= 0.0f ? pos : Vector3.Slerp(state.PreviousPosition, pos, Time.deltaTime * FirstPersonFollowSpeed));
            }
            if (particles.shape.mesh != null)
            {
                Vector3 angles = particles.transform.rotation.eulerAngles;
                particles.transform.rotation = Quaternion.Euler(angles.x, anchorTransform.rotation.eulerAngles.y * rotationYModifier, angles.z);
            }
        }

        private void PositionAnimatedTexture(Transform anchor)
        {
            if (AnimatedTextureRenderer == null)
            {
                return;
            }
            else if (AnimatedTextureRendererIntensityThreshold >= 1.0f || Intensity == 0.0f)
            {
                AnimatedTextureRenderer.enabled = false;
            }
            else
            {
                RaycastHit hit;
                Vector3 pos = anchor.position;
                const float duration = 2.0f;

                // check if something above camera, if so disable animated texture
                if (Physics.Raycast(pos, Vector3.up, out hit, 200.0f, AnimatedTextureCollisionMaskAbove, QueryTriggerInteraction.Ignore) &&
                    hit.point.y > pos.y)
                {
                    if (AnimatedTextureRenderer.enabled && AnimatedTextureRenderer.sharedMaterial.GetFloat(WMS._AlphaMultiplierAnimation2) >= 1.0f)
                    {
                        // fade out
                        float start = AnimatedTextureRenderer.sharedMaterial.GetFloat(WMS._AlphaMultiplierAnimation2);
                        TweenFactory.Tween("WeatherMakerPrecipitationAnimatedTextureChange_" + GetInstanceID(), start, 0.0f, duration, TweenScaleFunctions.Linear, (t) =>
                        {
                            // Debug.LogFormat("Tween key: {0}, value: {1}, prog: {2}", t.Key, t.CurrentValue, t.CurrentProgress);
                            AnimatedTextureRenderer.sharedMaterial.SetFloat(WMS._AlphaMultiplierAnimation2, t.CurrentValue);
                        },
                        (t) =>
                        {
                            AnimatedTextureRenderer.enabled = false;
                        });
                    }
                }
                else
                {
                    if (Physics.Raycast(pos, Vector3.down, out hit, 200.0f, AnimatedTextureCollisionMaskBelow, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 newPos = hit.point;
                        float y = newPos.y;
                        newPos += (anchor.transform.forward * AnimatedTextureRendererPositionOffset.z);
                        newPos.y = y + AnimatedTextureRendererPositionOffset.y;
                        AnimatedTextureRenderer.transform.position = newPos;
                    }
                    float lerp = (Intensity * ExternalIntensityMultiplier) / Mathf.Max(0.01f, AnimatedTextureRendererIntensityThreshold);
                    float alpha = Mathf.Lerp(0.0f, 1.0f, lerp);
                    AnimatedTextureRenderer.sharedMaterial.SetFloat(WMS._AlphaMultiplierAnimation, alpha);
                    if (!AnimatedTextureRenderer.enabled)
                    {
                        AnimatedTextureRenderer.enabled = true;
                        float start = AnimatedTextureRenderer.sharedMaterial.GetFloat(WMS._AlphaMultiplierAnimation2);

                        // fade in
                        TweenFactory.Tween("WeatherMakerPrecipitationAnimatedTextureChange_" + GetInstanceID(), start, 1.0f, duration, TweenScaleFunctions.Linear, (t) =>
                        {
                            // Debug.LogFormat("Tween key: {0}, value: {1}, prog: {2}", t.Key, t.CurrentValue, t.CurrentProgress);
                            AnimatedTextureRenderer.sharedMaterial.SetFloat(WMS._AlphaMultiplierAnimation2, t.CurrentValue);
                        });
                    }
                }
            }
        }

        private void PositionAllElements(Transform t)
        {
            TransformParticleSystem(ParticleSystem, t, ForwardOffset, Height, 1.0f);
            TransformParticleSystem(ParticleSystemSecondary, t, SecondaryForwardOffset, SecondaryHeight, 1.0f);
            TransformParticleSystem(MistParticleSystem, t, MistForward, MistHeight, 0.0f);
            PositionAnimatedTexture(t);
            //UnityEngine.Debug.LogFormat("Position particles for {0} using transform {1}, position: {2}, forward: {3}", GetType().Name, t.name, t.position, t.forward);
        }

        /// <summary>
        /// Fires when collision enabled changes
        /// </summary>
        protected override void OnCollisionEnabledChanged()
        {
            base.OnCollisionEnabledChanged();
            UpdateCollisionForParticleSystem(ParticleSystem);
            UpdateCollisionForParticleSystem(ParticleSystemSecondary);
            UpdateCollisionForParticleSystem(MistParticleSystem);
            UpdateCollisionForParticleSystem(ExplosionParticleSystem);
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CreateMeshEmitter(ParticleSystem, ParticleSystemNearWidth, ParticleSystemFarWidth, ParticleSystemNearDepth, ParticleSystemFarDepth);
            CreateMeshEmitter(ParticleSystemSecondary, ParticleSystemSecondaryNearWidth, ParticleSystemSecondaryFarWidth, ParticleSystemSecondaryNearDepth, ParticleSystemSecondaryFarDepth);
            CreateMeshEmitter(MistParticleSystem, ParticleSystemMistNearDepth, ParticleSystemMistFarDepth, ParticleSystemMistNearDepth, ParticleSystemMistFarDepth);
            if (Application.isPlaying && AnimatedTextureRenderer != null && AnimatedTextureRenderer.sharedMaterial != null)
            {
                AnimatedTextureRenderer.sharedMaterial = AnimatedTextureRenderer.material;
                AnimatedTextureRenderer.sharedMaterial.SetFloat(WMS._AlphaMultiplierAnimation2, 0.0f);
            }
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected override void LateUpdate()
        {
            if (!IsFirstPerson)
            {
                PositionAllElements(transform);
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Application.isPlaying && AnimatedTextureRenderer != null && AnimatedTextureRenderer.sharedMaterial != null)
            {
                GameObject.Destroy(AnimatedTextureRenderer.sharedMaterial);
            }
        }

        /// <summary>
        /// Camera pre cull event
        /// </summary>
        /// <param name="camera">Camera</param>
        protected override void OnCameraPreCull(Camera camera)
        {
            if (!IsFirstPerson || Intensity == 0.0f || (WeatherMakerPrecipitationManagerScript.Instance != null &&
                WeatherMakerScript.GetCameraType(camera) != WeatherMakerCameraType.Normal && !WeatherMakerPrecipitationManagerScript.Instance.FollowNonNormalCameras))
            {
                return;
            }

#if UNITY_EDITOR

            else if (camera.cameraType != CameraType.SceneView &&
                camera.name.IndexOf("scenecamera", System.StringComparison.OrdinalIgnoreCase) < 0 &&
                camera.name.IndexOf("prerender", System.StringComparison.OrdinalIgnoreCase) < 0)
            {

#endif

                // put particle system on the new camera
                PositionAllElements(camera.transform);

                // if switching to a new camera, re-emit particles as they need to render around the new camera
                if (Intensity > 0.0f && Time.timeScale > 0.0f && (lastCamera == null || lastCamera != camera))
                {
                    // TODO: Give each camera it's own precipitation
                    // spawn new particles, Unity does not auto-update particle system when it moves, it waits until next frame
                    if (ParticleSystem.isPlaying)
                    {
                        ParticleSystem.Emit((int)Mathf.Round(ParticleSystem.emission.rateOverTime.constant * Time.deltaTime));
                        ParticleSystem.Simulate(Time.timeScale / 1000.0f, true, false, false);
                        ParticleSystem.Play(true);
                    }

                    if (MistParticleSystem != null && MistParticleSystem.isPlaying && Intensity >= MistThreshold)
                    {
                        MistParticleSystem.Emit((int)Mathf.Round(MistParticleSystem.emission.rateOverTime.constant * Time.deltaTime));
                        MistParticleSystem.Simulate(Time.timeScale / 1000.0f, true, false, false);
                        MistParticleSystem.Play(true);
                    }

                    if (ParticleSystemSecondary != null && ParticleSystemSecondary.isPlaying && Intensity >= SecondaryThreshold)
                    {
                        ParticleSystemSecondary.Emit((int)Mathf.Round(ParticleSystemSecondary.emission.rateOverTime.constant * Time.deltaTime));
                        ParticleSystemSecondary.Simulate(Time.timeScale / 1000.0f, true, false, false);
                        ParticleSystemSecondary.Play(true);
                    }
                }

                lastCamera = camera;

#if UNITY_EDITOR

            }

#endif

        }

        /// <summary>
        /// Camera post render event
        /// </summary>
        /// <param name="camera">Camera</param>
        protected override void OnCameraPostRender(Camera camera)
        {
        }
    }
}