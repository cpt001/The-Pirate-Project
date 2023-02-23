using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorkLocation : MonoBehaviour
{
    private string title;
    private Structure workSite;
    public bool isBeingWorked;
    [SerializeField] private int workTime;
    [SerializeField] private CargoSO.CargoType itemProduced;
    private PawnNavigation targetPawn;
    private Rigidbody _rb;
    [SerializeField] private Transform chainDestination;
    private float readyPercentage;
    private float percentIncrease = 2f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
        if (GetComponentInParent<Structure>())
        {
            workSite = GetComponentInParent<Structure>();
        }
        readyPercentage = Random.Range(0, 100);
        //title = GetComponentInParent<Transform>().name + "_"
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            isBeingWorked = true;
            targetPawn = other.GetComponent<PawnNavigation>();
            targetPawn.enrouteToAnotherLocation = false;
            targetPawn.timeToWait = workTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PawnNavigation>() == null)
        {
            if (readyPercentage <= 100)
            {
                readyPercentage += percentIncrease * Time.deltaTime;
            }
            else
            {
                readyPercentage = 100;
                
                Debug.Log("Work location ready for tending!");
            }
        }
        else
        {
            Debug.Log("Work location monitoring: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            isBeingWorked = false;
            if (targetPawn.timeToWait == 0)
            {
                targetPawn.GetComponentInChildren<WorldCargo>().cargoItem = itemProduced;
                if (chainDestination)
                {
                    targetPawn.GetComponentInChildren<WorldCargo>().destination = chainDestination;
                }
                EventsManager.TriggerEvent("AdjustCargo_" + targetPawn.name);
            }
        }
    }
}
