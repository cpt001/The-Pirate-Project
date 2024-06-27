using UnityEngine;

namespace FullSail
{
	[CreateAssetMenu(menuName = "Full Sail/Furled Map")]
	public class FurledMap : ScriptableObject
	{
		public enum Mode
		{
			Gradient,
			Curve,
		}

		public int				width		= 256;
		public int				height		= 32;
		public Mode				mode		= Mode.Curve;
		public Gradient			fill		= new Gradient();
		public AnimationCurve	fillCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0.1f));
	}
}