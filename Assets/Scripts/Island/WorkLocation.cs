using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moving toward a dedicated system of work location/assignment would probably be better than having singular structures. 
/// Or at least, the structure should act as a shop, crafting, and storage instead
/// 
/// System goal:
/// Work location is farmed, or grows idly.
/// -When the percentage hits 100%, an item from the structure's cargo produced list is generated.
/// -When a pawn sees 100%, it either spends time gathering the item, or grabs it
/// -The item is then placed in structure storage
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WorkLocation : MonoBehaviour
{
    [SerializeField] private Structure parentStructure;
    public bool isBeingWorked;
    [SerializeField] private int workTime;                          //How long a pawn will work at this spot in one sitting
    [SerializeField] private CargoSO.CargoType itemProduced;        //No matter what, this will need to be defined by me
    [SerializeField] private List<CargoSO.CargoType> canProduce;    //What can be produced at this location
    private PawnNavigation targetPawn;
    private PawnNavigation permedPawn;
    [SerializeField] private bool permanentWorker;
    private Rigidbody _rb;
    [SerializeField] private Transform chainDestination;
    [SerializeField] private bool growsIdly;   //if true, the item will grow without a pawn present
    private float readyPercentage;
    private float percentIncrease = 2f;
    private bool creatingItemForPlayerShop;
    private bool isTemporaryContainer;  //A temporary storage block for items in transit
    private bool isStorageItem;
    public bool isSalePoint;

    //Moved froms Awake -> Start; work locations were missing key data passed from structure awake
    private void Start()
    {
        Setup();
        //Debug.Log("Parent structure: " + parentStructure + " || Type: " + parentStructure.thisStructure);
        foreach (KeyValuePair<CargoSO.CargoType, int> cargo in parentStructure.cargoProduced)
        {
            //Debug.Log(cargo.Key + " can made here: " + this.name);
            canProduce.Add(cargo.Key);
        }


    }
    void Setup()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
        if (GetComponentInParent<Structure>())
        {
            parentStructure = GetComponentInParent<Structure>();
        }
        readyPercentage = Random.Range(0, 100);
    }

    private void WorkOnPlayerItemForStore()
    {
        EventsManager.StopListening("PlayerObjectReadyToCraft_" + parentStructure.name, WorkOnPlayerItemForStore);
        creatingItemForPlayerShop = true;
    }

    //Capture pawn data, assign permanent pawn if needed; also checks for player item crafting requests
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnNavigation>())
        {
            //Need customer slot. Allows for pawns to be assigned to locations that aren't part of their work cycle freely
            if (permanentWorker && !isBeingWorked && permedPawn == null)
            {
                isBeingWorked = true;
                permedPawn = other.GetComponent<PawnNavigation>();
                targetPawn.enrouteToAnotherLocation = false;
                permedPawn.timeToWait = workTime;
            }
            else if (!isBeingWorked)
            {
                isBeingWorked = true;
                targetPawn = other.GetComponent<PawnNavigation>();
                targetPawn.enrouteToAnotherLocation = false;
                targetPawn.timeToWait = workTime;

                if (isTemporaryContainer)
                {
                    WorldCargo cargoElement = GetComponent<WorldCargo>();
                    if (cargoElement.cargoItem != CargoSO.CargoType._undefined && targetPawn.GetComponent<WorldCargo>().cargoItem == CargoSO.CargoType._undefined)
                    {
                        //Theres an item, and the pawn has nothing - send to main storage depot
                        //targetPawn.agent.SetDestination();
                    }
                    else
                    {
                        if (targetPawn.GetComponent<WorldCargo>().cargoItem != CargoSO.CargoType._undefined)
                        {
                            targetPawn.GetComponent<WorldCargo>().TransferCargo(cargoElement);
                            targetPawn.agent.SetDestination(parentStructure.assignmentLocation.transform.position);
                        }
                        else
                        {
                            targetPawn.agent.SetDestination(parentStructure.assignmentLocation.transform.position);
                        }
                    }
                }
                if (isStorageItem)
                {
                    //
                }
            }

            if (isSalePoint)
            {
                isBeingWorked = true;
            }
        }

        if (creatingItemForPlayerShop)
        {
            //This listener is specifically for player items to be crafted
            EventsManager.StartListening("PlayerObjectReadyToCraft_" + parentStructure.name, WorkOnPlayerItemForStore);
        }
        else
        {
            EventsManager.StopListening("PlayerObjectReadyToCraft_" + parentStructure.name, WorkOnPlayerItemForStore);
        }
    }

    //Allows creation of cargo and items while the pawn is present
    private void OnTriggerStay(Collider other)
    {
        if (!growsIdly)
        {
            if (other.GetComponent<PawnNavigation>() != null)
            {
                if (permanentWorker)
                {
                    //Iterate on the object in need of attention
                    if (other.GetComponent<PawnNavigation>() == permedPawn)
                    {
                        if (readyPercentage <= 100)
                        {
                            readyPercentage += percentIncrease * Time.deltaTime;
                        }
                        else
                        {
                            SetNewDestination(permedPawn);
                        }
                    }
                }
                else
                {
                    if (other.GetComponent<PawnNavigation>() == targetPawn)
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
            }
        }
        else
        {
            if (readyPercentage <= 100)
            {
                readyPercentage += percentIncrease * Time.deltaTime;
            }
            else
            {
                readyPercentage = 100;
                parentStructure.assignmentLocation.GetComponent<WorkAssignment>().locationsToBeWorked.Add(this);
            }
            if (isBeingWorked)
            {
                SetNewDestination(targetPawn);
            }
        }
    }

    //This sets a new destination for the pawn when an item is completed
    private void SetNewDestination(PawnNavigation pawn)
    {
        isBeingWorked = false;
        pawn.timeToWait = 0;
        if (itemProduced != CargoSO.CargoType._undefined)
        {
            if (!chainDestination)
            {
                if (pawn.GetComponentInChildren<WorldCargo>().destination != null)
                {
                    //Triggers if there's a destination on the cargo
                    pawn.agent.SetDestination(pawn.GetComponent<WorldCargo>().destination.transform.position);
                    EventsManager.TriggerEvent("AdjustCargo_" + pawn.name);
                }
                else if (creatingItemForPlayerShop)
                {
                    //Triggers when an item's been created for etc
                }
                else
                {
                    //Triggered when theres no destination period
                }
            }
            else
            {
                //Triggers with a chain destination
                pawn.agent.SetDestination(chainDestination.transform.position);
            }
        }
        else
        {
            //Triggers if there's no cargo
            pawn.agent.SetDestination(parentStructure.transform.position);
        }
    }

    //This is just cleanup, ensuring the position is available for the future
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PawnNavigation>() == targetPawn)
        {
            isBeingWorked = false;
        }
        if (other.GetComponent<PawnNavigation>() == permedPawn)
        {
            isBeingWorked = false;
        }
    }
}
