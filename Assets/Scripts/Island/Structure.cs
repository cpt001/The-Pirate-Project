using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
/// <summary>
/// Changelog:
/// --> Obsolete, using structure tool instead
/// 
/// 
/// -Removed producer building system, as it's defunct with the new work location system
/// Current plan:
/// -> fix structure crafting to match a selection of items for the player.
/// -> create list of required items
/// -> request cargo deliver to building
/// 
/// Separating structure and residential structures into separate scripts for clarity of purpose
/// </summary>
public class Structure : MonoBehaviour
{
    private IslandController islandController;

    [Header("Structure Setup")]
    private string buildingOwner;
    private string buildingName;
    private bool isConstructionSite;    //Prevents requests from being made, unless they're construction related

    [Header("Job Details")]
    public int workStartTime;
    public int workEndTime;
    public bool worksThroughRest;
    public int dailyPay;

    [Header("Workers and Residents")]
    public int maxResidents;                                                    //Defines max occupancy of a house or shack
    public List<GameObject> masterWorkerList = new List<GameObject>();  //Controls all pawns assigned to structure
    [SerializeField] private int buildingWorkerLimit;
    public List<GameObject> workersOnDuty;

    [Header("Structure Inventory")]
    private bool isStore;        //Determines whether the building is a store for the player
    private int storageLimit;
    private bool canOperateWithNoInput;
    [SerializeField] private CargoSO emptyCargoObject;
    [SerializeField] private List<CargoSO> itemRequested;    //Items being requested by the structure
    [SerializeField] private List<CargoSO> currentStoreInventory;    //This is what's currently in the store

    private Dictionary<CargoSO.CargoType, int> cargoRequired = new Dictionary<CargoSO.CargoType, int>();    //Whats needed to make items here
    public Dictionary<CargoSO.CargoType, int> cargoAvailable = new Dictionary<CargoSO.CargoType, int>();   //This should be considered 'spares' - anything not requested by the island is kept here
    public Dictionary<CargoSO.CargoType, int> cargoProduced = new Dictionary<CargoSO.CargoType, int>();     //What's made here


    [Header("Inventory for Player")]
    private List<Item> currentStoreInventoryForPlayer;    //This is what's currently in the store that the player can purchase
    [SerializeField] private List<Item> storeInventoryForPlayer;   //This is what the store should have for player purchase




    //These are the master lists that control what should be in the store. 
    [Tooltip("Populate this with items the store should maintain for and from the island")]
    [SerializeField] private List<CargoSO> storeToIslandInventory;   //This controls what the store should have on hand at all times.
    [Header("Store saleable items")]
    [Tooltip("Populate this with items the store should sell to the player")] 
    public List<WorkLocation> workSites = new List<WorkLocation>();
    public List<CustomerInteraction> customerInteractionSites = new List<CustomerInteraction>();
    public Transform assignmentLocation;
    public Transform salePoint;

    #region Structure Enums
    public enum TownStructure
    {
        undefined_structure,

        Apiary,     //Bee house         ||Req: Wood     ||Creates: Honey
        Apothecary, //Legal potions     ||Req: Roots, animal bones, water       ||Creates: Health potions, antivenoms
        Armorer,    //Offers plate armor pieces.            ||Req: Steel ingots, iron ore, coal, wood, coal     ||Creates: Bracers, chestplates
        Armory,     //Stores weaponry for island defense.   ||Req: Gunpowder, weapons

        Bakery,     //Creates food      ||Req: Flour, Sugar, Wood, water, tools     ||Creates: Food
        Bank,       //Stores monies     ||Req: Gold, Jewelry
        Barber,     //Offers haircuts, and surgery      ||Req: Tools, Medical Supplies    
        Barn,       //Stores food items long term       ||Req: Storage, Food
        Bawdy_House,    //A place for pleasure          ||Req: Food.
        Blacksmith, //Creates weapons   ||Req: Steel ingots, iron ore, coal, wood, charcoal     ||Creates: Swords, daggers, firearms, tools
        //Bookstore,  //Stores books      ||Req: Books  //Excessive shops, and not really useful
        Broker,     //Warehouse liason
        Butcher,    //Creates food      ||Req: Raw meat, fish, tools    ||Creates: Food

