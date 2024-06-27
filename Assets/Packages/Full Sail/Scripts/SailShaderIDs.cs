using UnityEngine;

namespace FullSail
{
	public class SailShaderIDs
	{
		static public int	_Color;
		static public int	_MainTex;
		static public int	_Speed;
		static public int	_RippleNoise;
		static public int	_Emblem;
		static public int	_EmblemColor;
		static public int	_EmblemEMColor;
		static public int	_EmblemBackface;
		static public int	_Damage;
		static public int	_DamageTex;
		static public int	_DamageTex1;
		static public int	_DamageTex2;
		static public int	_BumpMap;
		static public int	_SpecColor;
		static public int	_Smoothness;
		static public int	_AlphaCutoff;
		static public int	_WindDir;
		static public int	_SailForward;
		static public int	_MaskMap;
		static public int	_MaskMapRev;
		static public int	_SailWind;
		static public int	_FillPercent;
		static public int	_SailLift;
		static public int	_SailLiftArch;
		static public int	_SailLiftSideArch;
		static public int	_SailReverse;
		static public int	_SailTaper;
		static public int	_SailShear;
		static public int	_SailTilt;
		static public int	_SailArch;
		static public int	_SailTopArch;
		static public int	_SailArchStart;
		static public int	_SailSideArch;
		static public int	_FullRipple;
		static public int	_Furl;
		static public int	_FurlOrder;
		static public int	_FurlMap;
		static public int	_FurledMap;
		static public int	_FurledRadius;
		static public int	_RippleStrength;
		static public int	_RippleSpeed;
		static public int	_RippleSeed;
		static public int	_RippleScale;
		static public int	_RippleSmooth;
		static public int	_Thickness;
		static public int	_Power;
		static public int	_Distortion;
		static public int	_Scale;
		static public int	_SubColor;
		static public int	_SailSideways;
		static public int	_ImpactCount;
		static public int	_ImpactPoints;
		static public int	_ImpactTex;
		static public int	_ImpactRipple;
		static public int	_ImpactLocation;
		static public int	_YScale;

		static bool	haveIds	= false;

		static public void MakeIds()
		{
			if ( haveIds )
				return;

			_Color				= Shader.PropertyToID("_Color");
			_MainTex			= Shader.PropertyToID("_MainTex");
			_Speed				= Shader.PropertyToID("_Speed");
			_RippleNoise		= Shader.PropertyToID("_RippleNoise");
			_Emblem				= Shader.PropertyToID("_EmblemTex");
			_EmblemColor		= Shader.PropertyToID("_EmblemColor");
			_EmblemEMColor		= Shader.PropertyToID("_EmblemEMColor");
			_EmblemBackface		= Shader.PropertyToID("_EmblemBackFace");
			_Damage				= Shader.PropertyToID("_Damage");
			_DamageTex			= Shader.PropertyToID("_DamageTex");
			_DamageTex1			= Shader.PropertyToID("_DamageTex1");
			_DamageTex2			= Shader.PropertyToID("_DamageTex2");
			_BumpMap			= Shader.PropertyToID("_BumpMap");
			_SpecColor			= Shader.PropertyToID("_SpecColor");
			_Smoothness			= Shader.PropertyToID("_Smoothness");
			_AlphaCutoff		= Shader.PropertyToID("_AlphaCutoff");
			_WindDir			= Shader.PropertyToID("_WindDir");
			_SailForward		= Shader.PropertyToID("_SailForward");
			_MaskMap			= Shader.PropertyToID("_MaskMap");
			_MaskMapRev			= Shader.PropertyToID("_MaskMapRev");
			_SailWind			= Shader.PropertyToID("_SailWind");
			_FillPercent		= Shader.PropertyToID("_FillPercent");
			_SailLift			= Shader.PropertyToID("_SailLift");
			_SailLiftArch		= Shader.PropertyToID("_SailLiftArch");
			_SailLiftSideArch	= Shader.PropertyToID("_SailLiftSideArch");
			_SailReverse		= Shader.PropertyToID("_SailReverse");
			_SailTaper			= Shader.PropertyToID("_SailTaper");
			_SailShear			= Shader.PropertyToID("_SailShear");
			_SailTilt			= Shader.PropertyToID("_SailTilt");
			_SailArch			= Shader.PropertyToID("_SailArch");
			_SailTopArch		= Shader.PropertyToID("_SailTopArch");
			_SailArchStart		= Shader.PropertyToID("_SailArchStart");
			_SailSideArch		= Shader.PropertyToID("_SailSideArch");
			_FullRipple			= Shader.PropertyToID("_FullRipple");
			_Furl				= Shader.PropertyToID("_Furl");
			_FurlOrder			= Shader.PropertyToID("_FurlOrder");
			_FurlMap			= Shader.PropertyToID("_FurlMap");
			_FurledMap			= Shader.PropertyToID("_FurledMap");
			_FurledRadius		= Shader.PropertyToID("_FurledRadius");
			_RippleStrength		= Shader.PropertyToID("_VorStrength");
			_RippleSpeed		= Shader.PropertyToID("_VorSpeed");
			_RippleSeed			= Shader.PropertyToID("_VorSeed");
			_RippleScale		= Shader.PropertyToID("_VorScale");
			_RippleSmooth		= Shader.PropertyToID("_VorSmoothness");
			_Thickness			= Shader.PropertyToID("_Thickness");
			_Power				= Shader.PropertyToID("_Power");
			_Distortion			= Shader.PropertyToID("_Distortion");
			_Scale				= Shader.PropertyToID("_Scale");
			_SubColor			= Shader.PropertyToID("_SubColor");
			_SailSideways		= Shader.PropertyToID("_SailSideways");
			_ImpactCount		= Shader.PropertyToID("_ImpactCount");
			_ImpactPoints		= Shader.PropertyToID("_ImpactPoints");
			_ImpactTex			= Shader.PropertyToID("_ImpactTex");
			_ImpactRipple		= Shader.PropertyToID("_ImpactRipple");
			_ImpactLocation		= Shader.PropertyToID("_ImpactLocation");
			_YScale				= Shader.PropertyToID("_YScale");

			haveIds = true;
		}
	}
}