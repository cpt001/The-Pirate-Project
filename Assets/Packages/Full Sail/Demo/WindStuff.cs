using UnityEngine;

namespace FullSail
{
	[ExecuteAlways]
	public class WindStuff : MonoBehaviour
	{
		public float		windDir;
		[Range(0, 10.0f)]
		public float		windStrength;
		public SailGroup	group;
		public Transform	forwardArrow;
		public Transform	windArrow;
		public Transform	windScale;

		void Update()
		{
			if ( windArrow )
			{
				Vector3 rot = windArrow.eulerAngles;
				rot.y = windDir + 180.0f;
				windArrow.eulerAngles = rot;
			}

			if ( windScale )
			{
				float sv = 0.2f + (0.2f * windStrength);
				Vector3 scl = new Vector3(sv, sv, sv);
				windScale.localScale = scl;
			}

			if ( group )
			{
				Vector3 windv;
				windv.x = Mathf.Sin(windDir * Mathf.Deg2Rad);
				windv.z = Mathf.Cos(windDir * Mathf.Deg2Rad);
				windv.y = 0.0f;

				group.SetValue(SailParamID.WindDir, windv);
				group.SetValue(SailParamID.Speed, windStrength);
			}
		}
	}
}