using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Utils
{
	/// <summary>
	/// Editor utility class to preview meshes on custom editors.
	/// </summary>
	public class GeometryAnalyzer {
		#region Vars
		/// <summary>
		/// Keeps the branch points when analyzing a tree structure.
		/// </summary>
        public List<Vector3> branchPoints = new List<Vector3> ();
		/// <summary>
		/// Keeps the sprout points when analyzing a tree structure.
		/// </summary>
        public List<Vector3> sproutPoints = new List<Vector3> ();
		/// <summary>
		/// Temp list for branches.
		/// </summary>
		List<BroccoTree.Branch> _branches = new List<BroccoTree.Branch> ();
		/// <summary>
		/// Temp list for sprouts.
		/// </summary>
		List<BroccoTree.Sprout> _sprouts = new List<BroccoTree.Sprout> ();
		#endregion
		
		#region Singleton
		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static GeometryAnalyzer _instance = null;
		/// <summary>
		/// Get the singleton instance.
		/// </summary>
		/// <returns>Singleton instance.</returns>
		public static GeometryAnalyzer Current () {
			if (_instance == null) {
				_instance = new GeometryAnalyzer ();
			}
			return _instance;
		}
		#endregion

		#region Ops
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			branchPoints.Clear ();
			sproutPoints.Clear ();
		}
		#endregion

		#region Traversing and Analyzing
		/// <summary>
		/// Gets positions from the branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="hierarchyLevel">Hierarchy level on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetBranchPositions (
			BroccoTree tree, 
			float relativePosition, 
			int hierarchyLevel = -1, 
			bool isAdditive = true)
		{
			_branches.Clear ();
			if (hierarchyLevel < 0) {
				_branches = tree.GetDescendantBranches ();
			} else {
				_branches = tree.GetDescendantBranches (hierarchyLevel);
			}
			if (!isAdditive) {
				branchPoints.Clear ();
			}
			for (int i = 0; i < _branches.Count; i++) {
				branchPoints.Add (_branches [i].GetPointAtPosition (relativePosition));
			}
		}
		/// <summary>
		/// Gets positions from the terminal branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetTerminalBranchPositions (
			BroccoTree tree, float relativePosition, bool isAdditive = true)
		{
			GetBranchPositions (tree, relativePosition, tree.GetOffspringLevel () - 1, isAdditive);
		}
		/// <summary>
		/// Gets positions from the base branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="hierarchyLevel">Hierarchy level on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetBaseBranchPositions (
			BroccoTree tree, float relativePosition, bool isAdditive = true)
		{
			GetBranchPositions (tree, relativePosition, 0, isAdditive);
		}
		/// <summary>
		/// Gets the positions from the sprouts of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="branchHierarchyLevel">Hierarchy level of the branches on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetSproutPositions (
			BroccoTree tree,
			int branchHierarchyLevel = -1,
			bool isAdditive = true)
		{
			_branches.Clear ();
			_sprouts.Clear ();
			if (branchHierarchyLevel < 0) {
				_branches = tree.GetDescendantBranches ();
			} else {
				_branches = tree.GetDescendantBranches (branchHierarchyLevel);
			}
			if (!isAdditive) {
				sproutPoints.Clear ();
			}
			for (int i = 0; i < _branches.Count; i++) {
				_sprouts = _branches [i].sprouts;
				for (int j = 0; j < _sprouts.Count; j++) {
					if (_sprouts [j].meshHeight > 0f) {
						sproutPoints.Add (
							_branches [i].GetPointAtPosition (
								_sprouts [j].position) + 
							_sprouts [j].sproutDirection.normalized * _sprouts [j].meshHeight);
					}
				}
			}
		}
		#endregion
	}
}