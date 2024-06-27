using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
/// <summary>
/// This class can be added to any pawn, and will automatically add any needed scripts.
/// -It will also act as runtime pawn generation whenever a new one is created.
/// -It also determines the pawn's parent
/// 
/// Explore fight or flight responses
/// </summary>

public enum NeedFulfillmentType
{
    happiness,
    hunger,
    exhaustion,
    love,
    drunkenness,
    health,
    adventure,
    cleanliness,
    social,
    religion,
    crime,
}

//[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PawnVisualGeneration))]    //Action required. See summary.
//Pawn mental state -- Creates the pawn's archetypes, mood tendencies, and monitors moodlets as the AI's needs change through the day.
[RequireComponent(typeof(PawnHealth))]              //Actions Required Summary: Finish sections: feminine health, fertility, birth defects, injury cascade
//[RequireComponent(typeof(PawnNeeds))]               //Major action required. See summary
[RequireComponent(typeof(PawnInventory))]              //Outdated script, replace? Might be better integrated with needs?
//[RequireComponent(typeof(PawnNavPlanning))]         //TBD, replaces pawn navigation
//Pawn Animator, pawn combat capability needed as well
public class PawnBaseClass : MonoBehaviour
{
    public bool generatedAtRuntime;
    public Transform currentParent;             //This will be used for ship pathing later, if I can't figure out runtime navmesh
    private IslandController islandController;
    public PawnVisualGeneration pawnVisual;// => GetComponent<PawnVisualGeneration>();
    //private PawnHealth pawnHealth;// => GetComponent<PawnHealth>();
    //public PawnNeeds pawnNeeds;// => GetComponent<PawnNeeds>();
    //private PawnWallet pawnWallet;// => GetComponent<PawnWallet>();
    //public PawnNavPlanning pawnNav => GetComponent<PawnNavPlanning>();

    [SerializeField] protected float pickInteractionInterval = 2f;
    protected float timeUntilNextInteraction = -1f;
    //protected BaseInteractions currentInteraction = null;
    protected bool startedPerforming = false;

    private enum PawnMasterState
    {

    }

    protected virtual void Awake()
    {

    }

    // Start is called before the first frame update
    protected virtual void Start()
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
    /*protected virtual void Update()
    {
        if (currentInteraction != null)
        {
            if (transform.position == currentInteraction.transform.position && !startedPerforming)
            {
                startedPerforming = true;
                //currentInteraction.Perform(this, OnInteractionFinished);
            }
        }

        if (currentInteraction == null)
        {
            timeUntilNextInteraction -= Time.deltaTime;
            if (timeUntilNextInteraction <= 0)
            {
                timeUntilNextInteraction = pickInteractionInterval;
                pawnNeeds.PickBestInteraction();
            }
        }
    }*/

    /*protected virtual void OnInteractionFinished(BaseInteractions interaction)
    {
        interaction.UnlockInteraction();
        currentInteraction = null;
        Debug.Log($"Finished {interaction.DisplayName}");
    }*/
}
