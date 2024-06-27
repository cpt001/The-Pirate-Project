using UnityEngine;
using System;
using System.Collections.Generic;

namespace FullRig
{
	[System.Serializable]
	public class RopeAttach
	{
		public Transform	obj;
		public Vector3		rot;
		[Range(0, 1)]
		public float		alpha;
	}

	[ExecuteAlways, RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class Rope : MonoBehaviour
	{
		public Transform		startAttach;
		public Transform		endAttach;
		public float			length			= 5.0f;	// The real-world length of the catenary (limited by the distance between the start/end point)
		public float			widthStart		= 0.0f; // The scale multiplier of the yz-axes of the mesh
		public float			widthEnd		= 0.0f; // The scale multiplier of the yz-axes of the mesh
		public float			swingAngle		= 0.5f;	// The catenary swing angle in radians
		public float			swingFreq		= 2.0f;	// The catenary swing frequency
		public float			swingOffset;
		[Range(0, 1)]
		public float			alpha			= 0.0f;
		public RopeEnd			startConnect;
		public Transform		startObj;
		public RopeEnd			endConnect;
		public Transform		endObj;
		public float			ropeForce		= 0.0f;
		public float			stretch			= 8.0f;
		[Range(0, 1)]
		public float			ropeStart;
		[Range(0, 1)]
		public float			ropeEnd;

		[Range(0, 1)]
		public float			ropeStartAttach;
		[Range(0, 1)]
		public float			ropeEndAttach;

		[Range(-0.2f, 0.2f)]
		public float			adjustZ			= 0.0f;
		[Range(-0.2f, 0.2f)]
		public float			adjustY			= 0.0f;

		[Range(-0.2f, 0.2f)]
		public float			adjustZEnd		= 0.0f;
		[Range(-0.2f, 0.2f)]
		public float			adjustYEnd		= 0.0f;

		public RopeAttach[]		attached;
		public List<RopeAttach>	attachedObjects	= new List<RopeAttach>();

		bool					dirty			= true;
		MaterialPropertyBlock	pblock;
		MeshRenderer			mr;
		float					lastLength;
		[SerializeField, HideInInspector]
		Vector3					p0;
		[SerializeField, HideInInspector]
		Vector3					p1;
		[SerializeField, HideInInspector]
		float					a;
		[SerializeField, HideInInspector]
		float					p;
		[SerializeField, HideInInspector]
		float					q;
		[SerializeField, HideInInspector]
		float					arcl;
		[SerializeField, HideInInspector]
		float					flipx;
		Vector4					adjust;
		Vector4					width;
		Rigidbody				rbs;
		Rigidbody				rbe;
		Rope					ropeS;
		Rope					ropeE;
		public Vector3			endPos = new Vector3(0.0f, 0.0f, 1.0f);
		public int				startIndex;
		public int				endIndex;
		public int				ropeMeshIndex;
		public bool				showStartConnect;
		public bool				showEndConnect;

		[Range(4, 128)]
		public static int		iterations		= 32;
		[Range(4, 128)]
		public static int		minIterations	= 32; 

		float					dist;
		Vector3					dir;
		Vector3					forceDir;
		Vector3					rbsOffset;
		Vector3					rbeOffset;
		public bool				isPrefab;

		public void SetDirty(bool _dirty = true)
		{
			dirty = _dirty;
		}

		public float GetArcLength()
		{
			return arcl;
		}

		public float GetDist()
		{
			return dist;
		}

		public Vector3 GetForceDir()
		{
			return forceDir;
		}

		public void SetLength(float _len)
		{
			length = _len;
			SetDirty();
		}

		public void SetEndObject(RopeEnd end, bool start = true)
		{
			Vector3 lscl = Vector3.one;

			if ( start )
			{
				if ( startObj )
				{
					lscl = startObj.transform.localScale;
					DestroyImmediate(startObj.gameObject);
					startObj = null;
				}

				if ( end && end.prefab )
				{
					GameObject newstart = Instantiate(end.prefab);
					newstart.transform.SetParent(transform);
					startConnect = end;
					startObj = newstart.transform;
					startObj.transform.localScale = lscl;
				}
				else
				{
					startConnect = null;
				}
			}
			else
			{
				if ( endObj )
				{
					lscl = endObj.transform.localScale;
					DestroyImmediate(endObj.gameObject);
					endObj = null;
				}

				if ( end && end.prefab )
				{
					GameObject newstart = Instantiate(end.prefab);
					newstart.transform.SetParent(transform);
					endConnect = end;
					endObj = newstart.transform;
					endObj.transform.localScale = lscl;
				}
				else
					endConnect = null;
			}

			SetDirty();
		}

		private void Awake()
		{
			isPrefab = true;
		}

		private void Start()
		{
			swingOffset = UnityEngine.Random.Range(0, 4.0f);

			pblock = new MaterialPropertyBlock();
			mr = GetComponent<MeshRenderer>();

			UpdateBounds();

			if ( endAttach )
			{
				rbe = endAttach.GetComponentInParent<Rigidbody>();

				ropeE = endAttach.GetComponent<Rope>();

				if ( rbe )
					rbeOffset = rbe.transform.InverseTransformPoint(endAttach.position);
			}

			if ( !startAttach )
				startAttach = transform;
			else
			{
				if ( startAttach != transform )
					ropeS = startAttach.GetComponent<Rope>();
			}

			if ( startAttach )
			{
				rbs = startAttach.GetComponentInParent<Rigidbody>();

				if ( rbs )
					rbsOffset = rbs.transform.InverseTransformPoint(startAttach.position);
			}

			SetDirty();
		}

		// Sets the length to be the same as the distance between the current ends
		public void SetLength()
		{
			length = Vector3.Distance(transform.position, endPos);
			stretch = length;
		}

		public void SetStartAttach(Transform obj)
		{
			startAttach = obj;
			if ( startAttach )
			{
				ropeS = startAttach.GetComponent<Rope>();
			}
			else
				ropeS = null;
		}

		public void SetEndAttach(Transform obj)
		{
			endAttach = obj;
			if ( endAttach )
			{
				ropeE = endAttach.GetComponent<Rope>();
			}
			else
				ropeE = null;
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

					b.size = new Vector3(40.0f, 80.0f, 40.0f);	//Vector3.zero;
					b.center = new Vector3(0.0f, 0.0f, 0.0f);
					mesh.bounds = b;
				}
			}
		}