        Candle_Maker,   //Creates candles   ||Req: Wax, String      ||Creates: Candles
        Carpenter,      //Creates furniture ||Req: Wood, tools, 
        Church,         //Satisfies religious fervor    ||Req: Jewelry
        Clay_Pit,       //Creates bricks.
        Clocktower,     //Ornamental
        Cobbler,        //Creates shoes and leatherware     ||Req: Leather, string, tools   ||Creates: Coats, vests, shoes, 
        //Cotton_Gin,     //Removed. Too early in the timeline.
        Courthouse,     //Ornamental

        Distillery,     //Creates beer      ||Req: Wheat, hops, grapes, sugar   ||Creates: Rum, beer, wine
        Dock,           //Ornamental
        Drydock,        //Creates ships, and repairs them.  ||Req: Wood, tar, rope

        Fishing_Hut,    //Fishing   ||Req: Wood, string     ||Creates: Fish
        Forge,          //Creates ingots    ||Req: Iron ore, Gold Ore, Coal     ||Creates: Tools, iron ingot, gold ingot, 

        Garrison,       //Houses a small group of soldiers to defend an island. ||Req: Food.
        Governors_Mansion,  //Ornamental
        Graveyard,      //Ornamental
        Gypsy_Wagon,    //Allows creation of unique and dangerous potions; as well as voodoo artistry.

        House,          //Ornamental.
        Hunter_Shack,   //Provides skins and meat to the local populace.    ||Creates: Meat, skins, food

        Jeweler_Parlor, //Creates jewelry.  ||Req: Gold, iron, coal     ||Creates: Rings, earrings, necklaces

        Leathersmith,   //Tans raw skins into usable leather.   ||Req: Skins        ||Creates: Leather
        Logging_Camp,   //Produces wood and charcoal
        Library,        //Ornamental
        Lighthouse,     //Ornamental

        Market_Stall,   //Offers food and occasional trinkets.  ||Req: Food
        Mill,           //Converts wheat, hops, and sugar into usable elements.     ||Req: Wheat, hops, sugar cane

        Pawn_Shop,      //Allows the player to sell of miscellaneous items easily.  ||Req: Jewelry, tools, swords, daggers, firearms, rings, earrings, necklaces, books 
        //Printer,        //Removed. Too early in the timeline.
        Prison,         //Self explanatory.
        Public_Square,  //Moreso a location marker than anything else.

        Room,           //Serves as a rented room within a larger structure

        Saw_Mill,       //Refines wood.        ||Creates: planks
        Shack,          //Simple single house.
        Shipwright,     //Sells available ships to the player, and allows light refit, and medium repairs.
        //Stable,         //A standalone stable would be attached to a structure, or on a large estate

        Tailor,         //Creates string and clothing from wool.
        Tar_Kiln,       //
        Tavern,         //
        Tattoo_Parlor,  //Allows the player and patrons to get tattoos  ||Req: Tools
        Town_Hall,      //

        Warehouse,      //
        Watchtower,     //
        Water_Well,     //
        Wig_Maker,      //

        Plantation,
        Mineshaft,
        Quarry,
    }
    public TownStructure thisStructure;

    private enum FortStructure
    {
        Not_Fort,
        Fort_Armory,
        Fort_Barracks,
        Fort_Battlement,
        Fort_Gallows,
        Fort_Kitchen,
        Fort_MessHall,
        Fort_MunitionStore,
        Fort_OfficerQuarters,
        Fort_Stocks,
        Fort_Vault,
        Fort_Watchtower,
    }
    [SerializeField] private FortStructure fortStructure;

