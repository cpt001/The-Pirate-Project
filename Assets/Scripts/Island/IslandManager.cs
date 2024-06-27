using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// This class serves two purposes.
/// -It holds data on what's produced, and what's needed by this island
/// -It generates A* Graph specific to this island
/// 
/// This class should only appear on islands that are capable of being walked on
/// </summary>

public class IslandManager : MonoBehaviour
{
    //This class monitors island towns, and destroys duplicates caused by map magic
    public List<MMTownSetup> townsOnIsland = new List<MMTownSetup>();

    public List<IslandManager> otherIslands = new List<IslandManager>();

    private Dictionary<IslandManager, CargoSO> otherIslandAndCargo = new Dictionary<IslandManager, CargoSO>();
    public List<CargoSO> cargoProducedByIsland = new List<CargoSO>();



    private void Start()
    {
        StartCoroutine(GenerateGraphForIsland());
    }

    private IEnumerator GenerateGraphForIsland()
    {
        AstarData data = AstarPath.active.data;
        GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;

        int width = 1024;
        int depth = 1024;
        float nodeSize = 0.75f;

        gg.center = new Vector3(transform.position.x + 500, 0, transform.position.z + 500);
        gg.SetDimensions(width, depth, nodeSize);

        gg.Scan();
        yield return null;
    }

    private void OnEnable()
    {
        foreach (IslandManager iM in FindObjectsOfType<IslandManager>())
        {
            if (iM != this)
            {
                otherIslands.Add(iM);
            }
        }

        if (otherIslands.Count != 0)
        {
            foreach (IslandManager island in otherIslands)
            {
                if (!island.otherIslands.Contains(this))
                {
                    island.otherIslands.Add(this);
                }
                if (island.isActiveAndEnabled)
                {
                    //int townIndexToAssign = 0;
                    foreach (MMTownSetup town in island.townsOnIsland)
                    {
                        //Check for their town's individual pathfinding grids
                        /*if (town.graph.graphIndex > townIndexToAssign)  //If the previous town is higher
                        {
                            townIndexToAssign = town.graph.graphIndex + 1;  //Add one to index for assignment
                        }*/
                        //Check their town's production
                    }
                }
                foreach (CargoSO cargoItem in island.cargoProducedByIsland)
                {
                    otherIslandAndCargo.Add(island, cargoItem);
                }
            }
        }


        foreach (MMTownSetup town in townsOnIsland)
        {
            //Set the pathfinding grid for the town
            //Get the town's production.
        }
    }

    private void OnDisable()
    {
        if (otherIslands.Count != 0)
        {
            /*foreach (MMTownSetup town in townsOnIsland)
            {
                town.graph.graph = null;
            }*/
        }
    }

    void UpdateProductionWithConstruction()
    {

    }
}
