using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// This class can be added to any pawn, and will automatically add any needed scripts.
/// -It will also act as runtime pawn generation whenever a new one is created.
/// -It also determines the pawn's parent
/// </summary>

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PawnVisualGeneration))]    //Action required. See summary.
[RequireComponent(typeof(PawnHealth))]              //Action required. See summary
[RequireComponent(typeof(PawnNeeds))]               //Major action required. See summary.
[RequireComponent(typeof(PawnWallet))]          //Outdated script, replace? Might be better integrated with needs?
[RequireComponent(typeof(PawnNavigation))]      //Outdated script, replace.
public class PawnBaseClass : MonoBehaviour
{
    public bool generatedAtRuntime;
    public Transform currentParent;             //This will be used for ship pathing later, if I can't figure out runtime navmesh
    private IslandController islandController;
    private PawnVisualGeneration pawnVisual;

    // Start is called before the first frame update
    void Start()
    {
        RaycastHit initRayHit;
        if (Physics.Raycast(transform.position, Vector3.down, out initRayHit, 3.0f))
        {
            if (initRayHit.transform.GetComponent<IslandController>())
            {
                currentParent = initRayHit.transform;
                //Set up NPCcontainer tag, look for, and reparent this to the tagged object 
                
                islandController = initRayHit.transform.GetComponent<IslandController>();
                EventsManager.TriggerEvent("PawnAddedToIsland");
            }
        }
        else
        {
            Debug.LogWarning(pawnVisual.characterName + " has no parent and is in a testing state. Expect things to break!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
