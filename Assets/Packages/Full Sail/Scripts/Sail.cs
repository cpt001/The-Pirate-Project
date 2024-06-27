using UnityEngine;
using System.Collections.Generic;

// TODO: damage should reduce flap/bulge as holes appear
// TODO: flapping bits when holes

// This script has a copy of the all the flag shader values. These values are applied via a MaterialPropertyBlock to the mesh renderer
// This allows you to have per sail values. This scrit also updates any LODs that are present.
// You can either use a script like this to control per sail values and then just have a single sail material on all or you can make
// multiple unique materials and add them as you need. Think the first way is better but you may well have you own plan.
// Check the shader for what each value does
// The sail needs to be part of a SailGroup to have the values applied
namespace FullSail
{
	// Ids for the various override params
	public enum SailParamID
	{
		None,
		Color,
		Albedo,
		RippleNoise,
		Emblem,
		EmblemColor,
		EmblemEMColor,
		EmblemBackface,
		Damage,
		DamageTex,
		DamageTex1,
		DamageTex2,
		BumpMap,
		SpecColor,
		Smoothness,
		AlphaCutoff,
		Speed,
		WindDir,
		SailForward,
		MaskMap,
		SailWind,
		FillPercent,
		SailLift,
		SailLiftArch,
		SailLiftSideArch,
		SailReverse,
		SailTaper,
		SailShear,
		SailArch,
		SailArchStart,
		FullRipple,
		Furl,
		FurlOrder,
		FurlMap,
		FurledMap,
		FurledRadius,
		RippleStrength,
		RippleSpeed,
		RippleSeed,
		RippleScale,
		RippleSmooth,
		Thickness,
		Power,
		Distortion,
		Scale,
		SubColor,
		SailSideArch,
		SailTilt,
		SailTopArch,
		SailSideways,
		MaskMapRev,
		ImpactCount,
		ImpactPoints,
		ImpactTex,
		ImpactRipple,
		ImpactLocation,
		YScale,
	}

	[System.Serializable]
	public class SailParam
	{
		public SailParamID	id;		// id of the param value
		public bool			active;	// is the override active
		public float		fval;	// float value for override
		public Color		cval;	// color value for override
		public Texture2D	tval;	// texture value for override
		public int			ival;	// int value for overrride
		public Vector4		vval;	// vector4 value for override
	}

	[ExecuteAlways]
	[HelpURL("http://www.macspeedee.com/full-sail/")]
	public class Sail : MonoBehaviour
	{
		public List<SailParam>			sailParams		= new List<SailParam>();
		public bool						dirty;
		MaterialPropertyBlock			pblock;
		MeshRenderer					mr;
		LODGroup						lodGroup;
		LOD[]							lods;
		float							animOffset;
		bool							visible;

		public const int				maxImpacts		= 16;
		public bool						showImpacts		= false;
		public int						impactCount		= 0;
		public Vector4[]				impactPoints	= new Vector4[maxImpacts];

		public Vector3					boundsSize		= new Vector3(10.0f, 10.0f, 2.0f);
		public Vector3					boundsOffset	= new Vector3(0.0f, -5.0f, 0.0f);

		private void OnBecameVisible()
		{
			visible = true;
		}

		private void OnBecameInvisible()
		{
			visible = false;
		}

		public void AddRipple(float x, float y, float size)
		{
			Vector4 loc = new Vector4(x, y, Time.timeSinceLevelLoad, size);
			SetValue(SailParamID.ImpactLocation, loc);
		}

		public void AddImpact(float x, float y, float size, int type, bool remove = false)
		{
			if ( remove && impactCount >= maxImpacts )
				RemoveImpact();

			if ( impactCount < maxImpacts )
			{
				impactPoints[impactCount] = new Vector4(x, y, size, (float)type);
				impactCount++;
			}
		}

		public void PatchImpact(int index)
		{
			if ( index < maxImpacts )
			{
				if ( impactPoints[index].w < 8.0f )
					impactPoints[index].w += 8.0f;
			}
		}

		public void UnPatchImpact(int index)
		{
			if ( index < maxImpacts )
			{
				if ( impactPoints[index].w >= 7.9f )
					impactPoints[index].w -= 8.0f;
			}
		}

