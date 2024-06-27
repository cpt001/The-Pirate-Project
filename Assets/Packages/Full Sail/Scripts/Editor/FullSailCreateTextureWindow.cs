using UnityEditor;
using UnityEngine;
using System.IO;

namespace FullSail
{
	public class FullSailCreateTextureWindow : EditorWindow
	{
		public enum TSize
		{
			S16		= 16,
			S32		= 32,
			S64		= 64,
			S128	= 128,
			S256	= 256,
			S512	= 512,
			S1024	= 1024,
		}

		public Vector2			scroll;
		public TextureFormat	format	= TextureFormat.RGB24;
		public TSize			size	= TSize.S256;
		public Texture2D		maskMap;
		public Texture2D		furlMap;
		public Texture2D		furledMap;
		Texture					logoImage;

		[ContextMenu("Help")]
		public void Help()
		{
			Application.OpenURL("http://www.macspeedee.com/full-sail/");
		}

		[MenuItem("Window/Full Sail/Create Sail Texture")]
		static void Init()
		{
			EditorWindow.GetWindow(typeof(FullSailCreateTextureWindow), false, "Sail Texture");
		}

		void OnGUI()
		{
			float w = EditorGUIUtility.currentViewWidth - 12.0f;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)position.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(position.width), GUILayout.Height(h1));
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			format = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", format);

			size = (TSize)EditorGUILayout.EnumPopup("Create Texture Size", size);
			float h = 64.0f;

			FullSailGUI.Header("Attach Mask");
			maskMap			= (Texture2D)EditorGUILayout.ObjectField("", maskMap, typeof(Texture2D), false, GUILayout.Width(w), GUILayout.Height(h));
			if ( maskMap && maskMap.isReadable == false )
				EditorGUILayout.HelpBox("Texture must be readable", MessageType.Warning);

			FullSailGUI.Header("Furl Map");
			furlMap			= (Texture2D)EditorGUILayout.ObjectField("", furlMap, typeof(Texture2D), false, GUILayout.Width(w), GUILayout.Height(h));
			if ( furlMap && furlMap.isReadable == false )
				EditorGUILayout.HelpBox("Texture must be readable", MessageType.Warning);

			FullSailGUI.Header("Furled Mask");
			furledMap		= (Texture2D)EditorGUILayout.ObjectField("", furledMap, typeof(Texture2D), false, GUILayout.Width(w), GUILayout.Height(h));
			if ( furledMap && furledMap.isReadable == false )
				EditorGUILayout.HelpBox("Texture must be readable", MessageType.Warning);

			if ( FullSailGUI.BigButton("Create Sail Map") )
				CreateTexture(maskMap, furlMap, furledMap, format, size);

			EditorGUILayout.EndScrollView();
		}

		void CreateTexture(Texture2D mask, Texture2D furl, Texture2D furled, TextureFormat format, TSize tsize)
		{
			if ( mask )
			{
				if ( mask && mask.isReadable == false )
					return;

				if ( furl && furl.isReadable == false )
					return;

				if ( furled && furled.isReadable == false )
					return;

				int size = (int)tsize;
				Texture2D both = new Texture2D(size, size, format, -1, true);

				Color maskCol	= Color.black;
				Color furlCol	= Color.black;
				Color furledCol	= Color.black;
				Color c = Color.black;

				Color[] colors = new Color[size * size];

				for ( int y = 0; y < size; y++ )
				{
					float v = (float)y / (float)size;

					for ( int x = 0; x < size; x++ )
					{
						float u = (float)x / (float)size;
						if ( mask )
							maskCol = mask.GetPixelBilinear(u, v);

						if ( furl )
							furlCol = furl.GetPixelBilinear(u, v);

						if ( furled )
							furledCol = furled.GetPixelBilinear(u, v);

						c.r = maskCol.r;
						c.g = furlCol.r;
						c.b = furledCol.r;
						c.a = 0.0f;

						colors[(y * size) + x] = c;
					}
				}

				both.SetPixels(colors);
				both.Apply(true, false);

				byte[] bytes = both.EncodeToPNG();

				string relpath = AssetDatabase.GetAssetPath(mask) +"\\" + mask.name + "_Sail.png";
				string path = Directory.GetParent(AssetDatabase.GetAssetPath(mask)) + "\\" + mask.name + "_Sail.png";
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
}