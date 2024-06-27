using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// This creates a new graph for a gangplank. Should allow connection between static and non-static grids.
/// </summary>

public class GangplankGrapher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateGangplankGraph());
    }

    private IEnumerator GenerateGangplankGraph()
    {
        AstarData data = AstarPath.active.data;
        RecastGraph rg = data.AddGraph(typeof(RecastGraph)) as RecastGraph;

        rg.cellSize = 0.1f;
        rg.useTiles = false;
        rg.minRegionSize = 500f;
        rg.walkableHeight = 1.5f;
        rg.maxSlope = 30f;
        rg.characterRadius = 0.3f;
        rg.rasterizeTerrain = false;

        rg.forcedBoundsCenter.x = transform.position.x;
        rg.forcedBoundsCenter.y = transform.position.y;
        rg.forcedBoundsCenter.z = transform.position.z;

        rg.name = "Gangplank_" + transform.parent.name;
        rg.forcedBoundsSize.x = 10f;
        rg.forcedBoundsSize.y = 26f;
        rg.forcedBoundsSize.z = 29f;


        rg.Scan();
        yield return null;
    }
}
