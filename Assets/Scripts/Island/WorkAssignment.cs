using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is responsible for assigning a local position to the pawn when they arrive at a structure
/// -Can handle worker assignment
/// -Can handle customer assignment
/// </summary>
public class WorkAssignment : MonoBehaviour
{
    private Structure parentStructure;
    [SerializeField] private bool isMultiTask;  //Sets whether a location should only assign locations based on whats available to work
    [SerializeField] private float timer;
    public List<WorkLocation> locationsToBeWorked = new List<WorkLocation>();   //Whats available to work
    public List<CustomerInteraction> locationsToBeShopped = new List<CustomerInteraction>();    //Where to send customers


    private void Awake()
    {
        parentStructure = GetComponentInParent<Structure>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnGeneration>())
        {
            //If the other object works at this location
            if (parentStructure.masterWorkerList.Contains(other.GetComponent<GameObject>()))
            {
                StartCoroutine(ShortWaitTimer(timer, other.GetComponent<PawnNavigation>()));
            }
            else
            {
                //Send customers to interaction locations;
                if (other.GetComponent<PawnNavigation>().agent.destination == this.transform.position)
                {
                    StartCoroutine(ShortWaitTimer(0, other.GetComponent<PawnNavigation>()));
                }
                else
                {
                    //Ignore, the pawn is just passing through
                }
            }
        }
    }

    private IEnumerator ShortWaitTimer(float timerInput, PawnNavigation thisPawn)
    {
        thisPawn.agent.ResetPath(); //This was the fix that was needed
        yield return new WaitForSeconds(timerInput);
        //If the pawn is a worker at this location
        if (parentStructure.masterWorkerList.Contains(thisPawn.gameObject))
        {
            if (!isMultiTask)
            {
                foreach (WorkLocation workSite in parentStructure.workSites)
                {
                    if (!workSite.isBeingWorked)
                    {
                        EventsManager.TriggerEvent("NewDestination_" + thisPawn.name);
                        thisPawn.agent.SetDestination(workSite.transform.position);
                        workSite.isBeingWorked = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (WorkLocation workSite in locationsToBeWorked)
                {
                    if (!workSite.isBeingWorked)
                    {
                        EventsManager.TriggerEvent("NewDestination_" + thisPawn.name);
                        thisPawn.agent.SetDestination(workSite.transform.position);
                        workSite.isBeingWorked = true;
                        break;
                    }
                }
            }
        }
        else
        {
            foreach (CustomerInteraction interactSpot in locationsToBeShopped)
            {
                if (!interactSpot.isOccupied)
                {
                    EventsManager.TriggerEvent("NewDestination_" + thisPawn.name);
                    thisPawn.agent.SetDestination(interactSpot.transform.position);
                    interactSpot.isOccupied = true;
                    break;
                }
                else
                {
                    break;
                    //Its occupied. Ignore
                }
            }
        }                                                                                                                                                                                                                       
    }
}
