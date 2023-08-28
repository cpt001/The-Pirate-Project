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
/// 
/// 
/// </summary>
public class IslandController : MonoBehaviour
{
    //public List<Structure> structuresOnIsland = new List<Structure>();
    public Dictionary<Structure, bool> structureCheck = new Dictionary<Structure, bool>();
    public Dictionary<Structure, CargoSO.CargoType> cargoRequests = new Dictionary<Structure, CargoSO.CargoType>();
    public Dictionary<Structure, CargoSO.CargoType> currentCargoOnIsland = new Dictionary<Structure, CargoSO.CargoType>();

    private bool isSpecialistIsland;
    private enum IslandSpecialization
    {
        Not_Specialist,

    }
    private int islandTier;
    private int population;
    public List<CargoSO> cargoRequestQueue;
    private List<CargoSO> cargoRequestsSent;
    //public Dictionary<Structure, CargoSO> structureTracker = new Dictionary<Structure, CargoSO>();
    public List<GameObject> unassignedWorkers = new List<GameObject>();
    public List<PawnGeneration> pawnsOnIsland = new List<PawnGeneration>();

    
}
