using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FullRig
{
	[CustomEditor(typeof(Rope))]
	[CanEditMultipleObjects]
	public class RopeEditor : Editor
	{
		public enum MoveMode
		{
			None,
			Rope,
			End,
		}

		Texture			logoImage;
		List<RopeEnd>	ropeEnds	= new List<RopeEnd>();
		List<RopeType>	ropeTypes	= new List<RopeType>();
		Vector3			eposd;
		Vector3			sposd;
		int				eaxis		= -1;
		int				saxis		= -1;
		float			ea;
		float			sa;
		Transform[]		snapTargets;
		Rope[]			ropes;
		Vector3[]		ropeMidPoints;
		Transform		moveTarget;
		int				moveIsRope	= -1;
		bool			snapObj		= false;
		bool			snapSel		= false;
		bool			leftShift	= false;
		Vector3			objPos;
		Vector3			downEndPos;
		MoveMode		mode		= MoveMode.None;
		float			lengthDown;

		SerializedProperty _widthStart;
		SerializedProperty _widthEnd;
		SerializedProperty _length;
		SerializedProperty _stretch;
		SerializedProperty _ropeForce;
		SerializedProperty _swingAngle;
		SerializedProperty _swingFreq;
		SerializedProperty _swingOffset;
		SerializedProperty _startAttach;
		SerializedProperty _endAttach;
		SerializedProperty _ropeStart;
		SerializedProperty _ropeEnd;
		SerializedProperty _adjustZ;
		SerializedProperty _adjustY;
		SerializedProperty _adjustZEnd;
		SerializedProperty _adjustYEnd;
		SerializedProperty _startConnect;
		SerializedProperty _endConnect;
		SerializedProperty _attached;
		SerializedProperty _ropeStartAttach;
		SerializedProperty _ropeEndAttach;

		private void OnEnable()
		{
			_widthStart			= serializedObject.FindProperty("widthStart");
			_widthEnd			= serializedObject.FindProperty("widthEnd");
			_length				= serializedObject.FindProperty("length");
			_stretch			= serializedObject.FindProperty("stretch");
			_ropeForce			= serializedObject.FindProperty("ropeForce");
			_swingAngle			= serializedObject.FindProperty("swingAngle");
			_swingFreq			= serializedObject.FindProperty("swingFreq");
			_swingOffset		= serializedObject.FindProperty("swingOffset");
			_startAttach		= serializedObject.FindProperty("startAttach");
			_endAttach			= serializedObject.FindProperty("endAttach");
			_ropeStart			= serializedObject.FindProperty("ropeStart");
			_ropeEnd			= serializedObject.FindProperty("ropeEnd");
			_adjustZ			= serializedObject.FindProperty("adjustZ");
			_adjustY			= serializedObject.FindProperty("adjustY");
			_adjustZEnd			= serializedObject.FindProperty("adjustZEnd");
			_adjustYEnd			= serializedObject.FindProperty("adjustYEnd");
			_startConnect		= serializedObject.FindProperty("startConnect");
			_endConnect			= serializedObject.FindProperty("endConnect");
			_attached			= serializedObject.FindProperty("attached");
			_ropeStartAttach	= serializedObject.FindProperty("ropeStartAttach");
			_ropeEndAttach		= serializedObject.FindProperty("ropeEndAttach");
		}

		public override void OnInspectorGUI()
		{
			Rope mod = (Rope)target;

			serializedObject.Update();

			GetTargets();

			if ( ropeEnds == null || ropeEnds.Count == 0 )
			{
				string[] guids = AssetDatabase.FindAssets("t:RopeEnd");

				ropeEnds.Add(null);
				for ( int j = 0; j < guids.Length; j++ )
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[j]);

					RopeEnd end = (RopeEnd)AssetDatabase.LoadAssetAtPath(path, typeof(RopeEnd));

					if ( end )
						ropeEnds.Add(end);
				}
			}

			if ( ropeTypes == null || ropeTypes.Count == 0 )
			{
				string[] guids = AssetDatabase.FindAssets("t:RopeType");

				for ( int j = 0; j < guids.Length; j++ )
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[j]);

					RopeType rope = (RopeType)AssetDatabase.LoadAssetAtPath(path, typeof(RopeType));

					if ( rope )
						ropeTypes.Add(rope);
				}
			}

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullRigLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			FullRigGUI.Header("Rope");

			if ( GUILayout.Button("Set Length") )
			{
				FullRigGUI.RecordObject(mod, "Set Length");
				mod.SetLength();
			}

			FullRigGUI.Property("Length",					_length,			"Length of the Rope");
			EditorGUILayout.LabelField("Current Arc Length " + mod.GetArcLength().ToString("0.000") + " Dist " + mod.GetDist().ToString("0.000") + " dir " + mod.GetForceDir());

			//Rope.iterations = EditorGUILayout.IntSlider("Iterations", Rope.iterations, 4, 64);
			//Rope.minIterations = EditorGUILayout.IntSlider("Min Iterations", Rope.minIterations, 4, 64);

			FullRigGUI.Property("Stretch",					_stretch,			"Adjust the length of the rope when it is sagging");
			FullRigGUI.Property("Rope Force",				_ropeForce,			"If rope is connected to a rigidbody this controls the force applied when the rope is stretched");

			FullRigGUI.Header("Swing");
			FullRigGUI.Property("Swing Angle",				_swingAngle,		"How Far the rope will swing");
			FullRigGUI.Property("Swing Freq",				_swingFreq,			"How Fast the rope will swing");
			FullRigGUI.Property("Swing Offset",				_swingOffset,		"Offsets the swing animation so the ropes dont all swing the same");

			FullRigGUI.Header("Start");
			FullRigGUI.Property("Start Attach",				_startAttach,		"The object the start of the rope is attached to.");

			if ( !mod.startAttach )
				GUI.enabled = false;
			if ( GUILayout.Button("Disconnect") )
			{
				mod.SetStartAttach(null);
				mod.SetDirty();
				MySetDirty();
			}
			GUI.enabled = true;

			FullRigGUI.Property("Rope Start",				_ropeStartAttach,	"Adjusts where the rope mesh starts");
			FullRigGUI.Property("Start Width",				_widthStart,		"The width of the rope at the start, adds to any end object width value");
			FullRigGUI.Property("Start Connect",			_startConnect,		"Rigging connection object to add to start of the rope");
			FullRigGUI.Property("Rope Pos Start",			_ropeStart,			"If attached to a rope the position on the rope for the start");
			FullRigGUI.Property("Horizontal Strand Adjust", _adjustZ,			"Adjust the horizontal spacing of the rope strans if more than one");
			FullRigGUI.Property("Vertical Strand Adjust",	_adjustY,			"Adjust the vertical spacing of the rope strans if more than one");

			FullRigGUI.Header("End");

			FullRigGUI.Property("End Attach",				_endAttach,			"The object the start of the rope is attached to.");

			if ( GUILayout.Button("Disconnect") )
			{
				mod.SetEndAttach(null);
				mod.SetDirty();
				MySetDirty();
			}
			GUI.enabled = true;

			FullRigGUI.Property("Rope End",					_ropeEndAttach, "Adjusts where the rope mesh ends");
			FullRigGUI.Property("End Width",				_widthEnd,		"The width of the rope at the end, adds to any end object width value");
			FullRigGUI.Property("End Connect",				_endConnect,	"Rigging connection object to add to start of the rope");
			FullRigGUI.Property("Rope Start",				_ropeEnd,		"If end attached to a rope this is the position on that rope");
			FullRigGUI.Property("Horizontal Strand Adjust",	_adjustZEnd,	"Adjust the horizontal spacing of the rope strans if more than one");
			FullRigGUI.Property("Vertical Strand Adjust",	_adjustYEnd,	"Adjust the vertical spacing of the rope strans if more than one");

			FullRigGUI.Header("Attached Objects");

			FullRigGUI.Property("", _attached, "");

			if ( mod.startConnect )
			{
				FullRigGUI.FoldOut(ref mod.showStartConnect, "Start Connection");
				if ( mod.showStartConnect )
				{
					RopeEnd re = mod.startConnect;
					FullRigGUI.Float(mod,			"Connect Tangent",	ref re.tangent,		"The distance down the rope the end of the connect object aligns to.");
					FullRigGUI.Float(mod,			"Rope Start",		ref re.ropeStart,	"The distance from the end the rope mesh stops");
					FullRigGUI.Float(mod,			"Width",			ref re.width,		"The width of the rope that connects to this object");
					FullRigGUI.Float(mod,			"Adjust Z",			ref re.adjustZ,		"Adjusts the horizontal spacing of the strands on a multi strand rope");
					FullRigGUI.Float(mod,			"Adjust Y",			ref re.adjustY,		"Adjusts the vertical spacing of the strands of a multi strand rope");
					FullRigGUI.RopeNumFlags(mod,	"Rope Type",		ref re.ropeType,	"Which rope types this can be added to");
				}
			}

			if ( mod.endConnect )
			{
				FullRigGUI.FoldOut(ref mod.showStartConnect, "End Connection");
				if ( mod.showStartConnect )
				{
					RopeEnd re = mod.endConnect;
					FullRigGUI.Float(mod,			"Connect Tangent",	ref re.tangent,		"The distance down the rope the end of the connect object aligns to.");
					FullRigGUI.Float(mod,			"Rope Start",		ref re.ropeStart,	"The distance from the end the rope mesh stops");
					FullRigGUI.Float(mod,			"Width",			ref re.width,		"The width of the rope that connects to this object");
					FullRigGUI.Float(mod,			"Adjust Z",			ref re.adjustZ,		"Adjusts the horizontal spacing of the strands on a multi strand rope");
					FullRigGUI.Float(mod,			"Adjust Y",			ref re.adjustY,		"Adjusts the vertical spacing of the strands of a multi strand rope");
					FullRigGUI.RopeNumFlags(mod,	"Rope Type",		ref re.ropeType,	"Which rope types this can be added to");
				}
			}

			if ( GUI.changed )
			{
				for ( int i = 0; i < targets.Length; i++ )
				{
					Rope r = (Rope)targets[i];
					r.SetDirty();
					MySetDirty(r);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		void MySetDirty()
		{
			EditorUtility.SetDirty(target);
			FullRigGUI.SetDirty(target);
		}

		void MySetDirty(Object targ)
		{
			EditorUtility.SetDirty(targ);
			FullRigGUI.SetDirty(targ);
		}

		int MaxComI(Vector3 val)
		{
			if ( Mathf.Abs(val.x) > Mathf.Abs(val.y) )
			{
				if ( Mathf.Abs(val.x) > Mathf.Abs(val.z) )
					return 0;
				else
					return 2;
			}
			else
			{
				if ( Mathf.Abs(val.y) > Mathf.Abs(val.z) )
					return 1;
				else
					return 2;
			}
		}

		// Also check for mount pos component and use positions found in there
		void GetTargets()
		{
			if ( snapTargets == null || snapTargets.Length == 0 || ropes == null || ropes.Length == 0 )
			{
				snapTargets = FindObjectsOfType<Transform>();
				moveTarget = null;
				moveIsRope = -1;

				ropes = FindObjectsOfType<Rope>();
				ropeMidPoints = new Vector3[ropes.Length];

				for ( int i = 0; i < ropes.Length; i++ )
					ropeMidPoints[i] = ropes[i].transform.TransformPoint(ropes[i].CalcPos(0.5f));
			}
		}

		void DisplayTargets()
		{
			Rope rope = (Rope)target;

			if ( snapObj || snapSel )
			{
				// Display snap targets
				Handles.matrix = Matrix4x4.identity;
				Handles.color = Color.white;

				for ( int i = 0; i < snapTargets.Length; i++ )
				{
					if ( snapTargets[i] != rope.startAttach && snapTargets[i] != rope.endAttach && snapTargets[i] != rope.startObj && snapTargets[i] != rope.endObj && snapTargets[i] != rope.transform )
						Handles.DrawWireDisc(snapTargets[i].position, Vector3.forward, 0.1f, 2.0f);
				}

				for ( int i = 0; i < ropeMidPoints.Length; i++ )
				{
					if ( ropes[i] != rope )
						Handles.DrawWireDisc(ropeMidPoints[i], Vector3.forward, 0.1f, 2.0f);
				}
			}
		}

		List<Transform>	samePos = new List<Transform>();
		int sameCount;

		bool FindTarget(Vector3 sp)
		{
			Rope rope = (Rope)target;

			moveTarget = null;
			moveIsRope = -1;

			float closest = float.MaxValue;

			Camera sceneCamera = SceneView.lastActiveSceneView.camera;

			samePos.Clear();

			for ( int i = 0; i < snapTargets.Length; i++ )
			{
				if ( snapTargets[i] != rope.startAttach && snapTargets[i] != rope.endAttach && snapTargets[i] != rope.startObj && snapTargets[i] != rope.endObj && snapTargets[i] != rope.transform )
				{
					Vector3 tp = sceneCamera.WorldToScreenPoint(snapTargets[i].position);
					float dist = Vector3.Distance(sp, tp);

					if ( dist <= closest )
					{
						if ( dist < 32.0f )
						{
							if ( dist.Equals(closest) )
								samePos.Add(snapTargets[i]);
							else
							{
								samePos.Clear();
								samePos.Add(snapTargets[i]);
							}
							moveTarget = snapTargets[i];
						}
						closest = dist;
					}
				}
			}

			for ( int i = 0; i < ropeMidPoints.Length; i++ )
			{
				if ( ropes[i] != rope )
				{
					Vector3 tp = sceneCamera.WorldToScreenPoint(ropeMidPoints[i]);
					float dist = Vector3.Distance(sp, tp);

					if ( dist < closest )
					{
						closest = dist;

						if ( dist < 32.0f )
						{
							moveTarget = ropes[i].transform;
							moveIsRope = i;
						}
					}
				}
			}

			if ( moveIsRope == -1 )
			{
				if ( samePos.Count > 0 )
				{
					int c = sameCount;
					while ( c >= samePos.Count )
						c -= samePos.Count;

					moveTarget = samePos[c];
				}
			}

			if ( moveTarget || moveIsRope >= 0 )
				return true;

			return false;
		}

		bool moveBoth = false;
		Vector3	endOff;

		void OnSceneGUINew()
		{
			Rope rope = (Rope)target;

			if ( Tools.current == Tool.Move )
			{
				switch ( Event.current.type )
				{
					case EventType.KeyDown:
						if ( Event.current.keyCode == KeyCode.B )
						{
							moveBoth = true;
						}

						if ( Event.current.keyCode == KeyCode.A )
						{
							if ( rope.startAttach )
							{
								FullRigGUI.RecordObject(rope, "Disconnect");

								Rope er = rope.startAttach?.GetComponent<Rope>();
								if ( er )
								{
									rope.SetStartAttach(null);
									MySetDirty();
									rope.SetDirty();
								}
								else
								{
									rope.SetStartAttach(null);
									MySetDirty();
									rope.SetDirty();
								}
							}
							else
							{
								Camera sceneCamera = SceneView.lastActiveSceneView.camera;

								Vector3 cpos = rope.transform.position;
								Vector3 sp = sceneCamera.WorldToScreenPoint(cpos);

								if ( FindTarget(sp) )
								{
									FullRigGUI.RecordObject(rope, "Connect");

									if ( moveIsRope >= 0 )
									{
										rope.SetStartAttach(ropes[moveIsRope].transform);
										rope.ropeStart = 0.5f;
									}
									else
										rope.transform.position = moveTarget.position;

									rope.startAttach = moveTarget;
								}
							}

							Event.current.Use();
						}

						if ( Event.current.keyCode == KeyCode.S )
						{
							if ( rope.endAttach )
							{
								FullRigGUI.RecordObject(rope, "Disconnect");

								Rope er = rope.endAttach?.GetComponent<Rope>();
								if ( er )
								{
									rope.endPos = er.transform.TransformPoint(er.CalcPos(rope.ropeEnd));
									rope.SetEndAttach(null);
									MySetDirty();
									rope.SetDirty();
								}
								else
								{
									rope.endPos = rope.endAttach.position;
									rope.SetEndAttach(null);
									MySetDirty();
									rope.SetDirty();
								}
							}
							else
							{
								Camera sceneCamera = SceneView.lastActiveSceneView.camera;

								Vector3 cpos = rope.endPos;
								Vector3 sp = sceneCamera.WorldToScreenPoint(cpos);

								if ( FindTarget(sp) )
								{
									FullRigGUI.RecordObject(rope, "Connect");

									if ( moveIsRope >= 0 )
									{
										rope.SetEndAttach(ropes[moveIsRope].transform);
										rope.ropeEnd = 0.5f;
									}
									else
										rope.endPos = moveTarget.position;

									rope.endAttach = moveTarget;
								}
							}
							Event.current.Use();
						}


						//if ( Event.current.keyCode == KeyCode.Space )
							//snapObj = true;

						if ( Event.current.keyCode == KeyCode.X )
							snapSel = true;

						if ( Event.current.keyCode == KeyCode.C )
						{
							sameCount++;
							Event.current.Use();
						}
						break;

					case EventType.KeyUp:
						if ( Event.current.keyCode == KeyCode.B )
							moveBoth = false;

						if ( Event.current.keyCode == KeyCode.C )
						{

						}
						else
						{
							snapObj = false;
							snapSel = false;
						}
						break;

					case EventType.MouseDown:
						objPos		= rope.transform.position;
						downEndPos	= rope.endPos;
						mode		= MoveMode.None;
						lengthDown	= rope.length;
						endOff		= downEndPos - objPos;
						break;

					case EventType.MouseDrag:
						if ( mode == MoveMode.None )
						{
							if ( objPos != rope.transform.position )
								mode = MoveMode.Rope;
							else
							{
								if ( downEndPos != rope.endPos )
									mode = MoveMode.End;
							}
						}

						if ( moveBoth )
						{
							if ( mode == MoveMode.Rope )
								rope.endPos = rope.transform.position + endOff;
							else
							{
								if ( mode == MoveMode.End )
									rope.transform.position = rope.endPos - endOff;
							}
						}
						break;

					case EventType.MouseUp:
						if ( moveTarget )
						{
							FullRigGUI.RecordObject(rope, "Connection");
							Vector3 tpos = moveTarget.position;
							if ( moveIsRope >= 0 )
								tpos = ropeMidPoints[moveIsRope];

							switch ( mode )
							{
								case MoveMode.Rope:
									if ( snapObj )
										rope.transform.position = tpos;
									else
									{
										if ( snapSel )
										{
											if ( moveIsRope >= 0 )
											{
												rope.SetStartAttach(ropes[moveIsRope].transform);
												rope.ropeStart = 0.5f;
											}
											else
												rope.transform.position = moveTarget.position;

											rope.startAttach = moveTarget;
										}
									}
									break;

								case MoveMode.End:
									if ( snapObj )
										rope.endPos = tpos;
									else
									{
										if ( snapSel )
										{
											if ( moveIsRope >= 0 )
											{
												rope.SetEndAttach(ropes[moveIsRope].transform);
												rope.ropeEnd = 0.5f;
											}
											else
												rope.endPos = moveTarget.position;

											rope.endAttach = moveTarget;
										}
									}
									break;
							}

							moveTarget = null;
							moveIsRope = -1;
						}

						mode = MoveMode.None;
						rope.SetDirty();
						MySetDirty();
						break;
				}

				if ( !rope.endAttach )
				{
					Vector3 endPos = Handles.PositionHandle(rope.endPos, Quaternion.identity);
					if ( endPos != rope.endPos )
					{
						//if ( moveBoth )
						//{
							//Vector3 delta = endPos - rope.endPos;
							//rope.transform.position += delta;
						//}
						rope.endPos = endPos;
						rope.SetDirty();
						MySetDirty();
					}
				}

				if ( snapObj || snapSel )
				{
					Camera sceneCamera = SceneView.lastActiveSceneView.camera;

					// Display snap targets
					DisplayTargets();

					Vector3 cpos = rope.transform.position;
					if ( mode == MoveMode.End )
						cpos = rope.endPos;

					Vector3 sp = sceneCamera.WorldToScreenPoint(cpos);

					if ( FindTarget(sp) )
					{
						Handles.matrix = Matrix4x4.identity;
						Handles.color = Color.white;
						Vector3 tp = moveTarget.position;
						if ( moveIsRope >= 0 )
							tp = ropeMidPoints[moveIsRope];

						string multi = "";
						if ( samePos.Count > 1 )
							multi = " [Found " + samePos.Count + " targets at Same Location 'C' cycle]";

						Handles.DrawDottedLine(cpos, tp, 8.0f);
						if ( snapObj )
							Handles.Label(tp, "Snap to " + moveTarget.name + multi);

						if ( snapSel )
							Handles.Label(tp, "Set Target " + moveTarget.name + multi);
					}
				}
			}
		}

		private void OnDisable()
		{
			Tools.hidden = false;
		}

		void OnSceneGUI()
		{
			Rope rope = (Rope)target;
			rope.isPrefab = false;

			OnSceneGUINew();

			Handles.matrix = rope.transform.localToWorldMatrix;
			Handles.color = new Color(1.0f, 1.0f, 1.0f);

			//Bounds b = rope.GetComponent<MeshFilter>().sharedMesh.bounds;
			//Handles.DrawWireCube(b.center, b.size);

			Vector3 up;
			if ( rope.endConnect )
			{
				Vector3 ep = rope.CalcPosFromDist(rope.endConnect.tangent, false, out up);
				Handles.DotHandleCap(0, ep, Quaternion.identity, 0.02f, EventType.Repaint);
			}

			if ( rope.startConnect )
			{
				Vector3 ep = rope.CalcPosFromDist(rope.startConnect.tangent, true, out up);
				Handles.DotHandleCap(0, ep, Quaternion.identity, 0.02f, EventType.Repaint);
			}

			Handles.color = new Color(1.0f, 0.4f, 0.0f);

			if ( rope.startAttach != null && rope.startAttach != rope.transform && Tools.current == Tool.Move )
				Tools.hidden = true;
			else
				Tools.hidden = false;

			switch ( Event.current.type )
			{
				case EventType.KeyDown:
					if ( Event.current.keyCode == KeyCode.LeftShift )
						leftShift = true;

					if ( ropeEnds != null && ropeEnds.Count > 0 )
					{
						if ( Event.current.keyCode == KeyCode.Alpha1 )
						{
							Event.current.Use();
							if ( leftShift )
							{
								rope.startIndex--;
								if ( rope.startIndex < 0 )
									rope.startIndex = ropeEnds.Count - 1;
							}
							else
							{
								rope.startIndex++;
								if ( rope.startIndex >= ropeEnds.Count )
									rope.startIndex = 0;
							}

							if ( PrefabUtility.GetPrefabAssetType(rope.gameObject) != PrefabAssetType.Regular )
							{
								rope.SetEndObject(ropeEnds[rope.startIndex], true);
								MySetDirty();
							}
							else
								Debug.LogWarning("Cant Change ends on a prefab, Unpack first");
						}

						if ( Event.current.keyCode == KeyCode.Alpha2 )
						{
							Event.current.Use();
							if ( leftShift )
							{
								rope.endIndex--;
								if ( rope.endIndex < 0 )
									rope.endIndex = ropeEnds.Count - 1;
							}
							else
							{
								rope.endIndex++;
								if ( rope.endIndex >= ropeEnds.Count )
									rope.endIndex = 0;
							}

							if ( PrefabUtility.GetPrefabAssetType(rope.gameObject) != PrefabAssetType.Regular )
							{
								rope.SetEndObject(ropeEnds[rope.endIndex], false);
								MySetDirty();
							}
							else
								Debug.LogWarning("Cant Change ends on a prefab, Unpack first");
						}
					}

					if ( Event.current.keyCode == KeyCode.Alpha3 )
					{
						Event.current.Use();
						if ( leftShift )
						{
							rope.ropeMeshIndex--;
							if ( rope.ropeMeshIndex < 0 )
								rope.ropeMeshIndex = ropeTypes.Count - 1;
						}
						else
						{
							rope.ropeMeshIndex++;
							if ( rope.ropeMeshIndex >= ropeTypes.Count )
								rope.ropeMeshIndex = 0;
						}

						MeshFilter mf = rope.GetComponent<MeshFilter>();
						if ( mf )
						{
							mf.sharedMesh = ropeTypes[rope.ropeMeshIndex].mesh;
							rope.UpdateBounds();
						}

						MeshRenderer mr = rope.GetComponent<MeshRenderer>();
						if ( mr )
							mr.sharedMaterial = ropeTypes[rope.ropeMeshIndex].material;

						rope.SetDirty();
						MySetDirty();
					}
					break;

				case EventType.KeyUp:
					if ( Event.current.keyCode == KeyCode.LeftShift )
						leftShift = false;
					break;
			}

			Handles.color = new Color(1.0f, 0.4f, 0.0f); 
			Handles.matrix = Matrix4x4.identity;	//rope.transform.localToWorldMatrix;

			Vector3 pos = rope.transform.position;
			Quaternion rot = rope.transform.rotation;
			float hs = HandleUtility.GetHandleSize(pos) * 2.0f;
			float stretch = Handles.ScaleSlider(rope.stretch, pos, -Vector3.forward, rot, hs, 0.0f);
			Handles.Label(rope.transform.position - Vector3.forward * hs, "Stretch");
			if ( stretch != rope.stretch )
			{
				rope.stretch = stretch;
				rope.SetDirty();
				MySetDirty();
			}

			float length = Handles.ScaleSlider(lengthDown, pos, -Vector3.up, rot, hs, 0.0f);
			Handles.Label(pos - Vector3.up * hs, "Length");
			if ( length != lengthDown )
			{
				rope.length = lengthDown + ((length - lengthDown) * 0.1f);
				rope.SetDirty();
				MySetDirty();
			}

			float width = Handles.ScaleSlider(rope.widthStart + 1.0f, pos, -Vector3.right, rot, hs, 0.0f);
			Handles.Label(pos - Vector3.right * hs, "Width");
			if ( width != rope.widthStart + 1.0f )
			{
				rope.widthStart = width - 1.0f;
				rope.widthEnd = rope.widthStart;
				rope.SetDirty();
				MySetDirty();
			}

#if false
			Handles.matrix = rope.transform.localToWorldMatrix;
			Vector3 p = new Vector3(rope.boundsOffset.x + (rope.boundsSize.x * 0.5f), rope.boundsOffset.y, rope.boundsOffset.z);
			Vector3 np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dx = np.x - p.x;
				rope.boundsOffset.x += dx * 0.5f;
				rope.boundsSize.x += dx;
				rope.UpdateBounds();
				MySetDirty();
			}

			p = new Vector3(rope.boundsOffset.x - (rope.boundsSize.x * 0.5f), rope.boundsOffset.y, rope.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dx = np.x - p.x;
				rope.boundsOffset.x += dx * 0.5f;
				rope.boundsSize.x -= dx;
				rope.UpdateBounds();
				MySetDirty();
			}

			p = new Vector3(rope.boundsOffset.x, rope.boundsOffset.y + (rope.boundsSize.y * 0.5f), rope.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dy = np.y - p.y;
				rope.boundsOffset.y += dy * 0.5f;
				rope.boundsSize.y += dy;
				rope.UpdateBounds();
				MySetDirty();
			}

			p = new Vector3(rope.boundsOffset.x, rope.boundsOffset.y - (rope.boundsSize.y * 0.5f), rope.boundsOffset.z);
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dy = np.y - p.y;
				rope.boundsOffset.y += dy * 0.5f;
				rope.boundsSize.y -= dy;
				rope.UpdateBounds();
				MySetDirty();
			}

			p = new Vector3(rope.boundsOffset.x, rope.boundsOffset.y, rope.boundsOffset.z + (rope.boundsSize.z * 0.5f));
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dz = np.z - p.z;
				rope.boundsOffset.z += dz * 0.5f;
				rope.boundsSize.z += dz;
				rope.UpdateBounds();
				MySetDirty();
			}

			p = new Vector3(rope.boundsOffset.x, rope.boundsOffset.y, rope.boundsOffset.z - (rope.boundsSize.z * 0.5f));
			np = Handles.FreeMoveHandle(p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);

			if ( np != p )
			{
				float dz = np.z - p.z;
				rope.boundsOffset.z += dz * 0.5f;
				rope.boundsSize.z -= dz;
				rope.UpdateBounds();
				MySetDirty();
			}
