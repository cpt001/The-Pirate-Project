using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Utils;
using Broccoli.Factory;
using Broccoli.Pipe;

namespace Broccoli.Builder
{
	public class SproutCompositeBuilder {
		#region Vars
		GameObject _treeFactoryGO;
		TreeFactory _treeFactory = null;
		public TreeFactory treeFactory {
			get { 
				if (_treeFactory == null) {
					_treeFactoryGO = GameObject.Find (SproutCompositeBuilder.treeFactoryGOName);
					if (_treeFactoryGO == null) {
						_treeFactoryGO = new GameObject (SproutCompositeBuilder.treeFactoryGOName);
						_treeFactory = _treeFactoryGO.AddComponent<TreeFactory> ();
					} else {
						_treeFactoryGO.GetComponent<TreeFactory> ();
					}
				}
				return _treeFactory;
			}
		}
		static string treeFactoryGOName = "__broccoliSproutLabFactory";
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton for this class.
		/// </summary>
		static SproutCompositeBuilder _sproutCompositeBuilder = null;
		/// <summary>
		/// Gets the singleton instance for this class.
		/// </summary>
		/// <returns>The instance.</returns>
		public static SproutCompositeBuilder GetInstance() {
			if (_sproutCompositeBuilder == null) {
				_sproutCompositeBuilder = new SproutCompositeBuilder ();
			}
			return _sproutCompositeBuilder;
		}
		#endregion

		#region Data Ops
		/// <summary>
		/// Clear local variables.
		/// </summary>
		void Clear () {
		}
		void OnDestroy () {
			Debug.Log ("Destroying!!");
			Object.DestroyImmediate (_treeFactoryGO);
		}
		#endregion

		#region Processing
		public Mesh GenerateComposite (SproutComposite sproutComposite) {
			return null;
		}
		#endregion
	}
}