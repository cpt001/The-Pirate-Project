using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FullRig
{
	[CustomEditor(typeof(RopeLanyard))]
	public class RopeLanyardEditor : Editor
	{
		public enum MoveMode
		{
			None,
			Start,
			End,
		}

		Transform[]	snapTargets;
		Transform	moveTarget;
		bool		snapObj;
		bool		snapSel;
		MoveMode	mode	= MoveMode.None;
		Vector3		objPos;
		Vector3		downEndPos;

		public override void OnInspectorGUI()
		{
			RopeLanyard mod = (RopeLanyard)target;

			GetTargets();

			if ( GUILayout.Button("Skin") )
				MakeSkinMesh(mod);

			if ( GUILayout.Button("UnSkin") )
				UnSkin(mod);

			DrawDefaultInspector();

			if ( GUI.changed )
				EditorUtility.SetDirty(target);
		}

		void MySetDirty()
		{
			EditorUtility.SetDirty(target);
			FullRigGUI.SetDirty(target);
		}

		void GetTargets()
		{
			if ( snapTargets == null || snapTargets.Length == 0 )
			{
				snapTargets = FindObjectsOfType<Transform>();
				moveTarget = null;
			}
		}

		void DisplayTargets()
		{
			RopeLanyard rope = (RopeLanyard)target;

			if ( snapObj || snapSel )
			{
				// Display snap targets
				Handles.matrix = Matrix4x4.identity;
				Handles.color = Color.white;

				for ( int i = 0; i < snapTargets.Length; i++ )
				{
					Transform targ = snapTargets[i];
					if ( targ != rope.startAttach && targ != rope.endAttach && targ != rope.deadeyeStart && targ != rope.deadeyeEnd && targ != rope.lanyard && targ != rope.transform )
						Handles.DrawWireDisc(snapTargets[i].position, Vector3.forward, 0.1f, 2.0f);
				}
			}
		}

		bool FindTarget(Vector3 sp)
		{
			RopeLanyard rope = (RopeLanyard)target;

			moveTarget = null;

			float closest = float.MaxValue;

			Camera sceneCamera = SceneView.lastActiveSceneView.camera;

			for ( int i = 0; i < snapTargets.Length; i++ )
			{
				Transform targ = snapTargets[i];
				if ( targ != rope.startAttach && targ != rope.endAttach && targ != rope.deadeyeStart && targ != rope.deadeyeEnd && targ != rope.lanyard && targ != rope.transform )
				{
					Vector3 tp = sceneCamera.WorldToScreenPoint(snapTargets[i].position);
					float dist = Vector3.Distance(sp, tp);

					if ( dist < closest )
					{
						closest = dist;

						if ( dist < 32.0f )
							moveTarget = snapTargets[i];
					}
				}
			}

			if ( moveTarget )
				return true;

			return false;
		}

		void OnSceneGUINew()
		{
			RopeLanyard rope = (RopeLanyard)target;

			if ( Tools.current == Tool.Move )
			{
				switch ( Event.current.type )
				{
					case EventType.KeyDown:
						if ( Event.current.keyCode == KeyCode.Space )
							snapObj = true;

						if ( Event.current.keyCode == KeyCode.X )
							snapSel = true;
						break;

					case EventType.KeyUp:
						snapObj = false;
						snapSel = false;
						break;

					case EventType.MouseDown:
						objPos = rope.deadeyeStart.position;
						downEndPos = rope.deadeyeEnd.position;
						break;

					case EventType.MouseDrag:
						if ( mode == MoveMode.None )
						{
							if ( objPos != rope.deadeyeStart.position )
								mode = MoveMode.Start;
							else
							{
								if ( downEndPos != rope.deadeyeEnd.position )
									mode = MoveMode.End;
							}
						}
						break;

					case EventType.MouseUp:
						if ( moveTarget )
						{
							Vector3 tpos = moveTarget.position;

							switch ( mode )
							{
								case MoveMode.Start:
									if ( snapObj )
										rope.deadeyeStart.position = tpos;
									else
									{
										if ( snapSel )
										{
											rope.deadeyeStart.position = moveTarget.position;
											rope.startAttach = moveTarget;
										}
									}
									break;

								case MoveMode.End:
									if ( snapObj )
										rope.deadeyeEnd.position = tpos;
									else
									{
										if ( snapSel )
										{
											rope.deadeyeEnd.position = moveTarget.position;
											rope.endAttach = moveTarget;
										}
									}
									break;
							}

							moveTarget = null;
						}

						mode = MoveMode.None;
						MySetDirty();
						break;
				}

				if ( !rope.endAttach )
				{
					Vector3 endPos = Handles.PositionHandle(rope.deadeyeEnd.position, Quaternion.identity);
					if ( endPos != rope.deadeyeEnd.position )
					{
						rope.deadeyeEnd.position = endPos;
						MySetDirty();
					}
				}

				if ( snapObj || snapSel )
				{
					Camera sceneCamera = SceneView.lastActiveSceneView.camera;

					// Display snap targets
					DisplayTargets();

					Vector3 cpos = rope.deadeyeStart.position;
					if ( mode == MoveMode.End )
						cpos = rope.deadeyeEnd.position;

					Vector3 sp = sceneCamera.WorldToScreenPoint(cpos);

					if ( FindTarget(sp) )
					{
						Handles.matrix = Matrix4x4.identity;
						Handles.color = Color.white;
						Vector3 tp = moveTarget.position;

						Handles.DrawDottedLine(cpos, tp, 8.0f);
						if ( snapObj )
							Handles.Label(tp, "Snap to " + moveTarget.name);

						if ( snapSel )
							Handles.Label(tp, "Set Target " + moveTarget.name);
					}
				}
			}
		}

		private void OnDisable()
		{
			Tools.hidden = false;
		}

		//bool leftShift;

		void OnSceneGUI()
		{
			RopeLanyard rope = (RopeLanyard)target;

			if ( !rope.lanyard || !rope.deadeyeStart || !rope.deadeyeEnd )
				return;

			OnSceneGUINew();

			Handles.matrix = rope.transform.localToWorldMatrix;
			Handles.color = new Color(1.0f, 0.4f, 0.0f);

			if ( rope.startAttach != null && rope.startAttach != null )
				Tools.hidden = true;
			else
				Tools.hidden = false;

			switch ( Event.current.type )
			{
				case EventType.KeyDown:
					//if ( Event.current.keyCode == KeyCode.LeftShift )
						//leftShift = true;
					break;

				case EventType.KeyUp:
					//if ( Event.current.keyCode == KeyCode.LeftShift )
						//leftShift = false;
					break;
			}

			Handles.color = new Color(1.0f, 0.4f, 0.0f);
			Handles.matrix = rope.transform.localToWorldMatrix;

			if ( rope.endAttach )
			{
				Handles.matrix = Matrix4x4.identity;
				Vector3 ep = rope.endAttach.position;
				Handles.Label(ep, "  Disconnect");
				if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(rope.endAttach.position) * 0.2f, 0.0f, Handles.SphereHandleCap) )
				{
					rope.endAttach = null;
					MySetDirty();
				}
			}

			if ( rope.startAttach )
			{
				Handles.matrix = Matrix4x4.identity;
				Vector3 ep = rope.startAttach.position;
				Handles.Label(ep, "  Disconnect");
				if ( Handles.Button(ep, Quaternion.identity, HandleUtility.GetHandleSize(rope.startAttach.position) * 0.2f, 0.0f, Handles.SphereHandleCap) )
				{
					rope.startAttach = null;
					MySetDirty();
				}
			}
		}

		public void UnSkin(RopeLanyard rope)
		{
			SkinnedMeshRenderer mr = rope.GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				Material[] mats = mr.sharedMaterials;
				DestroyImmediate(mr);
				MeshRenderer newmr = rope.gameObject.AddComponent<MeshRenderer>();
				newmr.sharedMaterials = mats;
			}
		}

		public Mesh CopyMesh(Mesh _mesh)
		{
			Mesh clonemesh = new Mesh();
			clonemesh.vertices = _mesh.vertices;

			clonemesh.uv2 = _mesh.uv2;
			clonemesh.uv3 = _mesh.uv3;
			clonemesh.uv4 = _mesh.uv4;
			clonemesh.uv = _mesh.uv;
			clonemesh.normals = _mesh.normals;
			clonemesh.tangents = _mesh.tangents;
			clonemesh.colors = _mesh.colors;

			clonemesh.subMeshCount = _mesh.subMeshCount;

			for ( int s = 0; s < _mesh.subMeshCount; s++ )
				clonemesh.SetTriangles(_mesh.GetTriangles(s), s);

			clonemesh.boneWeights = _mesh.boneWeights;
			clonemesh.bindposes = _mesh.bindposes;
			clonemesh.name = _mesh.name;    // + "_copy";
			clonemesh.RecalculateBounds();

			return clonemesh;
		}

		public void MakeSkinMesh(RopeLanyard rope)
		{
			Material[] mats = null;

			MeshFilter mf = rope.lanyard.GetComponent<MeshFilter>();

			SkinnedMeshRenderer mr = rope.lanyard.GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				UnSkin(rope);
				mr = null;
			}

			if ( mr == null )
			{
				MeshRenderer oldmr = rope.lanyard.GetComponent<MeshRenderer>();
				mats = oldmr.sharedMaterials;

				mr = rope.lanyard.gameObject.AddComponent<SkinnedMeshRenderer>();

				if ( oldmr )
					DestroyImmediate(oldmr);
			}

			string path = AssetDatabase.GetAssetPath(mf.sharedMesh);

			Mesh smesh = CopyMesh(mf.sharedMesh);
			//Mesh smesh = mf.sharedMesh;


			string dir = System.IO.Path.GetDirectoryName(path);
			dir += "/" + rope.name + " Skinned" + ".asset";
			//Debug.Log("path " + path);
			//GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			//Debug.Log("prefab " + tname);

			mr.sharedMesh = smesh;
			Vector3[] verts = smesh.vertices;

			Vector3 toppos = rope.lanyard.InverseTransformPoint(rope.deadeyeEnd.position);
			Vector3 botpos = rope.lanyard.InverseTransformPoint(rope.deadeyeStart.position);
			Vector3 midpos = (toppos + botpos) * 0.5f;

			// now create skin info
			BoneWeight[] weights = new BoneWeight[smesh.vertexCount];

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 pos = verts[i];
				weights[i].boneIndex0 = 0;
				weights[i].boneIndex1 = 1;

				if ( pos.z < midpos.z )
				{
					weights[i].weight0 = 1.0f;
					weights[i].weight1 = 0.0f;
				}
				else
				{
					weights[i].weight0 = 0.0f;
					weights[i].weight1 = 1.0f;
				}
			}

			smesh.boneWeights = weights;

			Transform[] bones = new Transform[2];
			Matrix4x4[] poses = new Matrix4x4[2];

			bones[1] = rope.deadeyeEnd;
			poses[1] = rope.deadeyeEnd.worldToLocalMatrix * rope.transform.localToWorldMatrix;
			bones[0] = rope.deadeyeStart;
			poses[0] = rope.deadeyeStart.worldToLocalMatrix * rope.transform.localToWorldMatrix;

			smesh.bindposes = poses;
			mr.bones = bones;
			mr.updateWhenOffscreen = true;
			mr.materials = mats;

			EditorUtility.SetDirty(smesh);

			AssetDatabase.CreateAsset(smesh, dir);
			AssetDatabase.SaveAssets();
		}
	}
}