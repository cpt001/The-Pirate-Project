using UnityEngine;
using UnityEditor;
using System.IO;

namespace FullSail
{
	public class FurledMapCreateWindow : EditorWindow
	{
		public enum Mode
		{
			Gradient,
			Curve,
		}

		public string			fname	= "Furled Map";
		public int				width	= 256;
		public int				height	= 32;
		public Mode				mode	= Mode.Curve;
		public Gradient			fill	= new Gradient();
		public AnimationCurve	fillCrv = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.1f));
		Texture2D				workTexture;
		Texture					logoImage;

		[MenuItem("Window/Full Sail/Make Furled Map", false, 2000)]
		public static void DoWindow()
		{
			GetWindow<FurledMapCreateWindow>();
		}

		public void OnGUI()
		{
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

			GradientColorKey[] keys = fill.colorKeys;
			if ( keys.Length == 2 )
			{
				keys = new GradientColorKey[4];
				keys[0] = new GradientColorKey(Color.white, 0.0f);
				keys[1] = new GradientColorKey(Color.black, 0.25f);
				keys[2] = new GradientColorKey(Color.black, 0.75f);
				keys[3] = new GradientColorKey(Color.white, 1.0f);
				fill.colorKeys = keys;
			}

			fname	= EditorGUILayout.TextField("Name", fname);
			width	= EditorGUILayout.IntField("Width", width);
			height	= EditorGUILayout.IntField("Height", height);
			mode	= (Mode)EditorGUILayout.EnumPopup("Mode", mode);

			switch ( mode )
			{
				case Mode.Gradient:	fill = EditorGUILayout.GradientField("Fill", fill);	break;
				case Mode.Curve:	fillCrv = EditorGUILayout.CurveField("Fill Curve", fillCrv); break;
			}

			if ( workTexture == null || workTexture.width != width || workTexture.height != height )
			{
				workTexture = new Texture2D(width, height, TextureFormat.RGB24, false, true);
				UpdateTexture();
			}

			if ( GUI.changed )
				UpdateTexture();

			if ( FullSailGUI.BigButton("Save Map", "Save the furl map to a texture asset") )
				CreateTexture();
		}

		void UpdateTexture()
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				if ( mode == Mode.Curve )
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * width;
						for ( int x = 0; x < workTexture.width; x++ )
						{
							float alpha = (float)x / (float)workTexture.width;
							cols[index + x] = Color.white * fillCrv.Evaluate(alpha);
						}
					}
				}
				else
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * width;
						for ( int x = 0; x < workTexture.width; x++ )
						{
							float alpha = (float)x / (float)workTexture.width;
							cols[index + x] = fill.Evaluate(alpha);
						}
					}
				}
				workTexture.SetPixels(0, 0, width, height, cols);
				workTexture.Apply(false, false);
			}
		}

		void CreateTexture()
		{
			byte[] bytes = workTexture.EncodeToPNG();
			string relpath = "Assets/Full Sail/Textures/" + fname + ".png";
			string path = Directory.GetParent("Assets/Full Sail/Textures/") + "/" + fname + ".png";
			File.WriteAllBytes(path, bytes);

			AssetDatabase.ImportAsset(relpath, ImportAssetOptions.ForceUpdate);

			TextureImporter importer = AssetImporter.GetAtPath(relpath) as TextureImporter;
			importer.wrapMode		= TextureWrapMode.Clamp;
			importer.sRGBTexture	= false;
			importer.mipmapEnabled	= false;
			importer.isReadable		= false;
			importer.SaveAndReimport();

			AssetDatabase.Refresh();
		}
	}
}