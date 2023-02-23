using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCargo : MonoBehaviour
{
    [SerializeField] private int decayTimer;
    public CargoSO.CargoType cargoItem;
    public Transform destination; //Set this in work location trigger field
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
            meshRend.enabled = true;
            transform.name = "Cargo: " + cargoItem.ToString();
            if (attachedPawn)
            {
                attachedPawn.agent.SetDestination(destination.position);
                EventsManager.TriggerEvent("NewDestination_" + attachedPawn);
            }
        }
        else
        {
            meshRend.enabled = false;
        }
    }
}
