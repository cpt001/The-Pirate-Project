using UnityEngine;

namespace FullRig
{
	[ExecuteAlways]
	public class RopeLanyard : MonoBehaviour
	{
		// Do this with skinning
		public Transform	deadeyeStart;
		public Transform	deadeyeEnd;
		public Transform	lanyard;
		public Transform	startAttach;
		public Transform	endAttach;

		void Start()
		{
		}

		void Update()
		{
			if ( startAttach )
				deadeyeStart.position = startAttach.position;

			if ( endAttach )
				deadeyeEnd.position = endAttach.position;

			if ( deadeyeStart && deadeyeEnd )
			{
				deadeyeStart.LookAt(deadeyeEnd);
				deadeyeEnd.LookAt(deadeyeStart);
			}
		}

#if false
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

		public void MakeSkinMesh()
		{
			Material[] mats = null;

			MeshFilter mf = lanyard.GetComponent<MeshFilter>();

			SkinnedMeshRenderer mr = lanyard.GetComponent<SkinnedMeshRenderer>();

			if ( mr )
			{
				UnSkin();
				mr = null;
			}

			if ( mr == null )
			{
				MeshRenderer oldmr = lanyard.GetComponent<MeshRenderer>();
				mats = oldmr.sharedMaterials;

				mr = lanyard.gameObject.AddComponent<SkinnedMeshRenderer>();

				if ( oldmr )
					DestroyImmediate(oldmr);
			}

			Mesh smesh = mf.sharedMesh;
			Vector3[] verts = smesh.vertices;

			Vector3 toppos = lanyard.InverseTransformPoint(deadeyeEnd.position);
			Vector3 botpos = lanyard.InverseTransformPoint(deadeyeStart.position);
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

			bones[1] = deadeyeEnd;
			poses[1] = deadeyeEnd.worldToLocalMatrix * transform.localToWorldMatrix;
			bones[0] = deadeyeStart;
			poses[0] = deadeyeStart.worldToLocalMatrix * transform.localToWorldMatrix;

			smesh.bindposes = poses;
			mr.bones = bones;
			mr.updateWhenOffscreen = true;
			mr.materials = mats;
		}
#endif
	}
}