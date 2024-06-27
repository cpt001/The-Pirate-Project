using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace FullRig
{
	public class FullRigGUI
	{
		static GUIStyle		headerStyle;
		static GUIStyle		foldoutStyle;
		static GUIStyle		buttonStyle;
		public static bool	dragging = false;

		static public void SetDirty(Object target)
		{
			if ( target )
			{
				EditorUtility.SetDirty(target);
				if ( !Application.isPlaying )
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		static public void Header(string name)
		{
			if ( headerStyle == null )
			{
				headerStyle = new GUIStyle();
				headerStyle.normal.textColor	= Color.grey;
				headerStyle.fontSize			= 16;
				headerStyle.fontStyle			= FontStyle.Bold;
			}
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField(name, headerStyle);
			EditorGUILayout.EndVertical();
		}

		static public bool BigButton(string name, string tip = "")
		{
			if ( buttonStyle == null )
			{
				buttonStyle = new GUIStyle(EditorStyles.miniButton);
				buttonStyle.fontSize	= 16;
				buttonStyle.fontStyle	= FontStyle.Bold;
				buttonStyle.fixedHeight	= 40;
			}

			if ( GUILayout.Button(new GUIContent(name, tip), buttonStyle) )
				return true;

			return false;
		}

		static public void FoldOut(ref bool open, string name, string tip = "")
		{
			if ( headerStyle == null )
			{
				headerStyle = new GUIStyle();
				headerStyle.normal.textColor	= Color.white;
				headerStyle.fontSize			= 16;
				headerStyle.fontStyle			= FontStyle.Bold;
			}

			if ( foldoutStyle == null )
			{
				foldoutStyle = new GUIStyle(EditorStyles.foldout);
				Color col = new Color(0.8f, 0.8f, 0.8f, 1.0f);
				foldoutStyle.normal.textColor		= col;
				foldoutStyle.fontSize				= 16;
				foldoutStyle.fontStyle				= FontStyle.Bold;
				foldoutStyle.fontStyle				= FontStyle.Bold;
				foldoutStyle.normal.textColor		= col;
				foldoutStyle.onNormal.textColor		= col;
				foldoutStyle.hover.textColor		= col;
				foldoutStyle.onHover.textColor		= col;
				foldoutStyle.focused.textColor		= col;
				foldoutStyle.onFocused.textColor	= col;
				foldoutStyle.active.textColor		= col;
				foldoutStyle.onActive.textColor		= col;
			}

			EditorGUILayout.BeginVertical("box");
			open = EditorGUILayout.Foldout(open, new GUIContent(name, tip), true, foldoutStyle);
			EditorGUILayout.EndVertical();
		}

		static public void ShaderProperty(MaterialEditor matedit, MaterialProperty prop, string name, string tip = "")
		{
			matedit.ShaderProperty(prop, new GUIContent(name, tip));
		}

		static void BeginChangeCheck()
		{
			EditorGUI.BeginChangeCheck();
		}

		static bool EndChangeCheck()
		{
			return EditorGUI.EndChangeCheck();
		}

		static public void RecordObject(Object target, string name)
		{
			Undo.RecordObject(target, "Changed " + name);
		}

		static public void RecordObjects(Object[] targets, string name)
		{
			Undo.RecordObjects(targets, "Changed " + name);
		}

		static public void Slider(Object target, string name, ref float val, float min, float max, string tip)
		{
			BeginChangeCheck();
			float newval = EditorGUILayout.Slider(new GUIContent(name, tip), val, min, max);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
			}
		}

		static public void ColorField(Object target, string name, ref Color val, string tip = "")
		{
			BeginChangeCheck();
			Color newval = EditorGUILayout.ColorField(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
			}
		}

		static public void Texture2D(Object target, string name, ref Texture2D val, bool flag = true, string tip = "")
		{
			BeginChangeCheck();
			Texture2D newobj = (Texture2D)EditorGUILayout.ObjectField(new GUIContent(name, tip), val, typeof(Texture2D), flag);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newobj;
				SetDirty(target);
			}
		}

		static public void Vector4(Object target, string name, ref Vector4 val, string tip = "")
		{
			BeginChangeCheck();
			Vector4 newval = EditorGUILayout.Vector4Field(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
			}
		}

		static public void Vector3(Object target, string name, ref Vector3 val, string tip = "")
		{
			BeginChangeCheck();
			Vector3 newval = EditorGUILayout.Vector3Field(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
			}
		}

		static public bool Float(Object target, string name, ref float val, string tip = "")
		{
			BeginChangeCheck();
			float newval = EditorGUILayout.FloatField(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
				return true;
			}

			return false;
		}

		static public void Property(string name, SerializedProperty prop, string tip = "")
		{
			EditorGUILayout.PropertyField(prop, new GUIContent(name, tip));
		}

		static public void Int(Object target, string name, ref int val, string tip = "")
		{
			BeginChangeCheck();
			int newval = EditorGUILayout.IntField(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				SetDirty(target);
			}
		}

		static public bool Toggle(Object target, string name, ref bool val, string tip = "")
		{
			bool changed = false;
			BeginChangeCheck();
			bool newval = EditorGUILayout.Toggle(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				changed = true;
				SetDirty(target);
			}

			return changed;
		}

		static public void Curve(Object target, string name, ref AnimationCurve crv, string tip = "")
		{
			BeginChangeCheck();
			AnimationCurve newcrv = EditorGUILayout.CurveField(new GUIContent(name, tip), crv);
			if ( EndChangeCheck() )
			{
				Undo.RegisterCompleteObjectUndo(target, "Changed " + name);
				crv = newcrv;
				SetDirty(target);
			}
		}

		static public bool RopeNumFlags(Object target, string name, ref RopeNum val, string tip = "")
		{
			bool changed = false;
			BeginChangeCheck();
			RopeNum newval = (RopeNum)EditorGUILayout.EnumFlagsField(new GUIContent(name, tip), val);
			if ( EndChangeCheck() )
			{
				RecordObject(target, "Changed " + name);
				val = newval;
				changed = true;
				SetDirty(target);
			}

			return changed;
		}

		static public void Transform(Object target, string name, ref Transform obj, string tip = "")
		{
			BeginChangeCheck();
			Transform newobj = (Transform)EditorGUILayout.ObjectField(new GUIContent(name, tip), obj, typeof(Transform), true);
			if ( EndChangeCheck() )
			{
				Undo.RegisterCompleteObjectUndo(target, "Changed " + name);
				obj = newobj;
				SetDirty(target);
			}
		}

		static public void RopeEnd(Object target, string name, ref RopeEnd obj, string tip = "")
		{
			BeginChangeCheck();
			RopeEnd newobj = (RopeEnd)EditorGUILayout.ObjectField(new GUIContent(name, tip), obj, typeof(RopeEnd), true);
			if ( EndChangeCheck() )
			{
				Undo.RegisterCompleteObjectUndo(target, "Changed " + name);
				obj = newobj;
				SetDirty(target);
			}
		}

	}
}