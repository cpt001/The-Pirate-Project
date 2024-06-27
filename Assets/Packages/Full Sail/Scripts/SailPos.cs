using UnityEngine;

namespace FullSail
{
	[ExecuteAlways]
	public class SailPos : MonoBehaviour
	{
        const float			pi2	= 6.283185307179586476924f;
		const float			hpi	= 3.141592653589793238462f * 0.5f;

		// Can snap shot these and save in a class, and pass that in, up to user if a value changes to change it here 
		public Sail			sail;
		public SailGroup	group;
		[Range(0.0f, 1.0f)]
		public float		sailPosX;
		[Range(0.0f, 1.0f)]
		public float		sailPosY;
		public float		sailWidth	= 10.0f;
		public float		sailHeight	= 10.0f;
		Material			mat;
		Vector3				vertPos;
		Vector3				sailPos;

		float				_Speed		= 0.0f;
		Vector3				_WindDir;
		float				_SailWind;
		Vector3				_SailForward;
		float				_SailReverse;
		float				_FillPercent;
		float				_SailSideArch;
		Texture2D			_MaskMap;
		Texture2D			_MaskMapRev;
		float				_VorSeed;
		float				_VorSpeed;
		float				_VorScale;
		float				_VorStrength;
		float				_SailSideways;
		float				_SailLift;
		float				_SailArchStart;
		float				_SailLiftSideArch;
		float				_SailArch;
		float				_SailTopArch;
		float				_SailLiftArch;
		float				_SailTilt;
		Texture2D			_FurlMap;
		Texture2D			_FurledMap;
		float				_SailTaper;
		float				_SailShear;
		Vector4				_ImpactLocation;
		float				_Time;
		float				_FurledRadius;
		float				_Furl;
		Texture2D			_ImpactRipple;
		float				_FullRipple;
		Texture2D			_RippleNoise;

		float SimpleNoise1(Vector2 UV, float Scale)
		{
			if ( _RippleNoise )
			{
				float v = _RippleNoise.GetPixelBilinear(UV.x * Scale * 0.01f, UV.y * Scale * 0.01f).r;
				return v - 0.5f;
			}
			return 0.0f;
		}

		float wave(Vector2 position, Vector2 origin, float time)
		{
			float d = Vector2.Distance(position, origin);
			float t = time - (d * 0.5f);

			Color col = Color.gray;
			if ( _ImpactRipple )
				col = _ImpactRipple.GetPixelBilinear(t, 0.0f);
			return ((col.r - 0.5f) * 2.0f) * Mathf.Clamp01(1.0f - d);
		}

		Vector3 Deform(Vector2 uvin)
		{
			Vector3 locdir = _WindDir;
			Vector3 ldir = sail.transform.InverseTransformVector(_WindDir);
			locdir = ldir;

			float _sailWindLevel = (_SailWind);
			float dotamt = Vector3.Dot(ldir, _SailForward);
			float rev = 1.0f;
			float mask = 0.0f;

			if ( dotamt < 0 )
			{
				float swa = _sailWindLevel * _SailReverse * Mathf.Abs(dotamt);
				if ( swa > _sailWindLevel * _SailReverse )
					_sailWindLevel = _sailWindLevel * _SailReverse;
				else
					_sailWindLevel = swa;

				if ( _MaskMapRev )
					mask = _MaskMapRev.GetPixelBilinear(uvin.x, uvin.y).r;
			}
			else
			{
				if ( _MaskMap )
					mask = _MaskMap.GetPixelBilinear(uvin.x, uvin.y).r;
				_sailWindLevel *= dotamt;
			}

			Vector3 movedir = Vector3.Lerp(_SailForward * dotamt, locdir, _SailSideways);

			Vector2 nuv;
			nuv.x = uvin.x + ((_Time + _VorSeed) * _VorSpeed);
			nuv.y = uvin.y + ((_Time + _VorSeed) * _VorSpeed);
			float voroi = SimpleNoise1(nuv, _VorScale);

			Vector3 val = (_sailWindLevel * movedir * _Speed) * mask * rev;
			Vector3 rip = (mask * _SailForward * (rev * voroi * _VorStrength * _FillPercent * (1.0f - (_sailWindLevel * _FullRipple))));

			val += rip;

			float swls = _sailWindLevel * _Speed;
			float la = swls * (1.0f - uvin.y);
			val.y += la * _SailLift;

			// Side Arch
			float sa = Mathf.Abs((uvin.y * 2.0f) - 1.0f);
			float lsa = swls * _SailLiftSideArch;
			if ( uvin.x < 0.5 )
				val.x += (_SailSideArch + lsa) * Mathf.Cos(sa * hpi) * (1.0f - (uvin.x / 0.5f));
			else
				val.x -= (_SailSideArch + lsa) * Mathf.Cos(sa * hpi) * (1.0f - ((1.0f - uvin.x) / 0.5f));

			// Arch
			// bot arch
			float xa = 1.0f - Mathf.Abs((uvin.x * 2.0f) - 1.0f);
			float ya = 1.0f - (uvin.y / _SailArchStart);
			float sinx = Mathf.Sin(xa * hpi);
			if ( ya < 0.0f )
				ya = 0.0f;
			if ( uvin.y < _SailArchStart )
				val.y += (_SailArch + (la * _SailLiftArch)) * sinx * ya;

			// top arch
			val.y += (_SailTopArch * sinx * (1.0f - ya));

			// Impact ripple
			val.z += wave(uvin, _ImpactLocation, (_Time - _ImpactLocation.z)) * _ImpactLocation.w * mask;

			return val;
		}

