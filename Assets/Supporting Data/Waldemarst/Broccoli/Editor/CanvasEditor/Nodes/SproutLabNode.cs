using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Girth transform node.
	/// </summary>
	[Node (true, "Structure Generator/Sprout Lab", 120)]
	public class SproutLabNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID {
			get { return typeof (SproutLabNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.MeshGenerator; } }
		/// <summary>
		/// The sprout lab element.
		/// </summary>
		public SproutLabElement sproutLabElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Sprout Lab"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			SproutLabNode node = CreateInstance<SproutLabNode> ();
			node.name = "Sprout Lab";
			node.rectSize = new Vector2 (132, 72);
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				sproutLabElement = ScriptableObject.CreateInstance<SproutLabElement> ();
			} else {
				sproutLabElement = (SproutLabElement)pipelineElement;
			}
			this.pipelineElement = sproutLabElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (sproutLabElement != null) {
				int j = 0;
				Rect sproutGroupsRect = new Rect (7, 3, 8, 8);
				for (int i = 0; i < sproutLabElement.sproutComposites.Count; i++) {
					EditorGUI.DrawRect (sproutGroupsRect, 
						sproutLabElement.pipeline.sproutGroups.GetSproutGroupColor (
							sproutLabElement.sproutComposites [i].groupId));
					j++;
					if (j >= 4) {
						sproutGroupsRect.x += 11;
						sproutGroupsRect.y = 3;
						j = 0;
					} else {
						sproutGroupsRect.y += 11;
					}
				}
			}
		}
		#endregion
	}
}
