using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCargo : MonoBehaviour
{
    [SerializeField] private int decayTimer;
    [SerializeField] private CargoSO cargoItem;
    [SerializeField] private Transform destination;
    [SerializeField] private int value;

    private void Start()
    {
        if (cargoItem != null)
        {
            transform.name = "Cargo: " + cargoItem.cargoType.ToString();
            if (cargoItem.cargoType == CargoSO.CargoType._undefined)
            {
                Debug.Log("Cargo is undefined!");
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

    }
}