    private enum SpecialistStructure
    {
        Not_Specialist,
        Trading_Post,
        Smugglers_Hideout,
        Guardian_Fort,
        Cannibal_Island,
        Native_Island,
        Abandoned_Island,
        Shipwreck,
        Mission,
    }
    [SerializeField] private SpecialistStructure specialistStructure;
    #endregion

    private void Awake()
    {
        if (!islandController)
        {
            islandController = GetComponentInParent<IslandController>();
        }

        SetListeners();
        SetupStructureClass();
        CheckForWorkSites();

    }
    void SetListeners()
    {
        EventsManager.StartListening("NewDay", DayUpdate);
        EventsManager.StartListening("PawnAddedToIsland", WorkerCountUpdate);
        EventsManager.StartListening("SummonClerk_" + this.name, SendClerkToCounter);
    }
    void SetupStructureClass ()
    {
        if (islandController)
        {
            if (thisStructure != TownStructure.undefined_structure)
            {
                BuildingSetup();    //Create initial inventory, and request list
                islandController.structureCheck.Add(this, false);
                //Populate workers
            }
            else if (fortStructure != FortStructure.Not_Fort)
            {
                FortBuildingSetup();
            }
        }
    }
    private void CheckForWorkSites()
    {
        foreach (Transform t in transform)
        {
            if (t.GetComponent<WorkLocation>())
            {
                workSites.Add(t.GetComponent<WorkLocation>());
                if (!t.GetComponent<WorkLocation>().isSalePoint)
                {
                    buildingWorkerLimit++;
                }
            }
            if (t.GetComponent<CustomerInteraction>())
            {
                customerInteractionSites.Add(t.GetComponent<CustomerInteraction>());
            }
            if (t.name == "AssignmentLocation")
            {
                assignmentLocation = t;
            }
        }
    }

