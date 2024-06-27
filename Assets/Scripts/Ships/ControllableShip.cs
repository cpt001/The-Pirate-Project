using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using Pathfinding;

/// <summary>
/// Bottom H can be manipulated to create a sinking effect.
/// -If the ship is actively sinking, the forward speed can be slowed * sink level
/// -if it's below the main deck, the ship is sunk. Movement speed can be set to 0, or very slow.
/// </summary>
public class ControllableShip : MonoBehaviour
{
    public Crest.BoatProbes boatTarget;
    [SerializeField] private StarterAssets.ThirdPersonController playerController;
    [SerializeField] private CinemachineBrain cameraBrain;

    public bool anchorDropped;
    private Rigidbody _rb;
    [SerializeField] private Transform bowSprit;

    //public Queue

    private enum CrewRank
    {
        Captain,
        FirstMate,
        Boatswain,
        Leftenant,
        Crewmate,
        Swabbie
    }
    private Dictionary<GameObject, CrewRank> crew = new Dictionary<GameObject, CrewRank>();

    [Header("Sail Schematic")]
    [SerializeField] private List<FullSail.Sail> DeadSailRig;
    [SerializeField] private List<FullSail.Sail> BattleSailRig;
    [SerializeField] private List<FullSail.Sail> SlowSailRig;
    [SerializeField] private List<FullSail.Sail> HalfSailRig;
    [SerializeField] private List<FullSail.Sail> FullSailRig;
    /*[SerializeField] private List<GameObject> DeadSlowSail;
    [SerializeField] private List<GameObject> BattleSail;
    [SerializeField] private List<GameObject> SlowSail;
    [SerializeField] private List<GameObject> HalfSail;
    [SerializeField] private List<GameObject> FullSailGO;*/

    [Header("Broadside Setup")]
    [SerializeField] private List<Cannon> portBroadside;
    [SerializeField] private List<Cannon> starboardBroadside;
    [SerializeField] private List<Cannon> bowCannons;
    [SerializeField] private List<Cannon> sternCannons;
    [SerializeField] private enum FireMode
    {
        ForeToStern,
        SternToFore,
        Random,
        reset,
    }
    private FireMode firingOrder;

    private int setSail;

    private enum SailState
    {
        NoSail,
        //RowboatTug,
        DeadSlow,
        Battle,
        Slow,
        Half,
        Full,
    }
    [SerializeField] private SailState sailState;

    //This follows the Janka Hardness scale
    public enum HullWoodType
    {
        Pine,   //Basswood
        Poplar,
        Fir,    //SY Pine, Cherry, Black Walnut, Eng Oak, Teak, Red Oak
        Ash,
        Oak,    //Bamboo, wenge, Rosewood
        Hickory,    //Bubinga
        Mesquite,   //Ebony, Ipe
    }
    public HullWoodType hullWood;

    public enum LeakSeverity
    {
        Severe,
        Fast,
        Flowing,
        Trickle,
        Intact,

        Patched,
    }
    public LeakSeverity leakSpeed = LeakSeverity.Intact;

    public enum ShipClass
    {
        Schooner,
    }
    private ShipClass shipClass;

    public Dictionary<Crest.FloaterForcePoints, LeakSeverity> leakInformation = new Dictionary<Crest.FloaterForcePoints, LeakSeverity>();

