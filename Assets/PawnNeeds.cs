using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles AI's need calculations
/// 
/// Stats: The lower a stat is, the more critical it needs attention
/// Removed fun stat - redundant. Replaced by social, drunkenness, and adventure
/// Removed happiness decay. Happiness is automagically calculated based on all other stats.
/// Removed peace. Replaced with safety override, which ignores all calculations until a situation has passed.
/// Changed health. Simply marks the need for the AI to seek medicine.
/// Religion need increases dramatically during church, but can be overridden by job needs.
/// Removed stats dictionary. It was probably overcomplicating what should've been a relatively simple implementation
/// 
/// The approach regarding time of day functionality isn't the right direction to be going.
/// Pawns have individual needs that need to be fulfilled, and will automatically seek out smart objects that fulfill the need.
/// There are some modifiers that can be introduced via time of day, or situation, but generally the stats should decay in a natural enough function.
/// 
/// 
/// 
/// Current tasks:
/// Create stacked ordering of priority needs.
/// Look for local smart objects that fulfill the need the best
/// Set destination, check work/sleep condition before
/// 
/// ToDo:
/// Script is still fully non-functional.
/// -Needs will decrement with time on a log scale
/// -Needs will affect mood
/// -Low needs assign priority, allowing pawn to check for smart objects
/// -Needs to communicate with new navigation script. 
/// </summary>

public class PawnNeeds : PawnBaseClass
{
    ////Each of these stats goes from 0-100 based on fulfillment
    [Header("Stats")]
    [Range(0, 1)]
    public float 
        happiness,      //Master stat, dictates desire for pawn to be happy.
        hunger,         //How badly the AI wants to eat
        exhaustion,     //How badly the AI wants to sleep.
        love,           //How badly the AI wants to be with their significant other, or visit a bawdy house. Does not decay in children.
        drunkenness,    //How badly the AI wants to get drunk. 
        health,         //How badly the AI needs medicine
        adventure,      //How badly the AI wants to explore. Some AI may have no decay at all.
        cleanliness,    //How clean the AI feels. Encourages AI to take bath at least once a week.
        social,         //Recency since AI talked to a friend.
        religion,       //This stat is only used to send the pawn to church on sundays
        crime;          //Increases with low fun, low hunger, low adventure     (The pawn is desperate, or feels the need to excite its life)
    [Header("Decay per Hour")]
    public float hungerDecay, exhaustionDecay, loveDecay, drunkennessDecay, adventureDecay, cleanlinessDecay, socialDecay, crimeDecay;     //Overall stat

    private bool sleepObligation;   //Prevents the worker from going anywhere when asleep
    private bool workObligation;    //Prevents the worker from going anywhere when working
    private bool safetyOverride;    //Overrides AI priorities to seek safety in dangerous situations
    private bool conversationOverride; 

    public Transform prioritizedDestination;

    private int hourCount;
    public PawnNeeds partner;

    protected override void Awake()
    {
        base.Awake();
        EventsManager.StartListening("NewHour", HourToHour);
        //InitializeStats();

        //CalculateHappiness();
    }

    protected override void Start()
    {
        base.Start();
        InitializeStats();
    }

    void InitializeStats()
    {
        hunger = Random.Range(0f, 1f);
        //Debug.Log($"Hunger value: " + hunger);
        exhaustion = Random.Range(0f, 1f);
        love = Random.Range(0f, 1f);
        drunkenness = Random.Range(0f, 1f);
        adventure = Random.Range(0f, 1f);
        cleanliness = Random.Range(0f, 1f);
        religion = Random.Range(0f, 1f);
        crime = Random.Range(0f, 1f);

        hungerDecay = Random.Range(0.12f, 0.17f); //This should trigger a meal every 3-4 hours, ie; around 2-3 meals a day. Might need to increase with an after work modifier
        exhaustionDecay = Random.Range(0.04f, 0.06f);   //Should encourage AI to sleep every ~18 hours
        loveDecay = Random.Range(0.005f, 0.09f);
        drunkennessDecay = Random.Range(0.04f, 0.08f);  //Only decays after working. Should encourage AI to visit pub every few days
        adventureDecay = Random.Range(0.005f, 0.008f);  //Decays after work, fulfilled by exploring their home, moving, or otherwise 
        cleanlinessDecay = Random.Range(0.04f, 0.08f);  //Encourages AI to bathe every few days. Can be modified based on job type.
        socialDecay = Random.Range(0.02f, 0.3f);        //Frequency of needing to talk to other AI
        crimeDecay = Random.Range(0.02f, 0.09f);
    }

    private void LateUpdate()
    {
        //The AI will attempt to solve its issues at all times, and will calculate for the best possible interaction when it can.
        //Decay happens on the hour though.
    }

    //Contains decay, and internal hour counter
    private void HourToHour()
    {
        if (hourCount < 24)
        {
            hourCount++;
        }
        else
        {
            hourCount = 0;
        }

        hunger = Mathf.Clamp01(hunger - hungerDecay);
        exhaustion = Mathf.Clamp01(exhaustion - exhaustionDecay);
        love = Mathf.Clamp01(love - loveDecay);
        drunkenness = Mathf.Clamp01(drunkenness - drunkennessDecay);
        adventure = Mathf.Clamp01(adventure - adventureDecay);
        cleanliness = Mathf.Clamp01(cleanliness - cleanlinessDecay);
        social = Mathf.Clamp01(social - socialDecay);
        crime = Mathf.Clamp01((hunger + drunkenness + adventure + social / 4) - crimeDecay);
    }



    #region Non-functional calculations
    /*void HourlyDecrement()
    {
        foreach (KeyValuePair<string, float> kvp in stats)
        {
            if (kvp.Key == "Fun" || kvp.Key == "Exhaustion"|| kvp.Key == "Love"|| kvp.Key == || kvp.Key == || kvp.Key == || kvp.Key == )
            {

            }
            else if(kvp.Key == "Hunger" || kvp.Key == "")
            {

            }
        }
    }*/


    /*void CalculateHappiness()
    {
        happiness = happiness + ((fun + hunger + exhaustion + love + peace + drunkenness + adventure + cleanliness + health) / 9);
        //happiness = happiness / 9;
    }*/
    /*
    //Calculates the effect that hunger has on happiness non-linearly
    float CalculateFunImpact(float fun_Need)
    {
        float funNeedAfterPlay = Mathf.Max(fun_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(funNeedAfterPlay) - CalculateHungerImpact(fun_Need);
        return -1 * Mathf.Pow(fun_Need / 10, 2);
    }
    float CalculateHungerImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateExhaustionImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateLoveImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculatePeaceImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateDrunkennessImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateAdventureImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateCleanlinessImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateHealthImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }*/
    #endregion
}
