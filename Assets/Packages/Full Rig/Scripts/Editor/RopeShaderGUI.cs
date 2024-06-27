using UnityEngine;
using UnityEditor;

namespace FullRig
{
	public class RopeShaderGUI : ShaderGUI
	{
		public static bool generalParams	= false;
		public static bool ropeParams		= false;

		Texture logoImage;

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullRigLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			// General
			FullRigGUI.FoldOut(ref generalParams, "General", "General Shader params, color, main texture, bump map etc");
			if ( generalParams )
			{
				MaterialProperty color		= FindProperty("_Color",		properties);
				MaterialProperty maintex	= FindProperty("_MainTex",		properties);
				MaterialProperty gloss		= FindProperty("_Glossiness",	properties);
				MaterialProperty metal		= FindProperty("_Metallic",		properties);

				EditorGUILayout.BeginVertical("box");

				FullRigGUI.ShaderProperty(materialEditor, color,	"Color",			"Tints the objects texture colors");
				FullRigGUI.ShaderProperty(materialEditor, maintex,	"Main Texture",		"Main texture to use for the sail");
				FullRigGUI.ShaderProperty(materialEditor, gloss,	"Glossiness",		"How shiny the sail is");

				EditorGUILayout.EndVertical();
			}

			// Emblem
			FullRigGUI.FoldOut(ref ropeParams, "Rope", "Rope Values");
			if ( ropeParams )
			{
				//MaterialProperty width			= FindProperty("_Width",		properties);
				MaterialProperty swingphaseoff	= FindProperty("_SwingOffset",	properties);
				MaterialProperty swingangle		= FindProperty("_SwingAngle",	properties);
				MaterialProperty swingfreq		= FindProperty("_SwingFreq",	properties);

				//MaterialProperty adjust = FindProperty("_Adjust", properties);

				MaterialProperty stretch = FindProperty("_StretchUV", properties);

				EditorGUILayout.BeginVertical("box");

				//FullRigGUI.ShaderProperty(materialEditor, width,			"Width",			"Width of the rope strands");
				FullRigGUI.ShaderProperty(materialEditor, swingphaseoff,	"Swing Phase Off",	"Swing Animation offset, stops all ropes moving the same");
				FullRigGUI.ShaderProperty(materialEditor, swingangle,		"Swing Angle",		"Max Angle the rope will swing to");
				FullRigGUI.ShaderProperty(materialEditor, swingfreq,		"Swing Freq",		"Frequencey of the rope swing");

				FullRigGUI.ShaderProperty(materialEditor, stretch,			"Stretch UVs",		"Will adjust the uvs based on length 0 or 1");

				//FullRigGUI.ShaderProperty(materialEditor, adjustz, "Adjust Z", "Adjust width of the rope strands");
				//FullRigGUI.ShaderProperty(materialEditor, adjusty, "Adjust Y", "Adjust height of the rope strands");

				EditorGUILayout.EndVertical();
			}
		}
	}
}