		private void Update()
		{
			if ( isPrefab )
				endPos = transform.position + Vector3.forward * 2.0f;

			if ( visible )
			{
#if UNITY_EDITOR
				if ( !Application.isPlaying )
					dirty = true;
#endif				
				UpdateCurve();

				if ( attached != null )
				{
					for ( int i = 0; i < attached.Length; i++ )
					{
						if ( attached[i].obj )
						{
							Vector3 up;
							Vector3 p = CalcPos(attached[i].alpha, out up);
							Vector3 p1 = CalcPos(attached[i].alpha + 0.01f);
							attached[i].obj.position = transform.TransformPoint(CalcPos(attached[i].alpha));
							Quaternion look = Quaternion.LookRotation(transform.TransformPoint(p) - transform.TransformPoint(p1), up);
							attached[i].obj.rotation = look * Quaternion.Euler(attached[i].rot);
						}
					}
				}
			}
		}

		public void RopeUpdate()
		{
			if ( visible )
			{
#if UNITY_EDITOR
				if ( !Application.isPlaying )
					dirty = true;
#endif
				UpdateCurve();

				if ( attached != null )
				{
					for ( int i = 0; i < attached.Length; i++ )
					{
						if ( attached[i].obj )
						{
							Vector3 up;
							Vector3 p = CalcPos(attached[i].alpha, out up);
							Vector3 p1 = CalcPos(attached[i].alpha + 0.01f);
							attached[i].obj.position = transform.TransformPoint(CalcPos(attached[i].alpha));
							Quaternion look = Quaternion.LookRotation(transform.TransformPoint(p) - transform.TransformPoint(p1), up);
							attached[i].obj.rotation = look * Quaternion.Euler(attached[i].rot);
						}
					}
				}
			}
		}