		public Vector3 GetPos(Vector3 vertex, Vector2 texcoord1)
		{
			Vector3 defamt = Deform(texcoord1);
			float ft = 1.0f;
			Vector2 uv_Furl = texcoord1;	// * _FurlMap_ST.xy + _FurlMap_ST.zw;

			float f;
			if ( _FurlMap )
				f = _FurlMap.GetPixelBilinear(uv_Furl.x, uv_Furl.y).r;
			else
				f = 1.0f;

			float rf = _Furl;
			float fa = 0.0f;

			if ( f < rf )
			{
				ft = Mathf.Clamp01(1.0f - (rf - f));

				if ( rf > 1.0f )
					fa = rf - 1.0f;

				Vector3 furledvert;
				float frad;
				if ( _FurledMap )
					frad = _FurledRadius * _FurledMap.GetPixelBilinear(uv_Furl.x, 0.0f).r;
				else
					frad = _FurledRadius;

				furledvert.z = (Mathf.Sin(pi2 - (pi2 * texcoord1.y)) * frad);
				furledvert.y = (Mathf.Cos(pi2 - (pi2 * texcoord1.y)) * frad) - frad;
				furledvert.x = vertex.x;

				Vector3 vert = vertex + (defamt * (ft * ft));
				float xo = 1.0f - texcoord1.y;
				vert.y += _SailTilt * ((texcoord1.x * 2.0f) - 1.0f) * xo;
				vert.y *= ft;
				vert.x *= 1.0f + (_SailTaper * xo) * ft;
				vert.x += xo * ft * _SailShear;

				return Vector3.Lerp(vert, furledvert, fa);
			}
			else
			{
				Vector3 vert = vertex + defamt;
				float xo = 1.0f - texcoord1.y;
				vert.y += _SailTilt * ((texcoord1.x * 2.0f) - 1.0f) * xo;
				vert.y *= ft;
				vert.x *= 1.0f + (_SailTaper * xo) * ft;
				vert.x += xo * ft * _SailShear;

				return vert;
			}
		}

		float GetFloat(SailParamID id, Material mat)
		{
			if ( mat )
			{
				string pname = Sail.GetParamName(id);
				return mat.GetFloat(pname);
			}

			return 0.0f;
		}

		Vector4 GetVector(SailParamID id, Material mat)
		{
			if ( mat )
			{
				string pname = Sail.GetParamName(id);
				return mat.GetVector(pname);
			}

			return Vector4.zero;
		}

		Texture2D GetTexture(SailParamID id, Material mat)
		{
			if ( mat )
			{
				string pname = Sail.GetParamName(id);
				return (Texture2D)mat.GetTexture(pname);
			}

			return null;
		}

		float GetFloat(SailParamID id, Sail s, SailGroup g, Material m)
		{
			if ( s )
			{
				SailParam sp = s.GetParam(id);
				if ( sp != null )
					return sp.fval;
			}

			if ( g )
			{
				SailParam sp = g.GetParam(id);
				if ( sp != null )
					return sp.fval;
			}

			return GetFloat(id, m);
		}

		Vector4 GetVector(SailParamID id, Sail s, SailGroup g, Material m)
		{
			if ( s )
			{
				SailParam sp = s.GetParam(id);
				if ( sp != null )
					return sp.vval;
			}

			if ( g )
			{
				SailParam sp = g.GetParam(id);
				if ( sp != null )
					return sp.vval;
			}

			return GetVector(id, m);
		}

		Texture2D GetTexture(SailParamID id, Sail s, SailGroup g, Material m)
		{
			if ( s )
			{
				SailParam sp = s.GetParam(id);
				if ( sp != null )
					return sp.tval;
			}

			if ( g )
			{
				SailParam sp = g.GetParam(id);
				if ( sp != null )
					return sp.tval;
			}

			return GetTexture(id, m);
		}

