using UnityEditor;
using UnityEngine;

namespace FullSail
{
	[CustomEditor(typeof(FurledMap))]
	public class FurledMapEditor : Editor
	{
		Texture2D	workTexture;
		Texture		logoImage;

		public override void OnInspectorGUI()
		{
			FurledMap mod = (FurledMap)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			FullSailGUI.Header("Furled Map");

			if ( workTexture )
				GUILayout.Box(workTexture, GUILayout.Width(Screen.width), GUILayout.Height(workTexture.height));

			FullSailGUI.Header("Furled Map Params");

			mod.width	= EditorGUILayout.IntField("Width", mod.width);
			mod.height	= EditorGUILayout.IntField("Height", mod.height);
			mod.mode	= (FurledMap.Mode)EditorGUILayout.EnumPopup("Mode", mod.mode);

			switch ( mod.mode )
			{
				case FurledMap.Mode.Gradient:	mod.fill	= EditorGUILayout.GradientField("Fill", mod.fill); break;
				case FurledMap.Mode.Curve:		mod.fillCrv	= EditorGUILayout.CurveField("Fill Curve", mod.fillCrv); break;
			}

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

		void UpdateTexture(FurledMap mod)
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				if ( mod.mode == FurledMap.Mode.Curve )
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * workTexture.width;
						for ( int x = 0; x < workTexture.width; x++ )
						{
							float alpha = (float)x / (float)workTexture.width;
							cols[index + x] = Color.white * mod.fillCrv.Evaluate(alpha);
						}
					}
				}
				else
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * workTexture.width;
						for ( int x = 0; x < workTexture.width; x++ )
						{
							float alpha = (float)x / (float)workTexture.width;
							cols[index + x] = mod.fill.Evaluate(alpha);
						}
					}
				}
				workTexture.SetPixels(0, 0, workTexture.width, workTexture.height, cols);
				workTexture.Apply(false, false);
			}
		}

		void CreateTexture(FurledMap mod)
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