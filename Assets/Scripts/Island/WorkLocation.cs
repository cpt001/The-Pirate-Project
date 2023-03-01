using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moving toward a dedicated system of work location/assignment would probably be better than having singular structures. 
/// Or at least, the structure should act as a shop, crafting, and storage instead
/// </summary>
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
    [SerializeField] private bool growsIdly;   //if true, the item will grow without a pawn present
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
            if (!isBeingWorked) //Should prevent doubling up
            {
                isBeingWorked = true;
                targetPawn = other.GetComponent<PawnNavigation>();
                targetPawn.enrouteToAnotherLocation = false;
                targetPawn.timeToWait = workTime;
            }
        }
    }
    private void Update()
    {
        if (!growsIdly)
        {
            if (targetPawn)
            {
                if (readyPercentage <= 100) { readyPercentage += percentIncrease * Time.deltaTime; }
                else { SetNewDestination(targetPawn); }
            }
        }
        else
        {
            if (targetPawn && readyPercentage == 100)
            {
                
                SetNewDestination(targetPawn);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!growsIdly)
        {
            if (other.GetComponent<PawnNavigation>() != null)
            {
                if (readyPercentage <= 100)
                {
                    readyPercentage += percentIncrease * Time.deltaTime;
                }
                else
                {
                    SetNewDestination(targetPawn);
                }
            }
        }
        else
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

                    //Debug.Log("Work location ready for tending!");
                }
            }
            else
            {
                //Debug.Log("Work location monitoring: " + other.name);
            }
        }
    }

    private void SetNewDestination(PawnNavigation pawn)
    {
        isBeingWorked = false;
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
