using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

using Broccoli.Factory;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Pipe;
using Broccoli.Model;

namespace Broccoli.Component
{
	/// <summary>
	/// Trunk mesh generator component.
	/// </summary>
	public class TrunkMeshGeneratorComponent : TreeFactoryComponent {
		#region Vars
		TrunkMeshGeneratorElement trunkMeshGeneratorElement;
		NativeArray<Vector3> m_Vertices;
		NativeArray<Vector3> m_Normals;

		Vector3[] m_ModifiedVertices;
		Vector3[] m_ModifiedNormals;
		#endregion

		#region Job
		struct TrunkJob : IJobParallelFor {
			public NativeArray<Vector3> vertices;
			public NativeArray<Vector3> normals;
			public NativeArray<Vector4> uv5s;
			public NativeArray<Vector4> uv6s;
			public NativeArray<Vector4> uv7s;

			public int branchSkinId;
			public float maxLength;
			public float minLength;
			public float scaleAtBase;
			[NativeDisableParallelForRestriction]
			public NativeArray<float> baseRadialPositions;
			[NativeDisableParallelForRestriction]
			public NativeArray<float> baseRadialLengths;
			public float sinTime;
			public float cosTime;
			public float strength;

			public void Execute(int i) {
				if (uv6s[i].y == branchSkinId && 
					uv5s[i].y + 0.01f >= minLength && uv5s[i].y - 0.01f <= maxLength) {
					float pos = 1f - ((uv5s[i].y - minLength) / (maxLength - minLength));
					float radialScale = GetRadialScale (uv5s[i].x);
					radialScale = 1f + ((radialScale - 1f) * pos);
					vertices[i] = (vertices[i] - (Vector3)uv7s[i]) * radialScale;
					vertices[i] = (Vector3)uv7s[i] + vertices[i];
				}
			}
			public float GetRadialScale (float radialPosition) {
				if (radialPosition > 0 && radialPosition < 1) {
					int i;
					for (i = 0; i < baseRadialLengths.Length; i++) {
						if (radialPosition < baseRadialPositions [i]) {
							break;
						}
					}
					return baseRadialLengths [i];
				} else if (radialPosition == 1) {
					return baseRadialLengths [baseRadialLengths.Length - 1];
				} else {
					return baseRadialLengths [0];
				}
			}
		}
		#endregion

		#region Configuration
		/// <summary>
		/// Prepares the parameters to process with this component.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		protected override void PrepareParams (TreeFactory treeFactory,
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			base.PrepareParams (treeFactory, useCache, useLocalCache, processControl);
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Mesh;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) {
			if (pipelineElement != null && treeFactory != null) {
				// Get the trunk element.
				trunkMeshGeneratorElement = pipelineElement as TrunkMeshGeneratorElement;
				// Prepare the parameters.
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				// Get the Trunk mesh builder.
				BranchMeshBuilder branchMeshBuilder = BranchMeshBuilder.GetInstance ();
				TrunkMeshBuilder trunkMeshBuilder = (TrunkMeshBuilder) branchMeshBuilder.GetBranchMeshBuilder (BranchMeshBuilder.BuilderType.Trunk);
				// Process each branch skin.
				if (trunkMeshBuilder != null) {

					var enumerator = trunkMeshBuilder.branchInfos.GetEnumerator();
					while (enumerator.MoveNext()) {
						int branchSkinId = enumerator.Current.Key;

						Mesh mesh = treeFactory.meshManager.GetMesh (MeshManager.MeshData.Type.Branch);

						// Mark mesh as dynamic.
						mesh.MarkDynamic ();

						// Create job and set variables.
						TrunkJob trunkJob = new TrunkJob ();
						trunkJob.branchSkinId = branchSkinId;
						trunkJob.maxLength = enumerator.Current.Value.rangeLength;
						trunkJob.minLength = 0f;
						trunkJob.scaleAtBase = enumerator.Current.Value.scaleAtBase;
						trunkJob.sinTime = Mathf.Sin(Time.time);
						trunkJob.cosTime = Mathf.Cos(Time.time);
						trunkJob.strength = 0.4f;

						BezierCurve baseCurve = trunkMeshBuilder.baseCurves [branchSkinId];
						trunkJob.baseRadialPositions = new NativeArray<float> (baseCurve.points.Count, Allocator.TempJob);
						trunkJob.baseRadialLengths = new NativeArray<float> (baseCurve.points.Count, Allocator.TempJob);
						for (int i = 0; i < baseCurve.points.Count; i++) {
							CurvePoint cp = baseCurve.points [i];
							trunkJob.baseRadialPositions [i] = cp.relativePosition;
							trunkJob.baseRadialLengths [i] = cp.position.magnitude;
						}

						m_Vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.TempJob);
						m_Normals = new NativeArray<Vector3>(mesh.normals, Allocator.TempJob);
						m_ModifiedVertices = new Vector3[m_Vertices.Length];
						m_ModifiedNormals = new Vector3[m_Vertices.Length];
						trunkJob.vertices = m_Vertices;
						trunkJob.normals = m_Normals;

						List<Vector4> uv5s = new List<Vector4> ();
						mesh.GetUVs (4, uv5s);
						trunkJob.uv5s = new NativeArray<Vector4> (uv5s.ToArray (), Allocator.TempJob);
						List<Vector4> uv6s = new List<Vector4> ();
						mesh.GetUVs (5, uv6s);
						trunkJob.uv6s = new NativeArray<Vector4> (uv6s.ToArray (), Allocator.TempJob);
						List<Vector4> uv7s = new List<Vector4> ();
						mesh.GetUVs (6, uv7s);
						trunkJob.uv7s = new NativeArray<Vector4> (uv7s.ToArray (), Allocator.TempJob);

						// Execute job.
						JobHandle uvJobHandle = trunkJob.Schedule (uv5s.Count, 64);

						// Complete job.
						uvJobHandle.Complete ();

						trunkJob.vertices.CopyTo (m_ModifiedVertices);
						trunkJob.normals.CopyTo (m_ModifiedNormals);

						mesh.vertices = m_ModifiedVertices;
						mesh.normals = m_ModifiedNormals;

						// Dispose.
						trunkJob.vertices.Dispose ();
						trunkJob.normals.Dispose ();
						trunkJob.uv5s.Dispose ();
						trunkJob.uv6s.Dispose ();
						trunkJob.uv7s.Dispose ();
						trunkJob.baseRadialPositions.Dispose ();
						trunkJob.baseRadialLengths.Dispose ();
					}
				}
				return true;
			}
			return false;
		}
		#endregion
	}
}