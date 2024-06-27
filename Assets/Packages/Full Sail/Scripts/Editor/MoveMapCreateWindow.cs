using UnityEngine;
using UnityEditor;
using System.IO;

namespace FullSail
{
	public class MoveMapCreateWindow : EditorWindow
	{
		public string			fname			= "Move Map";
		public int				width			= 256;
		public int				height			= 256;

		// Have array of fix points. gradient across and down for ribs
		// attach point texture? or number attach points top, bottom, sides
		public float			fixFalloff		= 0.1f;
		public float			fixFalloffPow	= 0.2f;
		public float			fixTL			= 1.0f;
		public float			fixTR			= 1.0f;
		public float			fixBL			= 1.0f;
		public float			fixBR			= 1.0f;

		public Gradient			fill			= new Gradient();
		public float			fillRepeat		= 1.0f;
		public AnimationCurve	fillCrvX		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		public AnimationCurve	fillCrvY		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		Texture2D				workTexture;
		Texture					logoImage;

		[MenuItem("Window/Full Sail/Make Move Map", false, 2000)]
		public static void DoWindow()
		{
			GetWindow<MoveMapCreateWindow>();
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

			FullSailGUI.Header("Move Map");

			if ( workTexture )
				GUILayout.Box(workTexture, GUILayout.Width(Screen.width), GUILayout.Height(workTexture.height));

			FullSailGUI.Header("Move Map Params");

			fname	= EditorGUILayout.TextField("Name", fname);
			width	= EditorGUILayout.IntField("Width", width);
			height	= EditorGUILayout.IntField("Height", height);

			fixFalloff = EditorGUILayout.Slider("Fix Falloff", fixFalloff, 0.01f, 0.5f);
			fixFalloffPow = EditorGUILayout.Slider("Fix Falloff Power", fixFalloffPow, 0.0f, 20.0f);
			fixTL = EditorGUILayout.Slider("Fix Top Left", fixTL, 0.0f, 1.0f);
			fixTR = EditorGUILayout.Slider("Fix Top Right", fixTR, 0.0f, 1.0f);
			fixBL = EditorGUILayout.Slider("Fix Bot Left", fixBL, 0.0f, 1.0f);
			fixBR = EditorGUILayout.Slider("Fix Bot Right", fixBR, 0.0f, 1.0f);

			fill = EditorGUILayout.GradientField("Fill", fill);
			fillRepeat = EditorGUILayout.FloatField("Repeat", fillRepeat);
			fillCrvX = EditorGUILayout.CurveField("Fill Curve X", fillCrvX);
			fillCrvY = EditorGUILayout.CurveField("Fill Curve Y", fillCrvY);

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

		float DistFrom(float x, float y, float xt, float yt)
		{
			float dx = Mathf.Abs(xt - x);
			float dy = Mathf.Abs(yt - y);

			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		void UpdateTexture()
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				for ( int y = 0; y < workTexture.height; y++ )
				{
					int index = y * width;
					float ay = (float)y / (float)workTexture.height;
					float alphay = fillCrvY.Evaluate(ay);

					for ( int x = 0; x < workTexture.width; x++ )
					{
						float a = (float)x / (float)workTexture.width;
						float alphax = fillCrvX.Evaluate(a);

						float fa = (a * fillRepeat) % 1.0f;

						Color fc = fill.Evaluate(fa);

						Color prec = Color.white * alphay * alphax * fc;

						// Are we near corners
						float d = DistFrom(a, ay, 0.0f, 1.0f);
						if ( d < fixFalloff )
						{
							d /= fixFalloff;
							cols[index + x] = Color.Lerp(prec * fixTL, prec, Mathf.Pow(d, fixFalloffPow));
						}
						else
						{
							d = DistFrom(a, ay, 1.0f, 1.0f);
							if ( d < fixFalloff )
							{
								d /= fixFalloff;
								cols[index + x] = Color.Lerp(prec * fixTR, prec, Mathf.Pow(d, fixFalloffPow));
							}
							else
							{
								if ( DistFrom(a, ay, 0.0f, 0.0f) < fixFalloff )
								{
									d /= fixFalloff;
									cols[index + x] = Color.Lerp(prec, prec * fixBL, Mathf.Pow(d, fixFalloffPow));
								}
								else
								{
									if ( DistFrom(a, ay, 1.0f, 0.0f) < fixFalloff )
									{
										d /= fixFalloff;
										cols[index + x] = Color.Lerp(prec, prec * fixBR, Mathf.Pow(d, fixFalloffPow));
									}
									else
									{
										cols[index + x] = prec;
									}
								}
							}
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