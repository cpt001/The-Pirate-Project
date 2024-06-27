using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullRig
{
	public class MastBend : MonoBehaviour
	{
		public Transform top;
		public Transform bottom;
		//public Transform middle;

		public AnimationCurve	curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

		private void Start()
		{
			MakeSkinMesh();
		}

		public void UnSkin()
		{
			SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				Material[] mats = mr.sharedMaterials;
				DestroyImmediate(mr);
				MeshRenderer newmr = gameObject.AddComponent<MeshRenderer>();
				newmr.sharedMaterials = mats;
			}
		}

#if false
		public void MakeSkinMesh()
		{
			Material[] mats = null;

			MeshFilter mf = GetComponent<MeshFilter>();

			SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				UnSkin();
				mr = null;
			}

			if ( mr == null )
			{
				MeshRenderer oldmr = GetComponent<MeshRenderer>();
				mats = oldmr.sharedMaterials;

				mr = gameObject.AddComponent<SkinnedMeshRenderer>();

				if ( oldmr )
					DestroyImmediate(oldmr);
			}

			Mesh smesh = mf.sharedMesh;
#if false
			Mesh mesh = new Mesh();
			mesh.name = "Antenna";
			mesh.Clear();
			mesh.subMeshCount = smesh.subMeshCount;	// change all my skinning components
			mesh.vertices = smesh.vertices;
			mesh.uv = smesh.uv;
			mesh.uv2 = smesh.uv2;
			mesh.uv3 = smesh.uv3;
			mesh.uv4 = smesh.uv4;
			mesh.normals = smesh.normals;
			mesh.tangents = smesh.tangents;
			mesh.colors = smesh.colors;
			mesh.RecalculateBounds();

			for ( int i = 0; i < smesh.subMeshCount; i++ )
				mesh.SetTriangles(smesh.GetTriangles(i), i);
#endif
			Vector3[] verts = smesh.vertices;

			Vector3 p1 = smesh.bounds.min;
			Vector3 p2 = smesh.bounds.max;

			//float len = p2.y - p1.y;

			Vector3 toppos = transform.InverseTransformPoint(top.position);
			//Vector3 midpos = transform.InverseTransformPoint(middle.position);
			Vector3 botpos = transform.InverseTransformPoint(bottom.position);

			// now create skin info
			BoneWeight[] weights = new BoneWeight[smesh.vertexCount];

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 pos = verts[i];

				if ( pos.y < botpos.y )
				{
					weights[i].boneIndex0 = 0;
					weights[i].boneIndex1 = 1;

					weights[i].weight0 = 1.0f;
					weights[i].weight1 = 0.0f;
				}
				else
				{
					if ( pos.y < midpos.y )
					{
						weights[i].boneIndex0 = 0;
						weights[i].boneIndex1 = 1;

						weights[i].weight0 = (midpos.y - pos.y) / (midpos.y - botpos.y);
						weights[i].weight1 = 1.0f - weights[i].weight0;
					}
					else
					{
						if ( pos.y < toppos.y )
						{
							weights[i].boneIndex0 = 1;
							weights[i].boneIndex1 = 2;

							weights[i].weight0 = (toppos.y - pos.y) / (toppos.y - midpos.y);
							weights[i].weight1 = 1.0f - weights[i].weight0;
						}
						else
						{
							weights[i].boneIndex0 = 1;
							weights[i].boneIndex1 = 2;

							weights[i].weight0 = 0.0f;
							weights[i].weight1 = 1.0f;
						}
					}
				}
			}

			smesh.boneWeights = weights;

			Transform[] bones = new Transform[3];
			Matrix4x4[] poses = new Matrix4x4[3];

			bones[2] = top;
			poses[2] = top.worldToLocalMatrix * transform.localToWorldMatrix;
			bones[1] = middle;
			poses[1] = middle.worldToLocalMatrix * transform.localToWorldMatrix;
			bones[0] = bottom;
			poses[0] = bottom.worldToLocalMatrix * transform.localToWorldMatrix;

			smesh.bindposes = poses;
			mr.bones = bones;
			//mr.sharedMesh = mesh;
			mr.updateWhenOffscreen = true;
			mr.materials = mats;
		}
	}
#endif
		public void MakeSkinMesh()
		{
			Material[] mats = null;

			MeshFilter mf = GetComponent<MeshFilter>();

			SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				UnSkin();
				mr = null;
			}

			if ( mr == null )
			{
				MeshRenderer oldmr = GetComponent<MeshRenderer>();
				mats = oldmr.sharedMaterials;

				mr = gameObject.AddComponent<SkinnedMeshRenderer>();

				if ( oldmr )
					DestroyImmediate(oldmr);
			}

			Mesh smesh = mf.sharedMesh;
			Vector3[] verts = smesh.vertices;

			Vector3 p1 = smesh.bounds.min;
			Vector3 p2 = smesh.bounds.max;

			//float len = p2.y - p1.y;

			Vector3 toppos = transform.InverseTransformPoint(top.position);
			//Vector3 midpos = transform.InverseTransformPoint(middle.position);
			Vector3 botpos = transform.InverseTransformPoint(bottom.position);

			// now create skin info
			BoneWeight[] weights = new BoneWeight[smesh.vertexCount];

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 pos = verts[i];

				if ( pos.y < botpos.y )
				{
					weights[i].boneIndex0 = 0;
					weights[i].boneIndex1 = 1;

					weights[i].weight0 = 1.0f;
					weights[i].weight1 = 0.0f;
				}
				else
				{
					if ( pos.y < toppos.y )
					{
						weights[i].boneIndex0 = 0;
						weights[i].boneIndex1 = 1;

						float alpha = (toppos.y - pos.y) / (toppos.y - botpos.y);

						weights[i].weight0 = curve.Evaluate(alpha);	//(toppos.y - pos.y) / (toppos.y - botpos.y);
						weights[i].weight1 = 1.0f - weights[i].weight0;
					}
					else
					{
						weights[i].boneIndex0 = 0;
						weights[i].boneIndex1 = 1;

						weights[i].weight0 = 0.0f;
						weights[i].weight1 = 1.0f;
					}
				}
			}

			smesh.boneWeights = weights;

			Transform[] bones = new Transform[2];
			Matrix4x4[] poses = new Matrix4x4[2];

			bones[1] = top;
			poses[1] = top.worldToLocalMatrix * transform.localToWorldMatrix;
			bones[0] = bottom;
			poses[0] = bottom.worldToLocalMatrix * transform.localToWorldMatrix;

			smesh.bindposes = poses;
			mr.bones = bones;
			mr.updateWhenOffscreen = true;
			mr.materials = mats;
		}
	}
}
