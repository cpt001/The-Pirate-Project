float wave(float2 position, float2 origin, float time)
{
	float d = length(position - origin);
	float t = time - (d * 0.5);
	return ((tex2Dlod(_ImpactRipple, float4(t, 0, 0, 0)).r - 0.5) * 2) * clamp(1 - d, 0, 1);
}

float SimpleNoise1(float2 UV, float Scale)
{
	float v = tex2Dlod(_RippleNoise, float4(UV.x * Scale * 0.01, UV.y * Scale * 0.01, 0, 0)).r;
	return v - 0.5;
}

float3 Deform(float2 uvin)
{
	float3 locdir = _WindDir;
	float3 ldir = mul(unity_WorldToObject, _WindDir);
	locdir = ldir;

	float _sailWindLevel = (_SailWind);
	float dotamt = dot(ldir, _SailForward);
	float rev = 1.0;
	float mask;

	if ( dotamt < 0 )
	{
		float swa = _sailWindLevel * _SailReverse * abs(dotamt);
		if ( swa > _sailWindLevel * _SailReverse )
			_sailWindLevel = _sailWindLevel * _SailReverse;
		else
			_sailWindLevel = swa;

		mask = tex2Dlod(_MaskMapRev, float4(uvin, 0, 0)).r;
	}
	else
	{
		mask = tex2Dlod(_MaskMap, float4(uvin, 0, 0)).r;
		_sailWindLevel *= dotamt;
	}

	float3 movedir = (lerp(_SailForward * dotamt, locdir, _SailSideways));
	float vs = _VorSeed;

	float voroi = SimpleNoise1(uvin + ((_Time.y + vs) * _VorSpeed), _VorScale);
	float3 val = (_sailWindLevel * movedir * _Speed) * mask * rev;	// +(mask * locdir * (rev * voroi * _VorStrength * _FillPercent * (1.0 - (_sailWindLevel * _FullRipple)))));

	float3 rip = (mask * _SailForward * (rev * voroi * _VorStrength * _FillPercent * (1.0 - (_sailWindLevel * _FullRipple))));

	val += rip;

	float swls = _sailWindLevel * _Speed;
	float la = swls * (1.0 - uvin.y);
	val.y += la * _SailLift;

	// Side Arch
	float sa = abs((uvin.y * 2.0) - 1.0);
	float lsa = swls * _SailLiftSideArch;
	if ( uvin.x < 0.5 )
		val.x += (_SailSideArch + lsa) * cos(sa * hpi) * (1.0 - (uvin.x / 0.5));
	else
		val.x -= (_SailSideArch + lsa) * cos(sa * hpi) * (1.0 - ((1.0 - uvin.x) / 0.5));

	// Arch
	// bot arch
	float xa = 1.0 - abs((uvin.x * 2.0) - 1.0);
	float ya = 1.0 - (uvin.y / _SailArchStart);
	float sinx = sin(xa * hpi);
	if ( ya < 0.0 )
		ya = 0.0;
	if ( uvin.y < _SailArchStart )
		val.y += (_SailArch + (la * _SailLiftArch)) * sinx * ya;

	// top arch
	val.y += (_SailTopArch * sinx * (1 - ya));

	// Impact ripple
	val.z += wave(uvin.xy, _ImpactLocation.xy, (_Time.y - _ImpactLocation.z)) * _ImpactLocation.w * mask;

	return val;
}

float3 Deform(float2 uvin, float4 mask)
{
	float3 locdir = _WindDir;
	float3 ldir = mul(unity_WorldToObject, _WindDir);
	locdir = ldir;

	float _sailWindLevel = (_SailWind);	// * _FillPercent);
	float dotamt = dot(ldir, _SailForward);
	float rev = 1.0;
	//float mask;

	if ( dotamt < 0 )
	{
		//rev = abs(dotamt);	//lerp(1.0, _SailReverse, abs(dotamt));
		//if ( rev > _SailReverse )
		//	rev = _SailReverse;
		float swa = _sailWindLevel * _SailReverse * abs(dotamt);
		if ( swa > _sailWindLevel * _SailReverse )
			_sailWindLevel = _sailWindLevel * _SailReverse;
		else
			_sailWindLevel = swa;

		mask = tex2Dlod(_MaskMapRev, float4(uvin, 0, 0));
	}
	else
	{
		//mask = tex2Dlod(_MaskMap, float4(uvin, 0, 0)).r;
		_sailWindLevel *= dotamt;
	}

	float3 movedir = (lerp(_SailForward * dotamt, locdir, _SailSideways));

	float voroi = SimpleNoise1(uvin + ((_Time.y + _VorSeed)  * _VorSpeed), _VorScale);

	float3 val = (_sailWindLevel * movedir * _Speed) * mask * rev;	// +(mask * locdir * (rev * voroi * _VorStrength * _FillPercent * (1.0 - (_sailWindLevel * _FullRipple)))));

	float3 rip = (mask * _SailForward * (rev * voroi * _VorStrength * _FillPercent * (1.0 - (_sailWindLevel * _FullRipple))));

	val += rip;

	float swls = _sailWindLevel * _Speed;
	float la = swls * (1.0 - uvin.y);
	val.y += la * _SailLift;

	// Side Arch
	float sa = abs((uvin.y * 2.0) - 1.0);
	float lsa = swls * _SailLiftSideArch;
	if ( uvin.x < 0.5 )
		val.x += (_SailSideArch + lsa) * cos(sa * hpi) * (1.0 - (uvin.x / 0.5));
	else
		val.x -= (_SailSideArch + lsa) * cos(sa * hpi) * (1.0 - ((1.0 - uvin.x) / 0.5));

	// Arch
	// bot arch
	float xa = 1.0 - abs((uvin.x * 2.0) - 1.0);
	float ya = 1.0 - (uvin.y / _SailArchStart);
	float sinx = sin(xa * hpi);
	if ( ya < 0.0 )
		ya = 0.0;
	if ( uvin.y < _SailArchStart )
		val.y += (_SailArch + (la * _SailLiftArch)) * sinx * ya;

	// top arch
	val.y += (_SailTopArch * sinx * (1 - ya));

	// Impact ripple
	val.z += wave(uvin.xy, _ImpactLocation.xy, (_Time.y - _ImpactLocation.z)) * _ImpactLocation.w * mask;

	return val;
}

