using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a slot container.
//Add this to objects such as players, pawns, wagons, ships
//Give destination, clone object to associated container as needed
//Once at destination, add object to storehouse, null this out
public class WorldCargo : MonoBehaviour
{
    [SerializeField] private int decayTimer;
    public CargoSO.CargoType cargoItem;
    public Transform destination;           //This is checked against the island controller
    [SerializeField] private int value;
    private MeshRenderer meshRend;
    private PawnNavigation attachedPawn;

    private void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        CargoStateChange();
        attachedPawn = GetComponentInParent<PawnNavigation>();
        if (attachedPawn)
        {
            EventsManager.StartListening("AdjustCargo_" + attachedPawn.name, CargoStateChange);
        }
    }

    private void CargoStateChange()
    {
        if (cargoItem != CargoSO.CargoType._undefined)
        {
            if (meshRend)
            {
                meshRend.enabled = true;
            }
            transform.name = "Cargo: " + cargoItem.ToString();
            if (attachedPawn)
            {
                attachedPawn.agent.SetDestination(destination.position);
                EventsManager.TriggerEvent("NewDestination_" + attachedPawn);
            }
        }
        else
        {
            if (meshRend)
            {
                meshRend.enabled = false;
            }
        }
    }

    public void TransferCargo(WorldCargo targetContainer)
    {
        targetContainer.meshRend.enabled = true;
        targetContainer.name = "Cargo: " + cargoItem.ToString();
        targetContainer.destination = destination;

        meshRend.enabled = false;
        transform.name = "Cargo: ";
        destination = null;
    }

    void DestroyCargo()
    {
        meshRend.enabled = false;
        transform.name = "Cargo: ";
        destination = null;
    }
}
