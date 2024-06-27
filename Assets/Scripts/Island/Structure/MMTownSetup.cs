using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
/// <summary>
/// This script is responsible for generating controlled townships through map magic 2.
/// Its generated when a township location is established, and proceeds through a series of items that should 
/// create a fairly convincing looking township at the end.
/// Each township needs to fulfill a key set of parameters, including a minimum residency, and ideal output for shipping
/// 
/// How are town upgrades handled
/// -> towns need to grow over time, maxing with a certain level.
/// -> towns are grown when the population exceeds a certain threshold. 
/// -> the threshold is dynamic, based on the number of desired structures
/// 
/// 
/// MapMagic generates the island, and generates structure targets when it finalizes.
/// -With these targets, the script/town locates them, counts the number, and stores them for later use.
/// -On initial generation, the town will pick a maximum tier with the above input, assigns a phenotype, and then creates a queue of structures
/// -With the queue, the town generates a random number of buildings to initially generate, and applies buildings from the queue
/// </summary>
public class MMTownSetup : MonoBehaviour
{
    private string townName;

    private NavMeshSurface navSurface;
    #region Generation Flags
    //Generation Flags
    private bool abandoned;
    private bool nativeIsland;
    private bool capitalAccountedFor;
    private bool isCapital;
    private bool hasTownHall = false;
    #endregion

    private float generationRadius = 35f;

    #region Town Naming
    //Todo: More prefix and suffix
    private enum Prefix
    {
        Run,
        Gold,
        Sea,
        Rum,
        Kil,
        Mer,
        Mermaid,
        New,
        Dew,
        Little,
        Ship,
        Cape,
    }
    private Prefix townNamePrefix;
    private enum Suffix
    {
        ville,
        town,
        port,
        berg,
        borne,
        forth,
        point,
        folk,
        fort,
        stead,
        worth,
        pool,
        well,
        wick,
        bury,
        stone,
        ington,
        ingville,
        ship,
        state,
        lake,
        plain,
        bar,
        ridge,
        blight,
        flats,
        castle,
    }
    private Suffix townNameSuffix;
    #endregion
    private enum TownBuildingModule
    {
        //TS0 - Colony || 0-4
        FishingHut,         // Num workers: 2 || Num Housed: 2 - Shack can house 2 partners + kids, House can do 4
        GypsyWagon,         // 2 || 2
        HunterShack,        // 3 || 2 - 1 Shack
        LoggingCamp,        // 6 || 0 - 3 Shacks
        MarketStall,        // 2 || 0 - 

        //TS1 - Township || 5-12
        Apiary,             // 3 || 3
        Bakery,             // 4 || 2 - 1 Shack
        Blacksmith,         // 3 || 2 - 1 Shack
        BawdyHouse,         // 10 || 10
        ClayPit,            // 10 || 0 - 5 Shacks
        Prison,             // 2 || 0
        SawMill,            // 10 || 0
        Tavern,             // 4 || 14 - Has temp houses -7 shacks

        //TS2 - Small port || 13-26
        Apothecary,         // 3 || 3
        Armory,             // 1 || 0 - 1 Shack
        Barn,               // 4 || 0 - 2 Shacks
        Butcher,            // 3 || 3
        Carpenter,          // 4 || 4
        Church,             // 1 || 1
        Cobbler,            // 2 || 2
        Garrison,           // 6 || 6
        Leathersmith,       // 3 || 3
        PawnShop,           // 2 || 2
        Tailor,             // 4 || 4
        TarKiln,            // 3 || 0 - 2 Shacks
        Warehouse,          // 3 || 2 - 1 Shack
        Watchtower,         // 2 || 0 - 1 Shack

        //TS3 - Large port || 27-33
        Barber,             // 2 || 2
        Broker,             // 1 || 0 - 1 Shack
        CandleMaker,        // 2 || 2
        JewelerParlor,      // 2 || 2
        Mill,               // 2 || 0 - 1 Shack
        Shipwright,         // 2 || 0 - 1 Shack
        TattooParlor,       // 2 || 2

        //TS4 - Capital || 34-40
        Bank,               // 4 || 0 - 1 House
        Drydock,            // 10 || 0 - 5 Shacks
        House,              // 1 || 4
        Library,            // 3 || 3 
        WigMaker,           // 2 || 2
        Distillery,         // 6 || 0 - 3 Shacks
        Forge,              // 10 || 0 - 5 Shacks

