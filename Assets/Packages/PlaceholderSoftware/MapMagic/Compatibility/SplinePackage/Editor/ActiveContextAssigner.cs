#if MM_SPLINEPACKAGE

//using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.Splines;

namespace MapMagic.Nodes.SplinesGenerators
{

    public static class ActiveContextAssigner
    {
        static ActiveContextAssigner ()
        {
            UnitySplinesOutput214.OnSplineCreated -= AssignContext;
            UnitySplinesOutput214.OnSplineCreated += AssignContext;

        }

        static void AssignContext ()
        {
    		    UnityEditor.ActiveEditorTracker.sharedTracker.RebuildIfNecessary();
			    //Ensuring trackers are rebuilt before changing to SplineContext
			    UnityEditor.EditorApplication.delayCall += SetKnotPlacementTool;
        }

        static void SetKnotPlacementTool()
        {
                ToolManager.SetActiveContext<SplineToolContext>();
                //ToolManager.SetActiveTool<KnotPlacementTool>();
        }
    } 

}

#endif
