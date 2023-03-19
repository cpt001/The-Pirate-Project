using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentalStructure : MonoBehaviour
{
    private PawnGeneration buildingOwner;
    private bool isConstructionSite;

    private enum HouseType
    {
        _undefined,
        Room,
        Shack,
        House,
    }
    [Tooltip("Room is designed for one person. \n" + "Shacks are designed for a couple people. \n" + "Rooms are designed for 6 people")]
    private HouseType typeOfHouse;

    private List<CargoSO.CargoType> cargoRequired = new List<CargoSO.CargoType>();    //This list doesn't change. It tracks what the residence needs to provide happiness
    private Dictionary<CargoSO.CargoType, int> cargoOnSite = new Dictionary<CargoSO.CargoType, int>();

    private List<PawnGeneration> residentList = new List<PawnGeneration>();
    private Dictionary<Transform, PawnGeneration> bedAndOwner = new Dictionary<Transform, PawnGeneration>();

    private int maxResidents;
    private int maxChildren;

    private void Awake()
    {

        SetupResidenceType();

    }

    private void SetupResidenceType()
    {
        switch (typeOfHouse)
        {
            case HouseType._undefined:
                {
                    Debug.LogError(gameObject + " is an undefined residence!");
                    break;
                }
            case HouseType.Room:
                {
                    cargoRequired.Add(CargoSO.CargoType.Food);

                    maxResidents = 1;
                    if (buildingOwner.hasPartner)
                    {
                        maxResidents++;
                    }
                    maxChildren = 2;
                    break;
                }
            case HouseType.Shack:
                {
                    cargoRequired.Add(CargoSO.CargoType.Wax);
                    cargoRequired.Add(CargoSO.CargoType.Roots);
                    cargoRequired.Add(CargoSO.CargoType.Water);
                    cargoRequired.Add(CargoSO.CargoType.Flour);
                    cargoRequired.Add(CargoSO.CargoType.Wool);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat);
                    cargoRequired.Add(CargoSO.CargoType.Fish);
                    cargoRequired.Add(CargoSO.CargoType.Food);

                    maxResidents = 2;
                    maxChildren = 3;
                    break;
                }
            case HouseType.House:
                {
                    cargoRequired.Add(CargoSO.CargoType.Honey);
                    cargoRequired.Add(CargoSO.CargoType.Sugar);
                    cargoRequired.Add(CargoSO.CargoType.Roots);
                    cargoRequired.Add(CargoSO.CargoType.Water);
                    cargoRequired.Add(CargoSO.CargoType.Flour);
                    cargoRequired.Add(CargoSO.CargoType.Wool);
                    cargoRequired.Add(CargoSO.CargoType.Raw_Meat);
                    cargoRequired.Add(CargoSO.CargoType.Fish);
                    cargoRequired.Add(CargoSO.CargoType.Food);
                    maxResidents = 6;
                    maxChildren = 10;
                    break;
                }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
