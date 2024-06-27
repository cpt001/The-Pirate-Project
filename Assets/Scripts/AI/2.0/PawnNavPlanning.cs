using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// See if I can clean this up
/// -> update structures to interact with this script a bit better
/// 
/// This class needs to be dumbed down to primary components. 
/// It needs data pertaining to...
/// -Home, work locations
/// -Work status (specifically closing times)
/// </summary>
public class PawnNavPlanning : PawnBaseClass
{
    public PawnBaseClass pawn;
    public float moveSpeed; //How fast they move, ranged
    private bool isSprinting;

    public UnityEngine.AI.NavMeshAgent agent;


    public Structure homeStructure;
    public Structure churchStructure;
    public Structure workPlace;
    private enum OffDutyActivities
    {
        Tavern,
        Sleep,
        ShoreFishing,

    }
    private OffDutyActivities pawnLookingForwardTo;
    private bool onDuty;
    private bool dayOfRest;
    private bool restingDay;
    private bool churchExempt;

    public bool enrouteToAnotherLocation;
    public int timeToWait;
    private int internalHour = -1;
    private int sleepStartTime;
    private int sleepLength;
    [SerializeField] private List<Transform> possibleHomes = new List<Transform>();

    private enum CurrentHighestNeed
    {
        //This sets the smartobject target based on the current highest need; as set by pawnNeeds
    }

    protected override void Awake()
    {
        base.Awake();
        Setup();
    }

    protected override void Start()    //Reminder: Needs to be start, else the listener looks for the wrong pawn name
    {
        base.Start();
        EventsManager.StartListening("NewHour", HourToHour);
        //EventsManager.StartListening("NewDay", DayToDay);
        //EventsManager.StartListening("NewDestination_" + pawn.name, DestinationToggle);
        //Debug.Log("Listening: NewDestination_" + pawn.name);
        //EventsManager.StartListening("DayOfRest", DayOfRest);
        EventsManager.StartListening("FindHome_" + pawn.name, FindHomeStructure);
    }

    /*protected override void Update()
    {
        base.Update();
    }*/
    


    void FindHomeStructure()
    {
        if (!homeStructure)
        {
            if (workPlace != null)
            {
                if (workPlace.thisStructure == Structure.TownStructure.Governors_Mansion)
                {
                    foreach (Transform t in workPlace.transform)
                    {
                        if (t.GetComponent<Structure>())
                        {
                            Structure tempStructure = t.GetComponent<Structure>();
                            if (tempStructure.maxResidents > tempStructure.masterWorkerList.Count)
                            {
                                tempStructure.masterWorkerList.Add(gameObject);
                                homeStructure = tempStructure;
                                break;
                            }
                            else
                            {
                                WorkStructureCast();
                            }
                        }
                    }
                    if (homeStructure == null)
                    {
                        Debug.Log("No home found for " + pawn.name + " on structure " + workPlace.name);
                    }
                }
                //This work structure has no local rooms, and needs to find one elsewhere
                else
                {
                    WorkStructureCast();
                }

                //Sort homes by distance from work place
                possibleHomes = possibleHomes.OrderBy((d) => (d.position - workPlace.transform.position).sqrMagnitude).ToList();

                //Sorts through all available homes, and assigns a free one to the pawn
                foreach (Transform potentialHome in possibleHomes)
                {
                    Structure currentTarget = potentialHome.GetComponent<Structure>();
                    if (currentTarget.masterWorkerList.Count < currentTarget.maxResidents)
                    {
                        currentTarget.masterWorkerList.Add(gameObject);
                        homeStructure = currentTarget;
                        break;
                    }
                    else
                    {
                        //Debug.Log("No free homes found for: " + pawn.gameObject);
                    }
                }

                sleepLength = Mathf.RoundToInt(Random.Range(7, 10));
                sleepStartTime = workPlace.workStartTime - sleepLength;
                if (sleepStartTime < 0)
                {
                    Debug.Log(pawn.name + " sleep start time is outside parameters at: " + sleepStartTime);
                    //sleepStartTime = 24 - 
                }
                //Debug.Log("Sleep Start " + sleepStartTime);
                HourToHour();
            }
            else
            {
                Debug.Log("Workplace is null on: " + gameObject.name);
                homeStructure = GameObject.Find("Tavern").GetComponent<Structure>();
            }
        }
    }

    void WorkStructureCast()
    {
        //This finds possible homes around the pawn's worker structure
        RaycastHit[] itemsCasted = Physics.SphereCastAll(workPlace.transform.position, 500.0f, workPlace.transform.forward);
        foreach (RaycastHit rayHit in itemsCasted)
        {
            if (rayHit.transform.GetComponent<Structure>())
            {
                if (rayHit.transform.GetComponent<Structure>().thisStructure == Structure.TownStructure.House ||
                    rayHit.transform.GetComponent<Structure>().thisStructure == Structure.TownStructure.Shack)
                {
                    possibleHomes.Add(rayHit.transform);
                }
            }
        }
    }

    void Setup()
    {
        pawn = GetComponent<PawnBaseClass>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        //churchStructure = GameObject.Find("Church").GetComponent<Structure>();

        moveSpeed = Random.Range(2.2f, 3.5f);
        agent.speed = moveSpeed;
    }

    /*void SeekSmartObject()
    {
        //Create a list of closest objects to pawn
        //Create a list of interactions from target object.
        //If target object contains a target interaction, navigate to it
        var selectedObject = SmartObjectManager.Instance.RegisteredObjects[0];
        //When in range, perform interaction
        var selectedInteraction = selectedObject.Interactions[0];
        if (selectedInteraction.Usable())
        {
            currentInteraction = selectedInteraction;
            currentInteraction.LockInteraction();
            startedPerforming = false;

            if (!agent.SetDestination(selectedObject.interactionPoint))
            {
                Debug.LogError($"Could not move to {selectedObject.name}");
                currentInteraction = null;
            }
            else
            {
                Debug.Log($"Going to {currentInteraction.DisplayName} at {selectedObject.DisplayName}");
            }
        }
        
        if (homeStructure != null)
        {
            //This should seek through the home structure's items first, then look elsewhere, going outward from the home.
        }
    }*/

