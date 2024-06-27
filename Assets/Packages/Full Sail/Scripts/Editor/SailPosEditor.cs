using UnityEditor;
using UnityEngine;

namespace FullSail
{
	[CustomEditor(typeof(SailPos))]
	[CanEditMultipleObjects]
	public class SailPosEditor : Editor
	{
		Texture logoImage;

		public override void OnInspectorGUI()
		{
			SailPos mod = (SailPos)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			DrawDefaultInspector();

			if ( GUI.changed )
			{
				//mod.SetDirty();
				EditorUtility.SetDirty(target);
			}
		}

		Vector3 mpos;
		float	sx;
		float	sy;

		private void OnDisable()
		{
			Tools.hidden = false;
		}

		void OnSceneGUI()
		{
			SailPos mod = (SailPos)target;

			Tools.hidden = true;

			switch ( Event.current.type )
			{
				case EventType.MouseDown:
					mpos = mod.GetPos();	//mod.transform.InverseTransformPoint(mod.GetPos());
					sx = mod.sailPosX;
					sy = mod.sailPosY;
					break;
			}

			Handles.matrix = Matrix4x4.identity;	//mod.transform.localToWorldMatrix;

			Handles.color = new Color(1.0f, 0.4f, 0.0f);

			Vector3 pos = mod.GetPos();	//mod.transform.InverseTransformPoint(mod.GetPos());
			Vector3 np = Handles.FreeMoveHandle(pos, Quaternion.identity, 0.2f, Vector3.zero, Handles.SphereHandleCap);

			if ( np != pos )
			{
				Matrix4x4 tm = Matrix4x4.TRS(mod.transform.position, Quaternion.identity, Vector3.one);
				//Vector3 lpos = mod.transform.InverseTransformPoint(mpos);
				//Vector3 lnpos = mod.transform.InverseTransformPoint(np);
				Vector3 lpos = tm.inverse.MultiplyPoint(mpos);	//mod.transform.InverseTransformPoint(mpos);
				Vector3 lnpos = tm.inverse.MultiplyPoint(np);	//mod.transform.InverseTransformPoint(np);
				float dx = lnpos.x - lpos.x;
				mod.sailPosX = sx - (dx / 10.0f);
				mod.sailPosX = Mathf.Clamp01(mod.sailPosX);

				float dy = lnpos.y - lpos.y;
				mod.sailPosY = sy + (dy / 10.0f);
				mod.sailPosY = Mathf.Clamp01(mod.sailPosY);

				EditorUtility.SetDirty(mod);
			}
		}
	}
}