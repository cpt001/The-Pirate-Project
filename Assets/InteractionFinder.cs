using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionFinder : MonoBehaviour
{

    // Update is called once per frame
    void LateUpdate()
    {
        //Raycast forward
        //If raycast encounters...
        //Shippart
        //InventoryInteractable
        //NPC
        //Door
        //Set interaction prompt true
        //Allow interaction bool
        //Perform related function

        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, transform.forward, out rayHit, 1.0f))
        {
            if (rayHit.transform.CompareTag("ShipInteractable"))
            {
                Debug.Log("Ship interaction detected");
            }
            else if (rayHit.transform.CompareTag("InventoryInteractable"))
            {

            }
            else if (rayHit.transform.CompareTag("NPC"))
            {

            }
            else if (rayHit.transform.CompareTag("Door"))
            {

            }
        }
    }
}