    void HourToHour()
    {
        #region ResetInternalHours
        internalHour++;
        if (internalHour == 24)
        {
            internalHour = 0;
        }
        #endregion

        if (internalHour == 0)
        {
            //Fall back for game start, teleports pawn to home
            if (homeStructure)
            {
                //Debug.Log("Pawn teleporting!");
                if (agent.destination != homeStructure.transform.position)
                {
                    agent.Warp(homeStructure.transform.position);
                }
            }
            else
            {
                //Debug.Log("No home structure found for " + pawn.name);
                agent.Warp(GameObject.Find("Tavern").transform.position);
            }
        }
        //if (internalHour == pawn.sleepStartTime)
        {
            if (homeStructure)
            {
                agent.SetDestination(homeStructure.transform.position);
            }
            else if (homeStructure == null)
            {
                //Debug.Log(pawn.name + " seeking tavern");
                agent.SetDestination(GameObject.Find("Tavern").transform.position);
            }
        }
        /*if (workPlace)
        {
            if (pawn.age >= 14)
            {
                if (pawn.isSick != PawnGeneration.Sickness.Bedridden)
                {
                    if (internalHour == workPlace.workStartTime)
                    {
                        if (workPlace.assignmentLocation)
                        {
                            agent.SetDestination(workPlace.assignmentLocation.position);
                        }
                        else
                        {
                            agent.SetDestination(workPlace.transform.position);
                        }
                    }
                }
            }
        }//


        //This locks the pawn from walking somewhere else
        if (!enrouteToAnotherLocation)
        {
            if (timeToWait != 0)
            {
                timeToWait--;
            }
        }

        if (pawn.age >= 15)
        {
            if (pawn.isSick != PawnGeneration.Sickness.Bedridden)
            {
                if (workPlace && enrouteToAnotherLocation != true && !restingDay)
                {
                    EnrouteToWork();
                }
                else if (restingDay)
                {
                    SetErrandDestination();

                }
                else if (internalHour == pawn.sleepStartTime)
                {
                    if (homeStructure != null)
                    {
                        agent.SetDestination(homeStructure.transform.position);
                    }
                    else
                    {
                        agent.SetDestination(GameObject.Find("Tavern").transform.position);
                        //Debug.Log(pawn.name + " has no home, and is going to tavern; originated from: " + transform.position);
                    }
                }
            }
            else
            {
                BedriddenSickness();
            }
        }
        else if (pawn.age <= 15 && pawn.age >= 4)
        {
            Debug.Log("Pawn is young, but can still do chores/errands");
        }
        else if (pawn.age <= 4)
        {
            Debug.Log("Pawn is toddler");
            agent.SetDestination(homeStructure.transform.position);
        }


        //Had to move this down. Objects are destroyed at the end of an update cycle; prevents a repeated error
        if (homeStructure == null && workPlace == null)
        {
            //Debug.Log(pawn.name + " Both structures are null!");
            Destroy(gameObject);
        }
        else if (!homeStructure)
        {
            //Debug.Log(pawn.name + " Home is null!");
        }
        if (!workPlace)
        {
            //Debug.Log(pawn.name + " Workplace is null!");

        }
    }

    void EnrouteToWork()
    {
        //Check distance or collider, then apply agent.stop command
        //Structure seems okay, though its consistently requesting 1 additional worker
        //Implement day of rest, and exceptions
        //Most of the losses seem to be in this script. It's bad, but could be a lot worse
        if (internalHour == workPlace.workStartTime)
        {
            enrouteToAnotherLocation = true;

            //Debug.Log("Work Time");

            if (workPlace.assignmentLocation != null)
            {
                onDuty = true;
                //Debug.Log("Workplace: " + workPlace.name);
                agent.SetDestination(workPlace.assignmentLocation.position);
            }
            else
            {
                onDuty = true;
                agent.SetDestination(workPlace.transform.position);
            }
        }
        if (internalHour == workPlace.workEndTime)
        {
            AfterWorkLiveries();
        }
    }
    void AfterWorkLiveries()
    {
        switch (pawnLookingForwardTo)
        {
            case OffDutyActivities.Tavern:
                {
                    agent.SetDestination(GameObject.Find("Tavern").transform.position);
                    break;
                }
            case OffDutyActivities.Sleep:
                {
                    agent.SetDestination(homeStructure.transform.position);
                    break;
                }
        }


    }
    void SetErrandDestination()
    {
        if (restingDay && internalHour == churchStructure.workStartTime && !churchExempt)
        {
            agent.SetDestination(churchStructure.transform.position);
        }
    }

    void BedriddenSickness()
    {
        //Debug.Log(pawn.name + " is bedridden");
        agent.SetDestination(homeStructure.transform.position);
    }

    void DayToDay()
    {
        pawnLookingForwardTo = (OffDutyActivities)Random.Range(0, System.Enum.GetValues(typeof(OffDutyActivities)).Length);
        //Debug.Log("After work " + pawn.name + " is going to: " + pawnLookingForwardTo);
    }

    void DestinationToggle()
    {
        enrouteToAnotherLocation = true;
        //Debug.Log("New location received, route locked, enroute status: " + enrouteToAnotherLocation);
    }
    void DayOfRest()
    {
        dayOfRest = !dayOfRest;
    }*/
    }
}