		public void PatchImpact()
		{
			for ( int i = 0; i < maxImpacts; i++ )
			{
				if ( impactPoints[i].w < 8.0f )
				{
					impactPoints[i].w += 8.0f;
					break;
				}
			}
		}

		public void RemoveImpact(int index = 0)
		{
			if ( index < (maxImpacts - 1) && index < impactCount )
			{
				for ( int i = index; i < maxImpacts; i++ )
				{
					if ( i > 0 )
						impactPoints[i - 1] = impactPoints[i];
				}
				impactCount--;
			}
		}

		public void SetAnimOffset(float off)
		{
			animOffset = off;
		}

		public float GetAnimOffset()
		{
			return animOffset;
		}

		static public void SetParam(MaterialPropertyBlock pblock, SailParam sp)
		{
			if ( sp.active )
			{
				switch ( sp.id )
				{
					case SailParamID.Color:				pblock.SetColor("_Color", sp.cval); break;
					case SailParamID.Albedo:			if ( sp.tval ) pblock.SetTexture("_MainTex", sp.tval); break;
					case SailParamID.Speed:				pblock.SetFloat("_Speed", sp.fval); break;
					case SailParamID.RippleNoise:		if ( sp.tval ) pblock.SetTexture("_RippleNoise", sp.tval); break;
					case SailParamID.Emblem:			if ( sp.tval ) pblock.SetTexture("_EmblemTex", sp.tval); break;
					case SailParamID.EmblemColor:		pblock.SetColor("_EmblemColor", sp.cval); break;
					case SailParamID.EmblemEMColor:		pblock.SetColor("_EmblemEMColor", sp.cval); break;
					case SailParamID.EmblemBackface:	pblock.SetFloat("_EmblemBackface", sp.fval); break;
					case SailParamID.Damage:			pblock.SetFloat("_Damage", sp.fval); break;
					case SailParamID.DamageTex:			if ( sp.tval ) pblock.SetTexture("_DamageTex", sp.tval); break;
					case SailParamID.DamageTex1:		if ( sp.tval ) pblock.SetTexture("_DamageTex1", sp.tval); break;
					case SailParamID.DamageTex2:		if ( sp.tval ) pblock.SetTexture("_DamageTex2", sp.tval); break;
					case SailParamID.BumpMap:			if ( sp.tval ) pblock.SetTexture("_BumpMap", sp.tval); break;
					case SailParamID.SpecColor:			pblock.SetColor("_SpecColor", sp.cval); break;
					case SailParamID.Smoothness:		pblock.SetFloat("_Smoothness", sp.fval); break;
					case SailParamID.AlphaCutoff:		pblock.SetFloat("_AlphaCutoff", sp.fval); break;
					case SailParamID.WindDir:			pblock.SetVector("_WindDir", sp.vval); break;
					case SailParamID.SailForward:		pblock.SetVector("_SailForward", sp.vval); break;
					case SailParamID.MaskMap:			if ( sp.tval ) pblock.SetTexture("_MaskMap", sp.tval); break;
					case SailParamID.MaskMapRev:		if ( sp.tval ) pblock.SetTexture("_MaskMapRev", sp.tval); break;
					case SailParamID.SailWind:			pblock.SetFloat("_SailWind", sp.fval); break;
					case SailParamID.FillPercent:		pblock.SetFloat("_FillPercent", sp.fval); break;
					case SailParamID.SailLift:			pblock.SetFloat("_SailLift", sp.fval); break;
					case SailParamID.SailLiftArch:		pblock.SetFloat("_SailLiftArch", sp.fval); break;
					case SailParamID.SailLiftSideArch:	pblock.SetFloat("_SailLiftSideArch", sp.fval); break;
					case SailParamID.SailReverse:		pblock.SetFloat("_SailReverse", sp.fval); break;
					case SailParamID.SailTaper:			pblock.SetFloat("_SailTaper", sp.fval); break;
					case SailParamID.SailShear:			pblock.SetFloat("_SailShear", sp.fval); break;
					case SailParamID.SailTilt:			pblock.SetFloat("_SailTilt", sp.fval); break;
					case SailParamID.SailArch:			pblock.SetFloat("_SailArch", sp.fval); break;
					case SailParamID.SailTopArch:		pblock.SetFloat("_SailTopArch", sp.fval); break;
					case SailParamID.SailArchStart:		pblock.SetFloat("_SailArchStart", sp.fval); break;
					case SailParamID.SailSideArch:		pblock.SetFloat("_SailSideArch", sp.fval); break;
					case SailParamID.FullRipple:		pblock.SetFloat("_FullRipple", sp.fval); break;
					case SailParamID.Furl:				pblock.SetFloat("_Furl", sp.fval); break;
					case SailParamID.FurlOrder:			pblock.SetInt("_FurlOrder", sp.ival); break;
					case SailParamID.FurlMap:			if ( sp.tval ) pblock.SetTexture("_FurlMap", sp.tval); break;
					case SailParamID.FurledMap:			if ( sp.tval ) pblock.SetTexture("_FurledMap", sp.tval); break;
					case SailParamID.FurledRadius:		pblock.SetFloat("_FurledRadius", sp.fval); break;
					case SailParamID.RippleStrength:	pblock.SetFloat("_VorStrength", sp.fval); break;
					case SailParamID.RippleSpeed:		pblock.SetFloat("_VorSpeed", sp.fval); break;
					case SailParamID.RippleSeed:		pblock.SetFloat("_VorSeed", sp.fval); break;
					case SailParamID.RippleScale:		pblock.SetFloat("_VorScale", sp.fval); break;
					case SailParamID.RippleSmooth:		pblock.SetFloat("_VorSmoothness", sp.fval); break;
					case SailParamID.Thickness:			if ( sp.tval ) pblock.SetTexture("_Thickness", sp.tval); break;
					case SailParamID.Power:				pblock.SetFloat("_Power", sp.fval); break;
					case SailParamID.Distortion:		pblock.SetFloat("_Distortion", sp.fval); break;
					case SailParamID.Scale:				pblock.SetFloat("_Scale", sp.fval); break;
					case SailParamID.SubColor:			pblock.SetColor("_SubColor", sp.cval); break;
					case SailParamID.SailSideways:		pblock.SetFloat("_SailSideways", sp.fval); break;
					case SailParamID.ImpactCount:		pblock.SetInt("_ImpactCount", sp.ival); break;
					case SailParamID.ImpactPoints:		break;
					case SailParamID.ImpactTex:			if ( sp.tval ) pblock.SetTexture("_ImpactTex", sp.tval); break;
					case SailParamID.ImpactRipple:		if ( sp.tval ) pblock.SetTexture("_ImpactRipple", sp.tval); break;
					case SailParamID.ImpactLocation:	pblock.SetVector("_ImpactLocation", sp.vval); break;
					case SailParamID.YScale:			pblock.SetFloat("_YScale", sp.fval); break;
				}
			}
		}

