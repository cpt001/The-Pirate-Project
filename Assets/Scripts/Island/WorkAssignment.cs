using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkAssignment : MonoBehaviour
{
    private Structure parentStructure;
    [SerializeField] private float timer;
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
        thisPawn.agent.ResetPath(); //This was the fix that was needed
        yield return new WaitForSeconds(timerInput);
        foreach (WorkLocation workSite in parentStructure.workSites)
        {
            if (!workSite.isBeingWorked)
            {
                thisPawn.agent.SetDestination(workSite.transform.position);
                EventsManager.TriggerEvent("NewDestination_" + thisPawn.name);
                workSite.isBeingWorked = true;
                break;
            }
        }
    }
}
