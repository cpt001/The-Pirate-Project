using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Base;
using Broccoli.Pipe;

namespace Broccoli.Manager
{
	public class SproutCompositeManager {
		#region Vars
		/*
		Dictionary<int, SproutComposite> sproutComposites = new Dictionary<int, SproutComposite> ();
		Dictionary<int, Mesh> meshes = new Dictionary<int, Mesh> ();
		*/
		#endregion

		#region Management
		public bool AddMesh (SproutComposite sproutComposite, int lod, Mesh mesh) {
			/*
			int id = sproutComposite.id + lod;
			if (!meshes.ContainsKey (id)) {
				meshes.Add (id, mesh);
				return true;
			}
			*/
			return false;
		}
		public Mesh GetMesh (SproutComposite sproutComposite, int lod) {
			/*
			int id = sproutComposite.id + lod;
			if (meshes.ContainsKey (id)) {
				return meshes [id];
			}
			*/
			return null;
		}
		#endregion
	}
}