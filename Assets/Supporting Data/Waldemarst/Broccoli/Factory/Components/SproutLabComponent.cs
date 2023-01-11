using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sprout lab component.
	/// </summary>
	public class SproutLabComponent : TreeFactoryComponent {
		#region Vars
		/*
		/// <summary>
		/// The sprout lab element.
		/// </summary>
		SproutLabElement sproutLabElement = null;
		/// <summary>
		/// The sprout composite manager.
		/// </summary>
		SproutCompositeManager sproutCompositeManager = null;
		/// <summary>
		/// The sprout composite builder.
		/// </summary>
		SproutCompositeBuilder sproutCompositeBuilder = null;
		Mesh lod0;
		Mesh lod1;
		Mesh lod2;
		*/
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Factory.SproutLabComponent"/> class.
		/// </summary>
		public SproutLabComponent () {
			/*
			sproutCompositeBuilder = new SproutCompositeBuilder ();
			Debug.Log (sproutCompositeBuilder.treeFactory.name);
			sproutCompositeManager = new SproutCompositeManager ();
			GameObject lod0GO = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			lod0 = lod0GO.GetComponent<MeshFilter> ().sharedMesh;
			lod0GO = GameObject.CreatePrimitive (PrimitiveType.Capsule);
			lod1 = lod0GO.GetComponent<MeshFilter> ().sharedMesh;
			lod0GO = GameObject.CreatePrimitive (PrimitiveType.Cube);
			lod2 = lod0GO.GetComponent<MeshFilter> ().sharedMesh;
			*/
		}
		#endregion

		#region Configuration
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			/*
			return (int)TreeFactoryProcessControl.ChangedAspect.Mesh;
			*/
			return 0;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			/*
			base.Clear ();
			sproutLabElement = null;
			*/
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="ProcessControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl ProcessControl = null) {
			/*
			if (pipelineElement != null && tree != null) {
				sproutLabElement = pipelineElement as SproutLabElement;
				return true;
			}
			return false;
			*/
			return false;
		}
		#endregion

		#region Composite Manager
		public Mesh GetMesh (SproutComposite sproutComposite, int lod) {
			/*
			//return sproutCompositeManager.GetMesh (sproutComposite, lod);
			if (lod == 2) {
				return lod2;
			} else if (lod == 1) {
				return lod1;
			} else {
				return lod0;
			}
			*/
			return null;
		}
		#endregion
	}
}