		static public void SetParamFromID(MaterialPropertyBlock pblock, SailParam sp)
		{
			if ( sp.active )
			{
				switch ( sp.id )
				{
					case SailParamID.Color:				pblock.SetColor(SailShaderIDs._Color, sp.cval); break;
					case SailParamID.Albedo:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._MainTex, sp.tval); break;
					case SailParamID.Speed:				pblock.SetFloat(SailShaderIDs._Speed, sp.fval); break;
					case SailParamID.RippleNoise:		if ( sp.tval ) pblock.SetTexture(SailShaderIDs._RippleNoise, sp.tval); break;
					case SailParamID.Emblem:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._Emblem, sp.tval); break;
					case SailParamID.EmblemColor:		pblock.SetColor(SailShaderIDs._EmblemColor, sp.cval); break;
					case SailParamID.EmblemEMColor:		pblock.SetColor(SailShaderIDs._EmblemEMColor, sp.cval); break;
					case SailParamID.EmblemBackface:	pblock.SetFloat(SailShaderIDs._EmblemBackface, sp.fval); break;
					case SailParamID.Damage:			pblock.SetFloat(SailShaderIDs._Damage, sp.fval); break;
					case SailParamID.DamageTex:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._DamageTex, sp.tval); break;
					case SailParamID.DamageTex1:		if ( sp.tval ) pblock.SetTexture(SailShaderIDs._DamageTex1, sp.tval); break;
					case SailParamID.DamageTex2:		if ( sp.tval ) pblock.SetTexture(SailShaderIDs._DamageTex2, sp.tval); break;
					case SailParamID.BumpMap:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._BumpMap, sp.tval); break;
					case SailParamID.SpecColor:			pblock.SetColor(SailShaderIDs._SpecColor, sp.cval); break;
					case SailParamID.Smoothness:		pblock.SetFloat(SailShaderIDs._Smoothness, sp.fval); break;
					case SailParamID.AlphaCutoff:		pblock.SetFloat(SailShaderIDs._AlphaCutoff, sp.fval); break;
					case SailParamID.WindDir:			pblock.SetVector(SailShaderIDs._WindDir, sp.vval); break;
					case SailParamID.SailForward:		pblock.SetVector(SailShaderIDs._SailForward, sp.vval); break;
					case SailParamID.MaskMap:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._MaskMap, sp.tval); break;
					case SailParamID.MaskMapRev:		if ( sp.tval ) pblock.SetTexture(SailShaderIDs._MaskMapRev, sp.tval); break;
					case SailParamID.SailWind:			pblock.SetFloat(SailShaderIDs._SailWind, sp.fval); break;
					case SailParamID.FillPercent:		pblock.SetFloat(SailShaderIDs._FillPercent, sp.fval); break;
					case SailParamID.SailLift:			pblock.SetFloat(SailShaderIDs._SailLift, sp.fval); break;
					case SailParamID.SailLiftArch:		pblock.SetFloat(SailShaderIDs._SailLiftArch, sp.fval); break;
					case SailParamID.SailLiftSideArch:	pblock.SetFloat(SailShaderIDs._SailLiftSideArch, sp.fval); break;
					case SailParamID.SailReverse:		pblock.SetFloat(SailShaderIDs._SailReverse, sp.fval); break;
					case SailParamID.SailTaper:			pblock.SetFloat(SailShaderIDs._SailTaper, sp.fval); break;
					case SailParamID.SailShear:			pblock.SetFloat(SailShaderIDs._SailShear, sp.fval); break;
					case SailParamID.SailTilt:			pblock.SetFloat(SailShaderIDs._SailTilt, sp.fval); break;
					case SailParamID.SailArch:			pblock.SetFloat(SailShaderIDs._SailArch, sp.fval); break;
					case SailParamID.SailTopArch:		pblock.SetFloat(SailShaderIDs._SailTopArch, sp.fval); break;
					case SailParamID.SailArchStart:		pblock.SetFloat(SailShaderIDs._SailArchStart, sp.fval); break;
					case SailParamID.SailSideArch:		pblock.SetFloat(SailShaderIDs._SailSideArch, sp.fval); break;
					case SailParamID.FullRipple:		pblock.SetFloat(SailShaderIDs._FullRipple, sp.fval); break;
					case SailParamID.Furl:				pblock.SetFloat(SailShaderIDs._Furl, sp.fval); break;
					case SailParamID.FurlOrder:			pblock.SetInt(SailShaderIDs._FurlOrder, sp.ival); break;
					case SailParamID.FurlMap:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._FurlMap, sp.tval); break;
					case SailParamID.FurledMap:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._FurledMap, sp.tval); break;
					case SailParamID.FurledRadius:		pblock.SetFloat(SailShaderIDs._FurledRadius, sp.fval); break;
					case SailParamID.RippleStrength:	pblock.SetFloat(SailShaderIDs._RippleStrength, sp.fval); break;
					case SailParamID.RippleSpeed:		pblock.SetFloat(SailShaderIDs._RippleSpeed, sp.fval); break;
					case SailParamID.RippleSeed:		pblock.SetFloat(SailShaderIDs._RippleSeed, sp.fval); break;
					case SailParamID.RippleScale:		pblock.SetFloat(SailShaderIDs._RippleScale, sp.fval); break;
					case SailParamID.RippleSmooth:		pblock.SetFloat(SailShaderIDs._RippleSmooth, sp.fval); break;
					case SailParamID.Thickness:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._Thickness, sp.tval); break;
					case SailParamID.Power:				pblock.SetFloat(SailShaderIDs._Power, sp.fval); break;
					case SailParamID.Distortion:		pblock.SetFloat(SailShaderIDs._Distortion, sp.fval); break;
					case SailParamID.Scale:				pblock.SetFloat(SailShaderIDs._Scale, sp.fval); break;
					case SailParamID.SubColor:			pblock.SetColor(SailShaderIDs._SubColor, sp.cval); break;
					case SailParamID.SailSideways:		pblock.SetFloat(SailShaderIDs._SailSideways, sp.fval); break;
					case SailParamID.ImpactCount:		pblock.SetInt(SailShaderIDs._ImpactCount, sp.ival); break;
					case SailParamID.ImpactPoints:		break;
					case SailParamID.ImpactTex:			if ( sp.tval ) pblock.SetTexture(SailShaderIDs._ImpactTex, sp.tval); break;
					case SailParamID.ImpactRipple:		if ( sp.tval ) pblock.SetTexture(SailShaderIDs._ImpactRipple, sp.tval); break;
					case SailParamID.ImpactLocation:	pblock.SetVector(SailShaderIDs._ImpactLocation, sp.vval); break;
					case SailParamID.YScale:			pblock.SetFloat(SailShaderIDs._YScale, sp.fval); break;
				}
			}
		}

		static public string GetParamName(SailParamID id)
		{
			switch ( id )
			{
				case SailParamID.Color:				return "_Color";
				case SailParamID.Albedo:			return "_MainTex";
				case SailParamID.Speed:				return "_Speed";
				case SailParamID.RippleNoise:		return "_RippleNoise";
				case SailParamID.Emblem:			return "_EmblemTex";
				case SailParamID.EmblemColor:		return "_EmblemColor";
				case SailParamID.EmblemEMColor:		return "_EmblemEMColor";
				case SailParamID.EmblemBackface:	return "_EmblemBackface";
				case SailParamID.Damage:			return "_Damage";
				case SailParamID.DamageTex:			return "_DamageTex";
				case SailParamID.DamageTex1:		return "_DamageTex1";
				case SailParamID.DamageTex2:		return "_DamageTex2";
				case SailParamID.BumpMap:			return "_BumpMap";
				case SailParamID.SpecColor:			return "_SpecColor";
				case SailParamID.Smoothness:		return "_Smoothness";
				case SailParamID.AlphaCutoff:		return "_AlphaCutoff";
				case SailParamID.WindDir:			return "_WindDir";
				case SailParamID.SailForward:		return "_SailForward";
				case SailParamID.MaskMap:			return "_MaskMap";
				case SailParamID.MaskMapRev:		return "_MaskMapRev";
				case SailParamID.SailWind:			return "_SailWind";
				case SailParamID.FillPercent:		return "_FillPercent";
				case SailParamID.SailLift:			return "_SailLift";
				case SailParamID.SailLiftArch:		return "_SailLiftArch";
				case SailParamID.SailLiftSideArch:	return "_SailLiftSideArch";
				case SailParamID.SailReverse:		return "_SailReverse";
				case SailParamID.SailTaper:			return "_SailTaper";
				case SailParamID.SailShear:			return "_SailShear";
				case SailParamID.SailTilt:			return "_SailTilt";
				case SailParamID.SailArch:			return "_SailArch";
				case SailParamID.SailTopArch:		return "_SailTopArch";
				case SailParamID.SailArchStart:		return "_SailArchStart";
				case SailParamID.SailSideArch:		return "_SailSideArch";
				case SailParamID.FullRipple:		return "_FullRipple";
				case SailParamID.Furl:				return "_Furl";
				case SailParamID.FurlOrder:			return "_FurlOrder";
				case SailParamID.FurlMap:			return "_FurlMap";
				case SailParamID.FurledMap:			return "_FurledMap";
				case SailParamID.FurledRadius:		return "_FurledRadius";
				case SailParamID.RippleStrength:	return "_VorStrength";
				case SailParamID.RippleSpeed:		return "_VorSpeed";
				case SailParamID.RippleSeed:		return "_VorSeed";
				case SailParamID.RippleScale:		return "_VorScale";
				case SailParamID.RippleSmooth:		return "_VorSmoothness";
				case SailParamID.Thickness:			return "_Thickness";
				case SailParamID.Power:				return "_Power";
				case SailParamID.Distortion:		return "_Distortion";
				case SailParamID.Scale:				return "_Scale";
				case SailParamID.SubColor:			return "_SubColor";
				case SailParamID.SailSideways:		return "_SailSideways";
				case SailParamID.ImpactCount:		return "_ImpactCount";
				case SailParamID.ImpactPoints:		return "_ImpactPoints";
				case SailParamID.ImpactTex:			return "_ImpactTex";
				case SailParamID.ImpactRipple:		return "_ImpactRipple";
				case SailParamID.ImpactLocation:	return "_ImpactLocation";
			}

			return "";
		}

		// Use this to change a float param value, this will automatically set the param to dirty so it will update
		public void SetValue(SailParamID id, float val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.fval = val;
				dirty = true;
			}
		}

		// Use this to change a texture param value, this will automatically set the param to dirty so it will update
		public void SetValue(SailParamID id, Texture2D val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.tval = val;
				dirty = true;
			}
		}

		// Use this to change an int param value, this will automatically set the param to dirty so it will update
		public void SetValue(SailParamID id, int val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.ival = val;
				dirty = true;
			}
		}

		// Use this to change a Color param value, this will automatically set the param to dirty so it will update
		public void SetColor(SailParamID id, Color val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.cval = val;
				dirty = true;
			}
		}

		// Use this to change a vector param value, this will automatically set the param to dirty so it will update
		public void SetValue(SailParamID id, Vector4 val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.vval = val;
				dirty = true;
			}
		}

		// Call this if you make any changes to a sail param not using the SetValue method so that the material property block gets updated
		public void SetDirty(bool _dirty)
		{
			dirty = _dirty;
		}

		// Get the override param from the override list, then can set the value outside the script
		public SailParam GetParam(SailParamID id)
		{
			for ( int i = 0; i < sailParams.Count; i++ )
			{
				if ( sailParams[i].id == id )
					return sailParams[i];
			}

			return null;
		}

		void Start()
		{
			if ( impactPoints == null || impactPoints.Length < maxImpacts )
				impactPoints = new Vector4[maxImpacts];

			lodGroup = GetComponent<LODGroup>();

			if ( !lodGroup )
				mr = GetComponent<MeshRenderer>();
			else
				lods = lodGroup.GetLODs();

			UpdateBounds();
		}

		public void UpdateBounds()
		{
			MeshFilter mf = GetComponent<MeshFilter>();
			if ( mf )
			{
				Mesh mesh = mf.sharedMesh;
				if ( mesh )
				{
					Bounds b = mesh.bounds;
					if ( boundsSize.x == 0.0f )
					{
						boundsOffset = b.center;
						boundsSize	= b.size;
					}
					b.size = boundsSize;
					b.center = boundsOffset;
					mesh.bounds = b;
				}
			}
		}

		public void DoUpdate(List<SailParam> groupParams, bool groupdirty)
		{
			if ( !visible )
				return;

			if ( pblock == null )
				pblock = new MaterialPropertyBlock();

			if ( groupdirty || dirty )
			{
				pblock.Clear();

				for ( int i = 0; i < groupParams.Count; i++ )
				{
					//SetParam(pblock, groupParams[i]);
					SetParamFromID(pblock, groupParams[i]);
				}

				if ( sailParams != null )
				{
					for ( int i = 0; i < sailParams.Count; i++ )
					{
						//SetParam(pblock, sailParams[i]);
						SetParamFromID(pblock, sailParams[i]);
					}
				}

				//pblock.SetFloat("_VorSeed", animOffset);
				pblock.SetFloat(SailShaderIDs._RippleSeed, animOffset);
				Vector3 lscl = transform.localScale;

				pblock.SetFloat(SailShaderIDs._YScale, lscl.y);

				// Impacts
				//pblock.SetInt("_ImpactCount", impactCount);
				//if ( impactPoints.Length > 0 )
				//	pblock.SetVectorArray("_ImpactPoints", impactPoints);

				pblock.SetInt(SailShaderIDs._ImpactCount, impactCount);
				if ( impactPoints.Length > 0 )
					pblock.SetVectorArray(SailShaderIDs._ImpactPoints, impactPoints);
			}

			dirty = false;

			if ( mr )
				mr.SetPropertyBlock(pblock);
			else
			{
				if ( lodGroup )
				{
					if ( lods == null )
						lods = lodGroup.GetLODs();

					for ( int i = 0; i < lods.Length; i++ )
					{
						for ( int j = 0; j < lods[i].renderers.Length; j++ )
							lods[i].renderers[j].SetPropertyBlock(pblock);
					}
				}
			}
		}

		public Vector3 GetPointOnSail(float x, float y)
		{
			Vector3 pos = Vector3.zero;

			return pos;
		}
	}
}