using UnityEditor;
using UnityEngine;

namespace FullSail
{
	[CustomEditor(typeof(RippleMap))]
	public class RippleMapEditor : Editor
	{
		Texture2D	workTexture;
		Texture		logoImage;

		public override void OnInspectorGUI()
		{
			RippleMap mod = (RippleMap)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			FullSailGUI.Header("Ripple Map");

			if ( workTexture )
				GUILayout.Box(workTexture, GUILayout.Width(Screen.width), GUILayout.Height(32));

			FullSailGUI.Header("Ripple Map Params");

			FullSailGUI.Int(mod,	"Width",				ref mod.width,		"Width of the texture to create");
			FullSailGUI.Int(mod,	"Height",				ref mod.height,		"Height of the texture to create");
			FullSailGUI.Toggle(mod,	"Negate",				ref mod.negate,		"Will flip the curve in the Y axis");
			FullSailGUI.Slider(mod,	"Amplitude",			ref mod.amplitude,	-1.0f, 1.0f, "Use this is quickly increase or decrease the curve vertical values");
			FullSailGUI.Slider(mod, "Scale",				ref mod.scale,		0.0f, 2.0f, "Scale the axis for the lookup");
			FullSailGUI.Curve(mod,	"Ripple Wave Curve",	ref mod.waveCrv,	"The curve that defines the ripple effect");

			if ( workTexture == null || workTexture.width != mod.width || workTexture.height != mod.height )
			{
				workTexture = new Texture2D(mod.width, mod.height, TextureFormat.RGB24, false, true);
				UpdateTexture(mod);
			}

			if ( GUI.changed )
			{
				UpdateTexture(mod);
				EditorUtility.SetDirty(target);
			}

			if ( FullSailGUI.BigButton("Save Map", "Save the ripple map to a texture asset") )
				CreateTexture(mod);
		}

		void UpdateTexture(RippleMap mod)
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				for ( int y = 0; y < workTexture.height; y++ )
				{
					int index = y * workTexture.width;
					for ( int x = 0; x < workTexture.width; x++ )
					{
						float alpha = (float)x / (float)workTexture.width;
						float v = mod.waveCrv.Evaluate(alpha * mod.scale);

						v = (v * 2.0f) - 1.0f;
						v *= mod.amplitude;
						v = (v + 1.0f) * 0.5f;

						if ( mod.negate )
							v = 1.0f - v;

						cols[index + x] = Color.white * v;
					}
				}

				workTexture.SetPixels(0, 0, workTexture.width, workTexture.height, cols);
				workTexture.Apply(false, false);
			}
		}

		void CreateTexture(RippleMap mod)
		{
			byte[] bytes = workTexture.EncodeToPNG();
			string relpath = "Assets/Full Sail/Textures/" + mod.name + ".png";
			string path = System.IO.Directory.GetParent("Assets/Full Sail/Textures/") + "/" + mod.name + ".png";
			System.IO.File.WriteAllBytes(path, bytes);

			AssetDatabase.ImportAsset(relpath, ImportAssetOptions.ForceUpdate);

			TextureImporter importer = AssetImporter.GetAtPath(relpath) as TextureImporter;
			importer.wrapMode		= TextureWrapMode.Clamp;
			importer.sRGBTexture	= false;
			importer.mipmapEnabled	= false;
			importer.isReadable		= true;
			importer.SaveAndReimport();

			AssetDatabase.Refresh();
		}
	}
}