    public bool playerControllingShip;
    [SerializeField] private bool shipMarkedForGraphing = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        foreach (Transform t in transform.Find("BroadsidePort"))
        {
            if (t.gameObject.GetComponent<Cannon>())
            {
                portBroadside.Add(t.GetComponent<Cannon>());
            }
        }
        foreach (Transform t in transform.Find("BroadsideStarboard"))
        {
            if (t.gameObject.GetComponent<Cannon>())
            {
                starboardBroadside.Add(t.GetComponent<Cannon>());
            }
        }
    }

    private void Start()
    {
        if (shipMarkedForGraphing)
        {
            StartCoroutine(GenerateGraphForShip());
        }
    }

    //This creates a new A* graph for the ship
    private IEnumerator GenerateGraphForShip()
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

        switch (shipClass)
        {
            case ShipClass.Schooner:
            {
                rg.name = "SchoonerGraph";
                rg.forcedBoundsSize.x = 10f;
                rg.forcedBoundsSize.y = 26f;
                rg.forcedBoundsSize.z = 29f;
                break;
            }
        }


        rg.Scan();
        yield return null;
    }

    private void Update()
    {

        if (playerControllingShip)
        {
            //Debug.Log("Player is controlling ship");
            //playerController.MoveSpeed = 0;
            //playerController.JumpHeight = 0;
            playerController.playerPositionLocked = true;
            boatTarget._playerControlled = true;
            //cameraBrain.

            if (anchorDropped)
            {
                boatTarget._engineBias = 0;
                _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints.None;
            }

            if (Input.GetKeyDown(KeyCode.W) && sailState != SailState.Full)
            {
                Debug.Log("Sails up! " + sailState);
                setSail++;
                sailState = (SailState)setSail;
            }
            if (Input.GetKeyDown(KeyCode.S) && sailState != SailState.NoSail)
            {
                Debug.Log("Sails down!");
                setSail--;
                sailState = (SailState)setSail;

            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EventsManager.TriggerEvent("Broadside_Port_" + gameObject.transform.name);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EventsManager.TriggerEvent("Broadside_Starboard_" + gameObject.transform.name);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                firingOrder++;
                UpdateFiringMode();
                if (firingOrder == FireMode.reset)
                {
                    firingOrder = FireMode.ForeToStern;
                }
            }
        }
        else
        {
            playerController.playerPositionLocked = false;
            //playerController.MoveSpeed = 2.0f;
            //playerController.JumpHeight = 2.0f;
            boatTarget._playerControlled = false;
        }

        ShipSailCaseUpdate();
    }

    private void FixedUpdate()
    {
        CalculateLeaks();
    }

    void UpdateFiringMode()
    {
        //https://forum.unity.com/threads/sorting-a-list-of-vector3-by-x-values.126237/
        //IEnumerable<Vector3> sorted = portBroadside.OrderBy<>
        //smods.OrderBy(bastard => Vector3.Distance(transform.position, sm.transform.position));
        switch (firingOrder)
        {
            case FireMode.ForeToStern:
                {
                    portBroadside = portBroadside.OrderBy((d) => (d.transform.position - bowSprit.position).sqrMagnitude).ToList();
                    starboardBroadside = starboardBroadside.OrderBy((d) => (d.transform.position - bowSprit.position).sqrMagnitude).ToList();
                    break;
                }
            case FireMode.SternToFore:
                {
                    portBroadside = portBroadside.OrderBy((d) => (d.transform.position + bowSprit.position).sqrMagnitude).ToList();
                    starboardBroadside = starboardBroadside.OrderBy((d) => (d.transform.position + bowSprit.position).sqrMagnitude).ToList();
                    break;
                }
            case FireMode.Random:
                {
                    for (int i = 0; i < portBroadside.Count - 1; i++)
                    {
                        int rand = Random.Range(i, portBroadside.Count);
                        Cannon tempCannon = portBroadside[rand];
                        portBroadside[rand] = portBroadside[i];
                        portBroadside[i] = tempCannon;
                    }                    
                    for (int i = 0; i < starboardBroadside.Count - 1; i++)
                    {
                        int rand = Random.Range(i, starboardBroadside.Count);
                        Cannon tempCannon = starboardBroadside[rand];
                        starboardBroadside[rand] = starboardBroadside[i];
                        starboardBroadside[i] = tempCannon;
                    }
                    break;
                }
        }
    }

    void ShipSailCaseUpdate()
    {
        switch(sailState)
        {
            #region No Sails
            #region Old Version
            case SailState.NoSail:
            {
                    boatTarget._engineBias = 0;

                /*foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(false);
                    }*/
                    #endregion
                    foreach (FullSail.Sail sail in DeadSailRig)
                    {
                        sail.SetValue(FullSail.SailParamID.Furl, 2f);
                    }
                    foreach (FullSail.Sail sail in BattleSailRig)
                    {
                        sail.SetValue(FullSail.SailParamID.Furl, 2f);
                    }
                    foreach (FullSail.Sail sail in SlowSailRig)
                    {
                        sail.SetValue(FullSail.SailParamID.Furl, 2f);
                    }
                    foreach (FullSail.Sail sail in HalfSailRig)
                    {
                        sail.SetValue(FullSail.SailParamID.Furl, 2f);
                    }
                    foreach (FullSail.Sail sail in FullSailRig)
                    {
                        sail.SetValue(FullSail.SailParamID.Furl, 2f);
                    }
                    break;
            }
            #endregion
            #region Rowboat Tug (NYI)
            /*case SailState.RowboatTug:
            {
                    boatTarget._engineBias = 1;

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }
                    break;
            }*/
            #endregion
            #region Dead Slow Sail
            case SailState.DeadSlow:
                {
                    #region Original Sails
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = .5f;
                    }

                    /*foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(true);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(false);
                    }*/
                    #endregion
                    #region New Sails
                    foreach (FullSail.Sail sail in DeadSailRig)
                    {
                        //sail.GetParam(FullSail.SailParamID.Furl).fval = 0f;     //Need to set value, not get value
                        sail.SetValue(FullSail.SailParamID.Furl, 0f);
                    }
                    
                    #endregion
                    break;
                }
            #endregion            
            #region Battle Sails
            case SailState.Battle:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = .75f;
                    }

                    /*foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(true);
                    }*/
                    break;
                }
            #endregion            
            #region Slow Sail
            case SailState.Slow:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 1f;
                    }

                    /*foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(true);
                    }*/
                    break;
                }
            #endregion            
            #region Half Sail
            case SailState.Half:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 1.5f;
                    }

                    /*foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(true);
                    }*/
                    break;
                }
            #endregion            
            #region Full Sail
            case SailState.Full:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 2f;
                    }
                    /*foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSailGO)
                    {
                        sail.SetActive(true);
                    }*/
                    break;
                }
            #endregion
        }
    }

    //Triggered on cannonball impacting this collider; need to create a decal and particle in the future
    public void AddNewLeak(Vector3 position, LeakSeverity leakSeverity)
    {
        float bestDistance = 2000;
        Crest.FloaterForcePoints bestForcePoint = null;
        //Looks for nearest floater point
        foreach (Crest.FloaterForcePoints forcePoint in boatTarget._forcePoints)
        {
            float distance = Vector3.Distance(forcePoint._offsetPosition, position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestForcePoint = forcePoint;
            }
        }

        Debug.Log("Best force point: " + bestForcePoint);
        //Add to dict with assigned floater point, and leak rate
        if (!leakInformation.ContainsKey(bestForcePoint))   //This removes an error with duplication
        {
            leakInformation.Add(bestForcePoint, leakSeverity);
        }
        else
        {
            //This adds more damage to the leak
            if (leakInformation.ContainsValue(LeakSeverity.Severe))
            {
                //Do nothing; the leak is already as severe as it can get.
            }
            else
            {
                //Roll random, apply more damage or move on
                int rand = Random.Range(0, 2);
                if (rand == 1)
                {
                    //Get old value, wipe, apply new value
                    leakInformation[bestForcePoint]--;
                }
                else
                {
                    
                }
            }
        }
    }

    //Need to check if impact is above waterline somehow
    void CalculateLeaks()
    {
        //This iterates through each floater/leak, and applies the change as a leak over time
        foreach (KeyValuePair<Crest.FloaterForcePoints, LeakSeverity> leaker in leakInformation)
        {
            switch (leaker.Value)
            {
                case LeakSeverity.Severe:
                    {
                        leaker.Key._offsetPosition.y += .03f;
                        break;
                    }
                case LeakSeverity.Fast:
                    {
                        leaker.Key._offsetPosition.y += .015f;
                        break;
                    }
                case LeakSeverity.Flowing:
                    {
                        leaker.Key._offsetPosition.y += .002f;
                        break;
                    }
                case LeakSeverity.Trickle:
                    {
                        leaker.Key._offsetPosition.y += .0004f;
                        break;
                    }
                case LeakSeverity.Intact:
                    {
                        leaker.Key._offsetPosition.y = 0;
                        break;
                    }
                case LeakSeverity.Patched:
                    {
                        leaker.Key._offsetPosition.y = 0;
                        break;
                    }
            }
        }
/*
        //Master calculation. Might be useful later?
        foreach (Crest.FloaterForcePoints forcePoint in boatTarget._forcePoints)
        {
            //switch case for determining sink rate
            switch (leakSpeed)
            {
                case LeakSeverity.Severe:
                {
                        forcePoint._offsetPosition.y += .03f;
                        break;
                }
                case LeakSeverity.Fast:
                    {
                        forcePoint._offsetPosition.y += .015f;
                        break;
                    }
                case LeakSeverity.Flowing:
                    {
                        forcePoint._offsetPosition.y += .002f;
                        break;
                    }
                case LeakSeverity.Trickle:
                    {
                        forcePoint._offsetPosition.y += .0004f;
                        break;
                    }
                case LeakSeverity.Intact:
                    {
                        forcePoint._offsetPosition.y = 0;
                        break;
                    }
                case LeakSeverity.Patched:
                    {
                        forcePoint._offsetPosition.y = 0;
                        break;
                    }
            }
            //Apply sink rate to each force point with each frame, fixed update 
        }
*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collision with ship detected: " + collision.transform.name);
    }
}