		public void UpdateCurve()
		{
			if ( pblock == null )
				pblock = new MaterialPropertyBlock();

			Vector3 start	= Vector3.zero;
			Vector3 target;
			Vector3 worldEndPos;

			if ( ropeS )
				transform.position = ropeS.transform.TransformPoint(ropeS.CalcPos(ropeStart));
			else
			{
				if ( startAttach )
					transform.position = startAttach.position;
			}

			if ( endAttach && ropeE )
			{
				worldEndPos = endPos = ropeE.transform.TransformPoint(ropeE.CalcPos(ropeEnd));
				target = transform.InverseTransformPoint(worldEndPos);	//ropeE.transform.TransformPoint(ropeE.CalcPos(ropeEnd)));
			}
			else
			{
				if ( endAttach )
				{
					worldEndPos = endPos = endAttach.position;
					target = transform.InverseTransformPoint(worldEndPos);  //endAttach.position);
				}
				else
				{ 
					//target = endPos;    //new Vector3(0.0f, 0.0f, length);
					worldEndPos = endPos;
					target = transform.InverseTransformPoint(endPos);
				}
			}

			forceDir = (transform.position - worldEndPos).normalized;
			bool flip = target.y < start.y;

			p0 = flip ? target : start;
			p1 = flip ? start : target;

			Vector3 shift = (p1 - p0);// * transform.localScale;
			//Vector3 shift = Vector3.Scale((p1 - p0), transform.localScale);
			dist = shift.magnitude;	// * transform.localScale.z;
			dir = shift.normalized;

			if ( stretch > length )
				stretch = length;

			if ( dist != lastLength || dirty )
			{
				dirty = false;
				lastLength = dist;	//shift.magnitude;

				arcl = Mathf.Max(dist * 1.0001f, length);

				if ( dist < length )
				{
					float dl = ((length - dist * 1.0001f) / length);
					arcl -= dl * stretch;
				}

				float h = Mathf.Sqrt(shift.x * shift.x + shift.z * shift.z);

				float v = shift.y;
				float c = Mathf.Sqrt(arcl * arcl - v * v);
    
				if ( h == 0.0f )
					return;

				float a_min = 0.0f;
				float a_max = 1.0f;

				int i = 0;

				while ( i < iterations && c < 2.0f * a_max * Math.Sinh(h / (2.0f * a_max)) )
				{
					i += 1;
					a_min = a_max;
					a_max *= 2.0f;
				}

				i += minIterations;	//32;//16;	//minIterations;

				a = 0.0f;

				while ( i > 0 )
				{
					i -= 1;
					a = (a_min + a_max) * 0.5f;
					if ( c < 2.0f * a * Math.Sinh(h / (2.0f * a)) )
						a_min = a;
					else
						a_max = a;
				}

				p = (h - a * Mathf.Log((arcl + v) / (arcl - v))) / 2.0f;
				q = (v - arcl * (1.0f / (float)Math.Tanh(h / (2.0f * a)))) / 2.0f;

				flipx = flip ? 1.0f : 0.0f;

				float min = 0.0f;
				float max = 0.0f;

				adjust.x = adjustZ;
				adjust.y = adjustZEnd;
				adjust.w = adjustY;
				adjust.z = adjustYEnd;

				width.x = 1.0f + widthStart;
				width.y = 1.0f + widthEnd;
				width.z = length;

				if ( startConnect && startObj )
				{
					min = startConnect.ropeStart;
					adjust.x += startConnect.adjustZ;
					adjust.z += startConnect.adjustY;
					width.x = startConnect.width + widthStart;
				}

				if ( endConnect && endObj )
				{
					max = endConnect.ropeStart;
					adjust.y += endConnect.adjustZ;
					adjust.w += endConnect.adjustY;
					width.y = endConnect.width + widthEnd;
				}

				min += ropeStartAttach;
				max += ropeEndAttach;

				if ( dist <= length )
					pblock.SetVector("_MinMax", new Vector4(-min / length, 1.0f + (max / length), 0.0f, 0.0f));
				else
					pblock.SetVector("_MinMax",	new Vector4(-min / arcl, 1.0f + (max / arcl), 0.0f, 0.0f));
				pblock.SetVector("_P0",			p0);
				pblock.SetVector("_P1",			p1);
				pblock.SetVector("_Apq",		new Vector3(a, p, q));
				pblock.SetFloat("_ArcLength",	arcl);
				pblock.SetFloat("_FlipX",		flipx);
				pblock.SetVector("_Width",		width);
				pblock.SetVector("_Adjust",		adjust);
#if UNITY_EDITOR
				if ( Application.isPlaying )
					pblock.SetFloat("_SwingAngle",	swingAngle);
				else
					pblock.SetFloat("_SwingAngle", 0.0f);
#else
				pblock.SetFloat("_SwingAngle",	swingAngle);
#endif
				pblock.SetFloat("_SwingFreq",	swingFreq);
				pblock.SetFloat("_SwingOffset",	swingOffset);

				mr.SetPropertyBlock(pblock);
			}
			else
			{
#if UNITY_EDITOR
				if ( !Application.isPlaying )
				{
					float min = 0.0f;
					float max = 0.0f;

					if ( startConnect && startObj )
						min = startConnect.ropeStart;

					if ( endConnect && endObj )
						max = endConnect.ropeStart;

					if ( dist <= length )
						pblock.SetVector("_MinMax", new Vector4(-min / length, 1.0f + (max / length), 0.0f, 0.0f));
					else
						pblock.SetVector("_MinMax", new Vector4(-min / arcl, 1.0f + (max / arcl), 0.0f, 0.0f));
					pblock.SetVector("_P0", p0);
					pblock.SetVector("_P1", p1);
					pblock.SetVector("_Apq", new Vector3(a, p, q));
					pblock.SetFloat("_ArcLength", arcl);
					pblock.SetFloat("_FlipX", flipx);
					pblock.SetVector("_Width", width);
					pblock.SetVector("_Adjust", adjust);
					pblock.SetFloat("_SwingAngle", 0.0f);
					pblock.SetFloat("_SwingFreq", swingFreq);
					pblock.SetFloat("_SwingOffset", swingOffset);
					mr.SetPropertyBlock(pblock); 
				}
#endif
			}

			if ( startConnect )
			{
				Vector3 up;
				Vector3 p = CalcPosFromDist(startConnect.tangent, true, out up);
				if ( startObj )
				{
					startObj.transform.position = transform.position;
					Quaternion rot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
					Quaternion look = Quaternion.LookRotation(transform.position - transform.TransformPoint(p), up) * rot;
					startObj.transform.rotation = look;
				}
			}

			if ( endConnect )
			{
				Vector3 up;
				Vector3 p = CalcPosFromDist(endConnect.tangent, false, out up);
				if ( endObj )
				{
					endObj.transform.position = transform.TransformPoint(target);
					Quaternion rot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
					Quaternion look = Quaternion.LookRotation(transform.TransformPoint(target) - transform.TransformPoint(p), up) * rot;
					endObj.transform.rotation = look;
				}
			}
		}

