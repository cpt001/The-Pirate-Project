using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sprout mesh generator component.
	/// </summary>
	public class SproutMeshGeneratorComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The sprout mesh builder.
		/// </summary>
		SproutMeshBuilder sproutMeshBuilder = null;
		/// <summary>
		/// The sprout mesh generator element.
		/// </summary>
		SproutMeshGeneratorElement sproutMeshGeneratorElement = null;
		/// <summary>
		/// The sprout meshes.
		/// </summary>
		Dictionary<int, SproutMesh> sproutMeshes = new Dictionary <int, SproutMesh> ();
		/// <summary>
		/// The sprout mappers.
		/// </summary>
		Dictionary<int, SproutMap> sproutMappers = new Dictionary <int, SproutMap> ();
		/// <summary>
		/// Flag to reduce the complexity of sprouts for LOD purposes.
		/// </summary>
		bool simplifySprouts = false;
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
			sproutMeshBuilder = SproutMeshBuilder.GetInstance ();

			// Gather all SproutMap objects from elements downstream.
			PipelineElement pipelineElement = 
				sproutMeshGeneratorElement.GetDownstreamElement (PipelineElement.ClassType.SproutMapper);
			sproutMappers.Clear ();
			if (pipelineElement != null && pipelineElement.isActive) {
				SproutMapperElement sproutMapperElement = (SproutMapperElement)pipelineElement;
				for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
					if (sproutMapperElement.sproutMaps[i].groupId > 0) {
						sproutMappers.Add (sproutMapperElement.sproutMaps[i].groupId, sproutMapperElement.sproutMaps[i]);
					}
				}
			}

			// Gather all SproutMesh objects from element.
			sproutMeshes.Clear ();
			for (int i = 0; i < sproutMeshGeneratorElement.sproutMeshes.Count; i++) {
				sproutMeshes.Add (sproutMeshGeneratorElement.sproutMeshes[i].groupId, sproutMeshGeneratorElement.sproutMeshes[i]);
			}

			sproutMeshBuilder.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
			sproutMeshBuilder.SetGravity (GlobalSettings.gravityDirection);
			//sproutMeshBuilder.mapST = MaterialManager.leavesShaderType != MaterialManager.LeavesShaderType.TreeCreatorOrSimilar;
			sproutMeshBuilder.mapST = true;

			// Switch simplify flag for LOD processing.
			/*
			if (!treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				if (processControl != null && processControl.pass == 1) {
					simplifySprouts = true;
				} else {
					simplifySprouts = false;
				}
			} else {
				simplifySprouts = false;
			}
			*/
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructureGirth; // TODO
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			sproutMeshBuilder = null;
			sproutMeshGeneratorElement = null;
			sproutMeshes.Clear ();
			sproutMappers.Clear ();
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
			if (pipelineElement != null && tree != null) {
				sproutMeshGeneratorElement = pipelineElement as SproutMeshGeneratorElement;
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				BuildMesh (treeFactory, processControl.lodIndex);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Removes the product of this component on the factory processing.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void Unprocess (TreeFactory treeFactory) {
			treeFactory.meshManager.DeregisterMeshByType (MeshManager.MeshData.Type.Sprout);
		}
		/// <summary>
		/// Builds the mesh or meshes for the sprouts.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="lodIndex">Index for the LOD definition.</param>
		private void BuildMesh (TreeFactory treeFactory, int lodIndex) {
			var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
			int groupId;
			sproutMeshBuilder.PrepareBuilder (sproutMeshes, sproutMappers);
			while (sproutMeshesEnumerator.MoveNext ()) {
				groupId = sproutMeshesEnumerator.Current.Key;
				bool isTwoSided = treeFactory.materialManager.IsSproutTwoSided ();
				//if (sproutGroups.ContainsKey (groupId)) {
				if (pipelineElement.pipeline.sproutGroups.HasSproutGroup (groupId)) {
					if (sproutMappers.ContainsKey (groupId) && sproutMeshes[groupId].mode != SproutMesh.Mode.Mesh) {
						if (sproutMappers [groupId].IsTextured ()) {
							sproutMeshBuilder.AssignSproutAreas (tree, groupId, sproutMappers [groupId]);
							List<SproutMap.SproutMapArea> sproutAreas = sproutMappers [groupId].sproutAreas;
							for (int i = 0; i < sproutAreas.Count; i++) {
								if (sproutAreas[i].enabled) {
									Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, 
										                 groupId, TranslateSproutMesh (sproutMeshes [groupId]), sproutAreas[i], i, isTwoSided);
									ApplyNormalMode (groupMesh, Vector3.zero);
									treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId, i);
									treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId, i);
									List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
									for (int j = 0; j < sproutMeshDatas.Count; j++) {
										MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[j].startIndex, 
											                                sproutMeshDatas[j].length,
																			sproutMeshDatas[j].position, 
											                                0, 
											                                sproutMeshDatas[j].origin,
											                                MeshManager.MeshData.Type.Sprout,
											                                groupId,
											                                i);
										meshPart.sproutId = sproutMeshDatas[j].sproutId;
										meshPart.branchId = sproutMeshDatas[j].branchId;
									}
								} else {
									treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId, i);
								}
							}
						} else {
							Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, groupId, TranslateSproutMesh (sproutMeshes [groupId]));
							ApplyNormalMode (groupMesh, Vector3.zero);
							treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId);
							treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
							List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
							for (int i = 0; i < sproutMeshDatas.Count; i++) {
								MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[i].startIndex, 
									sproutMeshDatas[i].length,
									sproutMeshDatas[i].position,
									0, 
									sproutMeshDatas[i].origin,
									MeshManager.MeshData.Type.Sprout,
									groupId);
								meshPart.sproutId = sproutMeshDatas[i].sproutId;
								meshPart.branchId = sproutMeshDatas[i].branchId;
							}
						}
					} else {
						// Process without sprout areas.
						Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, 
							groupId, sproutMeshes [groupId]);
						ApplyNormalMode (groupMesh, Vector3.zero);
						treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId);
						treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
						List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
						for (int i = 0; i < sproutMeshDatas.Count; i++) {
							MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[i].startIndex,
								sproutMeshDatas[i].length,
								sproutMeshDatas[i].position,
								0,
								sproutMeshDatas[i].origin,
								MeshManager.MeshData.Type.Sprout,
								groupId);
							meshPart.branchId = sproutMeshDatas[i].branchId;
							meshPart.sproutId = sproutMeshDatas[i].sproutId;
						}
					}
				}
			}
			/*
			if (lodIndex == 1) {
				sproutMeshGeneratorElement.verticesCountFirstPass = treeFactory.meshManager.GetVerticesCount ();
				sproutMeshGeneratorElement.trianglesCountFirstPass = treeFactory.meshManager.GetTrianglesCount ();
			} else {
				sproutMeshGeneratorElement.verticesCountSecondPass = treeFactory.meshManager.GetVerticesCount ();
				sproutMeshGeneratorElement.trianglesCountSecondPass = treeFactory.meshManager.GetTrianglesCount ();
			}
			*/
			/*
			if (treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				sproutMeshGeneratorElement.showLODInfoLevel = 1;
			} else if (!treeFactory.treeFactoryPreferences.prefabUseLODGroups) {
				sproutMeshGeneratorElement.showLODInfoLevel = 2;
			} else {
				sproutMeshGeneratorElement.showLODInfoLevel = -1;
			}
			*/
		}
		/// <summary>
		/// Reprocess normals for the sprout mesh.
		/// </summary>
		/// <param name="targetMesh">Target sprout mesh.</param>
		/// <param name="offset">Vector3 offset from the normal reference point (depending on the normal mode applied).</param>
		void ApplyNormalMode (Mesh targetMesh, Vector3 offset) {
			if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.PerSprout) return;
			Vector3 referenceCenter = targetMesh.bounds.center;
			if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.TreeOrigin) {
				referenceCenter.y = 0;
			} else if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.SproutsBase) {
				referenceCenter.y -= targetMesh.bounds.size.y / 2f;
			}
			List<Vector3> normals = new List<Vector3> ();
			List<Vector3> vertices = new List<Vector3> ();
			targetMesh.GetNormals (normals);
			targetMesh.GetVertices (vertices);
			for (int i = 0; i < normals.Count; i++) {
				normals [i] = Vector3.Lerp (normals[i], (vertices[i] - referenceCenter + offset).normalized, sproutMeshGeneratorElement.normalModeStrength);
			}
			targetMesh.SetNormals (normals);
		}
		/// <summary>
		/// Simplifies sprout mesh parameters for LOD purposes.
		/// </summary>
		/// <param name="sproutMesh">SproutMesh to evaluate.</param>
		/// <returns>Translated SproutMesh.</returns>
		SproutMesh TranslateSproutMesh (SproutMesh sproutMesh) {
			if (simplifySprouts) {
				if (sproutMesh.mode == SproutMesh.Mode.GridPlane) {
					SproutMesh simplyfiedSproutMesh = sproutMesh.Clone ();
					if (sproutMesh.resolutionHeight > sproutMesh.resolutionWidth) {
						simplyfiedSproutMesh.resolutionWidth = 1;
						simplyfiedSproutMesh.resolutionHeight = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionHeight / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionHeight);
					} else if (sproutMesh.resolutionWidth > sproutMesh.resolutionHeight) {
						simplyfiedSproutMesh.resolutionHeight = 1;
						simplyfiedSproutMesh.resolutionWidth = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionWidth / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionWidth);
					} else {
						simplyfiedSproutMesh.resolutionHeight = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionHeight / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionHeight);
						simplyfiedSproutMesh.resolutionWidth = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionWidth / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionWidth);
					}
					return simplyfiedSproutMesh;
				} else if (sproutMesh.mode == SproutMesh.Mode.PlaneX) {
					
				}
			}
			return sproutMesh;
		}
		#endregion
	}
}