using UnityEditor;
using UnityEngine;

namespace FullSail
{
	[CustomEditor(typeof(SailGroup))]
	public class SailGroupEditor : Editor
	{
		Texture logoImage;

		public override void OnInspectorGUI()
		{
			SailGroup mod = (SailGroup)target;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			mod.randomRipple = EditorGUILayout.BeginToggleGroup("Add Ripple Offsets", mod.randomRipple);
			mod.seed = EditorGUILayout.IntField("Seed", mod.seed);
			EditorGUILayout.MinMaxSlider("Anim Range", ref mod.minAnimOffset, ref mod.maxAnimOffset, 0.0f, 10.0f);
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("sailObjs"));
			if ( GUILayout.Button(new GUIContent("Add Child Sails", "Add any sails found on child objects to the group")) )
			{
				FullSailGUI.RecordObject(target, "Added Child Sails");

				mod.sailObjs.Clear();
				Sail[] ss = mod.GetComponentsInChildren<Sail>();
				for ( int i = 0; i < ss.Length; i++ )
					mod.sailObjs.Add(ss[i]);
			}

			FullSailGUI.Header("Sail Overrides");

			for ( int i = 0; i < mod.sailParams.Count; i++ )
			{
				SailParam sp = mod.sailParams[i];

				bool changed = false;
				if ( SailEditor.ShowSailParam(mod, sp, ref changed) )
				{
					FullSailGUI.RecordObject(target, "Removed Group Override");
					mod.sailParams.RemoveAt(i);
				}
			}

			if ( GUILayout.Button(new GUIContent("Add Group Override", "Add a new sail param override to the group list")) )
			{
				FullSailGUI.RecordObject(target, "Added new override");
				SailParam sp	= new SailParam();
				sp.active		= true;
				sp.cval			= Color.white;
				mod.dirty		= true;

				mod.sailParams.Add(sp);
			}

			FullSailGUI.FoldOut(ref mod.showRange, "Random Values");
			if ( mod.showRange )
			{
				for ( int i = 0; i < mod.randomValues.Count; i++ )
				{
					SailParamRange pr = mod.randomValues[i];

					EditorGUILayout.BeginHorizontal("box");

					pr.active = EditorGUILayout.Toggle("", pr.active, GUILayout.Width(20));
					pr.id = (SailParamID)EditorGUILayout.EnumPopup("", pr.id, GUILayout.Width(100));

					EditorGUIUtility.labelWidth = 20;
					pr.fmin = EditorGUILayout.FloatField(" ", pr.fmin, GUILayout.Width(100));
					pr.fmax = EditorGUILayout.FloatField(" ", pr.fmax, GUILayout.Width(100));

					if ( GUILayout.Button("-", GUILayout.Width(16)) )
						mod.randomValues.RemoveAt(i);

					EditorGUILayout.EndHorizontal();
				}

				if ( GUILayout.Button("Add Range") )
				{
					mod.randomValues.Add(new SailParamRange());
				}
			}

			if ( GUI.changed )
			{
				mod.dirty = true;
				EditorUtility.SetDirty(mod);
			}
		}
	}
}