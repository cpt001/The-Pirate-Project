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

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PawnVisualGeneration))]    //Action required. See summary.
//Pawn mental state -- Creates the pawn's archetypes, mood tendencies, and monitors moodlets as the AI's needs change through the day.
[RequireComponent(typeof(PawnHealth))]              //Actions Required Summary: Finish sections: feminine health, fertility, birth defects, injury cascade
[RequireComponent(typeof(PawnNeeds))]               //Major action required. See summary
[RequireComponent(typeof(PawnWallet))]              //Outdated script, replace? Might be better integrated with needs?
[RequireComponent(typeof(PawnNavPlanning))]         //TBD, replaces pawn navigation
//Pawn Animator, pawn combat capability needed as well
public class PawnBaseClass : MonoBehaviour
{
    public bool generatedAtRuntime;
    public Transform currentParent;             //This will be used for ship pathing later, if I can't figure out runtime navmesh
    private IslandController islandController;
    public PawnVisualGeneration pawnVisual;// => GetComponent<PawnVisualGeneration>();
    //private PawnHealth pawnHealth;// => GetComponent<PawnHealth>();
    public PawnNeeds pawnNeeds;// => GetComponent<PawnNeeds>();
    //private PawnWallet pawnWallet;// => GetComponent<PawnWallet>();
    //private PawnNavPlanning pawnNav;// => GetComponent<PawnNavPlanning>();

    [SerializeField] protected float pickInteractionInterval = 2f;
    protected float timeUntilNextInteraction = -1f;
    protected BaseInteractions currentInteraction = null;
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
    protected virtual void Update()
    {
        if (currentInteraction != null)
        {
            if (transform.position == currentInteraction.transform.position && !startedPerforming)
            {
                startedPerforming = true;
                currentInteraction.Perform(this, OnInteractionFinished);
            }
        }

        if (currentInteraction == null)
        {
            timeUntilNextInteraction -= Time.deltaTime;
            if (timeUntilNextInteraction <= 0)
            {
                timeUntilNextInteraction = pickInteractionInterval;
                PickBestInteraction();
            }
        }
    }

    protected virtual void OnInteractionFinished(BaseInteractions interaction)
    {
        interaction.UnlockInteraction();
        currentInteraction = null;
        Debug.Log($"Finished {interaction.DisplayName}");
    }

    public void UpdateIndividualStat(NeedFulfillmentType target, float amount)
    {
        Debug.Log($"Update {target} by {amount}");
        switch (target)
        {
            case NeedFulfillmentType.happiness:     pawnNeeds.happiness += amount; break;
            case NeedFulfillmentType.hunger:        pawnNeeds.happiness += amount; break;
            case NeedFulfillmentType.exhaustion:    pawnNeeds.exhaustion += amount; break;
            case NeedFulfillmentType.love:          pawnNeeds.love += amount; break;
            case NeedFulfillmentType.drunkenness:   pawnNeeds.drunkenness += amount; break;
            case NeedFulfillmentType.health:        pawnNeeds.health += amount; break;
            case NeedFulfillmentType.adventure:     pawnNeeds.adventure += amount; break;
            case NeedFulfillmentType.cleanliness:   pawnNeeds.cleanliness += amount; break;
            case NeedFulfillmentType.social:        pawnNeeds.social += amount; break;
            case NeedFulfillmentType.religion:      pawnNeeds.religion += amount; break;
            case NeedFulfillmentType.crime:         pawnNeeds.crime += amount; break;
        }
    }

    [SerializeField] protected float defaultInteractionScore = 0f;
    float ScoreInteraction(BaseInteractions interaction)
    {
        if (interaction.statChanges.Length == 0)
        {
            return defaultInteractionScore;
        }
        float score = 0f;

        foreach (var change in interaction.statChanges)
        {
            score += scoreChange(change.targetStat, change.value);
        }

        return score;
    }

    float scoreChange(NeedFulfillmentType target, float amount)
    {
        float currentValue = 0f;
        switch (target)
        {
            case NeedFulfillmentType.happiness: currentValue = pawnNeeds.happiness; break;
            case NeedFulfillmentType.hunger:    currentValue = pawnNeeds.happiness; break;
            case NeedFulfillmentType.exhaustion: currentValue = pawnNeeds.exhaustion; break;
            case NeedFulfillmentType.love:      currentValue = pawnNeeds.love; break;
            case NeedFulfillmentType.drunkenness: currentValue = pawnNeeds.drunkenness; break;
            case NeedFulfillmentType.health:    currentValue = pawnNeeds.health; break;
            case NeedFulfillmentType.adventure: currentValue = pawnNeeds.adventure; break;
            case NeedFulfillmentType.cleanliness: currentValue = pawnNeeds.cleanliness; break;
            case NeedFulfillmentType.social:    currentValue = pawnNeeds.social; break;
            case NeedFulfillmentType.religion:  currentValue = pawnNeeds.religion; break;
            case NeedFulfillmentType.crime:     currentValue = pawnNeeds.crime; break;
        }

        return (1f - currentValue) * amount;
    }

    class ScoredInteraction
    {
        public SmartObject targetObject;
        public BaseInteractions interaction;
        public float score;
    }

    void PickBestInteraction()
    {
        //Loop through all objects
        List<ScoredInteraction> unsortedInteractions = new List<ScoredInteraction>();
        foreach (var smartObject in SmartObjectManager.Instance.RegisteredObjects)
        {
            //loop through all interactions
            foreach (var interaction in smartObject.Interactions)
            {
                if (!interaction.Usable())
                    continue;

                float score = ScoreInteraction(interaction);

                unsortedInteractions.Add(new ScoredInteraction() { targetObject = smartObject, 
                                                                interaction = interaction, 
                                                                score = score });
            }
        }
        if (unsortedInteractions.Count == 0)
            return;

        var sortedInteractions = unsortedInteractions.OrderBy(scoredInteractions => scoredInteractions.score).ToList();
    }
}
