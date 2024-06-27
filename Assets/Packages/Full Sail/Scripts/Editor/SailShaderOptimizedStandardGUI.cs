using UnityEngine;
using UnityEditor;

namespace FullSail
{
	public class SailShaderOptimizedStandardGUI : ShaderGUI
	{
		public static bool generalParams	= false;
		public static bool emblemParams		= false;
		public static bool damageParams		= false;
		public static bool rippleParams		= false;
		public static bool furlingParams	= false;
		public static bool sailParams		= false;
		public static bool impactParams		= false;

		Texture logoImage;
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			Material mat = materialEditor.target as Material;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)Screen.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(Screen.width), GUILayout.Height(h1));
			}

			// General
			FullSailGUI.FoldOut(ref generalParams, "General", "General Shader params, color, main texture, bump map etc");
			if ( generalParams )
			{
				MaterialProperty color		= FindProperty("_Color",		properties);
				MaterialProperty maintex	= FindProperty("_MainTex",		properties);
				MaterialProperty speccol	= FindProperty("_SpecColor",	properties);
				MaterialProperty gloss		= FindProperty("_Glossiness",	properties);
				MaterialProperty metal		= FindProperty("_Metallic",		properties);
				MaterialProperty bumpmap	= FindProperty("_BumpMap",		properties);

				EditorGUILayout.BeginVertical("box");

				FullSailGUI.ShaderProperty(materialEditor, color,	"Color",			"Tints the objects texture colors");
				FullSailGUI.ShaderProperty(materialEditor, maintex,	"Main Texture",		"Main texture to use for the sail");
				FullSailGUI.ShaderProperty(materialEditor, speccol,	"Specular Color",	"Color of the specular highlights");
				FullSailGUI.ShaderProperty(materialEditor, gloss,	"Glossiness",		"How shiny the sail is");
				FullSailGUI.ShaderProperty(materialEditor, metal,	"Metallic",			"How metallic the sail is");
				FullSailGUI.ShaderProperty(materialEditor, bumpmap,	"Bump Map",			"Bump map to use for the flag");

				EditorGUILayout.EndVertical();
			}

			// Emblem
			FullSailGUI.FoldOut(ref emblemParams, "Emblem", "Emblem texture, colors and backface of sail options");
			if ( emblemParams )
			{
				MaterialProperty emblemtex		= FindProperty("_EmblemTex",		properties);
				MaterialProperty emblemcolor	= FindProperty("_EmblemColor",		properties);
				MaterialProperty emblememcolor	= FindProperty("_EmblemEmColor",	properties);
				MaterialProperty emblembackface	= FindProperty("_EmblemBackface",	properties);

				EditorGUILayout.BeginVertical("box");

				FullSailGUI.ShaderProperty(materialEditor, emblemtex,		"Texture",	"Texture to apply as an emblem to the sail");
				FullSailGUI.ShaderProperty(materialEditor, emblemcolor,		"Color",	"Tints the emblem color");
				FullSailGUI.ShaderProperty(materialEditor, emblememcolor,	"Emissive",	"Emblem can be made to glow with this color");
				FullSailGUI.ShaderProperty(materialEditor, emblembackface,	"Backface",	"Controls how much of the Emblem texture is shown on the back of the sail");

				EditorGUILayout.EndVertical();
			}

			// Damage values
			FullSailGUI.FoldOut(ref damageParams, "Damage", "Damage textures and damage value");
			if ( damageParams )
			{
				MaterialProperty damage		= FindProperty("_Damage",		properties);
				MaterialProperty damagetex1	= FindProperty("_DamageTex",	properties);
				MaterialProperty damagetex2	= FindProperty("_DamageTex1",	properties);
				MaterialProperty damagetex3	= FindProperty("_DamageTex2",	properties);
				MaterialProperty cutoff		= FindProperty("_Cutoff",		properties);

				EditorGUILayout.BeginVertical("box");

				FullSailGUI.ShaderProperty(materialEditor, damage,		"Damage",			"How damaged the sail is");
				FullSailGUI.ShaderProperty(materialEditor, damagetex1,	"Damage Tex",		"First level of damage texture");
				FullSailGUI.ShaderProperty(materialEditor, damagetex2,	"Damage Tex 1",		"Second level of damage texture");
				FullSailGUI.ShaderProperty(materialEditor, damagetex3,	"Damage Tex 2",		"Third level of damage texture");
				FullSailGUI.ShaderProperty(materialEditor, cutoff,		"Alpha  Cutoff",	"Alpha Cutoff value for Damage Alpha");

				EditorGUILayout.EndVertical();
			}

			// Sail params
			FullSailGUI.FoldOut(ref sailParams, "Sail", "All the sail shape controls as well as speed and directions");
			if ( sailParams )
			{
				MaterialProperty speed				= FindProperty("_Speed",			properties);
				MaterialProperty winddir			= FindProperty("_WindDir",			properties);
				MaterialProperty sailforward		= FindProperty("_SailForward",		properties);
				MaterialProperty maskmap			= FindProperty("_MaskMap",			properties);
				MaterialProperty sailwind			= FindProperty("_SailWind",			properties);
				MaterialProperty fillpercent		= FindProperty("_FillPercent",		properties);
				MaterialProperty saillift			= FindProperty("_SailLift",			properties);
				MaterialProperty sailliftarch		= FindProperty("_SailLiftArch",		properties);
				MaterialProperty sailliftsidearch	= FindProperty("_SailLiftSideArch",	properties);
				MaterialProperty sailarch			= FindProperty("_SailArch",			properties);
				MaterialProperty sailarchstart		= FindProperty("_SailArchStart",	properties);
				MaterialProperty sailsidearch		= FindProperty("_SailSideArch",		properties);
				MaterialProperty sailtoparch		= FindProperty("_SailTopArch",		properties);
				MaterialProperty sailtaper			= FindProperty("_SailTaper",		properties);
				MaterialProperty sailshear			= FindProperty("_SailShear",		properties);
				MaterialProperty sailtilt			= FindProperty("_SailTilt",			properties);
				MaterialProperty sailreverse		= FindProperty("_SailReverse",		properties);
				MaterialProperty fullripple			= FindProperty("_FullRipple",		properties);

				EditorGUILayout.BeginVertical("box");

				FullSailGUI.ShaderProperty(materialEditor, speed,				"Speed",				"Speed of the wind");
				FullSailGUI.ShaderProperty(materialEditor, winddir,				"Wind Direction",		"Direction vector of the wind");
				FullSailGUI.ShaderProperty(materialEditor, sailforward,			"Sail Forward",			"Forward vector of the Sail Mesh");
				FullSailGUI.ShaderProperty(materialEditor, maskmap,				"Mask Map",				"Texture for controlling the sail movement\nR controls the main sail movement due to wind.\nG controls the furling of the sail\nB Controls the furled states of the sail");
				FullSailGUI.ShaderProperty(materialEditor, sailwind,			"Sail Wind",			"How the sail fills with wind");
				FullSailGUI.ShaderProperty(materialEditor, fillpercent,			"Fill Percent",			"Used with the above value to control how much of the wind the sail captures");
				FullSailGUI.ShaderProperty(materialEditor, saillift,			"Sail Lift",			"Controls how the bottom of the sail lifts with the wind");
				FullSailGUI.ShaderProperty(materialEditor, sailliftarch,		"Arch from Lift",		"Controls how much bottom arch is introduced as the sail lifts");
				FullSailGUI.ShaderProperty(materialEditor, sailliftsidearch,	"Side Arch from Lift",	"Controls how much side arch is introduced as the sail lifts");
				FullSailGUI.ShaderProperty(materialEditor, sailarch,			"Arch",					"How arched is the bottom of the sail when at rest");
				FullSailGUI.ShaderProperty(materialEditor, sailarchstart,		"Arch Start",			"How far down the sail does the bottom arching start");
				FullSailGUI.ShaderProperty(materialEditor, sailsidearch,		"Side Arch",			"How arched are the sides of the sails");
				FullSailGUI.ShaderProperty(materialEditor, sailtoparch,			"Top Arch",				"Adds an arch to the top of the sail");
				FullSailGUI.ShaderProperty(materialEditor, sailtaper,			"Taper",				"Controls how the sail width changes at the bottom of the sail");
				FullSailGUI.ShaderProperty(materialEditor, sailshear,			"Shear",				"Controls how offset the bottom of the sail is from the top");
				FullSailGUI.ShaderProperty(materialEditor, sailtilt,			"Tilt",					"Controls how offset the bottom of the sail tilts");
				FullSailGUI.ShaderProperty(materialEditor, sailreverse,			"Reverse Amount",		"If the wind is blowing in reverse direct controls how much the sail moves in that case");
				FullSailGUI.ShaderProperty(materialEditor, fullripple,			"Full Ripple Reduce",	"Reduces the amount of the ripple as the sail is stretched by the wind");

				EditorGUILayout.EndVertical();
			}

			// Furling
			FullSailGUI.FoldOut(ref furlingParams, "Furling", "Options to control the furling of the sail");
			if ( furlingParams )
			{
				MaterialProperty furl		= FindProperty("_Furl",			properties);
				MaterialProperty furlradius	= FindProperty("_FurledRadius",	properties);

				EditorGUILayout.BeginVertical("box");

				EditorGUILayout.HelpBox("The Furling for the sail is controlled by the G channel of the Mask Map texture.\nThe Furled size is controlled by the B value in the maskmap.\nThere is a tool to help yon combine texture maps to build the control texture.", MessageType.Info);
				FullSailGUI.ShaderProperty(materialEditor, furl,		"Furl Amount",		"How furled the sail is");
				FullSailGUI.ShaderProperty(materialEditor, furlradius,	"Furled Radius",	"Radius of the furled sail");

				EditorGUILayout.EndVertical();
			}

			// Rippled
			FullSailGUI.FoldOut(ref rippleParams, "Ripple", "Ripple options controls the speed, size amount etc");
			if ( rippleParams )
			{
				MaterialProperty vorspeed		= FindProperty("_VorSpeed",			properties);
				MaterialProperty vorseed		= FindProperty("_VorSeed",			properties);
				MaterialProperty vorstrength	= FindProperty("_VorStrength",		properties);
				MaterialProperty vorscale		= FindProperty("_VorScale",			properties);
				MaterialProperty vorsmooth		= FindProperty("_VorSmoothness",	properties);
				MaterialProperty noise			= FindProperty("_RippleNoise", properties);

				EditorGUILayout.BeginVertical("box");

				FullSailGUI.ShaderProperty(materialEditor, vorspeed,	"Speed",		"Speed of the ripples on the sail");
				FullSailGUI.ShaderProperty(materialEditor, vorseed	,	"Seed",			"Offset value for each sail so they dont all animate the same way");
				FullSailGUI.ShaderProperty(materialEditor, vorstrength,	"Strength",		"How large the ripples are");
				FullSailGUI.ShaderProperty(materialEditor, vorscale,	"Scale",		"Size of the ripple cells that control the animation");
				FullSailGUI.ShaderProperty(materialEditor, vorsmooth,	"Smoothness",	"Smoothness of the ripples");
				FullSailGUI.ShaderProperty(materialEditor, noise,		"Ripple Noise", "Ripple Noise");

				EditorGUILayout.EndVertical();
			}

			MaterialProperty impactcount = FindProperty("_ImpactCount", properties, false);
			if ( impactcount != null )
			{
				FullSailGUI.FoldOut(ref impactParams, "Impacts", "Params for the Impact effects of the Sail");
				if ( impactParams )
				{
					MaterialProperty impacttex	= FindProperty("_ImpactTex",		properties);
					MaterialProperty ripple		= FindProperty("_ImpactRipple",		properties);
					MaterialProperty location	= FindProperty("_ImpactLocation",	properties);
					MaterialProperty impcutoff	= FindProperty("_ImpactCutoff",		properties);

					EditorGUILayout.BeginVertical("box");

					FullSailGUI.ShaderProperty(materialEditor, impactcount,	"Impact Count", "How many impacts to apply to the sail");
					FullSailGUI.ShaderProperty(materialEditor, impacttex,	"Impact Tiles", "The Impact tile texture");
					FullSailGUI.ShaderProperty(materialEditor, ripple,		"Ripple Texture(A)", "The wave effect texture");
					FullSailGUI.ShaderProperty(materialEditor, location,	"Location", "Location of the last hit(XY) Time(Z) Strength(W)");
					FullSailGUI.ShaderProperty(materialEditor, impcutoff,	"Impact Cutoff", "Impact texture alpha cutoff");

					EditorGUILayout.EndVertical();
				}
			}

		}
	}
}