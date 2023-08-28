using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This will hold a customer until an interaction is performed from a worklocation, the structure closes, or another event happens
/// -It will also sell items/cargo to customers, both player and otherwise
/// -> to do: modify structure with open and close bool (public)
/// -> Create interaction between cashier (work location), and this
/// </summary>
public class CustomerInteraction : MonoBehaviour
{
    [SerializeField] private Structure parentStructure;
    public bool isOccupied;
    [SerializeField] private bool isPurchasePoint;  //Toggles whether something can be purchased here, or if this is just a waiting spot
    private int waitingTime;
    private List<CargoSO.CargoType> itemsAvailableForPurchase;
    private int internalTimer;
    private PawnNavigation pawnNav;

    private void Awake()
    {
        if (GetComponentInParent<Structure>())
        {
            parentStructure = GetComponentInParent<Structure>();
        }
        EventsManager.StartListening("NewHour", HourTracker);
    }

    void HourTracker()
    {
        internalTimer++;
        if (internalTimer == 24)
        {
            internalTimer = 0;
        }

        if (internalTimer == parentStructure.workEndTime && isOccupied)
        {
            if (pawnNav)
            {
                pawnNav.agent.SetDestination(pawnNav.homeStructure.transform.position);
                EventsManager.TriggerEvent("NewDestination_" + pawnNav.name);
            }
        }
    }

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            pawnNav = other.GetComponent<PawnNavigation>();
            //Maybe time to create a pawn wallet/inventory?
            if (parentStructure.masterWorkerList.Contains(pawnNav.gameObject))
            {
                //Do nothing if it's a worker from the worker list
            }
            else
            {
                isOccupied = true;
                if (isPurchasePoint)
                {
                    //Gets the available cargo for purchase
                    foreach (KeyValuePair<CargoSO.CargoType, int> kvp in parentStructure.cargoAvailable)
                    {
                        itemsAvailableForPurchase.Add(kvp.Key);
                    }

                    EventsManager.TriggerEvent("SummonClerk_" + parentStructure.name);
                    foreach (CustomerInteraction ci in parentStructure.customerInteractionSites)
                    {
                        if (ci.isOccupied && ci != this)
                        {
                            
                        }
                        else
                        {
                            EventsManager.TriggerEvent("ReturnClerk_" + parentStructure.name);
                            break;
                        }
                    }

                    
                    //Send a message to structure's worker list, pick closest worker to counter, route to counter
                }
                else
                {
                    //Short wait timer
                    foreach (CustomerInteraction ci in parentStructure.customerInteractionSites)
                    {
                        if (ci.isPurchasePoint && !isOccupied)
                        {
                            //Route pawn to location
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            pawnNav = null;
            isOccupied = false;
        }
    }
    //Short wait timer, can be overridden
}
