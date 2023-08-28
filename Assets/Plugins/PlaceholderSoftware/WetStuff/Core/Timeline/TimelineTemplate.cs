using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PlaceholderSoftware.WetStuff.Timeline
{
    /// <summary>
    ///     Helper for defining a new timeline
    /// </summary>
    /// <typeparam name="TData">The data object which is contained within this timeline</typeparam>
    /// <typeparam name="TClip">The clip which contains the data</typeparam>
    /// <typeparam name="TMixer">The mixer which mixes clips</typeparam>
    /// <typeparam name="TTrack">The track which contains clips</typeparam>
    /// <typeparam name="TIntermediate">The type which the mixer produces and can be applied to a TBinding object</typeparam>
    /// <typeparam name="TBinding">The type of object this timeline changes</typeparam>
    public static class TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>
        where TData : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseData, new()
        where TClip : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseClip
        where TMixer : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseMixer, new()
        where TTrack : TemplatedTimeline<TData, TClip, TMixer, TTrack, TIntermediate, TBinding>.BaseTrack
        where TBinding : MonoBehaviour
    {
        #region  Nested Types

        /// <summary>
        ///     Base class for the track.  Must have 3 attributes attached and configured in a certain way:
        ///     - `TrackColorAttribute` must be present
        ///     - `TrackClipTypeAttribute` must be present and the type must be the `TClip` type
        ///     - `TrackBindingTypeAttribute` must be present and the type must be the `TBinding` type
        /// </summary>
        [Serializable]
        public abstract class BaseTrack
            : TrackAsset
        {
            public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
            {
#if UNITY_EDITOR
                // Check that all the attributes have been applied to this track
                GetAttribute<TrackColorAttribute>(this);

                var clip = GetAttribute<TrackClipTypeAttribute>(this);
                if (clip.inspectedType != typeof(TClip))
                    throw new UnityException(string.Format("Expected track clip type to be '{0}', not '{1}'", typeof(TClip).Name, clip.inspectedType.Name));

                var bind = GetAttribute<TrackBindingTypeAttribute>(this);
                if (bind.type != typeof(TBinding))
                    throw new UnityException(string.Format("Expected track binding to be bound to '{0}', not '{1}'", typeof(TBinding).Name, bind.type.Name));
#endif

                return ScriptPlayable<TMixer>.Create(graph, inputCount);
            }

            public override void GatherProperties([NotNull] PlayableDirector director, IPropertyCollector driver)
            {
#if UNITY_EDITOR
                // Boilerplate to make sure the track (when running in editor mode) does not permanently overwrite properties
                var comp = director.GetGenericBinding(this) as TBinding;
                if (comp == null)
                    return;
                var so = new UnityEditor.SerializedObject(comp);
                var iter = so.GetIterator();
                while (iter.NextVisible(true))
                {
                    if (iter.hasVisibleChildren)
                        continue;

                    driver.AddFromName<TBinding>(comp.gameObject, iter.propertyPath);
                }

#endif
                base.GatherProperties(director, driver);
            }
        }

        /// <summary>
        ///     Base class for the mixer
        /// </summary>
        [Serializable]
        public abstract class BaseMixer
            : PlayableBehaviour
        {
            private TBinding _trackBinding;

            /// <summary>
            ///     Indicates if the first frame has happened yet
            /// </summary>
            protected bool FirstFrameHappened { get; private set; }

            /// <summary>
            ///     The default state to use
            /// </summary>
            protected TIntermediate Default { get; private set; }

            public override void ProcessFrame(Playable playable, FrameData info, object playerData)
            {
                _trackBinding = playerData as TBinding;
                if (_trackBinding == null)
                    return;

                if (!FirstFrameHappened)
                    Default = GetState(_trackBinding);

                ApplyState(Mix(playable, info, _trackBinding), _trackBinding);

                FirstFrameHappened = true;
            }

            public override void OnGraphStop(Playable playable)
            {
                if (FirstFrameHappened)
                    ApplyState(Default, _trackBinding);

                FirstFrameHappened = false;
            }

            /// <summary>
            ///     Get the state of the object
            /// </summary>
            /// <remarks> (used to retrieve the initial state)</remarks>
            /// <param name="trackBinding"></param>
            /// <returns></returns>
            protected abstract TIntermediate GetState([NotNull] TBinding trackBinding);

            /// <summary>
            ///     Apply the output of the mixer to the given object
            /// </summary>
            /// <param name="intermediate"></param>
            /// <param name="trackBinding"></param>
            protected abstract void ApplyState(TIntermediate intermediate, [NotNull] TBinding trackBinding);

            /// <summary>
            ///     Mix the playables to produce an intermediate result representing the output of the mixer
            /// </summary>
            /// <param name="playable"></param>
            /// <param name="info"></param>
            /// <param name="trackBinding"></param>
            /// <returns></returns>
            protected abstract TIntermediate Mix(Playable playable, FrameData info, [NotNull] TBinding trackBinding);
        }

        /// <summary>
        ///     Base class for clips. Must have 1 attributes attached and configured in a certain way:
        ///     - `DisplayNameAttribute` must be present
        /// </summary>
        [Serializable]
        public abstract class BaseClip
            : PlayableAsset, ITimelineClipAsset
        {
            [UsedImplicitly, SerializeField] public TData Template = new TData();

            /// <summary>
            ///     Get the capabilities of this clip.
            /// </summary>
            public abstract ClipCaps clipCaps { get; }

            public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            {
#if UNITY_EDITOR
                GetAttribute<DisplayNameAttribute>(this);
                GetAttribute<SerializableAttribute>(this);

                GetAttribute<SerializableAttribute>(Template);
#endif

                var playable = ScriptPlayable<TData>.Create(graph, Template);
                Configure(playable.GetBehaviour());
                return playable;
            }

            /// <summary>
            ///     Configure this clip with a data object
            /// </summary>
            /// <param name="data"></param>
            // ReSharper disable once VirtualMemberNeverOverridden.Global
            protected virtual void Configure(TData data)
            {
            }
        }

        /// <summary>
        ///     Base class for the data on this timeline
        /// </summary>
        [Serializable]
        public abstract class BaseData
            : PlayableBehaviour
        {
        }

        #endregion

#if UNITY_EDITOR
        private static TAttr GetAttribute<TAttr>([NotNull] object obj)
        {
            var attrs = obj.GetType().GetCustomAttributes(typeof(TAttr), true);
            if (attrs == null || attrs.Length == 0)
                throw new UnityException(string.Format("Must put a `{0}` on {1}", typeof(TAttr).Name, obj.GetType().Name));

            return (TAttr) attrs.Single();
        }
#endif
    }
}