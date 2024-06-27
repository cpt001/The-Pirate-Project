using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using Den.Tools.Splines;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;

#if MM_SPLINEPACKAGE
using Unity.Mathematics;
using UnityEngine.Splines;
#endif

namespace MapMagic.Nodes.SplinesGenerators
{
	[System.Serializable]
	[GeneratorMenu(
		menu = "Spline", 
		name = "Unity Splines Out", 
		section=2, 
		colorType = typeof(SplineSys), 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Height")]
	public sealed class  UnitySplinesOutput214 : OutputGenerator, IInlet<SplineSys>  //virtually standard generator (never writes to products)
	{
		public OutputLevel outputLevel = OutputLevel.Main;
		public override OutputLevel OutputLevel { get{ return outputLevel; } }

		public static Action OnSplineCreated;

		public override void Generate (TileData data, StopToken stop)
		{
			//loading source
			if (stop!=null && stop.stop) return;
			SplineSys src = data.ReadInletProduct(this);
			if (src == null) return; 

			//adding to finalize
			if (stop!=null && stop.stop) return;
			if (enabled)
			{
				data.StoreOutput(this, typeof(UnitySplinesOutput214), this, src); 
				data.MarkFinalize(Finalize, stop);
			}
			else 
				data.RemoveFinalize(finalizeAction);
		}


		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop)
		{
			#if MM_SPLINEPACKAGE

			//purging if no outputs
			int splinesCount = data.OutputsCount(typeof(UnitySplinesOutput214), inSubs:true);
			if (splinesCount == 0)
			{
				if (stop!=null && stop.stop) return;
				data.MarkApply(ApplyData.Empty);
				return;
			}

			//merging splines
			SplineSys mergedSpline = null;
			if (splinesCount > 1)
			{
				mergedSpline = new SplineSys();
				
				//foreach (SplineSys spline in outputs)
				//	mergedSpline.Add...
			}
			else 
			{
				foreach ((UnitySplinesOutput214 output, SplineSys product, MatrixWorld biomeMask) 
					in data.Outputs<UnitySplinesOutput214,SplineSys,MatrixWorld>(typeof(UnitySplinesOutput214), inSubs:true))
						{ mergedSpline = product; break; }
			}

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyData applyData = new ApplyData() {spline=mergedSpline};
			Graph.OnOutputFinalized?.Invoke(typeof(UnitySplinesOutput214), data, applyData, stop);
			data.MarkApply(applyData);

			#endif
		}
		

		public class ApplyData : IApplyData
		{
			public SplineSys spline;

			public static Action OnSplineCreated;

			public void Apply(Terrain terrain)
			{
				#if MM_SPLINEPACKAGE

				//finding holder
				SplineContainer splineContainer = terrain.GetComponent<SplineContainer>(); 
				if (splineContainer == null) splineContainer = terrain.transform.parent.GetComponentInChildren<SplineContainer>();

				//or creating it
				if (splineContainer == null)
				{
					//TODO: it doesn't find a component after script serialization. Unity Splines script resets itself

					//removing and recreating spline then 
					Transform prevTfm = terrain.transform.parent.Find("Spline");
					if (prevTfm != null)
						GameObject.DestroyImmediate(prevTfm.gameObject);

					GameObject go = CreateSplineGameObject();

					go.transform.parent = terrain.transform.parent;
					go.transform.localPosition = new Vector3();
					go.name = "Spline";

					OnSplineCreated?.Invoke();

					splineContainer = go.GetComponent<SplineContainer>(); 
				}

				splineContainer.Splines = ConvertSplineSysToSpline(spline);

				#endif
			}


            #region Copy of the creation code from Unity Splines Editor (brought here to be called in runtime)

				#if MM_SPLINEPACKAGE

				static void CreateNewSpline ()
				{
					var gameObject = CreateSplineGameObject();

					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;

					OnSplineCreated?.Invoke();
				}

				static Spline[] CreateTempSplineData ()
				{
					Spline[] splines = new Spline[3];
					for (int s=0; s<splines.Length; s++)
					{
						BezierKnot[] knots = new BezierKnot[3];
						for (int k=0; k<knots.Length; k++)
							knots[k] = new BezierKnot(new float3(0,0,0), new float3(-1,0,0), new float3(1,0,0));

						splines[s] = new Spline(knots);
					}

					return splines;
				}

				static Spline[] ConvertSplineSysToSpline (SplineSys sys)
				{
					Spline[] splines = new Spline[sys.lines.Length];
					for (int s=0; s<splines.Length; s++)
					{
						Line sysLine = sys.lines[s];
						BezierKnot[] knots;
						
						//for single-segment lines to avoid reading past array
						if (sysLine.segments.Length == 1)
						{
							knots = new BezierKnot[2];
							knots[0] = new BezierKnot(sysLine.segments[0].start.pos, -sysLine.segments[0].start.dir, sysLine.segments[0].start.dir);
							knots[1] = new BezierKnot(sysLine.segments[0].end.pos, sysLine.segments[0].end.dir, -sysLine.segments[0].end.dir);
						}

						//common case 
						else
						{
							knots = new BezierKnot[sysLine.segments.Length + 1];

							//start
							knots[0] = new BezierKnot(sysLine.segments[0].start.pos, -sysLine.segments[0].start.dir, sysLine.segments[0].start.dir);

							//middle
							for (int k=1; k<knots.Length-1; k++)
								knots[k] = new BezierKnot(sysLine.segments[k].start.pos, sysLine.segments[k-1].end.dir, sysLine.segments[k].start.dir);

							//end
							Segment lastSegment = sysLine.segments[sysLine.segments.Length-1];
							knots[knots.Length-1] = new BezierKnot(lastSegment.end.pos, lastSegment.end.dir, -lastSegment.end.dir);
						}

						splines[s] = new Spline(knots);
					}

					return splines;
				}

				internal static GameObject CreateSplineGameObject ()
				{
					#if UNITY_EDITOR
					var name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(null, "Spline");
					var gameObject = UnityEditor.ObjectFactory.CreateGameObject(name, typeof(SplineContainer));
					#else
					var gameObject = new GameObject();
					gameObject.AddComponent<SplineContainer>();
					gameObject.name = "Spline";
					#endif

					var container = gameObject.GetComponent<SplineContainer>();
					container.Splines = CreateTempSplineData();
            
					//UnityEditor.Selection.activeGameObject = gameObject;
					return gameObject;
				}

				#endif

            #endregion


            public static ApplyData Empty 
				{get{ return new ApplyData() { spline = null }; }}

			public int Resolution {get{ return 0; }}
		}


		public static void Purge(CoordRect rect, Terrain terrain)
		{

		}

		public override void ClearApplied (TileData data, Terrain terrain)
		{
			/*TerrainData terrainData = terrain.terrainData;
			Vector3 terrainSize = terrainData.size;

			terrainData.detailPrototypes = new DetailPrototype[0];
			terrainData.SetDetailResolution(32, 32);*/

			throw new System.NotImplementedException();
		}
	}


}