using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureCollisionDetector : MonoBehaviour
{
    private StructureTool associatedStructure => gameObject.GetComponentInParent<StructureTool>();

    void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collision detected: " + other.gameObject);
        if (other.gameObject.CompareTag("Structure"))
        {
            //Debug.Log("Building collision; repositioning: " + transform.parent + " / " + other.transform.parent);
            //associatedStructure.buildingPositionSet = false;
            //associatedStructure.assignedTown.SetBuildingPosition(transform.parent);
        }
    }
}