        //TS0 - Specialist || 41-43
        Mineshaft,          // 20 || 10 shacks
        Quarry,             // 20 || 10 shacks
        Plantation,         // 20 || 10 shacks
    }
    private TownBuildingModule townBuilding;

    public List<StructureTool> structuresInTown;
    [SerializeField] private GameObject structureToolPrefab;

    //When the world generates, MM/island creates a number of object points, tagged as structure gen targets
    //This script needs to grab those when they spawn nearby, and then can use those to assign the structures that this script plans out
    [SerializeField] private List<StructureSO> structureSOs = new();
    [SerializeField] private List<Transform> StructureGenerationTargets = new();
    private enum TownPhenotype  //Each of these phenotypes sets a different probability of each structure spawning
    {
        CaravanSite,
        FertileIsland,
        FreePort,
        FishingVillage,
        IndustrialTown,
        LighthouseKeep,
        MiningColony,
        MercantileTradePort,
        ShipBuilders,
        Stronghold,
        SailorsRespite,
        SwampTown,
        PenalColony,
        Ranchland,
        WoodShrouded,
        NativeIsland,
    }
    private TownPhenotype townPhenotype;
    [SerializeField] private Queue<StructureSO> structureQueue = new();
    [SerializeField] private int townMaxTier = 0;
    private int t0MaxBldg, t1MaxBldg, t2MaxBldg, t3MaxBldg, t4MaxBldg;

    //Need to add reference to name function
    //Todo: Add more pre/suffixes | fix name referencing
    void SetTownName()
    {
        if (townPhenotype == TownPhenotype.NativeIsland)
        {
            townName = townPhenotype.ToString();
        }
        else
        {
            //Generate a possible prefix and suffix
            int randPrefix = Random.Range(0, 3);
            int randSuffix = Random.Range(0, 2);
            //Pick a prefix
            if (randPrefix == 2)
            {
                //Choose name from prefix list
                townNamePrefix = (Prefix)Random.Range(0, System.Enum.GetValues(typeof(Prefix)).Length);
                townName = townNamePrefix.ToString();
            }
            else if (randPrefix == 1)
            {
                //Choose npc name from names list   -- choose m or f, pick from one of those names, add as prefix
                townName = "Named";
            }
            else
            {
                //Set name as designated production
                townName = townPhenotype.ToString();
            }

            //Pick a suffix
            if (randSuffix == 1)
            {
                townNameSuffix = (Suffix)Random.Range(0, System.Enum.GetValues(typeof(Suffix)).Length);
                townName = townName + townNameSuffix.ToString();
            }
            else
            {

            }
        }

        //Apply name to gameobject
        gameObject.name = townName;
    }

    void Start()
    {
        //This is part of exclusion - preventing multiple townships from generating in the same area
        if (!transform.GetComponentInParent<IslandManager>())
        {
            GameObject parent = transform.parent.gameObject;
            parent.AddComponent<IslandManager>();
            GetComponentInParent<IslandManager>().townsOnIsland.Add(this);
        }
        else
        {
            GetComponentInParent<IslandManager>().townsOnIsland.Add(this);
        }
        SetTownName();
        GetBuildingPositions();
        SetTownMaxTier();
        SetStructureProbability();
        StartCoroutine(DecideStructureQueue());
        //SpawnBuildingFromQueue();
    }

