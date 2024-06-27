using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureTool : MonoBehaviour
{
    /// <summary>
    /// Structures generate from the call set in MM town setup. 
    /// When a new structure is spawned, it pulls the required structure from the townsetup
    /// The structure pulls data on what it should look like, and how many pawns to spawn
    /// Structure function should probably be separated into another class, or an entirely different script for clarity
    /// </summary>
    /// 

    public MMTownSetup assignedTown;
    private bool modelSupportsMultiple;

    public bool buildingPositionSet;


    private GameObject modelToSpawn;
    private List<PawnBaseClass> Residents = new List<PawnBaseClass>();
    private int numberOfAIToSpawn;
    private int numberOfShacksToAdd = 0;
    [SerializeField] private GameObject AI;
    
    #region Models
[Header("Completed Models")]
    [SerializeField] private GameObject bankModel;               //Bank, Armory
    [SerializeField] private GameObject barnModel;               //Barn, Armorer
    [SerializeField] private GameObject bigWorkshopModel;        //Blacksmith, Carpenter
    [SerializeField] private GameObject fishingShackModel;       //FishingHut
    [SerializeField] private GameObject churchModel;             //Church

    [SerializeField] private GameObject genericBuilding1;        //Bakery, Barber, Butcher, Cobbler, JewelerParlor, Library, PawnShop, Tattoo Parlor, WigMaker
    [SerializeField] private GameObject innModel;                //Tavern, BawdyHouse
    [SerializeField] private GameObject mineOfficeModel;         //Mineshaft, Quarry
    [SerializeField] private GameObject shackModel;              //Shack, HunterShack, Logging Camp, Broker
    [SerializeField] private GameObject warehouseModel;          //Warehouse, Sawmill
[Header("No Model")]
    [SerializeField] private GameObject NYIsmallWorkshopModel;   //Apiary, Apothecary, CandleMaker, Leathersmith, Tailor, TattooParlor, 
    [SerializeField] private GameObject NYIgraveyardModel;
    [SerializeField] private GameObject NYIgypsyWagonModel;
    [SerializeField] private GameObject NYImarketStallModel;
    [SerializeField] private GameObject NYIclayPitModel;
    [SerializeField] private GameObject NYIprisonModel;
    [SerializeField] private GameObject NYIgarrisonModel;
    [SerializeField] private GameObject NYIwatchtowerModel;
    [SerializeField] private GameObject NYImillModel;
    [SerializeField] private GameObject NYIshipwrightModel;
    [SerializeField] private GameObject NYIdrydockModel;
    [SerializeField] private GameObject NYIdistilleryModel;
    [SerializeField] private GameObject NYIforgeModel;
    #endregion

    //Future Models
    //Clocktower, Courthouse, Plantation
    //Dock, Distillery, Forge, House, Tar Kiln
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
        //?Broker,     //Warehouse liason
        Butcher,    //Creates food      ||Req: Raw meat, fish, tools    ||Creates: Food

        Candle_Maker,   //Creates candles   ||Req: Wax, String      ||Creates: Candles
        Carpenter,      //Creates furniture ||Req: Wood, tools, 
        Church,         //Satisfies religious fervor    ||Req: Jewelry
        Clay_Pit,       //Creates bricks.
        Clocktower,     //Ornamental
        Cobbler,        //Creates shoes and leatherware     ||Req: Leather, string, tools   ||Creates: Coats, vests, shoes, 
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
        Prison,         //Self explanatory.
        Public_Square,  //Moreso a location marker than anything else.

        Room,           //Serves as a rented room within a larger structure

        Saw_Mill,       //Refines wood.        ||Creates: planks
        Shack,          //Simple single house.
        Shipwright,     //Sells available ships to the player, and allows light refit, and medium repairs.

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

    public enum T0Structure
    {
        undefined_Structure,
        FishingHut,
        Graveyard,
        GypsyWagon,
        HunterShack,
        LoggingCamp,
        MarketStall,
        Shack,
    }
    public T0Structure t0Structure;
    public enum T1Structure
    {
        undefined_Structure,
        Apiary,
        Bakery,
        Blacksmith,
        ClayPit,
        Prison,
        SawMill,
        Inn,
        BawdyHouse,
    }
    public T1Structure t1Structure;
    public enum T2Structure
    {
        undefined_Structure,
        Apothecary,
        Armory,
        Barn,
        Carpenter,
        Church,
        Cobbler,
        Garrison,
        Leathersmith,
        PawnShop,
        Tailor,
        TarKiln,
        Warehouse,
        Watchtower,
    }
    public T2Structure t2Structure;
    public enum T3Structure
    {
        undefined_Structure,
        Armorer,
        Barber,
        Broker,
        CandleMaker,
        JewelerParlor,
        Mill,
        Shipwright,
        TattooParlor,
        Butcher,
    }
    public T3Structure t3Structure;
    public enum T4Structure
    {
        undefined_Structure,
        Bank,
        Drydock,
        House,
        Library,
        WigMaker,
    }
    public T4Structure t4Structure;
    public enum CapitolBuildings
    {
        undefined_Structure,
        Distillery,
        Forge,
    }
    public CapitolBuildings t5Structure;

    private bool lateStartInitialized;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 parentDirection = new Vector3(transform.parent.position.x, transform.position.y, transform.parent.position.z);
        transform.LookAt(parentDirection);
    }

    // Update is only a late start call.
    //This function generates the structure name, defines its model, and communicates exclusivity to the town setup script
    void Update()
    {
        if (!lateStartInitialized)
        {
            assignedTown = transform.GetComponentInParent<MMTownSetup>();
            #region Structure Model Definition
            if (t0Structure != T0Structure.undefined_Structure)
            {
                gameObject.name = assignedTown.name + "_" + t0Structure.ToString();
                switch (t0Structure)
                {
                    case T0Structure.FishingHut:
                        {
                            modelToSpawn = fishingShackModel;
                            
                            break;
                        }
                    /*case T0Structure.Graveyard:
                        {
                            if (!assignedTown.hasGraveyard)
                            {
                                modelToSpawn = NYIgraveyardModel;
                                //assignedTown.hasGraveyard = true;
                                break;
                            }
                            else
                            {
                                //This regenerates the script, since its an exclusive building
                                int randBuilding = Random.Range(0, 8);
                                t0Structure = (T0Structure)randBuilding;
                                lateStartInitialized = false;
                                break;
                            }

                        }*/
                    case T0Structure.GypsyWagon:
                        {
                            modelToSpawn = NYIgypsyWagonModel;
                            break;
                        }
                    case T0Structure.HunterShack:
                        {
                            modelToSpawn = shackModel;
                            break;
                        }
                    case T0Structure.LoggingCamp:
                        {
                            modelToSpawn = shackModel;
                            break;
                        }
                    case T0Structure.MarketStall:
                        {
                            int rand = Random.Range(0, 2);
                            if (rand == 1)
                            {
                                modelToSpawn = NYImarketStallModel;
                                break;
                            }
                            else
                            {
                                modelToSpawn = NYIgypsyWagonModel;
                                break;
                            }
                        }
                    case T0Structure.Shack:
                        {
                            modelToSpawn = shackModel;
                            break;
                        }
                }
            }
            else if (t1Structure != T1Structure.undefined_Structure)
            {
                gameObject.name = assignedTown.name + "_" + t1Structure.ToString();
                switch (t1Structure)
                {
                    case T1Structure.Apiary:
                        {
                            modelToSpawn = NYIsmallWorkshopModel;
                            break;
                        }
                    case T1Structure.Bakery:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                    case T1Structure.Blacksmith:
                        {
                            modelToSpawn = bigWorkshopModel;
                            break;
                        }
                    case T1Structure.ClayPit:
                        {
                            modelToSpawn = NYIclayPitModel;
                            break;
                        }
                    case T1Structure.Prison:
                        {
                            modelToSpawn = NYIprisonModel;
                            break;
                        }
                    case T1Structure.SawMill:
                        {
                            modelToSpawn = warehouseModel;
                            break;
                        }
                    case T1Structure.Inn:
                        {
                            modelToSpawn = innModel;
                            break;
                        }
                    case T1Structure.BawdyHouse:
                        {
                            modelToSpawn = innModel;
                            break;
                        }
                }

            }
            else if (t2Structure != T2Structure.undefined_Structure)
            {
                gameObject.name = assignedTown.name + "_" + t2Structure.ToString();
                switch (t2Structure)
                {
                    case T2Structure.Apothecary:
                        {
                            modelToSpawn = NYIsmallWorkshopModel;
                            break;
                        }
                    case T2Structure.Armory:
                        {
                            modelToSpawn = bankModel;
                            break;
                        }
                    case T2Structure.Barn:
                        {
                            modelToSpawn = barnModel;
                            break;
                        }
                    case T2Structure.Carpenter:
                        {
                            modelToSpawn = bigWorkshopModel;
                            break;
                        }
                    case T2Structure.Church:
                        {
                            modelToSpawn = churchModel;
                            break;
                        }
                    case T2Structure.Garrison:
                        {
                            modelToSpawn = NYIgarrisonModel;
                            break;
                        }
                    case T2Structure.Leathersmith:
                        {
                            modelToSpawn = NYIsmallWorkshopModel;
                            break;
                        }
                    case T2Structure.PawnShop:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                    case T2Structure.Tailor:
                        {
                            modelToSpawn = NYIsmallWorkshopModel;
                            break;
                        }
                    case T2Structure.TarKiln:
                        {
                            modelToSpawn = NYIclayPitModel;
                            break;
                        }
                    case T2Structure.Warehouse:
                        {
                            modelToSpawn = warehouseModel;
                            break;
                        }
                    case T2Structure.Watchtower:
                        {
                            modelToSpawn = NYIwatchtowerModel;
                            break;
                        }
                }

            }
            else if (t3Structure != T3Structure.undefined_Structure)
            {
                gameObject.name = assignedTown.name + "_" + t3Structure.ToString();
                switch (t3Structure)
                {
                    case T3Structure.Armorer:
                        {
                            modelToSpawn = barnModel;
                            break;
                        }
                    case T3Structure.Barber:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                    case T3Structure.Broker:
                        {
                            modelToSpawn = shackModel;
                            break;
                        }
                    case T3Structure.CandleMaker:
                        {
                            modelToSpawn = bigWorkshopModel;
                            break;
                        }
                    case T3Structure.JewelerParlor:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                    case T3Structure.Mill:
                        {
                            modelToSpawn = NYImillModel;
                            break;
                        }
                    case T3Structure.Shipwright:
                        {
                            modelToSpawn = NYIshipwrightModel;
                            break;
                        }
                    case T3Structure.TattooParlor:
                        {
                            modelToSpawn = NYIsmallWorkshopModel;
                            break;
                        }
                    case T3Structure.Butcher:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                }
            }
            else if (t4Structure != T4Structure.undefined_Structure)
            {
                gameObject.name = assignedTown.name + "_" + t4Structure.ToString();
                switch (t4Structure)
                {
                    case T4Structure.Bank:
                        {
                            modelToSpawn = bankModel;
                            break;
                        }
                    case T4Structure.Drydock:
                        {
                            modelToSpawn = NYIdrydockModel;
                            break;
                        }
                    case T4Structure.House:
                        {
                            modelToSpawn = shackModel;
                            break;
                        }
                    case T4Structure.Library:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                    case T4Structure.WigMaker:
                        {
                            modelToSpawn = genericBuilding1;
                            break;
                        }
                }
            }
            else
            {
                gameObject.name = assignedTown.name + "_" + t5Structure.ToString();
                switch (t5Structure)
                {
                    case CapitolBuildings.Distillery:
                        {
                            modelToSpawn = NYIdistilleryModel;
                            break;
                        }
                    case CapitolBuildings.Forge:
                        {
                            modelToSpawn = NYIforgeModel;
                            break;
                        }
                }
            }
            #endregion

            assignedTown.structuresInTown.Add(this);
            //Define building model
            //Define interior decorations

            if (modelToSpawn != null)
            {
                gameObject.tag = "Structure";
                Instantiate(modelToSpawn, transform);
            }

            lateStartInitialized = true;
        }
        if (!buildingPositionSet && modelToSpawn != null)
        {
            //assignedTown.SetBuildingPosition(transform);
        }
        for (int i = 0; i < numberOfAIToSpawn; i++)
        {
            Instantiate(AI, transform.position, transform.rotation);
        }
    }
}
