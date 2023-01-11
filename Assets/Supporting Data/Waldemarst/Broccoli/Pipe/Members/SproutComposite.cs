using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout composite.
	/// </summary>
	[System.Serializable]
	public class SproutComposite {
		/// <summary>
		/// The composite identifier.
		/// </summary>
		public int compositeId = 0;
		/// <summary>
		/// The variation identifier.
		/// </summary>
		public int variationId = 0; // TODO: lingo changed from areaId.
		/// <summary>
		/// Type of composite enumerator.
		/// </summary>
		public enum Type {
			Branch,
			Flower
		}
		/// <summary>
		/// Type of the composite.
		/// </summary>
		public Type type = Type.Branch;
		/// <summary>
		/// The group identifier using this composite.
		/// </summary>
		public int groupId = 0;
		/// <summary>
		/// LOD0 enabled.
		/// </summary>
		public bool lod0Enabled = true;
		/// <summary>
		/// LOD1 enabled.
		/// </summary>
		public bool lod1Enabled = true;
		/// <summary>
		/// LOD2 enabled.
		/// </summary>
		public bool lod2Enabled = true;
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public int id {
			get { return compositeId * 1000 + variationId * 10 + (int)type; }
		}
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public SproutComposite Clone () {
			SproutComposite sproutComposite = new SproutComposite ();
			sproutComposite.compositeId = compositeId;
			sproutComposite.variationId = variationId;
			sproutComposite.type = type;
			sproutComposite.groupId = groupId;
			sproutComposite.lod0Enabled = lod0Enabled;
			sproutComposite.lod1Enabled = lod1Enabled;
			sproutComposite.lod2Enabled = lod2Enabled;
			return sproutComposite;
		}
	}
}