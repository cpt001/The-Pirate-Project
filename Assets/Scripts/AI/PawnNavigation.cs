using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PawnGeneration))]
public class PawnNavigation : MonoBehaviour
{
    private PawnGeneration pawn;
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

    private void Awake()
    {
        Setup();
    }

    private void Start()    //Reminder: Needs to be start, else the listener looks for the wrong pawn name
    {
        EventsManager.StartListening("NewHour", HourToHour);
        EventsManager.StartListening("NewDay", DayToDay);
        EventsManager.StartListening("NewDestination_" + pawn.name, DestinationToggle);
        //Debug.Log("Listening: NewDestination_" + pawn.name);
        EventsManager.StartListening("DayOfRest", DayOfRest);
        EventsManager.StartListening("FindHome_" + pawn.name, FindHomeStructure);
    }
    void FindHomeStructure()
    {
        List<Transform> possibleHomes = new List<Transform>();
        RaycastHit[] itemsCasted = Physics.SphereCastAll(workPlace.transform.position, 500.0f, workPlace.transform.forward);
        foreach (RaycastHit rayHit in itemsCasted)
        {
            if (rayHit.transform.GetComponent<Structure>())
            {
                //Debug.Log("Possible home detected: " + rayHit.transform.name);
                possibleHomes.Add(rayHit.transform);
            }
        }

        Transform bestTarget = null;
        float closestDstSq = Mathf.Infinity;
        Vector3 workplacePosition = workPlace.transform.position;
        foreach (Transform potentialTarget in possibleHomes)
        {
            if (!homeStructure)
            {
                Vector3 dirToTarget = potentialTarget.position - workplacePosition;
                if (potentialTarget.GetComponent<Structure>())
                {
                    Structure tempStructure = potentialTarget.GetComponent<Structure>();
                    if ((tempStructure.thisStructure == Structure.TownStructure.House ||
                        tempStructure.thisStructure == Structure.TownStructure.Shack))
                    {
                        if (tempStructure.masterWorkerList.Count < tempStructure.maxResidents)
                        {
                            float dsqrToTarget = dirToTarget.sqrMagnitude;
                            if (dsqrToTarget < closestDstSq)
                            {
                                closestDstSq = dsqrToTarget;
                                bestTarget = potentialTarget;
                            }
                        }
                        else
                        {
                            //Debug.Log("No unoccupied houses for: " + pawn.name);
                            //No available houses
                        }
                    }
                    else
                    {
                        //I think some type of error is preventing some pawns from finding homes, even when there are some available
                        //Debug.Log("No houses or shacks found for " + pawn.name);
                    }
                }
            }
            sleepLength = Mathf.RoundToInt(Random.Range(7, 10));
            sleepStartTime = workPlace.workStartTime - sleepLength;
            Debug.Log("Sleep Start " + sleepStartTime);
            HourToHour();
        }
        bestTarget.GetComponent<Structure>().masterWorkerList.Add(pawn);    //This throws a null ref
        homeStructure = bestTarget.GetComponent<Structure>();
        //Debug.Log(pawn.name + " Workplace: " + workPlace.gameObject + " || Home: " + homeStructure.gameObject);
    }

    void Setup()
    {
        pawn = GetComponent<PawnGeneration>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        churchStructure = GameObject.Find("Church").GetComponent<Structure>();

        moveSpeed = Random.Range(2.2f, 3.5f);
        agent.speed = moveSpeed;
    }

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
            if (homeStructure)
            {
                agent.SetDestination(homeStructure.transform.position);
                //Debug.Log(pawn.name + " going to home structure");

            }
            else
            {

            }
        }
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
                    if (homeStructure)
                    {
                        agent.SetDestination(homeStructure.transform.position);
                    }
                    else
                    {
                        //Debug.Log(pawn.name + " has no home!");
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
    }

    void EnrouteToWork()
    {
        //Check distance or collider, then apply agent.stop command
        //Structure seems okay, though its consistently requesting 1 additional worker
        //Implement day of rest, and exceptions
        //Most of the losses seem to be in this script. It's bad, but could be a lot worse
        if (internalHour == workPlace.workStartTime)
        {
            if (workPlace.assignmentLocation != null)
            {
                onDuty = true;
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
        Debug.Log(pawn.name + " is bedridden");
        agent.SetDestination(homeStructure.transform.position);
    }

    void DayToDay()
    {
        pawnLookingForwardTo = (OffDutyActivities)Random.Range(0, System.Enum.GetValues(typeof(OffDutyActivities)).Length);
        Debug.Log("After work " + pawn.name + " is going to: " + pawnLookingForwardTo);
    }

    void DestinationToggle()
    {
        enrouteToAnotherLocation = true;
        Debug.Log("New location received, route locked, enroute status: " + enrouteToAnotherLocation);
    }
    void DayOfRest()
    {
        dayOfRest = !dayOfRest;
    }
}
