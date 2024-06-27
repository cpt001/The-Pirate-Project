using System.Collections.Generic;
using UnityEngine;

namespace FullSail
{
	// Example script for furling the sails, makes use of smoothdamping to give a nice look and any furlorder values found in the sails
	[HelpURL("http://www.macspeedee.com/full-sail/")]
	[ExecuteAlways]
	public class SailsFurl : MonoBehaviour
	{
		public class FurlVal
		{
			public float		furl;			// wanted furl value for the sail, smoothdamp will move towards this value
			public float		cfurl;			// current furl value for the sail, set from smoothdamp
			public float		vel;			// vel ref for smoothdamp
			public int			order;			// furl order for this sail
			public SailParam	sp;				// param controlling the furling for this sail
		}

		[Range(0, 1)]
		public float		furl;				// How furled the group is 0 = all sails unfurled, 1 all sails fully furled
		public float		speed		= 1.0f;	// How fast the sails furls
		public SailGroup	group;				// The group of sails to be furled by this script
		int					furlCount	= 0;
		List<FurlVal>		furlVals	= new List<FurlVal>();

		void Start()
		{
			if ( !group )
				group = GetComponent<SailGroup>();

			if ( group )
			{
				furlVals.Clear();

				SailParam sp = group.GetParam(SailParamID.Furl);
				if ( sp != null )
				{
					SailParam sp1 = group.GetParam(SailParamID.FurlOrder);
					if ( sp1 != null )
					{
						furlCount	= sp1.ival;
						FurlVal fv	= new FurlVal();
						fv.order	= sp1.ival;
						fv.sp		= sp;
						furlVals.Add(fv);
					}
				}

				for ( int i = 0; i < group.sailObjs.Count; i++ )
				{
					if ( group.sailObjs[i] )
					{
						sp = group.sailObjs[i].GetParam(SailParamID.Furl);
						if ( sp != null )
						{
							SailParam sp1 = group.sailObjs[i].GetParam(SailParamID.FurlOrder);
							if ( sp1 != null )
							{
								if ( sp1.ival > furlCount )
									furlCount = sp1.ival;

								FurlVal fv	= new FurlVal();
								fv.order	= sp1.ival;
								fv.sp		= sp;
								furlVals.Add(fv);
							}
						}
					}
				}
			}
		}

		void Update()
		{
			if ( group )
			{
				float ff = furl * (furlCount + 01);

				for ( int i = 0; i < furlVals.Count; i++ )
				{
					if ( (float)furlVals[i].order < ff )
						furlVals[i].furl = 2.0f;
					else
						furlVals[i].furl = 0.0f;

					if ( Application.isPlaying )
						furlVals[i].sp.fval = Mathf.SmoothDamp(furlVals[i].sp.fval, furlVals[i].furl, ref furlVals[i].vel, speed);
					else
						furlVals[i].sp.fval = furlVals[i].furl;
				}

				group.SetDirty(true);
			}
		}
	}
}