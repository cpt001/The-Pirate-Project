using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace FullSail
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Sail))]
	public class SailEditor : Editor
	{
		static public bool ShowParam(Object mod, SailParam sp)
		{
			float		fval = sp.fval;
			int			ival = sp.ival;
			Vector4		vval = sp.vval;
			Color		cval = sp.cval;
			Texture2D	tval = sp.tval;

			switch ( sp.id )
			{
				case SailParamID.None:				break;
				case SailParamID.RippleNoise:		FullSailGUI.Texture2D(mod,	"", ref tval, false, "Noise to use for Ripples"); break;
				case SailParamID.Color:				FullSailGUI.ColorField(mod,	"",	ref cval, "Main Color tint for the sail"); break;
				case SailParamID.EmblemColor:		FullSailGUI.ColorField(mod,	"", ref cval, "Tint color for the emblem, alpha value for transparency"); break;
				case SailParamID.EmblemEMColor:		FullSailGUI.ColorField(mod,	"", ref cval, "Emissive Color for the emblem"); break;
				case SailParamID.SpecColor:			FullSailGUI.ColorField(mod,	"",	ref cval, "Specular color"); break;
				case SailParamID.SubColor:			FullSailGUI.ColorField(mod,	"",	ref cval, "Sub surface scattering color"); break;
				case SailParamID.Albedo:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Main sail texture"); break;
				case SailParamID.Emblem:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Texture to apply as an emblem to the sail"); break;
				case SailParamID.DamageTex:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "First stage of damage texture"); break;
				case SailParamID.DamageTex1:		FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Second stage of damage texture"); break;
				case SailParamID.DamageTex2:		FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Third stage of damage texture"); break;
				case SailParamID.BumpMap:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Bumpmap for the sail"); break;
				case SailParamID.MaskMap:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Texture to control the sail movement (R), if using the Optimized shader then also controls Furl (G) and Furled (B)"); break;
				case SailParamID.MaskMapRev:		FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Texture to control the sail movement (R) for when wind is against the sail, if using the Optimized shader then also controls Furl (G) and Furled (B)"); break;
				case SailParamID.FurlMap:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Map that controls how the sail furls (R)"); break;
				case SailParamID.FurledMap:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Map that controls the look of the fully furled sail");	break;
				case SailParamID.Thickness:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Texture used to control the thickness of the sail for translucency (R)"); break;
				case SailParamID.WindDir:			FullSailGUI.Vector4(mod,	"",	ref vval, "Direction vector of the wind"); break;
				case SailParamID.SailForward:		FullSailGUI.Vector4(mod,	"",	ref vval, "Forward vector of the sail usually 0 0 1 0"); break;
				case SailParamID.EmblemBackface:	FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "How much of the emblem is shown on the back of the sail"); break;
				case SailParamID.Damage:			FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "Damage for the sail"); break;
				case SailParamID.Smoothness:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "Specular smoothness value"); break;
				case SailParamID.AlphaCutoff:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "Alpha cutoff for the sail and damage"); break;
				case SailParamID.Speed:				FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 8.0f, "Speed of the wind for the sail"); break;
				case SailParamID.SailWind:			FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "How the wind moves the sail, combines with Fill Percent"); break;
				case SailParamID.FillPercent:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "How the wind fills the sail, combines with SailWind"); break;
				case SailParamID.SailLift:			FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 2.0f, "Controls how much the bottom of the sail will lift as it fills with wind"); break;
				case SailParamID.SailLiftArch:		FullSailGUI.Slider(mod,		"",	ref fval, -2.0f, 2.0f, "Controls how much the bottom of the sail will arch as it lifts"); break;
				case SailParamID.SailLiftSideArch:	FullSailGUI.Slider(mod,		"",	ref fval, -2.0f, 2.0f, "Controls how much the sides of the sails will arch as it lifts"); break;
				case SailParamID.SailReverse:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "If the wind is against the sail this will limit the movement, stops masks showing through etc"); break;
				case SailParamID.SailTaper:			FullSailGUI.Slider(mod,		"",	ref fval, -4.0f, 4.0f, "How much the sail tapers in or out at the bottom"); break;
				case SailParamID.SailArch:			FullSailGUI.Slider(mod,		"",	ref fval, -4.0f, 4.0f, "How arched is the sail, this is added to by any lift arch"); break;
				case SailParamID.SailTopArch:		FullSailGUI.Slider(mod,		"",	ref fval, -4.0f, 4.0f, "How arched is the top of the sail"); break;
				case SailParamID.SailArchStart:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "How far down the sail the arch starts to appear"); break;
				case SailParamID.SailSideArch:		FullSailGUI.Slider(mod,		"",	ref fval, -4.0f, 4.0f, "How arched are the sail sides"); break;
				case SailParamID.SailSideways:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "How arched are the sail moves when wind is side on"); break;
				case SailParamID.FullRipple:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 1.0f, "This value reduces the amount of ripple visible as the sail fills with wind"); break;
				case SailParamID.Furl:				FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 2.0f, "How furled the sail is"); break;
				case SailParamID.FurledRadius:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 4.0f, "Radius of the furled sail"); break;
				case SailParamID.RippleStrength:	FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 5.0f, "How much the ripple effect moves the sail, gets used with wind value"); break;
				case SailParamID.RippleSpeed:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 10.0f, "Speed of the rippling effect"); break;
				case SailParamID.RippleSeed:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 10.0f, "A value that is used to offset the ripple animation to stop all sails moving the same way"); break;
				case SailParamID.RippleScale:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 10.0f, "Size of the ripple waves"); break;
				case SailParamID.RippleSmooth:		FullSailGUI.Slider(mod,		"",	ref fval, 0.0f, 10.0f, "Smoothness of the ripple waves"); break;
				case SailParamID.SailShear:			FullSailGUI.Float(mod,		"",	ref fval, "Controls how much the bottom of the sail is moved relative to the bottom"); break;
				case SailParamID.SailTilt:			FullSailGUI.Float(mod,		"",	ref fval, "Controls how much the bottom of the sail tilts"); break;
				case SailParamID.Power:				FullSailGUI.Float(mod,		"",	ref fval, "Controls how the translucency falloffs from the light direction"); break;
				case SailParamID.Distortion:		FullSailGUI.Float(mod,		"",	ref fval, "Translucency distortion"); break;
				case SailParamID.Scale:				FullSailGUI.Float(mod,		"",	ref fval, "Adjusts the amount of translucency"); break;
				case SailParamID.FurlOrder:			FullSailGUI.Int(mod,		"",	ref ival, "Controls the order the sail furls when in a sail group"); break;
				case SailParamID.ImpactCount:		FullSailGUI.Int(mod,		"",	ref ival, "How many impacts to apply to the sail"); break;
				case SailParamID.ImpactTex:			FullSailGUI.Texture2D(mod,	"",	ref tval, false, "Impact texture atlas"); break;
				case SailParamID.ImpactRipple:		FullSailGUI.Texture2D(mod,	"",	ref tval, false, "The impact wave ripple texture (A)"); break;
				case SailParamID.ImpactLocation:	FullSailGUI.Vector4(mod,	"",	ref vval, "The location (XY), time(Z) and strength(W) of the impact"); break;
			}

			if ( !sp.fval.Equals(fval) || sp.ival != ival || !sp.vval.Equals(vval) || sp.tval != tval || !sp.cval.Equals(cval) )
			{
				FullSailGUI.RecordObject(mod, "Changed Sail Param " + sp.id);

				sp.fval = fval;
				sp.ival = ival;
				sp.tval = tval;
				sp.cval = cval;
				sp.vval = vval;
				return true;
			}

			return false;
		}

		public class ParamIDEnum
		{
			public string		name;
			public SailParamID	id;
		}

		static public List<ParamIDEnum> paramIDEnums = new List<ParamIDEnum>();
		static public string[] paramIDNames;

		static public int Compare(ParamIDEnum o1, ParamIDEnum o2)
		{
			return o1.name.CompareTo(o2.name);
		}

		static public void BuildParamEnum()
		{
			string[] enames = System.Enum.GetNames(typeof(SailParamID));

			if ( paramIDEnums == null || paramIDEnums.Count != enames.Length )
			{
				SailParamID[] evalues = (SailParamID[])System.Enum.GetValues(typeof(SailParamID));
				paramIDEnums.Clear();

				for ( int i = 0; i < enames.Length; i++ )
				{
					ParamIDEnum en = new ParamIDEnum();
					en.id = evalues[i];
					en.name = enames[i];
					paramIDEnums.Add(en);
				}
				paramIDEnums.Sort(Compare);

				paramIDNames = new string[paramIDEnums.Count];

				for ( int i = 0; i < paramIDEnums.Count; i++ )
				{
					paramIDNames[i] = paramIDEnums[i].name;
				}
			}
		}

		static public SailParamID SailParam(string name, SailParamID id, string tip = "")
		{
			BuildParamEnum();

			int index = 0;

			for ( int i = 0; i < paramIDEnums.Count; i++ )
			{
				if ( paramIDEnums[i].id == id )
				{
					index = i;
					break;
				}
			}

			int newid = EditorGUILayout.Popup(name, index, paramIDNames, GUILayout.Width(100));

			return paramIDEnums[newid].id;
		}

		static public bool ShowSailParam(Object mod, SailParam sp, ref bool changed)
		{
			EditorGUILayout.BeginHorizontal("box");

			bool active = EditorGUILayout.Toggle("", sp.active, GUILayout.Width(20));
			if ( active != sp.active )
			{
				FullSailGUI.RecordObject(mod, "Changed Sail Param ID");
				sp.active = active;
				changed = true;
			}

			SailParamID newid = SailParam("", sp.id);

			if ( newid != sp.id )
			{
				FullSailGUI.RecordObject(mod, "Changed Sail Param ID");
				sp.id = newid;
				changed = true;
			}

			if ( ShowParam(mod, sp) )
				changed = true;

			if ( GUILayout.Button("-", GUILayout.Width(16)) )
				return true;

			EditorGUILayout.EndHorizontal();

			return false;
		}

		public bool ShowSailParam1(Object mod, SailParam sp)
		{
			EditorGUILayout.BeginHorizontal("box");

			sp.active = EditorGUILayout.Toggle("", sp.active, GUILayout.Width(20));
			sp.id = (SailParamID)EditorGUILayout.EnumPopup("", sp.id, GUILayout.Width(100));

			ShowParam(mod, sp);

			if ( GUILayout.Button("-", GUILayout.Width(16)) )
				return true;

			EditorGUILayout.EndHorizontal();

			return false;
		}

		Texture logoImage;

		public override void OnInspectorGUI()
		{
			Sail mod = (Sail)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width - 0), GUILayout.Height(h1));
			}

			FullSailGUI.Header("Overrides");
			for ( int i = 0; i < mod.sailParams.Count; i++ )
			{
				SailParam sp = mod.sailParams[i];

				SailParamID oldid = sp.id;

				bool changed = false;

				if ( ShowSailParam(target, sp, ref changed) )
				{
					FullSailGUI.RecordObjects(targets, "Removed Sail override");

					for ( int j = 0; j < targets.Length; j++ )
					{
						Sail sail = (Sail)targets[j];
						// find params with same id to remove
						for ( int k = 0; k < sail.sailParams.Count; k++ )
						{
							if ( sail.sailParams[k].id == oldid )
							{
								sail.sailParams.RemoveAt(k);
								break;
							}
						}
					}
				}
				else
				{
					if ( sp.id != oldid )
					{
						for ( int j = 0; j < targets.Length; j++ )
						{
							if ( target != targets[j] )
							{
								Sail sail = (Sail)targets[j];

								for ( int k = 0; k < sail.sailParams.Count; k++ )
								{
									if ( sail.sailParams[k].id == oldid )
									{
										sail.sailParams[k].id = sp.id;
										break;
									}
								}
							}
						}
					}

					if ( changed )
					{
						for ( int j = 0; j < targets.Length; j++ )
						{
							if ( target != targets[j] )
							{
								Sail sail = (Sail)targets[j];

								for ( int k = 0; k < sail.sailParams.Count; k++ )
								{
									if ( sail.sailParams[k].id == mod.sailParams[i].id )
									{
										sail.sailParams[k].fval = mod.sailParams[i].fval;
										sail.sailParams[k].cval = mod.sailParams[i].cval;
										sail.sailParams[k].ival = mod.sailParams[i].ival;
										sail.sailParams[k].tval = mod.sailParams[i].tval;
										sail.sailParams[k].vval = mod.sailParams[i].vval;
									}
								}
							}
						}
					}
				}
			}

			if ( GUILayout.Button("Add Override") )
			{
				FullSailGUI.RecordObjects(targets, "Added Sail override");

				// targets
				for ( int i = 0; i < targets.Length; i++ )
				{
					Sail sail = (Sail)targets[i];

					SailParam sp = new SailParam();
					sp.active	= true;
					sp.cval		= Color.white;
					sail.sailParams.Add(sp);
				}
			}

			// test code
			FullSailGUI.FoldOut(ref mod.showImpacts, "Impacts");

			if ( mod.showImpacts )
			{
				mod.impactCount = EditorGUILayout.IntSlider("Impact Count", mod.impactCount, 0, Sail.maxImpacts);

				for ( int i = 0; i < mod.impactCount; i++ )
				{
					Vector4 pos = mod.impactPoints[i];

					EditorGUILayout.LabelField("Impact " + i);
					EditorGUILayout.BeginVertical("box");

					EditorGUILayout.BeginHorizontal("box");
					EditorGUIUtility.labelWidth = 30.0f;
					pos.x = EditorGUILayout.Slider("X", pos.x, -1.0f, 2.0f);
					pos.y = EditorGUILayout.Slider("Y", pos.y, -1.0f, 2.0f);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal("box");

					EditorGUIUtility.labelWidth = 30.0f;
					pos.z = EditorGUILayout.Slider("Size", pos.z, 0.0f, 1.0f);
					pos.w = EditorGUILayout.Slider("Type", pos.w, 0.0f, 16.0f);
					EditorGUILayout.EndHorizontal();

					mod.impactPoints[i] = pos;

					EditorGUILayout.EndVertical();
				}
			}

			Vector3 nsize = FullSailGUI.Vector3(mod, "Bounds Size", mod.boundsSize, "Use to adjust the sail mesh bounds to stop early culling");

			if ( nsize != mod.boundsSize )
			{
				mod.boundsSize = nsize;

				for ( int i = 0; i < targets.Length; i++ )
				{
					if ( target != targets[i] )
					{
						Sail sail = (Sail)targets[i];

						sail.boundsSize = nsize;
					}
				}
			}

			Vector3 nc = FullSailGUI.Vector3(mod, "Bounds Offset", mod.boundsOffset, "Use to adjust the sail mesh bounds to stop early culling");

			if ( nc != mod.boundsOffset )
			{
				mod.boundsOffset = nc;

				for ( int i = 0; i < targets.Length; i++ )
				{
					if ( target != targets[i] )
					{
						Sail sail = (Sail)targets[i];

						sail.boundsOffset = nc;
					}
				}
			}

			if ( GUI.changed )
			{
				//mod.dirty = true;
				//EditorUtility.SetDirty(mod);
				MySetDirty();
			}
		}

		void MySetDirty()
		{
			Sail mod = (Sail)target;

			mod.dirty = true;
			EditorUtility.SetDirty(mod);
			if ( !Application.isPlaying )
				EditorSceneManager.MarkSceneDirty(mod.gameObject.scene);
		}

		void UpdateTargets()
		{
			Sail ssail = (Sail)target;
			ssail.UpdateBounds();
		}

		void OnSceneGUI()
		{
			Sail sail = (Sail)target;

			Handles.matrix = sail.transform.localToWorldMatrix;
			Handles.color = new Color(1.0f, 0.4f, 0.0f);
			Handles.DrawWireCube(sail.boundsOffset, sail.boundsSize);

			Vector3 p = new Vector3(sail.boundsOffset.x + (sail.boundsSize.x * 0.5f), sail.boundsOffset.y, sail.boundsOffset.z);
			Vector3 np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dx = np.x - p.x;
				sail.boundsOffset.x += dx * 0.5f;
				sail.boundsSize.x += dx;// * 0.5f;
				UpdateTargets();
			}

			p = new Vector3(sail.boundsOffset.x - (sail.boundsSize.x * 0.5f), sail.boundsOffset.y, sail.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dx = np.x - p.x;
				sail.boundsOffset.x += dx * 0.5f;
				sail.boundsSize.x -= dx;
				UpdateTargets();
			}

			p = new Vector3(sail.boundsOffset.x, sail.boundsOffset.y + (sail.boundsSize.y * 0.5f), sail.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dy = np.y - p.y;
				sail.boundsOffset.y += dy * 0.5f;
				sail.boundsSize.y += dy;// * 0.5f;
				UpdateTargets();
			}

			p = new Vector3(sail.boundsOffset.x, sail.boundsOffset.y - (sail.boundsSize.y * 0.5f), sail.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dy = np.y - p.y;
				sail.boundsOffset.y += dy * 0.5f;
				sail.boundsSize.y -= dy;
				UpdateTargets();
			}

			p = new Vector3(sail.boundsOffset.x, sail.boundsOffset.y, sail.boundsOffset.z + (sail.boundsSize.z * 0.5f));
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dz = np.z - p.z;
				sail.boundsOffset.z += dz * 0.5f;
				sail.boundsSize.z += dz;// * 0.5f;
				UpdateTargets();
			}

			p = new Vector3(sail.boundsOffset.x, sail.boundsOffset.y, sail.boundsOffset.z - (sail.boundsSize.z * 0.5f));
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dz = np.z - p.z;
				sail.boundsOffset.z += dz * 0.5f;
				sail.boundsSize.z -= dz;
				UpdateTargets();
			}
		}
	}
}