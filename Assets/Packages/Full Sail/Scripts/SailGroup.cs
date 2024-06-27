using System.Collections.Generic;
using UnityEngine;

namespace FullSail
{
	[System.Serializable]
	public class SailParamRange
	{
		public SailParamID	id;
		public bool			active;
		public float		fmin;
		public float		fmax;
		public int			seed;
	}

	// this script shows you can control a group of sails with a set of overide values.
	// Each sail is still free to have its own overrides
	[ExecuteAlways]
	[HelpURL("http://www.macspeedee.com/full-sail/")]
	public class SailGroup : MonoBehaviour
	{
		public List<Sail>			sailObjs		= new List<Sail>();			// Sails controlled by this group, if empty will be filled with all child sails
		public List<SailParam>		sailParams		= new List<SailParam>();	// Sail overrides for this group (each sail can override or have it own values)
		public bool					dirty;									// set to true after any changes to the sailparam values. Call SetDirty()

		// random seed for sails, so need random seed and multiplier, and enable
		public bool					randomRipple	= true;					// Set this to true if you want the group to apply a random offset to each sail for the animation
		public int					seed			= 0;					// Seed for random number generator
		public float				minAnimOffset	= 0.0f;					// min anim offset to apply
		public float				maxAnimOffset	= 10.0f;				// max anim offset to apply

		public List<SailParamRange>	randomValues	= new List<SailParamRange>();
		public bool					showRange;
		// noise to speed value option
		// set speed for group

		void Start()
		{
			SailShaderIDs.MakeIds();

			if ( sailObjs.Count == 0 )
			{
				Sail[] sails = GetComponentsInChildren<Sail>();
				sailObjs = new List<Sail>(sails);
				SetDirty(true);
			}

			if ( randomRipple )
			{
				Random.InitState(seed);
				for ( int i = 0; i < sailObjs.Count; i++ )
				{
					if ( sailObjs[i] )
						sailObjs[i].SetAnimOffset(Random.Range(minAnimOffset, maxAnimOffset));
				}
			}

			if ( Application.isPlaying )
			{
				for ( int i = 0; i < randomValues.Count; i++ )
				{
					if ( randomValues[i].id != SailParamID.None && randomValues[i].active )
					{
						for ( int j = 0; j < sailObjs.Count; j++ )
						{
							if ( sailObjs[j] )
							{
								SailParam sp = new SailParam();
								sp.id = randomValues[i].id;
								sp.active = true;
								sp.fval = Random.Range(randomValues[i].fmin, randomValues[i].fmax);
								sailObjs[j].sailParams.Add(sp);
							}
						}
					}
				}
			}
		}

		void OnEnable()
		{
			SetDirty(true);
		}

		// Call this to set an override value from script
		public void SetValue(SailParamID id, float val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.fval = val;
				dirty = true;
			}
		}

		public void SetValue(SailParamID id, Vector3 val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.vval = val;
				dirty = true;
			}
		}

		public void SetValue(SailParamID id, Texture2D val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.tval = val;
				dirty = true;
			}
		}

		public void SetValue(SailParamID id, int val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.ival = val;
				dirty = true;
			}
		}

		public void SetValue(SailParamID id, Color val)
		{
			SailParam sp = GetParam(id);
			if ( sp != null )
			{
				sp.cval = val;
				dirty = true;
			}
		}

		public void SetDirty(bool _dirty = true)
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

		void Update()
		{
			//Shader.SetGlobalFloat("_CustomTime", Time.time);//0.0f);//Time.timeSinceLevelLoad);
#if UNITY_EDITOR
			SailShaderIDs.MakeIds();

			if ( !Application.isPlaying )
				dirty = true;
#endif
			for ( int i = 0; i < sailObjs.Count; i++ )
			{
				if ( sailObjs[i] )
				{
					sailObjs[i].DoUpdate(sailParams, dirty);
				}
			}

			dirty = false;
		}
	}
}