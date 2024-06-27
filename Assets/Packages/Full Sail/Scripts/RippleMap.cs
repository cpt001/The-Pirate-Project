using UnityEngine;

namespace FullSail
{
	[CreateAssetMenu(menuName = "Full Sail/Ripple Map")]
	public class RippleMap : ScriptableObject
	{
		public int				width		= 256;		// Width of texture to make
		public int				height		= 12;		// height of texture to make, currently only needs to be 1 high
		public bool				negate		= false;	// negate the curve values when making texture
		public float			amplitude	= 1.0f;		// change the curve values, easier than editing all the points
		public float			scale		= 1.0f;		// scales the X axis to compress the effect of stretch it
		public AnimationCurve	waveCrv = new AnimationCurve(
											 new Keyframe(0.00f, 0.50f, 0, 0),
											 new Keyframe(0.05f, 1.00f, 0, 0),
											 new Keyframe(0.15f, 0.10f, 0, 0),
											 new Keyframe(0.25f, 0.80f, 0, 0),
											 new Keyframe(0.35f, 0.30f, 0, 0),
											 new Keyframe(0.45f, 0.60f, 0, 0),
											 new Keyframe(0.55f, 0.40f, 0, 0),
											 new Keyframe(0.65f, 0.55f, 0, 0),
											 new Keyframe(0.75f, 0.46f, 0, 0),
											 new Keyframe(0.85f, 0.52f, 0, 0),
											 new Keyframe(0.99f, 0.50f, 0, 0)
										 );
	}
}