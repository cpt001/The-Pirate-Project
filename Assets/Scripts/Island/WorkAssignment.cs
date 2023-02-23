using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkAssignment : MonoBehaviour
{
    private Structure parentStructure;
    [SerializeField] private bool isMultiTask;
    [SerializeField] private float timer;
    public List<WorkLocation> locationsToBeWorked = new List<WorkLocation>();


    private void Awake()
    {
        parentStructure = GetComponentInParent<Structure>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnGeneration>())
        {
            StartCoroutine(ShortWaitTimer(timer, other.GetComponent<PawnGeneration>()));            
        }
    }

    private IEnumerator ShortWaitTimer(float timerInput, PawnGeneration thisPawn)
    {
        //Debug.Log(thisPawn.name + " detected, stopping");
        thisPawn.pawnNavigator.agent.ResetPath(); //This was the fix that was needed
        yield return new WaitForSeconds(timerInput);
        foreach (WorkLocation workSite in parentStructure.workSites)
        {
            if (!workSite.isBeingWorked)
            {
                EventsManager.TriggerEvent("NewDestination_" + thisPawn.name);
                //Debug.Log("Broadcasting: NewDestination_" + thisPawn.name);
                thisPawn.pawnNavigator.agent.SetDestination(workSite.transform.position);
                workSite.isBeingWorked = true;
                break;
            }
        }
    }
}
