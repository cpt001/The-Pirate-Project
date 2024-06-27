using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class should help immensely with the setup of structures in a longer term format. 
//It stores the relevant data for each structure, without requiring all the specific details required
[CreateAssetMenu(fileName = "New Structure", menuName = "Structure Data")]
public class StructureSO : ScriptableObject
{
    public float structureSpawnProbability = 0;
    public int structureTier;
    public string structureName;
    public Dictionary<CargoSO, int> constructionMaterials;
    public GameObject baseStructureShape;

    public int maxWorkers;
    public int numHousable;

    [SerializeField] private int workStartTime;
    [SerializeField] private int workEndTime;
    [SerializeField] private List<CargoSO> cargoInput; 
    [SerializeField] private List<CargoSO> cargoOutput;
    [SerializeField] private List<Item> purchaseableItems;
}