    //This function determines how many structures may spawn, the resulting tier, town phenotype, and how many structures exist per tier
    //Todo: The structure counts probably need to be tightened a bit
    void SetTownMaxTier()
    {
        int potentialStructureCount = StructureGenerationTargets.Count;
        if (potentialStructureCount <= 8)  //Under 8
        {
            //Debug.Log("Under 8 structures");
            townMaxTier = Mathf.RoundToInt(Random.Range(0, 3)); //0, 1, 2
        }
        else if (potentialStructureCount <= 20 && potentialStructureCount > 8) //Between 8 and 20
        {
            //Debug.Log("Between 8 to 20 structures");
            townMaxTier = Mathf.RoundToInt(Random.Range(1, 4)); //1, 2, 3
        }
        else if (potentialStructureCount > 20)
        {
            //Debug.Log("In else territory");
            if (!capitalAccountedFor)
            {
                townMaxTier = Mathf.RoundToInt(Random.Range(3, 6)); //3, 4, 5 = Capital
            }
            else
            {
                townMaxTier = Mathf.RoundToInt(Random.Range(2, 5)); //2, 3, 4
            }
        }

        townPhenotype = (TownPhenotype)Random.Range(0, System.Enum.GetValues(typeof(TownPhenotype)).Length);
        switch (townMaxTier)
        {
            case 0:
                {
                    t0MaxBldg = potentialStructureCount;
                    break;
                }
            case 1:
                {
                    int t1variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    t1MaxBldg = Mathf.RoundToInt(potentialStructureCount / 2) - t1variance;
                    t0MaxBldg = Mathf.RoundToInt(potentialStructureCount / 2) + t1variance;

                    Debug.Log("Town t1. Variance: " + t1variance + " | t0: " + t0MaxBldg + " | t1: " + t1MaxBldg);
                    break;
                }
            case 2:
                {
                    int t1variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t2variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    t2MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t2variance;
                    t1MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t1variance + t2variance;
                    t0MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) + t1variance;

                    Debug.Log("Town t2. Variance 1: " + t1variance + " | Variance 2: " + t2variance + " | t0: " + t0MaxBldg + " | t1: " + t1MaxBldg + " | t2: " + t2MaxBldg);

                    break;
                }
            case 3:
                {
                    int t1variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t2variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t3variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    t3MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t3variance;
                    t2MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t2variance + t3variance;
                    t1MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t1variance + t2variance;
                    t0MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) + t1variance;

                    Debug.Log("Town t3. Variance 1: " + t1variance + " | Variance 2: " + t2variance + " | Variance 3: " + t3variance
                        + " | t0: " + t0MaxBldg + " | t1: " + t1MaxBldg + " | t2: " + t2MaxBldg + " | t3: " + t3MaxBldg);

                    break;
                }
            case 4:
                {
                    int t1variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t2variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t3variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    int t4variance = Mathf.RoundToInt(potentialStructureCount / Random.Range(4, 9));
                    t3MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t4variance;
                    t2MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t3variance + t4variance;
                    t2MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t2variance + t3variance;
                    t1MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) - t1variance + t2variance;
                    t0MaxBldg = Mathf.RoundToInt(potentialStructureCount / 3) + t1variance;

                    Debug.Log("Town t3. Variance 1: " + t1variance + " | Variance 2: " + t2variance + " | Variance 3: " + t3variance + " | Variance 4: " + t4variance
                        + " | t0: " + t0MaxBldg + " | t1: " + t1MaxBldg + " | t2: " + t2MaxBldg + " | t3: " + t3MaxBldg + " | t4: " + t4MaxBldg);