		private void FixedUpdate()
		{
			float force = Mathf.Max((dist - length) * ropeForce, 0.0f);
			if ( force > 0.0f )
			{
				if ( rbs )
					rbs.AddForceAtPosition(force * -forceDir, rbs.transform.TransformPoint(rbsOffset));

				if ( rbe )
					rbe.AddForceAtPosition(force * forceDir, rbe.transform.TransformPoint(rbeOffset));
			}
		}

		public void RopeFixedUpdate()
		{
			float force = Mathf.Max((dist - length) * ropeForce, 0.0f);
			if ( force > 0.0f )
			{
				if ( rbs )
					rbs.AddForceAtPosition(force * -forceDir, rbs.transform.TransformPoint(rbsOffset));

				if ( rbe )
					rbe.AddForceAtPosition(force * forceDir, rbe.transform.TransformPoint(rbeOffset));
			}
		}

		float asinh(float x)
		{
			return Mathf.Log(x + Mathf.Sqrt(x * x + 1));
		}

		public Vector3 CalcPos(float alpha)
		{
			Vector3 up;
			return CalcPos(alpha, out up);
		}

		public Vector3 CalcPos(float alpha, out Vector3 up)
		{
			Vector3 shift = p1 - p0;
			Vector3 side = new Vector3(-shift.z, 0.0f, shift.x).normalized;
			float l = Mathf.Sqrt(shift.x * shift.x + shift.z * shift.z);

			//Vector3 wv = new Vector3(alpha, 0.0f, 0.0f);
			float wv = alpha;

			float lx = Mathf.Lerp(wv, 1.0f - wv, flipx);
			float wx = (a * asinh(lx * arcl / a - (float)Math.Sinh(p / a)) + p);
			float t = wx / l;

			float tf = t + 0.01f;
			float y0 = a * (float)Math.Cosh((t * l - p) / a) + q;
			float y1 = a * (float)Math.Cosh((tf * l - p) / a) + q;

			float sag = y0 - shift.y * t;
			float time = Time.timeSinceLevelLoad;
			float wave = Mathf.Sin(Mathf.PI * swingFreq * time + swingOffset);
			if ( flipx > 0.0f )
				wave = -wave;

#if UNITY_EDITOR
			float sa = swingAngle;
			if ( !Application.isPlaying )
				sa = 0.0f;
#else 
			float sa = swingAngle;
#endif
			float swing_xz = Mathf.Sin(wave * sa * 0.5f) * sag;
			float swing_y = Mathf.Cos(wave * sa * 0.5f) * sag;
			Vector3 swing = new Vector3(swing_xz * side.x, swing_y, swing_xz * side.z);

			Vector3 c0 = new Vector3(shift.x * t, y0, shift.z * t);
			Vector3 c1 = new Vector3(shift.x * tf, y1, shift.z * tf);

			Vector3 forward = (c1 - c0).normalized;
			up = Vector3.Cross(side, forward);

			//float x = side.x * wv.z * 1.0f * (1.0f - flipx * 2.0f);
			//float z = side.z * wv.z * 1.0f * (1.0f - flipx * 2.0f);

			//return c0 + p0 + swing + new Vector3(x, 0, z) + up * wv.y * 1.0f;
			return c0 + p0 + swing;// + up * wv;
		}

