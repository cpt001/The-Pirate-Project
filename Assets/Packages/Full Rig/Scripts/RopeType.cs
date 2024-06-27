using UnityEngine;

namespace FullRig
{
	[CreateAssetMenu(menuName = "Full Rig/Rope Type")]

	public class RopeType : ScriptableObject
	{
		public Mesh		mesh;
		public Material	material;
		public RopeNum	ropeNum	= RopeNum.One;
	}
}