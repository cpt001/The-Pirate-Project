using UnityEditor;
using UnityEngine;

namespace FullSail
{
	[CustomEditor(typeof(FurlMap))]
	public class FurlMapEditor : Editor
	{
		Texture2D	workTexture;
		Texture		logoImage;

		public override void OnInspectorGUI()
		{
			FurlMap mod = (FurlMap)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			FullSailGUI.Header("Furl Map");

			if ( workTexture )
				GUILayout.Box(workTexture, GUILayout.Width(Screen.width), GUILayout.Height(workTexture.height));

			FullSailGUI.Header("Furl Map Params");

			mod.width	= EditorGUILayout.IntField("Width", mod.width);
			mod.height	= EditorGUILayout.IntField("Height", mod.height);
			mod.fill	= EditorGUILayout.GradientField("Fill", mod.fill);
			mod.fillCrv	= EditorGUILayout.CurveField("Fill Curve", mod.fillCrv);

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

			if ( FullSailGUI.BigButton("Save Map", "Save the furl map to a texture asset") )
				CreateTexture(mod);
		}

		void UpdateTexture(FurlMap mod)
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				for ( int y = 0; y < workTexture.height; y++ )
				{
					float alphay = 1.0f - ((float)y / (float)workTexture.height);

					int index = y * workTexture.width;
					for ( int x = 0; x < workTexture.width; x++ )
					{
						float alpha = (float)x / (float)workTexture.width;

						float my = mod.fillCrv.Evaluate(alpha);

						Color gc = mod.fill.Evaluate(alphay);
						if ( gc.r > my )
						{
							float co = gc.r - my;

							Color bc = mod.fill.Evaluate(alphay);
							Color cc = mod.fill.Evaluate(0.0f);

							gc = Color.Lerp(bc, cc, co / my);
						}
						else
						{
							// below the curve
							float co = my - gc.r;

							Color cc = mod.fill.Evaluate(alphay);
							Color bc = mod.fill.Evaluate(1.0f);

							gc = Color.Lerp(cc, bc, co / my);
						}

						cols[index + x] = gc;
					}
				}

				workTexture.SetPixels(0, 0, workTexture.width, workTexture.height, cols);
				workTexture.Apply(false, false);
			}
		}

		void CreateTexture(FurlMap mod)
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