		public Vector3 CalcPosNoUp(float alpha)
		{
			Vector3 shift = p1 - p0;
			Vector3 side = new Vector3(-shift.z, 0.0f, shift.x).normalized;
			float l = Mathf.Sqrt(shift.x * shift.x + shift.z * shift.z);

			float wv = alpha;

			float lx = Mathf.Lerp(wv, 1.0f - wv, flipx);
			float wx = (a * asinh(lx * arcl / a - (float)Math.Sinh(p / a)) + p);
			float t = wx / l;

			float tf = t + 0.01f;
			float y0 = a * (float)Math.Cosh((t * l - p) / a) + q;
			float y1 = a * (float)Math.Cosh((tf * l - p) / a) + q;

			float sag = y0 - shift.y * t;
			float time = Time.timeSinceLevelLoad;
			float wave = Mathf.Sin(Mathf.PI * swingFreq * time + swingOffset);
			if ( flipx > 0.0f )
				wave = -wave;

#if UNITY_EDITOR
			float sa = swingAngle;
			if ( !Application.isPlaying )
				sa = 0.0f;
#else
			float sa = swingAngle;
#endif
			float swing_xz = Mathf.Sin(wave * sa * 0.5f) * sag;
			float swing_y = Mathf.Cos(wave * sa * 0.5f) * sag;
			Vector3 swing = new Vector3(swing_xz * side.x, swing_y, swing_xz * side.z);

			Vector3 c0 = new Vector3(shift.x * t, y0, shift.z * t);
			//Vector3 c1 = new Vector3(shift.x * tf, y1, shift.z * tf);

			//Vector3 forward = (c1 - c0).normalized;
			//up = Vector3.Cross(side, forward);

			//float x = side.x * wv.z * 1.0f * (1.0f - flipx * 2.0f);
			//float z = side.z * wv.z * 1.0f * (1.0f - flipx * 2.0f);

			//return c0 + p0 + swing + new Vector3(x, 0, z) + up * wv.y * 1.0f;
			return c0 + p0 + swing;// + up * wv;
		}

