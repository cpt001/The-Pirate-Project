using UnityEditor;
using UnityEngine;

// Show cannon reload status
// have some rough guid as to where the cannons are aiming, the view control is good but hard to tell roughly where a barage is going to go

namespace FullSail
{
	[CustomEditor(typeof(MoveMap))]
	public class MoveMapEditor : Editor
	{
		Texture2D	workTexture;
		Texture		logoImage;

		public override void OnInspectorGUI()
		{
			MoveMap mod = (MoveMap)target;

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

			mod.width			= EditorGUILayout.IntField("Width", mod.width);
			mod.height			= EditorGUILayout.IntField("Height", mod.height);
			mod.baseMap			= (Texture2D)EditorGUILayout.ObjectField("Base Map", mod.baseMap, typeof(Texture2D), false);
			mod.fixFalloff		= EditorGUILayout.Slider("Fix Falloff", mod.fixFalloff, 0.01f, 0.5f);
			mod.fixFalloffPow	= EditorGUILayout.Slider("Fix Falloff Power", mod.fixFalloffPow, 0.0f, 20.0f);
			mod.fixTL			= EditorGUILayout.Slider("Fix Top Left", mod.fixTL, 0.0f, 1.0f);
			mod.fixTR			= EditorGUILayout.Slider("Fix Top Right", mod.fixTR, 0.0f, 1.0f);
			mod.fixBL			= EditorGUILayout.Slider("Fix Bot Left", mod.fixBL, 0.0f, 1.0f);
			mod.fixBR			= EditorGUILayout.Slider("Fix Bot Right", mod.fixBR, 0.0f, 1.0f);
			mod.useFill			= EditorGUILayout.BeginToggleGroup("Use Horizontal Fill", mod.useFill);
			mod.fill			= EditorGUILayout.GradientField("Fill", mod.fill);
			mod.fillRepeat		= EditorGUILayout.FloatField("Repeat", mod.fillRepeat);
			EditorGUILayout.EndToggleGroup();
			mod.useVertFill		= EditorGUILayout.BeginToggleGroup("Use Vertical Fill", mod.useVertFill);
			mod.vertFill		= EditorGUILayout.GradientField("Vertical Fill", mod.vertFill);
			mod.vertFillRepeat	= EditorGUILayout.FloatField("Vertical Repeat", mod.vertFillRepeat);
			EditorGUILayout.EndToggleGroup();
			mod.useCurves		= EditorGUILayout.BeginToggleGroup("Use Curves", mod.useCurves);
			mod.useEdgeCurves	= EditorGUILayout.Toggle("Use Edge Curves", mod.useEdgeCurves);

			if ( mod.useEdgeCurves )
			{
				mod.fillCrvY	= EditorGUILayout.CurveField("Fill Curve Left", mod.fillCrvY);
				mod.fillCrvY1	= EditorGUILayout.CurveField("Fill Curve Right", mod.fillCrvY1);
				mod.fillCrvX1	= EditorGUILayout.CurveField("Fill Curve Top", mod.fillCrvX1);
				mod.fillCrvX	= EditorGUILayout.CurveField("Fill Curve Bottom", mod.fillCrvX);
			}
			else
			{
				mod.fillCrvX	= EditorGUILayout.CurveField("Fill Curve X", mod.fillCrvX);
				mod.fillCrvY	= EditorGUILayout.CurveField("Fill Curve Y", mod.fillCrvY);
			}
			EditorGUILayout.EndToggleGroup();
			mod.useBlob			= EditorGUILayout.BeginToggleGroup("Use Blob", mod.useBlob);
			mod.blobAmt			= EditorGUILayout.Slider("Blob Amount", mod.blobAmt, 0.0f, 1.0f);
			mod.blobPos			= EditorGUILayout.Vector2Field("Blob Pos", mod.blobPos);
			mod.blobRadius		= EditorGUILayout.Slider("Blob Radius", mod.blobRadius, 0.0f, 1.0f);
			mod.blobFalloff		= EditorGUILayout.Slider("Blob Falloff", mod.blobFalloff, 0.0f, 1.0f);
			mod.blobFalloffPow	= EditorGUILayout.Slider("Blob Falloff Power", mod.blobFalloffPow, 0.0f, 20.0f);

			EditorGUILayout.EndToggleGroup();

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

		float DistFrom(float x, float y, float xt, float yt)
		{
			float dx = Mathf.Abs(xt - x);
			float dy = Mathf.Abs(yt - y);

			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		void UpdateTexture(MoveMap mod)
		{
			if ( workTexture )
			{
				Color[] cols = workTexture.GetPixels();

				if ( mod.baseMap )
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * workTexture.width;
						float ay = (float)y / (float)workTexture.height;

						for ( int x = 0; x < workTexture.width; x++ )
						{
							float a = (float)x / (float)workTexture.width;

							Color c = mod.baseMap.GetPixelBilinear(a, ay);
							cols[index + x] = c;
						}
					}
				}

				if ( mod.useCurves )
				{
					if ( mod.useEdgeCurves )
					{
						for ( int y = 0; y < workTexture.height; y++ )
						{
							int index = y * workTexture.width;
							float ay = (float)y / (float)workTexture.height;
							float alphay = mod.fillCrvY.Evaluate(ay);
							float alphay1 = mod.fillCrvY1.Evaluate(ay);

							for ( int x = 0; x < workTexture.width; x++ )
							{
								float a = (float)x / (float)workTexture.width;
								float alphax = mod.fillCrvX.Evaluate(a);
								float alphax1 = mod.fillCrvX1.Evaluate(a);

								float fa = (a * mod.fillRepeat) % 1.0f;
								float vfa = (ay * mod.vertFillRepeat) % 1.0f;

								Color fc = Color.white;
								Color vfc = Color.white;

								if ( mod.useFill )
									fc = mod.fill.Evaluate(fa);
								if ( mod.useVertFill )
									vfc = mod.vertFill.Evaluate(vfa);

								if ( fc.r < vfc.r )
								{

								}
								else
									fc = vfc;

								Color prec = Color.white * Mathf.Lerp(alphay, alphay1, a) * Mathf.Lerp(alphax, alphax1, ay) * fc;

								// Are we near corners
								float d = DistFrom(a, ay, 0.0f, 1.0f);
								if ( d < mod.fixFalloff )
								{
									d /= mod.fixFalloff;
									cols[index + x] = Color.Lerp(prec * mod.fixTL, prec, Mathf.Pow(d, mod.fixFalloffPow));
								}
								else
								{
									d = DistFrom(a, ay, 1.0f, 1.0f);
									if ( d < mod.fixFalloff )
									{
										d /= mod.fixFalloff;
										cols[index + x] = Color.Lerp(prec * mod.fixTR, prec, Mathf.Pow(d, mod.fixFalloffPow));
									}
									else
									{
										d = DistFrom(a, ay, 0.0f, 0.0f);
										if ( d < mod.fixFalloff )
										{
											d /= mod.fixFalloff;
											cols[index + x] = Color.Lerp(prec * mod.fixBL, prec, Mathf.Pow(d, mod.fixFalloffPow));
										}
										else
										{
											d = DistFrom(a, ay, 1.0f, 0.0f);

											if ( d < mod.fixFalloff )
											{
												d /= mod.fixFalloff;
												cols[index + x] = Color.Lerp(prec * mod.fixBR, prec, Mathf.Pow(d, mod.fixFalloffPow));
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
					}
					else
					{
						for ( int y = 0; y < workTexture.height; y++ )
						{
							int index = y * workTexture.width;
							float ay = (float)y / (float)workTexture.height;
							float alphay = mod.fillCrvY.Evaluate(ay);

							for ( int x = 0; x < workTexture.width; x++ )
							{
								float a = (float)x / (float)workTexture.width;
								float alphax = mod.fillCrvX.Evaluate(a);

								float fa = (a * mod.fillRepeat) % 1.0f;
								float vfa = (ay * mod.vertFillRepeat) % 1.0f;

								Color fc = Color.white;
								Color vfc = Color.white;

								if ( mod.useFill )
									fc = mod.fill.Evaluate(fa);
								if ( mod.useVertFill )
									vfc = mod.vertFill.Evaluate(vfa);

								if ( fc.r < vfc.r )
								{
								}
								else
									fc = vfc;

								Color prec = Color.white * alphay * alphax * fc;

								// Are we near corners
								float d = DistFrom(a, ay, 0.0f, 1.0f);
								if ( d < mod.fixFalloff )
								{
									d /= mod.fixFalloff;
									cols[index + x] = Color.Lerp(prec * mod.fixTL, prec, Mathf.Pow(d, mod.fixFalloffPow));
								}
								else
								{
									d = DistFrom(a, ay, 1.0f, 1.0f);
									if ( d < mod.fixFalloff )
									{
										d /= mod.fixFalloff;
										cols[index + x] = Color.Lerp(prec * mod.fixTR, prec, Mathf.Pow(d, mod.fixFalloffPow));
									}
									else
									{
										d = DistFrom(a, ay, 0.0f, 0.0f);
										if ( d < mod.fixFalloff )
										{
											d /= mod.fixFalloff;
											cols[index + x] = Color.Lerp(prec * mod.fixBL, prec, Mathf.Pow(d, mod.fixFalloffPow));
										}
										else
										{
											d = DistFrom(a, ay, 1.0f, 0.0f);

											if ( d < mod.fixFalloff )
											{
												d /= mod.fixFalloff;
												cols[index + x] = Color.Lerp(prec * mod.fixBR, prec, Mathf.Pow(d, mod.fixFalloffPow));
											}
											else
											{
												cols[index + x] = prec;
											}
										}
									}
								}

								if ( mod.useBlob )
								{
									d = DistFrom(a, ay, mod.blobPos.x, mod.blobPos.y);
									if ( d < mod.blobRadius )
									{
										cols[index + x] *= mod.blobAmt;	// * Mathf.Pow(d, mod.blobFalloffPow);
									}
									else
									{
										if ( d < mod.blobRadius + mod.blobFalloff )
										{
											float ca = Mathf.Lerp(mod.blobAmt, 1.0f, Mathf.Pow((d - mod.blobRadius) / mod.blobFalloff, mod.blobFalloffPow));
											cols[index + x] *= ca;	//(mod.blobAmt - (Mathf.Pow((d - mod.blobRadius) / mod.blobFalloff, mod.blobFalloffPow)));
										}
									}
								}
							}
						}
					}
				}
				else
				{
					for ( int y = 0; y < workTexture.height; y++ )
					{
						int index = y * workTexture.width;
						float ay = (float)y / (float)workTexture.height;

						for ( int x = 0; x < workTexture.width; x++ )
						{
							float a = (float)x / (float)workTexture.width;

							float fa = (a * mod.fillRepeat) % 1.0f;
							float vfa = (ay * mod.vertFillRepeat) % 1.0f;

							Color fc = Color.white;
							Color vfc = Color.white;

							if ( mod.useFill )
								fc = mod.fill.Evaluate(fa);
							if ( mod.useVertFill )
								vfc = mod.vertFill.Evaluate(vfa);

							if ( fc.r < vfc.r )
								cols[index + x] *= fc;
							else
								cols[index + x] *= vfc;

							if ( mod.useBlob )
							{
								float d = DistFrom(a, ay, mod.blobPos.x, mod.blobPos.y);
								if ( d < mod.blobRadius )
									cols[index + x] *= mod.blobAmt;
								else
								{
									if ( d < mod.blobRadius + mod.blobFalloff )
									{
										float ca = Mathf.Lerp(mod.blobAmt, 1.0f, Mathf.Pow((d - mod.blobRadius) / mod.blobFalloff, mod.blobFalloffPow));
										cols[index + x] *= ca;
									}
								}
							}
						}
					}
				}
				workTexture.SetPixels(0, 0, workTexture.width, workTexture.height, cols);
				workTexture.Apply(false, false);
			}
		}

		void CreateTexture(MoveMap mod)
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