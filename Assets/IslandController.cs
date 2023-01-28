using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
/// <summary>
/// Idea:
/// So, this is implementation 1 of the island controller.
/// Each structure requires a certain set of items to produce items for its own inventory. 
/// The structure makes a request to the island controller, seeking the item as needed.
/// If the item is unavailable, and not in production/being made, then it sends a request to any nearby aligned islands.
/// If the item is unavailable at nearby aligned islands, it sends a request to a neighboring nation that is friendly.
/// If the call is answered negatively, the controller will look through the game manifest, to see if it's available on any island.
/// If not, the resource is requested from off-map *the host countries continent*, at an exorbetant price
/// </summary>
public class IslandController : MonoBehaviour
{
    //public List<Structure> structuresOnIsland = new List<Structure>();
    public Dictionary<Structure, bool> structureCheck = new Dictionary<Structure, bool>();

    private bool isSpecialistIsland;
    private enum IslandSpecialization
    {
        Not_Specialist,

    }
    private int islandTier;
    private int population;
    public List<CargoSO> cargoRequestQueue;
    private List<CargoSO> cargoRequestsSent;
    private int requestCount;
    //public Dictionary<Structure, CargoSO> structureTracker = new Dictionary<Structure, CargoSO>();
    public List<PawnGeneration> unassignedWorkers = new List<PawnGeneration>();
    public List<PawnGeneration> pawnsOnIsland = new List<PawnGeneration>();
    private UnityAction pawnJobAssignment;

    private void Awake()
    {
        pawnJobAssignment = new UnityAction(AssignJobToPawn);
        EventsManager.StartListening("PawnAddedToIsland", pawnJobAssignment);
        /*foreach(KeyValuePair<Structure, bool> kvp in structureCheck)
        {
            Debug.Log("Structure listing: " + kvp.Key);
        }*/
    }

    private void AssignJobToPawn()
    {

    }

    /*private void FixedUpdate()
    {
        if (cargoRequestQueue.Count != 0)
        {
            if (cargoRequestQueue.Count != cargoRequestsSent.Count)
            {
                RequestCount();
            }
        }
    }

    public void RequestCount()
    {
        foreach (CargoSO cargoRequested in cargoRequestQueue)
        {
            foreach (KeyValuePair<Structure, CargoSO> structureInventory in structureTracker)
            {
                if (structureInventory.Value != null)
                {
                    if (structureInventory.Value == cargoRequested && structureInventory.Value.markedForTransitByPawn == null)
                    {
                        structureInventory.Value.markedForTransitByPawn = structureInventory.Key.buildingWorkers.ElementAt(Random.Range(0, structureInventory.Key.buildingWorkers.Count));
                    }
                }
            }
        }
    }*/
}