		void GetParams()
		{
			mat = null;

			if ( sail )
				mat = sail.GetComponent<MeshRenderer>()?.sharedMaterial;

			if ( mat )
			{
				// Dyanmic
				_Speed				= GetFloat(SailParamID.Speed, sail, group, mat);
				_Furl				= GetFloat(SailParamID.Furl, sail, group, mat);
				_SailWind			= GetFloat(SailParamID.SailWind, sail, group, mat);
				_WindDir			= GetVector(SailParamID.WindDir, sail, group, mat);
				_ImpactLocation		= GetVector(SailParamID.ImpactLocation, sail, group, mat);
				_SailReverse		= GetFloat(SailParamID.Furl, sail, group, mat);
				_FillPercent		= GetFloat(SailParamID.FillPercent, sail, group, mat);
				_SailSideArch		= GetFloat(SailParamID.SailSideArch, sail, group, mat);
				_VorSeed			= GetFloat(SailParamID.RippleSeed, sail, group, mat);
				_VorSpeed			= GetFloat(SailParamID.RippleSpeed, sail, group, mat);
				_VorScale			= GetFloat(SailParamID.RippleScale, sail, group, mat);
				_VorStrength		= GetFloat(SailParamID.RippleStrength, sail, group, mat);
				_SailSideways		= GetFloat(SailParamID.SailSideways, sail, group, mat);
				_SailLift			= GetFloat(SailParamID.SailLift, sail, group, mat);
				_SailArchStart		= GetFloat(SailParamID.SailArchStart, sail, group, mat);
				_SailLiftSideArch	= GetFloat(SailParamID.SailLiftSideArch, sail, group, mat);
				_SailArch			= GetFloat(SailParamID.SailArch, sail, group, mat);
				_SailTopArch		= GetFloat(SailParamID.SailTopArch, sail, group, mat);
				_SailLiftArch		= GetFloat(SailParamID.SailLiftArch, sail, group, mat);
				_SailTilt			= GetFloat(SailParamID.SailTilt, sail, group, mat);
				_SailTaper			= GetFloat(SailParamID.SailTaper, sail, group, mat);
				_SailShear			= GetFloat(SailParamID.SailShear, sail, group, mat);
				//_Time				= GetFloat(SailParamID.Time, _sail, group, mat);
				_FurledRadius		= GetFloat(SailParamID.FurledRadius, sail, group, mat);
				_FullRipple			= GetFloat(SailParamID.FullRipple, sail, group, mat);
				_SailForward		= GetVector(SailParamID.SailForward, sail, group, mat);
				_MaskMap			= GetTexture(SailParamID.MaskMap, sail, group, mat);
				_MaskMapRev			= GetTexture(SailParamID.MaskMapRev, sail, group, mat);
				_FurlMap			= GetTexture(SailParamID.FurlMap, sail, group, mat);
				_FurledMap			= GetTexture(SailParamID.FurledMap, sail, group, mat);
				_ImpactRipple		= GetTexture(SailParamID.ImpactRipple, sail, group, mat);
				_RippleNoise		= GetTexture(SailParamID.RippleNoise, sail, group, mat);
			}
		}

		private void Start()
		{
			if ( !sail )
			{
				sail = GetComponent<Sail>();
				if ( !sail )
					sail = GetComponentInParent<Sail>();
			}

			if ( !group )
			{
				if ( sail )
					group = sail.GetComponentInParent<SailGroup>();
			}

			GetParams();
		}

		public Vector3 GetPos()
		{
			return sailPos;
		}

		// Update sets the position then ropes etc can attach to that
		private void LateUpdate()
		{
			if ( sail  )
			{
				if ( mat == null )
					GetParams();

				_Time			= Time.timeSinceLevelLoad;
				_Speed			= GetFloat(SailParamID.Speed, sail, group, mat);
				_Furl			= GetFloat(SailParamID.Furl, sail, group, mat);
				_SailWind		= GetFloat(SailParamID.SailWind, sail, group, mat);
				_WindDir		= GetVector(SailParamID.WindDir, sail, group, mat);
				_ImpactLocation	= GetVector(SailParamID.ImpactLocation, sail, group, mat);
				_VorSeed		= sail.GetAnimOffset();

				vertPos.x		= ((sailWidth * 0.5f) + 0.0f) - sailPosX * sailWidth;
				vertPos.y		= (-sailHeight + 0.0f) + sailPosY * sailHeight;
				vertPos.z		= 0.0f;

				sailPos = sail.transform.TransformPoint(GetPos(vertPos, new Vector2(sailPosX, sailPosY)));
				transform.position = sailPos;
			}
		}
	}
}