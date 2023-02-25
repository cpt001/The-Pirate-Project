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
    public Structure workPlace;
    private bool dayOfRest;
    private bool restingDay;

    public bool enrouteToAnotherLocation;
    public int timeToWait;
    private int internalHour;

    private void Start()    //Reminder: Needs to be start, else the listener looks for the wrong pawn name
    {
        Setup();
        EventsManager.StartListening("NewHour", HourToHour);
        EventsManager.StartListening("NewDay", DayToDay);
        EventsManager.StartListening("NewDestination_" + pawn.name, DestinationToggle);
        //Debug.Log("Listening: NewDestination_" + pawn.name);
        EventsManager.StartListening("DayOfRest", DayOfRest);
        EventsManager.StartListening("FindHome_" + pawn.name, FindHomeStructure);
    }
    //Possible homes will need to be defined by running a raycube?
    void FindHomeStructure()
    {
        //Debug.Log("Looking for home...");
        List<Transform> possibleHomes = new List<Transform>();
        RaycastHit[] itemsCasted = Physics.SphereCastAll(workPlace.transform.position, 500.0f, workPlace.transform.forward);
        foreach (RaycastHit rayHit in itemsCasted)
        {
            if (rayHit.transform.GetComponent<Structure>())
            {
                Debug.Log("Possible home detected: " + rayHit.transform.name);
                possibleHomes.Add(rayHit.transform);
            }
        }

        Transform bestTarget = null;
        float closestDstSq = Mathf.Infinity;
        Vector3 workplacePosition = workPlace.transform.position;
        foreach (Transform potentialTarget in possibleHomes)
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
                        Debug.Log("No unoccupied houses for: " + pawn.name);
                        //No available houses
                    }
                }
                else
                {
                    Debug.Log("No houses or shacks found for " + pawn.name);
                }
            }
        }
        bestTarget.GetComponent<Structure>().masterWorkerList.Add(pawn);    //This throws a null ref
        homeStructure = bestTarget.GetComponent<Structure>();
        Debug.Log(pawn.name + " Workplace: " + workPlace.gameObject + " || Home: " + homeStructure.gameObject);
    }

    void Setup()
    {
        pawn = GetComponent<PawnGeneration>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();


        moveSpeed = Random.Range(2.2f, 3.5f);
        agent.speed = moveSpeed;
        
    }

    void HourToHour()
    {
        internalHour++;
        if (internalHour == 24)
        {
            internalHour = 0;
        }
        if (internalHour == 0)
        {
            agent.SetDestination(homeStructure.transform.position);
        }
        if (!enrouteToAnotherLocation)
        {
            if (timeToWait != 0)    //This is the cause. Building a super state for time of day might be the fix
            {
                //Debug.Log(name + " Wait timer: " + timeToWait);

                timeToWait--;
            }
        }

        if (pawn.age >= 15)  //This logic should be moved, possibly into the destination toggle
        {
            if (pawn.isSick != PawnGeneration.Sickness.Bedridden)
            {
                if (workPlace && timeToWait == 0 && enrouteToAnotherLocation != true)
                {
                    //Debug.Log("Routing to workplace");
                    if (workPlace.assignmentLocation != null)
                    {
                        agent.destination = workPlace.assignmentLocation.position;
                    }
                    else
                    {
                        agent.destination = workPlace.transform.position;
                    }
                }
                //Check distance or collider, then apply agent.stop command
                //Structure seems okay, though its consistently requesting 1 additional worker
                //Implement day of rest, and exceptions
                //Most of the losses seem to be in this script. It's bad, but could be a lot worse
                if (!restingDay)
                {
                    //Workday schedule

                }
                else
                {
                    //Resting day schedule
                }
            }
        }
    }



    void DayToDay()
    {

    }

    void DestinationToggle()
    {
        enrouteToAnotherLocation = true;
        Debug.Log("New location received, route locked, enroute status: " + enrouteToAnotherLocation);
    }
    void DayOfRest()
    {
        dayOfRest = true;
    }
}