		public Vector3 CalcPosFromDist(float distance, bool start, out Vector3 up)	//, out float ratio)
		{
			if ( start )
			{
				float a = distance / arcl;
				Vector3 p1 = CalcPosNoUp(a);//, out up);

				float d = p1.magnitude;
				//ratio = d / distance;
				a /= d / distance;
				return CalcPos(a, out up);
			}
			else
			{
				float a = distance / arcl;
				float a1 = a;
				Vector3 p1 = CalcPosNoUp(1.0f - a);	//, out up);

				float d = (transform.InverseTransformPoint(endPos) - p1).magnitude;
				//ratio = d / distance;
				a /= d / distance;
				return CalcPos(1.0f - a, out up);
			}
		}

		public Vector3 CalcPosSimple(float alpha, out Vector3 up)
		{
			Vector3 shift = p1 - p0;
			Vector3 side = new Vector3(-shift.z, 0.0f, shift.x).normalized;
			float l = Mathf.Sqrt(shift.x * shift.x + shift.z * shift.z);

			Vector3 wv = new Vector3(alpha, 0.0f, 0.0f);

			float lx = Mathf.Lerp(wv.x, 1.0f - wv.x, flipx);
			float wx = (a * asinh(lx * arcl / a - (float)Math.Sinh(p / a)) + p);
			float t = wx / l;

			float tf = t + 0.01f;
			float y0 = a * (float)Math.Cosh((t * l - p) / a) + q;
			float y1 = a * (float)Math.Cosh((tf * l - p) / a) + q;

			float sag = y0 - shift.y * t;
			float time = Time.timeSinceLevelLoad;
			float wave = Mathf.Sin(Mathf.PI * swingFreq * time + swingOffset);
			if ( flipx > 0.0f )
				wave = -wave;

#if UNITY_EDITOR
			float sa = swingAngle;
			if ( !Application.isPlaying )
				sa = 0.0f;
#else
			float sa = swingAngle;
#endif
			float swing_xz = Mathf.Sin(wave * sa * 0.5f) * sag;
			float swing_y = Mathf.Cos(wave * sa * 0.5f) * sag;
			Vector3 swing = new Vector3(swing_xz * side.x, swing_y, swing_xz * side.z);

			Vector3 c0 = new Vector3(shift.x * t, y0, shift.z * t);
			Vector3 c1 = new Vector3(shift.x * tf, y1, shift.z * tf);

			Vector3 forward = (c1 - c0).normalized;
			up = Vector3.Cross(side, forward);

			float x = side.x * wv.z * 1.0f * (1.0f - flipx * 2.0f);
			float z = side.z * wv.z * 1.0f * (1.0f - flipx * 2.0f);

			return c0 + p0 + swing + new Vector3(x, 0, z) + up * wv.y * 1.0f;
		}

		bool visible = true;

		private void OnBecameVisible()
		{
			visible = true;
		}

		private void OnBecameInvisible()
		{
			visible = false;
		}
	}
}