                    break;
                }
            case 5:
                {

                    break;
                }
        }
    }

    //This sets the spawn probabilities for every structure
    void SetStructureProbability()
    {
        //Get the phenotype
        switch (townPhenotype)
        {
            #region Caravan Site (CS)
            case TownPhenotype.CaravanSite:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Fertile Island
            case TownPhenotype.FertileIsland:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Free Port (FP)
            case TownPhenotype.FreePort:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Fishing Village (FV)
            case TownPhenotype.FishingVillage:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Church"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Industrial Town (IT)
            case TownPhenotype.IndustrialTown:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0.4f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Lighthouse Keep (LK)
            case TownPhenotype.LighthouseKeep:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0.4f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Mining Colony (MC)
            case TownPhenotype.MiningColony:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0.6f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Mercantile Trade Port (MTP)
            case TownPhenotype.MercantileTradePort:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0.2f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Ship Builders (SB)
            case TownPhenotype.ShipBuilders:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Stronghold (SH)
            case TownPhenotype.Stronghold:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Sailors Respite (SR)
            case TownPhenotype.SailorsRespite:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Swamp Town (ST)
            case TownPhenotype.SwampTown:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Penal Colony (PC)
            case TownPhenotype.PenalColony:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0.6f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Ranch Land (RL)
            case TownPhenotype.Ranchland:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Wood Shrouded (WS)
            case TownPhenotype.WoodShrouded:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.4f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0.2f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0.2f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0.4f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
            #endregion
            #region Native Island (NI)
            case TownPhenotype.NativeIsland:
                {
                    //Get each structure
                    foreach (StructureSO str in structureSOs)
                    {
                        //Set based on the structure id - .6, .4, .2, 0
                        switch (str.structureName)
                        {
                            #region Tier 0
                            case ("Fishing Hut"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Gypsy"): { str.structureSpawnProbability = 0f; break; }
                            case ("Hunter Shack"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Logging Camp"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Market Stall"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 1
                            case ("Apiary"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Bakery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bawdy House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Blacksmith"): { str.structureSpawnProbability = 0f; break; }
                            case ("Saw Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tavern"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Tier 2
                            case ("Apothecary"): { str.structureSpawnProbability = 0f; break; }
                            case ("Armory"): { str.structureSpawnProbability = 0f; break; }
                            case ("Barn"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Carpenter"): { str.structureSpawnProbability = 0f; break; }
                            case ("Cobbler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Garrison"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Leathersmith"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Pawn Shop"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tailor"): { str.structureSpawnProbability = 0f; break; }
                            case ("Tar Kiln"): { str.structureSpawnProbability = 0f; break; }
                            case ("Warehouse"): { str.structureSpawnProbability = 0f; break; }
                            case ("Watchtower"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 3
                            case ("Barber"): { str.structureSpawnProbability = 0f; break; }
                            case ("Butcher"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Candle Maker"): { str.structureSpawnProbability = 0f; break; }
                            case ("Jeweler"): { str.structureSpawnProbability = 0f; break; }
                            case ("Mill"): { str.structureSpawnProbability = 0f; break; }
                            case ("Shipwright"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Tattoo Parlor"): { str.structureSpawnProbability = 0.6f; break; }
                            #endregion
                            #region Tier 4
                            case ("Admiralty House"): { str.structureSpawnProbability = 0f; break; }
                            case ("Bank"): { str.structureSpawnProbability = 0f; break; }
                            case ("Drydock"): { str.structureSpawnProbability = 0f; break; }
                            case ("Library"): { str.structureSpawnProbability = 0f; break; }
                            case ("Wig Maker"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Capital
                            case ("Distillery"): { str.structureSpawnProbability = 0f; break; }
                            case ("Forge"): { str.structureSpawnProbability = 0f; break; }
                            #endregion
                            #region Specialist and Housing
                            case ("ClayPit"): { str.structureSpawnProbability = 0.6f; break; }
                            case ("Mineshaft"): { str.structureSpawnProbability = 0f; break; }
                            case ("Plantation"): { str.structureSpawnProbability = 0f; break; }
                            case ("Quarry"): { str.structureSpawnProbability = 0f; break; }
                                #endregion
                        }
                    }
                    break;
                }
                #endregion
        }
    }

    /// <summary>
    /// This rolls one structure at a time. Each tier needs to roll for each structure within that tier, until the queue is filled.
    /// Todo:
    /// -Some structures can spawn houses. This detracts from the structure population
    /// -When a town rises in tier, it needs to add an appropriate point of interest to the town for generation queue
    /// -In the future, this script needs to take into account generation flags as well
    /// </summary>
    private IEnumerator DecideStructureQueue()
    {
        //When a building with a negative housing balance gets queued, it needs to queue a house immediately after
        //Any one warehouse needs to spawn a broker next if one isn't already queued
        //while (structureQueue.Count < StructureGenerationTargets.Count)
        {
            #region Structure list divided by tier
            List<StructureSO> structuret0SOList = new();
            List<StructureSO> structuret1SOList = new();
            List<StructureSO> structuret2SOList = new();
            List<StructureSO> structuret3SOList = new();
            List<StructureSO> structuret4SOList = new();
            List<StructureSO> structuret5SOList = new();
            foreach (StructureSO structure in structureSOs)
            {
                if (structure.structureTier == 0) { structuret0SOList.Add(structure); }
                if (structure.structureTier == 1) { structuret1SOList.Add(structure); }
                if (structure.structureTier == 2) { structuret2SOList.Add(structure); }
                if (structure.structureTier == 3) { structuret3SOList.Add(structure); }
                if (structure.structureTier == 4) { structuret4SOList.Add(structure); }
                if (structure.structureTier == 5) { structuret5SOList.Add(structure); }
            }
            #endregion
            #region Points of Interest
            /*foreach (StructureSO so in structureSOs)
            {

                if (so.structureName == "Town Square")      
                {
                    int RandChance = Random.Range(0, 1);
                    if (RandChance > .4f)
                    {
                        structureQueue.Enqueue(so);
                        t2MaxBldg--;
                    }
                }
                if (so.structureName == "Dock")             
                {
                    int RandChance = Random.Range(0, 1);
                    if (RandChance > .6f)
                    {
                        structureQueue.Enqueue(so);
                        t1MaxBldg--;
                    }
                }
                if (so.structureName == "Graveyard")        
                { 
                    structureQueue.Enqueue(so); 
                    t1MaxBldg--; 
                }
                if (so.structureName == "Fort")
                {
                    int RandChance = Random.Range(0, 1);
                    if (RandChance > .2f)
                    {
                        structureQueue.Enqueue(so);
                        t2MaxBldg--;
                    }
                }
                if (so.structureName == "Church")           
                { 
                    structureQueue.Enqueue(so); 
                    t2MaxBldg--; 
                }
                if (so.structureName == "Prison")
                {
                    structureQueue.Enqueue(so);
                    t2MaxBldg--;
                }
                if (so.structureName == "Town Hall")        
                {
                    int RandChance = Random.Range(0, 1);
                    if (RandChance > .4f)
                    {
                        hasTownHall = true;
                        structureQueue.Enqueue(so);
                        t3MaxBldg--;
                    }
                }
                if (so.structureName == "Governors Mansion") 
                {
                    if (!hasTownHall)
                    {
                        structureQueue.Enqueue(so);
                        t3MaxBldg--;
                    }
                }
                if (so.structureName == "Lighthouse")       
                { 
                    structureQueue.Enqueue(so); 
                    t3MaxBldg--; 
                }
                if (so.structureName == "Clocktower")       
                { 
                    structureQueue.Enqueue(so); 
                    t4MaxBldg--; 
                }
                //Direct assignment seems to override the for loops. May need to apply within
                //Need to check for the limiter per tier. Above may solve
            }*/
            #endregion
            #region Tier 0
            while (structureQueue.Count < t0MaxBldg )   //Swapping to a while loop may fix the issue where a for loop does not
            {
                if (t0MaxBldg == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well") { structureQueue.Enqueue(so); }
                    }
                }
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret0SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret0SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret0SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret0SOList[randChoice].maxWorkers > structuret0SOList[randChoice].numHousable)
                        {
                            //Debug.Log("Housing difference found");
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret0SOList[randChoice].maxWorkers - structuret0SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t0MaxBldg--;
                                AddHouseToQueue();
                            }
                            //If there are no remaining t0 structure slots, remove from t1, place in t0
                            if (t0MaxBldg == -1 && t1MaxBldg > 0)
                            {
                                Debug.Log("Reductive housing difference");
                                t1MaxBldg--;
                                t0MaxBldg++;
                            }
                            //If there are no remaining slots in t0 or t1, replace this structure with a house
                            else if (t0MaxBldg == -1 && t1MaxBldg >= 0)
                            {
                                //Need to remove the last structure from the queue, and probably replace it with a house
                                Debug.Log("Replacitive housing difference");
                                //structureQueue.Clear();
                                AddHouseToQueue();
                            }
                        }
                    }
                }

            }
            /*for (int i = 0; i < t0MaxBldg; i++)
            {
                //Queue the well first in line -- Why does this trigger twice?
                if (i == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well")
                        {
                            //Debug.Log("Water well added under SO " + so.name);
                            structureQueue.Enqueue(so);
                        }
                    }
                }
                //Roll for every other structure afterward
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret0SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret0SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret0SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret0SOList[randChoice].maxWorkers > structuret0SOList[randChoice].numHousable)
                        {
                            //Debug.Log("Housing difference found");
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret0SOList[randChoice].maxWorkers - structuret0SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t0MaxBldg--;
                                AddHouseToQueue();
                            }
                            //If there are no remaining t0 structure slots, remove from t1, place in t0
                            if (t0MaxBldg == -1 && t1MaxBldg > 0)
                            {
                                Debug.Log("Reductive housing difference");
                                t1MaxBldg--;
                                t0MaxBldg++;
                            }
                            //If there are no remaining slots in t0 or t1, replace this structure with a house
                            else if (t0MaxBldg == -1 && t1MaxBldg >= 0)
                            {
                                //Need to remove the last structure from the queue, and probably replace it with a house
                                Debug.Log("Replacitive housing difference");
                                //structureQueue.Clear();
                                AddHouseToQueue();
                            }
                        }
                    }
                }
            }*/
            #endregion
            #region Tier 1
            for (int i = 0; i < t1MaxBldg; i++)
            {
                //Queue the well first in line -- Why does this trigger twice?
                if (i == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well")
                        {
                            //Debug.Log("Water well added under SO " + so.name);
                            structureQueue.Enqueue(so);
                        }
                    }
                }
                //Roll for every other structure afterward
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret1SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret1SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret1SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret1SOList[randChoice].maxWorkers < structuret1SOList[randChoice].numHousable)
                        {
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret1SOList[randChoice].maxWorkers - structuret1SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t1MaxBldg--;
                                AddHouseToQueue();
                            }
                            //If there are no remaining t0 structure slots, remove from t1, place in t0
                            if (t1MaxBldg == -1 && t2MaxBldg > 0)
                            {
                                Debug.Log("Reductive housing difference");
                                t2MaxBldg--;
                                t1MaxBldg++;
                            }
                            //If there are no remaining slots in t0 or t1, replace this structure with a house
                            else if (t1MaxBldg == -1 && t2MaxBldg >= 0)
                            {
                                //Need to remove the last structure from the queue, and probably replace it with a house
                                Debug.Log("Replacitive housing difference");
                                //structureQueue.Clear();
                                AddHouseToQueue();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Tier 2
            for (int i = 0; i < t2MaxBldg; i++)
            {
                //Queue the well first in line -- Why does this trigger twice?
                if (i == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well")
                        {
                            //Debug.Log("Water well added under SO " + so.name);
                            structureQueue.Enqueue(so);
                        }
                    }
                }
                //Roll for every other structure afterward
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret2SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret2SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret2SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret2SOList[randChoice].maxWorkers > structuret2SOList[randChoice].numHousable)
                        {
                            //Debug.Log("Housing difference found");
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret2SOList[randChoice].maxWorkers - structuret2SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t2MaxBldg--;
                                AddHouseToQueue();
                            }
                            //If there are no remaining t0 structure slots, remove from t1, place in t0
                            if (t2MaxBldg == -1 && t1MaxBldg > 0)
                            {
                                Debug.Log("Reductive housing difference");
                                t3MaxBldg--;
                                t2MaxBldg++;
                            }
                            //If there are no remaining slots in t0 or t1, replace this structure with a house
                            else if (t2MaxBldg == -1 && t3MaxBldg >= 0)
                            {
                                //Need to remove the last structure from the queue, and probably replace it with a house
                                Debug.Log("Replacitive housing difference");
                                structureQueue.Clear();
                                AddHouseToQueue();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Tier 3
            for (int i = 0; i < t3MaxBldg; i++)
            {
                //Queue the well first in line -- Why does this trigger twice?
                if (i == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well")
                        {
                            //Debug.Log("Water well added under SO " + so.name);
                            structureQueue.Enqueue(so);
                        }
                    }
                }
                //Roll for every other structure afterward
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret3SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret3SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret3SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret3SOList[randChoice].maxWorkers > structuret3SOList[randChoice].numHousable)
                        {
                            //Debug.Log("Housing difference found");
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret3SOList[randChoice].maxWorkers - structuret3SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t3MaxBldg--;
                                AddHouseToQueue();
                            }
                            //If there are no remaining t0 structure slots, remove from t1, place in t0
                            if (t3MaxBldg == -1 && t4MaxBldg > 0)
                            {
                                Debug.Log("Reductive housing difference");
                                t4MaxBldg--;
                                t3MaxBldg++;
                            }
                            //If there are no remaining slots in t0 or t1, replace this structure with a house
                            else if (t3MaxBldg == -1 && t4MaxBldg >= 0)
                            {
                                //Need to remove the last structure from the queue, and probably replace it with a house
                                Debug.Log("Replacitive housing difference");
                                //structureQueue.Clear();
                                AddHouseToQueue();
                            }
                        }
                    }
                }
            }
            #endregion
            #region Tier 4
            for (int i = 0; i < t4MaxBldg; i++)
            {
                //Queue the well first in line -- Why does this trigger twice?
                if (i == 0)
                {
                    foreach (StructureSO so in structureSOs)
                    {
                        if (so.structureName == "Water Well")
                        {
                            //Debug.Log("Water well added under SO " + so.name);
                            structureQueue.Enqueue(so);
                        }
                    }
                }
                //Roll for every other structure afterward
                else
                {
                    //Generate a random choice from the list, and add it to the queue
                    int randChoice = Random.Range(0, structuret4SOList.Count);
                    float randRoll = Random.Range(0, 2);
                    if (randRoll <= structuret4SOList[randChoice].structureSpawnProbability)
                    {
                        structureQueue.Enqueue(structuret4SOList[randChoice]);  //Queue the random structure
                        //Check the structure's housing capacity. If less, add house
                        if (structuret4SOList[randChoice].maxWorkers > structuret4SOList[randChoice].numHousable)
                        {
                            //Debug.Log("Housing difference found");
                            //Find difference in housing, and add houses accordingly
                            int housingDifference = Mathf.RoundToInt(structuret4SOList[randChoice].maxWorkers - structuret4SOList[randChoice].numHousable);
                            housingDifference = Mathf.RoundToInt(housingDifference / 2);
                            for (int j = 0; j < housingDifference; j++)
                            {
                                Debug.Log("Simple housing difference");
                                t4MaxBldg--;
                                AddHouseToQueue();
                            }
                            //T4 is capital. It shouldn't have an overflow
                        }
                    }
                }
            }
            #endregion
            yield return new WaitForEndOfFrame();
            SpawnBuildingFromQueue();
        }

        void AddHouseToQueue()
        {
            //Debug.Log("House should be added to queue");
            foreach (StructureSO so in structureSOs)
            {
                if (so.structureName == "Housing")
                {
                    structureQueue.Enqueue(so);
                }
            }
        }

        //This handles structure spawning
        //Todo: Handle structures outside a key radius, and allow randomized face direction
        void SpawnBuildingFromQueue()
        {
            foreach (Transform t in StructureGenerationTargets)
            {
                Vector3 parentDirection = new Vector3(transform.position.x, t.position.y, transform.position.z);
                if (structureQueue.Count != 0)
                {
                    t.GetComponent<MMStructureSpawner>().SetStructure(structureQueue.Dequeue());
                }
                else
                {
                    Debug.Log("Squeue didn't populate correctly");
                }
                //Bldgs need to face center if within certain radius
                t.transform.LookAt(parentDirection);
            }
        }

        /*//The insanity of this is being caused by the raycast not always detecting the terrain.
        public void SetBuildingPosition(Transform objectToSet)
        {
            //This gives the building coordinates to generate to
            objectToSet.transform.localPosition = Random.insideUnitSphere * generationRadius;
            RaycastHit rayHit;
            if (Physics.Raycast(objectToSet.transform.position, Vector3.up, out rayHit) || Physics.Raycast(objectToSet.transform.position, -Vector3.up, out rayHit))
            {
                //Debug.Log("Raycast for: " + objectToSet + " || "+ rayHit.point);
                //Vector3 localHit = transform.InverseTransformPoint(rayHit.point);
                //->objectToSet.transform.localPosition = transform.InverseTransformPoint(rayHit.point);
                objectToSet.transform.position = rayHit.point;
                objectToSet.GetComponent<StructureTool>().buildingPositionSet = true;
            }
            else
            {
                //Debug.Log("Raycast not detecting anything for " + objectToSet);
                objectToSet.transform.position = new Vector3(objectToSet.transform.position.x, 0, objectToSet.transform.position.z);
                SetBuildingPosition(objectToSet);
            }
            //Vector3 lookDirection = new Vector3(objectToSet.parent.transform.position.x, objectToSet.parent.transform.localPosition.y, 0);//transform.position.z);
            //objectToSet.transform.LookAt(lookDirection);
            //Instantiate doors at target position - this requires some changes to the prefabs
        }*/
    }

    private void GetBuildingPositions()
    {
        foreach (RaycastHit r in Physics.SphereCastAll(transform.position, 80.0f, Vector3.forward))
        {
            if (r.transform.CompareTag("StructureGenTarget"))
            {
                StructureGenerationTargets.Add(r.transform);
            }
        }
        foreach (Transform t in StructureGenerationTargets)
        {
            t.parent = transform;
        }
    }
}