#endif

			float disconOff = 0.1f;
			float disconSize = 0.25f;

			if ( rope.endAttach )
			{
				Rope er = rope.endAttach.GetComponent<Rope>();

				if ( er )
				{
					switch ( Event.current.type )
					{
						case EventType.MouseDown:
							eposd = rope.transform.InverseTransformPoint(er.transform.TransformPoint(er.CalcPos(rope.ropeEnd)));
							ea = rope.ropeEnd;
							eaxis = -1;
							break;
					}

					Handles.color = Color.green;
					Vector3 epos = rope.transform.InverseTransformPoint(er.transform.TransformPoint(er.CalcPos(rope.ropeEnd)));

					Handles.matrix = rope.transform.localToWorldMatrix;
					Handles.Label(epos, "  Move");
					Vector3 nepos = Handles.FreeMoveHandle(epos, Quaternion.identity, HandleUtility.GetHandleSize(epos) * disconSize, Vector3.zero, Handles.SphereHandleCap);
					if ( nepos != epos )
					{
						Vector3 delta = nepos - eposd;
						if ( eaxis == -1 )
							eaxis = MaxComI(delta);

						float dx = delta[eaxis];//MaxCom(delta);//nepos.x - sposd.x;
						rope.ropeEnd = ea - (dx / 10.0f);
						rope.ropeEnd = Mathf.Clamp01(rope.ropeEnd);

						MySetDirty();
					}

					Handles.matrix = Matrix4x4.identity;
					Vector3 ep = rope.transform.TransformPoint(epos);
					ep.y += disconOff;
					Handles.Label(ep, "  Disconnect");
					if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(ep) * disconSize, HandleUtility.GetHandleSize(ep) * disconSize, Handles.SphereHandleCap) )
					{
						FullRigGUI.RecordObject(rope, "Disconnect");
						rope.endPos = er.transform.TransformPoint(er.CalcPos(rope.ropeEnd));
						rope.SetEndAttach(null);
						MySetDirty();
						rope.SetDirty();
					}
				}
				else
				{
					Handles.matrix = Matrix4x4.identity;
					Vector3 ep = rope.endAttach.position;
					Handles.Label(ep, "  Disconnect");
					if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(rope.endAttach.position) * disconSize, 0.0f, Handles.SphereHandleCap) )
					{
						FullRigGUI.RecordObject(rope, "Disconnect");
						rope.endPos = rope.endAttach.position;
						rope.SetEndAttach(null);
						MySetDirty();
						rope.SetDirty();
					}
				}
			}

			if ( rope.startAttach && rope.startAttach != rope.transform )
			{
				Rope er = rope.startAttach.GetComponent<Rope>();

				if ( er )
				{
					switch ( Event.current.type )
					{
						case EventType.MouseDown:
							sposd = rope.transform.InverseTransformPoint(er.transform.TransformPoint(er.CalcPos(rope.ropeStart)));
							sa = rope.ropeStart;
							saxis = -1;
							break;
					}

					Handles.matrix = rope.transform.localToWorldMatrix;

					Handles.color = Color.red;
					Vector3 epos = rope.transform.InverseTransformPoint(er.transform.TransformPoint(er.CalcPos(rope.ropeStart)));

					Handles.Label(epos, "  Move");
					Vector3 nepos = Handles.FreeMoveHandle(epos, Quaternion.identity, HandleUtility.GetHandleSize(epos) * disconSize, Vector3.zero, Handles.SphereHandleCap);
					if ( nepos != epos )
					{
						Vector3 delta = nepos - sposd;

						if ( saxis == -1 )
							saxis = MaxComI(delta);

						float dx = delta[saxis];

						rope.ropeStart = sa - (dx / 10.0f);
						rope.ropeStart = Mathf.Clamp01(rope.ropeStart);

						MySetDirty();
					}

					Handles.matrix = Matrix4x4.identity;	//rope.transform.localToWorldMatrix;

					Vector3 ep = rope.transform.TransformPoint(epos);
					ep.y += disconOff;
					Handles.Label(ep, "  Disconnect");
					if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(ep) * disconSize, 0.0f, Handles.SphereHandleCap) )
					{
						FullRigGUI.RecordObject(rope, "Disconnect");
						rope.SetStartAttach(null);
						MySetDirty();
						rope.SetDirty();
					}
				}
				else
				{
					Handles.matrix = Matrix4x4.identity;
					Vector3 ep = rope.startAttach.position;
					ep.y += disconOff;
					Handles.Label(ep, "  Disconnect");
					if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(rope.startAttach.position) * disconSize, 0.0f, Handles.SphereHandleCap) )
					{
						FullRigGUI.RecordObject(rope, "Disconnect");
						rope.SetStartAttach(null);
						MySetDirty();
						rope.SetDirty();
					}
				}
			}
		}
	}
}