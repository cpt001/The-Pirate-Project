using UnityEngine;

namespace FullRig
{
	[System.Flags]
	public enum RopeNum
	{
		One		= 1,
		Two		= 2,
		Four	= 4,
		Six		= 8,
		All		= 255,
	}

	[CreateAssetMenu(menuName = "Full Rig/Rope End Object")]

	public class RopeEnd : ScriptableObject
	{
		public float		ropeStart	= 0.0f;
		public float		tangent		= 0.01f;
		public float		adjustZ		= 0.0f;
		public float		adjustY		= 0.0f;
		public float		width		= 1.0f;
		public RopeNum		ropeType	= RopeNum.All;
		public GameObject	prefab; 
	}
}