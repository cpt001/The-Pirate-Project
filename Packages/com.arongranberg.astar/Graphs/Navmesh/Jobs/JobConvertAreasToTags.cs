using Pathfinding.Graphs.Navmesh.Voxelization.Burst;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>Convert recast region IDs to the tags that should be applied to the nodes</summary>
	[BurstCompile]
	public struct JobConvertAreasToTags : IJob {
		[ReadOnly]
		public NativeList<int> inputAreas;
		/// <summary>Element type: uint</summary>
		public unsafe UnsafeAppendBuffer* outputTags;

		public void Execute () {
			unsafe {
				outputTags->Reset();
				for (int i = 0; i < inputAreas.Length; i++) {
					var area = inputAreas[i];
					// The user supplied IDs start at 1 because 0 is reserved for NotWalkable
					uint tag = (area & VoxelUtilityBurst.TagReg) != 0 ? (uint)(area & VoxelUtilityBurst.TagRegMask) - 1 : 0;
					outputTags->Add(tag);
				}
			}
		}
	}
}
