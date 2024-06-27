using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is responsible for taking the data from the structure SO, and implementing it into practical use
/// It will set the structure shape, spawn NPCs, set allowed work times, and keep track of a building's inventory and cargo
/// </summary>
public class MMStructureSpawner : MonoBehaviour
{
    [SerializeField] private StructureSO assignedSO;
    public void SetStructure(StructureSO structureBlueprint)
    {
        GetComponent<BoxCollider>().enabled = false;
        assignedSO = structureBlueprint;
        this.name = structureBlueprint.structureName;
        if (structureBlueprint.baseStructureShape != null)
        {
            Instantiate(structureBlueprint.baseStructureShape, this.transform, false);
        }
        else
        {
            throw new System.Exception("Structure blueprint not implemented on " + structureBlueprint.name);
        }
    }
}
