using UnityEngine;

namespace FullSail
{
	[CreateAssetMenu(menuName = "Full Sail/Move Map")]
	public class MoveMap : ScriptableObject
	{
		public int				width			= 256;
		public int				height			= 256;

		// Have array of fix points. gradient across and down for ribs
		// attach point texture? or number attach points top, bottom, sides
		public float			fixFalloff		= 0.1f;
		public float			fixFalloffPow	= 0.2f;
		public float			fixTL			= 1.0f;
		public float			fixTR			= 1.0f;
		public float			fixBL			= 1.0f;
		public float			fixBR			= 1.0f;

		public bool				useFill			= false;
		public Gradient			fill			= new Gradient();
		public float			fillRepeat		= 1.0f;
		public bool				useVertFill		= false;
		public Gradient			vertFill		= new Gradient();
		public float			vertFillRepeat	= 1.0f;
		public bool				useCurves		= true;
		public AnimationCurve	fillCrvX		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		public AnimationCurve	fillCrvY		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		public bool				useEdgeCurves	= false;
		public AnimationCurve	fillCrvX1		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		public AnimationCurve	fillCrvY1		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.0f));
		public bool				useBlob			= false;
		public Vector2			blobPos			= new Vector3(0.5f, 0.5f);
		public float			blobRadius		= 0.25f;
		public float			blobAmt			= 0.1f;
		public float			blobFalloff		= 0.1f;
		public float			blobFalloffPow	= 0.2f;
		public Texture2D		baseMap;
	}
}