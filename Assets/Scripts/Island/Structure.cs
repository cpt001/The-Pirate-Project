using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Structure : MonoBehaviour
{
    private IslandController islandController;

    [Header("Structure Setup")]
    private string buildingOwner;
    private string buildingName;
    [Tooltip("Structure does not have local residency, and is attached to a nearby structure for its workers")]
    [SerializeField] private bool hasLinkedResidence;
    [SerializeField] private Structure linkedStructure;
    public bool worksThroughRest;
    private int workerCount;
    public List<PawnGeneration> masterWorkerList = new List<PawnGeneration>();
    [SerializeField] private int buildingWorkerLimit;
    private bool isConstructionSite;    //Prevents requests from being made, unless they're construction related

    private bool canSellWaresToPlayer;
    private bool isSpecialistStructure;        //Determines whether the building is a specialist
    private bool storageBuilding;           //Determines whether the building idly contains extra resources from and for the island
    private bool producerBuilding = false;  //Determines whether the building can produce with no input
    private int productionRangeMax = 20;
    private int productionRangeMin = 10;
    private int storageLimit;
    [SerializeField] private CargoSO emptyCargoObject;

    [SerializeField] private List<CargoSO> itemRequested;    //Items being requested by the structure
    private List<CargoSO> itemBeingMade;    //Items being made by the store (should prevent the item's prereq from being requested
    [SerializeField] private List<CargoSO> currentStoreInventory;    //This is what's currently in the store
    private List<Item> currentStoreForPlayerInventory;    //This is what's currently in the store that the player can purchase

    //These are the master lists that control what should be in the store. 
    [Tooltip("Populate this with items the store should maintain for and from the island")]
    [SerializeField] private List<CargoSO> storeToIslandInventory;   //This controls what the store should have on hand at all times.
    [Header("Store saleable items")]
    [Tooltip("Populate this with items the store should sell to the player")] 
    [SerializeField] private List<Item> storeInventoryForPlayer;   //This controls what the store should have on hand at all times.

    private enum TownStructure
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

    }
    [SerializeField] private TownStructure thisStructure;

    private enum FortStructure
    {
        Not_Fort,
        Fort_Barracks,
        Fort_OfficerQuarters,
        Fort_Stocks,
        Fort_Gallows,
        Fort_Kitchen,
        Fort_MessHall,
        Fort_Armory,
        Fort_MunitionStore,
        Fort_Watchtower,
        Fort_Battlement,
        Fort_Vault,
    }
    [SerializeField] private FortStructure fortStructure;

    private enum SpecialistStructure
    {
        Not_Specialist,
        Plantation,
        Trading_Post,
        Mineshaft,
        Quarry,
        Smugglers_Hideout,
        Guardian_Fort,
        Cannibal_Island,
        Native_Island,
        Abandoned_Island,
        Shipwreck,
        Mission,
    }
    [SerializeField] private SpecialistStructure specialistStructure;

    private UnityAction dayUpdate;
    private UnityAction updateWorkerCount;

    private void Awake()
    {
        if (!islandController)
        {
            islandController = GetComponentInParent<IslandController>();
        }
        if (thisStructure != TownStructure.undefined_structure && islandController != null)
        {
            //islandController.structureTracker.Add(this, null);  //This is a simple tracker that defines all the possible buildings on the island
            BuildingSetup();    //Create initial inventory, and request list
            islandController.structureCheck.Add(this, false);
            //Populate workers
        }
        else if ((fortStructure != FortStructure.Not_Fort) && islandController != null)
        {
            Debug.Log("Fort structure");
            FortBuildingSetup();
        }
        else if ((specialistStructure != SpecialistStructure.Not_Specialist) && islandController != null)
        {
            //Debug.Log("Specialist structure");
            producerBuilding = true;
        }
        dayUpdate = new UnityAction(DayUpdate);
        updateWorkerCount = new UnityAction(WorkerCountUpdate);
    }

    private void WorkerCountUpdate()
    {
        if (buildingWorkerLimit > 0 && masterWorkerList.Count <= buildingWorkerLimit)
        {
            //Debug.Log(thisStructure + " worker limit is not zero; " + buildingWorkerLimit);
            for (int i = 0; i < buildingWorkerLimit; i++)
            {
                //Debug.Log("Called successfully");
                if (islandController.unassignedWorkers.Count != 0 && islandController.unassignedWorkers[i] != null)
                {
                    masterWorkerList.Add(islandController.unassignedWorkers[i]);
                    islandController.unassignedWorkers[i].workPlace = this;
                    islandController.unassignedWorkers.RemoveAt(i);
                }
            }
            workerCount = masterWorkerList.Count; //shift1Workers.Count + shift2Workers.Count + shift3Workers.Count;
            if (workerCount >= buildingWorkerLimit)
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

    private void Update()
    {
        if (masterWorkerList.Count < 0)
        {
            Debug.Log(thisStructure + " has a non-zero pop");
        }
    }

    private void OnEnable()
    {
        EventsManager.StartListening("NewDay", dayUpdate);
        EventsManager.StartListening("PawnAddedToIsland", updateWorkerCount);
    }
    private void OnDisable()
    {
        EventsManager.StopListening("NewDay", dayUpdate);
    }

    private void DayUpdate()
    {
        //Debug.Log("Update requested by structure: " + name);
        if (islandController != null)
        {
            if (!storageBuilding && !producerBuilding)  //Normal building, converts materials into other materials, or items.
            {
                //CommunicateRequestsToIslandController();
                UpdateInventoryStatus();    //Request
                CraftNewMaterial();         //Creation/Conversion

            }
            if (producerBuilding)   //Creates raw materials
            {
                CreateRawMaterial();

            }
            if (storageBuilding)    //Stores all types of materials
            {

            }
        }
    }

/*    void CommunicateRequestsToIslandController()
    {
        if (itemRequested.Count != 0)
        {
            foreach (CargoSO cargoRequest in itemRequested) //Check through each item in the request queue
            {
                if (!islandController.structureTracker.ContainsValue(cargoRequest)) //If its not in the structure tracker already
                {
                    islandController.structureTracker.Add(this, cargoRequest);  //Add to request list
                }
            }
        }
    }*/

    void UpdateInventoryStatus()
    {
        if (currentStoreInventory.Count != storeInventoryForPlayer.Count)    //If the list count isnt identical
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
        }
    }

    void CraftNewMaterial()
    {
        if (storeInventoryForPlayer.Count != 0)
        {
            //Cargo SO consumed to become Item for player.
            if (currentStoreForPlayerInventory.Count != storeInventoryForPlayer.Count)
            {
                foreach (Item sellableItem in storeInventoryForPlayer)  //Each possible item to be sold
                {
                    if (!currentStoreForPlayerInventory.Contains(sellableItem)) //If it does not contain a matching item
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
    }

    void CreateRawMaterial()
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
    }


    void BuildingSetup()
    {
        if (!isSpecialistStructure)
        {
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
                        //Needs:    Wood, sugar
                        //Creates:  Honey, wax
                        //Bonus:    Sugar
                        //Initial inventory

                        //Requested inventory
                        break;
                    }
                case TownStructure.Apothecary:
                    {
                        //Needs:    [Recipe Dependent]Roots, animal parts, water, coal, flour, sugar, gold, medical supplies
                        //Creates:  Roots, animal parts
                        break;
                    }
                case TownStructure.Armorer:
                    {

                        break;
                    }
                case TownStructure.Armory:
                    {

                        break;
                    }
                case TownStructure.Bakery:
                    {

                        break;
                    }
                case TownStructure.Bank:
                    {

                        break;
                    }
                case TownStructure.Barber:
                    {

                        break;
                    }
                case TownStructure.Barn:
                    {

                        break;
                    }
                case TownStructure.Bawdy_House:
                    {

                        break;
                    }
                case TownStructure.Blacksmith:
                    {

                        break;
                    }
                case TownStructure.Broker:
                    {

                        break;
                    }
                case TownStructure.Butcher:
                    {

                        break;
                    }
                case TownStructure.Candle_Maker:
                    {

                        break;
                    }
                case TownStructure.Carpenter:
                    {

                        break;
                    }
                case TownStructure.Church:
                    {

                        break;
                    }
                case TownStructure.Clocktower:
                    {

                        break;
                    }
                case TownStructure.Cobbler:
                    {

                        break;
                    }
                case TownStructure.Courthouse:
                    {

                        break;
                    }
                case TownStructure.Distillery:
                    {

                        break;
                    }
                case TownStructure.Dock:
                    {

                        break;
                    }
                case TownStructure.Drydock:
                    {

                        break;
                    }
                case TownStructure.Fishing_Hut:
                    {

                        break;
                    }
                case TownStructure.Forge:
                    {

                        break;
                    }
                case TownStructure.Garrison:
                    {

                        break;
                    }
                case TownStructure.Governors_Mansion:
                    {

                        break;
                    }
                case TownStructure.Graveyard:
                    {

                        break;
                    }
                case TownStructure.Gypsy_Wagon:
                    {

                        break;
                    }
                case TownStructure.House:
                    {

                        break;
                    }
                case TownStructure.Hunter_Shack:    //Can produce independently
                    {
                        //Creates animal parts, raw meat, skins, animal fat
                        producerBuilding = true;    
                        break;
                    }
                case TownStructure.Jeweler_Parlor:
                    {

                        break;
                    }
                case TownStructure.Leathersmith:
                    {

                        break;
                    }
                case TownStructure.Logging_Camp:
                    {
                        //Creates logs and charcoal
                        producerBuilding = true;
                        break;
                    }
                case TownStructure.Library:
                    {

                        break;
                    }
                case TownStructure.Lighthouse:
                    {

                        break;
                    }
                case TownStructure.Market_Stall:
                    {

                        break;
                    }
                case TownStructure.Mill:
                    {

                        break;
                    }
                case TownStructure.Pawn_Shop:
                    {

                        break;
                    }
                case TownStructure.Prison:
                    {

                        break;
                    }
                case TownStructure.Public_Square:
                    {

                        break;
                    }
                case TownStructure.Saw_Mill:    //Can produce independently
                    {
                        producerBuilding = true;    //Creates wood
                        break;
                    }
                case TownStructure.Shack:
                    {

                        break;
                    }
                case TownStructure.Shipwright:
                    {

                        break;
                    }
                case TownStructure.Tailor:
                    {

                        break;
                    }
                case TownStructure.Tar_Kiln:
                    {

                        break;
                    }
                case TownStructure.Tavern:
                    {

                        break;
                    }
                case TownStructure.Tattoo_Parlor:
                    {

                        break;
                    }
                case TownStructure.Town_Hall:
                    {

                        break;
                    }
                case TownStructure.Warehouse:
                    {

                        break;
                    }
                case TownStructure.Watchtower:
                    {

                        break;
                    }
                case TownStructure.Water_Well:  //Can produce independently
                    {

                        break;
                    }
                case TownStructure.Wig_Maker:
                    {

                        break;
                    }
            }
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
}
