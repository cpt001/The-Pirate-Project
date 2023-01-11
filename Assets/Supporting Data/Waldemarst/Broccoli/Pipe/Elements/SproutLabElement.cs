using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout lab element.
	/// </summary>
	[System.Serializable]
	public class SproutLabElement : PipelineElement, ISproutGroupConsumer {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Transform; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.MeshGenerator; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.SproutLab; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.meshGeneratorWeight + 20; }
		}
		/// <summary>
		/// The sprout maps.
		/// </summary>
		public List<SproutComposite> sproutComposites = new List<SproutComposite> ();
		/// <summary>
		/// The index of the selected sprout composite.
		/// </summary>
		public int selectedCompositeIndex = -1;
		/// <summary>
		/// The assigned sprout groups.
		/// </summary>
		private List<int> assignedSproutGroups = new List<int> ();
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.SproutLabElement"/> class.
		/// </summary>
		public SproutLabElement () {}
		#endregion

		#region Sprout Composites
		/// <summary>
		/// Determines whether this instance can add sprout composite.
		/// </summary>
		/// <returns><c>true</c> if this instance can add sprout composite; otherwise, <c>false</c>.</returns>
		public bool CanAddSproutComposite () {
			return true; // TODO
		}
		/// <summary>
		/// Adds the sprout composite.
		/// </summary>
		/// <param name="sproutComposite">Sprout composite.</param>
		public void AddSproutComposite (SproutComposite sproutComposite) {
			if (pipeline != null) {
				sproutComposites.Add (sproutComposite);
			}
		}
		/// <summary>
		/// Removes a sprout composite.
		/// </summary>
		/// <param name="listIndex">List index.</param>
		public void RemoveSproutComposite (int listIndex) {
			if (pipeline != null) {
				sproutComposites.RemoveAt (listIndex);
			}
		}
		/// <summary>
		/// Gets an array of sprout group ids assigned to the element.
		/// </summary>
		/// <returns>The sprout groups assigned.</returns>
		public List<int> GetSproutGroupsAssigned () {
			assignedSproutGroups.Clear ();
			for (int i = 0; i < sproutComposites.Count; i++) {
				if (sproutComposites[i].groupId >= 0) {
					assignedSproutGroups.Add (sproutComposites[i].groupId);
				}
			}
			return assignedSproutGroups;
		}
		#endregion

		#region Sprout Group Consumer
		/// <summary>
		/// Look if certain sprout group is being used in this element.
		/// </summary>
		/// <returns><c>true</c>, if sprout group is being used, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool HasSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutComposites.Count; i++) {
				if (sproutComposites[i].groupId == sproutGroupId)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Commands the element to stop using certain sprout group.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void StopSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutComposites.Count; i++) {
				if (sproutComposites[i].groupId == sproutGroupId) {
					#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject (this, "Sprout Group Removed from Composite");
					#endif
					sproutComposites[i].groupId = 0;
				}
			}
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			SproutLabElement clone = ScriptableObject.CreateInstance<SproutLabElement> ();
			SetCloneProperties (clone);
			for (int i = 0; i < sproutComposites.Count; i++) {
				clone.sproutComposites.Add (sproutComposites [i].Clone ());
			}
			clone.selectedCompositeIndex = selectedCompositeIndex;
			return clone;
		}
		#endregion
	}
}