using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkAssignment : MonoBehaviour
{
    private Structure parentStructure;
    [SerializeField] private bool isMultiTask;  //Sets whether a location should only assign locations based on whats available to work
    [SerializeField] private float timer;
    public List<WorkLocation> locationsToBeWorked = new List<WorkLocation>();   //Whats available to work


    private void Awake()
    {
        parentStructure = GetComponentInParent<Structure>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnGeneration>())
        {
            StartCoroutine(ShortWaitTimer(timer, other.GetComponent<PawnNavigation>()));            
        }
    }

    private IEnumerator ShortWaitTimer(float timerInput, PawnNavigation thisPawn)
    {
        //Debug.Log(thisPawn.name + " detected, stopping");
        thisPawn.agent.ResetPath(); //This was the fix that was needed
        yield return new WaitForSeconds(timerInput);
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
            foreach(WorkLocation workSite in locationsToBeWorked)
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
}
