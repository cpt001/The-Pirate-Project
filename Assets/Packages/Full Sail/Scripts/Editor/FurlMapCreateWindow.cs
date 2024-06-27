using UnityEngine;
using UnityEditor;
using System.IO;

namespace FullSail
{
	public class FurlMapCreateWindow : EditorWindow
	{
		public string			fname	= "Furl Map";
		public int				width	= 256;
		public int				height	= 256;
		public Gradient			fill	= new Gradient();
		public AnimationCurve	fillCrv = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.1f));
		Texture2D				workTexture;
		Texture					logoImage;

		[MenuItem("Window/Full Sail/Make Furl Map", false, 2000)]
		public static void DoWindow()
		{
			GetWindow<FurlMapCreateWindow>();
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

			FullSailGUI.Header("Furl Map");

			if ( workTexture )
				GUILayout.Box(workTexture, GUILayout.Width(Screen.width), GUILayout.Height(workTexture.height));

			FullSailGUI.Header("Furl Map Params");

			fname	= EditorGUILayout.TextField("Name", fname);
			width	= EditorGUILayout.IntField("Width", width);
			height	= EditorGUILayout.IntField("Height", height);

			fill = EditorGUILayout.GradientField("Fill", fill);
			fillCrv = EditorGUILayout.CurveField("Fill Curve", fillCrv);

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

				for ( int y = 0; y < workTexture.height; y++ )
				{
					float alphay = 1.0f - ((float)y / (float)workTexture.height);

					int index = y * width;
					for ( int x = 0; x < workTexture.width; x++ )
					{
						float alpha = (float)x / (float)workTexture.width;

						float my = fillCrv.Evaluate(alpha);

						Color gc = fill.Evaluate(alphay);
						if ( gc.r > my )
						{
							float co = gc.r - my;

							Color bc = fill.Evaluate(alphay);
							Color cc = fill.Evaluate(0.0f);

							gc = Color.Lerp(bc, cc, co / my);
						}
						else
						{
							// below the curve
							float co = my - gc.r;

							Color cc = fill.Evaluate(alphay);
							Color bc = fill.Evaluate(1.0f);

							gc = Color.Lerp(cc, bc, co / my);
						}

						cols[index + x] = gc;
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