    //This is called whenever a new pawn is detected on the island. 
    //It checks against the structure's worker count, then adds workers until sated.
    private void WorkerCountUpdate()
    {
        if (islandController)
        {
            if (buildingWorkerLimit > 0 && masterWorkerList.Count <= buildingWorkerLimit)
            {
                //Debug.Log(thisStructure + " worker limit is not zero; " + buildingWorkerLimit);
                for (int i = 0; i < buildingWorkerLimit; i++)
                {
                    //This communicates with the island controller, and sends a message to the new worker to find a nearby home.
                    if (islandController.unassignedWorkers.Count != 0 && islandController.unassignedWorkers[i] != null)
                    {
                        masterWorkerList.Add(islandController.unassignedWorkers[i]);
                        islandController.unassignedWorkers[i].GetComponent<PawnNavigation>().workPlace = this;
                        EventsManager.TriggerEvent("FindHome_" + islandController.unassignedWorkers[i].name);
                        islandController.unassignedWorkers.RemoveAt(i);
                    }
                }
                if (masterWorkerList.Count == buildingWorkerLimit)
                {
                    {
                        islandController.structureCheck[this] = true;
                    }
                }
            }
            else
            {
                //Debug.Log(thisStructure + " worker limit is 0!");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " has no islandController");
        }

    }
    //Each day, a request for new materials is sent to the island controller.
    //These requests are for materials not on the island
    private void DayUpdate()
    {
        if (islandController != null && isStore)
        {
            UpdateInventoryStatus();    //Request
            //CraftNewMaterial();         //Creation/Conversion
        }
    }

    //This function manages all inventories in this structure.
    void UpdateInventoryStatus()
    {
        //
        foreach (KeyValuePair<CargoSO.CargoType, int> itemAndAmount in cargoAvailable)
        {
            for (int i = 0; i > itemAndAmount.Value; i++)
            {
                islandController.currentCargoOnIsland.Add(this, itemAndAmount.Key);
            }
        }

        //This function creates items for the player
        if (currentStoreInventoryForPlayer.Count <= storeInventoryForPlayer.Count)
        {
            //Compare each cargo value
            foreach (KeyValuePair<CargoSO.CargoType, int> cargoNeeded in cargoRequired)
            {
                foreach (KeyValuePair<CargoSO.CargoType, int> cargoOffered in cargoAvailable)
                {
                    //If the cargo key is the same
                    if (cargoNeeded.Key == cargoOffered.Key)
                    {
                        //If the number of cargo is equal or greater than what's needed
                        if (cargoOffered.Value <= cargoNeeded.Value)
                        {
                            //Update work location or work assignment with a new job
                            EventsManager.TriggerEvent("PlayerObjectReadyToCraft_" + name);
                        }
                        if (cargoOffered.Value > cargoNeeded.Value)
                        {
                            //Check the island's inventory for existing cargo
                            if (islandController.currentCargoOnIsland.ContainsValue(cargoNeeded.Key))
                            {
                                //Set cargo's destination to this structure
                                //Remove cargo from current cargo on island dictionary in IC (prevents double claims)
                                //Check structure inventory for object
                                //
                            }

                            islandController.cargoRequests.Add(this, cargoNeeded.Key);
                        }
                    }
                }
            }
        }







        /*if (currentStoreInventory.Count != storeInventoryForPlayer.Count)    //If the list count isnt identical
        {
            if ((currentStoreInventory.Count + itemBeingMade.Count) != storeInventoryForPlayer.Count)    //If the list and the make count isnt identical
            {
                if ((currentStoreInventory.Count + itemBeingMade.Count + itemRequested.Count) != storeInventoryForPlayer.Count)  //If the total counts do not add up to the total count between all of them
                {
                    foreach (CargoSO storeItem in storeToIslandInventory)   //Check through the manifest
                    {
                        if (!currentStoreInventory.Contains(storeItem)) //If the current inventory doesnt have the target item
                        {
                            if (!itemRequested.Contains(storeItem)) //If the target isnt already in the request queue
                            {
                                itemRequested.Add(storeItem);   //Add it to the request list.
                                islandController.cargoRequestQueue.Add(storeItem);  //Add to the island's request listm
                            }
                        }
                    }
                }
            }
        }*/
    }

    void SendClerkToCounter()
    {
        masterWorkerList = masterWorkerList.OrderBy((d) => (d.transform.position - salePoint.position).sqrMagnitude).ToList();
        masterWorkerList[0].GetComponent<PawnGeneration>().pawnNavigator.agent.SetDestination(salePoint.position);
        EventsManager.TriggerEvent("NewDestination_" + masterWorkerList[0].name);
    }

    /*void CraftNewMaterial()
    {
        if (storeInventoryForPlayer.Count != 0)
        {
            //Cargo SO consumed to become Item for player.
            if (currentStoreInventoryForPlayer.Count != storeInventoryForPlayer.Count)
            {
                foreach (Item sellableItem in storeInventoryForPlayer)  //Each possible item to be sold
                {
                    if (!currentStoreInventoryForPlayer.Contains(sellableItem)) //If it does not contain a matching item
                    {
                        foreach (CargoSO cargoItem in sellableItem.craftingMaterials)   //Get all of the cargo items required to craft said item
                        {
                            itemRequested.Add(cargoItem);   //Add them to the request list
                        }
                    }
                }
            }
        }
        else
        {
            //Cargo SO consumed to create Cargo SO
        }
    }*/

    //This function is defunct, in favor of work location system. Kept for archival purposes.
    /*void CreateRawMaterial()
    {
        //Randomize amount per day.
        //Add to list
        //Debug.Log("Create raw called");
        int resourcesProduced = Mathf.RoundToInt(Random.Range(productionRangeMin, productionRangeMax));
        switch (specialistStructure)
        {
            case SpecialistStructure.Mineshaft:
                {
                    for (int i = 0; i < resourcesProduced; i++)
                    {
                        //Debug.Log("Ore added");
                        var oreObject = emptyCargoObject;
                        oreObject.cargoType = CargoSO.CargoType.Ore;
                        currentStoreInventory.Add(oreObject);

                    }
                    break;
                }
        }
    }*/

    //This sets building cargo in and out, and working times
    void BuildingSetup()
    {
        //Refactor this into the individual structures for specific requirements
        if (isConstructionSite)
        {
            cargoRequired.Add(CargoSO.CargoType.Tools, 1);
            cargoRequired.Add(CargoSO.CargoType.Bricks, 1);
            cargoRequired.Add(CargoSO.CargoType.Rope, 1);
            cargoRequired.Add(CargoSO.CargoType.Planks, 1);
            cargoRequired.Add(CargoSO.CargoType.Tar, 1);
        }

        switch (thisStructure)
        {
            case TownStructure.undefined_structure:
                {
                    if ((fortStructure == FortStructure.Not_Fort && specialistStructure == SpecialistStructure.Not_Specialist) 
                        || (fortStructure == FortStructure.Not_Fort || specialistStructure == SpecialistStructure.Not_Specialist))
                    {
                        Debug.Log(gameObject + " is undefined! Making inactive!");
                        gameObject.SetActive(false);
                        break;
                    }
                    else
                    {
                        Debug.Log("Structure should be defined as a fort or specialist structure");
                        break;
                    }
                }
            case TownStructure.Apiary:
                {
                    //Work start and end times
                    workStartTime = 12;
                    workEndTime = 18;

                    //Cargo required for bonus production or satisfaction
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Sugar, 1);

                    //Cargo available when building first generates -- Moved to function below
                    //cargoAvailable.Add(CargoSO.CargoType.Wood, Mathf.RoundToInt(Random.Range(0, 3)));
                    //cargoAvailable.Add(CargoSO.CargoType.Sugar, Mathf.RoundToInt(Random.Range(0, 3)));

                    //Cargo produced, and how much should be produced per round
                    cargoProduced.Add(CargoSO.CargoType.Honey, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Wax, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Apothecary:
                {
                    workStartTime = 9;
                    workEndTime = 17;

                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Animal_Parts, 1);

                    cargoProduced.Add(CargoSO.CargoType.Honey, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Sugar, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Roots, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Animal_Parts, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Water, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Coal, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Flour, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Medical_Supplies, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Armorer:
                {
                    workStartTime = 7;
                    workEndTime = 16;

                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    cargoRequired.Add(CargoSO.CargoType.Leather, 1);
                    break;
                }
            case TownStructure.Armory:
                {
                    workStartTime = 8;
                    workEndTime = 14;

                    cargoRequired.Add(CargoSO.CargoType.Gunpowder, 1);
                    cargoRequired.Add(CargoSO.CargoType.Weapons, 1);
                    break;
                }
            case TownStructure.Bakery:
                {
                    workStartTime = 4;
                    workEndTime = 16;

                    cargoRequired.Add(CargoSO.CargoType.Honey, 1);
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Sugar, 1);
                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Flour, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Bank:
                {
                    workStartTime = 10;
                    workEndTime = 6;

                    cargoRequired.Add(CargoSO.CargoType.Gold, 1);

                    cargoProduced.Add(CargoSO.CargoType.Gold, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Barber:
                {
                    workStartTime = 8;
                    workEndTime = 8;

                    cargoRequired.Add(CargoSO.CargoType.Medical_Supplies, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    break;
                }
            case TownStructure.Barn:
                {
                    workStartTime = 5;
                    workEndTime = 13;

                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Flour, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Wool, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Bawdy_House:
                {
                    workStartTime = 14;
                    workEndTime = 22;

                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Food, 1);
                    break;
                }
            case TownStructure.Blacksmith:
                {
                    workStartTime = 8;
                    workEndTime = 14;

                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    cargoRequired.Add(CargoSO.CargoType.Leather, 1);
                    cargoRequired.Add(CargoSO.CargoType.Steel_Ingot, 1);
                    cargoRequired.Add(CargoSO.CargoType.Ore, 1);

                    cargoProduced.Add(CargoSO.CargoType.Tools, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Steel_Ingot, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Broker:
                {
                    workStartTime = 7;
                    workEndTime = 16;
                    //This structure sells warehouse space to the player
                    break;
                }
            case TownStructure.Butcher:
                {
                    workStartTime = 6;
                    workEndTime = 15;

                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);

                    cargoProduced.Add(CargoSO.CargoType.Animal_Parts, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Candle_Maker:
                {
                    workStartTime = 9;
                    workEndTime = 5;
                    cargoRequired.Add(CargoSO.CargoType.Wax, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    break;
                }
            case TownStructure.Carpenter:
                {
                    workStartTime = 9;
                    workEndTime = 5;
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    break;
                }
            case TownStructure.Church:
                {
                    workStartTime = 5;
                    workEndTime = 9;
                    cargoRequired.Add(CargoSO.CargoType.Gold, 1);
                    cargoRequired.Add(CargoSO.CargoType.Books, 1);
                    cargoRequired.Add(CargoSO.CargoType.Alcohol, 1);
                    cargoRequired.Add(CargoSO.CargoType.Jewelry, 1);
                    break;
                }
            case TownStructure.Clay_Pit:
                {
                    workStartTime = 6;
                    workEndTime = 14;
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Bricks, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Clocktower:
                {
                    //Provides a mood boost in bigger cities
                    break;
                }
            case TownStructure.Cobbler:
                {
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);
                    cargoRequired.Add(CargoSO.CargoType.Leather, 1);
                    cargoRequired.Add(CargoSO.CargoType.Skins, 1);

                    cargoProduced.Add(CargoSO.CargoType.Clothing, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Courthouse:
                {
                    //Provides a mood boost in bigger cities
                    break;
                }
            case TownStructure.Distillery:
                {
                    cargoRequired.Add(CargoSO.CargoType.Sugar, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Wheat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Grapes, 1);

                    cargoProduced.Add(CargoSO.CargoType.Alcohol, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Dock:
                {
                    cargoProduced.Add(CargoSO.CargoType.Fish, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Drydock:
                {
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Rope, 1);
                    cargoRequired.Add(CargoSO.CargoType.Planks, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tar, 1);

                    break;
                }
            case TownStructure.Fishing_Hut:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);

                    cargoProduced.Add(CargoSO.CargoType.Fish, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Forge:
                {
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Gold, 1);
                    cargoRequired.Add(CargoSO.CargoType.Steel_Ingot, 1);
                    cargoRequired.Add(CargoSO.CargoType.Ore, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);

                    cargoProduced.Add(CargoSO.CargoType.Gold, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Tools, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Steel_Ingot, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Garrison:
                {
                    cargoRequired.Add(CargoSO.CargoType.Gunpowder, 1);
                    cargoRequired.Add(CargoSO.CargoType.Weapons, 1);

                    break;
                }
            case TownStructure.Governors_Mansion:
                {
                    //Provides a player service, and a mood boost
                    break;
                }
            case TownStructure.Graveyard:
                {
                    cargoRequired.Add(CargoSO.CargoType.Bricks, 1);

                    break;
                }
            case TownStructure.Gypsy_Wagon:
                {
                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Animal_Parts, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Gold, 1);
                    cargoRequired.Add(CargoSO.CargoType.Medical_Supplies, 1);
                    cargoRequired.Add(CargoSO.CargoType.Leather, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);
                    cargoRequired.Add(CargoSO.CargoType.Skins, 1);

                    cargoProduced.Add(CargoSO.CargoType.Roots, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Animal_Parts, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Gold, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Medical_Supplies, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.House:
                {
                    maxResidents = 4;

                    cargoRequired.Add(CargoSO.CargoType.Honey, 1);
                    cargoRequired.Add(CargoSO.CargoType.Sugar, 1);
                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Flour, 1);
                    cargoRequired.Add(CargoSO.CargoType.Wool, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);

                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Hunter_Shack:
                {
                    cargoRequired.Add(CargoSO.CargoType.Animal_Parts, 1);
                    cargoRequired.Add(CargoSO.CargoType.Medical_Supplies, 1);
                    cargoRequired.Add(CargoSO.CargoType.Gunpowder, 1);
                    cargoRequired.Add(CargoSO.CargoType.Weapons, 1);
                    cargoRequired.Add(CargoSO.CargoType.Rope, 1);

                    cargoProduced.Add(CargoSO.CargoType.Honey, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Animal_Parts, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Leather, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Raw_Meat, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Skins, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Jeweler_Parlor:
                {
                    cargoRequired.Add(CargoSO.CargoType.Steel_Ingot, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);
                    cargoRequired.Add(CargoSO.CargoType.Gold, 1);

                    cargoProduced.Add(CargoSO.CargoType.Jewelry, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Leathersmith:
                {
                    cargoRequired.Add(CargoSO.CargoType.Skins, 1);

                    cargoProduced.Add(CargoSO.CargoType.Leather, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Library:
                {
                    cargoRequired.Add(CargoSO.CargoType.Books, 1);

                    break;
                }
            case TownStructure.Lighthouse:
                {
                    //Provides a mood boost
                    break;
                }
            case TownStructure.Logging_Camp:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);

                    break;
                }
            case TownStructure.Market_Stall:
                {
                    cargoRequired.Add(CargoSO.CargoType.Honey, 1);
                    cargoRequired.Add(CargoSO.CargoType.Sugar, 1);
                    cargoRequired.Add(CargoSO.CargoType.Food, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);
                    cargoRequired.Add(CargoSO.CargoType.Grapes, 1);

                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Mill:
                {
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Wheat, 1);

                    cargoProduced.Add(CargoSO.CargoType.Flour, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Mineshaft:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Ore, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Pawn_Shop:
                {
                    cargoProduced.Add(CargoSO.CargoType.Rope, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Jewelry, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Plantation:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Wheat, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Grapes, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Sugar, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Roots, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Prison:
                {
                    cargoRequired.Add(CargoSO.CargoType.Animal_Parts, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Rope, 1);

                    break;
                }
            case TownStructure.Public_Square:
                {
                    //Provides a mood boost
                    break;
                }
            case TownStructure.Quarry:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tools, 1);

                    cargoProduced.Add(CargoSO.CargoType.Ore, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Bricks, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Room:
                {
                    //Special structure
                    maxResidents = 1;
                    break;
                }
            case TownStructure.Saw_Mill:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wood, 1);

                    cargoProduced.Add(CargoSO.CargoType.Planks, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Shack:
                {
                    maxResidents = 3;
                    cargoRequired.Add(CargoSO.CargoType.Wax, 1);
                    cargoRequired.Add(CargoSO.CargoType.Roots, 1);
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Flour, 1);
                    cargoRequired.Add(CargoSO.CargoType.Wool, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);

                    cargoProduced.Add(CargoSO.CargoType.Wood, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Roots, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Coal, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Food, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Wool, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Raw_Meat, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Skins, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Rope, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Shipwright:
                {
                    cargoRequired.Add(CargoSO.CargoType.Gunpowder, 1);
                    cargoRequired.Add(CargoSO.CargoType.Weapons, 1);
                    cargoRequired.Add(CargoSO.CargoType.Rope, 1);
                    cargoRequired.Add(CargoSO.CargoType.Planks, 1);
                    cargoRequired.Add(CargoSO.CargoType.Tar, 1);

                    cargoProduced.Add(CargoSO.CargoType.Rope, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Tailor:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wool, 1);
                    cargoRequired.Add(CargoSO.CargoType.Clothing, 1);

                    cargoProduced.Add(CargoSO.CargoType.Leather, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Clothing, Mathf.RoundToInt(Random.Range(0, 3)));
                    cargoProduced.Add(CargoSO.CargoType.Rope, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Tar_Kiln:
                {
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Coal, 1);

                    cargoProduced.Add(CargoSO.CargoType.Tar, Mathf.RoundToInt(Random.Range(0, 3)));
                    break;
                }
            case TownStructure.Tattoo_Parlor:
                {
                    //Mood boost
                    break;
                }
            case TownStructure.Tavern:
                {
                    cargoRequired.Add(CargoSO.CargoType.Water, 1);
                    cargoRequired.Add(CargoSO.CargoType.Food, 1);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat, 1);
                    cargoRequired.Add(CargoSO.CargoType.Fish, 1);
                    cargoRequired.Add(CargoSO.CargoType.Alcohol, 1);

                    break;
                }

            case TownStructure.Town_Hall:
                {
                    //Mood boost
                    break;
                }
            case TownStructure.Warehouse:
                {
                    //Service
                    break;
                }
            case TownStructure.Watchtower:
                {
                    //Mood boost
                    break;
                }
            case TownStructure.Water_Well:  //Can produce independently
                {
                    cargoProduced.Add(CargoSO.CargoType.Water, Mathf.RoundToInt(Random.Range(0, 3)));

                    break;
                }
            case TownStructure.Wig_Maker:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wool, 1);
                    break;
                }
        }
        
        //Gets each type of cargo thats required, and generates a random amount to the available inventory when the structure finishes setup
        foreach (KeyValuePair<CargoSO.CargoType, int> cargoAndType in cargoRequired)
        {
            cargoAvailable.Add(cargoAndType.Key, Mathf.RoundToInt(Random.Range(0, 10)));
        }
    }

    void FortBuildingSetup()
    {
        switch (fortStructure)
        {
            case FortStructure.Not_Fort:
                {
                    break;
                }
        }
    }

    //Removed. While it should work, I think this is overcomplicating the way things work. Adding a work assignment (something that already works) is pretty minor
    /*private void OnTriggerEnter (Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            Debug.Log("Pawn spotted entering " + this.name);
            PawnNavigation tempPawn = other.GetComponent<PawnNavigation>();
            if (!assignmentLocation)
            {
                //If the pawn's working here
                if (tempPawn.workPlace = this)
                {
                    foreach(WorkLocation w in workSites)
                    {
                        //If the worksite's permanent worker field is null
                        if (w.permanentWorker == null)
                        {
                            //If there's no title for the worker, set it, rename pawn, and send pawn to worksite location
                            if (tempPawn.pawn.workTitle == null)
                            {
                                w.permanentWorker = tempPawn;
                                tempPawn.pawn.workTitle = this.name;
                                tempPawn.pawn.name = tempPawn.pawn.name + " {" + this.name + "}";
                                EventsManager.TriggerEvent("NewDestination_" + tempPawn.name);
                                tempPawn.agent.SetDestination(w.transform.position);
                            }
                            //Otherwise, just send the pawn to their workplace
                            else if (tempPawn.pawn.workTitle == this.name)
                            {
                                EventsManager.TriggerEvent("NewDestination_" + tempPawn.name);
                                tempPawn.agent.SetDestination(w.transform.position);
                            }
                        }
                        //it needs to ignore otherwise
                        else
                        {
                            
                        }
                    }
                }
                else if (tempPawn.homeStructure = this)
                {

                }
                else
                {
                    //Deliveries
                    //Shoppers
                    //Visitors
                    //
                }
            }
        }
    }*/
}
