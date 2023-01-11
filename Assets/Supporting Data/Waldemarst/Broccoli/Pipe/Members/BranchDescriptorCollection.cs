using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Container and manager for branch descriptors.
	/// </summary>
	[System.Serializable]
	public class BranchDescriptorCollection {
		[System.Serializable]
		public class SproutMapDescriptor {
			public float alphaFactor = 1.0f;
			public SproutMapDescriptor Clone () {
				SproutMapDescriptor clone = new SproutMapDescriptor ();
				clone.alphaFactor = alphaFactor;
				return clone;
			}
		}
		#region Vars
		/// <summary>
		/// The branch descriptors.
		/// </summary>
		[SerializeField]
		public List<BranchDescriptor> branchDescriptors = new List<BranchDescriptor> ();
		[SerializeField]
		public int branchDescriptorIndex = 0;
		public int lastBranchDescriptorIndex = 0;
		#endregion

		#region Map Vars
        /// <summary>
        /// Main texture for branches.
        /// </summary>
        public Texture2D branchAlbedoTexture = null;
        /// <summary>
        /// Normal map texture for branches.
        /// </summary>
        public Texture2D branchNormalTexture = null;
        public float branchTextureYDisplacement = 0f;
        public List<SproutMap.SproutMapArea> sproutAMapAreas = new List<SproutMap.SproutMapArea> ();
        public List<SproutMap.SproutMapArea> sproutBMapAreas = new List<SproutMap.SproutMapArea> ();
		public List<SproutMapDescriptor> sproutAMapDescriptors = new List<SproutMapDescriptor> ();
		public List<SproutMapDescriptor> sproutBMapDescriptors = new List<SproutMapDescriptor> ();
		public enum ColorVariance {
			None,
			Shades
		}
		public ColorVariance colorVarianceA = ColorVariance.None;
		public float minColorShadeA = 0.75f;
		public float maxColorShadeA = 1f;
		public float minColorTintA = 0f;
		public float maxColorTintA = 0.2f;
		public Color colorTintA = Color.white;
		public float metallicA = 0f;
        public float glossinessA = 0f;
		public Color subsurfaceColorA = Color.white;
		public ColorVariance colorVarianceB = ColorVariance.None;
		public float minColorShadeB = 0.75f;
		public float maxColorShadeB = 1f;
		public float minColorTintB = 0f;
		public float maxColorTintB = 0.2f;
		public Color colorTintB = Color.white;
		public float metallicB = 0f;
        public float glossinessB = 0f;
		public Color subsurfaceColorB = Color.white;
        #endregion

		#region Export Settings Vars
		/// <summary>
		/// Available texture export modes.
		/// </summary>
		public enum ExportMode {
			SelectedSnapshot,
			Atlas
		}
		/// <summary>
		/// Export mode selected.
		/// </summary>
		public ExportMode exportMode = ExportMode.Atlas;
		/// <summary>
		/// Path to save the textures relative to the data application path.
		/// </summary>
		public string exportPath = "";
		public string exportFileName = "branch_";
		public int exportTake = 0;
		/// <summary>
		/// Texture size.
		/// </summary>
		public enum TextureSize
		{
			_128px,
			_256px,
			_512px,
			_1024px,
			_2048px
		}
		public TextureSize exportTextureSize = TextureSize._1024px;
		public int exportAtlasPadding = 5;
		public bool exportAlbedoEnabled = true;
		public int exportTexturesFlags = 15;
		#endregion

		#region Clone
		public BranchDescriptorCollection Clone () {
			BranchDescriptorCollection clone = new BranchDescriptorCollection ();
			for (int i = 0; i < branchDescriptors.Count; i++) {
				clone.branchDescriptors.Add (branchDescriptors [i].Clone ());
			}
			clone.branchDescriptorIndex = branchDescriptorIndex;
			clone.lastBranchDescriptorIndex = lastBranchDescriptorIndex;
			clone.branchTextureYDisplacement = branchTextureYDisplacement;
            clone.branchAlbedoTexture = branchAlbedoTexture;
            clone.branchNormalTexture = branchNormalTexture;
			for (int i = 0; i < sproutAMapAreas.Count; i++) {
                clone.sproutAMapAreas.Add (sproutAMapAreas [i].Clone ());
            }
			for (int i = 0; i < sproutBMapAreas.Count; i++) {
                clone.sproutBMapAreas.Add (sproutBMapAreas [i].Clone ());
            }
			for (int i = 0; i < sproutAMapDescriptors.Count; i++) {
                clone.sproutAMapDescriptors.Add (sproutAMapDescriptors [i].Clone ());
            }
			for (int i = 0; i < sproutBMapDescriptors.Count; i++) {
                clone.sproutBMapDescriptors.Add (sproutBMapDescriptors [i].Clone ());
            }
			clone.colorVarianceA = colorVarianceA;
			clone.minColorShadeA = minColorShadeA;
			clone.maxColorShadeA = maxColorShadeA;
			clone.minColorTintA = minColorTintA;
			clone.maxColorTintA = maxColorTintA;
			clone.colorTintA = colorTintA;
			clone.metallicA = metallicA;
			clone.glossinessA = glossinessA;
			clone.subsurfaceColorA = subsurfaceColorA;
			clone.colorVarianceB = colorVarianceB;
			clone.minColorShadeB = minColorShadeB;
			clone.maxColorShadeB = maxColorShadeB;
			clone.minColorTintB = minColorTintB;
			clone.maxColorTintB = maxColorTintB;
			clone.colorTintB = colorTintB;
			clone.metallicB = metallicB;
			clone.glossinessB = glossinessB;
			clone.subsurfaceColorB = subsurfaceColorB;
			clone.exportMode = exportMode;
			clone.exportPath = exportPath;
			clone.exportFileName = exportFileName;
			clone.exportTake = exportTake;
			clone.exportTextureSize = exportTextureSize;
			clone.exportAtlasPadding = exportAtlasPadding;
			clone.exportTexturesFlags = exportTexturesFlags;
			return clone;
		}
		#endregion
	}
}